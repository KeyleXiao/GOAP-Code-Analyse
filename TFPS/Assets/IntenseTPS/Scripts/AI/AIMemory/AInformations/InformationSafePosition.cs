using UnityEngine;

namespace Information
{
    public class InformationSafePosition : InformationP
    {
        public Attribute<Vector3> safePosition;
        public Attribute<float> distanceFromCurrentPosition;
        public Attribute<float> distanceFromTarget;
        public bool isBeingUsed = false;

        public InformationSafePosition(
            Vector3 _safePos,
            float _safePosConf,

            float _distanceFromCurrentPos,
            float _distanceFromCurrentPosConf,

            float _distanceFromCurrentTarget,
            float _distanceFromCurrentTargetConf
            ) : base()
        {
            safePosition.Set(_safePos, _safePosConf);
            distanceFromCurrentPosition.Set(_distanceFromCurrentPos, _distanceFromCurrentPosConf);
            distanceFromTarget.Set(_distanceFromCurrentTarget, _distanceFromCurrentTargetConf);
            UpdateOverallConfidence();
        }

        public override void UpdateOverallConfidence()
        {
            OverallConfidence = (safePosition.Confidence + distanceFromCurrentPosition.Confidence + distanceFromTarget.Confidence) / 3;
        }

        public override string ToString()
        {
            return string.Format("SafePositionConfidence = {0:0.00} | DistanceFromTargetConfidence = {1:0.00} | overallConfidence = {2:0.00}",
                safePosition.Confidence, distanceFromCurrentPosition.Confidence, OverallConfidence);
        }
    }
}