using System.CommandLine;
using System.CommandLine.Invocation;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model.Room;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Commands.HomeCommands;

public class SurveyCommand : Command {
    private readonly GameSession session;

    public SurveyCommand(GameSession session) : base("survey", "Creates a survey") {
        this.session = session;
        IsHidden = Constant.HideHomeCommands;

        AddAlias("poll");

        var options = new Argument<string[]>(name: "options", description: "Survey options");
        AddArgument(options);

        this.SetHandler<InvocationContext, string[]>(Handle, options);
    }

    private void Handle(InvocationContext context, string[] options) {
        Character character = session.Player.Value.Character;
        if (session.Field is not HomeFieldManager field) {
            return;
        }

        Plot? plot = session.Housing.GetFieldPlot();
        if (plot == null || plot.OwnerId != character.AccountId) {
            return;
        }

        if (options.Length == 0) {
            session.Send(HomeActionPacket.SurveyMessage());
            return;
        }

        string firstOption = options.First().ToLower();
        switch (firstOption) {
            case "open" or "secret": {
                    string[] topic = options.Skip(1).ToArray();
                    if (topic.Length == 0) {
                        session.Send(HomeActionPacket.SurveyMessage());
                        return;
                    }

                    string question = string.Join(" ", topic);
                    if (string.IsNullOrEmpty(question)) {
                        session.Send(HomeActionPacket.SurveyMessage());
                        return;
                    }

                    var survey = new HomeSurvey(FieldManager.NextGlobalId(), question, firstOption == "open");

                    field.SetHomeSurvey(survey);
                    session.Send(HomeActionPacket.SurveyQuestion(survey));
                    return;
                }
            case "add": {
                    HomeSurvey? survey = field.HomeSurvey;
                    if (survey is null || survey.Ended) {
                        session.Send(HomeActionPacket.SurveyMessage());
                        return;
                    }

                    string[] option = options.Skip(1).ToArray();
                    if (option.Length == 0) {
                        return;
                    }

                    string optionText = string.Join(" ", option);
                    if (string.IsNullOrEmpty(optionText)) {
                        return;
                    }

                    if (survey.AddOption(optionText)) {
                        session.Send(HomeActionPacket.SurveyAddOption(survey));
                    }
                    return;
                }

            case "start": {
                    HomeSurvey? survey = field.HomeSurvey;
                    if (survey is null || survey.Started || survey.Ended) {
                        session.Send(HomeActionPacket.SurveyMessage());
                        return;
                    }

                    survey.Start(session.CharacterId, session.Field.Players.Values.Select(x => x.Value.Character).ToList());
                    session.Field.Broadcast(HomeActionPacket.SurveyStart(survey));
                    return;
                }
            case "end": {
                    HomeSurvey? survey = field.HomeSurvey;
                    if (survey is null || survey.Ended) {
                        session.Send(HomeActionPacket.SurveyMessage());
                        return;
                    }

                    session.Field.Broadcast(HomeActionPacket.SurveyEnd(survey));
                    survey.End();
                    field.RemoveHomeSurvey();
                    return;
                }
        }
    }
}
