using Information;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages <see cref="InformationP"/>'s of type <see cref="InformationSuspicion"/> with derived types.
/// </summary>
public class AISystemSuspicionManager : AISystem
{
    public float firm01ToBeSure = .95f;
    public float firm01NotToBeSure = .1f;
    public float startToLoseTimeAfterInformationUpdate = 1f;
    public float loseSpeedMultiplier = .8f;
    public float upSpeedMultiplier = 2f;

    public int maxUnsureSuspCount = 5;
    public float unsureForgetTime = 60;

    public override void OnUpdate(ref bool needToReplan, ref bool needToReevaluateGoals, AIBrain ai)
    {
        List<InformationSuspicion> suspections = new List<InformationSuspicion>(ai.Memory.Items.OfType<InformationSuspicion>());
        foreach (var infoSuspicion in suspections)
        {
            // losing target if not seen for 'startToLoseTimeAfterUpdate' seconds
            if (Time.time - infoSuspicion.UpdateTime > startToLoseTimeAfterInformationUpdate)
            {
                infoSuspicion.SuspicionFirm -= DeltaTime * loseSpeedMultiplier;
            }
            // confidence going up with overall percentage ratio
            else
            {
                infoSuspicion.SuspicionFirm += DeltaTime * infoSuspicion.OverallConfidence * upSpeedMultiplier;
            }
            infoSuspicion.SuspicionFirm = Mathf.Clamp01(infoSuspicion.SuspicionFirm);

            if (infoSuspicion.SuspicionFirm < firm01NotToBeSure && infoSuspicion.IsSure)
            {
                if (infoSuspicion.IsSure)
                {
                    infoSuspicion.FoundAndLost = true;
                    if (infoSuspicion.BaseTransform && infoSuspicion.GetType() == typeof(InformationAlive))
                        ai.Memory.BroadcastToListeners(new Messages.AIMessageSuspicionLost(infoSuspicion.BaseTransform));
                }
                infoSuspicion.IsSure = false;
            }
            else if (infoSuspicion.SuspicionFirm > firm01ToBeSure && !infoSuspicion.IsSure)
            {
                if (!infoSuspicion.IsSure)
                {
                    infoSuspicion.LastPositionChecked = false;
                    infoSuspicion.LostAndFound = true;
                    if (infoSuspicion.BaseTransform)
                        ai.Memory.BroadcastToListeners(new Messages.AIMessageSuspicionFound(infoSuspicion));
                }
                infoSuspicion.IsSure = true;
            }
        }

        var unsureSusps = ai.Memory.Items.OfType<InformationSuspicion>().Where(x => !x.IsSure).OrderByDescending(x => x.UpdateTime).ToList();
        for (int i = 0; i < unsureSusps.Count; i++)
            if (Time.time - unsureSusps[i].UpdateTime > unsureForgetTime && !unsureSusps[i].IsBeingUsed)
            {
                ai.Memory.Remove(unsureSusps[i]);
                unsureSusps.RemoveAt(i);
            }

        if (unsureSusps.Count > maxUnsureSuspCount)
        {
            int removeCount = unsureSusps.Count - maxUnsureSuspCount;
            for (int i = 0; i < removeCount; i++)
            {
                if (!unsureSusps[i].IsBeingUsed)
                    ai.Memory.Remove(unsureSusps[i]);
            }
        }
    }
}