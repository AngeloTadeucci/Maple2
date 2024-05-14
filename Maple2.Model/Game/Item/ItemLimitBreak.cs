using System.Collections.Generic;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public sealed class ItemLimitBreak : IByteSerializable, IByteDeserializable {
    public static readonly ItemLimitBreak Default = new ItemLimitBreak();

    public int Level { get; private set; }
    public readonly IDictionary<BasicAttribute, BasicOption> BasicOptions;
    public readonly IDictionary<SpecialAttribute, SpecialOption> SpecialOptions;

    public ItemLimitBreak() {
        BasicOptions = new Dictionary<BasicAttribute, BasicOption>();
        SpecialOptions = new Dictionary<SpecialAttribute, SpecialOption>();
    }

    public ItemLimitBreak Clone() {
        return new ItemLimitBreak(Level, BasicOptions, SpecialOptions);
    }

    public ItemLimitBreak(int level, IDictionary<BasicAttribute, BasicOption> basicOptions,
                          IDictionary<SpecialAttribute, SpecialOption> specialOptions) {
        Level = level;
        BasicOptions = basicOptions;
        SpecialOptions = specialOptions;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(Level);

        writer.WriteInt(BasicOptions.Count);
        foreach ((BasicAttribute type, BasicOption option) in BasicOptions) {
            writer.WriteShort((short) type);
            writer.Write<BasicOption>(option);
        }
        writer.WriteInt(SpecialOptions.Count);
        foreach ((SpecialAttribute type, SpecialOption option) in SpecialOptions) {
            writer.WriteShort((short) type);
            writer.Write<SpecialOption>(option);
        }
    }

    public void ReadFrom(IByteReader reader) {
        Level = reader.ReadInt();

        int statCount = reader.ReadInt();
        for (int i = 0; i < statCount; i++) {
            var type = (BasicAttribute) reader.ReadShort();
            BasicOptions[type] = reader.Read<BasicOption>();
        }
        int specialCount = reader.ReadInt();
        for (int i = 0; i < specialCount; i++) {
            var type = (SpecialAttribute) reader.ReadShort();
            SpecialOptions[type] = reader.Read<SpecialOption>();
        }
    }
}
