using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class QuestHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.Quest;

    private enum Command : byte {
        Accept = 2,
        Complete = 4,
        Abandon = 6,
        Exploration = 8,
        Tracking = 9,
        GoToNpc = 12,
        GoToDungeon = 13,
        SkyFortress = 14,
        MapleGuide = 16,
        ResumeDungeon = 19,
        Dispatch = 20,
        RemoteComplete = 24, // Maybe? This is mainly used for Navigator
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required TableMetadataStorage TableMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Accept:
                HandleAccept(session, packet);
                break;
            case Command.Complete:
                HandleComplete(session, packet);
                break;
            case Command.Abandon:
                HandleForfeit(session, packet);
                break;
            case Command.Exploration:
                HandleAddExplorationQuests(session, packet);
                break;
            case Command.Tracking:
                HandleSetTracking(session, packet);
                break;
            case Command.GoToNpc:
                HandleGoToNpc(session, packet);
                break;
            case Command.SkyFortress:
                HandleSkyFortressTeleport(session);
                break;
            case Command.MapleGuide:
                HandleMapleGuide(session, packet);
                break;
            case Command.RemoteComplete:
                HandleRemoteComplete(session, packet);
                return;
        }
    }

    private static void HandleAccept(GameSession session, IByteReader packet) {
        int questId = packet.ReadInt();
        int npcObjectId = packet.ReadInt();

        if (!session.QuestMetadata.TryGet(questId, out QuestMetadata? metadata)) {
            return;
        }

        bool isPostbox = npcObjectId == 0 && metadata.Basic.UsePostbox;
        bool fieldNpcExists = session.Field != null && session.Field.Npcs.TryGetValue(npcObjectId, out FieldNpc? _);

        if (!isPostbox && !fieldNpcExists) {
            return;
        }

        session.Quest.Start(questId);
    }

    private void HandleComplete(GameSession session, IByteReader packet) {
        int questId = packet.ReadInt();

        if (!session.Quest.TryGetQuest(questId, out Quest? quest)) {
            return;
        }
        if (!session.Quest.Complete(quest)) {
            Logger.Warning("Could not complete quest {QuestId}", questId);
        }
    }

    private static void HandleForfeit(GameSession session, IByteReader packet) {
        int questId = packet.ReadInt();

        if (!session.Quest.TryGetQuest(questId, out Quest? quest) || !quest.Metadata.Basic.Forfeitable) {
            return;
        }

        if (quest.CompletionCount > 0) {
            quest.State = QuestState.Completed; // ?? how do you revert?
        }

        if (session.Quest.Remove(quest)) {
            session.Send(QuestPacket.Abandon(quest.Id));
        }
    }

    private static void HandleAddExplorationQuests(GameSession session, IByteReader packet) {
        int listSize = packet.ReadInt();

        for (int i = 0; i < listSize; i++) {
            int questId = packet.ReadInt();

            if (session.Quest.TryGetQuest(questId, out Quest? _)) {
                continue;
            }

            session.Quest.Start(questId);
        }
    }

    private static void HandleSetTracking(GameSession session, IByteReader packet) {
        int questId = packet.ReadInt();
        bool tracking = packet.ReadBool();

        if (!session.Quest.TryGetQuest(questId, out Quest? quest) || quest.State is QuestState.Completed) {
            return;
        }

        session.Send(QuestPacket.SetTracking(questId, tracking));
    }

    private static void HandleGoToNpc(GameSession session, IByteReader packet) {
        int questId = packet.ReadInt();

        if (!session.Quest.TryGetQuest(questId, out Quest? quest) || !quest.Metadata.GoToNpc.Enabled) {
            return;
        }

        session.Send(session.PrepareField(quest.Metadata.GoToNpc.MapId, portalId: quest.Metadata.GoToNpc.PortalId)
            ? FieldEnterPacket.Request(session.Player)
            : FieldEnterPacket.Error(MigrationError.s_move_err_default));
    }

    private void HandleMapleGuide(GameSession session, IByteReader packet) {
        int id = packet.ReadInt();

        if (!TableMetadata.LearningQuestTable.Entries.TryGetValue(id, out LearningQuestTable.Entry? metadata)) {
            return;
        }

        if (session.Player.Value.Character.Level < metadata.RequiredLevel ||
            (session.Quest.TryGetQuest(metadata.QuestId, out Quest? quest) && quest.State == QuestState.Completed)) {
            return;
        }

        session.Send(session.PrepareField(metadata.GoToMapId, metadata.GoToPortalId, session.CharacterId)
            ? FieldEnterPacket.Request(session.Player)
            : FieldEnterPacket.Error(MigrationError.s_move_err_default));
    }

    private static void HandleRemoteComplete(GameSession session, IByteReader packet) {
        int questId = packet.ReadInt();

        if (!session.Quest.TryGetQuest(questId, out Quest? quest)) {
            return;
        }

        session.Quest.Complete(quest);
    }

    private static void HandleSkyFortressTeleport(GameSession session) {
        session.Send(session.PrepareField(2000422, -1, session.CharacterId)
            ? FieldEnterPacket.Request(session.Player)
            : FieldEnterPacket.Error(MigrationError.s_move_err_default));
    }
}
