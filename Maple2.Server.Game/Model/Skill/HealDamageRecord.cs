using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Server.Game.Model.Skill;

public class HealDamageRecord : IByteSerializable {
    public readonly IActor Caster;
    public readonly IActor Target;
    public readonly int OwnerId;
    public readonly int HpAmount;
    public readonly int SpAmount;
    public readonly int EpAmount;

    public HealDamageRecord(IActor caster, IActor target, int ownerId, AdditionalEffectMetadataRecovery recovery) {
        Caster = caster;
        Target = target;
        OwnerId = ownerId;

        HpAmount = (int) (recovery.HpValue + recovery.HpRate * target.Stats[BasicAttribute.Health].Total
                                           + recovery.RecoveryRate * caster.Stats[BasicAttribute.MagicalAtk].Current);
        SpAmount = (int) (recovery.SpValue + recovery.SpRate * target.Stats[BasicAttribute.Spirit].Total);
        EpAmount = (int) (recovery.EpValue + recovery.EpRate * target.Stats[BasicAttribute.Stamina].Total);
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(Caster.ObjectId);
        writer.WriteInt(Target.ObjectId);
        writer.WriteInt(OwnerId);
        writer.WriteInt(HpAmount);
        writer.WriteInt(SpAmount);
        writer.WriteInt(EpAmount);
    }
}
