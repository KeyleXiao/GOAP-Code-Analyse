/// <summary>
/// Sets World State of <see cref="AIBrain"/> <see cref="DS.weaponLoaded"/> and <see cref="DS.haveAmmo"/> <see cref="true"/> or <see cref="false"/>
/// </summary>
public class AISystemAmmoManager : AISystem
{
    public override void OnUpdate(ref bool needToReplan, ref bool needToReevaluateGoals, AIBrain ai)
    {
        GunAtt gunAtt = ai.GetCurrentWeaponScript();
        if (gunAtt != null)
        {
            if (gunAtt.currentClipCapacity <= 0)
                ai.WorldState.SetKey(DS.weaponLoaded, false);
            else
                ai.WorldState.SetKey(DS.weaponLoaded, true);
        }
        else
        {
            ai.WorldState.SetKey(DS.weaponLoaded, true);
        }

        // AI is considered to always has ammo for now
        ai.WorldState.SetKey(DS.haveAmmo, true);
    }
}