using System.Collections.Generic;
using System.Runtime.InteropServices;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Event;

/// <summary>
/// Mapleopoly
/// </summary>
public class BlueMarble : GameEventInfo {
    public IList<BlueMarbleEntry> Entries { get; set; } = new List<BlueMarbleEntry>();
    public IList<BlueMarbleTile> Tiles { get; set; } = new List<BlueMarbleTile>();

    public override void WriteTo(IByteWriter writer) {
        writer.WriteInt(Id);
        writer.WriteInt(Entries.Count);
        foreach (BlueMarbleEntry entry in Entries) {
            writer.Write<BlueMarbleEntry>(entry);
        }
    }
}

public class BlueMarbleTile : IByteSerializable {
    public int Position { get; init; }
    public BlueMarbleTileType Type { get; init; }
    public int MoveAmount { get; init; }
    public BlueMarbleItem Item { get; init; }

    public void WriteTo(IByteWriter writer) {
        writer.Write<BlueMarbleTileType>(Type);
        writer.WriteInt(MoveAmount);
        writer.Write<BlueMarbleItem>(Item);
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 13)]
public readonly record struct BlueMarbleEntry(int TripAmount, BlueMarbleItem Item);

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 9)]
public readonly record struct BlueMarbleItem(int ItemId, byte ItemRarity, int ItemAmount);
