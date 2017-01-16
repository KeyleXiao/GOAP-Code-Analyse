using UnityEngine;

namespace Information
{
    public abstract class InformationDangerExplosive : InformationDanger
    {
        public Attribute<float> distance;

        public bool IsReacted { get; set; }
        public bool IsBeingUsedByAction { get; set; }
        private string name;

        public InformationDangerExplosive
            (
            string _name,
            Transform _dangerTransform,

            Vector3 _dangerPos,
            float _dangerPosConfidence,

            float _dangerDistance,
            float _dangerDistanceConfidence
            ) : base(_name, _dangerTransform, _dangerPos, _dangerPosConfidence)
        {
            distance.Set(_dangerDistance, _dangerDistanceConfidence);
            this.UpdateOverallConfidence();
        }

        public InformationDangerExplosive(InformationDangerExplosive info) : base(info)
        {
            distance.Set(info.distance);
            UpdateOverallConfidence();
        }

        public void Update(

            Vector3 _dangerPos,
            float _dangerPosConfidence,

            float _dangerDistance,
            float _dangerDistanceConfidence
            )
        {
            base.Update(_dangerPos, _dangerPosConfidence);
            distance.Set(_dangerDistance, _dangerDistanceConfidence);
            this.UpdateOverallConfidence();
        }

        public new void UpdateOverallConfidence()
        {
            OverallConfidence = lastKnownPosition.Confidence;
        }
    }
}