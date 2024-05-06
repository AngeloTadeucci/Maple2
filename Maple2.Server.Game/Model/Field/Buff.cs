using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model.Skill;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Util;
using Maple2.Tools;
using Serilog;

namespace Maple2.Server.Game.Model;

public class Buff : IUpdatable, IByteSerializable {
    private readonly FieldManager field;
    public readonly AdditionalEffectMetadata Metadata;
    public readonly int ObjectId;
    public long CastUid { get; set; }

    public readonly IActor Caster;
    public readonly IActor Owner;

    public int Id => Metadata.Id;
    public short Level => Metadata.Level;

    public long StartTick { get; private set; }
    public long EndTick { get; private set; }
    public long IntervalTick { get; private set; }
    public long NextProcTick { get; protected set; }
    public int ProcCount { get; private set; }
    public int Stacks { get; private set; }
    public long ShieldHealth { get; set; }

    public bool Enabled { get; private set; }

    private bool activated;
    private readonly bool canExpire;
    private bool canProc;

    private readonly ILogger logger = Log.ForContext<Buff>();

    public Buff(FieldManager field, AdditionalEffectMetadata metadata, int objectId, IActor caster, IActor owner) {
        this.field = field;
        Metadata = metadata;
        ObjectId = objectId;

        Caster = caster;
        Owner = owner;

        // Buffs with IntervalTick=0 will just proc a single time
        IntervalTick = metadata.Property.IntervalTick > 0 ? metadata.Property.IntervalTick : metadata.Property.DurationTick + 1000;

        Stack();
        NextProcTick = StartTick + this.Metadata.Property.DelayTick + this.Metadata.Property.IntervalTick;
        UpdateEnabled(false);
        canProc = metadata.Property.KeepCondition != BuffKeepCondition.UnlimitedDuration;
        canExpire = metadata.Property.KeepCondition != BuffKeepCondition.UnlimitedDuration && EndTick >= StartTick;
    }

    public bool Stack(int amount = 1) {
        Stacks = Math.Min(Stacks + 1, Metadata.Property.MaxCount);
        StartTick = Environment.TickCount64;

        if (Stacks == 1 || Metadata.Property.ResetCondition != BuffResetCondition.PersistEndTick) {
            EndTick = StartTick + Metadata.Property.DurationTick;
        }
        return true;
    }

    public void RemoveStack(int amount = 1) {
        Stacks = Math.Max(0, Stacks - amount);
        if (Stacks == 0) {
            Owner.Buffs.Remove(Id);
        }
    }

    public virtual void Update(long tickCount) {
        if (!activated) {
            if (Metadata.Update.Cancel != null) {
                foreach (int id in Metadata.Update.Cancel.Ids) {
                    if (Metadata.Update.Cancel.CheckSameCaster && Owner.ObjectId != Caster.ObjectId) {
                        continue;
                    }

                    Owner.Buffs.Remove(id);
                }
            }

            activated = true;
        }

        if (canExpire && !canProc && tickCount > EndTick) {
            Owner.Buffs.Remove(Id);
            return;
        }

        if (!UpdateEnabled()) {
            return;
        }

        if (!canProc || tickCount < NextProcTick) {
            return;
        }

        Proc();
    }

    private bool UpdateEnabled(bool notifyField = true) {
        bool enabled = Metadata.Condition.Check(Caster, Owner, Owner);
        if (Enabled != enabled) {
            Enabled = enabled;
            if (notifyField) {
                field.Broadcast(BuffPacket.Update(this));
            }
        }

        return enabled;
    }

    public void Disable() {
        Enabled = false;
        canProc = false;
    }
    public void Enable() => Enabled = true;

    private void Proc() {
        ProcCount++;

        ApplyRecovery();
        ApplyDotDamage();
        ApplyDotBuff();
        ApplyCancel();
        ModifyDuration();

        foreach (SkillEffectMetadata effect in Metadata.Skills) {
            if (effect.Condition != null) {
                // logger.Error("Buff Condition-Effect unimplemented from {Id} on {Owner}", Id, Owner.ObjectId);
                switch (effect.Condition.Target) {
                    case SkillEntity.Target:
                        Owner.ApplyEffect(Caster, Owner, effect);
                        break;
                    case SkillEntity.Caster:
                        Caster.ApplyEffect(Caster, Owner, effect);
                        break;
                    default:
                        logger.Error("Invalid Buff Target: {Target}", effect.Condition.Target);
                        break;
                }
            } else if (effect.Splash != null) {
                field.AddSkill(Caster, effect, [Owner.Position], Owner.Rotation);
            }
        }

        NextProcTick += IntervalTick;
        if (NextProcTick > EndTick) {
            canProc = false;
        }
    }

