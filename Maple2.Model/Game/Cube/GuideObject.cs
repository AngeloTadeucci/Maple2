using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public enum GuideObjectType : short {
    Construction = 0,
    Fishing = 1,
    SkillGuide = 2,
}

public interface IGuideObject : IByteSerializable {
    public GuideObjectType Type { get; }
}

public class ConstructionGuideObject : IGuideObject {
    public GuideObjectType Type => GuideObjectType.Construction;

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(1);
    }
}

public class FishingGuideObject : IGuideObject {
    public GuideObjectType Type => GuideObjectType.Fishing;

    public readonly FishingRodTable.Entry Rod;
    public readonly FishTable.Spot Spot;

    public FishingGuideObject(FishingRodTable.Entry rod, FishTable.Spot spot) {
        Rod = rod;
        Spot = spot;
    }

    public void WriteTo(IByteWriter writer) { }
}

public class SkillMagicControlGuide : IGuideObject {
    public GuideObjectType Type => GuideObjectType.SkillGuide;

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong();
        writer.WriteInt();
        writer.WriteShort();
        writer.WriteByte();
        writer.WriteByte();
        writer.WriteInt();
        writer.WriteByte(); // count
        // for (int i = 0; i < count; i++) {
        //     writer.WriteLong();
        //     writer.WriteByte();
        // }
    }
}
