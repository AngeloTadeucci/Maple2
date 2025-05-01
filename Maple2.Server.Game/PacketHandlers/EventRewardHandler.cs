using Maple2.Database.Extensions;
using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Event;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class EventRewardHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.EventReward;

    private enum Command : byte {
        ClaimTimeRunFinalReward = 3,
        ClaimTimeRunStepReward = 4,
        FlipGalleryCard = 7,
        ClaimGalleryReward = 9,
        OpenBingo = 20,
        CrossOffBingoNumber = 22,
        ClaimBingoReward = 23,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.ClaimTimeRunStepReward:
                HandleClaimTimeRunStepReward(session, packet);
                return;
            case Command.ClaimTimeRunFinalReward:
                HandleClaimTimeRunFinalReward(session);
                return;
            case Command.FlipGalleryCard:
                HandleFlipGalleryCard(session, packet);
                return;
            case Command.ClaimGalleryReward:
                HandleClaimGalleryReward(session);
                return;
            case Command.OpenBingo:
                HandleOpenBingo(session);
                return;
            case Command.CrossOffBingoNumber:
                HandleCrossOffBingoNumber(session, packet);
                return;
            case Command.ClaimBingoReward:
                HandleClaimBingoReward(session, packet);
                return;
        }
    }

    private void HandleClaimTimeRunStepReward(GameSession session, IByteReader packet) {
        int steps = packet.ReadInt();

        GameEvent? gameEvent = session.FindEvent(GameEventType.TimeRunEvent).FirstOrDefault();
        if (gameEvent?.Metadata.Data is not TimeRunEvent timeRunEvent) {
            return;
        }

        if (!timeRunEvent.StepRewards.TryGetValue(steps, out RewardItem rewardItem)) {
            return;
        }

        Item? item = session.Field.ItemDrop.CreateItem(rewardItem.ItemId, rewardItem.Rarity, rewardItem.Amount);
        if (item == null) {
            return;
        }

        if (!session.Item.Inventory.Add(item, true)) {
            session.Item.MailItem(item);
        }

        session.Send(EventRewardPacket.ClaimTimeRunStepReward(rewardItem));
    }

    private void HandleClaimTimeRunFinalReward(GameSession session) {
        GameEvent? gameEvent = session.FindEvent(GameEventType.TimeRunEvent).FirstOrDefault();
        if (gameEvent?.Metadata.Data is not TimeRunEvent timeRunEvent) {
            return;
        }

        RewardItem rewardItem = timeRunEvent.FinalReward;

        Item? item = session.Field.ItemDrop.CreateItem(rewardItem.ItemId, rewardItem.Rarity, rewardItem.Amount);
        if (item == null) {
            return;
        }

        if (!session.Item.Inventory.Add(item, true)) {
            session.Item.MailItem(item);
        }

        session.Send(EventRewardPacket.ClaimTimeRunFinalReward(rewardItem));
    }

    private void HandleFlipGalleryCard(GameSession session, IByteReader packet) {
        byte index = packet.ReadByte();

        // Get only the first gallery event. There should only be one.
        GameEvent? gameEvent = session.FindEvent(GameEventType.Gallery).FirstOrDefault();

        if (gameEvent?.Metadata.Data is not Gallery gallery) {
            return;
        }

        if (index >= gallery.QuestIds.Length) {
            return;
        }

        int questId = gallery.QuestIds[index];

        // Check flip limit before starting the quest
        GameEventUserValue userValue = session.GameEvent.Get(
            GameEventUserValueType.GalleryCardFlipCount,
            gameEvent.Id,
            DateTime.Today.AddDays(1).ToEpochSeconds());
        int value = userValue.Int();
        if (value >= gallery.RevealDayLimit) {
            return;
        }

        QuestError error = session.Quest.Start(questId);
        if (error != QuestError.none) {
            session.Send(QuestPacket.Error(error));
            return;
        }

        session.GameEvent.Set(gameEvent.Id, GameEventUserValueType.GalleryCardFlipCount, value + 1);

        session.Send(EventRewardPacket.FlipGalleryCard(index));
    }

    private void HandleClaimGalleryReward(GameSession session) {
        GameEvent? gameEvent = session.FindEvent(GameEventType.Gallery).FirstOrDefault();

        if (gameEvent?.Metadata.Data is not Gallery gallery) {
            return;
        }

        bool allQuestsCompleted = gallery.QuestIds.All(questId =>
            session.Quest.TryGetQuest(questId, out Quest? quest) && quest.State == QuestState.Completed);
        if (!allQuestsCompleted) {
            // All quests need to be completed.
            return;
        }

        GameEventUserValue userValue = session.GameEvent.Get(GameEventUserValueType.GalleryClaimReward, gameEvent.Id, gameEvent.EndTime);
        int value = userValue.Int();
        if (value > 0) {
            // already claimed
            return;
        }

        session.GameEvent.Set(gameEvent.Id, GameEventUserValueType.GalleryClaimReward, value + 1);

        foreach (RewardItem rewardItem in gallery.RewardItems) {
            Item? item = session.Field.ItemDrop.CreateItem(rewardItem.ItemId, rewardItem.Rarity, rewardItem.Amount);
            if (item == null) {
                continue;
            }

            if (!session.Item.Inventory.Add(item)) {
                session.Item.MailItem(item);
            }
        }

        session.Send(EventRewardPacket.ClaimGalleryReward(gallery.RewardItems));
    }

    private void HandleOpenBingo(GameSession session) {
        GameEvent? gameEvent = session.FindEvent(GameEventType.BingoEvent).FirstOrDefault();
        if (gameEvent?.Metadata.Data is not BingoEvent bingoEvent) {
            return;
        }

        DateTime startTime = gameEvent.StartTime.FromEpochSeconds();
        // days since event started
        int days = Math.Max(0, (int)(DateTime.UtcNow - startTime).TotalDays);
        if (days >= bingoEvent.Numbers.Length) {
            Logger.Error("Bingo event is incorrectly set up. There are not enough days for the event.");
            return;
        }

        // This uid sets up what board layout the user will have.
        GameEventUserValue uid = session.GameEvent.Get(GameEventUserValueType.BingoUid, gameEvent.Id, gameEvent.EndTime);
        if (string.IsNullOrEmpty(uid.Value)) {
            session.GameEvent.Set(gameEvent.Id, GameEventUserValueType.BingoUid, uid.Value);
        }

        GameEventUserValue checkedNumbers = session.GameEvent.Get(GameEventUserValueType.BingoNumbersChecked, gameEvent.Id, gameEvent.EndTime);
        GameEventUserValue rewardsClaimed = session.GameEvent.Get(GameEventUserValueType.BingoRewardsClaimed, gameEvent.Id, gameEvent.EndTime);

        session.Send(EventRewardPacket.OpenBingo(int.Parse(uid.Value), checkedNumbers.Value, rewardsClaimed.Value, bingoEvent.Numbers[days]));
    }

    private void HandleCrossOffBingoNumber(GameSession session, IByteReader packet) {
        GameEvent? gameEvent = session.FindEvent(GameEventType.BingoEvent).FirstOrDefault();
        if (gameEvent?.Metadata.Data is not BingoEvent bingoEvent) {
            return;
        }

        int itemId = packet.ReadInt();
        int bingoNumber = packet.ReadInt();

        // Check if the item is a pencil
        if (itemId != bingoEvent.PencilItemId && itemId != bingoEvent.PencilPlusItemId) {
            return;
        }

        GameEventUserValue checkedNumbers = session.GameEvent.Get(GameEventUserValueType.BingoNumbersChecked, gameEvent.Id, gameEvent.EndTime);
        List<int> checkedNumbersList = string.IsNullOrEmpty(checkedNumbers.Value) ? [] : checkedNumbers.Value.Split(',').Select(int.Parse).ToList();
        if (checkedNumbersList.Contains(bingoNumber)) {
            return;
        }

        if (itemId == bingoEvent.PencilItemId) {
            DateTime startTime = gameEvent.StartTime.FromEpochSeconds();
            int days = Math.Max(0, (int)(DateTime.UtcNow - startTime).TotalDays);
            if (!bingoEvent.Numbers[days].Contains(bingoNumber)) {
                return;
            }
        }

        Item? item = session.Item.Inventory.Find(itemId).FirstOrDefault();
        if (item == null) {
            return;
        }

        checkedNumbersList.Add(bingoNumber);
        session.Item.Inventory.Consume(item.Uid, 1);
        session.GameEvent.Set(gameEvent.Id, GameEventUserValueType.BingoNumbersChecked, checkedNumbers.Value);

        GameEventUserValue rewardsClaimed = session.GameEvent.Get(GameEventUserValueType.BingoRewardsClaimed, gameEvent.Id, gameEvent.EndTime);

        session.Send(EventRewardPacket.UpdateBingo(checkedNumbers.Value, rewardsClaimed.Value));
    }

    private void HandleClaimBingoReward(GameSession session, IByteReader packet) {
        int index = packet.ReadInt();
        GameEvent? gameEvent = session.FindEvent(GameEventType.BingoEvent).FirstOrDefault();
        if (gameEvent?.Metadata.Data is not BingoEvent bingoEvent) {
            return;
        }

        if (index >= bingoEvent.Rewards.Length) {
            return;
        }

        GameEventUserValue rewardsClaimed = session.GameEvent.Get(GameEventUserValueType.BingoRewardsClaimed, gameEvent.Id, gameEvent.EndTime);
        List<int> rewardsClaimedList = string.IsNullOrEmpty(rewardsClaimed.Value) ? [] : rewardsClaimed.Value.Split(',').Select(int.Parse).ToList();
        if (rewardsClaimedList.Contains(index)) {
            return;
        }
        rewardsClaimedList.Add(index);
        session.GameEvent.Set(gameEvent.Id, GameEventUserValueType.BingoRewardsClaimed, string.Join(',', rewardsClaimedList));

        GameEventUserValue checkedNumbers = session.GameEvent.Get(GameEventUserValueType.BingoNumbersChecked, gameEvent.Id, gameEvent.EndTime);

        foreach (RewardItem rewardItem in bingoEvent.Rewards[index].Items) {
            Item? item = session.Field.ItemDrop.CreateItem(rewardItem.ItemId, rewardItem.Rarity, rewardItem.Amount);
            if (item == null) {
                continue;
            }

            if (!session.Item.Inventory.Add(item, true)) {
                session.Item.MailItem(item);
            }
        }
        session.Send(EventRewardPacket.ClaimBingoReward(bingoEvent.Rewards[index].Items));
        session.Send(EventRewardPacket.UpdateBingo(checkedNumbers.Value, rewardsClaimed.Value));
    }
}
