using Information;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Selects/Changes target from <see cref="InformationAlive"/>'s of <see cref="AIMemory"/>, sets world state <see cref="DS.haveTarget"/> of <see cref="AIBrain"/>
/// </summary>
public class AISystemTargetSelector : AISystem
{
    public bool alwaysSelectMaxConfidenceTarget = true;
    public float targetSwitchMinInterval = 3f;
    private float targetChangedAt = -1;

    public override void OnUpdate(ref bool needToReplan, ref bool needToReevaluateGoals, AIBrain ai)
    {
        if (ai.HaveCurrentTarget())
        {
            // target is dead
            if (ai.InfoCurrentTarget.IsDead || ai.InfoCurrentTarget.transform && ai.InfoCurrentTarget.transform.GetComponent<Health>() &&
                ai.InfoCurrentTarget.transform.GetComponent<Health>().health <= 0
                 )
            {
                var tempCTarget = ai.InfoCurrentTarget;
                ai.InfoCurrentTarget = null;
                ai.WorldState.SetKey(DS.haveTarget, false);

                needToReplan = true;
                needToReevaluateGoals = true;

                tempCTarget.IsDead = true;
                ai.Memory.BroadcastToListeners(new Messages.AIMessageTargetDead(tempCTarget.BaseTransform));

                var susps = ai.Memory.Items.OfType<InformationSuspicion>().Where(x => x.BaseTransform != null && x.BaseTransform == tempCTarget.BaseTransform).ToList();
                foreach (var x in susps)
                    ai.Memory.Remove(x);
            }
            else if (!ai.InfoCurrentTarget.HaveFirePosition)
            {
                ai.InfoCurrentTarget = null;
                ai.WorldState.SetKey(DS.haveTarget, false);

                needToReplan = true;
                needToReevaluateGoals = true;
            }
        }

        // Removing current target conditions
        if (ai.InfoCurrentTarget != null)
        {
            if (ai.InfoCurrentTarget.health.Value <= 0 || !ai.InfoCurrentTarget.IsSure)
            {
                ai.InfoCurrentTarget = null;
                ai.WorldState.SetKey(DS.haveTarget, false);
                needToReplan = true;
                needToReevaluateGoals = true;
            }
        }

        List<InformationAlive> listFightables = new List<InformationAlive>(ai.Memory.Items.OfType<InformationAlive>());

        foreach (var infoFightable in listFightables)
        {
            if (infoFightable.IsDead)
            {
                var susps = ai.Memory.Items.OfType<InformationSuspicion>().Where(x => x.BaseTransform != null && x.BaseTransform == infoFightable.BaseTransform).ToList();
                foreach (var w in susps)
                    ai.Memory.Remove(w);
                //ai.Memory.BroadcastToListeners(ai.Memory.GameObject, infoFightable);

                continue;
            }
            if (infoFightable.IsSure && infoFightable.HaveFirePosition && infoFightable.lastKnownPosition.Confidence > 0 && infoFightable.health.Value > 0)
            {
                if (ai.InfoCurrentTarget == null) // there is no target, set it
                {
                    ai.InfoCurrentTarget = infoFightable;
                    ai.WorldState.SetKey(DS.haveTarget, true);

                    needToReplan = true; // replan when new target is found
                    needToReevaluateGoals = true;

                    targetChangedAt = Time.time;
                }
                else if (   // check to see if we can switch to a new target
                        alwaysSelectMaxConfidenceTarget &&
                        infoFightable.SuspicionFirm > ai.InfoCurrentTarget.SuspicionFirm &&
                        Time.time - targetChangedAt > targetSwitchMinInterval &&
                        ai.InfoCurrentTarget.transform != infoFightable.transform &&
                        infoFightable.lastKnownPosition.Confidence > 0 &&
                        infoFightable.health.Value > 0 &&
                        infoFightable.HaveFirePosition
                        )
                {
                    ai.InfoCurrentTarget = infoFightable;

                    needToReplan = true;
                    needToReevaluateGoals = true;
                    targetChangedAt = Time.time;
                }
            }
        }
    }
}