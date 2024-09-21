﻿using System.Diagnostics.CodeAnalysis;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class HomeLayout {
    public long Uid { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public byte Area { get; set; }
    public byte Height { get; set; }
    public HomeBackground Background { get; set; }
    public HomeLighting Lighting { get; set; }
    public HomeCamera Camera { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public List<HomeLayoutCube> Cubes { get; set; } = null!;

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator HomeLayout?(Maple2.Model.Game.HomeLayout? other) {
        return other == null ? null : new HomeLayout {
            Uid = other.Uid,
            Id = other.Id,
            Name = other.Name,
            Area = other.Area,
            Height = other.Height,
            Timestamp = other.Timestamp,
            Cubes = other.Cubes.ConvertAll(cube => (HomeLayoutCube) cube),
            Background = other.Background,
            Lighting = other.Lighting,
            Camera = other.Camera,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.HomeLayout?(HomeLayout? other) {
        if (other == null) {
            return null;
        }

        return new Maple2.Model.Game.HomeLayout(other.Uid, other.Id, other.Name, other.Area, other.Height, other.Timestamp, other.Cubes.ConvertAll(cube => (PlotCube) cube)) {
            Background = other.Background,
            Lighting = other.Lighting,
            Camera = other.Camera,
        };
    }

    public static void Configure(EntityTypeBuilder<HomeLayout> builder) {
        builder.ToTable("home-layout");
        builder.HasKey(layout => layout.Uid);
    }
}
