using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Serilog;

namespace Maple2.Server.Game.Manager;

public class StatsManager {
    private readonly GameSession session;
    private readonly IReadOnlyDictionary<short, IReadOnlyDictionary<BasicAttribute, long>> levelStats;

    public readonly Stats Values;

    public StatsManager(GameSession session) {
        this.session = session;

        Player player = session.Player;
        session.ServerTableMetadata.UserStatTable.JobStats.TryGetValue(player.Character.Job.Code(), out IReadOnlyDictionary<short, IReadOnlyDictionary<BasicAttribute, long>>? stats);
        if (stats is not null) {
            levelStats = stats;
            stats.TryGetValue(player.Character.Level, out IReadOnlyDictionary<BasicAttribute, long>? metadata);
            if (metadata is not null) {
                Values = new Stats(metadata, player.Character.Job.Code());
                return;
            }

            throw new Exception("Failed to initialize StatsManager, could not find metadata for level " + player.Character.Level + ".");
        }

        throw new Exception("Failed to initialize StatsManager, could not find metadata for job " + player.Character.Job.Code() + ".");

    }

    public void Refresh() {
        Character character = session.Player.Value.Character;
        if (levelStats.TryGetValue(character.Level, out IReadOnlyDictionary<BasicAttribute, long>? metadata)) {
            Values.Reset(metadata, character.Job.Code());
        } else {
            Log.Logger.Error("Failed to refresh stats for {Job} level {Level}.", character.Job.Code(), character.Level);
            Values.Reset(character.Job.Code(), character.Level);
        }

        // TODO: after resetting, we also need to re-add stats from buffs/equips.

        session.Send(StatsPacket.Init(session.Player));
        session.Field?.Broadcast(StatsPacket.Update(session.Player), session);
    }
}
