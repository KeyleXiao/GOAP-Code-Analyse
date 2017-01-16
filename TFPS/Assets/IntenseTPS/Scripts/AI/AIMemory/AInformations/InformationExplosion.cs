using UnityEngine;

namespace Information
{
    public class InformationExplosion : InformationDangerExplosive
    {
        public InformationExplosion
            (
            string _name,
            Transform _dangerTransform,

            Vector3 _dangerPos,
            float _dangerPosConfidence,

            float _distance,
            float _distanceConfidence
            ) : base(_name, _dangerTransform, _dangerPos, _dangerPosConfidence, _distance, _distanceConfidence)
        {
            UpdateOverallConfidence();
        }

        public InformationExplosion(InformationExplosion info) : base(info)
        {
            UpdateOverallConfidence();
        }

        public new void Update(
            Vector3 _dangerPos,
            float _dangerPosConfidence,

            float _distance,
            float _distanceConfidence
         )
        {
            base.Update(_dangerPos, _dangerPosConfidence, _distance, _distanceConfidence);
            UpdateOverallConfidence();
        }

        public new void UpdateOverallConfidence()
        {
            // overall confidence of explosion is decreased with distance down to minconfidence distance
            OverallConfidence = distance.Confidence * lastKnownPosition.Confidence;
        }

        public override string ToString()
        {
            return string.Format("Explosion | DangerTra: {0} | Pos: {1} | PosConf: {2:0.00} | Dist: {3:0.0} | DistConf: {4:0.00} | OC: {5:0.00}",
                dangerTransform, lastKnownPosition.Value, lastKnownPosition.Confidence, distance.Value, distance.Confidence, OverallConfidence
                );
        }
    }
}