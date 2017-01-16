using Player.Triggers;
using UnityEngine;

namespace Player
{
    public class JumpAirControlSMB : CustomSMB
    {
        private const string c_overrideKey = "OnAir";
        private static int cap_Grounded = Animator.StringToHash("Grounded");
        private static int cap_VelocityYPar = Animator.StringToHash("VelocityY");
        private static readonly int _RunJumpStartState = Animator.StringToHash("RunJumpStart");
        private static readonly int _idleJumpStartState = Animator.StringToHash("IdleJumpStart");
        private static readonly int _fall = Animator.StringToHash("Fall");
        private static readonly int footOnGround = Animator.StringToHash("FootOnGround");
        private static readonly int _toIdleState = Animator.StringToHash("ToIdle");
        private static readonly int handOnGround = Animator.StringToHash("HandOnGround");
        private static readonly int _rollState = Animator.StringToHash("Roll");
        private static readonly int _toRunState = Animator.StringToHash("ToRun");
        private static readonly int _handOnGroundWaitState = Animator.StringToHash("HandOnGroundWait");
        private static readonly int _handOnGroundToIdleState = Animator.StringToHash("HandOnGroundToIdle");
        private static readonly int _idleTorise = Animator.StringToHash("IdleToRise");
        private static readonly int _riseToTop = Animator.StringToHash("RiseToTop");
        private static readonly int _topPose = Animator.StringToHash("TopPose");
        private static readonly int _topToFall = Animator.StringToHash("TopToFall");
        private Transform playerT;
        private float horizontal;
        private float vertical;
        private Rigidbody rb;
        private Transform moveReference;

        public override void OnEnabled(Animator anim)
        {
            playerT = userInput.transform;
            rb = userInput.GetComponent<Rigidbody>();
            if (userInput.cameraRig)
                moveReference = userInput.cameraRig.FindChild("Move Reference");
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            horizontal = userInput.Horizontal;
            vertical = userInput.Vertical;

            if (!animator.GetBool(cap_Grounded))
                animator.SetFloat(cap_VelocityYPar, Mathf.Lerp(animator.GetFloat(cap_VelocityYPar), playerT.GetComponent<Rigidbody>().velocity.y, Time.deltaTime * player.jumpProps.velocityParamSmooth));

            if (stateInfo.shortNameHash == _RunJumpStartState || stateInfo.shortNameHash == _idleJumpStartState || stateInfo.shortNameHash == footOnGround ||
                stateInfo.shortNameHash == _toIdleState || stateInfo.shortNameHash == handOnGround || stateInfo.shortNameHash == _rollState ||
                stateInfo.shortNameHash == _toRunState || stateInfo.shortNameHash == _handOnGroundWaitState || stateInfo.shortNameHash == _handOnGroundToIdleState)
                return;

            Vector3 moveDirection = Vector3.zero;
            float targetAngle = 0f;
            CalculateRefs(ref moveDirection, ref targetAngle, userInput.Horizontal, userInput.Vertical);

            float angleSmooth = Mathf.Lerp(0, targetAngle, moveDirection.magnitude * Time.deltaTime * player.jumpProps.airRotationSpeed);

            if (player.jumpProps.useTorqueRotation)
                rb.AddTorque(playerT.up * angleSmooth, ForceMode.VelocityChange);
            else
                player.transform.rotation = Quaternion.Euler(0, angleSmooth, 0) * player.transform.rotation;

            if (isOnAir(stateInfo))
            {
                Vector3 stickDirection = new Vector3(userInput.Horizontal, 0, userInput.Vertical);
                Vector3 camDirection = moveReference.forward;
                camDirection.y = 0;
                Quaternion refShift1 = Quaternion.FromToRotation(Vector3.forward, camDirection);
                Vector3 targetMoveDirection = refShift1 * stickDirection;
                player.playerPhysic.MoveDir = targetMoveDirection;
                player.playerPhysic.AddJumpControlForce = true;
            }
            else
                player.playerPhysic.AddJumpControlForce = false;

            if (rb.velocity.y < player.jumpProps.airDownForceStartVelocity)
                player.playerPhysic.AddJumpDownForce = true;
            else
                player.playerPhysic.AddJumpDownForce = false;

            float sqrM = Vector2.SqrMagnitude(new Vector2(horizontal, vertical)) > 1 ? Vector2.SqrMagnitude(new Vector2(horizontal, vertical).normalized) : Vector2.SqrMagnitude(new Vector2(horizontal, vertical));
            animator.SetFloat("Speed", sqrM, .2f, Time.deltaTime);

            if (animator.GetBool(cap_Grounded))
            {
                player.playerPhysic.AddJumpControlForce = false;
                player.playerPhysic.AddJumpDownForce = false;
            }
        }

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            if (player && player.SmbGroundedCheck)
                player.SmbGroundedCheck.PauseSet = false;

            if (player)
            {
                animator.SetFloat(cap_VelocityYPar, 0);

                short priority = -10;
                player.SmbWeapon.TriggS.Override(c_overrideKey, priority, new WeaponSystemTriggers(false));
                player.SmbCover.TriggS.Override(c_overrideKey, priority, new CoverSystemTriggers(false));
                player.SmbThrow.TriggS.Override(c_overrideKey, priority, new ThrowSystemTriggers(false));
                player.SmbLookIK.OverrideToDeactivateLookAt(new DeactivatedLookAtParams(player.defaultLookAtIKStyleIndex), priority, c_overrideKey);
            }
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
            player.playerPhysic.AddJumpControlForce = false;
            player.playerPhysic.AddJumpDownForce = false;

            if (player.SmbWeapon.TriggS.IsOverridenWithKey(c_overrideKey))
            {
                player.SmbWeapon.TriggS.Release(c_overrideKey);
                player.SmbCover.TriggS.Release(c_overrideKey);
                player.SmbThrow.TriggS.Release(c_overrideKey);
                player.SmbLookIK.ReleaseOverrideLookAt(c_overrideKey);
            }
        }

        private bool isOnAir(AnimatorStateInfo si) // states that we can control
        {
            return (si.shortNameHash == _idleTorise || si.shortNameHash == _riseToTop || si.shortNameHash == _topPose ||
                si.shortNameHash == _topToFall || si.shortNameHash == _fall || si.shortNameHash == _rollState);
        }

        private void CalculateRefs(ref Vector3 targetMoveDirection, ref float targetAngle, float Horizontal, float Vertical)
        {
            Vector3 stickDirection = new Vector3(Horizontal, 0, Vertical);
            Vector3 camDirection = moveReference.forward;
            camDirection.y = 0;

            Quaternion refShift1 = Quaternion.FromToRotation(Vector3.forward, camDirection);
            Quaternion refShift2 = new Quaternion(userInput.transform.rotation.x, userInput.transform.rotation.y * -1f, userInput.transform.rotation.z, userInput.transform.rotation.w);

            targetMoveDirection = refShift1 * stickDirection;
            Vector3 axisSign = Vector3.Cross(targetMoveDirection, userInput.transform.forward);
            targetAngle = Vector3.Angle(userInput.transform.forward, targetMoveDirection) * (axisSign.y >= 0 ? -1f : 1f);

            targetMoveDirection = refShift2 * targetMoveDirection;

            if (targetMoveDirection == Vector3.zero)
                targetAngle = 0;
        }
    }
}