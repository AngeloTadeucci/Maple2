using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Scripting.Trigger;

namespace Maple2.Server.Game.Trigger;

public partial class TriggerContext {
    public void AddCinematicTalk(int npcId, string illustId, string msg, int duration, Align align, int delayTick) {
        DebugLog("[AddCinematicTalk] npcId:{NpcId}, illustId:{IllustrationId}, msg:{Message}, duration:{Duration}, align:{Align}, delayTick:{Delay}",
            npcId, illustId, msg, duration, align, delayTick);
        Broadcast(CinematicPacket.Talk(npcId, illustId, msg, duration, align));
    }

    public void RemoveCinematicTalk() {
        DebugLog("[RemoveCinematicTalk]");
        Broadcast(CinematicPacket.RemoveTalk());
    }

    public void CloseCinematic() {
        DebugLog("[CloseCinematic]");
        // guessing
        Broadcast(CinematicPacket.HideUi());
    }

    public void PlaySceneMovie(string fileName, int movieId, string skipType) {
        DebugLog("[PlaySceneMovie] fileName:{FileName}, movieId:{MovieId}, skipType:{SkipType}", fileName, movieId, skipType);
        Broadcast(TriggerPacket.UiStartMovie(fileName, movieId));
    }

    public void SetCinematicIntro(string text) {
        DebugLog("[SetCinematicIntro] text:{Text}", text);
        Broadcast(CinematicPacket.Intro(text));
    }

    public void SetOnetimeEffect(int id, bool enabled, string path) {
        DebugLog("[SetOnetimeEffect] id:{Id}, enabled:{Enabled}, path:{Path}", id, enabled, path);
        Broadcast(CinematicPacket.OneTimeEffect(id, enabled, path));
    }

    public void SetCinematicUi(int type, string script, bool arg3) {
        WarnLog("[SetCinematicUI] type:{Type}, script:{Script}, arg3:{Arg3}", type, script, arg3);
        switch (type) {
            case 0:
                Broadcast(CinematicPacket.ToggleUi(false));
                return;
            case 1:
                Broadcast(CinematicPacket.ToggleUi(true));
                return;
            case 2:
                Broadcast(CinematicPacket.HideUi());
                return;
            case 3:
                Broadcast(CinematicPacket.LetterboxTransition(script));
                return;
            case 4:
                Broadcast(CinematicPacket.FadeTransition(script));
                return;
            case 5:
                Broadcast(CinematicPacket.HorizontalTransition(script));
                return;
            case 6:
                Broadcast(CinematicPacket.VerticalTransition(script));
                return;
            case 7: // many
                return;
            case 9:
                Broadcast(CinematicPacket.Opening(script, arg3));
                return;
        }
    }

    public void SetSceneSkip(dynamic state, string nextState) {
        WarnLog("[SetSceneSkip] state:{State}, nextState:{NextState}", nameof(state), nextState);
        skipState = state;
        Broadcast(CinematicPacket.SetSkipScene(nextState));
    }

    public void SetSkip(dynamic state) {
        WarnLog("[SetSkip] state:{State}", nameof(state));
        skipState = state;
        Broadcast(CinematicPacket.SetSkipState(""));
    }

    public void AddBalloonTalk(int spawnId, string msg, int duration, int delayTick, int npcId) {
        DebugLog("[AddBalloonTalk] spawnId:{SpawnId}, msg:{Message}, duration:{Duration}, delayTick:{Delay}, npcId:{NpcId}", spawnId, msg, duration, delayTick, npcId);
        if (spawnId == 0) {
            FieldPlayer? fieldPlayer = Field.Players.Values.FirstOrDefault();
            if (fieldPlayer == null) {
                ErrorLog("[AddBalloonTalk] No players found in field");
                return;
            }

            Broadcast(CinematicPacket.BalloonTalk(false, fieldPlayer.ObjectId, msg, duration, delayTick));
            return;
        }

        FieldNpc? fieldNpc = Field.Npcs.Values.FirstOrDefault(npc => npc.SpawnPointId == spawnId);
        if (fieldNpc == null) {
            ErrorLog("[AddBalloonTalk] No NPC with spawnId:{SpawnId} found in field", spawnId);
            return;
        }

        Broadcast(CinematicPacket.BalloonTalk(true, fieldNpc.ObjectId, msg, duration, delayTick));
    }

    public void RemoveBalloonTalk(int spawnId) {
        DebugLog("[RemoveBalloonTalk] spawnId:{SpawnId}", spawnId);
        foreach (FieldNpc npc in Field.Npcs.Values) {
            if (spawnId == 0 || npc.SpawnPointId == spawnId) {
                Broadcast(CinematicPacket.RemoveBalloonTalk(npc.ObjectId));
            }
        }
    }

    public void ShowCaption(
        string type,
        string title,
        string script,
        Align align,
        float offsetRateX,
        float offsetRateY,
        int duration,
        float scale
    ) {
        DebugLog("[ShowCaption] type:{Type}, title:{Title}, script:{Script}", type, title, script);
        Broadcast(CinematicPacket.Caption(type, title, script, align, offsetRateX, offsetRateY, duration, scale));
    }
}
