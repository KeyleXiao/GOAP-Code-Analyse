using System.Collections.Generic;

namespace Player.Triggers
{
    public class SystemTrigger
    {
        protected Dictionary<string, bool> Triggers;

        public SystemTrigger()
        {
            Triggers = new Dictionary<string, bool>();
        }

        public bool GetTrigger(string triggerName)
        {
            return Triggers[triggerName];
        }
    }
}