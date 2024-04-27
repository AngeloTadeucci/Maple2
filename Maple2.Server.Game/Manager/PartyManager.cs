using System;
using System.Linq;
using System.Threading;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Party;
using Maple2.Server.Core.Sync;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util.Sync;
using Maple2.Server.World.Service;
using Maple2.Tools.Extensions;
using Serilog;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;

namespace Maple2.Server.Game.Manager;

public class PartyManager : IDisposable {
    private readonly GameSession session;
    private readonly WorldClient world;

    public Party? Party { get; private set; }
    public int Id => Party?.Id ?? 0;

    private readonly CancellationTokenSource tokenSource;

    private readonly ILogger logger = Log.Logger.ForContext<PartyManager>();

    public PartyManager(WorldClient world, GameSession session) {
        this.session = session;
        this.world = world;
        tokenSource = new CancellationTokenSource();

        PartyInfoResponse response = session.World.PartyInfo(new PartyInfoRequest {
            CharacterId = session.CharacterId,
        });
        if (response.Party != null) {
            SetParty(response.Party);
        }
    }

    public void Dispose() {
        session.Dispose();
        tokenSource.Dispose();

        if (Party == null) {
            return;
        }

        foreach (PartyMember member in Party.Members.Values) {
            member.Dispose();
        }

        if (!CheckDisband(session.CharacterId)) {
            // Find new leader
            if (session.CharacterId == Party.LeaderCharacterId) {
                world.Party(new PartyRequest {
                    RequestorId = session.CharacterId,
                    UpdateLeader = new PartyRequest.Types.UpdateLeader {
                        PartyId = Party.Id,
                    },
                });
            }
        }
    }

    public void Load() {
        if (Party == null) {
            return;
        }

        session.Send(PartyPacket.Load(Party));
    }

    public bool SetParty(PartyInfo info) {
        if (Party != null) {
            return false;
        }

        PartyMember[] members = info.Members.Select(member => {
            if (!session.PlayerInfo.GetOrFetch(member.CharacterId, out PlayerInfo? playerInfo)) {
                return null;
            }

            var result = new PartyMember {
                PartyId = info.Id,
                Info = playerInfo.Clone(),
                JoinTime = member.JoinTime,
                LoginTime = member.LoginTime,
            };
            return result;
        }).WhereNotNull().ToArray();

        PartyMember? leader = members.SingleOrDefault(member => member.CharacterId == info.LeaderCharacterId);
        if (leader == null) {
            logger.Error("Party {PartyId} does not have a valid leader", info.Id);
            session.Send(PartyPacket.Error(PartyError.s_party_err_not_found));
            return false;
        }

        var party = new Party(info.Id, leader) {
            CreationTime = info.CreationTime,
            DungeonId = info.DungeonId,
            MatchPartyName = info.MatchPartyName,
            MatchPartyId = info.MatchPartyId,
            IsMatching = info.IsMatching,
            RequireApproval = info.RequireApproval,
        };
        foreach (PartyMember member in members) {
            if (party.Members.TryAdd(member.CharacterId, member)) {
                BeginListen(member);
            }
        }

        Party = party;

        session.Player.Value.Character.PartyId = Party.Id;
        session.Send(PartyPacket.Load(Party));
        return true;
    }

    public void RemoveParty() {
        if (Party == null) {
            return;
        }

        Party = null;
        session.Player.Value.Character.PartyId = 0;
    }

    public bool AddMember(PartyMember member) {
        if (Party == null) {
            return false;
        }
        if (!Party.Members.TryAdd(member.CharacterId, member)) {
            return false;
        }

        BeginListen(member);
        session.Send(PartyPacket.Joined(member));
        return true;
    }

    public bool RemoveMember(long characterId, bool isKick, bool isSelf) {
        if (Party == null) {
            return false;
        }
        if (!Party.Members.TryRemove(characterId, out PartyMember? member)) {
            return false;
        }
        EndListen(member);

        session.Send(isKick ? PartyPacket.Kicked(member.CharacterId) : PartyPacket.Leave(member.CharacterId, isSelf));
        return true;
    }

    public bool UpdateLeader(long newLeaderCharacterId) {
        if (Party == null) {
            return false;
        }

        PartyMember? leader = Party.Members.Values.SingleOrDefault(member => member.CharacterId == newLeaderCharacterId);
        if (leader == null) {
            logger.Error("Party {PartyId} does not have a valid leader", Party.Id);
            session.Send(PartyPacket.Error(PartyError.s_party_err_not_found));
            return false;
        }

        Party.LeaderCharacterId = leader.CharacterId;
        Party.LeaderAccountId = leader.Info.AccountId;
        Party.LeaderName = leader.Info.Name;

        session.Send(PartyPacket.NotifyUpdateLeader(newLeaderCharacterId));
        return true;
    }

    public bool Disband() {
        if (Party == null) {
            return false;
        }

        foreach (PartyMember member in Party.Members.Values) {
            EndListen(member);
        }

        session.Send(PartyPacket.Disband());
        RemoveParty();
        return true;
    }

    public bool CheckDisband(long characterId) {
        // Check if any other player is online
        if (!Party!.Members.Values.Any(partyMember => partyMember.Info.Online && partyMember.CharacterId != characterId)) {
            world.Party(new PartyRequest {
                Disband = new PartyRequest.Types.Disband {
                    PartyId = Id,
                },
            });
            return true;
        }
        return false;
    }

    public PartyMember? GetMember(string name) {
        return Party?.Members.Values.FirstOrDefault(member => member.Name == name);
    }

    public PartyMember? GetMember(long characterId) {
        if (Party?.Members.TryGetValue(characterId, out PartyMember? member) == true) {
            return member;
        }

        return null;
    }

    #region PlayerInfo Events
    private void BeginListen(PartyMember member) {
        // Clean up previous token if necessary
        if (member.TokenSource != null) {
            logger.Warning("BeginListen called on Member {Id} that was already listening", member.CharacterId);
            EndListen(member);
        }

        member.TokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token);
        CancellationToken token = member.TokenSource.Token;
        var listener = new PlayerInfoListener(UpdateField.Party, (type, info) => SyncUpdate(token, member.CharacterId, type, info));
        session.PlayerInfo.Listen(member.Info.CharacterId, listener);
    }

    private void EndListen(PartyMember member) {
        member.TokenSource?.Cancel();
        member.TokenSource?.Dispose();
        member.TokenSource = null;
    }

    private bool SyncUpdate(CancellationToken cancel, long id, UpdateField type, IPlayerInfo info) {
        if (cancel.IsCancellationRequested || Party == null || !Party.Members.TryGetValue(id, out PartyMember? member)) {
            return true;
        }

        bool wasOnline = member.Info.Online;
        member.Info.Update(type, info);
        member.LoginTime = info.UpdateTime;

        if (type == UpdateField.Health || type == UpdateField.Level) {
            session.Send(PartyPacket.UpdateStats(member));
        } else {
            session.Send(PartyPacket.Update(member));
        }

        if (member.Info.Online != wasOnline) {
            session.Send(member.Info.Online
                ? PartyPacket.NotifyLogin(member)
                : PartyPacket.NotifyLogout(member.CharacterId));
        }
        return false;
    }
    #endregion
}
