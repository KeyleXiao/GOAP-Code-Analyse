using StateSystems;
using UnityEngine;

namespace Actions
{
    public class ActionAttackMeleeGun : AIAction
    {
        public float minTimeIntervalToAddToPlan = 5f;
        public float distanceToSetLowCost = 2f;
        public float lowCost = .3f;

        public override void OnStart(AIBrain ai)
        {
            AddNeededStateSystem<AIStateSystemAnimator>();
            AddNeededStateSystem<AIStateSystemWeapon>();

            repeatType = ET.ActionType.Once;

            preConditions.Add(DS.isNearCurrentTarget, true);
            preConditions.Add(DS.weaponArmed, true);

            postEffects.Add(DS.killTarget, true);

            correspondingState = ET.StateType.Animate;
        }

        public override void Activate(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemWeapon>().HitMelee(ai);
            ai.GetStateSystem<AIStateSystemAnimator>().DisableLayer(ai, 1, false, false);
        }

        public override void CalculateCost(AIBrain ai)
        {
            if (ai.HaveCurrentTarget())
            {
                float distance = Vector3.Distance(ai.Transform.position, ai.GetCurrentTargetPos());
                if (distance < distanceToSetLowCost)
                {
                    Cost = lowCost;
                }
                else
                    Cost = 10f;
            }
            else
                Cost = 10f;
        }

        public override bool CanActivate(AIBrain ai)
        {
            if (ai.HaveCurrentTarget() && ai.WorldState.CompareKey(DS.isNearCurrentTarget, true))
                return true;
            return false;
        }

        public override bool CanBeAddedToPlan(AIBrain ai)
        {
            if (ai.HaveCurrentTarget() && Time.time - LastUsedAt > minTimeIntervalToAddToPlan)
                return true;
            return false;
        }

        public override void DeActivate(AIBrain ai)
        {
            ai.WorldState.SetKey(DS.isNearCurrentTarget, false);
        }

        public override void GeneralPostEffects(AIBrain ai)
        {
            ai.WorldState.SetKey(DS.isNearCurrentTarget, false);
        }

        public override bool IsCompleted(AIBrain ai)
        {
            return ai.GetStateSystem<AIStateSystemWeapon>().IsMeleeHitEnded(ai);
        }

        public override bool IsStillValid(AIBrain ai)
        {
            if (ai.HaveCurrentTarget())
                return true;
            return false;
        }

        public override bool IsInterruptableBySystems(AIBrain ai)
        {
            return false;
        }
    }
}