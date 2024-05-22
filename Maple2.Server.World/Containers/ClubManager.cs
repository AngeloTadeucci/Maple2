using System;
using Maple2.Database.Storage;
using Maple2.Model.Game;

namespace Maple2.Server.World.Containers;

public class ClubManager : IDisposable {
    public required GameStorage GameStorage { get; init; }
    public required ChannelClientLookup ChannelClients { get; init; }

    public readonly Club Club;

    public ClubManager(Club club) {
        Club = club;
    }

    public void Dispose() {
        using GameStorage.Request db = GameStorage.Context();
        db.SaveClub(Club);
    }
}
