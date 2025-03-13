﻿using System.Net;
using Grpc.Core;
using Maple2.Database.Storage;
using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Dungeon;
using Maple2.Model.Game.Party;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.World.Service;
using Serilog;
using MigrationType = Maple2.Server.World.Service.MigrationType;

namespace Maple2.Server.Game.Manager;

public class DungeonManager {
    private readonly GameSession session;

    public IDictionary<int, DungeonRecord> Records { get; set; }
    private IDictionary<int, DungeonEnterLimit> EnterLimits => session.Player.Value.Character.DungeonEnterLimits;
    private IDictionary<int, DungeonRankReward> RankRewards => session.Player.Value.Unlock.DungeonRankRewards;

    public DungeonFieldManager? Field { get; set; }
    public DungeonRoomTable.DungeonRoomMetadata? Metadata;
    private int LobbyRoomId { get; set; }

    private Party? Party => session.Party.Party;

    private readonly ILogger logger = Log.Logger.ForContext<DungeonManager>();

    public DungeonManager(GameSession session) {
        this.session = session;
        Records = new Dictionary<int, DungeonRecord>();
        Init();
    }

    private void Init() {
        using GameStorage.Request db = session.GameStorage.Context();
        Records = db.GetDungeonRecords(session.CharacterId);
    }

    public void Load() {
        using GameStorage.Request db = session.GameStorage.Context();
        foreach (DungeonRoomTable.DungeonRoomMetadata metadata in session.TableMetadata.DungeonRoomTable.Entries.Values) {
            if (!Records.TryGetValue(metadata.Id, out DungeonRecord? record)) {
                record = new DungeonRecord(metadata.Id);
                record = db.CreateDungeonRecord(record, session.CharacterId);
                if (record == null) {
                    logger.Error("Failed to create dungeon record for dungeonId {dungeonId}", metadata.Id);
                    continue;
                }
                Records.Add(metadata.Id, record);
            }

            DungeonEnterLimit limit = GetEnterLimit(metadata.Limit);
            EnterLimits[metadata.Id] = limit;
        }

        session.Send(DungeonRoomPacket.Load(Records));
        session.Send(DungeonRoomPacket.RankRewards(RankRewards));
        session.Send(FieldEntrancePacket.Load(EnterLimits));
    }

    public void UpdateDungeonEnterLimit() {
        bool updated = false;
        foreach (int dungeonId in EnterLimits.Keys) {
            if (!session.TableMetadata.DungeonRoomTable.Entries.TryGetValue(dungeonId, out DungeonRoomTable.DungeonRoomMetadata? metadata)) {
                continue;
            }

            DungeonEnterLimit newLimit = GetEnterLimit(metadata.Limit);
            if (EnterLimits[dungeonId] != newLimit) {
                EnterLimits[dungeonId] = newLimit;
                updated = true;
            }
        }

        if (updated) {
            session.PlayerInfo.SendUpdate(new PlayerUpdateRequest {
                AccountId = session.AccountId,
                CharacterId = session.CharacterId,
                DungeonEnterLimits = { EnterLimits.Select(dungeon => new DungeonEnterLimitUpdate {
                    DungeonId = dungeon.Key,
                    Limit = (int) dungeon.Value,
                })},
                Async = true,
            });
            session.Send(FieldEntrancePacket.Load(EnterLimits));
        }
    }

