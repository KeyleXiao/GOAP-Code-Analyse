using Information;
using Sensors;
using UnityEngine;

public class AIGoalDodgeBullet : AIGoal
{
    public float minIntervalToSetApplicapable = 2.5f;
    public float activatablePercentage = 80; // %

    public override void OnStart(AIBrain ai)
    {
        goalStates.Add(DS.bulletDodged, true);
    }

    public override void CalculatePriority(AIBrain ai)
    {
        if (ai.GetSystem<AISystemDamageManager>() != null)
            priority = priorityRange.y - (priorityRange.y - priorityRange.x) * (ai.GetSystem<AISystemDamageManager>().OverallDamageToHumanoidConf);
        else
            priority = priorityRange.y;
    }

    public override bool IsApplicapable(AIBrain ai)
    {
        bool applyChance = false;
        if (Time.time - lastUsedAt > minIntervalToSetApplicapable && UnityEngine.Random.Range(0, 100) < activatablePercentage)
            applyChance = true;

        bool retval = ai.WorldState.CompareKey(DS.haveTarget, true) &&
             ai.WorldState.CompareKey(DS.takingBulletDamage, true) &&
             ai.HaveCurrentTarget() && ai.GetSensor<SensorDodgeSideFinder>().RequestInfo<InformationDodgeSide>(ai) != null && applyChance;

        if (retval)
            lastUsedAt = Time.time;
        return retval;
    }
}