using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Marriage = Maple2.Model.Game.Marriage;
using MarriageExp = Maple2.Model.Game.MarriageExp;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public Marriage? CreateMarriage(long partner1Id, long partner2Id) {
            BeginTransaction();

            Model.Marriage marriage = new Model.Marriage {
                Partner1Id = partner1Id,
                Partner2Id = partner2Id,
                Status = MaritalStatus.Engaged,
                Profile = string.Empty,
                Partner1Message = string.Empty,
                Partner2Message = string.Empty,
                CreationTime = DateTime.Now,
            };
            Context.Marriage.Add(marriage);
            if (!SaveChanges()) {
                return null;
            }

            return Commit() ? GetMarriage(marriage.Id) : null;
        }

        private Marriage? GetMarriage(long marriageId) {
            Model.Marriage? marriage = Context.Marriage.Find(marriageId);
            if (marriage == null) {
                return null;
            }

            PlayerInfo? partner1 = GetPlayerInfo(marriage.Partner1Id);
            PlayerInfo? partner2 = GetPlayerInfo(marriage.Partner2Id);
            if (partner1 == null || partner2 == null) {
                return null;
            }
            return new Marriage {
                Id = marriage.Id,
                CreationTime = marriage.CreationTime.ToEpochSeconds(),
                ExpHistory = marriage.ExpHistory.Select<Model.MarriageExp, MarriageExp>(exp => exp).ToList(),
                Partner1 = new MarriagePartner {
                    Info = partner1,
                    Message = marriage.Partner1Message,
                },
                Partner2 = new MarriagePartner {
                    Info = partner2,
                    Message = marriage.Partner2Message,
                },
                Status = marriage.Status,
            };
        }


        public MarriageInfo GetMarriageInfo(long characterId) {
            Model.Marriage? model = Context.Marriage.FirstOrDefault(member => member.Partner1Id == characterId || member.Partner2Id == characterId);
            if (model == null) {
                return new MarriageInfo();
            }

            PlayerInfo? owner = GetPlayerInfo(model.Partner1Id);
            PlayerInfo? partner = GetPlayerInfo(model.Partner2Id);

            return owner == null || partner == null ? new MarriageInfo() : new MarriageInfo {
                Status = model.Status,
                CreationTime = model.CreationTime.ToEpochSeconds(),
                Partner1Name = owner.Name,
                Partner2Name = partner.Name,
            };
        }
    }
}
