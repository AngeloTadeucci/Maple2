using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class InsigniaHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.Insignia;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required TableMetadataStorage TableMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        if (session.Field == null) {
            return;
        }

        // Check and remove any existing insignia buffs
        if (TableMetadata.InsigniaTable.Entries.TryGetValue(session.Player.Value.Character.Insignia, out InsigniaTable.Entry? oldInsigniaMetadata) && oldInsigniaMetadata.BuffId > 0) {
            session.Player.Buffs.Remove(oldInsigniaMetadata.BuffId);
        }

        short insigniaId = packet.ReadShort();
        if (!TableMetadata.InsigniaTable.Entries.TryGetValue(insigniaId, out InsigniaTable.Entry? newInsigniaMetadata)) {
            return;
        }

        session.Player.Value.Character.Insignia = insigniaId;

        bool display = false;
        switch (newInsigniaMetadata.Type) {
            case InsigniaConditionType.title:
                display = session.Player.Value.Unlock.Titles.Contains(newInsigniaMetadata.Code);
                break;
            case InsigniaConditionType.adventure_level:
                display = session.Player.Value.Account.PrestigeLevel >= 100;
                break;
            case InsigniaConditionType.trophy_point:
                display = session.Player.Value.Character.AchievementInfo.Total >= 1000;
                break;
            case InsigniaConditionType.enchant:
                display = session.Item.Equips.Gear.Any(item => item.Value.Enchant?.Enchants >= 12 && item.Value.Rarity > 3);
                break;
            case InsigniaConditionType.level:
                display = session.Player.Value.Character.Level >= 50;
                break;
            case InsigniaConditionType.vip:
                display = session.Player.Value.Account.PremiumTime > DateTime.UtcNow.ToEpochSeconds();
                break;
            default:
                Logger.Information("Unhandled insignia condition type: {type}", newInsigniaMetadata.Type);
                break;
        }

        if (display && newInsigniaMetadata.BuffId > 0) {
            session.Player.Buffs.AddBuff(session.Player, session.Player, newInsigniaMetadata.BuffId, newInsigniaMetadata.BuffLevel);
        }

        session.Field.Broadcast(InsigniaPacket.Update(session.Player.ObjectId, insigniaId, display));
    }
}
