using Information;
using StateSystems;
using UnityEngine;

namespace Actions
{
    public class ActionObserveDangerExplosive : AIAction
    {
        public Vector2 randLookNormalizeTime01 = new Vector2(.7f, 1f);
        public float minDistanceFromDangerToSetApplicapable = 8; // prevent observing near dangers-let another action take care of it

        private InformationDangerExplosive infoDangerEx;
        private float _tempFinNormalizeTime;
        private float _angle;

        public override void OnStart(AIBrain ai)
        {
            AddNeededStateSystem<AIStateSystemLookAt>();
            AddNeededStateSystem<AIStateSystemAnimator>();

            repeatType = ET.ActionType.Once;

            preConditions.Add(DS.dangerExplosiveExists, true);
            preConditions.Add(DS.weaponAimed, false);

            postEffects.Add(DS.dangerExplosiveExists, false);

            correspondingState = ET.StateType.Animate;
        }

        public override bool CanActivate(AIBrain ai)
        {
            infoDangerEx = ai.Memory.GetHighestOverall<InformationDangerExplosive>();

            if (infoDangerEx != null)
                return true;
            return false;
        }

        public override void Activate(AIBrain ai)
        {
            infoDangerEx.IsBeingUsedByAction = true;

            ai.GetStateSystem<AIStateSystemLookAt>().SetLookAtPosNStartLook(ai, infoDangerEx.lastKnownPosition.Value, ET.LookAtType.ToPosition);

            ai.GetStateSystem<AIStateSystemAnimator>().AnimateBool(ai, "LookPanicked", true, false, false, "", "Locomotion", 0);

            ai.GetStateSystem<AIStateSystemAnimator>().DisableLayer(ai, 1, false, false);

            _tempFinNormalizeTime = Random.Range(randLookNormalizeTime01.x, randLookNormalizeTime01.y);
        }

        public override bool CanBeAddedToPlan(AIBrain ai)
        {
            infoDangerEx = ai.Memory.GetHighestOverall<InformationDangerExplosive>();

            if (infoDangerEx != null && Vector3.Distance(infoDangerEx.lastKnownPosition.Value, ai.Transform.position) > minDistanceFromDangerToSetApplicapable)
                return true;
            return false;
        }

        public override void CalculateCost(AIBrain ai)
        {
            Cost = infoDangerEx.distance.Confidence;
        }

        public override void GeneralPostEffects(AIBrain ai)
        {
            base.GeneralPostEffects(ai);
            infoDangerEx.IsBeingUsedByAction = false;
            infoDangerEx.IsReacted = true;
            ai.GetStateSystem<AIStateSystemLookAt>().StopLooking(ai);
        }

        public override void DeActivate(AIBrain ai)
        {
            infoDangerEx.IsBeingUsedByAction = false;
            ai.GetStateSystem<AIStateSystemLookAt>().StopLooking(ai);
        }

        public override bool IsCompleted(AIBrain ai)
        {
            if (ai.stateSystemAnimator.IsStartedAnimationFinished("", "Locomotion"))
            {
                return true;
            }
            return false;
        }

        public override void OnUpdate(AIBrain ai)
        {
            Vector3 aiToDangerNoY = Vector3.zero;
            if (!infoDangerEx.dangerTransform)
                aiToDangerNoY = (-ai.Transform.position + new Vector3(infoDangerEx.lastKnownPosition.Value.x, ai.Transform.position.y, infoDangerEx.lastKnownPosition.Value.z)).normalized;
            else
                aiToDangerNoY = (-ai.Transform.position + new Vector3(infoDangerEx.dangerTransform.position.x, infoDangerEx.dangerTransform.position.y, infoDangerEx.dangerTransform.position.z)).normalized;
            _angle = Mathf.Sign(Vector3.Dot(aiToDangerNoY, ai.Transform.right)) * Vector3.Angle(aiToDangerNoY, ai.Transform.forward);

            ai.GetStateSystem<AIStateSystemAnimator>().AnimateFloat(ai, "Angle", _angle, false, true, "", "Locomotion");
            _tempFinNormalizeTime -= Time.deltaTime;
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