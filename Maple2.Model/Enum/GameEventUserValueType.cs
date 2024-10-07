﻿// ReSharper disable InconsistentNaming

namespace Maple2.Model.Enum;

public enum GameEventUserValueType {
    // Attendance Event
    AttendanceActive = 100, //?? maybe. String is "True"
    AttendanceCompletedTimestamp = 101,
    AttendanceRewardsClaimed = 102,
    AttendanceEarlyParticipationRemaining = 103,
    AttendanceAccumulatedTime = 106,

    // DTReward
    DTRewardStartTime = 700, // start time
    DTRewardCurrentTime = 701, // current item accumulated time
    DTRewardRewardIndex = 702, // unk value seen is "1"
    DTRewardTotalTime = 703, // TOTAL accumulated time

    // Blue Marble / Mapleopoly
    MapleopolyTotalSlotCount = 800,
    MapleopolyFreeRollAmount = 801,
    MapleopolyTotalTrips = 802, // unsure

    // Rock Paper Scissors Event
    RPSDailyMatches = 1800,
    RPSRewardsClaimed = 1801,
}
