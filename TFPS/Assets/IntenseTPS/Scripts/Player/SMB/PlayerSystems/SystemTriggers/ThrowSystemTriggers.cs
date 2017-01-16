namespace Player.Triggers
{
    public class ThrowSystemTriggers : SystemTrigger
    {
        public const string ct_PullOut = "Pull Out";
        public const string ct_IndicatorPress = "Indicator Press";
        public const string ct_Switch_Throwable = "Switch Throwable";

        public ThrowSystemTriggers()
        {
            Triggers.Add(ct_PullOut, true);
            Triggers.Add(ct_IndicatorPress, true);
            Triggers.Add(ct_Switch_Throwable, true);
        }

        public ThrowSystemTriggers(bool _ct_PullOut, bool _ct_indicatorPress, bool _ct_Switch_Throwable)
        {
            Triggers.Add(ct_PullOut, _ct_PullOut);
            Triggers.Add(ct_IndicatorPress, _ct_indicatorPress);
            Triggers.Add(ct_Switch_Throwable, _ct_Switch_Throwable);
        }

        public ThrowSystemTriggers(bool _val)
        {
            Triggers.Add(ct_PullOut, _val);
            Triggers.Add(ct_IndicatorPress, _val);
            Triggers.Add(ct_Switch_Throwable, _val);
        }
    }
}