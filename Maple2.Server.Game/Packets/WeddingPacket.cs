using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class WeddingPacket {
    private enum Command : byte {
        UpdateMarriage = 0,
        UpdateHall = 1,
        Error = 2,
        Proposal = 3,
        Engaged = 6,
        Reserve = 13,
        ConfirmReservation = 15,
        RequestCancelReservation = 16,
        DeclinedCancelReservation = 17,
        DeclineCancelReservation = 18,
        AcceptCancelReservation = 19,
        ChangeReservationRequest = 20,
        DeclineReservationChange = 21,
        AcceptReservationChange = 22,
        Unknown23 = 23,
        SearchInvitee = 27,
        SendInvite = 29,
        StartCeremony = 30,
    }

    public static ByteWriter UpdateMarriage(Marriage marriage) {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.UpdateMarriage);
        pWriter.WriteClass<Marriage>(marriage);

        return pWriter;
    }

    public static ByteWriter UpdateHall(WeddingHall hall) {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.UpdateHall);
        pWriter.WriteClass<WeddingHall>(hall);
        pWriter.WriteInt();

        return pWriter;
    }

    public static ByteWriter Error(WeddingError error) {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<WeddingError>(error);

        return pWriter;
    }

    public static ByteWriter Proposal(string name, long characterId) {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.Proposal);
        pWriter.WriteLong(characterId);
        pWriter.WriteUnicodeString(name);

        return pWriter;
    }

    public static ByteWriter Engaged(Marriage marriage) {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.Engaged);
        pWriter.WriteClass<Marriage>(marriage);

        return pWriter;
    }

    public static ByteWriter Reserve(WeddingHall hall) {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.Reserve);
        pWriter.WriteClass<WeddingHall>(hall);

        return pWriter;
    }

    public static ByteWriter ConfirmReservation(WeddingHall hall) {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.ConfirmReservation);
        pWriter.WriteClass<WeddingHall>(hall);

        return pWriter;
    }

    public static ByteWriter RequestCancelReservation() {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.RequestCancelReservation);

        return pWriter;
    }

    public static ByteWriter DeclineCancelReservation() {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.DeclineCancelReservation);

        return pWriter;
    }

    public static ByteWriter DeclinedCancelReservation(WeddingError reply) {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.DeclinedCancelReservation);
        pWriter.Write<WeddingError>(reply);

        return pWriter;
    }

    public static ByteWriter AcceptCancelReservation(WeddingError reply) {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.AcceptCancelReservation);
        pWriter.Write<WeddingError>(reply);

        return pWriter;
    }

    public static ByteWriter ChangeReservationRequest(WeddingHall hall, long differenceCost) {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.ChangeReservationRequest);
        pWriter.WriteClass<WeddingHall>(hall);
        pWriter.WriteLong(differenceCost);

        return pWriter;
    }

    public static ByteWriter DeclineReservationChange() {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.DeclineReservationChange);

        return pWriter;
    }

    public static ByteWriter AcceptReservationChange(WeddingHall hall, long differenceCost) {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.AcceptReservationChange);
        pWriter.WriteClass<WeddingHall>(hall);
        pWriter.WriteLong(differenceCost);

        return pWriter;
    }

    /// <summary>
    /// Gets sent every field load? not sure why
    /// </summary>
    /// <returns></returns>
    public static ByteWriter Unknown23() {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.Unknown23);

        return pWriter;
    }
}
