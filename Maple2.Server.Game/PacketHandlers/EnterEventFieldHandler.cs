using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game.Event;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class EnterEventFieldHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.EnterEventField;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required GameStorage GameStorage { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        using GameStorage.Request db = GameStorage.Context();
        GameEvent? gameEvent = session.FindEvent(GameEventType.EventFieldPopup).FirstOrDefault();
        if (gameEvent?.Metadata.Data is not EventFieldPopup fieldPopup) {
            session.Send(ChatPacket.Alert(StringCode.s_err_timeevent_move_field));
            return;
        }

        if (session.Player.Field.MapId == fieldPopup.MapId) {
            session.Send(ChatPacket.Alert(StringCode.s_err_timeevent_samefield));
            return;
        }

        if (session.Player.InBattle) {
            session.Send(ChatPacket.Alert(StringCode.s_err_timeevent_battle));
            return;
        }

        session.Send(session.PrepareField(fieldPopup.MapId)
            ? FieldEnterPacket.Request(session.Player)
            : FieldEnterPacket.Error(MigrationError.s_move_err_default));
    }
}
