using System.Collections.Generic;
using System.Linq;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game;

public class SkillPoint : IByteSerializable {
    public const short RANKS = 2;
    public readonly IDictionary<SkillPointSource, PointRank> Points;

    public int TotalPoints => Points.Values.Sum(pointRank => pointRank.TotalPoints);

    public PointRank this[SkillPointSource type] {
        get => Points[type];
        set => Points[type] = value;
    }


    public SkillPoint() {
        Points = new Dictionary<SkillPointSource, PointRank>();
        foreach (SkillPointSource source in System.Enum.GetValues<SkillPointSource>()) {
            Points[source] = new PointRank();
            foreach (short rank in Enumerable.Range(0, RANKS)) {
                Points[source][rank] = 0;
            }
        }
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(TotalPoints);
        writer.WriteInt(Points.Count);
        foreach ((SkillPointSource source, PointRank point) in Points) {
            writer.Write<SkillPointSource>(source);
            writer.WriteClass<PointRank>(point);
        }
        writer.WriteInt();
    }

    public class PointRank : IByteSerializable {
        public IDictionary<short, int> Ranks;

        public int this[short rank] {
            get => Ranks[rank];
            set => Ranks[rank] = value;
        }

        public int TotalPoints => Ranks.Values.Sum();

        public PointRank() {
            Ranks = new Dictionary<short, int>();
        }

        public void WriteTo(IByteWriter writer) {
            writer.WriteInt(Ranks.Count);
            foreach ((short rank, int points) in Ranks) {
                writer.WriteShort(rank);
                writer.WriteInt(points);
            }
        }
    }
}
