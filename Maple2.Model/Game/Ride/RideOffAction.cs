using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class RideOffAction : IByteSerializable {
    private readonly RideOffType type;
    private readonly bool forced;

    public RideOffAction(bool forced) : this(RideOffType.Default) {
        this.forced = forced;
    }

    protected RideOffAction(RideOffType type) {
        this.type = type;
    }

    public virtual void WriteTo(IByteWriter writer) {
        writer.Write<RideOffType>(type);
        writer.WriteBool(forced);
    }
}

public class RideOffActionUseSkill() : RideOffAction(RideOffType.UseSkill) {

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteInt(); // RideOffAction+28
        writer.WriteByte(); // RideOffAction+32
        writer.WriteInt(); // RideOffAction+36
    }
}

public class RideOffActionInteract() : RideOffAction(RideOffType.Interact) {

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteInt(); // RideOffAction+28
        writer.WriteUnicodeString();
    }
}

public class RideOffActionTaxi() : RideOffAction(RideOffType.Taxi) {

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteInt(); // RideOffAction+28
    }
}

public class RideOffActionCashCall() : RideOffAction(RideOffType.CashCall) {

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteInt();  // RideOffAction+28
        writer.WriteByte(); // RideOffAction+32
        writer.WriteLong();  // RideOffAction+20
    }
}

public class RideOffActionBeautyShop() : RideOffAction(RideOffType.BeautyShop);

public class RideOffActionTakeLr() : RideOffAction(RideOffType.TakeLr) {

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteInt();  // RideOffAction+28
        writer.WriteByte(); // RideOffAction+32
    }
}

public class RideOffActionHold() : RideOffAction(RideOffType.Hold);

public class RideOffActionRecall() : RideOffAction(RideOffType.Recall) {

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteLong(); // RideOffAction+16
    }
}

public class RideOffActionSummonPetOn() : RideOffAction(RideOffType.SummonPetOn) {

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteLong(); // RideOffAction+16
        writer.WriteByte(); // RideOffAction+40
    }
}

public class RideOffActionSummonPetTransfer() : RideOffAction(RideOffType.SummonPetTransfer) {

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteLong(); // RideOffAction+16
    }
}

public class RideOffActionHomeConvenient() : RideOffAction(RideOffType.HomeConvenient) {

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteByte(); // RideOffAction+28
    }
}

public class RideOffActionDisableField() : RideOffAction(RideOffType.DisableField);

public class RideOffActionDead() : RideOffAction(RideOffType.Dead);

public class RideOffActionAdditionalEffect() : RideOffAction(RideOffType.AdditionalEffect);

public class RideOffActionRidingUi() : RideOffAction(RideOffType.RidingUi);

public class RideOffActionHomemade() : RideOffAction(RideOffType.Homemade) {

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteUnicodeString();
        writer.WriteInt(); // RideOffAction+32
        writer.WriteShort(); // RideOffAction+36
        writer.WriteInt(); // RideOffAction+40
    }
}

public class RideOffActionAutoInteraction() : RideOffAction(RideOffType.AutoInteraction) {

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteInt();  // RideOffAction+28
        writer.WriteByte(); // RideOffAction+32
        writer.WriteShort(); // RideOffAction+36
    }
}

public class RideOffActionAutoClimb() : RideOffAction(RideOffType.AutoClimb);

public class RideOffActionCoupleEmotion() : RideOffAction(RideOffType.CoupleEmotion) {

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteInt();  // RideOffAction+28
        writer.WriteLong(); // RideOffAction+16
    }
}

// This can't be used for RideOff?
//
// public class RideOffActionReact : RideOffAction {
//     public RideOffActionReact() : base(RideOffType.React) { }
//
//     public override void WriteTo(IByteWriter writer) {
//         base.WriteTo(writer);
//         writer.WriteInt();  // RideOffAction+28
//     }
// }

public class RideOffActionUseFunctionItem() : RideOffAction(RideOffType.UseFunctionItem) {

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteLong();  // RideOffAction+16
        writer.WriteUnicodeString();
    }
}

public class RideOffActionNurturing() : RideOffAction(RideOffType.Nurturing) {

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteUnicodeString();
        writer.WriteInt(); // RideOffAction+32
    }
}

public class RideOffActionGroggy() : RideOffAction(RideOffType.Groggy);

public class RideOffActionUnRideSkill() : RideOffAction(RideOffType.UnRideSkill);

public class RideOffActionUseGlideItem() : RideOffAction(RideOffType.UseGlideItem) {

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteLong(); // RideOffAction+16
        writer.WriteByte(); // RideOffAction+40
        writer.WriteInt(); // RideOffAction+44
    }
}

public class RideOffActionHideAndSeek() : RideOffAction(RideOffType.HideAndSeek);
