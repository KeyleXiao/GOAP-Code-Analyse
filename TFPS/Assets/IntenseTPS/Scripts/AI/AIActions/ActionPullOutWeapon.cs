using StateSystems;

namespace Actions
{
    public class ActionPullOutWeapon : AIAction
    {
        public override void OnStart(AIBrain ai)
        {
            AddNeededStateSystem<AIStateSystemAnimator>();
            AddNeededStateSystem<AIStateSystemWeapon>();

            repeatType = ET.ActionType.Once;

            preConditions.Add(DS.haveWeapon, true);

            postEffects.Add(DS.weaponArmed, true);

            correspondingState = ET.StateType.Animate;
        }

        public override void Activate(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemWeapon>().ArmWeapon(ai);
            ai.GetStateSystem<AIStateSystemAnimator>().EnableLayer(ai, 1, true, false);
        }

        public override void DeActivate(AIBrain ai)
        {
            base.GeneralPostEffects(ai);
        }

        public override bool IsCompleted(AIBrain ai)
        {
            return ai.GetStateSystem<AIStateSystemWeapon>().IsWeaponArmingFinished(ai) ? true : false;
        }

        public override bool IsStillValid(AIBrain ai)
        {
            return true;
        }

        public override void GeneralPostEffects(AIBrain ai)
        {
            base.GeneralPostEffects(ai);
        }

        public override bool CanActivate(AIBrain ai)
        {
            return ai.CurrentWeapon ? true : false;
        }

        public override bool IsInterruptableBySystems(AIBrain ai)
        {
            return false;
        }
    }
}