namespace Player.Triggers
{
    public class WeaponSystemTriggers : SystemTrigger
    {
        public const string ct_PullOut = "Pull Out";
        public const string ct_Holster = "Holster";
        public const string ct_SwitchWeapon = "Switch Weapon";
        public const string ct_AimWeapon = "Aim Weapon";
        public const string ct_Reload = "Reload Weapon";
        public const string ct_Drop = "Drop Weapon";
        public const string ct_Collect = "Collect Weapon";
        public const string ct_Modify = "Modify Weapon";
        public const string ct_Flashlight = "Flash light";

        public WeaponSystemTriggers()
        {
            Triggers.Add(ct_PullOut, true);
            Triggers.Add(ct_Holster, true);
            Triggers.Add(ct_SwitchWeapon, true);
            Triggers.Add(ct_AimWeapon, true);
            Triggers.Add(ct_Reload, true);
            Triggers.Add(ct_Drop, true);
            Triggers.Add(ct_Collect, true);
            Triggers.Add(ct_Modify, true);
            Triggers.Add(ct_Flashlight, true);
        }

        public WeaponSystemTriggers(bool _ct_PullOut, bool _ct_holster, bool _ct_switchWeapon,
            bool _ct_aimWeapon, bool _ct_ReloadWeapon, bool _ct_Drop, bool _ct_WeaponCollect, bool _ct_Modify, bool _ct_Flashlight)
        {
            Triggers.Add(ct_PullOut, _ct_PullOut);
            Triggers.Add(ct_Holster, _ct_holster);
            Triggers.Add(ct_SwitchWeapon, _ct_switchWeapon);
            Triggers.Add(ct_AimWeapon, _ct_aimWeapon);
            Triggers.Add(ct_Reload, _ct_ReloadWeapon);
            Triggers.Add(ct_Drop, _ct_Drop);
            Triggers.Add(ct_Collect, _ct_WeaponCollect);
            Triggers.Add(ct_Modify, _ct_Modify);
            Triggers.Add(ct_Flashlight, _ct_Flashlight);
        }

        public WeaponSystemTriggers(bool setAllTo)
        {
            Triggers.Add(ct_PullOut, setAllTo);
            Triggers.Add(ct_Holster, setAllTo);
            Triggers.Add(ct_SwitchWeapon, setAllTo);
            Triggers.Add(ct_AimWeapon, setAllTo);
            Triggers.Add(ct_Reload, setAllTo);
            Triggers.Add(ct_Drop, setAllTo);
            Triggers.Add(ct_Collect, setAllTo);
            Triggers.Add(ct_Modify, setAllTo);
            Triggers.Add(ct_Flashlight, setAllTo);
        }
    }
}