using System.Runtime.CompilerServices;

namespace Maple2.Server.Game.Scripting.Trigger;

public class TriggerState {
    private readonly dynamic state;

    public string Name => state.ToString();

    public TriggerState(dynamic state) {
        this.state = state;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TriggerState? OnEnter() {
        dynamic? result = state.on_enter();
        return result != null ? new TriggerState(result) : null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TriggerState? OnTick() {
        dynamic? result = state.on_tick();
        return result != null ? new TriggerState(result) : null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnExit() {
        state.on_exit();
    }
}
