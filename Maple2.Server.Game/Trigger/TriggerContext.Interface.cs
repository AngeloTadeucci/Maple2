using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Model.Widget;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Scripting.Trigger;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Trigger;

public partial class TriggerContext {
    public void CreateWidget(string type) {
        DebugLog("[CreateWidget] type:{Type}", type);
        var widget = type switch {
            "OxQuiz" => new OxQuizWidget(Field),
            _ => new Widget(Field),
        };
        Field.Widgets[type] = widget;
    }

    public void GuideEvent(int eventId) {
        DebugLog("[GuideEvent] eventId:{Id}", eventId);
        Broadcast(TriggerPacket.UiGuide(eventId));
    }

    public void HideGuideSummary(int entityId, int textId) {
        DebugLog("[HideGuideSummary] entityId:{EntityId}, textId:{TextId}", entityId, textId);
        Broadcast(TriggerPacket.UiHideSummary(entityId));
    }

    public void Notice(int type, string script, bool arg3) {
        DebugLog("[Notice] type:{Type}, script:{Script}, arg3:{Arg3}", type, script, arg3);
        Broadcast(NoticePacket.Notice(NoticePacket.Flags.Mint, new InterfaceText(script), 3000));
    }

    public void PlaySystemSoundByUserTag(int userTagId, string soundKey) {
        DebugLog("[PlaySystemSoundByUserTag] userTagId:{TagId}, soundKey:{SoundKey}", userTagId, soundKey);
        foreach (FieldPlayer player in Field.Players.Values) {
            if (player.TagId == userTagId) {
                player.Session.Send(SoundPacket.System(soundKey));
            }
        }
    }

    public void PlaySystemSoundInBox(string sound, params int[] boxIds) {
        DebugLog("[PlaySystemSoundInBox] sound:{Sound}, boxIds:{BoxIds}", sound, string.Join(", ", boxIds));
        if (boxIds.Length == 0) {
            Broadcast(SoundPacket.System(sound));
            return;
        }

        foreach (int boxId in boxIds) {
            foreach (FieldPlayer player in PlayersInBox(boxId)) {
                player.Session.Send(SoundPacket.System(sound));
            }
        }
    }

    public void ScoreBoardCreate(string type, string title, int maxScore) {
        ErrorLog("[ScoreBoardCreate] type:{Type}, maxScore:{MaxScore}", type, maxScore);
    }

    public void ScoreBoardRemove() {
        ErrorLog("[ScoreBoardRemove]");
    }

    public void ScoreBoardSetScore(int score) {
        ErrorLog("[ScoreBoardSetScore] score:{Score}", score);
    }

    public void SetEventUiRound(int[] rounds, int arg3, int vOffset) {
        DebugLog("[SetEventUIRound] rounds:{Rounds}, arg3:{Arg3}, vOffset:{VOffset}", string.Join(", ", rounds), arg3, vOffset);
        int round = rounds.ElementAtOrDefault(0, 1);
        int maxRound = rounds.ElementAtOrDefault(1, 1);
        int minRound = rounds.ElementAtOrDefault(2, 1);
        if (minRound == maxRound) {
            return;
        }
        Broadcast(MassiveEventPacket.Round(round, maxRound, minRound, vOffset));
    }

    public void SetEventUiScript(BannerType type, string script, int duration, string[] boxIds) {
        DebugLog("[SetEventUIScript] type:{Type}, script:{Script}, duration:{Duration}, boxIds:{BoxIds}", type, script, duration, string.Join(", ", boxIds));
        ByteWriter packet = MassiveEventPacket.Banner(type, script, duration);

        int[] notBoxIdInts = boxIds
            .Where(id => id.StartsWith("!")).
            Select(id => int.Parse(id.Substring(1)))
            .ToArray();
        int[] boxIdInts = boxIds
            .Where(id => !id.StartsWith("!"))
            .Select(int.Parse)
            .ToArray();

        foreach (int notBoxId in notBoxIdInts) {
            foreach (FieldPlayer player in PlayersNotInBox(notBoxId)) {
                player.Session.Send(packet);
            }
        }

        foreach (int boxId in boxIdInts) {
            foreach (FieldPlayer player in PlayersInBox(boxId)) {
                player.Session.Send(packet);
            }
        }

    }

