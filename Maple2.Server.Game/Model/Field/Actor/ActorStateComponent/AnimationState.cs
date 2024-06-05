using Maple2.Model.Metadata;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model.Enum;

namespace Maple2.Server.Game.Model.Field.Actor.ActorStateComponent;

public class AnimationState {
    private readonly IActor actor;

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
    public short IdleSequenceId { get; init; }
    private AnimationSequence? queuedSequence;
    private float queuedSequenceSpeed;
    private AnimationType queuedSequenceType;
    private bool queuedResetSequence = false;
    private bool reportingKeyframeEvent;
    private bool IsPlayerAnimation { get; set; }
    public float MoveSpeed { get; set; }
    public float AttackSpeed { get; set; }
    public float SequenceSpeed { get; private set; }
    private float lastSequenceTime { get; set; }
    private float sequenceEnd { get; set; }
    private LoopData sequenceLoop { get; set; }
    private long sequenceEndTick { get; set; }
    private long sequenceLoopEndTick { get; set; }
    private long lastTick { get; set; }
    private bool isLooping { get; set; }
    private bool loopOnlyOnce { get; set; }
    private AnimationType sequenceType { get; set; }

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

        RigMetadata = actor.NpcMetadata?.GetAnimation(modelName);
        MoveSpeed = 1;
        AttackSpeed = 1;
        IsPlayerAnimation = actor is FieldPlayer;

        if (RigMetadata is null) {
            IdleSequenceId = 0;

            return;
        }

