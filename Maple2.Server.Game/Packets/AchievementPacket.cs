using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class AchievementPacket {
    private enum Command : byte {
        Initialize = 0,
        Load = 1,
        Update = 2,
        Favorite = 4,
    }

    public static ByteWriter Initialize() {
        var pWriter = Packet.Of(SendOp.Achieve);
        pWriter.Write<Command>(Command.Initialize);

        return pWriter;
    }

    public static ByteWriter Load(IList<Achievement> achievements) {
        var pWriter = Packet.Of(SendOp.Achieve);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(achievements.Count);

        foreach (Achievement achievement in achievements) {
            pWriter.WriteInt(achievement.Id);
            pWriter.WriteInt(1); // Unknown
            pWriter.WriteClass<Achievement>(achievement);
        }

        return pWriter;
    }

    public static ByteWriter Update(Achievement achievement) {
        var pWriter = Packet.Of(SendOp.Achieve);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteInt(achievement.Id);
        pWriter.WriteClass<Achievement>(achievement);

        return pWriter;
    }

    public static ByteWriter Favorite(Achievement achievement) {
        var pWriter = Packet.Of(SendOp.Achieve);
        pWriter.Write<Command>(Command.Favorite);
        pWriter.WriteInt(achievement.Id);
        pWriter.WriteBool(achievement.Favorite);

        return pWriter;
    }
}
