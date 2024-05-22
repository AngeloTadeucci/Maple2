using Maple2.Model.Game;
using Maple2.Server.Core.Sync;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util.Sync;
using Maple2.Server.World.Service;
using Maple2.Tools.Extensions;
using Serilog;

namespace Maple2.Server.Game.Manager;

public class ClubManager : IDisposable {
    private readonly GameSession session;

    public Club? Club { get; private set; }
    public long Id => Club?.Id ?? 0;

    private readonly CancellationTokenSource tokenSource;

    private readonly ILogger logger = Log.Logger.ForContext<ClubManager>();

    public ClubManager(ClubInfo clubInfo, GameSession session) {
        this.session = session;
        tokenSource = new CancellationTokenSource();

        SetClub(clubInfo);
    }

    public void Dispose() {
        session.Dispose();
        tokenSource.Dispose();

        if (Club != null) {
            foreach (ClubMember member in Club.Members) {
                member.Dispose();
            }
        }
    }

    public void Load() {
        if (Club == null) {
            return;
        }

        session.Send(ClubPacket.Update(Club));
    }

    public bool SetClub(ClubInfo info) {
        if (Club != null) {
            return false;
        }

        var clubMembers = info.Members.Select(member => {
            if (!session.PlayerInfo.GetOrFetch(member.CharacterId, out PlayerInfo? playerInfo)) {
                logger.Error("Failed to get player info for character {CharacterId}", member.CharacterId);
                return null;
            }

            return new ClubMember {
                Info = playerInfo,
            };
        }).WhereNotNull().ToList();

        if (!session.PlayerInfo.GetOrFetch(info.LeaderId, out PlayerInfo? leaderInfo)) {
            logger.Error("Failed to get player info for character {CharacterId}", info.LeaderId);
            return false;
        }

        ClubMember leader = new ClubMember {
            Info = leaderInfo,
        };

        Club = new Club(info.Id, info.Name, leader) {
            CreationTime = info.CreationTime,
        };

        foreach (ClubMember member in clubMembers) {
            Club.Members.Add(member);
            BeginListen(member);
        }

        return true;
    }

    #region PlayerInfo Events
    private void BeginListen(ClubMember member) {
        // Clean up previous token if necessary
        if (member.TokenSource != null) {
            logger.Warning("BeginListen called on Member {Id} that was already listening", member.Info.CharacterId);
            EndListen(member);
        }

        member.TokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token);
        CancellationToken token = member.TokenSource.Token;
        var listener = new PlayerInfoListener(UpdateField.Club, (type, info) => SyncUpdate(token, member.Info.CharacterId, type, info));
        session.PlayerInfo.Listen(member.Info.CharacterId, listener);
    }

    private void EndListen(ClubMember member) {
        member.TokenSource?.Cancel();
        member.TokenSource?.Dispose();
        member.TokenSource = null;
    }

    private bool SyncUpdate(CancellationToken cancel, long id, UpdateField type, IPlayerInfo info) {
        var member = Club?.Members.FirstOrDefault(member => member.Info.CharacterId == id);
        if (cancel.IsCancellationRequested || Club == null || member == null) {
            return true;
        }

        bool wasOnline = member.Info.Online;
        string name = member.Info.Name;
        member.Info.Update(type, info);
        // member.LoginTime = info.UpdateTime;

        // if (name != member.Info.Name) {
        //     session.Send(GuildPacket.UpdateMemberName(name, member.Name));
        // }

        // if (type == UpdateField.Map) {
        //     session.Send(GuildPacket.UpdateMemberMap(member.Name, member.Info.MapId));
        // } else {
        //     session.Send(GuildPacket.UpdateMember(member.Info));
        // }

        // if (member.Info.Online != wasOnline) {
        //     session.Send(member.Info.Online
        //         ? GuildPacket.NotifyLogin(member.Name)
        //         : GuildPacket.NotifyLogout(member.Name, member.LoginTime));
        // }
        return false;
    }
    #endregion
}