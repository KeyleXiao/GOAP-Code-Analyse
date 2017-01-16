public class AIGoalIdle : AIGoal
{
    public override void OnStart(AIBrain ai)
    {
        goalStates.Add(DS.aiStatus, ET.AiStatus.Idle);
        goalStates.Add(DS.weaponArmed, false);
    }

    public override bool IsApplicapable(AIBrain ai)
    {
        return true;
    }
}