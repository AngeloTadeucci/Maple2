using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Numerics;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class NpcCommand : GameCommand {
    private readonly GameSession session;
    private readonly NpcMetadataStorage npcStorage;

    public NpcCommand(GameSession session, NpcMetadataStorage npcStorage) : base(AdminPermissions.SpawnNpc, "npc", "Npc spawning.") {
        this.session = session;
        this.npcStorage = npcStorage;

        var id = new Argument<int>("id", "Id of npc to spawn.");

        AddArgument(id);
        this.SetHandler<InvocationContext, int>(Handle, id);
    }

    private void Handle(InvocationContext ctx, int npcId) {
        try {
            if (session.Field == null || !npcStorage.TryGet(npcId, out NpcMetadata? metadata)) {
                ctx.Console.Error.WriteLine($"Invalid Npc: {npcId}");
                return;
            }

            Vector3 position = session.Player.Position;
            Vector3 rotation = session.Player.Rotation;
            FieldNpc? fieldNpc = session.Field.SpawnNpc(metadata, position, rotation);
            if (fieldNpc == null) {
                ctx.Console.Error.WriteLine($"Failed to spawn npc: {npcId}");
                return;
            }
            session.Field.Broadcast(FieldPacket.AddNpc(fieldNpc));
            session.Field.Broadcast(ProxyObjectPacket.AddNpc(fieldNpc));
            fieldNpc.Update(Environment.TickCount64);

            ctx.ExitCode = 0;
        } catch (SystemException ex) {
            ctx.Console.Error.WriteLine(ex.Message);
            ctx.ExitCode = 1;
        }
    }
}

public class AnimateNpcCommand : GameCommand {
    private readonly GameSession session;
    private readonly NpcMetadataStorage npcStorage;

    public AnimateNpcCommand(GameSession session, NpcMetadataStorage npcStorage) : base(AdminPermissions.Debug, "anim-npc", "Animate npc.") {
        this.session = session;
        this.npcStorage = npcStorage;

        var id = new Argument<int?>("id", () => null, "Object id of npc to animate.");
        var animation = new Argument<string?>("animation", () => null, "Animation to play.");
        var duration = new Option<int>("--duration", () => -1, "Duration of animation in milliseconds.");

        AddArgument(id);
        AddArgument(animation);
        AddOption(duration);

        this.SetHandler<InvocationContext, int?, string?, int>(Handle, id, animation, duration);
    }

    private void Handle(InvocationContext ctx, int? objectId, string? animation, int duration) {
        if (session.Field == null) {
            ctx.Console.Error.WriteLine("No field loaded.");
            return;
        }

        if (objectId is null) {
            ctx.Console.Error.WriteLine("Npcs in map:");
            session.Field.Npcs.Values.ToList().ForEach(npc => {
                int id = npc.Value.Id;
                int objectId = npc.ObjectId;
                string? name = npc.Value.Metadata.Name;
                ctx.Console.Out.WriteLine($"ObjectId: {objectId}, Id: {id}, Name: {name}");
            });
            return;
        }

        session.Field.Npcs.TryGetValue(objectId.Value, out FieldNpc? fieldNpc);
        if (fieldNpc is null) {
            ctx.Console.Error.WriteLine($"Invalid Npc object id: {objectId}");
            return;
        }

        if (animation is null) {
            ctx.Console.Error.WriteLine("Available Animations:");
            foreach (string anim in fieldNpc.Value.Animations.Keys) {
                ctx.Console.Out.WriteLine(anim);
            }
            return;
        }

        string? animationKey = fieldNpc.Value.Animations.Keys.FirstOrDefault(anim => anim.Equals(animation, StringComparison.CurrentCultureIgnoreCase));

        if (animationKey is null) {
            ctx.Console.Error.WriteLine($"Invalid Animation: {animation}");
            ctx.Console.Error.WriteLine($"Available Animations: {string.Join(", ", fieldNpc.Value.Animations.Keys)}");
            return;
        }

        fieldNpc.Animate(animationKey, duration);
    }
}
