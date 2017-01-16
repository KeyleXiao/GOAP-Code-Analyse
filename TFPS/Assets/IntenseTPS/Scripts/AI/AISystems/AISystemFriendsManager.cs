/// <summary>
/// Sets World State of <see cref="AIBrain"/> <see cref="DS.haveFriendsAround"/> to <see cref="true"/>/<see cref="false"/> by looking at <see cref="AIMemory"/>
/// </summary>
public class AISystemFriendsManager : AISystem
{
    public override void OnUpdate(ref bool needToReplan, ref bool needToReevaluateGoals, AIBrain ai)
    {
        if (ai.Memory.Friends != null && ai.Memory.Friends.Count > 0)
            ai.WorldState.SetKey(DS.haveFriendsAround, true);
        else
            ai.WorldState.SetKey(DS.haveFriendsAround, false);
    }
}