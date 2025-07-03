using Microsoft.EntityFrameworkCore;
using Mail = Maple2.Model.Game.Mail;
using Item = Maple2.Model.Game.Item;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public Mail? GetMail(long mailId, long characterId) {
            Model.Mail? model = Context.Mail.Find(characterId, mailId);
            if (model == null) {
                return null;
            }

            Mail mail = model;
            foreach (Item item in GetAllItems(mailId)) {
                mail.Items.Add(item);
            }

            return mail;
        }

        public ICollection<Mail> GetSentMail(long characterId) {
            Mail[] mails = Context.Mail.Where(mail => mail.SenderId == characterId)
                .AsEnumerable()
                .Select<Model.Mail, Mail>(mail => mail)
                .ToArray();

            foreach (Mail mail in mails) {
                foreach (Item item in GetAllItems(mail.Id)) {
                    mail.Items.Add(item);
                }
            }

            return mails;
        }

        // Binds all mails from an account to the first character that access them
        public void BindAccountMailsToCharacter(long accountId, long characterId) {
            Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            BeginTransaction();

            List<Model.Mail> mails = Context.Mail.Where(mail => mail.ReceiverId == accountId).ToList();
            if (mails.Count == 0) {
                Commit();
                return;
            }

            foreach (Model.Mail mail in mails) {
                Context.Mail.Remove(mail);
            }

            Context.SaveChanges();

            foreach (Model.Mail mail in mails) {
                mail.ReceiverId = characterId;
                Context.Mail.Add(mail);
            }

            Context.SaveChanges();

            if (!Commit()) {
                throw new Exception("Failed to bind account mails to character");
            }
        }

        public ICollection<Mail> GetAllMail(long characterId, long minId = 0) {
            Mail[] mails = Context.Mail.Where(mail => mail.ReceiverId == characterId)
                .Where(mail => mail.Id > minId)
                .AsEnumerable()
                .Select<Model.Mail, Mail>(mail => mail)
                .ToArray();

            foreach (Mail mail in mails) {
                foreach (Item item in GetAllItems(mail.Id)) {
                    mail.Items.Add(item);
                }
            }

            return mails;
        }

        public Mail? CreateMail(Mail mail) {
            Model.Mail model = mail;
            model.Id = 0;
            if (mail.Items.Count == 0) {
                Context.Mail.Add(model);
                return SaveChanges() ? model : null;
            }

            BeginTransaction();
            Context.Mail.Add(model);
            if (!SaveChanges()) {
                return null;
            }

            SaveItems(model.Id, mail.Items.ToArray());
            if (!Commit()) {
                return null;
            }

            Mail updatedMail = model;
            foreach (Item item in mail.Items) {
                updatedMail.Items.Add(item);
            }

            return updatedMail;
        }

        public Mail? MarkMailRead(long mailId, long characterId) {
            Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

            Model.Mail? mail = Context.Mail.Find(characterId, mailId);
            if (mail == null || mail.ReadTime > DateTime.Now) {
                return null;
            }

            mail.ReadTime = DateTime.Now;
            Context.Mail.Update(mail);
            return SaveChanges() ? mail : null;
        }

        public bool DeleteMail(long mailId, long characterId) {
            Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

            Model.Mail? mail = Context.Mail.Find(characterId, mailId);
            if (mail == null) {
                return false;
            }

            Context.Mail.Remove(mail);
            return SaveChanges();
        }

        public Mail? UpdateMail(Mail mail) {
            Model.Mail model = mail;
            Context.Mail.Update(model);
            return SaveChanges() ? model : null;
        }
    }
}
