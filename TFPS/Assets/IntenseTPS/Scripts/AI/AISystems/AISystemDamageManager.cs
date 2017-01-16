using Information;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Sets World State of <see cref="AIBrain"/> <see cref="DS.takingBulletDamage"/> to <see cref="true"/>/<see cref="false"/> by looking at <see cref="Information.InformationReceivedDamageBulletToHumanoid"/>'s in <see cref="AIMemory"/>
/// </summary>
public class AISystemDamageManager : AISystem
{
    private float _humanoidBulletDamageSens = 0;

    public float humanoidDamageSensMultiplier = .3f;
    public float bulletToHumanoidLoseSpeed = .2f;

    [Range(0, 1)]
    public float takingBulletDamageAtMinSens = .01f;

    public float OverallDamageToHumanoidConf { get; private set; }

    public override void OnStart(AIBrain ai)
    {
    }

    public override void OnUpdate(ref bool needToReplan, ref bool needToReevaluateGoals, AIBrain ai)
    {
        List<InformationReceivedDamageBulletToHumanoid> infoRDBHs = ai.Memory.Items.OfType<InformationReceivedDamageBulletToHumanoid>().ToList();
        foreach (InformationReceivedDamageBulletToHumanoid info in infoRDBHs)
        {
            ai.Memory.Remove(info);
            _humanoidBulletDamageSens += info.damage.Confidence * humanoidDamageSensMultiplier;
        }
        _humanoidBulletDamageSens -= DeltaTime * bulletToHumanoidLoseSpeed;
        _humanoidBulletDamageSens = Mathf.Clamp01(_humanoidBulletDamageSens);
        OverallDamageToHumanoidConf = _humanoidBulletDamageSens;

        if (_humanoidBulletDamageSens > takingBulletDamageAtMinSens)
        {
            ai.WorldState.SetKey(DS.takingBulletDamage, true);
            ai.WorldState.SetKey(DS.bulletDodged, false);
            needToReevaluateGoals = true;
            needToReplan = true;
        }
        else
        {
            ai.WorldState.SetKey(DS.takingBulletDamage, false);
        }
    }
}