    private DungeonEnterLimit GetEnterLimit(DungeonRoomLimit metadata) {
        if (metadata.MinLevel > session.Player.Value.Character.Level) {
            return DungeonEnterLimit.MinLevel;
        }

        if (metadata.RequiredAchievementId > 0 && !session.Achievement.HasAchievement(metadata.RequiredAchievementId)) {
            return DungeonEnterLimit.Achievement;
        }

        if (metadata.VipOnly && !session.Config.IsPremiumClubActive()) {
            return DungeonEnterLimit.Vip;
        }

        if (metadata.GearScore > 0 && session.Stats.Values.GearScore < metadata.GearScore) {
            return DungeonEnterLimit.Gearscore;
        }

        if (metadata.ClearDungeonIds.Length > 0) {
            foreach (int dungeonId in metadata.ClearDungeonIds) {
                if (!Records.TryGetValue(dungeonId, out DungeonRecord? record) || record.ClearTimestamp == 0) {
                    return DungeonEnterLimit.DungeonClear;
                }
            }
        }

        if (metadata.Buffs.Length > 0 && metadata.Buffs.Any(buffId => !session.Buffs.HasBuff(buffId))) {
            return DungeonEnterLimit.Buff;
        }

        if (metadata.EquippedRecommendedWeapon) {
            if (!session.Item.Equips.Gear.TryGetValue(EquipSlot.LH, out Item? item)) {
                return DungeonEnterLimit.RecommendedWeapon;
            }
            switch (session.Player.Value.Character.Job.Code()) {
                case JobCode.Newbie:
                    if (!item.Type.IsBludgeon) {
                        return DungeonEnterLimit.RecommendedWeapon;
                    }
                    break;
                case JobCode.Knight:
                    if (!item.Type.IsLongsword) {
                        return DungeonEnterLimit.RecommendedWeapon;
                    }
                    break;
                case JobCode.Berserker:
                    if (!item.Type.IsGreatsword) {
                        return DungeonEnterLimit.RecommendedWeapon;
                    }
                    break;
                case JobCode.Wizard:
                    if (!item.Type.IsStaff) {
                        return DungeonEnterLimit.RecommendedWeapon;
                    }
                    break;
                case JobCode.Priest:
                    if (!item.Type.IsScepter) {
                        return DungeonEnterLimit.RecommendedWeapon;
                    }
                    break;
                case JobCode.Archer:
                    if (!item.Type.IsBow) {
                        return DungeonEnterLimit.RecommendedWeapon;
                    }
                    break;
                case JobCode.HeavyGunner:
                    if (!item.Type.IsCannon) {
                        return DungeonEnterLimit.RecommendedWeapon;
                    }
                    break;
                case JobCode.Assassin:
                    if (!item.Type.IsThrowingStar) {
                        return DungeonEnterLimit.RecommendedWeapon;
                    }
                    break;
                case JobCode.Thief:
                    if (!item.Type.IsDagger) {
                        return DungeonEnterLimit.RecommendedWeapon;
                    }
                    break;
                case JobCode.RuneBlader:
                    if (!item.Type.IsBlade) {
                        return DungeonEnterLimit.RecommendedWeapon;
                    }
                    break;
                case JobCode.Striker:
                    if (!item.Type.IsKnuckle) {
                        return DungeonEnterLimit.RecommendedWeapon;
                    }
                    break;
                case JobCode.SoulBinder:
                    if (!item.Type.IsOrb) {
                        return DungeonEnterLimit.RecommendedWeapon;
                    }
                    break;
            }
        }
        return DungeonEnterLimit.None;
    }

    public void SetDungeon(DungeonFieldManager field) {
        Field = field;
        Metadata = field.Metadata;
    }

