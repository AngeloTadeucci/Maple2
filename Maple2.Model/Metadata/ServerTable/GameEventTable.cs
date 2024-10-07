using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Maple2.Model.Enum;
using Maple2.Model.Game;
// ReSharper disable InconsistentNaming

namespace Maple2.Model.Metadata;

public record GameEventTable(IReadOnlyDictionary<int, GameEventMetadata> Entries) : ServerTable;

public record GameEventMetadata(
    int Id,
    GameEventType Type,
    DateTime StartTime,
    DateTime EndTime,
    TimeSpan StartPartTime,
    TimeSpan EndPartTime,
    DayOfWeek[] ActiveDays,
    GameEventData Data,
    string Value1,
    string Value2,
    string Value3,
    string Value4
);

public record StringBoard(
    string Text,
    int StringId) : GameEventData;

public record StringBoardLink(
    string Link) : GameEventData;

public record MeretMarketNotice(
    string Text) : GameEventData;

public record TrafficOptimizer(
    int RideSyncInterval,
    int UserSyncInterval,
    int LinearMovementInterval,
    int GuideObjectSyncInterval) : GameEventData;

public record LobbyMap(
    int MapId) : GameEventData;

public record EventFieldPopup(
    int MapId) : GameEventData;

public record Rps(
    int GameTicketId,
    Rps.RewardData[] Rewards,
    string ActionsHtml) : GameEventData {

    public record RewardData(
        int PlayCount,
        RewardItem[] Rewards);
}

public record SaleChat(
    int WorldChatDiscount,
    int ChannelChatDiscount) : GameEventData;

public record AttendGift(
    RewardItem[] Items,
    string Name,
    string MailTitle,
    string MailContent,
    string Link,
    int RequiredPlaySeconds,
    AttendGift.Require? Requirement) : GameEventData {

    public record Require(
        AttendGiftRequirement Type,
        int Value1,
        int Value2); // Value2 is used for ItemID type. this is duration in days ?
};

public record BlueMarble(
    ItemComponent RequiredItem,
    BlueMarble.Round[] Rounds,
    BlueMarble.Slot[] Slots) : GameEventData {

    public record Round(
        int RoundCount,
        ItemComponent Item);

    public record Slot(
        BlueMarbleSlotType Type,
        int MoveAmount,
        ItemComponent Item);
}

public record ReturnUser(
    int Season,
    int[] QuestIds,
    int RequiredUserValue,
    DateTimeOffset RequiredTime,
    int RequiredLevel) : GameEventData;

public record FieldEffect(
    int[] MapIds,
    string Effect) : GameEventData;

/// <summary>
/// Enables quests with eventTag to be enabled/active.
/// </summary>
public record QuestTag(
    string Tag) : GameEventData;

/// <summary>
/// Attendance-like event. Gives rewards for the specified duration a user is logged in.
/// </summary>
public record DTReward(
    DTReward.Entry[] Entries
) : GameEventData {
    public record Entry(
        int StartDuration,
        int EndDuration,
        int MailContentId,
        RewardItem Item);
}

/// <summary>
/// Enables the specified items to be shown/bought in the building menu
/// </summary>
public record ConstructShowItem(
    int CategoryId,
    string CategoryName,
    int[] ItemIds) : GameEventData;

/// <summary>
/// Enables building on the specified maps. Still need to add player account ID to the list of participants. Check packet.
/// </summary>
public record MassiveConstructionEvent(
    int[] MapIds) : GameEventData;

/// <summary>
/// Discount amount for buying new UGC plots.
/// </summary>
/// <param name="DiscountAmount">Discount amount in a whole int. ex. 9000 = 90% discount.</param>
public record UGCMapContractSale(
    int DiscountAmount) : GameEventData;

/// <summary>
/// Discount amount for extending UGC plots.
/// </summary>
/// <param name="DiscountAmount">Discount amount in a whole int. ex. 9000 = 90% discount.</param>
public record UGCMapExtensionSale(
    int DiscountAmount) : GameEventData;

public record LoginNotice : GameEventData;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "!")]
[JsonDerivedType(typeof(StringBoard), "StringBoard")]
[JsonDerivedType(typeof(StringBoardLink), "StringBoardLink")]
[JsonDerivedType(typeof(MeretMarketNotice), "MeretMarketNotice")]
[JsonDerivedType(typeof(TrafficOptimizer), "TrafficOptimizer")]
[JsonDerivedType(typeof(LobbyMap), "LobbyMap")]
[JsonDerivedType(typeof(EventFieldPopup), "EventFieldPopup")]
[JsonDerivedType(typeof(Rps), "Rps")]
[JsonDerivedType(typeof(SaleChat), "SaleChat")]
[JsonDerivedType(typeof(AttendGift), "AttendGift")]
[JsonDerivedType(typeof(BlueMarble), "BlueMarble")]
[JsonDerivedType(typeof(ReturnUser), "ReturnUser")]
[JsonDerivedType(typeof(LoginNotice), "LoginNotice")]
[JsonDerivedType(typeof(FieldEffect), "FieldEffect")]
[JsonDerivedType(typeof(QuestTag), "QuestTag")]
[JsonDerivedType(typeof(DTReward), "DTReward")]
[JsonDerivedType(typeof(ConstructShowItem), "ConstructShowItem")]
[JsonDerivedType(typeof(MassiveConstructionEvent), "MassiveConstructionEvent")]
[JsonDerivedType(typeof(UGCMapContractSale), "UGCMapContractSale")]
[JsonDerivedType(typeof(UGCMapExtensionSale), "UGCMapExtensionSale")]
public abstract record GameEventData;
