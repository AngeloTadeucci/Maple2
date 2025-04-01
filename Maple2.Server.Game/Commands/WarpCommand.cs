using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Net;
using System.Numerics;
using System.Text;
using Grpc.Core;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.World.Service;
using MigrationType = Maple2.Server.World.Service.MigrationType;

namespace Maple2.Server.Game.Commands;

public class WarpCommand : Command {
    private const string NAME = "warp";
    private const string DESCRIPTION = "Map warping.";

    private readonly GameSession session;
    private readonly MapMetadataStorage mapStorage;

    public WarpCommand(GameSession session, MapMetadataStorage mapStorage) : base(NAME, DESCRIPTION) {
        this.session = session;
        this.mapStorage = mapStorage;

        var mapId = new Argument<int>("id", "Id of map to warp to.");
        var portalId = new Option<int>(["--portal", "-p"], () => -1, "Id of portal to teleport to.");

        AddArgument(mapId);
        AddOption(portalId);
        this.SetHandler<InvocationContext, int, int>(Handle, mapId, portalId);
    }

    private void Handle(InvocationContext ctx, int mapId, int portalId) {
        try {
            if (!mapStorage.TryGet(mapId, out MapMetadata? map)) {
                ctx.Console.Error.WriteLine($"Invalid Map: {mapId}");
                return;
            }

            ctx.Console.Out.WriteLine($"Warping to '{map.Name}' ({map.Id})");
            session.Send(session.PrepareField(map.Id, portalId)
                ? FieldEnterPacket.Request(session.Player)
                : FieldEnterPacket.Error(MigrationError.s_move_err_default));

            ctx.ExitCode = 0;
        } catch (SystemException ex) {
            ctx.Console.Error.WriteLine(ex.Message);
            ctx.ExitCode = 1;
        }
    }
}

public class GotoCommand : Command {
    private const string NAME = "goto";
    private const string DESCRIPTION = "Map warping by name or player.";

    public GotoCommand(GameSession session, MapMetadataStorage mapStorage) : base(NAME, DESCRIPTION) {
        AddCommand(new MapCommand(session, mapStorage));
        AddCommand(new PlayerCommand(session));
    }

    private class MapCommand : Command {
        private readonly GameSession session;
        private readonly MapMetadataStorage mapStorage;

        public MapCommand(GameSession session, MapMetadataStorage mapStorage) : base("map", "Go to map by name.") {
            this.session = session;
            this.mapStorage = mapStorage;

            var mapName = new Argument<string[]>("name", Array.Empty<string>, "Name of the map.");
            var mapIndex = new Option<int>(["--index", "-i"], () => -1, "Index of the map to warp to.");

            AddArgument(mapName);
            AddOption(mapIndex);
            this.SetHandler<InvocationContext, string[], int>(Handle, mapName);
        }

        private void Handle(InvocationContext ctx, string[] mapName, int mapIndex) {
            try {
                string query = string.Join(' ', mapName);
                List<MapMetadata> results = mapStorage.Search(query);
                if (results.Count == 0) {
                    ctx.Console.Out.WriteLine("No results found.");
                    return;
                }

                if (mapIndex >= 0 && mapIndex < results.Count && mapIndex != -1) {
                    MapMetadata choosenMap = results[mapIndex];
                    ctx.Console.Out.WriteLine($"Warping to '{choosenMap.Name}' ({choosenMap.Id})");
                    session.Send(session.PrepareField(choosenMap.Id, -1)
                        ? FieldEnterPacket.Request(session.Player)
                        : FieldEnterPacket.Error(MigrationError.s_move_err_default));

                    ctx.ExitCode = 0;
                    return;
                }

                if (results.Count > 1) {
                    var builder = new StringBuilder($"<b>{results.Count} results for '{query}':</b>");
                    int index = 0;
                    foreach (MapMetadata result in results) {
                        builder.Append($"\n• {index++} - {result.Id}: {result.Name}");
                    }

                    ctx.Console.Out.WriteLine(builder.ToString());
                    ctx.ExitCode = 0;
                    return;
                }

                MapMetadata map = results.First();
                ctx.Console.Out.WriteLine($"Warping to '{map.Name}' ({map.Id})");
                session.Send(session.PrepareField(map.Id, -1)
                    ? FieldEnterPacket.Request(session.Player)
                    : FieldEnterPacket.Error(MigrationError.s_move_err_default));

                ctx.ExitCode = 0;
            } catch (SystemException ex) {
                ctx.Console.Error.WriteLine(ex.Message);
                ctx.ExitCode = 1;
            }
        }
    }
    private class PlayerCommand : Command {
        private readonly GameSession session;

        public PlayerCommand(GameSession session) : base("player", "Go to player.") {
            this.session = session;

            var playerName = new Argument<string>("name", "Name of the player.");
            AddArgument(playerName);
            this.SetHandler<InvocationContext, string>(Handle, playerName);
        }

        private void Handle(InvocationContext ctx, string playerName) {
            using GameStorage.Request db = session.GameStorage.Context();
            long characterId = db.GetCharacterId(playerName);
            if (characterId == 0) {
                ctx.Console.Out.WriteLine($"Player '{playerName}' not found.");
                return;
            }

            if (!session.PlayerInfo.GetOrFetch(characterId, out PlayerInfo? playerInfo)) {
                ctx.Console.Out.WriteLine($"Player '{playerName}' not found.");
                return;
            }

            PlayerWarpResponse? warpResponse = session.World.PlayerWarp(new PlayerWarpRequest {
                RequesterId = session.CharacterId,
                GoToPlayer = new PlayerWarpRequest.Types.GoToPlayer {
                    Channel = playerInfo.Channel,
                    CharacterId = playerInfo.CharacterId,
                },
            });

            if (warpResponse is not { Error: 0 }) {
                ctx.Console.Out.WriteLine($"Failed to warp to '{playerName}'.");
                return;
            }

            if (playerInfo.Channel == session.Player.Value.Character.Channel) {
                session.Send(session.PrepareField(playerInfo.MapId, roomId: warpResponse.RoomId, position: new Vector3(warpResponse.X, warpResponse.Y, warpResponse.Z))
                    ? FieldEnterPacket.Request(session.Player)
                    : FieldEnterPacket.Error(MigrationError.s_move_err_default));

                ctx.ExitCode = 0;
                return;
            }

            try {
                var request = new MigrateOutRequest {
                    AccountId = session.AccountId,
                    CharacterId = session.CharacterId,
                    MachineId = session.MachineId.ToString(),
                    Server = Server.World.Service.Server.Game,
                    Channel = playerInfo.Channel,
                    MapId = playerInfo.MapId,
                    RoomId = warpResponse.RoomId,
                    PositionX = warpResponse.X,
                    PositionY = warpResponse.Y,
                    PositionZ = warpResponse.Z,
                    Type = MigrationType.Normal,
                };

                MigrateOutResponse response = session.World.MigrateOut(request);
                var endpoint = new IPEndPoint(IPAddress.Parse(response.IpAddress), response.Port);
                session.Send(MigrationPacket.GameToGame(endpoint, response.Token, playerInfo.MapId));
                session.State = SessionState.ChangeMap;
            } catch (RpcException ex) {
                session.Send(MigrationPacket.GameToGameError(MigrationError.s_move_err_default));
                session.Send(NoticePacket.Disconnect(new InterfaceText(ex.Message)));
            } finally {
                session.Disconnect();
            }
        }
    }

}