    public void CreateDungeonRoom(int dungeonId, bool withParty) {
        if (!session.TableMetadata.DungeonRoomTable.Entries.TryGetValue(dungeonId, out DungeonRoomTable.DungeonRoomMetadata? metadata)) {
            session.Send(DungeonRoomPacket.Error(DungeonRoomError.s_room_dungeon_notOpenTimeDungeon));
            return;
        }

        if (EnterLimits.TryGetValue(dungeonId, out DungeonEnterLimit limit) && limit != DungeonEnterLimit.None) {
            session.Send(FieldEntrancePacket.Error(dungeonId, limit));
            return;
        }

        if (withParty) {
            if (Party == null) {
                session.Send(DungeonRoomPacket.Error(DungeonRoomError.s_room_dungeon_error_invalidPartyOID));
                return;
            }
            if (Party.LeaderCharacterId != session.CharacterId) {
                session.Send(DungeonRoomPacket.Error(DungeonRoomError.s_room_party_err_not_chief));
                return;
            }
        }


        DungeonFieldManager? dungeonField = session.FieldFactory.CreateDungeon(metadata, session.CharacterId, Party);
        if (dungeonField == null) {
            session.Send(DungeonRoomPacket.Error(DungeonRoomError.s_room_dungeon_NotAllowTime));
            return;
        }
        LobbyRoomId = dungeonField.RoomId;

        SetDungeon(dungeonField);

        if (withParty) {
            var request = new PartyRequest {
                RequestorId = session.CharacterId,
                SetDungeon = new PartyRequest.Types.SetDungeon {
                    PartyId = Party!.Id,
                    DungeonId = dungeonId,
                    DungeonRoomId = dungeonField.RoomId,
                    Set = true,
                },
            };

            try {
                session.World.Party(request);
            } catch (RpcException) { }
        }

        MigrateToDungeon();
    }

    public void EnterLobby() {
        if (Party == null) {
            return;
        }
        FieldManager? field = session.Field.FieldFactory.Get(roomId: Party.DungeonLobbyRoomId);
        if (field is not DungeonFieldManager dungeonField) {
            return;
        }

        SetDungeon(dungeonField);
        MigrateToDungeon();
    }

    public void EnterInitField() {
        if (Field == null || Metadata == null) {
            logger.Error("Field is null, cannot enter dungeon");
            return;
        }

        if (!Field.RoomFields.TryGetValue(Metadata.FieldIds[0], out DungeonFieldManager? firstField)) {
            logger.Error("First field is null, cannot enter dungeon");
            return;
        }

        session.Send(session.PrepareField(firstField.MapId, roomId: firstField.RoomId) ? FieldEnterPacket.Request(session.Player) :
            FieldEnterPacket.Error(MigrationError.s_move_err_default));
    }

    public void SetDungeon(int dungeonId, int roomId, bool set) {
        if (set) {
            if (!session.TableMetadata.DungeonRoomTable.Entries.TryGetValue(dungeonId, out DungeonRoomTable.DungeonRoomMetadata? metadata)) {
                logger.Error("Dungeon metadata not found for dungeonId {dungeonId}", dungeonId);
                return;
            }

            Metadata = metadata;
            LobbyRoomId = roomId;
        } else {
            Metadata = null;
            Field = null;
            LobbyRoomId = 0;
        }
    }

    private void MigrateToDungeon() {
        if (Metadata == null) {
            logger.Error("Dungeon metadata is null, cannot migrate to dungeon");
            return;
        }
        try {
            var request = new MigrateOutRequest {
                AccountId = session.AccountId,
                CharacterId = session.CharacterId,
                MachineId = session.MachineId.ToString(),
                Server = Server.World.Service.Server.Game,
                MapId = Metadata.LobbyFieldId,
                RoomId = LobbyRoomId,
                Type = MigrationType.Dungeon,
            };

            MigrateOutResponse response = session.World.MigrateOut(request);
            var endpoint = new IPEndPoint(IPAddress.Parse(response.IpAddress), response.Port);
            session.Send(MigrationPacket.GameToGame(endpoint, response.Token, Metadata.LobbyFieldId));
            session.State = SessionState.ChangeMap;
        } catch (RpcException ex) {
            session.Send(MigrationPacket.GameToGameError(MigrationError.s_move_err_default));
            session.Send(NoticePacket.Disconnect(new InterfaceText(ex.Message)));
        } finally {
            session.Disconnect();
        }
    }

    public void Save(GameStorage.Request db) {
        db.SaveDungeonRecords(session.CharacterId, Records.Values.ToArray());
    }
}
