using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game;

public class RideOnAction : IByteSerializable {
    public readonly RideOnType Type;
    public readonly int RideId;
    public readonly int ObjectId;

    public RideOnAction(int rideId, int objectId) : this(RideOnType.Default, rideId, objectId) { }

    protected RideOnAction(RideOnType type, int rideId, int objectId) {
        Type = type;
        RideId = rideId;
        ObjectId = objectId;
    }

    public virtual void WriteTo(IByteWriter writer) {
        writer.Write<RideOnType>(Type);
        writer.WriteInt(RideId);
        writer.WriteInt(ObjectId);
    }
}

public class RideOnActionDefault : RideOnAction {
    private readonly Item item;
    public int ItemId => item.Id;
    public long ItemUid => item.Uid;

    public RideOnActionDefault(int rideId, int objectId, Item item) : base(RideOnType.Default, rideId, objectId) {
        this.item = item;
    }

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteInt(item.Id);
        writer.WriteLong(item.Uid);
        writer.WriteClass<UgcItemLook>(item.Template ?? UgcItemLook.Default);
    }
}

public class RideOnActionBattle : RideOnAction {
    public readonly int SkillId;
    public readonly short SkillLevel;

    public RideOnActionBattle(int rideId, int objectId, int skillId, short skillLevel) : base(RideOnType.Battle, rideId, objectId) {
        SkillId = skillId;
        SkillLevel = skillLevel;
    }

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteInt(SkillId);
        writer.WriteShort(SkillLevel);
    }
}

public class RideOnActionObject(int rideId, int objectId) : RideOnAction(RideOnType.Object, rideId, objectId);
