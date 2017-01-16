using UnityEngine;

namespace Messages
{
    public class AIMessageSuspicionLost : MessageBase<AIMessageSuspicionLost>
    {
        public Transform lostTarget;

        public AIMessageSuspicionLost(Transform _deadTarget)
        {
            lostTarget = _deadTarget;
        }
    }
}