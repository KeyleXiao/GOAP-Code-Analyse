using UnityEngine;

namespace Information
{
    public class InformationDodgeSide : InformationP
    {
        public Attribute<float> angle;
        public Attribute<Vector3> positionEstimated;

        public InformationDodgeSide(
            float _angle,
            float _angleConfidence,

            Vector3 _positionEstimated,
            float _positionEstimatedConfidence

            ) : base()
        {
            angle.Set(_angle, _angleConfidence);
            positionEstimated.Set(_positionEstimated, _positionEstimatedConfidence);

            UpdateOverallConfidence();
        }

        public override void UpdateOverallConfidence()
        {
            OverallConfidence = 1 - (Mathf.Abs(Mathf.Abs(angle.Value) - 90) / 90);
        }
    }
}