using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model.Enum;
using Maple2.Server.Game.Model.Skill;
using System.Numerics;
using Maple2.Server.Game.Model.Field.Actor.ActorStateComponent;
using static Maple2.Server.Game.Model.ActorStateComponent.TaskState;

namespace Maple2.Server.Game.Model.ActorStateComponent;

public partial class MovementState {
    class NpcSkillCastTask : NpcTask {
        private readonly MovementState movement;

        public SkillRecord? Cast;
        public int SkillId { get; init; }
        public short SkillLevel { get; init; }
        public long SkillUid { get; init; }
        public int FaceTarget;
        public Vector3 FacePos;

        public NpcSkillCastTask(TaskState queue, MovementState movement, int id, short level, int faceTarget, Vector3 facePos, long uid) : base(queue, NpcTaskPriority.BattleAction) {
            this.movement = movement;
            SkillId = id;
            SkillLevel = level;
            SkillUid = uid;
            FaceTarget = faceTarget;
            FacePos = facePos;
        }

        protected override void TaskResumed() {
            movement.SkillCast(this, SkillId, SkillLevel, SkillUid, 0);
        }

        protected override void TaskFinished(bool isCompleted) {
            movement.castSkill = null;
            movement.castTask = null;
            movement.Idle();
            movement.actor.AppendDebugMessage((isCompleted ? "Finished" : "Canceled") + " cast\n");
        }
    }

    private void SkillCastFaceTarget(SkillRecord cast, IActor target, int faceTarget) {
        Vector3 offset = target.Position - actor.Position;
        float distance = offset.LengthSquared();

        if (faceTarget != 1) {
            if (!cast.Motion.MotionProperty.FaceTarget || cast.Metadata.Data.AutoTargeting is null) {
                return;
            }

            var autoTargeting = cast.Metadata.Data.AutoTargeting;

            bool shouldFaceTarget = autoTargeting.MaxDistance == 0 || distance <= autoTargeting.MaxDistance;
            shouldFaceTarget |= autoTargeting.MaxHeight == 0 || offset.Y <= autoTargeting.MaxHeight;

            if (!shouldFaceTarget) {
                return;
            }

            distance = (float) Math.Sqrt(distance);
            offset *= (1 / distance);

            float degreeCosine = (float) Math.Cos(autoTargeting.MaxDegree / 2);
            float dot = Vector3.Dot(offset, actor.Transform.FrontAxis);

            shouldFaceTarget = autoTargeting.MaxDegree == 0 || dot >= degreeCosine;

            if (!shouldFaceTarget) {
                return;
            }
        } else {
            distance = (float) Math.Sqrt(distance);
            offset *= (1 / distance);
        }

        actor.Transform.LookTo(offset);
    }

    private void SkillCast(NpcSkillCastTask task, int id, short level, long uid, byte motion) {
        if (castTask != task) {
            castTask?.Cancel();
        }

        if (!CanTransitionToState(ActorState.PcSkill)) {
            task.Cancel();

            return;
        }

        Velocity = new Vector3(0, 0, 0);

        SkillRecord? cast = actor.CastSkill(id, level, uid, motion);

        if (cast is null) {
            task.Cancel();

            return;
        }

        if (!actor.AnimationState.TryPlaySequence(cast.Motion.MotionProperty.SequenceName, cast.Motion.MotionProperty.SequenceSpeed, AnimationType.Skill, out AnimationSequence? sequence)) {
            task.Cancel();

            return;
        }

        if (task.FacePos != new Vector3(0, 0, 0)) {
            actor.Transform.LookTo(Vector3.Normalize(task.FacePos - actor.Position));
        } else if (actor.BattleState.Target is not null) {
            SkillCastFaceTarget(cast, actor.BattleState.Target, task.FaceTarget);
        }

        castTask = task;
        castSkill = cast;
        task.Cast = cast;

        //if (faceTarget && actor.BattleState.Target is not null) {
        //    actor.Transform.LookTo(Vector3.Normalize(actor.BattleState.Target.Position - actor.Position));
        //}

        SetState(ActorState.PcSkill);

        stateSequence = sequence;

        return;
    }
}
