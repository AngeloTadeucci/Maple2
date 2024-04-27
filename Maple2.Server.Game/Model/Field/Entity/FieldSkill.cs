using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model.Skill;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Util;
using Maple2.Tools.Collision;
using Serilog;

namespace Maple2.Server.Game.Model;

public class FieldSkill : FieldEntity<SkillMetadata> {
    public IActor Caster { get; init; }
    public int Interval { get; }
    public int FireCount { get; private set; }
    public bool Enabled => FireCount < 0 || NextTick <= endTick || Environment.TickCount64 <= endTick;
    public bool Active { get; private set; } = true;

    public readonly Vector3[] Points;
    public readonly bool UseDirection;
    private readonly long endTick;
    public long NextTick { get; private set; }

    private readonly ILogger logger = Log.ForContext<FieldSkill>();

    public FieldSkill(FieldManager field, int objectId, IActor caster,
                      SkillMetadata value, int interval, params Vector3[] points) : base(field, objectId, value) {
        Caster = caster;
        Points = points;
        Interval = interval;
        FireCount = -1;
        NextTick = Environment.TickCount64 + interval;
    }

    public FieldSkill(FieldManager field, int objectId, IActor caster,
                      SkillMetadata value, int fireCount, SkillEffectMetadataSplash splash, params Vector3[] points) : base(field, objectId, value) {
        Caster = caster;
        Points = points;
        Interval = splash.Interval;
        FireCount = fireCount;
        UseDirection = splash.UseDirection;

        long baseTick = Environment.TickCount64;
        if (splash.ImmediateActive) {
            NextTick = baseTick;
            endTick = baseTick + splash.RemoveDelay + (FireCount - 1) * splash.Interval;
        } else {
            NextTick = baseTick + splash.Delay + splash.Interval;
            endTick = baseTick + splash.Delay + splash.RemoveDelay + FireCount * splash.Interval;
        }
        if (splash.OnlySensingActive) {
            endTick = baseTick + splash.Delay + splash.RemoveDelay;
            Active = false;
        }
    }

