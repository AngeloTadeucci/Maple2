using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Tools.Extensions;
using Maple2.Trigger.Enum;

namespace Maple2.Server.Game.Trigger;

public partial class TriggerContext {
    public void CreateWidget(WidgetType type) {
        ErrorLog("[CreateWidget] type:{Type}", type);
        Field.Widgets[type] = new Widget(type);
    }

    public void GuideEvent(int eventId) {
        DebugLog("[GuideEvent] eventId:{Id}", eventId);
        Broadcast(TriggerPacket.UiGuide(eventId));
    }

    public void HideGuideSummary(int entityId, int textId) {
        DebugLog("[HideGuideSummary] entityId:{EntityId}, textId:{TextId}", entityId, textId);
        Broadcast(TriggerPacket.UiHideSummary(entityId));
    }

    public void Notice(bool arg1, string script, bool arg3) {
        DebugLog("[Notice] arg1:{Arg1}, script:{Script}, arg3:{Arg3}", arg1, script, arg3);
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

    public void ScoreBoardCreate(string type, int maxScore) {
        ErrorLog("[ScoreBoardCreate] type:{Type}, maxScore:{MaxScore}", type, maxScore);
    }

    public void ScoreBoardRemove() {
        ErrorLog("[ScoreBoardRemove]");
    }

    public void ScoreBoardSetScore(bool score) {
        ErrorLog("[ScoreBoardSetScore] score:{Score}", score);
    }

    public void SetEventUI(byte type, string script, int duration, int boxId, int notBoxId) {
        DebugLog("[SetEventUI] type:{Type}, script:{Script}, duration:{Duration}, boxId:{BoxId}, notBoxID:{NotBoxId}", type, script, duration, boxId, notBoxId);
        Func<ByteWriter> getPacket;
        switch (type) {
            case 0:
                IList<int> list = script.Split(",")
                    .Select(str => int.TryParse(str, out int result) ? result : 0)
                    .ToList();
                int round = list.ElementAtOrDefault(0, 1);
                int maxRound = list.ElementAtOrDefault(1, 1);
                int minRound = list.ElementAtOrDefault(2, 1);

                getPacket = () => MassiveEventPacket.Round(round, maxRound, minRound);
                break;
            case 1:
                getPacket = () => MassiveEventPacket.Banner(BannerType.Text, script, duration);
                break;
            case 2: // arg3=0,3
                getPacket = () => MassiveEventPacket.Countdown(script, 0, 3);
                break;
            case 3:
                getPacket = () => MassiveEventPacket.Banner(BannerType.Winner, script, duration);
                break;
            case 4:
                getPacket = () => MassiveEventPacket.Banner(BannerType.Lose, script, duration);
                break;
            case 5:
                getPacket = () => MassiveEventPacket.Banner(BannerType.GameOver, script, duration);
                break;
            case 6:
                getPacket = () => MassiveEventPacket.Banner(BannerType.Bonus, script, duration);
                break;
            case 7:
                getPacket = () => MassiveEventPacket.Banner(BannerType.Success, script, duration);
                break;
            default:
                return;
        }

        if (notBoxId > 0) {
            foreach (FieldPlayer player in PlayersNotInBox(notBoxId)) {
                player.Session.Send(getPacket());
            }
        } else {
            foreach (FieldPlayer player in PlayersInBox(boxId)) {
                player.Session.Send(getPacket());
            }
        }
    }

    public void SetVisibleUI(string[] uiNames, bool visible) {
        ErrorLog("[SetVisibleUI] uiNames:{UiNames}, visible:{Visible}", string.Join(", ", uiNames), visible);
    }

    public void ShowCountUI(string text, byte stage, byte count, byte soundType) {
        DebugLog("[ShowCountUI] text:{Text}, stage:{Stage}, count:{Count}, soundType:{SoundType}", text, stage, count, soundType);
        Broadcast(MassiveEventPacket.Countdown(text, stage, count, soundType));
    }

    public void ShowEventResult(EventResultType type, string text, int duration, int userTagId, int boxId, bool isOutSide) {
        ErrorLog("[ShowEventResult] type:{Type}, text:{Text}, duration:{Duration}, userTagId:{TagId}, boxId:{BoxId}, isOutSide:{IsOutside}",
            type, text, duration, userTagId, boxId, isOutSide);
    }

    public void ShowGuideSummary(int entityId, int textId, int duration) {
        DebugLog("[ShowGuideSummary] entityId:{EntityId}, textId:{TextId}, duration:{Duration}", entityId, textId, duration);
        Broadcast(TriggerPacket.UiShowSummary(entityId, textId, duration));
    }

    public void ShowRoundUI(int round, int duration) {
        DebugLog("[ShowRoundUI] round:{Round}, duration:{Duration}", round, duration);
        Broadcast(MassiveEventPacket.StartRound(round, duration));
    }

    public void SideNpcTalk(int npcId, string illust, int duration, string script, string voice, SideNpcTalkType type, string usm) {
        WarnLog("[SideNpcTalk] npcId:{NpcId}, illust:{Illustration}, duration:{Duration}, script:{Script}, voice:{Voice}, type:{Type}, usm:{Usm}",
            npcId, illust, duration, script, voice, type, usm);
        switch (type) {
            case SideNpcTalkType.Default:
                return;
            case SideNpcTalkType.Talk:
                Broadcast(TriggerPacket.SidePopupTalk(duration, illust, voice, script));
                return;
            case SideNpcTalkType.TalkBottom:
                return;
            case SideNpcTalkType.CutIn:
                Broadcast(TriggerPacket.SidePopupCutIn(duration, illust, voice, script));
                return;
            case SideNpcTalkType.Movie:
                return;
        }
    }

    public void WidgetAction(WidgetType type, string name, int widgetArgType, string args) {
        ErrorLog("[WidgetAction] type:{Type}, name:{Name}, widgetArgType:{ArgType}, args:{Args}", type, name, widgetArgType, args);
        if (!Field.Widgets.TryGetValue(type, out Widget? widget)) {
            return;
        }
    }

    #region Conditions
    public bool WidgetCondition(WidgetType type, string condition, string value) {
        DebugLog("[WidgetCondition] type:{Type}, condition:{Condition}, value:{Value}", type, condition, value);
        if (!Field.Widgets.TryGetValue(type, out Widget? widget)) {
            return false;
        }
        if (!widget.Conditions.TryGetValue(condition, out string? widgetValue)) {
            return false;
        }

        value = value.Trim();
        return string.IsNullOrWhiteSpace(value) || widgetValue == value;
    }
    #endregion
}
