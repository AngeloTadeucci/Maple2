using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Numerics;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class PetCommand : GameCommand {
    private readonly GameSession session;
    private readonly NpcMetadataStorage npcStorage;
    private readonly ItemMetadataStorage itemStorage;

    public PetCommand(GameSession session, NpcMetadataStorage npcStorage, ItemMetadataStorage itemStorage) : base(AdminPermissions.SpawnItem, "pet", "Hungry field pet spawning.") {
        this.session = session;
        this.npcStorage = npcStorage;
        this.itemStorage = itemStorage;

        var id = new Argument<int>("id", "Id of pet to spawn.");

        AddArgument(id);
        this.SetHandler<InvocationContext, int>(Handle, id);
    }

    private void Handle(InvocationContext ctx, int petId) {
        if (session.Field is null) return;

        try {
            if (!itemStorage.TryGetPet(petId, out ItemMetadata? itemMetadata)) {
                ctx.Console.Error.WriteLine($"Invalid Pet: {petId}");
                return;
            }

            Vector3 position = session.Player.Position;
            Vector3 rotation = session.Player.Rotation;
            FieldPet? fieldPet = session.Field.SpawnPet(new Item(itemMetadata), position, rotation);
            if (fieldPet == null) {
                ctx.Console.Error.WriteLine($"Failed to spawn pet: {petId}");
                return;
            }

            session.Field.Broadcast(FieldPacket.AddPet(fieldPet));
            session.Field.Broadcast(ProxyObjectPacket.AddPet(fieldPet));

            ctx.ExitCode = 0;
        } catch (SystemException ex) {
            ctx.Console.Error.WriteLine(ex.Message);
            ctx.ExitCode = 1;
        }
    }
}
