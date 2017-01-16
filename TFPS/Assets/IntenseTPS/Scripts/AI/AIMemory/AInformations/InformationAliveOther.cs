using UnityEngine;

namespace Information
{
    public class InformationAliveOther : InformationAlive
    {
        public InformationAliveOther
            (
            string name,
            Transform _transform,
            Vector3 _lastKnownPosition,
            float _lastKPosConfidence,
            float _health,
            float _healthConfidence
            ) : base(name, _transform, _lastKnownPosition, _lastKPosConfidence, _health, _healthConfidence)
        {
            UpdateOverallConfidence();
        }

        public InformationAliveOther(InformationAlive info) : base(info)
        {
            UpdateOverallConfidence();
        }

        public new void Update(
         Vector3 _lastKnownPosition,
            float _lastKPosConfidence,
            float _health,
            float _healthConfidence
         )
        {
            base.Update(_lastKnownPosition, _lastKPosConfidence, _healthConfidence, _healthConfidence);
            UpdateOverallConfidence();
        }

        public new void UpdateOverallConfidence()
        {
            HaveFirePosition = true;
            base.UpdateOverallConfidence();
        }
    }
}