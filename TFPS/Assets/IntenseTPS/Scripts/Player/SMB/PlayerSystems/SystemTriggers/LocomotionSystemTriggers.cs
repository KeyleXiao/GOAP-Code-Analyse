namespace Player.Triggers
{
    public class LocomotionSystemTriggers : SystemTrigger
    {
        public const string ct_ToggleWalk = "Walk-Run";
        public const string ct_ToggleCrouch = "Crouch";
        public const string ct_FreeLook = "Free Look";
        public const string ct_Move = "Move";
        public const string ct_Jump = "Jump";

        public LocomotionSystemTriggers()
        {
            Triggers.Add(ct_ToggleWalk, true);
            Triggers.Add(ct_ToggleCrouch, true);
            Triggers.Add(ct_FreeLook, true);
            Triggers.Add(ct_Move, true);
            Triggers.Add(ct_Jump, true);
        }

        public LocomotionSystemTriggers(bool _toggleWalk, bool _toggleCrouch, bool _freeLook, bool _move, bool _ct_Jump)
        {
            Triggers.Add(ct_ToggleWalk, _toggleWalk);
            Triggers.Add(ct_ToggleCrouch, _toggleCrouch);
            Triggers.Add(ct_FreeLook, _freeLook);
            Triggers.Add(ct_Move, _move);
            Triggers.Add(ct_Jump, _ct_Jump);
        }

        public LocomotionSystemTriggers(bool _val)
        {
            Triggers.Add(ct_ToggleWalk, _val);
            Triggers.Add(ct_ToggleCrouch, _val);
            Triggers.Add(ct_FreeLook, _val);
            Triggers.Add(ct_Move, _val);
            Triggers.Add(ct_Jump, _val);
        }
    }
}