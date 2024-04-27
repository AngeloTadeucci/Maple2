using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands;

public class QuestCommand : Command {
    private const string NAME = "quest";
    private const string DESCRIPTION = "Field information.";

    private readonly GameSession session;
    private readonly QuestMetadataStorage questStorage;

    public QuestCommand(GameSession session, QuestMetadataStorage questStorage) : base(NAME, DESCRIPTION) {
        this.session = session;
        this.questStorage = questStorage;

        var id = new Argument<int>("id", "Id of quest to modify.");
        var state = new Option<QuestState>(new[] { "--state", "-s" }, () => QuestState.None, "State of the quest.");

        AddArgument(id);
        AddOption(state);
        this.SetHandler<InvocationContext, int, QuestState>(Handle, id, state);
    }

    private void Handle(InvocationContext ctx, int id, QuestState state) {
        if (!questStorage.TryGet(id, out QuestMetadata? metadata)) {
            ctx.Console.Error.WriteLine($"Quest id {id} does not exist.");
            ctx.ExitCode = 1;
            return;
        }

        if (state is QuestState.Started or QuestState.Completed) {
            Unlock unlock = session.Player.Value.Unlock;
            if (!unlock.Quests.TryGetValue(id, out Quest? quest)) {
                quest = new Quest(metadata) {
                    State = QuestState.Started,
                    Track = true,
                    StartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                };
                unlock.Quests[id] = quest;
                session.Send(QuestPacket.Start(quest));
            }

            if (state == quest.State) {
                ctx.Console.Error.WriteLine($"Quest is already in state: {state}");
                return;
            }

            if (state == QuestState.Completed) {
                quest.State = QuestState.Completed;
                quest.CompletionCount++;
                quest.EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                session.Send(QuestPacket.Complete(quest));
            }
        }
    }
}
