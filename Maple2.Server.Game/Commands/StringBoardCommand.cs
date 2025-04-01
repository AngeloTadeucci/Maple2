using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class StringBoardCommand : Command {
    private const string NAME = "stringboard";
    private const string DESCRIPTION = "Manage string board events (scrolling text top left).";

    private readonly GameSession session;

    public StringBoardCommand(GameSession session) : base(NAME, DESCRIPTION) {
        this.session = session;
        AddCommand(new AddStringBoardCommand(session));
        AddCommand(new RemoveStringBoardCommand(session));
        AddCommand(new ListStringBoardCommand(session));
    }

    private class AddStringBoardCommand : Command {
        private readonly GameSession session;

        public AddStringBoardCommand(GameSession session) : base("add", "Add string board event.") {
            this.session = session;

            var message = new Argument<string[]>("message", Array.Empty<string>, "Message of string board.");

            AddArgument(message);
            this.SetHandler<InvocationContext, string[]>(Handle, message);
        }

        private void Handle(InvocationContext ctx, string[] message) {
            string text = string.Join(" ", message);

            try {
                AdminResponse response = session.World.Admin(new AdminRequest {
                    RequesterId = session.CharacterId,
                    AddStringBoard = new AdminRequest.Types.AddStringBoard {
                        Message = text,
                    },
                });

                if (!string.IsNullOrEmpty(response.Message)) {
                    ctx.Console.Out.WriteLine($"Failed to add string board: {response.Message}");
                    return;
                }
            } catch (Exception ex) {
                ctx.Console.Out.WriteLine($"Failed to add string board: {ex.Message}");
                return;
            }
        }
    }

    private class RemoveStringBoardCommand : Command {
        private readonly GameSession session;

        public RemoveStringBoardCommand(GameSession session) : base("remove", "Remove string board event.") {
            this.session = session;

            var id = new Argument<int>("id", "id of string board.");

            AddArgument(id);
            this.SetHandler<InvocationContext, int>(Handle, id);

        }

        private void Handle(InvocationContext ctx, int id) {
            try {
                AdminResponse response = session.World.Admin(new AdminRequest {
                    RequesterId = session.CharacterId,
                    RemoveStringBoard = new AdminRequest.Types.RemoveStringBoard {
                        Id = id,
                    },
                });

                if (!string.IsNullOrEmpty(response.Message)) {
                    ctx.Console.Out.WriteLine($"Failed to remove string board: {response.Message}");
                    return;
                }
            } catch (Exception ex) {
                ctx.Console.Out.WriteLine($"Failed to remove string board: {ex.Message}");
                return;
            }
        }
    }

    private class ListStringBoardCommand : Command {
        private readonly GameSession session;

        public ListStringBoardCommand(GameSession session) : base("list", "List all string boards.") {
            this.session = session;
            this.SetHandler<InvocationContext>(Handle);

        }

        private void Handle(InvocationContext ctx) {
            try {
                AdminResponse response = session.World.Admin(new AdminRequest {
                    RequesterId = session.CharacterId,
                    ListStringBoard = new AdminRequest.Types.ListStringBoard(),
                });

                if (string.IsNullOrEmpty(response.Message)) {
                    ctx.Console.Out.WriteLine($"Failed to find any string boards.");
                    return;
                }

                ctx.Console.Out.WriteLine(response.Message);
            } catch (Exception ex) {
                ctx.Console.Out.WriteLine($"Failed to find string board: {ex.Message}");
                return;
            }
        }
    }
}
