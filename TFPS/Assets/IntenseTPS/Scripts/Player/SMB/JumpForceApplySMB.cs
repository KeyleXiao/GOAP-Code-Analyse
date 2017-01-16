using UnityEngine;
using System.Collections;

namespace Player
{
    public class JumpForceApplySMB : CustomSMB
    {
        public enum WhenToApplyJumpForce
        {
            OnStateEnter, OnStateExit, TimeAfterEnter
        }
        public enum WhichForceToApply
        {
            IdleJumping, RunJumping,
        }
        
        public WhenToApplyJumpForce whenToApplyForce;
        public WhichForceToApply whichForceToApply;
        public float timeAfterEnterToApplyForce = .2f;
        private float tempTime;
        private Rigidbody rb;

        public override void OnEnabled(Animator anim)
        {
            rb = userInput.GetComponent<Rigidbody>();
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (whenToApplyForce == WhenToApplyJumpForce.OnStateEnter)
            {
                if (whichForceToApply == WhichForceToApply.IdleJumping)
                    rb.AddForce(new Vector3(0, player.jumpProps.idleJumpUpForce, 0), ForceMode.Impulse);
                else
                    rb.AddForce(new Vector3(0, player.jumpProps.runJumpUpForce, 0), ForceMode.Impulse);
            }
            tempTime = timeAfterEnterToApplyForce;

        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (whenToApplyForce != WhenToApplyJumpForce.TimeAfterEnter)
                return;
            if (tempTime < 0)
            {
                if (whichForceToApply == WhichForceToApply.IdleJumping)
                    rb.AddForce(new Vector3(0, player.jumpProps.idleJumpUpForce, 0), ForceMode.Impulse);
                else
                    rb.AddForce(new Vector3(0, player.jumpProps.runJumpUpForce, 0), ForceMode.Impulse);
                tempTime = float.MaxValue;
            }
            tempTime -= Time.deltaTime;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (whenToApplyForce == WhenToApplyJumpForce.OnStateExit)
            {
                if (whichForceToApply == WhichForceToApply.IdleJumping)
                    rb.AddForce(new Vector3(0, player.jumpProps.idleJumpUpForce, 0), ForceMode.Impulse);
                else
                    rb.AddForce(new Vector3(0, player.jumpProps.runJumpUpForce, 0), ForceMode.Impulse);
            }
        }
    }

}
