namespace Actions
{
    public class ActionGoToPosition : AIAction
    {
        public override void OnStart(AIBrain ai)
        {
            AddNeededStateSystem<StateSystems.AIStateSystemMove>();

            repeatType = ET.ActionType.Once;

            correspondingState = ET.StateType.Move;
        }

        public override void Activate(AIBrain ai)
        {
        }

        public override bool IsCompleted(AIBrain ai)
        {
            return true;
        }

        public override bool IsStillValid(AIBrain ai)
        {
            return true;
        }
    }
}