using StateSystems;

namespace Actions
{
    public class ActionReload : AIAction
    {
        public override void OnStart(AIBrain ai)
        {
            AddNeededStateSystem<AIStateSystemAnimator>();
            AddNeededStateSystem<AIStateSystemWeapon>();

            repeatType = ET.ActionType.Once;

            preConditions.Add(DS.weaponAimed, false);
            preConditions.Add(DS.haveWeapon, true);
            preConditions.Add(DS.haveAmmo, true);
            preConditions.Add(DS.weaponArmed, true);

            postEffects.Add(DS.weaponLoaded, true);

            correspondingState = ET.StateType.Animate;
        }

        public override void Activate(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemWeapon>().ReloadWeapon(ai);
        }

        public override bool CanBeAddedToPlan(AIBrain ai)
        {
            return ai.CurrentWeapon ? true : false;
        }

        public override void GeneralPostEffects(AIBrain ai)
        {
            base.GeneralPostEffects(ai);
        }

        public override bool IsCompleted(AIBrain ai)
        {
            if (ai.GetStateSystem<AIStateSystemWeapon>().WeaponReloadFinished(ai))
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