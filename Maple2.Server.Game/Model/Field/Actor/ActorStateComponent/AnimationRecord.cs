﻿﻿using Maple2.Model.Metadata;
using Maple2.Server.Game.Model.Enum;

namespace Maple2.Server.Game.Model.ActorStateComponent;

public class AnimationRecord {
    public AnimationSequenceMetadata? Sequence { get; set; }
    public float Speed { get; set; }
    public AnimationType Type { get; set; }
    public float LastTime { get; set; }
    public float EndTime { get; set; }
    public LoopData Loop { get; set; }
    public long EndTick { get; set; }
    public long LoopEndTick { get; set; }
    public bool IsLooping { get; set; }
    public bool LoopOnlyOnce { get; set; }
    public SkillMetadata? Skill { get; set; }

    public AnimationRecord() {
        Speed = 1;
        LastTime = 0;
        Loop = new LoopData(0, 0);
        IsLooping = false;
        EndTime = 0;
        Type = AnimationType.Misc;
    }

    public AnimationRecord(AnimationSequenceMetadata sequence, float speed, AnimationType type, SkillMetadata? skill = null) : this() {
        Sequence = sequence;
        Speed = speed;
        Type = type;
        Skill = skill;
    }

    public struct LoopData {
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
}
