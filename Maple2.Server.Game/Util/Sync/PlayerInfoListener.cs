using Maple2.Model.Game;
using Maple2.Server.Core.Sync;

namespace Maple2.Server.Game.Util.Sync;

public class PlayerInfoListener {
    public readonly UpdateField Type;
    // When callback returns true, it means the listener is completed.
    public readonly Func<UpdateField, IPlayerInfo, bool> Callback;

    public PlayerInfoListener(UpdateField type, Func<UpdateField, IPlayerInfo, bool> callback) {
        Type = type;
        Callback = callback;
    }
}
