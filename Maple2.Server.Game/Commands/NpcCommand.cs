using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Numerics;
using Maple2.Database.Storage;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class NpcCommand : Command {
    private const string NAME = "npc";
    private const string DESCRIPTION = "Npc spawning.";

    private readonly GameSession session;
    private readonly NpcMetadataStorage npcStorage;

    public NpcCommand(GameSession session, NpcMetadataStorage npcStorage) : base(NAME, DESCRIPTION) {
        this.session = session;
        this.npcStorage = npcStorage;

        Argument<int> id = new Argument<int>("id", "Id of npc to spawn.");

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

public class AnimateNpcCommand : Command {
    private const string NAME = "anim-npc";
    private const string DESCRIPTION = "Animate npc.";

    private readonly GameSession session;
    private readonly NpcMetadataStorage npcStorage;

    public AnimateNpcCommand(GameSession session, NpcMetadataStorage npcStorage) : base(NAME, DESCRIPTION) {
        this.session = session;
        this.npcStorage = npcStorage;

        Argument<int?> id = new Argument<int?>("id", () => null, "Id of npc to spawn.");
        Argument<string?> animation = new Argument<string?>("animation", () => null, "Animation to play.");

        AddArgument(id);
        AddArgument(animation);

        this.SetHandler<InvocationContext, int?, string?>(Handle, id, animation);
    }

    private void Handle(InvocationContext ctx, int? npcId, string? animation) {
        if (session.Field == null) {
            ctx.Console.Error.WriteLine("No field loaded.");
            return;
        }

        if (npcId is null) {
            ctx.Console.Error.WriteLine("Npcs in map:");
            session.Field.Npcs.Values.ToList().ForEach(npc => {
                int id = npc.Value.Id;
                string? name = npc.Value.Metadata.Name;
                ctx.Console.Out.WriteLine($"Id: {id}, Name: {name}");
            });
            return;
        }

        FieldNpc? fieldNpc = session.Field.Npcs.Values.FirstOrDefault(npc => npc.Value.Id == npcId);
        if (fieldNpc is null) {
            ctx.Console.Error.WriteLine($"Invalid Npc: {npcId}");
            return;
        }

        if (animation is null) {
            ctx.Console.Error.WriteLine("Available Animations:");
            foreach (string anim in fieldNpc.Value.Animations.Keys) {
                ctx.Console.Out.WriteLine(anim);
            }
            return;
        }

        string? animationKey = fieldNpc.Value.Animations.Keys.FirstOrDefault(anim => anim.ToLower() == animation.ToLower());

        if (animationKey is null) {
            ctx.Console.Error.WriteLine($"Invalid Animation: {animation}");
            ctx.Console.Error.WriteLine($"Available Animations: {string.Join(", ", fieldNpc.Value.Animations.Keys)}");
            return;
        }

        AnimationSequence? animationSequence = fieldNpc.Value.Animations[animationKey];

        fieldNpc.Animate(animationKey);
    }
}