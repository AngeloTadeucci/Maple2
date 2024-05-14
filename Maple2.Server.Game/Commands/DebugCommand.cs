using Maple2.Server.Game.Session;
using System.CommandLine.Invocation;
using System.CommandLine;
using Maple2.Database.Storage;
using System.CommandLine.IO;

namespace Maple2.Server.Game.Commands;

public class DebugCommand : Command {
    private const string NAME = "debug";
    private const string DESCRIPTION = "Debug information management.";

    private readonly NpcMetadataStorage npcStorage;

    public DebugCommand(GameSession session, NpcMetadataStorage npcStorage) : base(NAME, DESCRIPTION) {
        this.npcStorage = npcStorage;

        AddCommand(new DebugNpcAiCommand(session, npcStorage));
        AddCommand(new DebugAnimationCommand(session));
        AddCommand(new DebugSkillsCommand(session));
    }

    public class DebugNpcAiCommand : Command {
        private readonly GameSession session;
        private readonly NpcMetadataStorage npcStorage;

        public DebugNpcAiCommand(GameSession session, NpcMetadataStorage npcStorage) : base("ai", "Toggles displaying npc AI debug info.") {
            this.session = session;
            this.npcStorage = npcStorage;

            var enable = new Argument<bool?>("enable", () => true, "Enables & disables debug messages. Prints all AI state if true.");

            AddArgument(enable);

            this.SetHandler<InvocationContext, bool?>(Handle, enable);
        }

        private void Handle(InvocationContext ctx, bool? enabled) {
            if (session.Field == null) {
                ctx.Console.Error.WriteLine("No field loaded.");
                return;
            }

            session.Player.DebugAi = enabled ?? true;

            string message = enabled ?? true ? "Enabled" : "Disabled";
            ctx.Console.Out.WriteLine($"{message} AI debug info printing");
        }
    }


    private class DebugAnimationCommand : Command {
        private readonly GameSession session;

        public DebugAnimationCommand(GameSession session) : base("anims", "Prints player animation info.") {
            this.session = session;

            var enable = new Argument<bool?>("enable", () => true, "Enables & disables debug messages. Prints all animation state if true.");

            AddArgument(enable);

            this.SetHandler<InvocationContext, bool?>(Handle, enable);
        }

        private void Handle(InvocationContext ctx, bool? enabled) {
            session.Player.AnimationState.DebugPrintAnimations = enabled ?? true;

            string message = enabled ?? true ? "Enabled" : "Disabled";
            ctx.Console.Out.WriteLine($"{message} animation debug info printing");
        }
    }

    private class DebugSkillsCommand : Command {
        private readonly GameSession session;

        public DebugSkillsCommand(GameSession session) : base("skills", "Prints player skill packet info.") {
            this.session = session;

            var enable = new Argument<bool?>("enable", () => true, "Enables & disables debug messages. Prints all skill cast packets if true.");

            AddArgument(enable);

            this.SetHandler<InvocationContext, bool?>(Handle, enable);
        }

        private void Handle(InvocationContext ctx, bool? enabled) {
            session.Player.DebugSkills = enabled ?? true;

            string message = enabled ?? true ? "Enabled" : "Disabled";
            ctx.Console.Out.WriteLine($"{message} skill cast packet debug info printing");
        }
    }
}
