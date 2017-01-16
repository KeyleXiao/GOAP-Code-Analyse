using StateSystems;
using System.Collections.Generic;
using UnityEngine;

namespace Actions
{
    public class ActionFire : AIAction
    {
        public Vector2 randomFinishTimeBw = new Vector2(2, 6);
        private float _finishTime;

        public override void OnStart(AIBrain ai)
        {
            AddNeededStateSystem<AIStateSystemWeapon>();
            AddNeededStateSystem<AIStateSystemAnimator>();
            AddNeededStateSystem<AIStateSystemMove>();
            AddNeededStateSystem<AIStateSystemLookAt>();

            repeatType = ET.ActionType.Repetitive;

            preConditions.Add(new KeyValuePair<string, object>(DS.weaponLoaded, true));
            preConditions.Add(new KeyValuePair<string, object>(DS.weaponArmed, true));
            preConditions.Add(new KeyValuePair<string, object>(DS.haveTarget, true));
            preConditions.Add(new KeyValuePair<string, object>(DS.weaponAimed, true));
            preConditions.Add(new KeyValuePair<string, object>(DS.isNearCurrentTarget, false));

            postEffects.Add(new KeyValuePair<string, object>(DS.killTarget, true));

            correspondingState = ET.StateType.Animate;
        }

        public override void Activate(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemWeapon>().StartFiring(ai);

            ai.GetStateSystem<AIStateSystemLookAt>().SetLookAtPosNStartLook(ai, ET.LookAtType.ToCurrentTarget);

            ai.GetStateSystem<AIStateSystemMove>().SetTurnToPosNStartTurn(ai, ET.TurnToType.ToCurrentTarget);

            _finishTime = Random.Range(randomFinishTimeBw.x, randomFinishTimeBw.y);
        }

        public override void OnUpdate(AIBrain ai)
        {
            _finishTime -= Time.deltaTime;
        }

        public override bool IsStillValid(AIBrain ai)
        {
            if (!ai.HaveCurrentTarget() || !ai.GetStateSystem<AIStateSystemWeapon>().HaveAmmoOnClip(ai) || ai.WorldState.CompareKey(DS.isNearCurrentTarget, true))
                return false;

            return true;
        }

        public override bool CanBeAddedToPlan(AIBrain ai)
        {
            if (ai.HaveCurrentTarget() && ai.HaveCurrentWeapon())
                return true;
            return false;
        }

        public override bool CanActivate(AIBrain ai)
        {
            if (ai.HaveCurrentTarget() && ai.HaveCurrentWeapon())
                return true;
            return false;
        }

        public override void DeActivate(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemWeapon>().StopFiring(ai);
            ai.GetStateSystem<AIStateSystemMove>().StopTurning(ai);
            ai.GetStateSystem<AIStateSystemLookAt>().StopLooking(ai);
        }

        public override bool IsCompleted(AIBrain ai)
        {
            if (_finishTime < 0)
                return true;
            return false;
        }

        public override void GeneralPostEffects(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemWeapon>().StopFiring(ai);
            ai.GetStateSystem<AIStateSystemMove>().StopTurning(ai);
        }
    }
}