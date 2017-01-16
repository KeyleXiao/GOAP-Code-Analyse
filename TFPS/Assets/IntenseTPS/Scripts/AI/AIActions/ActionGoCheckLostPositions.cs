using Information;
using Sensors;
using StateSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Actions
{
    public class ActionGoCheckLostPositions : ActionGoToPosition
    {
        public int maxRandCheckPos = 3;
#if UNITY_EDITOR
        public bool showShapes = false;
#endif

        private List<InformationRandGoToCheckPosition> infoCheckPositions;
        private InformationSuspicion checkingInfo;
        private int checkPosIndex;

        public override void OnStart(AIBrain ai)
        {
            base.OnStart(ai);
            AddNeededStateSystem<AIStateSystemMove>();
            AddNeededStateSystem<AIStateSystemWeapon>();
            AddNeededSensor<SensorRandPosAroundGtor>();

            preConditions.Add(new KeyValuePair<string, object>(DS.weaponAimed, true));

            postEffects.Add(new KeyValuePair<string, object>(DS.aiAlertness, ET.AiAlertLevel.Relaxed));

            correspondingState = ET.StateType.Move;
        }

        public override void Activate(AIBrain ai)
        {
            checkPosIndex = 0;

            ai.GetStateSystem<AIStateSystemMove>().SetMoveToPositionNStartMove
                (ai, ET.MoveType.Walk, ET.MoveToType.ToPosition, infoCheckPositions[checkPosIndex].checkPosition.Value);
        }

        public override bool CanActivate(AIBrain ai)
        {
            checkingInfo = ai.Memory.Items.OfType<InformationSuspicion>().Where(x => /*!x.IsSure &&*/ !x.LastPositionChecked).OrderByDescending(x => x.UpdateTime).FirstOrDefault();

            bool retVal = false;
            if (checkingInfo != null)
                infoCheckPositions = ai.GetSensor<SensorRandPosAroundGtor>().RequestAllInfo(checkingInfo.lastKnownPosition.Value);
            if (infoCheckPositions != null)
            {
                retVal = true;
                if (infoCheckPositions.Count > maxRandCheckPos)
                    infoCheckPositions.RemoveAll(x => infoCheckPositions.IndexOf(x) >= maxRandCheckPos);
            }
            if (retVal)
                checkingInfo.IsBeingUsed = true;
            return retVal;
        }

        public override bool CanBeAddedToPlan(AIBrain ai)
        {
            return ai.Memory.Items.OfType<InformationSuspicion>().Where(x => !x.LastPositionChecked).FirstOrDefault() != null;
        }

        public override bool IsCompleted(AIBrain ai)
        {
            return checkPosIndex >= infoCheckPositions.Count - 1;
        }

        public override void OnUpdate(AIBrain ai)
        {
#if UNITY_EDITOR
            if (showShapes)
                foreach (var x in infoCheckPositions)
                    Debug.DrawRay(x.checkPosition.Value, Vector3.up * 10);
#endif

            if (checkPosIndex < infoCheckPositions.Count && ai.GetStateSystem<AIStateSystemMove>().ReachedDestination(ai, .3f))
                ai.GetStateSystem<AIStateSystemMove>().SetMoveToPosition
                    (ai, ET.MoveType.Walk, ET.MoveToType.ToPosition, infoCheckPositions[++checkPosIndex].checkPosition.Value);
        }

        public override bool IsStillValid(AIBrain ai)
        {
            if (!ai.Memory.Items.Contains(checkingInfo))
                return false;
            return true;
        }

        public override void DeActivate(AIBrain ai)
        {
            if (checkingInfo != null)
                checkingInfo.IsBeingUsed = false;
            infoCheckPositions = null;
            ai.GetStateSystem<AIStateSystemMove>().StopMoving(ai);
        }

        public override void GeneralPostEffects(AIBrain ai)
        {
            infoCheckPositions = null;
            ai.GetStateSystem<AIStateSystemMove>().StopMoving(ai);
            if (checkingInfo != null)
            {
                checkingInfo.LastPositionChecked = true;
                checkingInfo.IsBeingUsed = false;
            }
            base.GeneralPostEffects(ai);
        }

        public override bool IsInterruptableBySystems(AIBrain ai)
        {
            return true;
        }
    }
}