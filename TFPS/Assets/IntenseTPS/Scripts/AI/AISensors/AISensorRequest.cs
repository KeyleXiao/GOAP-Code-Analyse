using Information;
using System.Collections.Generic;

namespace Sensors
{
    /// <summary>
    /// Scriptable sensor which will be used on demand
    /// </summary>
    public abstract class AISensorRequest : AISensor
    {
        // These are for reference
        public virtual T RequestInfo<T>(AIBrain ai) where T : InformationP { return null; }

        public virtual List<T> RequestAllInfo<T>(AIBrain ai) where T : InformationP
        {
            return null;
        }
    }
}