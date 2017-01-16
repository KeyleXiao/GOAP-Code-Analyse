using UnityEngine;

namespace Information
{
    public class InformationSuspicion : InformationP
    {
        #region Props

        private float suspicionFirm;
        private string name;

        public float SuspicionFirm
        {
            get { return suspicionFirm; }
            set { suspicionFirm = Mathf.Clamp01(value); }
        }

        public bool IsBeingUsed { get; set; }

        private bool isSure;

        public bool IsSure
        {
            get { return isSure; }
            set
            {
                isSure = value;
            }
        }

        private bool foundAndLost;

        public bool FoundAndLost
        {
            get { return foundAndLost; }
            set { foundAndLost = value; lostAndFound = !value; }
        }

        private bool lostAndFound;

        public bool LostAndFound
        {
            get { return lostAndFound; }
            set { lostAndFound = value; foundAndLost = !value; }
        }

        public bool LastPositionChecked { get; set; }

        public Transform BaseTransform { get; protected set; }

        #endregion Props

        public Attribute<Vector3> lastKnownPosition;

        public InformationSuspicion(string _name,
            Vector3 _lastKnownPosition,
            float _lastKPosConfidence
            ) : base()
        {
            name = _name;
            IsSure = false;
            lastKnownPosition.Set(_lastKnownPosition, _lastKPosConfidence);

            UpdateOverallConfidence();
        }

        public InformationSuspicion(InformationSuspicion info) : base(info)
        {
            name = info.name;
            IsSure = false;
            lastKnownPosition.Set(info.lastKnownPosition);
            if (info.BaseTransform)
                BaseTransform = info.BaseTransform;
            UpdateOverallConfidence();
        }

        public void Update(
            Vector3 _lastKnownPosition,
            float _lastKPosConfidence
            )
        {
            UpdateTime = Time.time;
            lastKnownPosition.Set(_lastKnownPosition, _lastKPosConfidence);

            UpdateOverallConfidence();
        }

        public new void UpdateOverallConfidence()
        {
            OverallConfidence = lastKnownPosition.Confidence;
        }

        public override string ToString()
        {
            return string.Format("\"Suspicion\"Name:{0}| LastPos:{1}, {2:0.00} | C:{3:0.00} | IsSure:{4} | Firm:{5}",
                name, lastKnownPosition.Value, lastKnownPosition.Confidence, OverallConfidence, IsSure, SuspicionFirm
                );
        }
    }
}