using System.Numerics;
using Maple2.Database.Storage;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.PacketHandlers;

public class FishingHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.Fishing;

    private enum Command : byte {
        Prepare = 0,
        Stop = 1,
        Catch = 8,
        Start = 9,
        FailMinigame = 10,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required ItemMetadataStorage ItemMetadata { private get; init; }
    public required TableMetadataStorage TableMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Prepare:
                HandlePrepare(session, packet);
                break;
            case Command.Stop:
                HandleStop(session);
                break;
            case Command.Catch:
                HandleCatch(session, packet);
                break;
            case Command.Start:
                HandleStart(session, packet);
                break;
            case Command.FailMinigame:
                HandleFailMinigame(session);
                break;
        }
    }

    private void HandlePrepare(GameSession session, IByteReader packet) {
        long fishingRodUid = packet.ReadLong();
        if (session.Field == null || session.GuideObject != null) {
            return;
        }

        Item? rod = session.Item.Inventory.Get(fishingRodUid);
        if (rod == null || rod.Metadata.Function?.Type != ItemFunction.FishingRod) {
            session.Send(FishingPacket.Error(FishingError.s_fishing_error_invalid_item));
            return;
        }
        if (!int.TryParse(rod.Metadata.Function?.Parameters, out int rodCode) || !TableMetadata.FishingRodTable.Entries.TryGetValue(rodCode, out FishingRodTable.Entry? rodMetadata)) {
            session.Send(FishingPacket.Error(FishingError.s_fishing_error_invalid_item));
            return;
        }
        if (session.Mastery[MasteryType.Fishing] < rodMetadata.MinMastery) {
            session.Send(FishingPacket.Error(FishingError.s_fishing_error_fishingrod_mastery));
            return;
        }
        if (!TableMetadata.FishingSpotTable.Entries.TryGetValue(session.Field.MapId, out FishingSpotTable.Entry? spotMetadata)) {
            session.Send(FishingPacket.Error(FishingError.s_fishing_error_notexist_fish));
            return;
        }
        if (session.Mastery[MasteryType.Fishing] < spotMetadata.MinMastery) {
            session.Send(FishingPacket.Error(FishingError.s_fishing_error_lack_mastery));
            return;
        }

        IList<Vector3> fishingBlocks = GetFishingBlocks(session.Player.Position, session.Player.Rotation).ToArray();
        // get the fishing block closest to player
        Vector3 guidePosition = fishingBlocks.MinBy(block =>
            Vector2.Distance(new Vector2(session.Player.Position.X, session.Player.Position.Y), new Vector2(block.X, block.Y)));
        // Move guide 1 block up so it sits on top of the fishing block
        guidePosition.Z += Constant.BlockSize;
        var guideObject = new FishingGuideObject(rodMetadata, spotMetadata);
        session.GuideObject = session.Field.SpawnGuideObject(session.Player, guideObject, guidePosition);

        session.Field.Broadcast(GuideObjectPacket.Create(session.GuideObject));
        session.Send(FishingPacket.LoadTiles(fishingBlocks));
        session.Send(FishingPacket.Prepare(fishingRodUid));
    }

    private static IEnumerable<Vector3> GetFishingBlocks(Vector3 position, Vector3 rotation) {
        /* Note this is all wrong and is a temp placeholder until the actual functionality is implemented.
         * Normally how it should work is we get the intercardinal direction that the player is facing closest to (Southwest, northwest, southeast, northeast)
         * We then scan in front of the player a 5x3x3 area for liquid blocks
         * The liquid block CANNOT have another block on top of it
         * It cannot be ABOVE the block the player is standing on (i.e. scan the z axis same level as player and 2 blocks below max(?. May be more. Needs confirmation)
         *
         * For now, we will only be getting near-ish blocks.
         */

        Vector3 checkCoordinates = position.Align();
        float direction = rotation.AlignRotation().Z;

        checkCoordinates.Z -= Constant.BlockSize;
        Vector3 checkBlock = checkCoordinates;
        switch (direction) {
            case Constant.NorthEast: {
                    checkBlock.Y += 2 * Constant.BlockSize; // start at the corner

                    for (int yAxis = 0; yAxis < 5; yAxis++) {
                        for (int xAxis = 0; xAxis < 3; xAxis++) {
                            checkBlock.X += Constant.BlockSize;

                            // Normally here we would scan Z levels for liquid blocks
                            // For now we will just add the coordinate in the same level to the list;
                            yield return checkBlock;
                        }
                        checkBlock.Y -= Constant.BlockSize;
                        checkBlock.X = checkCoordinates.X; // reset X
                    }
                    break;
                }
            case Constant.NorthWest: {
                    checkBlock.X += 2 * Constant.BlockSize; // start at the corner

                    for (int xAxis = 0; xAxis < 5; xAxis++) {
                        for (int yAxis = 0; yAxis < 3; yAxis++) {
                            checkBlock.Y += Constant.BlockSize;
                            // Normally here we would scan Z levels for liquid blocks
                            // For now we will just add the coordinate in the same level to the list;
                            yield return checkBlock;
                        }
                        checkBlock.X -= Constant.BlockSize;
                        checkBlock.Y = checkCoordinates.Y; // reset Y
                    }
                    break;
                }
            case Constant.SouthWest: {
                    checkBlock.Y -= 2 * Constant.BlockSize; // start at the corner

                    for (int yAxis = 0; yAxis < 5; yAxis++) {
                        for (int xAxis = 0; xAxis < 3; xAxis++) {
                            checkBlock.X -= Constant.BlockSize;
                            // Normally here we would scan Z levels for liquid blocks
                            // For now we will just add the coordinate in the same level to the list;
                            yield return checkBlock;
                        }
                        checkBlock.Y += Constant.BlockSize;
                        checkBlock.X = checkCoordinates.X; // reset X
                    }
                    break;
                }
            case Constant.SouthEast: {
                    checkBlock.X -= 2 * Constant.BlockSize; // start at the corner

                    for (int xAxis = 0; xAxis < 5; xAxis++) {
                        for (int yAxis = 0; yAxis < 3; yAxis++) {
                            checkBlock.Y -= Constant.BlockSize;
                            // Normally here we would scan Z levels for liquid blocks
                            // For now we will just add the coordinate in the same level to the list;
                            yield return checkBlock;
                        }
                        checkBlock.X += Constant.BlockSize;
                        checkBlock.Y = checkCoordinates.Y; // reset Y
                    }
                    break;
                }
        }
    }

    private static void HandleStop(GameSession session) {
        if (session.Field == null || session.GuideObject == null) {
            return;
        }

        session.Send(FishingPacket.Stop());
        session.Field.Broadcast(GuideObjectPacket.Remove(session.GuideObject));
        session.GuideObject = null;
        session.FishingMiniGameActive = false;
    }

    private void HandleCatch(GameSession session, IByteReader packet) {
        if (session.Field == null || session.GuideObject?.Value is not FishingGuideObject fishingGuideObject) {
            return;
        }

        bool success = packet.ReadBool();

        // TODO: Get the fishing block underneath the guide object and validate what fish can be caught there
        // For now we will assume all fish in the map are valid
        FishTable.Entry[] entries = TableMetadata.FishTable.Entries.Values
            .Where(entry => entry.HabitatMapIds.Contains(session.Field.MapId) && entry.Mastery <= session.Mastery[MasteryType.Fishing])
            .ToArray();
        if (entries.Length == 0) {
            session.Send(FishingPacket.Error(FishingError.s_fishing_error_notexist_fish));
            HandleStop(session);
            return;
        }

        FishTable.Entry fishEntry = GetFishToCatch(entries);
        // determine size of fish
        int fishSize = Random.Shared.NextDouble() switch {
            >= 0.0 and < 0.01 when session.FishingMiniGameActive => Random.Shared.Next(fishEntry.BigSize.Max + 1, fishEntry.BigSize.Max * 10 + 1), // prize
            >= 0.0 and < 0.03 => Random.Shared.Next(fishEntry.BigSize.Min, fishEntry.BigSize.Max + 1), // big
            >= 0.03 and < 0.15 => Random.Shared.Next(fishEntry.SmallSize.Max + 1, fishEntry.BigSize.Min), // medium
            >= 0.15 => Random.Shared.Next(fishEntry.SmallSize.Min, fishEntry.SmallSize.Max + 1), // small
            _ => Random.Shared.Next(fishEntry.SmallSize.Min, fishEntry.SmallSize.Max + 1),
        };

        FishEntry? fish = null;
        if (success) {
            if (!session.Player.Value.Unlock.FishAlbum.TryGetValue(fishEntry.Id, out fish)) {
                fish = new FishEntry(fishEntry.Id);
                session.Player.Value.Unlock.FishAlbum[fishEntry.Id] = fish;
                AddMastery(session, fishingGuideObject.Spot, fishEntry, CaughtFishType.FirstKind);
            }

            if (fishSize > fishEntry.BigSize.Max) {
                fish.TotalPrizeFish++;
                AddMastery(session, fishingGuideObject.Spot, fishEntry, CaughtFishType.Prize);
                session.Field.Broadcast(FishingPacket.PrizeFish(session.PlayerName, fishEntry.Id));
                session.ConditionUpdate(ConditionType.fish_big, codeLong: fishEntry.Id, targetLong: session.Player.Value.Character.MapId);
                session.Exp.AddExp(ExpType.fishing, Constant.FishingBigFishExpModifier);
            } else {
                if (fishSize >= fishEntry.BigSize.Min) {
                    session.ConditionUpdate(ConditionType.fish_goldmedal, codeLong: fishEntry.Id);
                }
                AddMastery(session, fishingGuideObject.Spot, fishEntry, CaughtFishType.Default);
                session.Exp.AddExp(ExpType.fishing);
            }

            fish.TotalCaught++;
            fish.LargestSize = Math.Max(fishSize, fish.LargestSize);
        }

        session.Send(FishingPacket.CatchFish(fishEntry.Id, fishSize, false, fish));
        session.FishingMiniGameActive = false;

        if (success) {
            CatchItem(session);
            session.ConditionUpdate(ConditionType.fish, codeLong: fishEntry.Id, targetLong: session.Player.Value.Character.MapId);
        } else {
            session.ConditionUpdate(ConditionType.fish_fail);
        }
    }

    private void CatchItem(GameSession session) {
        if (Random.Shared.NextDouble() > Constant.FishingItemChance || TableMetadata.FishingRewardTable.Entries.Count == 0) {
            return;
        }

        FishingItemType type = Random.Shared.NextDouble() switch {
            >= 0 and < 0.825 => FishingItemType.Trash,
            >= 0.825 and < 0.975 => FishingItemType.LightBox,
            >= 0.975 and < 0.995 => FishingItemType.HeavyBox,
            >= 0.995 => FishingItemType.Skin,
            _ => FishingItemType.Trash,
        };

        FishingRewardTable.Entry[] items = TableMetadata.FishingRewardTable.Entries.Values
            .Where(entry => entry.Type == type)
            .OrderBy(_ => Random.Shared.Next())
            .Take(Constant.FishingRewardsMaxCount)
            .ToArray();
        foreach (FishingRewardTable.Entry entry in items) {
            Item? item = session.Item.CreateItem(entry.Id, entry.Rarity, entry.Amount);
            if (item == null) {
                return;
            }

            if (!session.Item.Inventory.Add(item, true)) {
                return;
            }
        }

        session.Send(FishingPacket.CatchItem(items));
    }

    private static void AddMastery(GameSession session, FishingSpotTable.Entry spotEntry, FishTable.Entry fishEntry, CaughtFishType catchType) {
        if (session.Mastery[MasteryType.Fishing] >= Constant.FishingMasteryMax) {
            return;
        }

        int exp = fishEntry.Rarity;
        switch (catchType) {
            case CaughtFishType.Prize:
            case CaughtFishType.FirstKind:
                exp = fishEntry.Rarity * Constant.FishingMasteryIncreaseFactor;
                break;
            case CaughtFishType.Default:
                if (session.Mastery[MasteryType.Fishing] >= spotEntry.MaxMastery && Random.Shared.NextDouble() > Constant.FishingMasteryAdditionalExpChance) {
                    return;
                }

                // give mastery exp if within the fishing spot's mastery range
                if (spotEntry.MinMastery >= session.Mastery[MasteryType.Fishing]) {
                    exp = Random.Shared.NextDouble() switch {
                        >= 0.0 and < 0.80 => 1,
                        >= 0.80 and < 0.98 => 2,
                        >= 0.98 => 3,
                        _ => 1,
                    };
                }
                break;
        }

        short level = session.Mastery.GetLevel(MasteryType.Fishing);
        session.Mastery[MasteryType.Fishing] += exp;
        session.Send(FishingPacket.IncreaseMastery(fishEntry.Id, level, exp, catchType));
    }

    private static FishTable.Entry GetFishToCatch(FishTable.Entry[] entries) {
        FishTable.Entry[] filteredFish;
        // loop until there's an acceptable rarity in the entries
        do {
            int rarity = Random.Shared.NextDouble() switch {
                >= 0 and < 0.60 => 1,
                >= 0.60 and < 0.85 => 2,
                >= 0.85 and < 0.95 => 3,
                >= 0.95 => 4,
                _ => 1,
            };

            filteredFish = entries.Where(x => x.Rarity == rarity).ToArray();
        } while (filteredFish.Length == 0);

        // get one fish from selected rarity
        return filteredFish.Random();
    }

    private void HandleStart(GameSession session, IByteReader packet) {
        if (session.Field == null || session.GuideObject?.Value is not FishingGuideObject fishingGuideObject) {
            return;
        }

        var fishingBlock = packet.Read<Vector3B>();
        // WIP. Here we would check if the fishing block is valid and type of liquid to get proper fish.
        if (!TableMetadata.FishTable.Entries.Values.Any(entry => entry.HabitatMapIds.Contains(session.Field.MapId))) {
            session.Send(FishingPacket.Error(FishingError.s_fishing_error_notexist_fish));
            return;
        }

        int durationTick = Constant.FisherBoreDuration;
        if (Random.Shared.NextDouble() < Constant.FishingSuccessChance) {
            if (Random.Shared.NextDouble() < Constant.FishingMiniGameChance) {
                session.FishingMiniGameActive = true;
            }
            int rodFishingTick = durationTick - fishingGuideObject.Rod.ReduceTime;
            durationTick = Random.Shared.Next(rodFishingTick - rodFishingTick / 3, rodFishingTick);
        } else {
            durationTick = Constant.FisherBoreDuration * 2; // if tick is over the base fishing tick, it will fail
        }

        session.Send(FishingPacket.Start(durationTick, session.FishingMiniGameActive));
    }

    private static void HandleFailMinigame(GameSession session) {
        session.FishingMiniGameActive = false;
    }
}
