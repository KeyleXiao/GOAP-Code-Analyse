using Information;
using System.Linq;

/// <summary>
/// Sets World State of <see cref="AIBrain"/> <see cref="DS.dangerExplosiveExists"/> to <see cref="true"/>/<see cref="false"/> by looking at <see cref="Information.InformationDangerExplosive"/>'s in <see cref="AIMemory"/>
/// </summary>
public class AISystemDangerExplosiveManager : AISystem
{
    public override void OnUpdate(ref bool needToReplan, ref bool needToReevaluateGoals, AIBrain ai)
    {
        var allDangers = ai.Memory.Items.OfType<InformationDangerExplosive>().ToList();

        foreach (InformationDangerExplosive infoDExp in allDangers)
        {
            if (!infoDExp.IsReacted && infoDExp.dangerTransform)
            {
                needToReevaluateGoals = true;
                ai.WorldState.SetKey(DS.dangerExplosiveExists, true);
            }
            else if (!infoDExp.dangerTransform && !infoDExp.IsBeingUsedByAction)
                ai.Memory.Remove(infoDExp);
        }

        var isThereDanger = ai.Memory.Items.OfType<InformationDangerExplosive>().Where(x => !x.IsReacted && !x.IsBeingUsedByAction).ToList().Count > 0;
        if (isThereDanger)
        {
        }
        else
            ai.WorldState.SetKey(DS.dangerExplosiveExists, false);
    }
}