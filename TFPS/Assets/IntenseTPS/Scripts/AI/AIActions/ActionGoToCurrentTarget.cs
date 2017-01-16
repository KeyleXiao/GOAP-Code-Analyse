using StateSystems;

namespace Actions
{
    public class ActionGoToCurrentTarget : ActionGoToPosition
    {
        public float reachTolerance = .35f;

        public override void OnStart(AIBrain ai)
        {
            base.OnStart(ai);
            AddNeededStateSystem<AIStateSystemMove>();

            preConditions.Add(DS.weaponAimed, false);

            postEffects.Add(DS.isNearCurrentTarget, true);

            correspondingState = ET.StateType.Move;
        }

        public override void Activate(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemMove>().SetMoveToPositionNStartMove
                (ai, ET.MoveType.Run, ET.MoveToType.ToCurrentTarget);
        }

        public override bool CanActivate(AIBrain ai)
        {
            return ai.HaveCurrentTarget();
        }

        public override bool CanBeAddedToPlan(AIBrain ai)
        {
            return ai.HaveCurrentTarget();
        }

        public override bool IsCompleted(AIBrain ai)
        {
            return ai.GetStateSystem<AIStateSystemMove>().ReachedDestination(ai, reachTolerance);
        }

        public override bool IsStillValid(AIBrain ai)
        {
            return ai.HaveCurrentTarget();
        }

        public override void DeActivate(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemMove>().StopMoving(ai);
        }

        public override void GeneralPostEffects(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemMove>().StopMoving(ai);
            base.GeneralPostEffects(ai);
        }

        public override bool IsInterruptableBySystems(AIBrain ai)
        {
            return true;
        }
    }
}