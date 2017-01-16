using Information;
using UnityEngine;

public class AIGoalReactToDangerExplosive : AIGoal
{
    public float addGoalToPlanInterval = 2.5f;

    public override void OnStart(AIBrain ai)
    {
        goalStates.Add(DS.dangerExplosiveExists, false);
    }

    public override void CalculatePriority(AIBrain ai)
    {
        var info = ai.Memory.GetHighestOverall<InformationDangerExplosive>();
        if (info != null)
            priority = priorityRange.y - (priorityRange.y - priorityRange.x) * (info.OverallConfidence);
        else
            priority = priorityRange.y;
    }

    public override bool IsApplicapable(AIBrain ai)
    {
        if (ai.WorldState.CompareKey(DS.dangerExplosiveExists, true) && Time.time - lastUsedAt > addGoalToPlanInterval)
            return true;
        return false;
    }
}