    private void ApplyRecovery() {
        if (Metadata.Recovery == null) {
            return;
        }

        var record = new HealDamageRecord(Caster, Owner, ObjectId, Metadata.Recovery);
        var updated = new List<BasicAttribute>(3);
        if (record.HpAmount != 0) {
            Owner.Stats[BasicAttribute.Health].Add(record.HpAmount);
            updated.Add(BasicAttribute.Health);
        }
        if (record.SpAmount != 0) {
            Owner.Stats[BasicAttribute.Spirit].Add(record.SpAmount);
            updated.Add(BasicAttribute.Spirit);
        }
        if (record.EpAmount != 0) {
            Owner.Stats[BasicAttribute.Stamina].Add(record.EpAmount);
            updated.Add(BasicAttribute.Stamina);
        }

        if (updated.Count > 0) {
            field.Broadcast(StatsPacket.Update(Owner, updated.ToArray()));
        }
        field.Broadcast(SkillDamagePacket.Heal(record));
    }

    private void ApplyDotDamage() {
        if (Metadata.Dot.Damage == null) {
            return;
        }

        var record = new DotDamageRecord(Caster, Owner, Metadata.Dot.Damage) {
            ProcCount = ProcCount,
        };
        var targetUpdated = new List<BasicAttribute>(3);
        if (record.HpAmount != 0) {
            Owner.Stats[BasicAttribute.Health].Add(record.HpAmount);
            targetUpdated.Add(BasicAttribute.Health);
        }
        if (record.SpAmount != 0) {
            Owner.Stats[BasicAttribute.Spirit].Add(record.SpAmount);
            targetUpdated.Add(BasicAttribute.Spirit);
        }
        if (record.EpAmount != 0) {
            Owner.Stats[BasicAttribute.Stamina].Add(record.EpAmount);
            targetUpdated.Add(BasicAttribute.Stamina);
        }

        if (targetUpdated.Count <= 0) {
            return;
        }

        field.Broadcast(StatsPacket.Update(Owner, targetUpdated.ToArray()));
        field.Broadcast(SkillDamagePacket.DotDamage(record));
        if (record.RecoverHp != 0) {
            Caster.Stats[BasicAttribute.Health].Add(record.RecoverHp);
            field.Broadcast(StatsPacket.Update(Caster, BasicAttribute.Health));
        }
    }

    private void ApplyDotBuff() {
        if (Metadata.Dot.Buff == null) {
            return;
        }

        AdditionalEffectMetadataDot.DotBuff dotBuff = Metadata.Dot.Buff;
        if (dotBuff.Target == SkillEntity.Target) {
            Owner.AddBuff(Caster, Owner, dotBuff.Id, dotBuff.Level);
        } else {
            Caster.AddBuff(Caster, Owner, dotBuff.Id, dotBuff.Level);
        }
    }

    private void ApplyCancel() {
        if (Metadata.Update.Cancel == null) {
            return;
        }

        if (Metadata.Update.Cancel.Ids.Length > 0) {
            foreach (int id in Metadata.Update.Cancel.Ids) {
                if (Owner.Buffs.Buffs.TryGetValue(id, out Buff? buff)
                    && (!Metadata.Update.Cancel.CheckSameCaster || buff.Caster.ObjectId == Caster.ObjectId)) {
                    Owner.Buffs.Remove(id);
                }
            }

            List<Buff> buffsToRemove = Owner.Buffs.Buffs.Values
                .Where(buff =>
                    Metadata.Update.Cancel.Categories.Contains(buff.Metadata.Property.Category)
                    && (!Metadata.Update.Cancel.CheckSameCaster || buff.Caster.ObjectId == Caster.ObjectId)
                ).ToList();

            buffsToRemove.ForEach(buff => Owner.Buffs.Remove(buff.Id));
        }
    }

    public void ModifyDuration() {
        foreach (AdditionalEffectMetadataUpdate.ModifyDuration modifyDuration in Metadata.Update.Duration) {
            if (!Owner.Buffs.Buffs.TryGetValue(modifyDuration.Id, out Buff? buff)) {
                continue;
            }
            buff.EndTick += (long) modifyDuration.Value;
            if (modifyDuration.Rate > 0) {
                long remainingDuration = (long) (modifyDuration.Rate * (buff.EndTick - Environment.TickCount64));
                buff.EndTick += (modifyDuration.Rate >= 1) ? remainingDuration : -remainingDuration;
            }

            // restart proc if possible
            if (NextProcTick < EndTick) {
                canProc = true;
            }
            field.Broadcast(BuffPacket.Update(buff));
        }
    }

    public void WriteTo(IByteWriter writer) {
        WriteAdditionalEffect(writer);
        WriteShieldHealth(writer);
    }

    public void WriteAdditionalEffect(IByteWriter writer) {
        writer.WriteInt((int) StartTick);
        writer.WriteInt((int) EndTick);
        writer.WriteInt(Id);
        writer.WriteShort(Level);
        writer.WriteInt(Stacks);
        writer.WriteBool(Enabled);
    }

    public void WriteShieldHealth(IByteWriter writer) {
        writer.WriteLong(ShieldHealth);
    }
}
