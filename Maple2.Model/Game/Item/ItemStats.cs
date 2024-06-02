using System.Collections.Generic;
using System.Text;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game;

public sealed class ItemStats : IByteSerializable, IByteDeserializable {
    public static readonly ItemStats Default = new ItemStats();

    public const int TYPE_COUNT = 5;

    public enum Type {
        Constant = 0,
        Static = 1,
        Random = 2,
        Title = 3,
        Empowerment = 4,
    }

    private readonly Option[] options;

    public ItemStats() {
        options = new Option[TYPE_COUNT];
        for (int i = 0; i < TYPE_COUNT; i++) {
            options[i] = new Option();
        }
    }

    public ItemStats(Dictionary<BasicAttribute, BasicOption>[] basicOption, Dictionary<SpecialAttribute, SpecialOption>[] specialOption) {
        // Ensure all entries are set.
        options = new Option[TYPE_COUNT];
        for (int i = 0; i < TYPE_COUNT; i++) {
            options[i] = new Option(
                basicOption.ElementAtOrDefault(i, () => new Dictionary<BasicAttribute, BasicOption>()),
                specialOption.ElementAtOrDefault(i, () => new Dictionary<SpecialAttribute, SpecialOption>()));
        }
    }

    public ItemStats Clone() {
        var stats = new ItemStats();
        for (int i = 0; i < TYPE_COUNT; i++) {
            stats.options[i] = new Option(
                new Dictionary<BasicAttribute, BasicOption>(options[i].Basic),
                new Dictionary<SpecialAttribute, SpecialOption>(options[i].Special));
        }
        return stats;
    }

    public Option this[Type type] {
        get => options[(int) type];
        set => options[(int) type] = value;
    }

    public void WriteTo(IByteWriter writer) {
        for (int i = 0; i < TYPE_COUNT; i++) {
            Option option = options[i];
            writer.WriteShort((short) option.Basic.Count);
            foreach ((BasicAttribute type, BasicOption basicOption) in option.Basic) {
                writer.WriteShort((short) type);
                writer.Write<BasicOption>(basicOption);
            }
            writer.WriteShort((short) option.Special.Count);
            foreach ((SpecialAttribute type, SpecialOption specialOption) in option.Special) {
                writer.WriteShort((short) type);
                writer.Write<SpecialOption>(specialOption);
            }
        }
        writer.WriteInt();
    }

    public void ReadFrom(IByteReader reader) {
        for (int i = 0; i < TYPE_COUNT; i++) {
            Option option = options[i];
            short basicCount = reader.ReadShort();
            for (int j = 0; j < basicCount; j++) {
                var type = (BasicAttribute) reader.ReadShort();
                option.Basic[type] = reader.Read<BasicOption>();
            }
            short specialCount = reader.ReadShort();
            for (int j = 0; j < specialCount; j++) {
                var type = (SpecialAttribute) reader.ReadShort();
                option.Special[type] = reader.Read<SpecialOption>();
            }

            reader.ReadInt();
        }
    }

    public class Option {
        public readonly Dictionary<BasicAttribute, BasicOption> Basic;
        public readonly Dictionary<SpecialAttribute, SpecialOption> Special;

        public int Count => Basic.Count + Special.Count;

        public Option(Dictionary<BasicAttribute, BasicOption>? basicOption = null, Dictionary<SpecialAttribute, SpecialOption>? specialOption = null) {
            Basic = basicOption ?? new Dictionary<BasicAttribute, BasicOption>();
            Special = specialOption ?? new Dictionary<SpecialAttribute, SpecialOption>();
        }

        public override string ToString() {
            var builder = new StringBuilder();
            builder.AppendLine("BasicOption:");
            foreach ((BasicAttribute attribute, BasicOption option) in Basic) {
                builder.AppendLine($"- {attribute}={option}");
            }
            builder.AppendLine("SpecialOption:");
            foreach ((SpecialAttribute attribute, SpecialOption option) in Special) {
                builder.AppendLine($"- {attribute}={option}");
            }
            return builder.ToString();
        }
    }
}
