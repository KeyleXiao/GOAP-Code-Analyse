namespace StateSystems
{
    public abstract class AIStateSystem
    {
        public AIStateSystem()
        {
        }

        /// <summary>
        /// Called on Start - Place to manage references
        /// </summary>
        virtual public void OnStart(AIBrain ai) { }

        /// <summary>
        /// Called in the same update just after an <see cref="Actions.AIAction"/> is activated.
        /// You can use <param name="stateType" of <see cref="Actions.AIAction"/> to change behaviour
        /// </summary>
        virtual public void OnActionActivate(AIBrain ai, ET.StateType stateType) { }

        /// <summary>
        /// Called every update after an <see cref="Actions.AIAction"/> is activated
        /// You can use <param name="stateType" of <see cref="Actions.AIAction"/> to change behaviour
        /// </summary>
        virtual public void OnUpdate(AIBrain ai, ET.StateType stateType) { }

        /// <summary>
        /// Called in the same update after an <see cref="Actions.AIAction"/> is completed or deactivated.
        /// You can use <param name="stateType" of <see cref="Actions.AIAction"/> to change behaviour
        /// </summary>
        virtual public void OnActionExit(AIBrain ai, ET.StateType stateType) { }

        /// <summary>
        /// Called every OnAnimatorIK update after an <see cref="Actions.AIAction"/> is activated
        /// You can use <param name="stateType" of <see cref="Actions.AIAction"/> to change behaviour
        /// </summary>
        virtual public void OnAnimatorIK(AIBrain ai, int layerIndex, ET.StateType stateType) { }

        /// <summary>
        /// Called every OnAnimatorMove update after an <see cref="Actions.AIAction"/> is activated
        /// You can use <param name="stateType" of <see cref="Actions.AIAction"/> to change behaviour
        /// </summary>
        virtual public void OnAnimatorMove(AIBrain ai, ET.StateType stateType) { }
    }
}