        if (actor is FieldNpc npc) {
            NpcAction? idleAction =  npc.Value.Metadata.Action.Actions.FirstOrDefault();
            IdleSequenceId = RigMetadata.Sequences.FirstOrDefault(sequence => sequence.Key == idleAction?.Name).Value.Id;
            return;
        }
        IdleSequenceId = RigMetadata.Sequences.FirstOrDefault(sequence => sequence.Key == "Idle_A").Value.Id;
    }

    private void ResetSequence() {
        PlayingSequence = null;
        SequenceSpeed = 1;
        lastSequenceTime = 0;
        sequenceLoop = new LoopData(0, 0);
        lastTick = 0;
        isLooping = false;
        sequenceEnd = 0;
        sequenceType = AnimationType.Misc;
    }

    public bool TryPlaySequence(string name, float speed, AnimationType type) {
        if (RigMetadata is null) {
            return false;
        }

        bool animationSequenceExists = RigMetadata.Sequences.TryGetValue(name, out AnimationSequence? sequence);

        if (reportingKeyframeEvent) {
            queuedSequence = sequence;
            queuedSequenceSpeed = speed;
            queuedSequenceType = type;

            return animationSequenceExists;
        }

        if (!animationSequenceExists) {
            DebugPrint($"Attempt to play nonexistent sequence '{name}' at x{speed} speed, previous: '{PlayingSequence?.Name ?? "none"}' x{SequenceSpeed}");

            if (reportingKeyframeEvent) {
                queuedResetSequence = true;
            } else {
                ResetSequence();
            }

            return false;
        }

        PlaySequence(sequence!, speed, type);

        return true;
    }

    public bool TryPlaySequence(string name, float speed, AnimationType type, out AnimationSequence? sequence) {
        if (!TryPlaySequence(name, speed, type)) {
            sequence = null;

            return false;
        }

        if (queuedSequence is not null) {
            sequence = queuedSequence;
        } else {
            sequence = PlayingSequence;
        }

        return true;
    }

    private void PlaySequence(AnimationSequence sequence, float speed, AnimationType type) {
        if (PlayingSequence != sequence && actor is FieldNpc npc) {
            npc.SendControl = true;
        }

        ResetSequence();

        DebugPrint($"Playing sequence '{sequence!.Name}' at x{speed} speed, previous: '{PlayingSequence?.Name ?? "none"}' x{SequenceSpeed}");

        PlayingSequence = sequence;
        SequenceSpeed = speed;
        sequenceType = type;

        lastTick = actor.Field.FieldTick;
    }

    public void CancelSequence() {
        if (PlayingSequence is not null) {
            DebugPrint($"Canceled playing sequence: '{PlayingSequence.Name}' x{SequenceSpeed}");
        }

        if (reportingKeyframeEvent) {
            queuedResetSequence = true;

            return;
        }

        if (PlayingSequence != null && actor is FieldNpc npc) {
            npc.SendControl = true;
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
            AnimationType.Move => MoveSpeed,
            AnimationType.Skill => AttackSpeed,
            _ => 1
        };

        long lastServerTick = lastTick == 0 ? tickCount : lastTick;
        float speed = SequenceSpeed * sequenceSpeedModifier / 1000;
        float delta = (float) (tickCount - lastServerTick) * speed;
        float sequenceTime = lastSequenceTime + delta;

        foreach (AnimationKey key in PlayingSequence.Keys) {
            if (HasHitKeyframe(sequenceTime, key)) {
                HitKeyframe(sequenceTime, key, speed);
            }
        }

        // TODO: maybe make client grace period ping based instead?
        if (isLooping && sequenceLoop.end != 0 && sequenceTime > sequenceLoop.end) {
            if (!IsPlayerAnimation || tickCount <= sequenceLoopEndTick + Constant.ClientGraceTimeTick) {
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
            if (!IsPlayerAnimation || tickCount <= sequenceEndTick + Constant.ClientGraceTimeTick) {
                ResetSequence();
            }
        }

        lastTick = tickCount;
        lastSequenceTime = sequenceTime;
        reportingKeyframeEvent = false;

        if (queuedResetSequence) {
            if (PlayingSequence != null && actor is FieldNpc npc) {
                npc.SendControl = true;
            }

            ResetSequence();
        } else if (queuedSequence is not null) {
            PlaySequence(queuedSequence, queuedSequenceSpeed, queuedSequenceType);
        }

        queuedResetSequence = false;
        queuedSequence = null;
    }

    public void SetLoopSequence(bool shouldLoop, bool loopOnlyOnce) {
        isLooping = shouldLoop;
        this.loopOnlyOnce = loopOnlyOnce;
    }

    private bool HasHitKeyframe(float sequenceTime, AnimationKey key) {
        bool keyBeforeLoop = !isLooping || sequenceLoop.end == 0 || key.Time <= sequenceLoop.end + 0.001f;
        bool hitKeySinceLastTick = key.Time > lastSequenceTime && key.Time <= sequenceTime;

        return keyBeforeLoop && hitKeySinceLastTick;
    }

    public float GetSequenceSegmentTime(string keyframe1, string keyframe2) {
        if (PlayingSequence is null) {
            return -1;
        }

        float keyframe1Time = -1;
        float keyframe2Time = -1;

        foreach (AnimationKey key in PlayingSequence.Keys) {
            if (key.Name == keyframe1) {
                keyframe1Time = key.Time;
            }

            if (key.Name == keyframe2) {
                keyframe2Time = key.Time;

                break;
            }
        }

        // Segment doesn't exist or is malformed
        if (keyframe1Time == -1 || keyframe2Time == -1 || keyframe1Time > keyframe2Time) {
            return -1;
        }

        // Current time out of segment
        if (lastSequenceTime < keyframe1Time || lastSequenceTime >= keyframe2Time) {
            return -1;
        }

        if (keyframe1Time == keyframe2Time) {
            // Can only be in the segment, and at the end, or not in the segment
            return lastSequenceTime == keyframe1Time ? 1 : -1;
        }

        return (lastSequenceTime - keyframe1Time) / (keyframe2Time - keyframe1Time);
    }

    private void HitKeyframe(float sequenceTime, AnimationKey key, float speed) {
        reportingKeyframeEvent = true;

        DebugPrint($"Sequence '{PlayingSequence!.Name}' keyframe event '{key.Name}'");

        actor.KeyframeEvent(key.Name);

        switch (key.Name) {
            case "loopstart":
                sequenceLoop = new LoopData(key.Time, 0);
                break;
            case "loopend":
                sequenceLoop = new LoopData(sequenceLoop.start, key.Time);
                sequenceLoopEndTick = (long) ((sequenceTime - key.Time) / speed);

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
