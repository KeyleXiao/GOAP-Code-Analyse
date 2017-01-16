using UnityEngine;

namespace Actions
{
    public class ActionIdle : AIAction
    {
        private Vector2 inBetweenCost = new Vector2(.02f, .09f); // to switch between patrol and idle
        private Vector2 idleInBetweenTimer = new Vector2(4f, 7f);

        private float _tempIdleTimer;

        public override void OnStart(AIBrain ai)
        {
            Cost = UnityEngine.Random.Range(inBetweenCost.x, inBetweenCost.y);
            repeatType = ET.ActionType.Once;

            preConditions.Add(DS.weaponAimed, false);

            postEffects.Add(DS.aiStatus, ET.AiStatus.Idle);

            correspondingState = ET.StateType.Idle;
        }

        public override bool CanActivate(AIBrain ai)
        {
            return true;
        }

        public override void Activate(AIBrain ai)
        {
            _tempIdleTimer = Random.Range(idleInBetweenTimer.x, idleInBetweenTimer.y);
        }

        public override void CalculateCost(AIBrain ai)
        {
            _tempIdleTimer -= Time.deltaTime;
            Cost = UnityEngine.Random.Range(inBetweenCost.x, inBetweenCost.y);
        }

        public override void GeneralPostEffects(AIBrain ai)
        {
        }

        public override bool IsCompleted(AIBrain ai)
        {
            if (_tempIdleTimer < 0)
                return true;
            else
                return false;
        }

        public override void OnUpdate(AIBrain ai)
        {
            _tempIdleTimer -= Time.deltaTime;
        }

        public override bool IsStillValid(AIBrain ai)
        {
            return true;
        }
    }
}