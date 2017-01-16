using UnityEngine;

namespace Information
{
    public class InformationNMCanFirePosition : InformationP
    {
        public Attribute<float> angle;
        public Attribute<Vector3> positionEstimated;

        public InformationNMCanFirePosition(
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