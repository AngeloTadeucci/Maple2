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
        FlipGalleryCard = 7,
        ClaimGalleryReward = 9,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.FlipGalleryCard:
                HandleFlipGalleryCard(session, packet);
                return;
            case Command.ClaimGalleryReward:
                HandleClaimGalleryReward(session);
                return;

        }
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
}
