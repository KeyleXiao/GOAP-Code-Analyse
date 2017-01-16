namespace Player.Triggers
{
    public class CoverSystemTriggers : SystemTrigger
    {
        public const string ct_Cover = "Cover";
        public const string ct_Move = "Move";
        public const string ct_EdgePeek = "Peek";
        public const string ct_UpPeek = "UpPeek";
        public const string ct_MoveToCameraCover = "MoveToCameraCover";

        public CoverSystemTriggers()
        {
            Triggers.Add(ct_Cover, true);
            Triggers.Add(ct_Move, true);
            Triggers.Add(ct_EdgePeek, true);
            Triggers.Add(ct_UpPeek, true);
            Triggers.Add(ct_MoveToCameraCover, true);
        }

        public CoverSystemTriggers(bool val)
        {
            Triggers.Add(ct_Cover, val);
            Triggers.Add(ct_Move, val);
            Triggers.Add(ct_EdgePeek, val);
            Triggers.Add(ct_UpPeek, val);
            Triggers.Add(ct_MoveToCameraCover, val);
        }

        public CoverSystemTriggers(bool _ct_Cover, bool _ct_Move, bool _ct_Peek, bool _ct_UpPeek, bool _ct_CameraCover)
        {
            Triggers.Add(ct_Cover, _ct_Cover);
            Triggers.Add(ct_Move, _ct_Move);
            Triggers.Add(ct_EdgePeek, _ct_Peek);
            Triggers.Add(ct_UpPeek, _ct_UpPeek);
            Triggers.Add(ct_MoveToCameraCover, _ct_CameraCover);
        }
    }
}