using UnityEngine;

namespace Information
{
    public class InformationIncomingGrenade : InformationDangerExplosive
    {
        public Attribute<Vector3> direction;

        public InformationIncomingGrenade
            (
            string _name,
            Transform _dangerTransform,

            Vector3 _dangerPos,
            float _dangerPosConfidence,

            Vector3 _direction,
            float _directionConfidence,

            float _distanceFromGrenade,
            float _distanceFromGrenadeConfidence
            ) : base(_name, _dangerTransform, _dangerPos, _dangerPosConfidence, _distanceFromGrenade, _distanceFromGrenadeConfidence)
        {
            direction.Set(_direction, _directionConfidence);

            this.UpdateOverallConfidence();
        }

        public InformationIncomingGrenade(InformationIncomingGrenade info) : base(info)
        {
            direction.Set(info.direction);
            UpdateOverallConfidence();
        }

        public void Update(
            Vector3 _dangerPos,
            float _dangerPosConfidence,

            Vector3 _direction,
            float _directionConfidence,

            float _distanceFromGrenade,
            float _distanceFromGrenadeConfidence
         )
        {
            base.Update(_dangerPos, _dangerPosConfidence, _distanceFromGrenade, _distanceFromGrenadeConfidence);
            direction.Set(_direction, _directionConfidence);

            this.UpdateOverallConfidence();
        }

        public new void UpdateOverallConfidence()
        {
            OverallConfidence = direction.Confidence > distance.Confidence ? direction.Confidence * lastKnownPosition.Confidence : distance.Confidence;
        }

        public override string ToString()
        {
            return string.Format("Grenade | DangerTra: {0} | Pos: {1} | PosConf: {2:0.00} | Dir: {3} | DirConf: {4:0.00} | Dist: {5:0.0} | DistConf: {6:0.00} | OC: {7:0.00}",
                dangerTransform.ToString(), lastKnownPosition.Value, lastKnownPosition.Confidence, direction.Value, direction.Confidence, distance.Value, distance.Confidence,
                OverallConfidence
                );
        }
    }
}