using Information;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages <see cref="InformationSafePosition"/>'s of <see cref="AIMemory"/>, Modifies <see cref="DS.haveSafePosition"/> world state key
/// </summary>
public class AISystemSafePositionManager : AISystem
{
    public int maxSafePositionCountInMemory = 15;
    public float removeTime = 3f;
    public float safePositionReachDistance = .3f;

    public override void OnUpdate(ref bool needToReplan, ref bool needToReevaluateGoals, AIBrain ai)
    {
        List<InformationSafePosition> infoSafePositions = ai.Memory.Items.OfType<InformationSafePosition>().ToList();
        foreach (InformationSafePosition info in infoSafePositions)
        {
            if (Time.time - info.UpdateTime > removeTime && !info.isBeingUsed)
            {
                ai.Memory.Remove(info);
            }
        }
        infoSafePositions.RemoveAll(info => ((Time.time - info.UpdateTime > removeTime) && !info.isBeingUsed));

        infoSafePositions = infoSafePositions.OrderBy(x => x.OverallConfidence).ToList();
        infoSafePositions.RemoveAll(x => x.isBeingUsed);
        if (infoSafePositions.Count > maxSafePositionCountInMemory)
        {
            int removeCount = infoSafePositions.Count - maxSafePositionCountInMemory;
            for (int i = 0; i < removeCount; i++)
            {
                if (!infoSafePositions[i].isBeingUsed)
                {
                    ai.Memory.Remove(infoSafePositions[i]);
                }
                else
                    i--;
            }
        }

        if (ai.Memory.MemoryContainsObjectOfType(typeof(InformationSafePosition)))
            ai.WorldState.SetKey(DS.haveSafePosition, true);
        else
            ai.WorldState.SetKey(DS.haveSafePosition, false);
    }
}