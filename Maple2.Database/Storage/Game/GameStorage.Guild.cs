using System;
using System.Collections.Generic;
using System.Linq;
using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Tools.Extensions;
using Z.EntityFramework.Plus;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public Guild? GetGuild(long guildId) {
            return LoadGuild(guildId, string.Empty);
        }

        public Guild? GetGuild(string guildName) {
            return LoadGuild(0, guildName);
        }

        public bool GuildExists(long guildId = 0, string guildName = "") {
            return Context.Guild.Any(guild => guild.Id == guildId || guild.Name == guildName);
        }

        public IList<GuildMember> GetGuildMembers(IPlayerInfoProvider provider, long guildId) {
            return Context.GuildMember.Where(member => member.GuildId == guildId)
                .AsEnumerable()
                .Select(member => {
                    PlayerInfo? info = provider.GetPlayerInfo(member.CharacterId);
                    return info == null ? null : new GuildMember {
                        GuildId = member.GuildId,
                        Info = info,
                        Message = member.Message,
                        Rank = member.Rank,
                        WeeklyContribution = member.WeeklyContribution,
                        TotalContribution = member.TotalContribution,
                        DailyDonationCount = member.DailyDonationCount,
                        JoinTime = member.CreationTime.ToEpochSeconds(),
                        LoginTime = member.LoginTime.ToEpochSeconds(),
                        CheckinTime = member.CheckinTime.ToEpochSeconds(),
                        DonationTime = member.DonationTime.ToEpochSeconds(),
                    };
                })
                .WhereNotNull()
                .ToList();
        }

        public Guild? CreateGuild(string name, long leaderId) {
            BeginTransaction();

            var guild = new Model.Guild {
                Name = name,
                LeaderId = leaderId,
                HouseRank = 1,
                HouseTheme = 1,
                Ranks = new Model.GuildRank[6] {
                    new() {Name = "Master", Permission = GuildPermission.All},
                    new() {Name = "Jr. Master", Permission = GuildPermission.Default},
                    new() {Name = "Member 1", Permission = GuildPermission.Default},
                    new() {Name = "Member 2", Permission = GuildPermission.Default},
                    new() {Name = "New Member 1", Permission = GuildPermission.Default},
                    new() {Name = "New Member 2", Permission = GuildPermission.Default},
                },
                Buffs = new Model.GuildBuff[9] {
                    new() {Id = 1, Level = 1},
                    new() {Id = 2, Level = 1},
                    new() {Id = 3, Level = 1},
                    new() {Id = 4, Level = 1},
                    new() {Id = 10001, Level = 1},
                    new() {Id = 10002, Level = 1},
                    new() {Id = 10003, Level = 1},
                    new() {Id = 10004, Level = 1},
                    new() {Id = 10005, Level = 1},
                },
                Posters = Array.Empty<Model.GuildPoster>(),
                Npcs = Array.Empty<Model.GuildNpc>(),
            };
            Context.Guild.Add(guild);
            if (!SaveChanges()) {
                return null;
            }

            var guildLeader = new Model.GuildMember {
                GuildId = guild.Id,
                CharacterId = leaderId,
                Rank = 0,
                LoginTime = DateTime.Now,
            };
            Context.GuildMember.Add(guildLeader);
            if (!SaveChanges()) {
                return null;
            }

            return Commit() ? LoadGuild(guild.Id, string.Empty) : null;
        }

        public GuildMember? CreateGuildMember(long guildId, PlayerInfo info) {
            var member = new Model.GuildMember {
                GuildId = guildId,
                CharacterId = info.CharacterId,
                Rank = 5,
                LoginTime = DateTime.Now,
            };
            Context.GuildMember.Add(member);
            if (!SaveChanges()) {
                return null;
            }

            return new GuildMember {
                GuildId = member.GuildId,
                Info = info,
                Rank = member.Rank,
                JoinTime = member.CreationTime.ToEpochSeconds(),
                LoginTime = member.LoginTime.ToEpochSeconds(),
            };
        }

        public bool SaveGuild(Guild guild) {
            // Don't save guild if it was disbanded.
            if (!Context.Guild.Any(model => model.Id == guild.Id)) {
                return false;
            }

            BeginTransaction();

            Context.Guild.Update(guild);
            SaveGuildMembers(guild.Id, guild.Members.Values);

            return Commit();
        }

        public bool DeleteGuild(long guildId) {
            BeginTransaction();

            int count = Context.Guild.Where(guild => guild.Id == guildId).Delete();
            if (count == 0) {
                return false;
            }

            Context.GuildMember.Where(member => member.GuildId == guildId).Delete();
            Context.GuildApplication.Where(app => app.GuildId == guildId).Delete();

            return Commit();
        }

        public bool DeleteGuildMember(long guildId, long characterId) {
            int count = Context.GuildMember.Where(member => member.GuildId == guildId && member.CharacterId == characterId).Delete();
            return SaveChanges() && count > 0;
        }

        public bool DeleteGuildApplication(long applicationId) {
            int count = Context.GuildApplication.Where(app => app.Id == applicationId).Delete();
            return SaveChanges() && count > 0;
        }

        public bool DeleteGuildApplications(long characterId) {
            int count = Context.GuildApplication.Where(app => app.ApplicantId == characterId).Delete();
            return SaveChanges() && count > 0;
        }

        public bool SaveGuildMembers(long guildId, ICollection<GuildMember> members) {
            Dictionary<long, GuildMember> saveMembers = members
                .ToDictionary(member => member.CharacterId, member => member);
            IEnumerable<Model.GuildMember> existingMembers = Context.GuildMember
                .Where(member => member.GuildId == guildId)
                .Select(member => new Model.GuildMember {
                    CharacterId = member.CharacterId,
                });

            foreach (Model.GuildMember member in existingMembers) {
                if (saveMembers.Remove(member.CharacterId, out GuildMember? gameMember)) {
                    Context.GuildMember.Update(gameMember);
                } else {
                    Context.GuildMember.Remove(member);
                }
            }
            Context.GuildMember.AddRange(saveMembers.Values.Select<GuildMember, Model.GuildMember>(member => member));

            return SaveChanges();
        }

        public bool SaveGuildMember(GuildMember member) {
            Model.GuildMember? model = Context.GuildMember.Find(member.GuildId, member.CharacterId);
            if (model == null) {
                return false;
            }

            Context.GuildMember.Update(member);
            return SaveChanges();
        }

        // Note: GuildMembers must be loaded separately.
        private Guild? LoadGuild(long guildId, string guildName) {
            IQueryable<Model.Guild> query = guildId > 0
                ? Context.Guild.Where(guild => guild.Id == guildId)
                : Context.Guild.Where(guild => guild.Name == guildName);
            return query
                .Join(Context.Character, guild => guild.LeaderId, character => character.Id,
                    (guild, character) => new Tuple<Model.Guild, Model.Character>(guild, character))
                .AsEnumerable()
                .Select(entry => {
                    Model.Guild guild = entry.Item1;
                    Character character = entry.Item2;
                    return new Guild(guild.Id, guild.Name, character.AccountId, character.Id, character.Name) {
                        Emblem = guild.Emblem,
                        Notice = guild.Notice,
                        CreationTime = guild.CreationTime.ToEpochSeconds(),
                        Focus = guild.Focus,
                        Experience = guild.Experience,
                        Funds = guild.Funds,
                        HouseRank = guild.HouseRank,
                        HouseTheme = guild.HouseTheme,
                        Ranks = guild.Ranks.Select((rank, i) => new GuildRank {
                            Id = (byte) i,
                            Name = rank.Name,
                            Permission = rank.Permission,
                        }).ToArray(),
                        Buffs = guild.Buffs.Select(skill => new GuildBuff {
                            Id = skill.Id,
                            Level = skill.Level,
                            ExpiryTime = skill.ExpiryTime,
                        }).ToList(),
                        Posters = guild.Posters.Select(poster => new GuildPoster {
                            Id = poster.Id,
                            Picture = poster.Picture,
                            OwnerId = poster.OwnerId,
                            OwnerName = poster.OwnerName,
                        }).ToList(),
                        Npcs = guild.Npcs.Select(npc => new GuildNpc {
                            Type = npc.Type,
                            Level = npc.Level,
                        }).ToList(),
                    };
                })
                .FirstOrDefault();
        }
    }
}
