namespace Player
{
    /// <summary>
    /// Listen <see cref="WeaponEvents"/> using <see cref="WeaponCSMB.Events"/>
    /// </summary>
    public class WeaponEvents
    {
        #region Delegates

        public delegate void DoubleParamHandler(GunAtt oldWeapon, GunAtt newWeapon);

        public delegate void SingleGunAttParamHandler(GunAtt gunAtt);

        public delegate void CollectHandler();

        #endregion Delegates

        public event DoubleParamHandler onWeaponSwitch;

        public event SingleGunAttParamHandler onWeaponPullOut;

        public event SingleGunAttParamHandler onWeaponHolster;

        public event SingleGunAttParamHandler onWeaponHipFireAim;

        public event SingleGunAttParamHandler onWeaponSightAim;

        public event SingleGunAttParamHandler onWeaponAim;

        public event SingleGunAttParamHandler onWeaponUnAim;

        public event SingleGunAttParamHandler onWeaponFire;

        public event SingleGunAttParamHandler onReloadDone;

        public event DoubleParamHandler onDropWeapon;

        public event SingleGunAttParamHandler onWeaponCollect;

        public event CollectHandler onSupplyCollect;

        #region Invoke Events

        public void InvokeSupplyCollect()
        {
            if (onSupplyCollect != null)
                onSupplyCollect();
        }

        public void InvokeWeaponCollect(GunAtt gunAtt)
        {
            if (onWeaponCollect != null)
                onWeaponCollect(gunAtt);
        }

        public void InvokeWeaponDrop(GunAtt droppedWeapon, GunAtt nextWeapon)
        {
            if (onDropWeapon != null)
                onDropWeapon(droppedWeapon, nextWeapon);
        }

        public void InvokeWeaponSwitch(GunAtt oldWeapon, GunAtt newWeapon)
        {
            if (onWeaponSwitch != null)
                onWeaponSwitch(oldWeapon, newWeapon);
        }

        public void InvokeWeaponPullOut(GunAtt gunAtt)
        {
            if (onWeaponPullOut != null)
                onWeaponPullOut(gunAtt);
        }

        public void InvokeWeaponHolster(GunAtt gunAtt)
        {
            if (onWeaponHolster != null)
                onWeaponHolster(gunAtt);
        }

        public void InvokeWeaponHipFireAim(GunAtt gunAtt)
        {
            if (onWeaponHipFireAim != null)
                onWeaponHipFireAim(gunAtt);
            if (onWeaponAim != null)
                onWeaponAim(gunAtt);
        }

        public void InvokeWeaponSightAim(GunAtt gunAtt)
        {
            if (onWeaponSightAim != null)
                onWeaponSightAim(gunAtt);
            if (onWeaponAim != null)
                onWeaponAim(gunAtt);
        }

        public void InvokeWeaponUnAim(GunAtt gunAtt)
        {
            if (onWeaponUnAim != null)
                onWeaponUnAim(gunAtt);
        }

        public void InvokeWeaponFire(GunAtt gunAtt)
        {
            if (onWeaponFire != null)
                onWeaponFire(gunAtt);
        }

        public void InvokeWeaponReloadDone(GunAtt gunAtt)
        {
            if (onReloadDone != null)
                onReloadDone(gunAtt);
        }

        #endregion Invoke Events
    }
}