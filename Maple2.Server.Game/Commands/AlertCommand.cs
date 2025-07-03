using System.CommandLine;
using System.CommandLine.Invocation;
using Maple2.Model.Enum;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class AlertCommand : GameCommand {
    private readonly GameSession session;

    public AlertCommand(GameSession session) : base(AdminPermissions.Alert, "alert", "Send alert to the entire server.") {
        this.session = session;

        var message = new Argument<string[]>("message", () => [], "Message to display");
        var flag = new Option<int>(["--flag", "-f"], () => 1, $"Flags set for alert type. Possible flags are:\n"
                                                              + $"1 - Message\n" +
                                                              "4 - Alert\n" +
                                                              "16 - Mint\n" +
                                                              "64 - MessageBox\n" +
                                                              "128 - Disconnect after OK.\n" +
                                                              "512 - LargeAlert\n" +
                                                              "1024 - Banner");
        var duration = new Option<int>(["--duration", "-d"], () => 10000, "Duration in milliseconds. Only used if flag has LargeAlert.");
        AddArgument(message);
        AddOption(flag);
        AddOption(duration);

        this.SetHandler<InvocationContext, string[], int, int>(Handle, message, flag, duration);
    }

    private void Handle(InvocationContext ctx, string[] message, int flag, int duration) {
        string msg = string.Join(" ", message);
        if (string.IsNullOrEmpty(msg)) {
            ctx.Console.WriteLine("Message cannot be empty.");
            return;
        }

        try {
            var response = session.World.Admin(new AdminRequest {
                Alert = new AdminRequest.Types.Alert {
                    Message = msg,
                    Flags = flag,
                    Duration = duration,
                },
                RequesterId = session.CharacterId,
            });

            if (!string.IsNullOrEmpty(response.Message)) {
                ctx.Console.WriteLine(response.Message);
            }
        } catch (Exception ex) {
            ctx.Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
