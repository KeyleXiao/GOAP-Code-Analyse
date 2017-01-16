using UnityEngine;

/// <summary>
/// Sets World State of <see cref="AIBrain"/> <see cref="DS.isNearCurrentTarget"/> to <see cref="true"/>/<see cref="false"/> by looking at distance
/// </summary>
public class AISystemPositionTypeManager : AISystem
{
    public override void OnUpdate(ref bool needToReplan, ref bool needToReevaluateGoals, AIBrain ai)
    {
        if (Vector3.Distance(ai.GetCurrentTargetPos(), ai.Transform.position) > 1f)
            ai.WorldState.SetKey(DS.isNearCurrentTarget, false);
        else
        {
            if (ai.WorldState.CompareKey(DS.isNearCurrentTarget, false))
                needToReplan = true;
            ai.WorldState.SetKey(DS.isNearCurrentTarget, true);
        }
    }
}