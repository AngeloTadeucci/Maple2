using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using Maple2.Database.Storage;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class FieldCommand : Command {
    private const string NAME = "field";
    private const string DESCRIPTION = "Field information.";

    private readonly GameSession session;
    private readonly MapMetadataStorage mapStorage;

    public FieldCommand(GameSession session, MapMetadataStorage mapStorage) : base(NAME, DESCRIPTION) {
        this.session = session;
        this.mapStorage = mapStorage;

        AddCommand(new EntityInfoCommand(session));
        this.SetHandler<InvocationContext>(Handle);
    }

    private void Handle(InvocationContext ctx) {
        if (session.Field == null) {
            ctx.Console.Error.WriteLine("No active field");
            ctx.ExitCode = 1;
            return;
        }

        ctx.Console.Out.WriteLine($"Map: {session.Field.MapId}, OwnerId: {session.Field.OwnerId}, InstanceId: {session.Field.InstanceId}, ({session.Field.Metadata.XBlock})");
    }

    private class EntityInfoCommand : Command {
        private readonly GameSession session;

        public EntityInfoCommand(GameSession session) : base("entity", "Prints entity info.") {
            this.session = session;

            var objectId = new Argument<int>("id", "ObjectId of the entity.");

            AddArgument(objectId);
            this.SetHandler<InvocationContext, int>(Handle, objectId);
        }

        private void Handle(InvocationContext ctx, int objectId) {
            if (session.Field == null) {
                ctx.Console.Error.WriteLine("No active field");
                ctx.ExitCode = 1;
                return;
            }

            if (session.Field.Npcs.TryGetValue(objectId, out FieldNpc? npc)) {
                ctx.Console.Out.WriteLine($"Npc: {npc.Value.Metadata.Id} ({npc.Value.Metadata.Name})");
                ctx.Console.Out.WriteLine($"  Position: {npc.Position}");
                ctx.Console.Out.WriteLine($"  Rotation: {npc.Rotation}");
            }
            if (session.Field.Mobs.TryGetValue(objectId, out FieldNpc? mob)) {
                ctx.Console.Out.WriteLine($"Mob: {mob.Value.Metadata.Id} ({mob.Value.Metadata.Name})");
                ctx.Console.Out.WriteLine($"  Position: {mob.Position}");
                ctx.Console.Out.WriteLine($"  Rotation: {mob.Rotation}");
            }
        }
    }
}
