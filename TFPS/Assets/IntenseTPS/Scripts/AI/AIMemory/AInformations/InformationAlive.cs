using UnityEngine;

namespace Information
{
    public abstract class InformationAlive : InformationSuspicion
    {
        public Attribute<float> health;
        public Transform transform;

        private bool isDead;

        public bool IsDead
        {
            get { return isDead; }
            set { isDead = value; UpdateTime = Time.time; }
        }

        public bool HaveFirePosition { get; protected set; }

        public InformationAlive
            (
            string name,
            Transform _transform,
            Vector3 _lastKnownPosition,
            float _lastKPosConfidence,
            float _health,
            float _healthConfidence
            ) : base(name, _lastKnownPosition, _lastKPosConfidence)
        {
            transform = _transform;
            health.Set(_health, _healthConfidence);
            UpdateOverallConfidence();
            BaseTransform = transform;
        }

        public InformationAlive(InformationAlive info) : base(info)
        {
            transform = info.transform;
            health.Set(info.health);
            UpdateOverallConfidence();
            BaseTransform = info.BaseTransform;
        }

        public void Update(
         Vector3 _lastKnownPosition,
            float _lastKPosConfidence,
            float _health,
            float _healthConfidence
         )
        {
            base.Update(_lastKnownPosition, _lastKPosConfidence);
            health.Set(_health, _healthConfidence);
            UpdateOverallConfidence();
        }

        public new void UpdateOverallConfidence()
        {
            HaveFirePosition = true;
            base.UpdateOverallConfidence();
        }

        virtual public Vector3 GetFireToPosition()
        {
            return lastKnownPosition.Value;
        }
    }
}