using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.LuaFunctions;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class ResolveDeathPenaltyHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.ResolvePenalty;

    public override void Handle(GameSession session, IByteReader packet) {
        int npcObjectId = packet.ReadInt();
        if (!session.Field.Npcs.TryGetValue(npcObjectId, out FieldNpc? npc) || npc.Value.Metadata.Basic.Kind != 81) { // 81 = Doctor
            return;
        }

        if (session.Config.DeathPenaltyEndTick < session.Field.FieldTick) {
            return;
        }

        int mode = session.Field.Metadata.Property.Continent == Continent.ShadowWorld ? 1 : 0;
        int cost = Lua.CalcResolvePenaltyPrice((ushort) session.Player.Value.Character.Level, session.Config.DeathCount, mode);
        if (session.Currency.Meso < cost) {
            return;
        }

        session.Currency.Meso -= cost;
        session.Config.UpdateDeathPenalty(0);
        session.ConditionUpdate(ConditionType.resolve_panelty);
    }
}
