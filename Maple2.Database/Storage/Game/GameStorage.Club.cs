using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Game.Party;
using Maple2.Tools.Extensions;
using Z.EntityFramework.Plus;
using Club = Maple2.Model.Game.Club.Club;
using ClubMember = Maple2.Model.Game.Club.ClubMember;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public Club? GetClub(long clubId) {
            return Context.Club.Find(clubId);
        }

        public bool ClubExists(long clubId = 0, string clubName = "") {
            return Context.Club.Any(club => club.Id == clubId || club.Name == clubName);
        }

        public IList<long> ListClubs(long characterId) {
            return Context.ClubMember.Where(member => member.CharacterId == characterId)
                .Select(member => member.ClubId)
                .ToList();
        }

        public Club? CreateClub(string name, long leaderId, List<PartyMember> partyMembers) {
            BeginTransaction();
            var club = new Model.Club {
                Name = name,
                LeaderId = leaderId,
                CreationTime = DateTime.UtcNow,
                State = ClubState.Staged,
            };
            Context.Club.Add(club);
            if (!SaveChanges()) {
                return null;
            }

            foreach (PartyMember partyMember in partyMembers) {
                CreateClubMember(club.Id, partyMember.Info);
            }

            return Commit() ? GetClub(club.Id) : null;
        }

        public ClubMember? CreateClubMember(long clubId, PlayerInfo info) {
            var member = new Model.ClubMember {
                ClubId = clubId,
                CharacterId = info.CharacterId,
                LoginTime = DateTime.Now,
            };
            Context.ClubMember.Add(member);
            if (!SaveChanges()) {
                return null;
            }

            return new ClubMember {
                ClubId = member.ClubId,
                Info = info,
                JoinTime = member.CreationTime.ToEpochSeconds(),
                LoginTime = member.LoginTime.ToEpochSeconds(),
            };
        }

        public bool DeleteClub(long clubId) {
            BeginTransaction();

            int count = Context.Club.Where(club => club.Id == clubId).Delete();
            if (count == 0) {
                return false;
            }

            Context.ClubMember.Where(member => member.ClubId == clubId).Delete();

            return Commit();
        }

        public bool DeleteClubMember(long clubId, long characterId) {
            int count = Context.ClubMember.Where(member => member.ClubId == clubId && member.CharacterId == characterId).Delete();
            return SaveChanges() && count > 0;
        }

        public List<ClubMember> GetClubMembers(IPlayerInfoProvider provider, long clubId) {
            return Context.ClubMember.Where(member => member.ClubId == clubId)
                .AsEnumerable()
                .Select(member => {
                    PlayerInfo? info = provider.GetPlayerInfo(member.CharacterId);
                    return info == null ? null : new ClubMember {
                        Info = info,
                        JoinTime = member.CreationTime.ToEpochSeconds(),
                        ClubId = member.ClubId,
                        LoginTime = member.LoginTime.ToEpochSeconds(),
                    };
                }).WhereNotNull().ToList();
        }

        public bool SaveClub(Club club) {
            if (!Context.Club.Any(model => model.Id == club.Id)) {
                return false;
            }
            Context.Club.Update(club);
            SaveClubMembers(club.Id, club.Members.Values);
            return SaveChanges();
        }

        public bool SaveClubMembers(long clubId, ICollection<ClubMember> members) {
            Dictionary<long, ClubMember> saveMembers = members
                .ToDictionary(member => member.Info.CharacterId, member => member);
            IEnumerable<Model.ClubMember> existingMembers = Context.ClubMember
                .Where(member => member.ClubId == clubId)
                .Select(member => new Model.ClubMember {
                    CharacterId = member.CharacterId,
                });

            foreach (Model.ClubMember member in existingMembers) {
                if (saveMembers.Remove(member.CharacterId, out ClubMember? gameMember)) {
                    Model.ClubMember model = gameMember;
                    Context.ClubMember.Update(model);
                } else {
                    Context.ClubMember.Remove(member);
                }
            }
            Context.ClubMember.AddRange(saveMembers.Values.Select<ClubMember, Model.ClubMember>(member => member));

            return true;
        }

        public bool SaveClubMember(ClubMember member) {
            Model.ClubMember? model = Context.ClubMember.Find(member.ClubId, member.Info.CharacterId);
            if (model == null) {
                return false;
            }

            Context.ClubMember.Update(member);
            return SaveChanges();
        }
    }
}
