using Maple2.Model.Error;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;
using Maple2.Model.Game.Party;

namespace Maple2.Server.Game.Packets;

public static class PartySearchPacket {
    private enum Command : byte {
        Add = 0,
        Remove = 1,
        Load = 2,
        Error = 4,
    }

    public static ByteWriter Add(PartySearch search) {
        var pWriter = Packet.Of(SendOp.PartySearch);
        pWriter.Write<Command>(Command.Add);
        pWriter.WriteClass<PartySearch>(search);

        return pWriter;
    }

    public static ByteWriter Remove(long partySearchId) {
        var pWriter = Packet.Of(SendOp.PartySearch);
        pWriter.Write<Command>(Command.Add);
        pWriter.WriteLong(partySearchId);

        return pWriter;
    }

    public static ByteWriter Load(ICollection<PartySearch> entries) {
        var pWriter = Packet.Of(SendOp.PartySearch);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(entries.Count);
        foreach (PartySearch entry in entries) {
            pWriter.WriteBool(true);
            pWriter.WriteClass<PartySearch>(entry);
        }

        return pWriter;
    }

    public static ByteWriter Error(PartySearchError error, int category = 0) {
        var pWriter = Packet.Of(SendOp.PartySearch);
        pWriter.Write<Command>(Command.Error);
        pWriter.WriteInt(category);
        pWriter.Write<PartySearchError>(error);

        return pWriter;
    }
}
