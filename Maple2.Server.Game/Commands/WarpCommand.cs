using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Text;
using Maple2.Database.Storage;
using Maple2.Model.Error;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

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
        var portalId = new Option<int>(new[] { "--portal", "-p" }, () => -1, "Id of portal to teleport to.");

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
    private const string DESCRIPTION = "Map warping by name.";

    private readonly GameSession session;
    private readonly MapMetadataStorage mapStorage;

    public GotoCommand(GameSession session, MapMetadataStorage mapStorage) : base(NAME, DESCRIPTION) {
        this.session = session;
        this.mapStorage = mapStorage;

        var args = new Argument<string[]>("name", Array.Empty<string>, "Name of the map.");
        var index = new Option<int>(new[] { "--index", "-i" }, () => -1, "Index of the map to warp to.");

        AddArgument(args);
        AddOption(index);
        this.SetHandler<InvocationContext, string[], int>(Handle, args, index);
    }

    private void Handle(InvocationContext ctx, string[] args, int mapIndex) {
        try {
            string query = string.Join(' ', args);
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
