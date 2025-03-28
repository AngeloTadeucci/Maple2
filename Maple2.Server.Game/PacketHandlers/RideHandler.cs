using Maple2.Database.Storage;
using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class RideHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.RequestRide;

    private enum Command : byte {
        Start = 0,
        Stop = 1,
        Change = 2,
        Join = 3,
        Leave = 4,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required RideMetadataStorage RideMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Start:
                HandleStart(session, packet);
                return;
            case Command.Stop:
                HandleStop(session, packet);
                return;
            case Command.Change:
                HandleChange(session, packet);
                return;
            case Command.Join:
                HandleJoin(session, packet);
                return;
            case Command.Leave:
                HandleLeave(session);
                return;
        }
    }

    private void HandleStart(GameSession session, IByteReader packet) {
        if (session.Field == null || !session.Field.Metadata.Limit.Ride || session.Ride != null) {
            session.Send(NoticePacket.MessageBox(StringCode.s_action_cant_ride));
            return;
        }

        var type = packet.Read<RideOnType>();
        int rideId = packet.ReadInt();
        packet.ReadInt(); // ObjectId, Unset
        packet.ReadInt(); // ItemId, Unset
        long itemUid = packet.ReadLong();
        // client doesn't set this data?
        //var ugc = packet.ReadClass<UgcItemLook>();

        if (type != RideOnType.UseItem) {
            return;
        }

        Item? item = session.Item.Inventory.Get(itemUid, InventoryType.Mount);
        if (item == null) {
            session.Send(NoticePacket.MessageBox(StringCode.s_item_invalid_do_not_have));
            return;
        }
        if (item.IsExpired() || item.Metadata.Property.Ride != rideId || !RideMetadata.TryGet(rideId, out RideMetadata? metadata)) {
            session.Send(NoticePacket.MessageBox(StringCode.s_item_invalid_function_item));
            return;
        }

        if (item.Metadata.Limit.TransferType == TransferType.BindOnUse) {
            session.Item.Bind(item);
        }

        int objectId = FieldManager.NextGlobalId();
        var action = new RideOnActionUseItem(rideId, objectId, item);
        session.Ride = new Ride(session.Player.ObjectId, metadata, action);
        session.Field.Broadcast(RidePacket.Start(session.Ride));
    }

    private void HandleStop(GameSession session, IByteReader packet) {
        if (session.Field == null || session.Ride == null) {
            return;
        }

        var type = packet.Read<RideOffType>();
        if (type != RideOffType.Default) {
            return;
        }

        int ownerId = session.Ride.OwnerId;
        bool forced = packet.ReadBool();
        var action = new RideOffAction(forced);

        session.Ride = null;
        session.Field.Broadcast(RidePacket.Stop(ownerId, action));
    }

    private void HandleChange(GameSession session, IByteReader packet) {
        if (session.Field == null || session.Ride == null) {
            return;
        }

        int rideId = packet.ReadInt();
        long itemUid = packet.ReadLong();

        Item? item = session.Item.Inventory.Get(itemUid, InventoryType.Mount);
        if (item == null) {
            session.Send(NoticePacket.MessageBox(StringCode.s_item_invalid_do_not_have));
            return;
        }
        if (item.IsExpired() || item.Metadata.Property.Ride != rideId) {
            session.Send(NoticePacket.MessageBox(StringCode.s_item_invalid_function_item));
            return;
        }

        session.Ride = null;
        session.Field.Broadcast(RidePacket.Change(session.Player.ObjectId, rideId, itemUid));
    }

    private void HandleJoin(GameSession session, IByteReader packet) {
        if (session.Field == null || session.Ride != null) {
            return;
        }

        int otherId = packet.ReadInt();
        if (!session.Field.TryGetPlayer(otherId, out FieldPlayer? other)) {
            return;
        }
        if (other.Session.Ride == null || other.Session.Ride.Passengers.Length == 0) {
            return;
        }

        // TODO: must be friend, guild, or party?
        // s_multi_riding_item_desc

        sbyte index = (sbyte) Array.FindIndex(other.Session.Ride.Passengers, objectId => objectId == 0);
        other.Session.Ride.Passengers[index] = session.Player.ObjectId;
        session.Ride = other.Session.Ride;

        session.Field.Broadcast(RidePacket.Join(session.Ride.OwnerId, session.Player.ObjectId, index));
    }

    private void HandleLeave(GameSession session) {
        if (session.Field == null || session.Ride == null) {
            return;
        }
        if (session.Ride.Passengers.Length == 0) {
            return;
        }

        sbyte index = (sbyte) Array.FindIndex(session.Ride.Passengers, objectId => objectId == session.Player.ObjectId);
        if (index < 0) {
            return;
        }

        int ownerId = session.Ride.OwnerId;
        if (session.Field.TryGetPlayerById(ownerId, out FieldPlayer? owner) && owner.Session.Ride == session.Ride) {
            session.Player.MoveToPosition(session.Player.Position, session.Player.Rotation);
        }

        session.Ride.Passengers[index] = 0;
        session.Ride = null;

        session.Field.Broadcast(RidePacket.Leave(ownerId, session.Player.ObjectId));
    }
}
