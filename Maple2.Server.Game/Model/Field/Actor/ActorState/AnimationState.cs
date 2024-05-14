using Maple2.Model.Metadata;
using Maple2.Server.Core.Packets;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IronPython.Modules._ast;

namespace Maple2.Server.Game.Model.Field.Actor.ActorState;

public class AnimationState {
    // TODO: maybe make this a ping based variable?
    private const long ClientGraceTimeTick = 500; // max time to allow client to go past loop & sequence end
    private readonly IActor actor;

    public enum Type {
        Misc,
        Move,
        Skill
    }

    private struct LoopData {
        public float start;
        public float end;

        public LoopData(float start, float end) {
            this.start = start;
            this.end = end;
        }
    }

    public struct TickPair {
        public long server;
        public long client;

        // mobs
        public TickPair(long server) {
            this.server = server;
            this.client = server;
        }

        // players
        public TickPair(long server, long client) {
            this.server = server;
            this.client = client;
        }
    }

    public AnimationMetadata? RigMetadata { get; init; }
    public AnimationSequence? PlayingSequence { get; private set; }
    public TickPair CastStartTick { get; private set; }
    public float MoveSpeed { get; set; }
    public float AttackSpeed { get; set; }
    private float sequenceSpeed { get; set; }
    private float lastSequenceTime { get; set; }
    private float sequenceEnd { get; set; }
    private LoopData sequenceLoop { get; set; }
    private long sequenceEndTick { get; set; }
    private long sequenceLoopEndTick { get; set; }
    private TickPair lastTick { get; set; }
    private bool isLooping { get; set; }
    private bool loopOnlyOnce { get; set; }
    private Type sequenceType { get; set; }

    private bool debugPrintAnimations;
    public bool DebugPrintAnimations {
        get { return debugPrintAnimations; }
        set {
            if (actor is FieldPlayer) {
                debugPrintAnimations = value;
            }
        }
    }

    public AnimationState(IActor actor, string modelName) {
        this.actor = actor;

        RigMetadata = actor.NpcMetadata.GetAnimation(modelName);
        MoveSpeed = 1;
        AttackSpeed = 1;
    }

    private void ResetSequence() {
        PlayingSequence = null;
        CastStartTick = new TickPair(0);
        sequenceSpeed = 1;
        lastSequenceTime = 0;
        sequenceLoop = new LoopData(0, 0);
        lastTick = new TickPair(0);
        isLooping = false;
        sequenceEnd = 0;
        sequenceType = Type.Misc;
    }

    public bool TryPlaySequence(string name, float speed, Type type, long clientTick = 0) {
        if (RigMetadata is null) {
            return false;
        }

        bool found = RigMetadata.Sequences.TryGetValue(name, out AnimationSequence? sequence);

        ResetSequence();

        if (!found) {
            DebugPrint($"Attempt to play nonexistent sequence '{name}' at x{speed} speed, previous: '{PlayingSequence?.Name ?? "none"}' x{sequenceSpeed}");

            return false;
        }

        DebugPrint($"Playing sequence '{sequence!.Name}' at x{speed} speed, previous: '{PlayingSequence?.Name ?? "none"}' x{sequenceSpeed}");

        PlayingSequence = sequence;
        sequenceSpeed = speed;
        sequenceType = type;

        long fieldTick = actor.Field.FieldTick;

        lastTick = new TickPair(fieldTick, clientTick == 0 ? fieldTick : clientTick);
        CastStartTick = lastTick;

        return true;
    }

    public void CancelSequence() {
        if (PlayingSequence is not null) {
            DebugPrint($"Canceled playing sequence: '{PlayingSequence.Name}' x{sequenceSpeed}");
        }

        ResetSequence();
    }

    public void Update(long tickCount) {
        if (RigMetadata is null) {
            return;
        }

        if (PlayingSequence?.Keys is null) {

            ResetSequence();

            return;
        }

        float sequenceSpeedModifier = sequenceType switch {
            Type.Move => MoveSpeed,
            Type.Skill => AttackSpeed,
            _ => 1
        };

        long lastServerTick = lastTick.server == 0 ? tickCount : lastTick.server;
        float speed = sequenceSpeed * sequenceSpeedModifier / 1000;
        float delta = (float) (tickCount - lastServerTick) * speed;
        float sequenceTime = lastSequenceTime + delta;

        foreach (AnimationKey key in PlayingSequence.Keys) {
            if (HasHitKeyframe(sequenceTime, key)) {
                HitKeyframe(sequenceTime, key, speed);
            }
        }

        // TODO: maybe make client grace period ping based instead?
        if (isLooping && sequenceLoop.end != 0 && sequenceTime > sequenceLoop.end) {
            if (lastTick.server == lastTick.client || tickCount <= sequenceLoopEndTick + ClientGraceTimeTick) {
                if (loopOnlyOnce) {
                    isLooping = false;
                    loopOnlyOnce = false;
                }

                sequenceTime -= sequenceLoop.end - sequenceLoop.start;
                lastSequenceTime = sequenceTime - Math.Max(delta, sequenceTime - sequenceLoop.end + 0.001f);

                // play all keyframe events from loopstart to current
                foreach (AnimationKey key in PlayingSequence.Keys) {
                    if (HasHitKeyframe(sequenceTime, key)) {
                        HitKeyframe(sequenceTime, key, speed);
                    }
                }
            }
        }

        if (sequenceEnd != 0 && sequenceTime > sequenceEnd) {
            if (lastTick.server == lastTick.client || tickCount <= sequenceEndTick + ClientGraceTimeTick) {
                ResetSequence();
            }
        }

        lastTick = new TickPair(tickCount, lastTick.client + tickCount - lastTick.server);
        lastSequenceTime = sequenceTime;
    }

    public void SetLoopSequence(bool shouldLoop, bool loopOnlyOnce) {
        isLooping = shouldLoop;
        this.loopOnlyOnce = loopOnlyOnce;
    }

    private bool HasHitKeyframe(float sequenceTime, AnimationKey key) {
        bool keyBeforeLoop = sequenceLoop.end == 0 || key.Time <= sequenceLoop.end + 0.001f;
        bool hitKeySinceLastTick = key.Time > lastSequenceTime && key.Time <= sequenceTime;

        return keyBeforeLoop && hitKeySinceLastTick;
    }

    private void HitKeyframe(float sequenceTime, AnimationKey key, float speed) {
        DebugPrint($"Sequence '{PlayingSequence!.Name}' keyframe event '{key.Name}'");

        actor.KeyframeEvent(key.Name);

        switch(key.Name) {
            case "loopstart":
                sequenceLoop = new LoopData(key.Time, 0);
                break;
            case "loopend":
                sequenceLoop = new LoopData(sequenceLoop.start, key.Time);
                sequenceLoopEndTick = (long)((sequenceTime - key.Time) / speed);

                break;
            case "end":
                sequenceEnd = key.Time;
                sequenceEndTick = (long) ((sequenceTime - key.Time) / speed);
                break;
            default:
                break;
        }
    }

    private void DebugPrint(string message) {
        if (debugPrintAnimations && actor is FieldPlayer player) {
            player.Session.Send(NoticePacket.Message(message));
        }
    }
}