    public void SetEventUiCountdown(string script, int[] roundCountdown, string[] boxIds) {
        DebugLog("[SetEventUICountdown] script:{Script}, roundCountdown:{Countdown}, boxIds:{BoxIds}", script, string.Join(", ", roundCountdown), string.Join(", ", boxIds));
        if (roundCountdown.Length != 2) {
            return;
        }

        ByteWriter packet = MassiveEventPacket.Countdown(script, roundCountdown[0], roundCountdown[1]);
        foreach (string boxIdString in boxIds) {
            if (boxIdString.Contains('!')) {
                if (int.TryParse(boxIdString.Substring(1), out int notBoxId)) {
                    foreach (FieldPlayer player in PlayersNotInBox(notBoxId)) {
                        player.Session.Send(packet);
                    }
                }
                continue;
            }

            if (int.TryParse(boxIdString, out int boxId)) {
                if (boxId == 0) {
                    Broadcast(packet);
                    continue;
                }
                foreach (FieldPlayer player in PlayersInBox(boxId)) {
                    player.Session.Send(packet);
                }
            }
        }
    }

    public void SetVisibleUi(string[] uiNames, bool visible) {
        DebugLog("[SetVisibleUI] uiNames:{UiNames}, visible:{Visible}", string.Join(", ", uiNames), visible);
        Broadcast(TriggerPacket.SetVisibleUi(visible, string.Join(",", uiNames)));
    }

    public void ShowCountUi(string text, int stage, int count, int soundType) {
        DebugLog("[ShowCountUI] text:{Text}, stage:{Stage}, count:{Count}, soundType:{SoundType}", text, stage, count, soundType);
        Broadcast(MassiveEventPacket.Countdown(text, stage, count, soundType));
    }

    public void ShowEventResult(string type, string text, int duration, int userTagId, int boxId, bool isOutSide) {
        ErrorLog("[ShowEventResult] type:{Type}, text:{Text}, duration:{Duration}, userTagId:{TagId}, boxId:{BoxId}, isOutSide:{IsOutside}",
            type, text, duration, userTagId, boxId, isOutSide);
    }

    public void ShowGuideSummary(int entityId, int textId, int duration) {
        DebugLog("[ShowGuideSummary] entityId:{EntityId}, textId:{TextId}, duration:{Duration}", entityId, textId, duration);
        Broadcast(TriggerPacket.UiShowSummary(entityId, textId, duration));
    }

    public void ShowRoundUi(int round, int duration, bool isFinalRound) {
        DebugLog("[ShowRoundUI] round:{Round}, duration:{Duration}", round, duration);
        Broadcast(MassiveEventPacket.StartRound(round, duration));
    }

    public void SideNpcTalk(int npcId, string illust, int duration, string script, string voice) {
        DebugLog("[SideNpcTalkBottom] npcId:{NpcId}, illust:{Illustration}, duration:{Duration}, script:{Script}, voice:{Voice}",
            npcId, illust, duration, script, voice);
        Broadcast(TriggerPacket.SidePopupTalk(SideNpcTalkType.Default, duration, illust, voice, script));
    }

    public void SideNpcTalkBottom(int npcId, string illust, int duration, string script) {
        DebugLog("[SideNpcTalkBottom] npcId:{NpcId}, illust:{Illustration}, duration:{Duration}, script:{Script}", npcId, illust, duration, script);
        Broadcast(TriggerPacket.SidePopupTalk(SideNpcTalkType.TalkBottom, duration, illust, string.Empty, script));
    }

    public void SideNpcMovie(string usm, int duration) {
        DebugLog("[SideNpcMovie] usm:{Usm}, duration:{Duration}", usm, duration);
        Broadcast(TriggerPacket.SidePopupTalk(SideNpcTalkType.Movie, duration, usm: usm));
    }

    public void SideNpcCutin(string illust, int duration) {
        DebugLog("[SideNpcMovie] illust:{Illustration}, duration:{Duration}", illust, duration);
        Broadcast(TriggerPacket.SidePopupTalk(SideNpcTalkType.CutIn, duration, illust));
    }

    public void WidgetAction(string type, string func, string widgetArg, string desc, int widgetArgNum) {
        DebugLog("[WidgetAction] type:{Type}, func:{Func}, widgetArg:{Args}, desc:{Desc}, widgetArgNum:{ArgNum}", type, func, widgetArg, desc, widgetArgNum);
        if (!Field.Widgets.TryGetValue(type, out Widget? widget)) {
            return;
        }

        DebugLog("Calling Action on widget of type {Type}", widget.GetType().Name);
        widget.Action(func, widgetArgNum, widgetArg);
    }

    #region Conditions
    public int WidgetValue(string type, string name, string desc) {
        DebugLog("[WidgetValue] type:{Type}, name:{Name}, desc:{Desc}", type, name, desc);
        if (!Field.Widgets.TryGetValue(type, out Widget? widget)) {
            return 0;
        }

        return widget.Conditions.GetValueOrDefault(name);
    }
    #endregion
}
