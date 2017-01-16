using Shooter.StateSystems;
using UnityEngine;

/// <summary>
/// Manages weapon spread
/// </summary>
public class AiTargetLogic : MonoBehaviour
{
    private Vector3 defLocalPos;

    [System.NonSerialized]
    public AIBrain ai;

    [System.NonSerialized]
    public AIShooterStateSystemWeapon ssw;

    [System.NonSerialized]
    public AIShooterStateSystemLookAt ssl;

    private bool immutedSignX = false;
    private bool immutedSignY = false;
    private float immutedTargetX, immutedTargetY;
    private Vector2 cImmuted;
    private float eps = .02f;

    private void Start()
    {
        defLocalPos = transform.localPosition;
    }

    private void Update()
    {
        if (ai == null || ssw == null || ssl == null)
            return;

        GunAtt gunAtt = ai.GetCurrentWeaponScript();
        if (gunAtt)
        {
            if (Mathf.Abs(immutedTargetX - cImmuted.x) < eps)
            {
                immutedTargetX = (immutedSignX ? -1 : 1) * Random.Range(0, ssw.WHandIKProps.immutedWeaponSpreadAgentMultiplier.x * gunAtt.immutedSpreadMax.x);
                immutedSignX = !immutedSignX;
            }
            if (Mathf.Abs(immutedTargetY - cImmuted.y) < eps)
            {
                immutedTargetY = (immutedSignY ? -1 : 1) * Random.Range(0, ssw.WHandIKProps.immutedWeaponSpreadAgentMultiplier.y * gunAtt.immutedSpreadMax.y);
                immutedSignY = !immutedSignY;
            }
            Vector2 immutedTarget = new Vector2(immutedTargetX, immutedTargetY);
            cImmuted = Vector2.Lerp(cImmuted, immutedTarget, Time.deltaTime * ssw.WHandIKProps.immutedSpreadChangeSpeed);

            transform.localPosition = Vector3.Lerp(transform.localPosition, defLocalPos + new Vector3(cImmuted.x, cImmuted.y, 0), gunAtt.spreadRecoverSpeed * ssw.WHandIKProps.weaponSpreadRecoverAgentMultiplier);
            ssw.weaponBodyBob = Mathf.Lerp(ssw.weaponBodyBob, 0, gunAtt.bodyRecoverSpeedInverse * ssl.LookIKProps.weaponBodyRecoverSpeedMultiplier * Time.deltaTime);
        }
    }
}