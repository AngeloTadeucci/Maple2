using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class PrestigeHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.Prestige;

    private enum Command : byte {
        RankReward = 3,
        MissionReward = 5,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required TableMetadataStorage TableMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.RankReward:
                HandleRankReward(session, packet);
                break;
            case Command.MissionReward:
                HandleMissionReward(session, packet);
                break;
        }
    }

    private void HandleRankReward(GameSession session, IByteReader packet) {
        int level = packet.ReadInt();

        if (level > session.Exp.PrestigeLevel) {
            return;
        }

        if (session.Exp.PrestigeRewardsClaimed.Contains(level)) {
            return;
        }

        if (!TableMetadata.PrestigeLevelRewardTable.Entries.TryGetValue(level, out PrestigeLevelRewardMetadata? metadata)) {
            return;
        }

        Item? item = session.Field?.ItemDrop.CreateItem(metadata.Id, metadata.Rarity, metadata.Value);
        if (item == null) {
            return;
        }

        if (!session.Item.Inventory.Add(item, true)) {
            session.Send(ChatPacket.Alert(StringCode.s_err_inventory));
            return;
        }

        session.Exp.PrestigeRewardsClaimed.Add(level);
        session.Send(PrestigePacket.ClaimReward(level));
    }

    private void HandleMissionReward(GameSession session, IByteReader packet) {
        int missionId = packet.ReadInt();

        PrestigeMission? mission = session.Exp.PrestigeMissions.FirstOrDefault(m => m.Id == missionId);
        if (mission == null || mission.Awarded) {
            return;
        }

        if (!session.TableMetadata.PrestigeMissionTable.Entries.TryGetValue(missionId, out PrestigeMissionMetadata? metadata)) {
            return;
        }

        Item? item = session.Field?.ItemDrop.CreateItem(metadata.Item.ItemId, metadata.Item.Rarity, metadata.Item.Amount);
        if (item == null) {
            return;
        }

        if (!session.Item.Inventory.Add(item, true)) {
            session.Item.MailItem(item);
        }

        mission.Awarded = true;
        session.Send(PrestigePacket.UpdateMissions(session.Player.Value.Account));
    }
}
