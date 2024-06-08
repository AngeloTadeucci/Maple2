using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Club;
using Maple2.Server.Core.Sync;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util.Sync;
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

        if (!SetClub(clubInfo)) {
            logger.Error("Failed to set club for {Session}", session);
            return;
        }
    }

    public void Dispose() {
        session.Dispose();
        tokenSource.Dispose();

        if (Club != null) {
            foreach (ClubMember member in Club.Members.Values) {
                member.Dispose();
                if (member.CharacterId == session.CharacterId) {
                    using GameStorage.Request db = session.GameStorage.Context();
                    db.SaveClubMember(member);
                }
            }
        }
    }

    public void Load() {
        if (Club == null || Club.State == ClubState.Staged) {
            return;
        }

        session.Send(ClubPacket.Update(Club));
    }

    public bool SetClub(ClubInfo info) {
        if (Club != null) {
            return false;
        }

        List<ClubMember> clubMembers = info.Members.Select(member => {
            if (!session.PlayerInfo.GetOrFetch(member.CharacterId, out PlayerInfo? playerInfo)) {
                logger.Error("Failed to get player info for character {CharacterId}", member.CharacterId);
                return null;
            }

            return new ClubMember {
                Info = playerInfo.Clone(),
                LoginTime = member.LoginTime,
                JoinTime = member.JoinTime,
                ClubId = info.Id,
            };
        }).WhereNotNull().ToList();

        ClubMember? leader = clubMembers.SingleOrDefault(member => member.CharacterId == info.LeaderId);
        if (leader == null) {
            logger.Error("Club {ClubId} does not have a valid leader", info.Id);
            session.Send(ClubPacket.Error(ClubError.s_club_err_unknown));
            return false;
        }

        Club = new Club(info.Id, info.Name, leader) {
            CreationTime = info.CreationTime,
            State = (ClubState) info.State,
        };

        foreach (ClubMember member in clubMembers) {
            if (Club.Members.TryAdd(member.Info.CharacterId, member)) {
                BeginListen(member);
            }
        }

        session.Player.Value.Character.ClubIds.Add(Id);
        session.PlayerInfo.SendUpdate(new PlayerUpdateRequest {
            AccountId = session.AccountId,
            CharacterId = session.CharacterId,
            Async = true,
            Clubs = new ClubsInfo {
                Id = {
                    session.Player.Value.Character.ClubIds
                },
            },
        });
        return true;
    }

    public void UpdateLeader(long characterId) {
        if (Club == null) {
            return;
        }

        string oldLeader = Club.Leader.Name;
        if (!Club.Members.TryGetValue(characterId, out ClubMember? newLeader)) {
            logger.Error("Failed to find new leader for club {ClubId}", Club.Id);
            session.Send(ClubPacket.Error(ClubError.s_club_err_unknown));
            return;
        }

        Club.Leader = newLeader;
        Club.LeaderId = newLeader.CharacterId;
        session.Send(ClubPacket.UpdateLeader(Id, oldLeader, newLeader.Name));
    }

    public void Rename(string newName, long changedTime) {
        if (Club == null) {
            return;
        }

        Club.Name = newName;
        Club.NameChangeCooldown = changedTime;
        session.Send(ClubPacket.Rename(Id, newName, changedTime));
    }

    public void Disband() {
        if (Club == null) {
            return;
        }

        foreach (ClubMember member in Club.Members.Values) {
            EndListen(member);
        }

        session.Send(ClubPacket.Disband(Club.Id, Club.Leader.Name));

        RemoveClub();
    }

    public void RemoveClub() {
        if (Club == null) {
            return;
        }

        session.Clubs.TryRemove(Id, out _);
        session.Player.Value.Character.ClubIds.Remove(Id);
        session.PlayerInfo.SendUpdate(new PlayerUpdateRequest {
            AccountId = session.AccountId,
            CharacterId = session.CharacterId,
            Async = true,
            Clubs = new ClubsInfo {
                Id = {
                    session.Player.Value.Character.ClubIds
                },
            },
        });
        Club = null;
    }

    public bool AddMember(string requestorName, ClubMember member) {
        if (Club == null) {
            return false;
        }
        if (!Club.Members.TryAdd(member.CharacterId, member)) {
            return false;
        }

        BeginListen(member);
        session.Send(ClubPacket.NotifyAcceptInvite(member, requestorName));
        return true;
    }

    public bool RemoveMember(long characterId) {
        if (Club == null) {
            return false;
        }

        if (!Club.Members.TryRemove(characterId, out ClubMember? member)) {
            return false;
        }

        EndListen(member);

        if (session.CharacterId == characterId) {
            session.Send(ClubPacket.Leave(Id, member.Name));
            RemoveClub();
        } else {
            session.Send(ClubPacket.LeaveNotice(Id, member.Name));
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
        if (cancel.IsCancellationRequested || Club == null || !Club.Members.TryGetValue(id, out ClubMember? member)) {
            return true;
        }

        bool wasOnline = member.Info.Online;
        string name = member.Info.Name;
        member.Info.Update(type, info);

        if (name != member.Info.Name) {
            session.Send(ClubPacket.UpdateMemberName(name, member.Name));
        }

        if (type == UpdateField.Map) {
            session.Send(ClubPacket.UpdateMemberMap(Id, member.Name, member.Info.MapId));
        } else {
            session.Send(ClubPacket.UpdateMember(member));
        }

        if (session.CharacterId != member.CharacterId && member.Info.Online != wasOnline) {
            session.Send(member.Info.Online
                ? ClubPacket.NotifyLogin(Id, member.Name)
                : ClubPacket.NotifyLogout(Id, member.Name, member.LoginTime));
        }
        return false;
    }
    #endregion
}
