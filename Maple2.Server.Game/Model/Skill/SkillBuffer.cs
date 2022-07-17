﻿namespace Maple2.Server.Game.Model.Skill;

// Used to keep track and lookup pending skills
// This is a circular array and will overwrite old pending skills
public class SkillBuffer {
    private const int MAX_PENDING = 3;

    private readonly SkillRecord?[] casts;
    private int index;

    public SkillBuffer() {
        casts = new SkillRecord[MAX_PENDING];
        index = 0;
    }

    public void Add(SkillRecord cast) {
        casts[index] = cast;

        index = (index + 1) % MAX_PENDING;
    }

    public SkillRecord? Get(long uid) {
        for (int i = 0; i < MAX_PENDING; i++) {
            if (casts[i]?.Uid == uid) {
                return casts[i];
            }
        }

        return null;
    }

    public void Remove(long uid) {
        for (int i = 0; i < MAX_PENDING; i++) {
            if (casts[i]?.Uid != uid) {
                continue;
            }

            casts[i] = null;
            return;
        }
    }
}
