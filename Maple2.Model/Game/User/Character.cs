using System;
using System.Collections.Generic;
using System.Numerics;
using Maple2.Model.Common;
using Maple2.Model.Enum;

namespace Maple2.Model.Game;

public class Character {
    #region Immutable
    public long CreationTime { get; init; }
    public DateTime LastModified { get; init; }

    public long Id { get; init; }
    public long AccountId { get; init; }
    #endregion

    public long DeleteTime;

    public required string Name;
    public Gender Gender;
    public int MapId;
    public Job Job;

    public SkinColor SkinColor;
    public short Level = 1;
    public long Exp;
    public long RestExp;

    public int Title;
    public short Insignia;

    public int InstanceId;
    public int InstanceMapId;
    public short Channel;

    public long StorageCooldown;
    public long DoctorCooldown;

    public int ReviveMapId;
    public int ReturnMapId;
    public Vector3 ReturnPosition;
    public string Picture = string.Empty;
    public string Motto = string.Empty;
    public string GuildName = string.Empty;
    public long GuildId;
    public int PartyId;
    public Dictionary<long, string> Clubs = []; // ClubId, ClubName
    public required Mastery Mastery;
    public AchievementInfo AchievementInfo;
}