    public override void Update(long tickCount) {
        if (!Enabled) {
            Field.RemoveSkill(ObjectId);
            return;
        }

        if (tickCount < NextTick) {
            return;
        }

        // Check Activation for OnlySensingActive
        if (!Active) {
            foreach (SkillMetadataMotion motion in Value.Data.Motions) {
                foreach (SkillMetadataAttack attack in motion.Attacks) {
                    Prism[] prisms = Points.Select(point => attack.Range.GetPrism(point, UseDirection ? Rotation.Z : 0)).ToArray();
                    if (Field.GetTargets(prisms, attack.Range.ApplyTarget, attack.TargetCount).Any()) {
                        Active = true;
                        goto activated;
                    }
                }
            }

            NextTick = Environment.TickCount64 + Interval;
            return;
        }

    activated:
        // TODO: These are buffs? seems irrelevant to FieldSkill?
        // foreach (SkillEffectMetadata skill in Value.Data.Skills) {
        //     if (skill.Condition != null) {
        //         ConditionSkill(Field.Players.Values, skill);
        //     } else if (skill.Splash != null) {
        //         SplashSkill(skill);
        //     }
        // }

        var record = new SkillRecord(Value, 0, Caster) {
            TargetUid = (long) ObjectId << 32,
            Position = Position,
            Rotation = Rotation,
        };
        for (byte i = 0; record.TrySetMotionPoint(i); i++) {
            for (byte j = 0; record.TrySetAttackPoint(j); j++) {
                SkillMetadataAttack attack = record.Attack;
                record.TargetUid++;
                var damage = new DamageRecord {
                    CasterId = Caster.ObjectId,
                    OwnerId = ObjectId,
                    SkillId = Value.Id,
                    Level = Value.Level,
                    MotionPoint = record.MotionPoint,
                    AttackPoint = record.AttackPoint,
                    Direction = Rotation,
                };
                var targetRecords = new List<TargetRecord>();
                if (attack.Arrow.BounceType > 0) {
                    IActor[] targets = Array.Empty<IActor>();
                    var bounceTargets = new List<IActor>();
                    Vector3 position = Position;
                    long prevTargetUid = 0;
                    for (int bounce = 0; bounce <= attack.Arrow.BounceCount; bounce++) {
                        Vector3 box = attack.Arrow.Collision + attack.Arrow.CollisionAdd;
                        var circle = new Circle(new Vector2(position.X, position.Y), attack.Arrow.BounceRadius);
                        // var rectangle = new Rectangle(new Vector2(Position.X, Position.Y), box.X, box.Y, UseDirection ? Rotation.Z : 0);
                        var prism = new Prism(circle, position.Z, box.Z);

                        targets = attack.Arrow.BounceOverlap
                            ? Field.GetTargets(new[] { prism }, record.Attack.Range.ApplyTarget, 1, targets).ToArray()
                            : Field.GetTargets(new[] { prism }, record.Attack.Range.ApplyTarget, 1, bounceTargets).ToArray();
                        if (targets.Length <= 0) {
                            break;
                        }

                        IActor target = targets[0];
                        record.TargetUid++;
                        targetRecords.Add(new TargetRecord {
                            PrevUid = prevTargetUid,
                            Uid = record.TargetUid,
                            TargetId = target.ObjectId,
                            Index = (byte) targetRecords.Count,
                        });
                        bounceTargets.Add(target);
                        position = target.Position;
                        prevTargetUid = record.TargetUid;
                    }

                    Field.Broadcast(SkillDamagePacket.Target(record, targetRecords));
                    Field.Broadcast(SkillDamagePacket.Region(damage));
                    for (int t = 0; t < bounceTargets.Count; t++) {
                        IActor target = bounceTargets[t];
                        record.TargetUid = targetRecords[t].Uid;
                        record.Position = target.Position;
                        record.Targets = new[] { target };

                        // TODO: There should be some delay between bounces.
                        target.TargetAttack(record);
                        ApplyEffect(attack, target);
                    }
                } else {
                    Prism[] prisms = Points.Select(point => attack.Range.GetPrism(point, UseDirection ? Rotation.Z : 0)).ToArray();
                    IActor[] targets = Field.GetTargets(prisms, attack.Range.ApplyTarget, attack.TargetCount).ToArray();
                    // if (targets.Length > 0) {
                    //     logger.Debug("[{Tick}] {ObjectId}:{AttackPoint} Targeting: {Count}/{Limit} {Type}",
                    //         NextTick, ObjectId, attack.Point, targets.Length, attack.TargetCount, attack.Range.ApplyTarget);
                    //     foreach (IActor target in targets) {
                    //         logger.Debug("- {Id} @ {Position}", target.ObjectId, target.Position);
                    //     }
                    // }
                    // else if (Caster.ObjectId != 0) { // Not Ms2RegionSkill
                    //     logger.Debug("[{Tick}] {ObjectId}:{AttackPoint} has {Count}/{Limit} {Type}",
                    //         NextTick, ObjectId, attack.Point, 0, attack.TargetCount, attack.Range.ApplyTarget);
                    //     logger.Debug("{Range}", attack.Range);
                    //     foreach (Prism prism in prisms) {
                    //         logger.Debug("- {Prism}", prism);
                    //     }
                    // }

                    foreach (IActor target in targets) {
                        target.ApplyDamage(Caster, damage, attack);
                        // TODO: Set PrevUid?
                        targetRecords.Add(new TargetRecord {
                            Uid = (long) ObjectId << 32 | (uint) targetRecords.Count,
                            TargetId = target.ObjectId,
                            Index = (byte) targetRecords.Count,
                        });
                    }

                    Field.Broadcast(SkillDamagePacket.Target(record, targetRecords));
                    Field.Broadcast(SkillDamagePacket.Region(damage));
                    ApplyEffect(attack, targets);
                }

                if (attack.Skills.Any(effect => effect.Splash != null)) {
                    Field.AddSkill(record);
                }
            }
        }

        if (Interval == 0) {
            FireCount = 0;
            NextTick = int.MaxValue;
        } else {
            FireCount--;
            NextTick += Interval;
        }
    }

    private void ApplyEffect(SkillMetadataAttack attack, params IActor[] targets) {
        foreach (SkillEffectMetadata effect in attack.Skills.Where(effect => effect.Condition != null)) {
            if (effect.Condition == null) {
                logger.Fatal("Invalid Condition-Skill being handled: {Effect}", effect);
                continue;
            }

            switch (effect.Condition.Target) {
                case SkillEntity.Owner:
                    foreach (IActor target in targets) {
                        target.ApplyEffect(Caster, Caster, effect);
                    }
                    break;
                case SkillEntity.Target:
                    Caster.ApplyEffect(Caster, Caster, effect);
                    break;
                default:
                    logger.Error("Invalid FieldSkill Target: {Target}", effect.Condition.Target);
                    break;
            }
        }
    }
}
