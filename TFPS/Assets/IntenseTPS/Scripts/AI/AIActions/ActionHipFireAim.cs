using StateSystems;
using UnityEngine;

namespace Actions
{
    public class ActionHipFireAim : ActionAimWeapon
    {
        private float addToPlanChancePercentage = 80; // %

        public override void Activate(AIBrain ai)
        {
            base.Activate(ai);
            ai.GetStateSystem<AIStateSystemWeapon>().AimWeapon(ai, true);
        }

        public override bool CanBeAddedToPlan(AIBrain ai)
        {
            if (Random.Range(0, 100) < addToPlanChancePercentage && base.CanBeAddedToPlan(ai))
                return true;
            return false;
        }

        public override void CalculateCost(AIBrain ai)
        {
            float distFromTarget = ai.GetDistanceFromCurrentTarget();
            Cost = Mathf.Max(costInBetween.x, costInBetween.y - (Mathf.Clamp01(1 - distFromTarget / minCostAtDistance) * (costInBetween.y - costInBetween.x)));
        }
    }
}