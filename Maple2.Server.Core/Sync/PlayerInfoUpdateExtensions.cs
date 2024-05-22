using System;
using Google.Protobuf.WellKnownTypes;
using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Server.Core.Sync;

public static class PlayerInfoUpdateExtensions {
    public static void Update(this IPlayerInfo info, PlayerInfoUpdateEvent update) {
        if (update.Type.HasFlag(UpdateField.Profile)) {
            if (update.Request.HasName) {
                info.Name = update.Request.Name;
            }
            if (update.Request.HasMotto) {
                info.Motto = update.Request.Motto;
            }
            if (update.Request.HasPicture) {
                info.Picture = update.Request.Picture;
            }
        }
        if (update.Type.HasFlag(UpdateField.PremiumTime) && update.Request.HasPremiumTime) {
            info.PremiumTime = update.Request.PremiumTime;
        }
        if (update.Type.HasFlag(UpdateField.Job) && update.Request.HasJob) {
            info.Job = (Job) update.Request.Job;
        }
        if (update.Type.HasFlag(UpdateField.Level)) {
            if (update.Request.HasLevel) {
                info.Level = (short) update.Request.Level;
            }
        }
        if (update.Type.HasFlag(UpdateField.GearScore) && update.Request.HasGearScore) {
            info.GearScore = update.Request.GearScore;
        }
        if (update.Type.HasFlag(UpdateField.Map) && update.Request.HasMapId) {
            info.MapId = update.Request.MapId;
        }
        if (update.Type.HasFlag(UpdateField.Channel) && update.Request.HasChannel) {
            info.Channel = (short) update.Request.Channel;
        }
        if (update.Type.HasFlag(UpdateField.Health) && update.Request.Health != null) {
            info.CurrentHp = update.Request.Health.CurrentHp;
            info.TotalHp = update.Request.Health.TotalHp;
        }
        if (update.Type.HasFlag(UpdateField.Home) && update.Request.Home != null) {
            info.HomeName = update.Request.Home.Name;
            info.PlotMapId = update.Request.Home.MapId;
            info.PlotNumber = update.Request.Home.PlotNumber;
            info.ApartmentNumber = update.Request.Home.ApartmentNumber;
            info.PlotExpiryTime = update.Request.Home.ExpiryTime.Seconds;
        }
        if (update.Type.HasFlag(UpdateField.Trophy) && update.Request.Trophy != null) {
            info.AchievementInfo = new Model.Game.AchievementInfo {
                Combat = update.Request.Trophy.Combat,
                Adventure = update.Request.Trophy.Adventure,
                Lifestyle = update.Request.Trophy.Lifestyle,
            };
        }

        info.UpdateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public static void Update(this IPlayerInfo self, UpdateField type, IPlayerInfo other) {
        if (type.HasFlag(UpdateField.Profile)) {
            self.Name = other.Name;
            self.Motto = other.Motto;
            self.Picture = other.Picture;
        }
        if (type.HasFlag(UpdateField.Job)) {
            self.Job = other.Job;
        }
        if (type.HasFlag(UpdateField.Level)) {
            self.Level = other.Level;
        }
        if (type.HasFlag(UpdateField.GearScore)) {
            self.GearScore = other.GearScore;
        }
        if (type.HasFlag(UpdateField.Map)) {
            self.MapId = other.MapId;
        }
        if (type.HasFlag(UpdateField.Channel)) {
            self.Channel = other.Channel;
        }
        if (type.HasFlag(UpdateField.Health)) {
            self.CurrentHp = other.CurrentHp;
            self.TotalHp = other.TotalHp;
        }
        if (type.HasFlag(UpdateField.Home)) {
            self.HomeName = other.HomeName;
            self.MapId = other.PlotMapId;
            self.PlotNumber = other.PlotNumber;
            self.ApartmentNumber = other.ApartmentNumber;
            self.PlotExpiryTime = other.PlotExpiryTime;
        }
        if (type.HasFlag(UpdateField.Trophy)) {
            self.AchievementInfo = other.AchievementInfo;
        }
        if (type.HasFlag(UpdateField.PremiumTime)) {
            self.PremiumTime = other.PremiumTime;
        }

        self.UpdateTime = other.UpdateTime;
    }

    public static void SetFields(this PlayerUpdateRequest request, UpdateField type, PlayerInfo info) {
        if (type.HasFlag(UpdateField.Profile)) {
            request.Name = info.Name;
            request.Motto = info.Motto;
            request.Picture = info.Picture;
        }
        if (type.HasFlag(UpdateField.Job)) {
            request.Job = (int) info.Job;
        }
        if (type.HasFlag(UpdateField.Level)) {
            request.Level = info.Level;
        }
        if (type.HasFlag(UpdateField.GearScore)) {
            request.GearScore = info.GearScore;
        }
        if (type.HasFlag(UpdateField.Map)) {
            request.MapId = info.MapId;
        }
        if (type.HasFlag(UpdateField.Channel)) {
            request.Channel = info.Channel;
        }
        if (type.HasFlag(UpdateField.Health)) {
            request.Health = new HealthInfo {
                CurrentHp = info.CurrentHp,
                TotalHp = info.TotalHp,
            };
        }
        if (type.HasFlag(UpdateField.Home)) {
            request.Home = new HomeInfo {
                Name = info.HomeName,
                MapId = info.PlotMapId,
                PlotNumber = info.PlotNumber,
                ApartmentNumber = info.ApartmentNumber,
                ExpiryTime = new Timestamp { Seconds = info.PlotExpiryTime },
            };
        }
        if (type.HasFlag(UpdateField.Trophy)) {
            request.Trophy = new TrophyInfo {
                Combat = info.AchievementInfo.Combat,
                Adventure = info.AchievementInfo.Adventure,
                Lifestyle = info.AchievementInfo.Lifestyle,
            };
        }
        if (type.HasFlag(UpdateField.PremiumTime)) {
            request.PremiumTime = info.PremiumTime;
        }
    }
}
