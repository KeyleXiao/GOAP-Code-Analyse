using UnityEngine;

namespace Information
{
    public class InformationInnerAlert : InformationP
    {
        public ET.AiAlertLevel Alertness { get; private set; }
        public float AlertnessLevel { get; private set; }
        private string name;

        public bool IsAlerted
        {
            get
            {
                return ((Alertness == ET.AiAlertLevel.Alerted || Alertness == ET.AiAlertLevel.Aware) && AlertnessLevel > 0);
            }
        }

        public InformationInnerAlert(
            ET.AiAlertLevel _alertness, float _alertnessLevel
            ) : base()
        {
            name = "Inner Alertness";
            Alertness = _alertness;
            AlertnessLevel = _alertnessLevel;

            this.UpdateOverallConfidence();
        }

        public void Update(ET.AiAlertLevel _alertness, float _alertnessLevel)
        {
            UpdateTime = Time.time;
            Alertness = _alertness;
            AlertnessLevel = _alertnessLevel;
        }

        public new void UpdateOverallConfidence()
        {
            OverallConfidence = Alertness == ET.AiAlertLevel.Relaxed ? 0 : 1;
        }

        public override string ToString()
        {
            return string.Format("{0} | Alert: {1} | AlertLevel: {2:0.00}",
                name, Alertness, AlertnessLevel
                );
        }
    }
}