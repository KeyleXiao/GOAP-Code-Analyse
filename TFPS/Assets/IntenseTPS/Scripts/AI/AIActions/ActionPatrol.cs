using StateSystems;
using System.Collections.Generic;
using UnityEngine;

namespace Actions
{
    public class ActionPatrol : AIAction
    {
        public ET.PatrolType patrolType;
        private List<Vector3> patrolPoints;

        private int currentIndex = 0;
        private Vector2 inBetweenCost = new Vector2(.02f, .09f); // to switch between patrol and idle
        public float reachTolerance = .3f;

        public override void OnStart(AIBrain ai)
        {
            AddNeededStateSystem<AIStateSystemMove>();

            Cost = Random.Range(inBetweenCost.x, inBetweenCost.y);

            repeatType = ET.ActionType.Once;

            preConditions.Add(DS.weaponAimed, false);

            postEffects.Add(DS.aiStatus, ET.AiStatus.Idle);

            correspondingState = ET.StateType.Move;

            patrolPoints = ai.GetStateSystem<AIStateSystemMove>().PatrolPoints;
        }

        public override bool CanActivate(AIBrain ai)
        {
            if (patrolPoints != null && patrolPoints.Count > 1 && ai.Agent)
                return true;
            return false;
        }

        public override void Activate(AIBrain ai)
        {
            ChangePatrolPoint();
            ai.GetStateSystem<AIStateSystemMove>().SetMoveToPositionNStartMove(
                ai, ET.MoveType.Walk, ET.MoveToType.ToPosition, patrolPoints[currentIndex]);
        }

        public override bool IsCompleted(AIBrain ai)
        {
            return ai.GetStateSystem<AIStateSystemMove>().ReachedDestination(ai, reachTolerance);
        }

        public override void CalculateCost(AIBrain ai)
        {
            if (patrolPoints != null && patrolPoints.Count > 0 && Vector3.Distance(patrolPoints[0], ai.Transform.position) > 25f)
                Cost = .01f;
            else
                Cost = Random.Range(inBetweenCost.x, inBetweenCost.y);
        }

        public override void GeneralPostEffects(AIBrain ai)
        {
        }

        public override bool IsStillValid(AIBrain ai)
        {
            return true;
        }

        public void ChangePatrolPoint()
        {
            int index = 0;
            switch (patrolType)
            {
                case ET.PatrolType.Sequenced:
                    index = (currentIndex + 1) % patrolPoints.Count;
                    break;

                case ET.PatrolType.Random:
                    index = Random.Range(0, patrolPoints.Count);
                    break;

                default:
                    break;
            }
            if (index == currentIndex)
                ChangePatrolPoint();
            else
                currentIndex = index;
        }
    }
}