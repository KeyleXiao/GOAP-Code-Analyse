using StateSystems;
using UnityEngine;

namespace Actions
{
    public class ActionReloadFromSafe : ActionReload
    {
        public float addToPlanPercentage = 50;

        public override void OnStart(AIBrain ai)
        {
            base.OnStart(ai);

            AddNeededStateSystem<AIStateSystemAnimator>();
            AddNeededStateSystem<AIStateSystemMove>();
            AddNeededStateSystem<AIStateSystemWeapon>();

            preConditions.Add(DS.atSafePosition, true);
        }

        public override void Activate(AIBrain ai)
        {
            base.Activate(ai);
            ai.GetStateSystem<AIStateSystemMove>().SetTurnToPosNStartTurn(ai, ET.TurnToType.ToCurrentTarget);
        }

        public override bool CanActivate(AIBrain ai)
        {
            return true;
        }

        public override bool CanBeAddedToPlan(AIBrain ai)
        {
            return base.CanBeAddedToPlan(ai) && Random.Range(0, 100) < addToPlanPercentage && ai.HaveCurrentWeapon();
        }

        public override void GeneralPostEffects(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemMove>().StopTurning(ai);

            base.GeneralPostEffects(ai);
            ai.WorldState.SetKey(DS.atSafePosition, false);
        }

        public override void DeActivate(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemMove>().StopTurning(ai);
            base.DeActivate(ai);
            ai.WorldState.SetKey(DS.atSafePosition, false);
        }

        public override bool IsCompleted(AIBrain ai)
        {
            return base.IsCompleted(ai);
        }

        public override bool IsStillValid(AIBrain ai)
        {
            return base.IsStillValid(ai);
        }
    }
}