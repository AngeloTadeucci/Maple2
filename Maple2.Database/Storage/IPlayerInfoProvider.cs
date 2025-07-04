using Maple2.Model.Game;

namespace Maple2.Database.Storage;

public interface IPlayerInfoProvider {
    public PlayerInfo? GetPlayerInfo(long id);
}
