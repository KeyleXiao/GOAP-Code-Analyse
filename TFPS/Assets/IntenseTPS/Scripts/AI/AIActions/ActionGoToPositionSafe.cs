using Information;
using StateSystems;

namespace Actions
{
    public class ActionGoToPositionSafe : ActionGoToPosition
    {
        private InformationSafePosition safeInfo;

        public override void OnStart(AIBrain ai)
        {
            base.OnStart(ai);

            AddNeededSensor<Sensors.SensorSafePositionFinder>();

            preConditions.Add(DS.weaponAimed, false);
            preConditions.Add(DS.haveSafePosition, true);

            postEffects.Add(DS.atSafePosition, true);

            correspondingState = ET.StateType.Move;
        }

        public override void Activate(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemMove>().SetMoveToPositionNStartMove
                (ai, ET.MoveType.Run, ET.MoveToType.ToPosition, safeInfo.safePosition.Value);
        }

        public override bool CanActivate(AIBrain ai)
        {
            safeInfo = ai.Memory.GetHighestOverall<InformationSafePosition>();
            if (safeInfo == null)
                return false;

            safeInfo.isBeingUsed = true;

            if (base.CanActivate(ai))
            {
                return true;
            }
            return false;
        }

        public override bool CanBeAddedToPlan(AIBrain ai)
        {
            return ai.Memory.MemoryContainsObjectOfType(typeof(InformationSafePosition));
        }

        public override bool IsCompleted(AIBrain ai)
        {
            return ai.GetStateSystem<AIStateSystemMove>().ReachedDestination(ai, .07f);
        }

        public override bool IsStillValid(AIBrain ai)
        {
            return base.IsStillValid(ai);
        }

        public override void DeActivate(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemMove>().StopMoving(ai);
            if (safeInfo != null)
                safeInfo.isBeingUsed = false;
            ai.WorldState.SetKey(DS.atSafePosition, false);
        }

        public override void GeneralPostEffects(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemMove>().StopMoving(ai);
            base.GeneralPostEffects(ai);
            if (safeInfo != null)
                safeInfo.isBeingUsed = false;
            ai.Memory.Remove(safeInfo);
        }

        public override bool IsInterruptableBySystems(AIBrain ai)
        {
            return false;
        }
    }
}