using System;
using System.Collections.Generic;
using System.Linq;
using IronPython.Runtime;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Scripting.Npc;

public class NpcScriptContext {
    // These MUST be initialized externally. They are not passed via constructor to simplify scripts.
    public readonly GameSession Session;


    public NpcScriptContext(GameSession session) {
        Session = session;
    }

    public bool MovePlayer(int portalId) {
        if (Session.Field?.TryGetPortal(portalId, out FieldPortal? portal) != true) {
            return false;
        }
        Session.Send(NpcTalkPacket.MovePlayer(portalId));
        return true;
    }

    public void OpenDialog(string name, string tags) {
        Session.Send(NpcTalkPacket.OpenDialog(name, tags));
    }

    public bool RewardItem(PythonList list) {
        var rewards = new List<(int Id, int Rarity, int Amount)>();
        foreach (PythonTuple tuple in list) {
            if (tuple.Count != 3 || tuple[0] is not int itemId || tuple[1] is not int rarity || tuple[2] is not int amount) {
                return false;
            }

            rewards.Add((itemId, rarity, amount));
        }

        var results = new List<Item>();
        foreach ((int id, int rarity, int amount) in rewards) {
            Item? item = Session.Item.CreateItem(id, rarity, amount);
            if (item == null) {
                continue;
            }
            results.Add(item);
        }

        // Validate that reward is possible
        foreach (IGrouping<InventoryType, Item> group in results.GroupBy(item => item.Inventory)) {
            int requireSlots = group.Count();
            int freeSlots = Session.Item.Inventory.FreeSlots(group.Key);
            if (requireSlots > freeSlots) {
                return false;
            }
        }

        // TODO: We should send to mail if we can't add to inventory to guarantee reward.
        using GameStorage.Request db = Session.GameStorage.Context();
        db.BeginTransaction();
        List<Item> created = results.Select(item => db.CreateItem(Session.CharacterId, item))
            .Where(item => item != null)
            .ToList()!;
        db.Commit();

        foreach (Item item in created) {
            Session.Item.Inventory.Add(item, true);
        }
        Session.Send(NpcTalkPacket.RewardItem(created));
        return true;
    }

    public void RewardExp(long exp) {
        // TODO: properly inc exp
        Session.Player.Value.Character.Exp += exp;
        Session.Send(NpcTalkPacket.RewardExp(exp));
    }

    public void RewardMeso(long mesos) {
        Session.Currency.Meso += mesos;
        Session.Send(NpcTalkPacket.RewardMeso(mesos));
    }

    public bool HasItem(int itemId, int rarity) {
        return Session.Item.Inventory.Find(itemId, rarity).Any();
    }

    public bool HasMesos(long amount) {
        return Session.Currency.Meso >= amount;
    }

    public int Job() {
        return (int) Session.Player.Value.Character.Job;
    }

    public int Level() {
        return Session.Player.Value.Character.Level;
    }

    public bool QuestState(int questId, int questState) {
        Session.Quest.TryGetQuest(questId, out Quest? quest);
        bool questStateMatch = quest?.State == (QuestState) questState;
        Console.WriteLine($"Quest State: {questStateMatch}");
        return quest?.State == (QuestState) questState;
    }

    public bool MultiQuestState(List<int> questIds, int questState) {
        foreach (int questId in questIds) {
            if (Session.Quest.TryGetQuest(questId, out Quest? quest) && quest.State == (QuestState) questState) {
                return true;
            }
        }
        return false;
    }

    public int CurrentMap() {
        return Session.Field?.MapId ?? 0;
    }
}
