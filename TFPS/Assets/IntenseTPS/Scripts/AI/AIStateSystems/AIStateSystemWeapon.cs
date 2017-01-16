namespace StateSystems
{
    /// <summary>
    /// Class is used like an interface by <see cref="Shooter.StateSystems.AIShooterStateSystemWeapon"/>
    /// You can inherite from this class or <see cref="Shooter.StateSystems.AIShooterStateSystemWeapon"/> if necessary to use with different <see cref="MonoBehaviour"/>'s-Agent's
    /// </summary>
    public class AIStateSystemWeapon : AIStateSystem
    {
        virtual public void ReloadWeapon(AIBrain ai)
        {
        }

        virtual public void HoldWeaponWithLeftHand(AIBrain ai)
        {
        }

        virtual public void ReleaseLeftHandFromWeapon(AIBrain ai)
        {
        }

        virtual public bool WeaponReloadFinished(AIBrain ai)
        {
            return true;
        }

        virtual public void ArmWeapon(AIBrain ai)
        {
        }

        virtual public bool IsWeaponArmingFinished(AIBrain ai)
        {
            return true;
        }

        virtual public void DisArmWeapon(AIBrain ai)
        {
        }

        virtual public bool IsWeaponDisArmingFinished(AIBrain ai)
        {
            return true;
        }

        virtual public void AimWeapon(AIBrain ai, bool hipFire = false)
        {
        }

        virtual public void UnAimWeapon(AIBrain ai)
        {
        }

        virtual public void StartFiring(AIBrain ai)
        {
        }

        virtual public void StopFiring(AIBrain ai)
        {
        }

        virtual public void HitMelee(AIBrain ai)
        {
        }

        virtual public bool HaveAmmoOnClip(AIBrain ai)
        {
            return true;
        }

        virtual public bool WeaponAimingFinished(AIBrain ai)
        {
            return true;
        }

        virtual public bool WeaponUnAimingFinished(AIBrain ai)
        {
            return true;
        }

        virtual public bool IsMeleeHitEnded(AIBrain ai)
        {
            return true;
        }
    }
}