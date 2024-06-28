using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Scripting.Trigger;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public class MassiveEventPacket {
    private enum Command : byte {
        Round = 0,
        Countdown = 1,
        Banner = 2,
        Text = 3,
        RoundCountdown = 4,
        Winner = 5,
        GameOver = 6,
        StartRound = 7,
        PvpCountdown = 8,
    }

    public static ByteWriter Round(int round, int maxRound, int minRound, int verticalOffset = 0) {
        var pWriter = Packet.Of(SendOp.MassiveEvent);
        pWriter.Write<Command>(Command.Round);
        pWriter.WriteInt(round);
        pWriter.WriteInt(maxRound);
        pWriter.WriteInt(minRound);
        pWriter.WriteInt(verticalOffset);

        return pWriter;
    }

    public static ByteWriter Countdown(string text, int round, int countdown, int unknown = 1) {
        var pWriter = Packet.Of(SendOp.MassiveEvent);
        pWriter.Write<Command>(Command.Countdown);
        pWriter.WriteUnicodeString(text);
        pWriter.WriteInt(round);
        pWriter.WriteInt(countdown);
        pWriter.WriteInt(unknown);

        return pWriter;
    }

    public static ByteWriter Banner(BannerType type, string text, int duration) {
        var pWriter = Packet.Of(SendOp.MassiveEvent);
        pWriter.Write<Command>(Command.Banner);
        pWriter.Write<BannerType>(type);
        pWriter.WriteUnicodeString(text);
        pWriter.WriteInt(duration); // countdown if type=8

        return pWriter;
    }

    public static ByteWriter Text(InterfaceText text, int duration) {
        var pWriter = Packet.Of(SendOp.MassiveEvent);
        pWriter.Write<Command>(Command.Text);
        pWriter.WriteClass<InterfaceText>(text);
        pWriter.WriteInt(duration);

        return pWriter;
    }

    public static ByteWriter RoundCountdown(InterfaceText text, int round, int countdown, int unknown = 1) {
        var pWriter = Packet.Of(SendOp.MassiveEvent);
        pWriter.Write<Command>(Command.RoundCountdown);
        pWriter.WriteClass<InterfaceText>(text);
        pWriter.WriteInt(round);
        pWriter.WriteInt(countdown);
        pWriter.WriteInt(unknown);

        return pWriter;
    }

    public static ByteWriter Winner(InterfaceText text, int duration) {
        var pWriter = Packet.Of(SendOp.MassiveEvent);
        pWriter.Write<Command>(Command.Winner);
        pWriter.WriteClass<InterfaceText>(text);
        pWriter.WriteInt(duration);

        return pWriter;
    }

    public static ByteWriter GameOver(InterfaceText text, int duration) {
        var pWriter = Packet.Of(SendOp.MassiveEvent);
        pWriter.Write<Command>(Command.GameOver);
        pWriter.WriteClass<InterfaceText>(text);
        pWriter.WriteInt(duration);

        return pWriter;
    }

    public static ByteWriter StartRound(int round, int duration, bool isFinal = false) {
        var pWriter = Packet.Of(SendOp.MassiveEvent);
        pWriter.Write<Command>(Command.StartRound);
        pWriter.WriteInt(round);
        pWriter.WriteBool(isFinal);
        pWriter.WriteInt(duration);

        return pWriter;
    }

    public static ByteWriter PvpCountdown(int countdown) {
        var pWriter = Packet.Of(SendOp.MassiveEvent);
        pWriter.Write<Command>(Command.PvpCountdown);
        pWriter.WriteInt(countdown);

        return pWriter;
    }
}
