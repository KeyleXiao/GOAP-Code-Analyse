using UnityEngine;

namespace Information
{
    public class InformationReceivedDamageBulletToHumanoid : InformationReceivedDamage
    {
        public Attribute<Vector3> direction;
        public Attribute<float> force;
        private Transform bodyPartTransform;

        public InformationReceivedDamageBulletToHumanoid(
            float _damage,
            float _damageConfidence,

            Vector3 _direction,
            float _directionConfidence,

            float _force,
            float _forceConfidence,

            Transform _bodyPartTransform
            ) : base(_damage, _damageConfidence)
        {
            direction.Set(_direction, _directionConfidence);
            force.Set(_force, _forceConfidence);
            bodyPartTransform = _bodyPartTransform;
            UpdateOverallConfidence();
        }

        public override void UpdateOverallConfidence()
        {
            OverallConfidence = damage.Confidence;
        }

        public override string ToString()
        {
            return string.Format("Damage :{0:0.0} | BodyPartTransform :{1} | OverallC :{2}",
                damage.Value, bodyPartTransform.name, OverallConfidence
                );
        }
    }
}