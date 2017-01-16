using System;

namespace Sensors
{
    /// <summary>
    /// Scriptable sensor with update capability
    /// </summary>
    public class AISensorPolling : AISensor
    {
        [NonSerialized]
        public float LastWorkedUpdateTime;

        [NonSerialized]
        public float DeltaTimeSinceLastWork;

        public float updateInterval = .1f;

        public virtual bool OnUpdate(AIBrain _ai)
        {
            return false;
        }
    }
}