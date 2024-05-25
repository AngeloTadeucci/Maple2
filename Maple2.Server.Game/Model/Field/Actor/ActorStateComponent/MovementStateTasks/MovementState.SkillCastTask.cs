using Maple2.Model.Enum;
using Maple2.Server.Game.Model.Enum;
using Maple2.Server.Game.Model.Skill;
using System.Numerics;
using static Maple2.Server.Game.Model.Field.Actor.ActorStateComponent.TaskState;

namespace Maple2.Server.Game.Model.Field.Actor.ActorStateComponent;

public partial class MovementState {
    class NpcSkillCastTask : NpcTask {
        private MovementState movement;

        public SkillRecord? Cast;
        public int SkillId { get; init; }
        public short SkillLevel { get; init; }
        public long SkillUid { get; init; }

        public NpcSkillCastTask(TaskState queue, MovementState movement, int id, short level, long uid) : base(queue, NpcTaskPriority.BattleAction) {
            this.movement = movement;
            SkillId = id;
            SkillLevel = level;
            SkillUid = uid;
        }

        override protected void TaskResumed() {
            movement.SkillCast(this, SkillId, SkillLevel, SkillUid);
        }

        protected override void TaskFinished(bool isCompleted) {
            movement.castSkill = null;
            movement.castTask = null;
            movement.Idle();
            movement.actor.AppendDebugMessage((isCompleted ? "Finished" : "Canceled") + " cast\n");
        }
    }

    private void SkillCast(NpcSkillCastTask task, int id, short level, long uid) {
        castTask?.Cancel();

        if (!CanTransitionToState(ActorState.PcSkill)) {
            task.Cancel();

            return;
        }

        Velocity = new Vector3(0, 0, 0);

        SkillRecord? cast = actor.CastSkill(id, level, uid);

        if (cast is null) {
            return;
        }

        if (!actor.AnimationState.TryPlaySequence(cast.Motion.MotionProperty.SequenceName, cast.Motion.MotionProperty.SequenceSpeed, AnimationType.Skill)) {
            task.Cancel();

            return;
        }

        castTask = task;
        castSkill = cast;
        task.Cast = cast;

        //if (faceTarget && actor.BattleState.Target is not null) {
        //    actor.Transform.LookTo(Vector3.Normalize(actor.BattleState.Target.Position - actor.Position));
        //}

        SetState(ActorState.PcSkill, ActorSubState.Skill_Default);

        stateSequence = actor.AnimationState.PlayingSequence;

        return;
    }
}
