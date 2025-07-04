using Maple2.Model.Game;
using Maple2.Model.Game.GroupChat;
using Maple2.Server.Core.Sync;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util.Sync;
using Maple2.Tools.Extensions;
using Serilog;

namespace Maple2.Server.Game.Manager;

public class GroupChatManager : IDisposable {
    private readonly GameSession session;

    private GroupChat? groupChat;

    private readonly CancellationTokenSource tokenSource;

    private readonly ILogger logger = Log.Logger.ForContext<GroupChatManager>();

    public GroupChatManager(GroupChatInfo info, GameSession session) {
        this.session = session;
        tokenSource = new CancellationTokenSource();
        SetGroupChat(info);
    }

    public void Dispose() {
        session.Dispose();
        tokenSource.Dispose();

        if (groupChat != null) {
            foreach (GroupChatMember member in groupChat.Members.Values) {
                member.Dispose();
            }
        }
    }

    public void Load() {
        if (groupChat == null) {
            return;
        }
        session.Send(GroupChatPacket.Load(groupChat));
    }

    public bool SetGroupChat(GroupChatInfo info) {
        if (this.groupChat != null) {
            return false;
        }

        GroupChatMember[] members = info.Members.Select(member => {
            if (!session.PlayerInfo.GetOrFetch(member.CharacterId, out PlayerInfo? playerInfo)) {
                return null;
            }

            var result = new GroupChatMember {
                Info = playerInfo.Clone(),
            };
            return result;
        }).WhereNotNull().ToArray();

        var groupChat = new GroupChat(info.Id);
        foreach (GroupChatMember member in members) {
            if (groupChat.Members.TryAdd(member.CharacterId, member)) {
                BeginListen(member);
            }
        }

        this.groupChat = groupChat;

        session.Send(GroupChatPacket.Load(this.groupChat));
        return true;
    }

    public bool AddMember(GroupChatMember sender, GroupChatMember receiver) {
        if (groupChat == null) {
            return false;
        }
        if (!groupChat.Members.TryAdd(receiver.CharacterId, receiver)) {
            return false;
        }

        BeginListen(receiver);
        session.Send(GroupChatPacket.AddMember(receiver.Info, sender.Name, groupChat.Id));
        return true;
    }

    public bool RemoveMember(long characterId) {
        if (groupChat == null) {
            return false;
        }
        if (!groupChat.Members.TryRemove(characterId, out GroupChatMember? member)) {
            return false;
        }
        EndListen(member);

        session.Send(characterId == session.CharacterId ? GroupChatPacket.Leave(groupChat.Id) :
            GroupChatPacket.RemoveMember(member.Name, groupChat.Id));
        return true;
    }


    public bool Disband() {
        if (groupChat == null) {
            return false;
        }

        foreach (GroupChatMember member in groupChat.Members.Values) {
            EndListen(member);
        }

        groupChat = null;
        return true;
    }

    public void CheckDisband() {
        if (groupChat == null) {
            return;
        }

        // Checking less or equal to 1 because member logging off is still online at this point
        if (groupChat.Members.Values.Count(member => member.Info.Online) <= 1) {
            session.World.GroupChat(new GroupChatRequest {
                Disband = new GroupChatRequest.Types.Disband(),
                GroupChatId = groupChat.Id,
                RequesterId = session.CharacterId,
            });
        }
    }

    public GroupChatMember? GetMember(long characterId) {
        if (groupChat?.Members.TryGetValue(characterId, out GroupChatMember? member) == true) {
            return member;
        }

        return null;
    }

    #region PlayerInfo Events
    private void BeginListen(GroupChatMember member) {
        // Clean up previous token if necessary
        if (member.TokenSource != null) {
            logger.Warning("BeginListen called on Member {Id} that was already listening", member.CharacterId);
            EndListen(member);
        }

        member.TokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token);
        CancellationToken token = member.TokenSource.Token;
        var listener = new PlayerInfoListener(UpdateField.GroupChat, (type, info) => SyncUpdate(token, member.CharacterId, type, info));
        session.PlayerInfo.Listen(member.Info.CharacterId, listener);
    }

    private void EndListen(GroupChatMember member) {
        member.TokenSource?.Cancel();
        member.TokenSource?.Dispose();
        member.TokenSource = null;
    }

    private bool SyncUpdate(CancellationToken cancel, long characterId, UpdateField type, IPlayerInfo info) {
        if (cancel.IsCancellationRequested || groupChat == null || !groupChat.Members.TryGetValue(characterId, out GroupChatMember? member)) {
            return true;
        }

        bool wasOnline = member.Info.Online;
        member.Info.Update(type, info);

        if (member.Info.Online != wasOnline) {
            session.Send(member.Info.Online
                ? GroupChatPacket.LoginNotice(member.Name, groupChat.Id)
                : GroupChatPacket.LogoutNotice(member.Name, groupChat.Id));
        }
        return false;
    }
    #endregion
}
