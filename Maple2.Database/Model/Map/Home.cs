﻿using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class Home {
    public long AccountId { get; set; }

    public string Message { get; set; } = string.Empty;
    public byte Area { get; set; }
    public byte Height { get; set; }

    public int CurrentArchitectScore { get; set; }
    public int ArchitectScore { get; set; }

    public long DecorationLevel { get; set; }
    public long DecorationExp { get; set; }
    public long DecorationRewardTimestamp { get; set; }
    public List<int> InteriorRewardsClaimed { get; set; } = [];

    // Interior Settings
    public HomeBackground Background { get; set; }
    public HomeLighting Lighting { get; set; }
    public HomeCamera Camera { get; set; }

    public string? Passcode { get; set; }
    public IDictionary<HomePermission, HomePermissionSetting> Permissions { get; set; } = new Dictionary<HomePermission, HomePermissionSetting>();
    public List<long> Layouts { get; set; } = [];
    public List<long> Blueprints { get; set; } = [];

    public DateTime LastModified { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Home?(Maple2.Model.Game.Home? other) {
        return other == null ? null : new Home {
            AccountId = other.AccountId,

            Message = other.Message,
            Area = other.Area,
            Height = other.Height,
            CurrentArchitectScore = other.CurrentArchitectScore,
            ArchitectScore = other.ArchitectScore,
            Background = other.Background,
            Lighting = other.Lighting,
            Camera = other.Camera,
            Passcode = other.Passcode,
            Permissions = other.Permissions,
            Layouts = other.Layouts.Select(layout => layout.Uid).ToList(),
            Blueprints = other.Blueprints.Select(layout => layout.Uid).ToList(),
            DecorationLevel = other.DecorationLevel,
            DecorationExp = other.DecorationExp,
            DecorationRewardTimestamp = other.DecorationRewardTimestamp,
            InteriorRewardsClaimed = other.InteriorRewardsClaimed,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.Home?(Home? other) {
        if (other == null) {
            return null;
        }

        var home = new Maple2.Model.Game.Home {
            AccountId = other.AccountId,
            Message = other.Message,
            CurrentArchitectScore = other.CurrentArchitectScore,
            ArchitectScore = other.ArchitectScore,
            Passcode = other.Passcode,
            LastModified = other.LastModified.ToEpochSeconds(),
            DecorationLevel = other.DecorationLevel,
            DecorationExp = other.DecorationExp,
            DecorationRewardTimestamp = other.DecorationRewardTimestamp,
            InteriorRewardsClaimed = other.InteriorRewardsClaimed,
        };

        home.SetArea(other.Area);
        home.SetHeight(other.Height);
        home.SetBackground(other.Background);
        home.SetLighting(other.Lighting);
        home.SetCamera(other.Camera);
        foreach ((HomePermission permission, HomePermissionSetting setting) in other.Permissions) {
            home.Permissions[permission] = setting;
        }

        return home;
    }

    public static void Configure(EntityTypeBuilder<Home> builder) {
        builder.ToTable("home");
        builder.HasKey(home => home.AccountId);
        builder.OneToOne<Home, Account>()
            .HasForeignKey<Home>(home => home.AccountId);

        builder.Property(home => home.Area)
            .HasDefaultValue(Constant.MinHomeArea);
        builder.Property(home => home.Height)
            .HasDefaultValue(Constant.MinHomeHeight);

        builder.Property(home => home.Permissions).HasJsonConversion();
        builder.Property(home => home.Layouts).HasJsonConversion();
        builder.Property(home => home.Blueprints).HasJsonConversion();
        builder.Property(home => home.InteriorRewardsClaimed).HasJsonConversion();

        builder.Property(map => map.LastModified)
            .ValueGeneratedOnAddOrUpdate();
    }
}
