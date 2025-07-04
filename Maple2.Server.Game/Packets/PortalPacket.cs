using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;

namespace Maple2.Server.Game.Packets;

public static class PortalPacket {
    private enum Command : byte {
        Add = 0,
        Remove = 1,
        Update = 2,
        Move = 3,
    }

    public static ByteWriter Add(FieldPortal fieldPortal) {
        Portal portal = fieldPortal;

        var pWriter = Packet.Of(SendOp.FieldPortal);
        pWriter.Write<Command>(Command.Add);
        pWriter.WriteInt(portal.Id);
        pWriter.WriteBool(fieldPortal.Visible);
        pWriter.WriteBool(fieldPortal.Enabled);
        pWriter.Write<Vector3>(fieldPortal.Position);
        pWriter.Write<Vector3>(fieldPortal.Rotation);
        pWriter.Write<Vector3>(portal.Dimension);
        pWriter.WriteUnicodeString(fieldPortal.Model);
        pWriter.WriteInt(portal.TargetMapId);
        pWriter.WriteInt(fieldPortal.ObjectId);
        pWriter.Write<PortalActionType>(portal.ActionType);
        pWriter.WriteBool(fieldPortal.MinimapVisible);
        pWriter.WriteLong(fieldPortal.HomeId);
        pWriter.Write<PortalType>(portal.Type);
        pWriter.WriteInt(fieldPortal.EndTick);
        pWriter.WriteShort();
        pWriter.WriteInt(fieldPortal.StartTick);
        pWriter.WriteBool(!string.IsNullOrEmpty(fieldPortal.Password));
        pWriter.WriteUnicodeString(fieldPortal.OwnerName);
        pWriter.WriteUnicodeString();
        pWriter.WriteUnicodeString();

        return pWriter;
    }

    public static ByteWriter Remove(int portalId) {
        var pWriter = Packet.Of(SendOp.FieldPortal);
        pWriter.Write<Command>(Command.Remove);
        pWriter.WriteInt(portalId);

        return pWriter;
    }

    public static ByteWriter Update(FieldPortal fieldPortal, short unknown = 0) {
        var pWriter = Packet.Of(SendOp.FieldPortal);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteInt(fieldPortal.Value.Id);
        pWriter.WriteBool(fieldPortal.Visible);
        pWriter.WriteBool(fieldPortal.Enabled);
        pWriter.WriteBool(fieldPortal.MinimapVisible);
        pWriter.WriteShort(unknown);

        return pWriter;
    }

    public static ByteWriter Move(FieldPortal fieldPortal) {
        var pWriter = Packet.Of(SendOp.FieldPortal);
        pWriter.Write<Command>(Command.Move);
        pWriter.Write<Vector3>(fieldPortal.Position);
        pWriter.Write<Vector3>(fieldPortal.Rotation);

        return pWriter;
    }

    public static ByteWriter MoveByPortal(IActor actor, Portal portal) {
        var pWriter = Packet.Of(SendOp.UserMoveByPortal);
        pWriter.WriteInt(actor.ObjectId);
        pWriter.Write<Vector3>(portal.Position + new Vector3(0, 0, 25)); // Always seems to be offset by 25
        pWriter.Write<Vector3>(portal.Rotation);
        pWriter.WriteBool(true); // isPortal?

        return pWriter;
    }

    public static ByteWriter MoveByPortal(IActor actor, Vector3 position, Vector3 rotation) {
        var pWriter = Packet.Of(SendOp.UserMoveByPortal);
        pWriter.WriteInt(actor.ObjectId);
        pWriter.Write<Vector3>(position + new Vector3(0, 0, 25)); // Always seems to be offset by 25
        pWriter.Write<Vector3>(rotation);
        pWriter.WriteBool(false); // isPortal?

        return pWriter;
    }
}
