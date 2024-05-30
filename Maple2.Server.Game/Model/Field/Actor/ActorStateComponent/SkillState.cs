using Maple2.Model.Metadata;
using Maple2.Server.Game.Model.Skill;
using Maple2.Server.Game.Packets;
using System.Numerics;

namespace Maple2.Server.Game.Model.Field.Actor.ActorStateComponent;

public class SkillState {
    private readonly IActor actor;

    public SkillState(IActor actor) {
        this.actor = actor;
    }

    public void SkillCastAttack(SkillRecord cast, byte attackPoint, List<IActor> attackTargets) {
        if (!cast.TrySetAttackPoint(attackPoint)) {
            return;
        }

        SkillMetadataAttack attack = cast.Attack;

        if (attack.MagicPathId != 0) {
            if (actor.Field.TableMetadata.MagicPathTable.Entries.TryGetValue(attack.MagicPathId, out IReadOnlyList<MagicPath>? magicPaths)) {
                int targetIndex = 0;

                foreach (MagicPath path in magicPaths) {
                    int targetId = 0;

                    if (attack.Arrow.Overlap && attackTargets.Count > targetIndex) {
                        targetId = attackTargets[targetIndex].ObjectId;
                    }

                    var targets = new List<TargetRecord>();
                    var targetRecord = new TargetRecord {
                        Uid = 0 + 2 + targetIndex,
                        TargetId = targetId,
                        Unknown = 0,
                    };
                    targets.Add(targetRecord);

                    // TODO: chaining
                    // While chaining
                    // while (packet.ReadBool()) {
                    //     targetRecord = new TargetRecord {
                    //         PrevUid = targetRecord.Uid,
                    //         Uid = packet.ReadLong(),
                    //         TargetId = packet.ReadInt(),
                    //         Unknown = packet.ReadByte(),
                    //         Index = packet.ReadByte(),
                    //     };
                    //     targets.Add(targetRecord);
                    // }
                    if (attackTargets.Count > targetIndex) {
                        // if attack.direction == 3, use direction to target, if attack.direction == 0, use rotation maybe?
                        cast.Position = actor.Position;
                        cast.Direction = Vector3.Normalize(attackTargets[targetIndex].Position - actor.Position);
                    }

                    actor.Field.Broadcast(SkillDamagePacket.Target(cast, targets));
                }
            }
        }

        if (attack.CubeMagicPathId != 0) {

        }
    }
}
