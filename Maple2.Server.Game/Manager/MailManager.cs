using System.Collections.Immutable;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Manager;

public sealed class MailManager {
    private const int BATCH_SIZE = 5;
    private const int FETCH_DELAY = 10;

    private readonly GameSession session;
    private DateTime lastFetch;
    private long lastReceivedMail;
    private int unreadMail;

    private readonly SortedList<long, Mail> inbox;

    public MailManager(GameSession session) {
        this.session = session;

        inbox = new SortedList<long, Mail>();
        Fetch();
    }

    public void Notify(bool force = false) {
        if (Fetch(force) || force) {
            session.Send(MailPacket.Notify(unreadMail, unreadMail > 0));
        }
    }

    public void Load() {
        lock (inbox) {
            session.Send(MailPacket.StartList());
            foreach (ImmutableList<Mail> batch in inbox.Values.Batch(BATCH_SIZE)) {
                session.Send(MailPacket.Load(batch));
            }
            session.Send(MailPacket.EndList());
        }
    }

    public bool Delete(long mailId) {
        if (!inbox.ContainsKey(mailId)) {
            return false;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        if (!db.DeleteMail(mailId, session.AccountId, session.CharacterId)) {
            return false;
        }

        inbox.Remove(mailId);
        session.Send(MailPacket.Deleted(mailId));
        return true;
    }

    public MailError Read(long mailId) {
        if (!inbox.TryGetValue(mailId, out Mail? inboxMail)) {
            return MailError.mail_not_found;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        Mail? mail = db.GetMail(mailId, session.AccountId, session.CharacterId);
        if (mail == null) {
            return MailError.mail_not_found;
        }

        MailError error = ReadInternal(db, mail);
        if (error != MailError.none) {
            return error;
        }

        inboxMail.Update(mail);
        return MailError.none;
    }

    public MailError Collect(long mailId) {
        if (!inbox.TryGetValue(mailId, out Mail? inboxMail)) {
            return MailError.mail_not_found;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        Mail? mail = db.GetMail(mailId, session.AccountId, session.CharacterId);
        if (mail == null) {
            return MailError.mail_not_found;
        }

        lock (session.Item) {
            MailError error = CollectInternal(db, mail);
            if (error != MailError.none) {
                return error;
            }

            inboxMail.Update(mail);
            return MailError.none;
        }
    }

    private bool Fetch(bool force = false) {
        long accountId = session.AccountId;
        long characterId = session.CharacterId;

        lock (inbox) {
            if (!force && lastFetch.AddSeconds(FETCH_DELAY) > DateTime.Now) {
                return false;
            }

            bool newMail = false;
            using GameStorage.Request db = session.GameStorage.Context();
            foreach (Mail mail in db.GetAllMail(accountId, characterId, lastReceivedMail)) {
                if (mail.ReadTime == 0) {
                    unreadMail++;
                }

                newMail = true;
                inbox.Add(mail.Id, mail);
            }

            lastFetch = DateTime.Now;
            if (inbox.Count <= 0) {
                return false;
            }

            lastReceivedMail = inbox.Keys[inbox.Count - 1];
            return newMail;
        }
    }

    #region Internal (No locks)
    // ReadTime on input mail will be modified.
    private MailError ReadInternal(GameStorage.Request db, Mail mail) {
        if (mail.ReadTime > 0) {
            return MailError.s_mail_error_alreadyread;
        }

        Mail? readMail = db.MarkMailRead(mail.Id, session.AccountId, session.CharacterId);
        if (readMail == null) {
            return MailError.s_mail_error;
        }

        mail.ReadTime = readMail.ReadTime;
        session.Send(MailPacket.Read(mail));
        return MailError.none;
    }

    // Items on input mail will be cleared.
    private MailError CollectInternal(GameStorage.Request db, Mail mail) {
        if (mail.MesoCollected() && mail.MeretCollected() && mail.GameMeretCollected() && mail.Items.Count == 0) {
            return MailError.s_mail_error_already_receive;
        }

        // Validate that collection is possible
        foreach (IGrouping<InventoryType, Item> group in mail.Items.GroupBy(item => item.Inventory)) {
            int requireSlots = group.Count();
            int freeSlots = session.Item.Inventory.FreeSlots(group.Key);

            if (requireSlots > freeSlots) {
                return MailError.s_mail_error_receiveitem_to_inven;
            }
        }

        bool collectMeso = false;
        if (!mail.MesoCollected()) { // Allow mesos to be collected regardless and just overflow.
            mail.MesoCollectTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            collectMeso = true;
        }
        bool collectMeret = false;
        if (!mail.MeretCollected() && session.Currency.CanAddMeret(mail.Meret) == mail.Meret) {
            mail.MeretCollectTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            collectMeret = true;
        }
        bool collectGameMeret = false;
        if (!mail.GameMeretCollected() && session.Currency.CanAddGameMeret(mail.GameMeret) == mail.GameMeret) {
            mail.GameMeretCollectTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            collectGameMeret = true;
        }

        if (collectMeso || collectMeret || collectGameMeret) {
            Mail? updatedMail = db.UpdateMail(mail);
            if (updatedMail == null) {
                return MailError.s_mail_error;
            }
        }

        // Collect the mail only after we have fully validated everything.
        if (collectMeso) {
            session.Currency.Meso += mail.Meso;
        }
        if (collectMeret) {
            session.Currency.Meret += mail.Meret;
        }
        if (collectGameMeret) {
            session.Currency.GameMeret += mail.GameMeret;
        }

        for (int i = mail.Items.Count - 1; i >= 0; i--) {
            Item item = mail.Items[i];
            if (item.Amount <= 0) {
                return MailError.s_mail_error_attachcount;
            }

            if (!session.Item.Inventory.Add(item, notifyNew: true, commit: true)) {
                throw new InvalidOperationException($"Mail {mail.Id} was collected but items could not be added to inventory.");
            }

            mail.Items.RemoveAt(i);
        }

        session.Send(MailPacket.Collect(mail.Id));
        session.Send(MailPacket.CollectRead(mail));
        return MailError.none;
    }
    #endregion
}
