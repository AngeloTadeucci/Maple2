using System;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game;

public sealed class ItemTransfer(TransferFlag flag = 0, int remainTrades = 0, int repackageCount = 0, ItemBinding? binding = null)
    : IByteSerializable, IByteDeserializable {
    public static readonly ItemTransfer Default = new ItemTransfer();

    public TransferFlag Flag { get; set; } = flag;
    public int RemainTrades { get; set; } = remainTrades;
    public int RepackageCount { get; set; } = repackageCount;

    public ItemBinding? Binding { get; private set; } = binding;

    public ItemTransfer Clone() {
        return new ItemTransfer(Flag, RemainTrades, RepackageCount, Binding?.Clone());
    }

    public bool Bind(Character character) {
        if (!Flag.HasFlag(TransferFlag.Bind)) {
            return false;
        }

        if (Binding != null) {
            return false;
        }

        Binding = new ItemBinding(character.Id, character.Name);
        RemainTrades = 0;
        return true;
    }

    public void WriteTo(IByteWriter writer) {
        // CItemTransfer is CItem[66]
        writer.Write<TransferFlag>(Flag); // CItemTransfer[5]
        writer.WriteBool(false); // CItemTransfer[9] *bit-1*
        writer.WriteInt(RemainTrades); // CItemTransfer[10]
        writer.WriteInt(RepackageCount); // CItemTransfer[11]
        writer.WriteByte(); // CItemTransfer[12]
        writer.WriteBool(true); // CItemTransfer[9] *bit-10* (socketTransfer?)

        // CharBound means untradable, unsellable, bound to char (ignores TransferFlag)
        writer.WriteBool(Binding != null);
        if (Binding != null) {
            writer.WriteClass<ItemBinding>(Binding);
        }
    }

    public void ReadFrom(IByteReader reader) {
        Flag = reader.Read<TransferFlag>();
        reader.ReadByte();
        RemainTrades = reader.ReadInt();
        RepackageCount = reader.ReadInt();
        reader.ReadByte();
        reader.ReadBool();
        bool isBound = reader.ReadBool();
        if (isBound) {
            Binding = reader.ReadClass<ItemBinding>();
        }
    }

    public override bool Equals(object? obj) {
        if (ReferenceEquals(this, obj)) return true;
        if (!(obj is ItemTransfer other)) return false;
        return Flag == other.Flag && Equals(Binding, other.Binding);
    }

    public override int GetHashCode() {
        return HashCode.Combine((int) Flag, Binding);
    }
}
