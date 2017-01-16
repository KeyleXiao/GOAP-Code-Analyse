using UnityEngine;

namespace Messages
{
    public class AIMessageSuspicionPosition : MessageBase<AIMessageSuspicionPosition>
    {
        public Transform FoundTransform { get; private set; }
        public Vector3 FoundPos { get; private set; }
        public float FoundTime { get; private set; }

        public AIMessageSuspicionPosition(Transform _foundTransform, Vector3 _foundPos, float _foundTime)
        {
            FoundTransform = _foundTransform;
            FoundPos = _foundPos;
            FoundTime = _foundTime;
        }
    }
}