using Maple2.Model.Enum;
using Maple2.Model.Game.Event;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Serilog;

namespace Maple2.Server.Game.Manager;

public class MentoringManager : IDisposable {
    private readonly GameSession session;
    public MentorRole Role => session.Player.Value.Character.MentorRole;


    private readonly ILogger logger = Log.Logger.ForContext<MentoringManager>();

    public MentoringManager(GameSession session) {
        this.session = session;
    }

    public void Dispose() {
        session.Dispose();
    }

    public void Load() {
        session.Send(MentorPacket.Init(session.Player));

        session.Send(MentorPacket.AssignReturningUser());
        //  session.Send(MentorPacket.Load());

        //session.Send(MentorPacket.MyList());
        // session.Send(MentorPacket.Unknown12());
    }

    public void UpdateRole(MentorRole role) {
        GameEvent? gameEvent = session.FindEvent(GameEventType.ReturnUser).FirstOrDefault();
        if (gameEvent?.Metadata.Data is not ReturnUser returnUser) {
            return;
        }

        foreach (int questId in returnUser.QuestIds) {
            session.Quest.Start(questId);
        }

        session.Send(MentorPacket.UpdateRole(role, session.Player.ObjectId));
        Load();
    }

}
