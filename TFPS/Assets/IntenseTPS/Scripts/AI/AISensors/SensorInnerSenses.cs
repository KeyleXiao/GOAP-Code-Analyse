using Information;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sensors
{
    /// <summary>
    /// Updates agent's inner senses with interval
    /// </summary>
    public class SensorInnerSenses : AISensorPolling
    {
        public float notAlertedLowerThanConf01 = .25f;
        public float alertLevelDecreaseSpeed = .03f;

        private InformationInnerAlert infoAlert;

        public override void OnStart(AIBrain ai)
        {
            infoAlert = ai.Memory.AddNReturn(new InformationInnerAlert(ET.AiAlertLevel.Relaxed, 0));
        }

        public override bool OnUpdate(AIBrain ai)
        {
            #region Alert Sensor

            List<InformationSuspicion> sureItems = ai.Memory.Items.OfType<InformationSuspicion>().Where(x => x.IsSure).ToList();
            List<InformationSuspicion> unSureItems = ai.Memory.Items.OfType<InformationSuspicion>().Where(x => !x.IsSure).ToList();
            if (sureItems.Count > 0)
            {
                if (sureItems.OfType<InformationAlive>().ToList().Count > 0)
                {
                    // if an alive info exist in memory
                    infoAlert.Update(ET.AiAlertLevel.Aware, 1);
                }
                else if (sureItems.OfType<InformationDanger>().ToList().Count > 0)
                {
                    // if a suspicion info exist in memory
                    infoAlert.Update(ET.AiAlertLevel.Alerted, 1);
                }
            }
            else if (unSureItems.Count > 0)
            {
                float sum = 0;
                foreach (var x in unSureItems)
                    sum += x.lastKnownPosition.Confidence;
                if (sum > notAlertedLowerThanConf01)
                    infoAlert.Update(ET.AiAlertLevel.Alerted, 1);
            }
            else  // Slowly decrease alert level
            {
                float alertedConf = Mathf.Clamp01(infoAlert.AlertnessLevel - DeltaTimeSinceLastWork * alertLevelDecreaseSpeed);
                infoAlert.Update(alertedConf < notAlertedLowerThanConf01 ? ET.AiAlertLevel.Relaxed : infoAlert.Alertness, alertedConf);
            }

            ai.WorldState.SetKey(DS.aiAlertness, infoAlert.AlertnessLevel);

            #endregion Alert Sensor

            return true;
        }
    }
}