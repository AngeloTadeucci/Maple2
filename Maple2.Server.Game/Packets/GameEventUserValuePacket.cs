using Maple2.Database.Extensions;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class GameEventUserValuePacket {
    private enum Command : byte {
        Load = 0,
        Update = 1,
    }

    public static ByteWriter Load(IList<GameEventUserValue> userValues) {
        var pWriter = Packet.Of(SendOp.GameEventUserValue);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteByte();
        pWriter.WriteInt(userValues.Count);
        foreach (GameEventUserValue userValue in userValues) {
            pWriter.WriteClass<GameEventUserValue>(userValue);
        }

        return pWriter;
    }

    public static ByteWriter Update(GameEventUserValue userValue) {
        var pWriter = Packet.Of(SendOp.GameEventUserValue);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteByte();
        pWriter.WriteClass<GameEventUserValue>(userValue);

        return pWriter;
    }

    public static ByteWriter Test(int i) {
        var pWriter = Packet.Of(SendOp.GameEventUserValue);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteByte();

        //pWriter.WriteInt(1);
        pWriter.WriteInt(i);
        pWriter.WriteInt(700001);
        pWriter.WriteUnicodeString("False");
        pWriter.WriteLong(DateTime.Now.AddYears(1).ToEpochSeconds());

        return pWriter;
    }
}
