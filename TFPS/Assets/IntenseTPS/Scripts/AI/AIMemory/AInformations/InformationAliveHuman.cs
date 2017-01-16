using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Information
{
    public class InformationAliveHuman : InformationAlive
    {
        //public List<Vector3> visibleBoneFirePositions;
        public List<Attribute<Vector3>> visibleBoneFirePositions { get; private set; }

        public InformationAliveHuman
            (
            string name,
            Transform _transform,
            Vector3 _lastKnownPosition,
            float _lastKPosConfidence,
            float _health,
            float _healthConfidence,
            List<Attribute<Vector3>> _visibleFirePositions
            ) : base(name, _transform, _lastKnownPosition, _lastKPosConfidence, _health, _healthConfidence)
        {
            visibleBoneFirePositions = _visibleFirePositions;
            UpdateOverallConfidence();
        }

        public InformationAliveHuman(InformationAliveHuman info) : base(info)
        {
            visibleBoneFirePositions = new List<Attribute<Vector3>>();
            UpdateOverallConfidence();
        }

        public void Update(
         Vector3 _lastKnownPosition,
            float _lastKPosConfidence,
            float _health,
            float _healthConfidence,
         List<Attribute<Vector3>> _visibleFirePositions
         )
        {
            base.Update(_lastKnownPosition, _lastKPosConfidence, _health, _healthConfidence);
            visibleBoneFirePositions = _visibleFirePositions;
            HaveFirePosition = visibleBoneFirePositions != null && visibleBoneFirePositions.Count > 0;
            UpdateOverallConfidence();
        }

        public new void UpdateOverallConfidence()
        {
            base.UpdateOverallConfidence();
            OverallConfidence = OverallConfidence * ((visibleBoneFirePositions != null) ? (visibleBoneFirePositions.Count > 0 ? 1 : 0) : 1);
        }

        public override Vector3 GetFireToPosition()
        {
            if (visibleBoneFirePositions.Count > 0)
            {
                return visibleBoneFirePositions.OrderByDescending(x => x.Confidence).FirstOrDefault().Value;
            }
            else
                return Vector3.zero;
        }
    }
}