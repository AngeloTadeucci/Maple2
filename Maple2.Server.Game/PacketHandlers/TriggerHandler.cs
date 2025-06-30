using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Model.Widget;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class TriggerHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.Trigger;

    private enum Command : byte {
        Unknown4 = 4,
        SkipCutscene = 7,
        Ui = 8,
        LoadScript = 10,
        SaveScript = 12,
        DiscardScript = 21,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Unknown4:
                return;
            case Command.SkipCutscene:
                HandleSkipCutscene(session);
                return;
            case Command.Ui:
                HandleUpdateWidget(session, packet);
                return;
            case Command.LoadScript:
                HandleLoadScript(session, packet);
                return;
            case Command.SaveScript:
                HandleSaveScript(session, packet);
                return;
            case Command.DiscardScript:
                HandleDiscardScript(session, packet);
                return;
        }
    }

    private void HandleSkipCutscene(GameSession session) {
        if (session.Field == null) {
            return;
        }

        foreach (FieldTrigger trigger in session.Field.EnumerateTrigger()) {
            if (trigger.Skip()) {
                return;
            }
        }
    }

    private void HandleUpdateWidget(GameSession session, IByteReader packet) {
        if (session.Field == null) {
            return;
        }

        var widgetType = packet.Read<WidgetType>();
        int arg = packet.ReadInt();
        switch (widgetType) {
            case WidgetType.Guide:
                if (session.Field.Widgets.TryGetValue(widgetType.ToString(), out Widget? guideWidget)) {
                    guideWidget.Conditions["IsTriggerEvent"] = arg;
                }
                break;
            case WidgetType.SceneMovie:
                if (session.Field.Widgets.TryGetValue(widgetType.ToString(), out Widget? sceneMovieWidget)) {
                    sceneMovieWidget.Conditions["IsStop"] = arg;
                    session.Send(TriggerPacket.UiSkipMovie(arg));
                }
                break;
            case WidgetType.Round:
                // TODO: This is all a guess
                if (session.Field.Widgets.TryGetValue(widgetType.ToString(), out Widget? roundWidget)) {
                    switch (arg) {
                        case 0: // 0 = FailGameProgress
                            roundWidget.Conditions["GameFail"] = 0;
                            break;
                        case 1: // 1 = SuccessGameProgress
                            roundWidget.Conditions["GameClear"] = 0;
                            break;
                    }
                }
                break;
            default:
                Logger.Warning("Unhandled widget type: {WidgetType}", widgetType);
                break;
        }
    }

    private void HandleLoadScript(GameSession session, IByteReader packet) {
        int cubeId = packet.ReadInt();
    }

    private void HandleSaveScript(GameSession session, IByteReader packet) {
        int cubeId = packet.ReadInt();
        string xml = packet.ReadString();
    }

    private void HandleDiscardScript(GameSession session, IByteReader packet) {
        int cubeId = packet.ReadInt();
    }
}
