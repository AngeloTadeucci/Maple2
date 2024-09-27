﻿using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class HomeLayoutCube {
    public long Id { get; set; }
    public long HomeLayoutId { get; set; }
    public sbyte X { get; set; }
    public sbyte Y { get; set; }
    public sbyte Z { get; set; }
    public float Rotation { get; set; }

    public int ItemId { get; set; }
    public string InteractId { get; set; } = "";
    public HousingCategory HousingCategory { get; set; }
    public CubePortalSettings? PortalSettings { get; set; }
    public UgcItemLook? Template { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator PlotCube?(HomeLayoutCube? other) {
        return other == null ? null : new PlotCube(other.ItemId, other.Id, other.Template) {
            Position = new Vector3B(other.X, other.Y, other.Z),
            Rotation = other.Rotation,
            InteractId = other.InteractId,
            HousingCategory = other.HousingCategory,
            InteractState = other.HousingCategory is HousingCategory.Ranching or HousingCategory.Farming
                ? InteractCubeState.InUse
                : InteractCubeState.None,
            PortalSettings = other.PortalSettings,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator HomeLayoutCube?(PlotCube? other) {
        return other == null ? null : new HomeLayoutCube {
            X = other.Position.X,
            Y = other.Position.Y,
            Z = other.Position.Z,
            Rotation = other.Rotation,
            ItemId = other.ItemId,
            Template = other.Template,
            InteractId = other.InteractId,
            HousingCategory = other.HousingCategory,
            PortalSettings = other.PortalSettings,
        };
    }

    public static void Configure(EntityTypeBuilder<HomeLayoutCube> builder) {
        builder.ToTable("home-layout-cube");
        builder.HasKey(cube => cube.Id);

        builder.HasOne<HomeLayout>()
            .WithMany(ugcMap => ugcMap.Cubes)
            .HasForeignKey(cube => cube.HomeLayoutId);

        builder.Property(cube => cube.Template).HasJsonConversion();
        builder.Property(cube => cube.PortalSettings).HasJsonConversion();
    }
}
