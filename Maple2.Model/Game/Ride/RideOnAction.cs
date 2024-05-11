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

public class RideOnActionUseItem : RideOnAction {
    private readonly Item item;
    public int ItemId => item.Id;
    public long ItemUid => item.Uid;

    public RideOnActionUseItem(int rideId, int objectId, Item item) : base(RideOnType.UseItem, rideId, objectId) {
        this.item = item;
    }

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteInt(item.Id);
        writer.WriteLong(item.Uid);
        writer.WriteClass<UgcItemLook>(item.Template ?? UgcItemLook.Default);
    }
}

public class RideOnActionAdditionalEffect : RideOnAction {
    public readonly int SkillId;
    public readonly short SkillLevel;

    public RideOnActionAdditionalEffect(int rideId, int objectId, int skillId, short skillLevel) : base(RideOnType.AdditionalEffect, rideId, objectId) {
        SkillId = skillId;
        SkillLevel = skillLevel;
    }

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteInt(SkillId);
        writer.WriteShort(SkillLevel);
    }
}

public class RideOnActionHideAndSeek(int rideId, int objectId) : RideOnAction(RideOnType.HideAndSeek, rideId, objectId);
