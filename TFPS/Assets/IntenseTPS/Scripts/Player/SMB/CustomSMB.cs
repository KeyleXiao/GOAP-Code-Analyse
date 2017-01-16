using UnityEngine;

namespace Player
{
    public class CustomSMB : StateMachineBehaviour
    {
        [HideInInspector]
        public SetupAndUserInput userInput;

        [HideInInspector]
        public PlayerAtts player;

        public virtual void OnEnabled(Animator anim)
        {
        }

        public virtual void OnStart()
        {
        }
    }
}