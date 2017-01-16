using UnityEngine;

namespace Information
{
    public abstract class InformationP
    {
        private float overallConfidence;
        public float UpdateTime { get; protected set; }

        protected InformationP()
        {
            UpdateTime = Time.time;
        }

        public InformationP(InformationP info)
        {
            UpdateTime = info.UpdateTime;
        }

        virtual public void UpdateOverallConfidence()
        {
        }

        public float OverallConfidence
        {
            get
            {
                return overallConfidence;
            }
            protected set
            {
                value = Mathf.Clamp01(value);
                overallConfidence = value;
            }
        }
    }
}