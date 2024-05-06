﻿using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Packets;

namespace Maple2.Server.Game.Model;

public class FieldBreakable(FieldManager field, int objectId, string entityId, BreakableActor breakable)
    : FieldEntity<BreakableActor>(field, objectId, breakable) {
    public readonly string EntityId = entityId;

    private long nextTick;

    public BreakableState State { get; private set; } = breakable.Visible ? BreakableState.Show : BreakableState.Hide;
    public long BaseTick { get; private set; }

    private bool visible = breakable.Visible;
    public bool Visible {
        get => visible;
        set {
            if (!visible && value) {
                BaseTick = Environment.TickCount64;
            }
            visible = value;
        }
    }

    public bool UpdateState(BreakableState state) {
        if (State == state) {
            return false;
        }

        State = state;
        Field.Broadcast(BreakablePacket.Update(this));

        nextTick = State switch {
            BreakableState.Show => 0,
            BreakableState.Break => Environment.TickCount64 + Value.HideTime,
            BreakableState.Hide => Environment.TickCount64 + Value.ResetTime,
            BreakableState.Unknown5 => 0,
            BreakableState.Unknown6 => 0,
            _ => 0,
        };

        return true;
    }

    public override void Update(long tickCount) {
        if (nextTick == 0 || tickCount < nextTick) {
            return;
        }

        switch (State) {
            case BreakableState.Show:
                break;
            case BreakableState.Break:
                UpdateState(BreakableState.Hide);
                break;
            case BreakableState.Hide:
                UpdateState(BreakableState.Show);
                break;
            case BreakableState.Unknown5:
                break;
            case BreakableState.Unknown6:
                break;
        }
    }
}
