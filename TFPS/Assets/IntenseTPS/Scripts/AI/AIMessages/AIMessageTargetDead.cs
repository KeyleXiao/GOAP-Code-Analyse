using UnityEngine;

namespace Messages
{
    public class AIMessageTargetDead : MessageBase<AIMessageTargetDead>
    {
        public Transform deadTarget;

        public AIMessageTargetDead(Transform _deadTarget)
        {
            deadTarget = _deadTarget;
        }
    }
}