using UnityEngine;

namespace Player
{
    public class GroundedCheckSMB : CustomSMB
    {
        private static readonly int cap_Grounded = Animator.StringToHash("Grounded");

        private LayerMask groundLayer;
        private GroundedManager groundedManager = new GroundedManager();

        public bool PauseSet { get; set; }

        public override void OnEnabled(Animator anim)
        {
            groundLayer = player.groundLayer;
            groundedManager.Init(anim, groundLayer);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!PauseSet)
                animator.SetBool(cap_Grounded, groundedManager.IsGrounded);
        }

        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            groundedManager.CheckGroundedWithVelocity();
        }
    }
}