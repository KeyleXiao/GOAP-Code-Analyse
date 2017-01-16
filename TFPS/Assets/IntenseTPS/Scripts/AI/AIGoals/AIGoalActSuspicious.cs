using Information;
using System.Linq;

public class AIGoalActSuspicious : AIGoal
{
    public override void OnStart(AIBrain ai)
    {
        goalStates.Add(DS.aiAlertness, ET.AiAlertLevel.Relaxed);
    }

    public override void CalculatePriority(AIBrain ai)
    {
        if (ai.Memory.Items.OfType<InformationSuspicion>().Where(x => !x.LastPositionChecked).FirstOrDefault() != null)
            priority = priorityRange.x;
        else
            priority = priorityRange.y;
    }

    public override bool IsApplicapable(AIBrain ai)
    {
        if (!ai.WorldState.CompareKey(DS.aiAlertness, ET.AiAlertLevel.Relaxed) &&
            ai.Memory.Items.OfType<InformationSuspicion>().Where(x => !x.LastPositionChecked).FirstOrDefault() != null
            )
            return true;
        return false;
    }
}