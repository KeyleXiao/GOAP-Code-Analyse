using UnityEngine;

namespace Information
{
    public class InformationRandGoToCheckPosition : InformationP
    {
        public Attribute<Vector3> checkPosition;
        public Attribute<float> distFromSelf;
        public Attribute<float> distFromPoint;

        public InformationRandGoToCheckPosition(
            Vector3 _checkPosition,
            float _checkPositionConfidence,

            float _distFromSelf,
            float _distFromSelfConfidence,

            float _distFromPoint,
            float _distFromPointConfidence

            ) : base()
        {
            checkPosition.Set(_checkPosition, _checkPositionConfidence);
            distFromSelf.Set(_distFromSelf, _distFromSelfConfidence);
            distFromPoint.Set(_distFromPoint, _distFromPointConfidence);

            UpdateOverallConfidence();
        }

        public override void UpdateOverallConfidence()
        {
            OverallConfidence = (distFromSelf.Confidence + distFromPoint.Confidence) / 3;
        }
    }
}