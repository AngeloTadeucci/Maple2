﻿using Maple2.Model.Metadata;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model.Enum;

namespace Maple2.Server.Game.Model.ActorStateComponent;

/// <summary>
/// Manages animation sequences for actors in the game.
/// </summary>
public class AnimationState {
    private readonly IActor actor;
    private AnimationRecord? current;
    private AnimationRecord? queued;

    public readonly AnimationMetadata? RigMetadata;
    public AnimationSequenceMetadata? PlayingSequence => current?.Sequence;
    public short IdleSequenceId { get; init; }
    public float SequenceSpeed => current?.Speed ?? 1.0f;

    private bool isHandlingKeyframe;
    private bool IsPlayerAnimation => actor is FieldPlayer;
    public float MoveSpeed { get; set; } = 1;
    public float AttackSpeed { get; set; } = 1;
    private float lastSequenceTime;
    private float sequenceEnd;
    private LoopData sequenceLoop;
    private long lastTick;
    private long sequenceEndTick;
    private long sequenceLoopEndTick;

    private bool debugPrintAnimations;
    public bool DebugPrintAnimations {
        get { return debugPrintAnimations; }
        set {
            if (actor is FieldPlayer) {
                debugPrintAnimations = value;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the AnimationState class.
    /// </summary>
    /// <param name="actor">The actor this animation state belongs to</param>
    /// <param name="modelName">The model name to load animations for</param>
    public AnimationState(IActor actor, string modelName) {
        this.actor = actor;

        RigMetadata = actor.NpcMetadata?.GetAnimation(modelName);
        MoveSpeed = 1;
        AttackSpeed = 1;
        sequenceLoop = new LoopData(0, 0);

        if (RigMetadata is null) {
            IdleSequenceId = 0;
            return;
        }

        string idleName = "Idle_A";
        if (actor is FieldNpc npc) {
            idleName = npc.Value.Metadata.Action.Actions.FirstOrDefault()?.Name ?? idleName;
            IdleSequenceId = RigMetadata.Sequences.FirstOrDefault(sequence => sequence.Key.Contains(idleName)).Value.Id;
            return;
        }
        IdleSequenceId = RigMetadata.Sequences.FirstOrDefault(sequence => sequence.Key == idleName).Value.Id;
    }

    /// <summary>
    /// Resets the current animation sequence.
    /// </summary>
    private void ResetSequence() {
        if (current?.Sequence != null && actor is FieldNpc npc) {
            npc.SendControl = true;
        }
        current = null;
        lastSequenceTime = 0;
        sequenceLoop = new LoopData(0, 0);
        lastTick = 0;
        sequenceEnd = 0;
    }

    /// <summary>
    /// Attempts to play an animation sequence with the specified name, speed, and type.
    /// </summary>
    /// <param name="name">The name of the animation sequence to play</param>
    /// <param name="speed">The speed at which to play the animation</param>
    /// <param name="type">The type of animation (Move, Skill, or Misc)</param>
    /// <returns>True if the sequence was found and started (or queued), false otherwise</returns>
    public bool TryPlaySequence(string name, float speed, AnimationType type) {
        // Can't play animations without metadata
        if (RigMetadata is null || !RigMetadata.Sequences.TryGetValue(name, out AnimationSequenceMetadata? sequence)) {
            DebugPrint($"Attempt to play nonexistent sequence '{name}' at x{speed} speed, previous: '{PlayingSequence?.Name ?? "none"}' x{SequenceSpeed}");
            ResetSequence();
            return false;
        }

        // If we're currently processing a keyframe event, queue this sequence for later
        if (isHandlingKeyframe) {
            queued = new AnimationRecord(sequence, speed, type);
            return true;
        }

        // Play the sequence immediately
        PlaySequence(sequence, speed, type);
        return true;
    }

    /// <summary>
    /// Plays the specified animation sequence.
    /// </summary>
    /// <param name="sequenceMetadata">The animation sequence to play</param>
    /// <param name="speed">The speed at which to play the animation</param>
    /// <param name="type">The type of animation (Move, Skill, or Misc)</param>
    private void PlaySequence(AnimationSequenceMetadata sequenceMetadata, float speed, AnimationType type) {
        // For NPCs, set SendControl flag when changing sequences
        if (current?.Sequence != sequenceMetadata && actor is FieldNpc npc) {
            npc.SendControl = true;
        }

        // Log the sequence change
        DebugPrint($"Playing sequence '{sequenceMetadata.Name}' at x{speed} speed, previous: '{PlayingSequence?.Name ?? "none"}' x{SequenceSpeed}");

        // Reset current sequence state
        ResetSequence();

        // Set the new sequence properties
        current = new AnimationRecord(sequenceMetadata, speed, type);
        
        // Start tracking from current tick
        lastTick = actor.Field.FieldTick;
    }

    /// <summary>
    /// Cancels the currently playing animation sequence.
    /// </summary>
    public void CancelSequence() {
        // Log the cancellation if a sequence is playing
        if (PlayingSequence is not null) {
            DebugPrint($"Canceled playing sequence: '{PlayingSequence.Name}' x{SequenceSpeed}");
        }

        // If we're processing a keyframe event, queue the reset for later
        if (isHandlingKeyframe) {
            queued = null;
            return;
        }

        // Reset the sequence state
        ResetSequence();
    }

    /// <summary>
    /// Updates the animation state based on the current tick count.
    /// </summary>
    /// <param name="tickCount">The current server tick count</param>
    public void Update(long tickCount) {
        // Skip update if no animation metadata is available
        if (RigMetadata is null) {
            return;
        }

        // Reset if no valid sequence is playing
        if (PlayingSequence?.Keys.Count == 0) {
            ResetSequence();
            return;
        }

        // Calculate the current sequence time
        float sequenceSpeedModifier = current?.Type switch {
            AnimationType.Move => MoveSpeed,
            AnimationType.Skill => AttackSpeed,
            _ => 1,
        };

        long lastServerTick = lastTick == 0 ? tickCount : lastTick;
        float speed = SequenceSpeed * sequenceSpeedModifier / 1000;
        float delta = (float)(tickCount - lastServerTick) * speed;
        float sequenceTime = lastSequenceTime + delta;

        // Process keyframe events
        if (PlayingSequence?.Keys != null) {
            foreach (AnimationKey key in PlayingSequence.Keys) {
                if (HasHitKeyframe(sequenceTime, key)) {
                    HitKeyframe(sequenceTime, key, speed);
                }
            }
        }

        // Handle sequence looping
        if (current?.IsLooping == true && sequenceLoop.end != 0 && sequenceTime > sequenceLoop.end) {
            if (!IsPlayerAnimation || tickCount <= sequenceLoopEndTick + Constant.ClientGraceTimeTick) {
                if (current.LoopOnlyOnce) {
                    current.IsLooping = false;
                    current.LoopOnlyOnce = false;
                }

                sequenceTime -= sequenceLoop.end - sequenceLoop.start;
                lastSequenceTime = sequenceTime - Math.Max(delta, sequenceTime - sequenceLoop.end + 0.001f);

                // Play all keyframe events from loopstart to current
                if (PlayingSequence?.Keys != null) {
                    foreach (AnimationKey key in PlayingSequence.Keys) {
                        if (HasHitKeyframe(sequenceTime, key)) {
                            HitKeyframe(sequenceTime, key, speed);
                        }
                    }
                }
            }
        }

        // Check for sequence end
        if (sequenceEnd != 0 && sequenceTime > sequenceEnd) {
            if (!IsPlayerAnimation || tickCount <= sequenceEndTick + Constant.ClientGraceTimeTick) {
                ResetSequence();
            }
        }

        // Update timing state
        lastTick = tickCount;
        lastSequenceTime = sequenceTime;
        isHandlingKeyframe = false;

        // Process queued actions
        if (queued != null) {
            PlaySequence(queued.Sequence!, queued.Speed, queued.Type);
            queued = null;
        }
    }

    /// <summary>
    /// Sets whether the current sequence should loop.
    /// </summary>
    /// <param name="shouldLoop">Whether the sequence should loop</param>
    /// <param name="loopOnlyOnce">Whether the sequence should only loop once</param>
    public void SetLoopSequence(bool shouldLoop, bool loopOnlyOnce) {
        if (current is null) {
            return;
        }

        current.IsLooping = shouldLoop;
        current.LoopOnlyOnce = loopOnlyOnce;
    }

    /// <summary>
    /// Determines if a keyframe has been hit in the current update.
    /// </summary>
    /// <param name="sequenceTime">The current sequence time</param>
    /// <param name="key">The keyframe to check</param>
    /// <returns>True if the keyframe has been hit, false otherwise</returns>
    private bool HasHitKeyframe(float sequenceTime, AnimationKey key) {
        bool keyBeforeLoop = !current?.IsLooping ?? true || sequenceLoop.end == 0 || key.Time <= sequenceLoop.end + 0.001f;
        bool hitKeySinceLastTick = key.Time > lastSequenceTime && key.Time <= sequenceTime;

        return keyBeforeLoop && hitKeySinceLastTick;
    }

    /// <summary>
    /// Gets the normalized time within a segment defined by two keyframes.
    /// </summary>
    /// <param name="keyframe1">The name of the first keyframe</param>
    /// <param name="keyframe2">The name of the second keyframe</param>
    /// <returns>A value between 0 and 1 representing the position within the segment, or -1 if not in the segment</returns>
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

    /// <summary>
    /// Processes a keyframe event.
    /// </summary>
    /// <param name="sequenceTime">The current sequence time</param>
    /// <param name="key">The keyframe that was hit</param>
    /// <param name="speed">The current animation speed</param>
    private void HitKeyframe(float sequenceTime, AnimationKey key, float speed) {
        isHandlingKeyframe = true;

        DebugPrint($"Sequence '{PlayingSequence!.Name}' keyframe event '{key.Name}'");

        actor.KeyframeEvent(key.Name);

        switch (key.Name) {
            case "loopstart":
                sequenceLoop = new LoopData(key.Time, 0);
                break;
            case "loopend":
                sequenceLoop = new LoopData(sequenceLoop.start, key.Time);
                sequenceLoopEndTick = actor.Field.FieldTick + (long)((key.Time - sequenceTime) / speed);
                break;
            case "end":
                sequenceEnd = key.Time;
                sequenceEndTick = actor.Field.FieldTick + (long)((key.Time - sequenceTime) / speed);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Prints a debug message if debug printing is enabled.
    /// </summary>
    /// <param name="message">The message to print</param>
    private void DebugPrint(string message) {
        if (debugPrintAnimations && actor is FieldPlayer player) {
            player.Session.Send(NoticePacket.Message(message));
        }
    }

    /// <summary>
    /// Represents a loop section in an animation sequence
    /// </summary>
    private struct LoopData {
        public float start;
        public float end;

        public LoopData(float start, float end) {
            this.start = start;
            this.end = end;
        }
    }
}
