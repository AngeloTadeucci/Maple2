﻿using System.Numerics;
using Maple2.Database.Storage;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class FunctionCubeHandler : PacketHandler<GameSession> {

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required FunctionCubeMetadataStorage FunctionCubeMetadataStorage { private get; init; }
    // ReSharper restore All
    #endregion

    public override RecvOp OpCode => RecvOp.FunctionCube;

    private enum Command : byte {
        Use = 4,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        Command command = packet.Read<Command>();
        switch (command) {
            case Command.Use:
                HandleUseCube(session, packet);
                break;
        }
    }

    private void HandleUseCube(GameSession session, IByteReader packet) {
        string interactId = packet.ReadUnicodeString();
        byte unk = packet.ReadByte();

        // Can this happen outside a housing plot?
        Plot? plot = session.Housing.GetFieldPlot();
        if (plot is null) {
            return;
        }

        string positionHex = long.Parse(interactId.Split('_')[1]).ToString("X6");

        var position = new Vector3B(
            (sbyte) Convert.ToByte(positionHex[4..], 16),
            (sbyte) Convert.ToByte(positionHex.Substring(2, 2), 16),
            (sbyte) Convert.ToByte(positionHex[..2], 16)
        );

        if (!plot.Cubes.TryGetValue(position, out PlotCube? cube)) {
            return;
        }

        switch (cube.HousingCategory) {
            case HousingCategory.Event:
                if (cube.Interact?.Nurturing is not null) {
                    HandleNurturing(session, plot, cube);
                }

                break;
        }
    }
    private void HandleNurturing(GameSession session, Plot plot, PlotCube cube) {
        using GameStorage.Request db = session.GameStorage.Context();
        Nurturing? dbNurturing = db.GetNurturing(plot.OwnerId, cube.ItemId);
        if (dbNurturing is null) {
            Logger.Error("Nurturing not found for account id {0} and item {1}", session.AccountId, cube.ItemId);
            return;
        }

        if (dbNurturing.ClaimedGiftForStage > dbNurturing.NurturingMetadata.RequiredGrowth.Length) {
            // fully grown
            return;
        }

        // always update object reference
        cube.Interact!.Nurturing = dbNurturing;
        Nurturing? nurturing = cube.Interact.Nurturing;

        if (session.AccountId != plot.OwnerId) {
            HandlePlayNurturing(session, plot, cube, nurturing, db);
            return;
        }

        FunctionCubeMetadata.NurturingData.Growth requiredGrowth = nurturing.NurturingMetadata.RequiredGrowth.First(x => x.Stage == nurturing.ClaimedGiftForStage);
        // Claim reward
        if (nurturing.Exp >= requiredGrowth.Exp) {
            nurturing.ClaimedGiftForStage++;

            Item? rewardStageItem = session.Field.ItemDrop.CreateItem(requiredGrowth.Reward.Id, rarity: requiredGrowth.Reward.Rarity, amount: requiredGrowth.Reward.Amount);
            if (rewardStageItem is null) {
                Logger.Error("Failed to create the reward item for character {0} and item {1}", session.AccountId, cube.ItemId);
                return;
            }

            // drop the item
            FieldItem fieldRewardStageItem = session.Field.SpawnItem(session.Player, cube.Position, new Vector3(0, 0, cube.Rotation), rewardStageItem, session.CharacterId);
            session.Field.Broadcast(FieldPacket.DropItem(fieldRewardStageItem));
            db.UpdateNurturing(session.AccountId, cube);

            session.Field.Broadcast(FunctionCubePacket.AddFunctionCube(cube));
            return;
        }

        FunctionCubeMetadata.NurturingData.Item feedItem = nurturing.NurturingMetadata.Feed;
        Item? hasItem = session.Item.Inventory.Find(feedItem.Id, feedItem.Rarity).FirstOrDefault();
        if (hasItem is null) {
            session.Send(NoticePacket.Message("You don't have the required item to feed this pet.  TODO: Find correct string id")); // TODO: Find correct string id
            return;
        }

        session.Item.Inventory.Consume(hasItem.Uid, amount: 1);

        FunctionCubeMetadata.NurturingData.Item rewardFeedItem = nurturing.NurturingMetadata.RewardFeed;
        Item? rewardItem = session.Field.ItemDrop.CreateItem(rewardFeedItem.Id, rarity: rewardFeedItem.Rarity, amount: rewardFeedItem.Amount);
        if (rewardItem is null) {
            Logger.Error("Failed to create the reward item for character {0} and item {1}", session.AccountId, cube.ItemId);
            return;
        }

        // drop the item
        FieldItem fieldItem = session.Field.SpawnItem(session.Player, cube.Position, new Vector3(0, 0, cube.Rotation), rewardItem, session.CharacterId);
        session.Field.Broadcast(FieldPacket.DropItem(fieldItem));
        nurturing.Feed();
        db.UpdateNurturing(session.AccountId, cube);

        session.Field.Broadcast(FunctionCubePacket.AddFunctionCube(cube));
        session.Send(FunctionCubePacket.Feed(fieldItem.Value.Uid, cube));
    }

    private void HandlePlayNurturing(GameSession session, Plot plot, PlotCube cube, Nurturing nurturing, GameStorage.Request db) {
        if (nurturing.PetBy.Contains(session.AccountId)) {
            session.Send(NoticePacket.Message("You already played with this pet today. TODO: Find correct string id")); // TODO: Find correct string id
            return;
        }

        if (db.CountNurturingForAccount(cube.ItemId, session.AccountId) >= Constant.NurturingPlayMaxCount) {
            session.Send(NoticePacket.Message("You have already played with the maximum number of pets today. TODO: Find correct string id")); // TODO: Find correct string id
            return;
        }

        if (!nurturing.Play(session.AccountId)) {
            return;
        }

        FunctionCubeMetadata.NurturingData.Item rewardPlay = nurturing.NurturingMetadata.Feed;
        Item? rewardItem = session.Field.ItemDrop.CreateItem(rewardPlay.Id, rarity: rewardPlay.Rarity, amount: rewardPlay.Amount);
        if (rewardItem is null) {
            Logger.Error("Failed to create the reward item for character {0} and item {1}", session.AccountId, cube.ItemId);
            return;
        }

        // drop the item
        FieldItem fieldItem = session.Field.SpawnItem(session.Player, cube.Position, new Vector3(0, 0, cube.Rotation), rewardItem, session.CharacterId);
        session.Field.Broadcast(FieldPacket.DropItem(fieldItem));

        db.UpdateNurturing(plot.OwnerId, cube);

        session.Field.Broadcast(FunctionCubePacket.AddFunctionCube(cube));

        // Uncomment when issue https://github.com/AngeloTadeucci/Maple2/issues/280 is resolved
        // Mail? mail = CreateOwnerMail(session, plot.OwnerId, nurturing.NurturingMetadata);
        //
        // if (mail == null) {
        //     Logger.Error("Failed to create mail for character {0} and item {1}", session.AccountId, cube.ItemId);
        //     return;
        // }
        //
        // try {
        //     session.World.MailNotification(new MailNotificationRequest {
        //         AccountId = ownerId,
        //         MailId = mail.Id,
        //     });
        // } catch { /* ignored */
        // }
    }

    // private Mail? CreateOwnerMail(GameSession session, long ownerId, FunctionCubeMetadata.NurturingData nurturing) {
    //     using GameStorage.Request db = session.GameStorage.Context();
    //
    //     string contentId = nurturing.QuestTag switch {
    //         "NurturingPumpkinDevil" => "18101804",
    //         "NurturingGhostCats" => "19101804",
    //         _ => "",
    //     };
    //
    //     if (string.IsNullOrEmpty(contentId)) {
    //         Logger.Warning("Unknown event tag {0} for nurturing mail", nurturing.QuestTag);
    //         return null;
    //     }
    //
    //     var mail = new Mail {
    //         AccountId = ownerId,
    //         Type = MailType.System,
    //         Content = contentId,
    //         SenderName = session.PlayerName,
    //     };
    //
    //     mail = db.CreateMail(mail);
    //     if (mail is null) {
    //         Logger.Error("Failed to create mail for account {0}", session.AccountId);
    //         return null;
    //     }
    //
    //     FunctionCubeMetadata.NurturingData.Item rewardPlay = nurturing.Feed;
    //     Item? rewardItem = session.Field.ItemDrop.CreateItem(rewardPlay.Id, rarity: rewardPlay.Rarity, amount: rewardPlay.Amount);
    //     if (rewardItem is null) {
    //         Logger.Error("Failed to create the reward item for character {0} and item {1}", session.AccountId, rewardPlay.Id);
    //         return null;
    //     }
    //
    //     Item? item = db.CreateItem(mail.Id, rewardItem);
    //     if (item is null) {
    //         Logger.Error("Failed to create item for mail {0} and item {1}", mail.Id, rewardPlay.Id);
    //         return null;
    //     }
    //
    //     mail.Items.Add(item);
    //     return mail;
    // }
}