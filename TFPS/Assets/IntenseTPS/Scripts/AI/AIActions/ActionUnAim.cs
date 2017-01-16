using StateSystems;
using System.Collections.Generic;

namespace Actions
{
    public class ActionUnAim : AIAction
    {
        public override void OnStart(AIBrain ai)
        {
            AddNeededStateSystem<AIStateSystemWeapon>();
            AddNeededStateSystem<AIStateSystemAnimator>();
            AddNeededStateSystem<AIStateSystemMove>();

            repeatType = ET.ActionType.Once;

            preConditions.Add(new KeyValuePair<string, object>(DS.haveWeapon, true));
            preConditions.Add(new KeyValuePair<string, object>(DS.weaponArmed, true));

            postEffects.Add(new KeyValuePair<string, object>(DS.weaponAimed, false));

            correspondingState = ET.StateType.Animate;
        }

        public override bool CanActivate(AIBrain ai)
        {
            return true;
        }

        public override void Activate(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemWeapon>().UnAimWeapon(ai);
            ai.GetStateSystem<AIStateSystemMove>().StopTurning(ai);
        }

        public override void DeActivate(AIBrain ai)
        {
            base.GeneralPostEffects(ai);
        }

        public override bool IsCompleted(AIBrain ai)
        {
            return ai.GetStateSystem<AIStateSystemWeapon>().WeaponUnAimingFinished(ai);
        }

        public override void GeneralPostEffects(AIBrain ai)
        {
            base.GeneralPostEffects(ai);
            ai.GetStateSystem<AIStateSystemLookAt>().StopLooking(ai);
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