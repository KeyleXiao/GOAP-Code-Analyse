using StateSystems;
using UnityEngine;

namespace Actions
{
    public class ActionAimWeapon : AIAction
    {
        // ignore these if hipfireAim is not used
        public Vector2 costInBetween = new Vector2(1, 3);

        public float minCostAtDistance = 25f;

        public override void OnStart(AIBrain ai)
        {
            AddNeededStateSystem<AIStateSystemWeapon>();
            AddNeededStateSystem<AIStateSystemLookAt>();

            repeatType = ET.ActionType.Once;

            preConditions.Add(DS.haveWeapon, true);
            preConditions.Add(DS.weaponArmed, true);
            preConditions.Add(DS.weaponLoaded, true);

            postEffects.Add(DS.weaponAimed, true);

            correspondingState = ET.StateType.Animate;
        }

        public override void Activate(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemWeapon>().AimWeapon(ai, false);
            ai.GetStateSystem<AIStateSystemLookAt>().SetLookAtPosNStartLook(ai, ET.LookAtType.Forward);
        }

        public override void CalculateCost(AIBrain ai)
        {
            if (ai.HaveCurrentTarget())
            {
                float distFromTarget = ai.GetDistanceFromCurrentTarget();
                Cost = Mathf.Max(costInBetween.x, costInBetween.y - (Mathf.Clamp01(distFromTarget / minCostAtDistance) * (costInBetween.y - costInBetween.x)));
            }
            else
                Cost = .5f;
        }

        public override bool CanActivate(AIBrain ai)
        {
            if (ai.HaveCurrentWeapon())
                return true;
            return false;
        }

        public override bool CanBeAddedToPlan(AIBrain ai)
        {
            if (ai.HaveCurrentWeapon())
                return true;
            return false;
        }

        public override void DeActivate(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemWeapon>().UnAimWeapon(ai);
        }

        public override bool IsCompleted(AIBrain ai)
        {
            if (ai.GetStateSystem<AIStateSystemWeapon>().WeaponAimingFinished(ai))
                return true;
            return false;
        }

        public override void GeneralPostEffects(AIBrain ai)
        {
            base.GeneralPostEffects(ai);
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