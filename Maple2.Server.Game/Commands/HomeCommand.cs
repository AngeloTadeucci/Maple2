using System.CommandLine;
using System.CommandLine.Invocation;
using Maple2.Database.Storage;
using Maple2.Model.Game;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class HomeCommand : Command {
    private const string NAME = "home";
    private const string DESCRIPTION = "Home commands.";

    public HomeCommand(GameSession session, TableMetadataStorage tableMetadata) : base(NAME, DESCRIPTION) {
        AddCommand(new ExpCommand(session, tableMetadata));
        AddCommand(new LevelCommand(session));
        AddCommand(new SetExpCommand(session));
    }

    private class ExpCommand : Command {
        private readonly GameSession session;
        private readonly TableMetadataStorage tableMetadata;

        public ExpCommand(GameSession session, TableMetadataStorage tableMetadata) : base("exp", "Set home decoration exp.") {
            this.session = session;
            this.tableMetadata = tableMetadata;

            var exp = new Argument<int>("exp", "Decoration exp.");

            AddArgument(exp);
            this.SetHandler<InvocationContext, int>(Handle, exp);
        }

        private void Handle(InvocationContext context, int exp) {
            Home home = session.Player.Value.Home;
            home.GainExp(exp, tableMetadata.MasteryUgcHousingTable.Entries);
            session.Send(CubePacket.DesignRankReward(home));
        }
    }

    private class LevelCommand : Command {
        private readonly GameSession session;

        public LevelCommand(GameSession session) : base("level", "Set home decoration level.") {
            this.session = session;

            var level = new Argument<int>("level", "Decoration level.");

            AddArgument(level);
            this.SetHandler<InvocationContext, int>(Handle, level);
        }

        private void Handle(InvocationContext context, int level) {
            Home home = session.Player.Value.Home;
            home.DecorationLevel = level;
            session.Send(CubePacket.DesignRankReward(home));
        }
    }

    private class SetExpCommand : Command {
        private readonly GameSession session;

        public SetExpCommand(GameSession session) : base("setexp", "Set home decoration exp.") {
            this.session = session;

            var exp = new Argument<int>("exp", "Decoration exp.");

            AddArgument(exp);
            this.SetHandler<InvocationContext, int>(Handle, exp);
        }

        private void Handle(InvocationContext context, int exp) {
            Home home = session.Player.Value.Home;
            home.DecorationExp = exp;
            session.Send(CubePacket.DesignRankReward(home));
        }
    }
}
