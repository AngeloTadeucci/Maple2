using System.CommandLine;
using System.CommandLine.Invocation;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class InsigniaCommand : GameCommand {
    private readonly GameSession session;

    public InsigniaCommand(GameSession session) : base(AdminPermissions.GameMaster, "insignia", "Set the player's insignia") {
        this.session = session;
        var insigniaId = new Argument<short>("insigniaId", "The ID of the insignia to set");
        AddArgument(insigniaId);

        this.SetHandler<InvocationContext, short>(Handle, insigniaId);
    }

    private void Handle(InvocationContext context, short insigniaId) {
        if (session.Field == null) return;

        if (!session.TableMetadata.InsigniaTable.Entries.TryGetValue(insigniaId, out InsigniaTable.Entry? insigniaMetadata)) {
            context.Console.WriteLine($"Insignia ID {insigniaId} does not exist.");
            context.Console.WriteLine($"Available insignias: {string.Join(", ", session.TableMetadata.InsigniaTable.Entries.Keys)}");
            return;
        }

        // Check and remove any existing insignia buffs
        if (session.TableMetadata.InsigniaTable.Entries.TryGetValue(session.Player.Value.Character.Insignia, out InsigniaTable.Entry? oldInsigniaMetadata) && oldInsigniaMetadata.BuffId > 0) {
            session.Player.Buffs.Remove(oldInsigniaMetadata.BuffId, session.Player.ObjectId);
        }

        session.Player.Value.Character.Insignia = insigniaId;

        // Apply the new insignia buff if applicable
        if (insigniaMetadata.BuffId > 0) {
            session.Player.Buffs.AddBuff(session.Player, session.Player, insigniaMetadata.BuffId, insigniaMetadata.BuffLevel, session.Field.FieldTick);
        }

        session.Field.Broadcast(InsigniaPacket.Update(session.Player.ObjectId, insigniaId, true));
        context.Console.WriteLine($"Insignia set to ID {insigniaId}.");
    }
}
