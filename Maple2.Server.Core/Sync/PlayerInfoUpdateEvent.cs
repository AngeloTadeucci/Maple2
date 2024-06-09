using System;
using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Server.Core.Sync;

[Flags]
public enum UpdateField {
    None = 0,
    Profile = 1,
    Job = 2,
    Level = 4,
    GearScore = 8,
    Health = 16,
    Map = 32,
    Channel = 64,
    Home = 128,
    Trophy = 256,
    PremiumTime = 512,
    Clubs = 1024,

    // Presets
    Buddy = Profile | Job | Level | Map | Channel | Home | Trophy,
    Guild = Profile | Job | Level | GearScore | Map | Channel | Home | Trophy | Clubs,
    Club = Profile | Job | Level | GearScore | Map | Channel | Home | Trophy | Clubs,
    Party = Profile | Job | Level | GearScore | Health | Map | Channel | Home | Clubs,
    GroupChat = Profile | Job | Level | Map | Channel | Home | Clubs,
    All = int.MaxValue,
}

public class PlayerInfoUpdateEvent {
    public readonly UpdateField Type;
    public readonly PlayerUpdateRequest Request;

    public PlayerInfoUpdateEvent(PlayerUpdateRequest request) {
        Type = UpdateField.All;
        Request = request;
    }

    public PlayerInfoUpdateEvent(IPlayerInfo player, PlayerUpdateRequest request) {
        Request = request;
        if (request.HasName && player.Name != request.Name) {
            Type |= UpdateField.Profile;
        } else if (request.HasPicture && player.Picture != request.Picture) {
            Type |= UpdateField.Profile;
        } else if (request.HasMotto && player.Motto != request.Motto) {
            Type |= UpdateField.Profile;
        } else if (request.HasGender && player.Gender != (Gender) request.Gender) {
            Type |= UpdateField.Profile;
        }
        if (request.HasJob && player.Job != (Job) request.Job) {
            Type |= UpdateField.Job;
        }
        if (request.HasLevel && player.Level != request.Level) {
            Type |= UpdateField.Level;
        }
        if (request.HasGearScore && player.GearScore != request.GearScore) {
            Type |= UpdateField.GearScore;
        }
        if (request.HasMapId && player.MapId != request.MapId) {
            Type |= UpdateField.Map;
        }
        if ((request.HasChannel && player.Channel != request.Channel) ||
            (request.HasLastOnlineTime && player.LastOnlineTime != request.LastOnlineTime)) {
            Type |= UpdateField.Channel;
        }
        if (request.Health != null) {
            if (player.CurrentHp != request.Health.CurrentHp) {
                Type |= UpdateField.Health;
            } else if (player.TotalHp != request.Health.TotalHp) {
                Type |= UpdateField.Health;
            }
        }
        if (request.Home != null) {
            if (player.PlotMapId != request.Home.MapId) {
                Type |= UpdateField.Home;
            } else if (player.PlotNumber != request.Home.PlotNumber) {
                Type |= UpdateField.Home;
            } else if (player.ApartmentNumber != request.Home.ApartmentNumber) {
                Type |= UpdateField.Home;
            } else if (player.PlotExpiryTime != request.Home.ExpiryTime.Seconds) {
                Type |= UpdateField.Home;
            } else if (player.HomeName != request.Home.Name) {
                Type |= UpdateField.Home;
            }
        }
        if (request.Trophy != null) {
            if (player.AchievementInfo.Combat != request.Trophy.Combat) {
                Type |= UpdateField.Trophy;
            } else if (player.AchievementInfo.Adventure != request.Trophy.Adventure) {
                Type |= UpdateField.Trophy;
            } else if (player.AchievementInfo.Lifestyle != request.Trophy.Lifestyle) {
                Type |= UpdateField.Trophy;
            }
        }

        if (request.HasPremiumTime && player.PremiumTime != request.PremiumTime) {
            if (player.PremiumTime != request.PremiumTime) {
                Type |= UpdateField.PremiumTime;
            }
        }

        if (request.Clubs != null) {
            if (player.ClubIds != request.Clubs.Select(club => club.Id).ToList()) {
                Type |= UpdateField.Clubs;
            }
        }
    }
}
