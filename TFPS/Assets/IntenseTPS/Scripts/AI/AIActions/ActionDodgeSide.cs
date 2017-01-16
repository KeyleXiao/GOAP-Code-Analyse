using Information;
using Sensors;
using StateSystems;

namespace Actions
{
    public class ActionDodgeSide : AIAction
    {
        private InformationDodgeSide infoDodge;
        private SensorDodgeSideFinder sensorSideFinder;

        public override void OnStart(AIBrain ai)
        {
            AddNeededStateSystem<AIStateSystemAnimator>();

            AddNeededSensor<SensorDodgeSideFinder>();

            repeatType = ET.ActionType.Once;
            sensorSideFinder = ai.GetSensor<SensorDodgeSideFinder>();

            preConditions.Add(DS.takingBulletDamage, true);

            postEffects.Add(DS.bulletDodged, true);

            correspondingState = ET.StateType.Animate;
        }

        public override void Activate(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemAnimator>().AnimateTrigger(ai, "Dodge", false, false, "", "Locomotion", 0);
            ai.GetStateSystem<AIStateSystemAnimator>().AnimateFloat(ai, "Angle",
                infoDodge.angle.Value, false, false, "", "Locomotion", 0);
        }

        public override bool CanActivate(AIBrain ai)
        {
            infoDodge = sensorSideFinder.RequestInfo<InformationDodgeSide>(ai);

            if (infoDodge != null && ai.HaveCurrentTarget())
                return true;
            return false;
        }

        public override bool CanBeAddedToPlan(AIBrain ai)
        {
            if (ai.HaveCurrentWeapon() && ai.HaveCurrentTarget())
                return true;
            return false;
        }

        public override void DeActivate(AIBrain ai)
        {
            infoDodge = null;
        }

        public override void GeneralPostEffects(AIBrain ai)
        {
            base.GeneralPostEffects(ai);
            infoDodge = null;
        }

        public override bool IsCompleted(AIBrain ai)
        {
            if (ai.GetStateSystem<AIStateSystemAnimator>().IsStartedAnimationFinished("", "Locomotion"))
                return true;
            return false;
        }

        public override bool IsStillValid(AIBrain ai)
        {
            return true;
        }

        public override bool IsInterruptableBySystems(AIBrain ai)
        {
            return false;
        }
    }
}