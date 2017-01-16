public class AIGoalKillTarget : AIGoal
{
    public override void OnStart(AIBrain ai)
    {
        goalStates.Add(DS.killTarget, true);
    }

    public override void CalculatePriority(AIBrain ai)
    {
        if (!ai.HaveCurrentTarget())
            priority = priorityRange.y;
        else
            priority = priorityRange.y - (priorityRange.y - priorityRange.x) * (ai.InfoCurrentTarget.OverallConfidence);
    }

    public override bool IsApplicapable(AIBrain ai)
    {
        bool canApplyToBlackboard = ai.HaveCurrentTarget();
        return canApplyToBlackboard && ai.WorldState.CompareKey(DS.haveTarget, true);
    }
}