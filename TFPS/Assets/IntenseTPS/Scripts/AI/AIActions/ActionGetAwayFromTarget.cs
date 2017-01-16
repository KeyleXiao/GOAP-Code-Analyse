using Information;
using Sensors;
using StateSystems;
using System.Collections.Generic;
using UnityEngine;

namespace Actions
{
    public class ActionGetAwayFromTarget : ActionGoToPosition
    {
        private InformationNMCanFirePosition infoMovePos;
        public float preferredGetAwayDistance = 3f;

        public override void OnStart(AIBrain ai)
        {
            base.OnStart(ai);
            AddNeededStateSystem<AIStateSystemMove>();
            AddNeededSensor<SensorRandPosAroundGtor>();

            postEffects.Add(new KeyValuePair<string, object>(DS.isNearCurrentTarget, false));

            correspondingState = ET.StateType.Move;
        }

        public override void Activate(AIBrain ai)
        {
            GetMovePos(ai);
            ai.GetStateSystem<AIStateSystemMove>().SetMoveToPositionNStartMove
                (ai, ET.MoveType.Run, ET.MoveToType.ToPosition, infoMovePos.positionEstimated.Value);

            ai.GetStateSystem<AIStateSystemMove>().SetTurnToPosNStartTurn(ai, ET.TurnToType.ToCurrentTarget);
        }

        public override void OnUpdate(AIBrain ai)
        {
            if (ai.GetStateSystem<AIStateSystemMove>().ReachedDestination(ai))
            {
                GetMovePos(ai);
                ai.GetStateSystem<AIStateSystemMove>().SetMoveToPositionNStartMove
                (ai, ET.MoveType.Run, ET.MoveToType.ToPosition, infoMovePos.positionEstimated.Value);
            }
        }

        private void GetMovePos(AIBrain ai)
        {
            List<InformationNMCanFirePosition> infos = ai.GetSensor<SensorCanFireNMPositionFinder>().RequestAllInfo(ai);
            if (infos != null && infos.Count > 0)
            {
                float maxConf = -1;
                foreach (var info in infos)
                {
                    float distFromSelf = Vector3.Distance(ai.Transform.position, info.positionEstimated.Value);
                    float distSelfConf = 1 - Mathf.Clamp01(distFromSelf / 10);

                    float distFromTarget = Vector3.Distance(ai.GetCurrentTargetPos(), info.positionEstimated.Value);
                    float distTargetConf = Mathf.Clamp01(distFromTarget / 10);

                    float tempOveralConf = (distSelfConf + distTargetConf) / 2;
                    if (tempOveralConf > maxConf)
                    {
                        infoMovePos = info;
                        maxConf = tempOveralConf;
                    }
                }
            }
        }

        public override bool CanActivate(AIBrain ai)
        {
            return ai.HaveCurrentTarget() && ai.GetSensor<SensorCanFireNMPositionFinder>().RequestInfo(ai) != null;
        }

        public override bool CanBeAddedToPlan(AIBrain ai)
        {
            return ai.HaveCurrentTarget();
        }

        public override bool IsCompleted(AIBrain ai)
        {
            return Vector3.Distance(ai.Transform.position, ai.GetCurrentTargetPos()) > preferredGetAwayDistance;
        }

        public override bool IsStillValid(AIBrain ai)
        {
            return ai.HaveCurrentTarget();
        }

        public override void DeActivate(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemMove>().StopMoving(ai);
            ai.GetStateSystem<AIStateSystemMove>().StopTurning(ai);
        }

        public override void GeneralPostEffects(AIBrain ai)
        {
            ai.GetStateSystem<AIStateSystemMove>().StopMoving(ai);
            ai.GetStateSystem<AIStateSystemMove>().StopTurning(ai);
            base.GeneralPostEffects(ai);
        }

        public override bool IsInterruptableBySystems(AIBrain ai)
        {
            return true;
        }
    }
}