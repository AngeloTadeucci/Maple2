using System.Numerics;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util;
using Maple2.Tools.Extensions;
using static Maple2.Model.Error.MigrationError;

namespace Maple2.Server.Game.PacketHandlers;

public class TaxiHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.RequestTaxi;

    private enum Command : byte {
        Taxi = 1,
        RotorsAir = 2,
        MesoAir = 3,
        MeretAir = 4,
        Discover = 5,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required MapMetadataStorage MapMetadata { private get; init; }
    public required MapEntityStorage EntityMetadata { private get; init; }
    public required WorldMapGraphStorage WorldMapGraph { private get; init; }
    public required Lua.Lua Lua { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();

        switch (command) {
            case Command.Taxi:
                HandleTaxi(session, packet);
                return;
            case Command.RotorsAir:
                HandleRotorsAirTaxi(session, packet);
                return;
            case Command.MesoAir:
                HandleMesoAirTaxi(session, packet);
                return;
            case Command.MeretAir:
                HandleMeretAirTaxi(session, packet);
                return;
            case Command.Discover:
                HandleDiscoverTaxi(session);
                return;
        }
    }

    private void HandleTaxi(GameSession session, IByteReader packet) {
        int mapId = packet.ReadInt();

        if (!CheckMapCashCall(session, mapId, MapMetadata)) {
            return;
        }

        if (!session.Player.Value.Unlock.Taxis.Contains(mapId)) {
            return;
        }

        if (!MapMetadata.TryGet(mapId, out MapMetadata? map)) {
            return;
        }

        MapEntityMetadata? entities = EntityMetadata.Get(map.XBlock);
        if (entities?.Taxi == null) {
            return;
        }

        if (!WorldMapGraph.CanPathFind(session.Player.Value.Character.MapId, mapId, out int mapCount)) {
            return;
        }

        int cost = Lua.CalcTaxiCharge(mapCount, (ushort) session.Player.Value.Character.Level);

        if (session.Currency.Meso < cost) {
            session.Send(NoticePacket.MessageBox(StringCode.s_err_lack_meso));
            return;
        }
        session.Currency.Meso -= cost;

        Vector3 position = entities.Taxi.Position.Offset(-VectorExtensions.BLOCK_SIZE, entities.Taxi.Rotation);
        Vector3 rotation = entities.Taxi.Rotation;
        session.Send(session.PrepareField(mapId, position: position, rotation: rotation)
            ? FieldEnterPacket.Request(session.Player)
            : FieldEnterPacket.Error(s_move_err_default));
    }

    private void HandleRotorsAirTaxi(GameSession session, IByteReader packet) {
        int mapId = packet.ReadInt();
        if (!CheckMapCashCall(session, mapId, MapMetadata)) {
            return;
        }

        session.Send(session.PrepareField(mapId)
            ? FieldEnterPacket.Request(session.Player)
            : FieldEnterPacket.Error(s_move_err_default));
    }

    private void HandleMesoAirTaxi(GameSession session, IByteReader packet) {
        int mapId = packet.ReadInt();
        if (!CheckMapCashCall(session, mapId, MapMetadata)) {
            return;
        }

        int cost = Lua.CalcAirTaxiCharge((ushort) session.Player.Value.Character.Level);

        if (session.Currency.Meso < cost) {
            session.Send(NoticePacket.MessageBox(StringCode.s_err_lack_meso));
            return;
        }

        session.Currency.Meso -= cost;

        session.Send(session.PrepareField(mapId)
            ? FieldEnterPacket.Request(session.Player)
            : FieldEnterPacket.Error(s_move_err_default));
    }

    private void HandleMeretAirTaxi(GameSession session, IByteReader packet) {
        int mapId = packet.ReadInt();
        if (!CheckMapCashCall(session, mapId, MapMetadata)) {
            return;
        }


        if (session.Currency.Meret < Constant.MeretAirTaxiPrice) {
            session.Send(NoticePacket.MessageBox(StringCode.s_err_lack_meso));
            return;
        }

        session.Currency.Meret -= Constant.MeretAirTaxiPrice;

        session.Send(session.PrepareField(mapId)
            ? FieldEnterPacket.Request(session.Player)
            : FieldEnterPacket.Error(s_move_err_default));
    }

    private static void HandleDiscoverTaxi(GameSession session) {
        int mapId = session.Player.Value.Character.MapId;
        session.Player.Value.Unlock.Taxis.Add(mapId);
        session.Send(TaxiPacket.RevealTaxi(mapId));
        session.ConditionUpdate(ConditionType.taxifind);
        session.Exp.AddExp(ExpType.taxi);
    }

    private static bool CheckMapCashCall(GameSession session, int mapId, MapMetadataStorage mapMetadataStorage) {
        MapMetadataCashCall currentCashCall = session.Field.Metadata.CashCall;
        if (!currentCashCall.TaxiDeparture) {
            session.Send(NoticePacket.MessageBox(StringCode.s_err_cash_taxi_cannot_departure));
            return false;
        }

        if (!mapMetadataStorage.TryGet(mapId, out MapMetadata? map)) {
            return false;
        }

        if (!map.CashCall.TaxiDestination) {
            session.Send(NoticePacket.MessageBox(StringCode.s_err_cash_taxi_cannot_destination));
            return false;
        }

        return true;
    }
}
