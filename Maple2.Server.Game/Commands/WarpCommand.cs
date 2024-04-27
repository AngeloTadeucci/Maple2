using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
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
