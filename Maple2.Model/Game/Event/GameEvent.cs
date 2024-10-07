using System;
using System.Linq;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Event;

public class GameEvent : IByteSerializable {
    public GameEventMetadata Metadata { get; init; }
    public int Id => Metadata.Id;
    public string Name => Metadata.Type.ToString();
    public long StartTime => (long) (Metadata.StartTime.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds;
    public long EndTime => (long) (Metadata.EndTime.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds;

    public GameEvent(GameEventMetadata metadata) {
        Metadata = metadata;
    }

    public bool IsActive() {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (Metadata.StartTime > now) {
            return false;
        }

        if (Metadata.EndTime < now) {
            return false;
        }

        if (Metadata.ActiveDays.Length > 0 && !Metadata.ActiveDays.Contains(now.DayOfWeek)) {
            return false;
        }

        if (Metadata.StartPartTime != TimeSpan.Zero && Metadata.StartPartTime > now.TimeOfDay) {
            return false;
        }

        if (Metadata.EndPartTime != TimeSpan.Zero && Metadata.EndPartTime < now.TimeOfDay) {
            return false;
        }

        return true;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteUnicodeString(Name);
        switch (Metadata.Data) {
            case StringBoard stringBoard:
                writer.WriteInt(Id);
                writer.WriteInt(stringBoard.StringId);
                writer.WriteUnicodeString(stringBoard.Text);
                break;
            case StringBoardLink stringBoardLink:
                writer.WriteInt(Id);
                writer.WriteUnicodeString(stringBoardLink.Link);
                break;
            case SaleChat saleChat:
                writer.WriteInt(Id);
                writer.WriteInt(saleChat.WorldChatDiscount);
                writer.WriteInt(saleChat.ChannelChatDiscount);
                break;
            case EventFieldPopup eventFieldPopup:
                writer.WriteInt(Id);
                writer.WriteInt(eventFieldPopup.MapId);
                break;
            case TrafficOptimizer trafficOptimizer:
                writer.WriteInt(Id);
                writer.WriteInt(trafficOptimizer.GuideObjectSyncInterval);
                writer.WriteInt(trafficOptimizer.RideSyncInterval);
                writer.WriteInt(100);
                writer.WriteInt();
                writer.WriteInt(trafficOptimizer.LinearMovementInterval);
                writer.WriteInt(trafficOptimizer.UserSyncInterval);
                writer.WriteInt(100);
                break;
            case BlueMarble blueMarble:
                writer.WriteInt(Id);
                writer.WriteInt(blueMarble.Rounds.Length);
                foreach (BlueMarble.Round round in blueMarble.Rounds) {
                    writer.WriteInt(round.RoundCount);
                    writer.WriteInt(round.Item.ItemId);
                    writer.WriteByte((byte) round.Item.Rarity);
                    writer.WriteInt(round.Item.Amount);
                }
                break;
            case AttendGift attendGift:
                writer.WriteInt(Id);
                writer.WriteLong(StartTime);
                writer.WriteLong(EndTime);
                writer.WriteUnicodeString(attendGift.Name);
                writer.WriteString(attendGift.Link);
                writer.WriteBool(true); // disable claim button
                writer.WriteInt(attendGift.RequiredPlaySeconds);
                writer.WriteByte();
                writer.WriteInt();
                var currencyType = AttendGiftCurrencyType.None;
                writer.Write<AttendGiftCurrencyType>(currencyType);

                if (currencyType != AttendGiftCurrencyType.None) {
                    writer.WriteInt(); // Skip Days Allowed
                    writer.WriteLong(); // Skip Day Cost
                    writer.WriteInt();
                }
                break;
            case MeretMarketNotice notice:
                writer.WriteUnicodeString(notice.Text);
                break;
            case Rps rps:
                writer.WriteInt(Id);
                writer.WriteUnicodeString(rps.ActionsHtml);
                writer.WriteInt(rps.Rewards.Length);
                foreach (Rps.RewardData reward in rps.Rewards) {
                    writer.WriteInt(reward.PlayCount);
                    foreach (RewardItem item in reward.Rewards) {
                        writer.Write<RewardItem>(item);
                    }
                }
                writer.WriteInt(rps.GameTicketId);
                writer.WriteInt(Id);
                writer.WriteLong(EndTime);
                break;
            case LobbyMap lobbyMap:
                writer.WriteInt(Id);
                writer.WriteInt(lobbyMap.MapId);
                break;
            case ReturnUser returnUser:
                writer.WriteInt(Id);
                writer.WriteInt(); // season?
                writer.WriteLong(StartTime);
                writer.WriteLong(EndTime);
                writer.WriteInt(returnUser.QuestIds.Length);
                foreach (int questId in returnUser.QuestIds) {
                    writer.WriteInt(questId);
                }
                break;
            case LoginNotice:
                break;
            case FieldEffect fieldEffect:
                writer.WriteInt(Id);
                writer.WriteByte((byte) fieldEffect.MapIds.Length);
                foreach (int mapId in fieldEffect.MapIds) {
                    writer.WriteInt(mapId);
                }
                break;
            case QuestTag questTag:
                writer.WriteInt(Id);
                writer.WriteUnicodeString(questTag.Tag);
                writer.WriteLong(StartTime);
                writer.WriteLong(EndTime);
                break;
            case DTReward dtReward:
                writer.WriteInt(Id);
                writer.WriteUnicodeString(Metadata.Value1);
                writer.WriteUnicodeString(Metadata.Value2);
                writer.WriteUnicodeString(Metadata.Value3);
                writer.WriteUnicodeString(Metadata.Value4);
                break;
            case ConstructShowItem constructShowItem:
                writer.WriteInt(Id);
                writer.WriteInt(constructShowItem.CategoryId);
                writer.WriteUnicodeString(constructShowItem.CategoryName);
                writer.WriteShort(); // likely value3 but unknown what it means
                writer.WriteShort((short) constructShowItem.ItemIds.Length);
                foreach (int itemId in constructShowItem.ItemIds) {
                    writer.WriteInt(itemId);
                }
                break;
            case MassiveConstructionEvent massiveConstructionEvent:
                writer.WriteInt(Id);
                writer.WriteInt(massiveConstructionEvent.MapIds.Length);
                foreach (int mapId in massiveConstructionEvent.MapIds) {
                    writer.WriteInt(mapId);
                }
                writer.WriteInt(); // this is a certain state. 1 = active, 0 = inactive ?
                writer.WriteInt(); // account ids enabled to build in the specified maps
                /* foreach (int accountId in accountIds) {
                    writer.WriteLong(accountId);
                } */
                break;
            case UGCMapContractSale ugcMapContractSale:
                writer.WriteInt(Id);
                writer.WriteInt(ugcMapContractSale.DiscountAmount);
                break;
            case UGCMapExtensionSale ugcMapExtensionSale:
                writer.WriteInt(Id);
                writer.WriteInt(ugcMapExtensionSale.DiscountAmount);
                break;
        }
    }
}
