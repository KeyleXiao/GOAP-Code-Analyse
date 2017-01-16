using StateSystems;

namespace Actions
{
    public class ActionHolsterWeapon : AIAction
    {
        public override void OnStart(AIBrain ai)
        {
            AddNeededStateSystem<AIStateSystemAnimator>();
            AddNeededStateSystem<AIStateSystemWeapon>();

            repeatType = ET.ActionType.Once;

            preConditions.Add(DS.weaponAimed, false);
            preConditions.Add(DS.haveWeapon, true);
            preConditions.Add(DS.weaponArmed, true);

            postEffects.Add(DS.weaponArmed, false);

            correspondingState = ET.StateType.Animate;
        }

        public override void Activate(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemWeapon>().DisArmWeapon(ai);
        }

        public override void DeActivate(AIBrain ai)
        {
            base.GeneralPostEffects(ai);
        }

        public override bool IsCompleted(AIBrain ai)
        {
            return ai.GetStateSystem<AIStateSystemWeapon>().IsWeaponDisArmingFinished(ai) ? true : false;
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