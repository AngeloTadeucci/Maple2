using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Manager.Config;

public class StatAttributes : IByteSerializable {
    public readonly PointSources Sources;
    public readonly PointAllocation Allocation;

    public int TotalPoints => Sources.Count;
    public int UsedPoints => Allocation.Count;

    public StatAttributes() {
        Sources = new PointSources();
        Allocation = new PointAllocation();
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteClass<PointSources>(Sources);
        writer.WriteClass<PointAllocation>(Allocation);
    }

    public class PointSources : IByteSerializable {
        // MaxPoints - Trophy:38, Exploration:12, Prestige:50
        public readonly IDictionary<AttributePointSource, int> Points;

        public int Count => Points.Values.Sum();

        public int this[AttributePointSource type] {
            get => Points[type];
            set => Points[type] = value;
        }

        public PointSources() {
            Points = new Dictionary<AttributePointSource, int>();
            foreach (AttributePointSource source in Enum.GetValues(typeof(AttributePointSource))) {
                Points[source] = 0;
            }
        }

        public void WriteTo(IByteWriter writer) {
            writer.WriteInt(Points.Count);
            foreach ((AttributePointSource source, int amount) in Points) {
                writer.Write<AttributePointSource>(source);
                writer.WriteInt(amount);
            }
        }
    }

    public class PointAllocation : IByteSerializable {
        private readonly Dictionary<BasicAttribute, int> points;

        public BasicAttribute[] Attributes => points.Keys.ToArray();
        public int Count => points.Values.Sum();

        public int this[BasicAttribute type] {
            get => points.GetValueOrDefault(type);
            set {
                if (value < 0 || value > StatLimit(type)) {
                    return;
                }
                if (value == 0) {
                    points.Remove(type);
                    return;
                }

                points[type] = value;
            }
        }

        public PointAllocation() {
            points = new Dictionary<BasicAttribute, int>();
        }

        public static int StatLimit(BasicAttribute type) {
            return type switch {
                BasicAttribute.Strength => Constant.StatPointLimit_str,
                BasicAttribute.Dexterity => Constant.StatPointLimit_dex,
                BasicAttribute.Intelligence => Constant.StatPointLimit_int,
                BasicAttribute.Luck => Constant.StatPointLimit_luk,
                BasicAttribute.Health => Constant.StatPointLimit_hp,
                BasicAttribute.CriticalRate => Constant.StatPointLimit_cap,
                _ => 0,
            };
        }

        public void WriteTo(IByteWriter writer) {
            writer.WriteInt(points.Count);
            foreach ((BasicAttribute type, int value) in points) {
                writer.Write<BasicAttribute>(type);
                writer.WriteInt(value);
            }
        }
    }
}
