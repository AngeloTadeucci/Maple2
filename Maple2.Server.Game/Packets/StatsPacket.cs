using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;

namespace Maple2.Server.Game.Packets;

public static class StatsPacket {
    private enum Command : byte {
        Update = 0,
    }

    public static ByteWriter Init(Actor<Player> entity) {
        Stats stats = entity.Stats.Values;

        var pWriter = Packet.Of(SendOp.Stat);
        pWriter.WriteInt(entity.ObjectId);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteByte(Stats.BASIC_TOTAL);
        for (int i = 0; i < Stats.BASIC_TOTAL; i++) {
            var attribute = (BasicAttribute) i;
            pWriter.WriteAttribute(attribute, stats[attribute]);
        }

        return pWriter;
    }

    public static ByteWriter Update<T>(Actor<T> entity) {
        var pWriter = Packet.Of(SendOp.Stat);
        pWriter.WriteInt(entity.ObjectId);
        pWriter.Write<Command>(Command.Update);
        switch (entity.Value) {
            case Player:
                pWriter.WritePlayerStats(entity.Stats.Values);
                break;
            default:
                pWriter.WriteNpcStats(entity.Stats.Values);
                break;
        }

        return pWriter;
    }

    public static ByteWriter Update(IActor entity, params BasicAttribute[] attributes) {
        var pWriter = Packet.Of(SendOp.Stat);
        pWriter.WriteInt(entity.ObjectId);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteByte((byte) attributes.Length);
        foreach (BasicAttribute attribute in attributes) {
            pWriter.Write<BasicAttribute>(attribute);
            pWriter.WriteAttribute(attribute, entity.Stats.Values[attribute]);
        }

        return pWriter;
    }

    // Note: this is a copy of above to reduce array allocations.
    public static ByteWriter Update(IActor entity, BasicAttribute attribute) {
        var pWriter = Packet.Of(SendOp.Stat);
        pWriter.WriteInt(entity.ObjectId);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteByte(1);
        pWriter.Write<BasicAttribute>(attribute);
        pWriter.WriteAttribute(attribute, entity.Stats.Values[attribute]);

        return pWriter;
    }

    #region Helpers
    public static void WritePlayerStats(this IByteWriter pWriter, Stats stats) {
        pWriter.WriteByte(Stats.BASIC_TOTAL);
        for (int i = 0; i < Stat.TOTAL; i++) {
            pWriter.WriteLong(stats[BasicAttribute.Health][i]);
            pWriter.WriteInt((int) stats[BasicAttribute.AttackSpeed][i]);
            pWriter.WriteInt((int) stats[BasicAttribute.MovementSpeed][i]);
            pWriter.WriteInt((int) stats[BasicAttribute.JumpHeight][i]);
            pWriter.WriteInt((int) stats[BasicAttribute.MountSpeed][i]);
        }
    }

    public static void WriteNpcStats(this IByteWriter pWriter, Stats stats) {
        pWriter.WriteByte(Stats.BASIC_TOTAL);
        for (int i = 0; i < Stat.TOTAL; i++) {
            pWriter.WriteLong(stats[BasicAttribute.Health][i]);
            pWriter.WriteInt((int) stats[BasicAttribute.AttackSpeed][i]);
        }
    }

    private static void WriteAttribute(this IByteWriter pWriter, BasicAttribute attribute, Stat stat) {
        for (int i = 0; i < Stat.TOTAL; i++) {
            if (attribute == BasicAttribute.Health) {
                pWriter.WriteLong(stat[i]);
            } else {
                pWriter.WriteInt((int) stat[i]);
            }
        }
    }
    #endregion
}
