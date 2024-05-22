using System.Runtime.CompilerServices;

namespace Maple2.Server.Game.Scripting.Trigger;

public class TriggerState {
    private readonly dynamic state;

    public Lazy<string> Name => new(() => {
        // "<foo object at 0x000000000000002B>"
        string str = IronPython.Runtime.Operations.UserTypeOps.ToStringHelper(state);
        return str.Substring(str.IndexOf('<') + 1, str.IndexOf(' ') - str.IndexOf('<') - 1);
    });

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
