using UnityEngine;

namespace Information
{
    public abstract class InformationDanger : InformationSuspicion
    {
        public Transform dangerTransform { get; private set; }
        private string name;

        public InformationDanger
            (
            string _name,
            Transform _dangerTransform,
            Vector3 _dangerPos,
            float _dangerPosConfidence
            ) : base(_name, _dangerPos, _dangerPosConfidence)
        {
            UpdateTime = Time.time;
            dangerTransform = _dangerTransform;
            this.UpdateOverallConfidence();
            BaseTransform = dangerTransform;
        }

        public InformationDanger(InformationDanger info) : base(info)
        {
            dangerTransform = info.dangerTransform;
            UpdateOverallConfidence();
            BaseTransform = dangerTransform;
        }

        public new void Update(

            Vector3 _dangerPos,
            float _dangerPosConfidence
            )
        {
            base.Update(_dangerPos, _dangerPosConfidence);
            this.UpdateOverallConfidence();
        }

        public new void UpdateOverallConfidence()
        {
            OverallConfidence = lastKnownPosition.Confidence;
        }
    }
}