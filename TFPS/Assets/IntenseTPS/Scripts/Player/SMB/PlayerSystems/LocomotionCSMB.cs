using Player.Triggers;
using UnityEngine;

namespace Player
{
    public enum PredefinedLocomType { FreeWithKeys, StaticWithKeys, WithNavmesh, DeactivatedLayer }

    public enum MoveType { Idle, Walk, Run, Sprint };

    #region Override Params

    public class LocomotionParams
    {
    }

    public class LocomoteStaticParams : LocomotionParams
    {
        public Transform TurnToObject { get; private set; }
        public int LocomStyleIndex { get; private set; }

        public LocomoteStaticParams(Transform _turnToObject, int _locomStyleIndex)
        {
            TurnToObject = _turnToObject;
            LocomStyleIndex = _locomStyleIndex;
        }
    }

    public class LocomoteFreeParams : LocomotionParams
    {
        public int LocomStyleIndex { get; private set; }

        public LocomoteFreeParams(int _locomStyleIndex)
        {
            LocomStyleIndex = _locomStyleIndex;
        }
    }

    public class DeactivatedLocomLayerParams : LocomotionParams
    {
        public bool DeactivateRigidbody { get; private set; }

        public DeactivatedLocomLayerParams(bool _deactivateRigidbody)
        {
            DeactivateRigidbody = _deactivateRigidbody;
        }
    }

    public class LocomoteWithAgentFreeToTransformParams : LocomotionParams
    {
        public int LocomStyleIndex { get; private set; }
        public Transform MoveToTransform { get; private set; }
        public MoveType MoveType { get; private set; }

        public LocomoteWithAgentFreeToTransformParams(Transform _moveToTransform, MoveType _moveType, int _locomStyleIndex)
        {
            LocomStyleIndex = _locomStyleIndex;
            MoveType = _moveType;
            MoveToTransform = _moveToTransform;
        }
    }

    #endregion Override Params

    public class LocomotionCSMB : CustomPlayerSystemSMB
    {
        #region Const

        private const string c_overrideKey = "Locomotion csmb";

        private static readonly int ht_Locomotion = Animator.StringToHash("Locomotion");
        private static readonly int ht_Turning = Animator.StringToHash("Turning");

        private static readonly int cap_VelX = Animator.StringToHash("VelX");
        private static readonly int cap_VelY = Animator.StringToHash("VelY");
        private static readonly int cap_Angle = Animator.StringToHash("Angle");

        private static readonly int cap_Speed = Animator.StringToHash("Speed");
        private static readonly int cap_QuickTurn = Animator.StringToHash("QuickTurn");

        #endregion Const

        #region Properties

        public LocomotionEvents Locomotion_Events { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsWalking { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool AllowSprint { get; set; }
        public bool IsMovingWithAgent { get; private set; }
        public PredefinedLocomType LocomType { get; set; }
        public Transform TurnToTransform { get; set; }
        public Transform MoveToTransformWithAgent { get; set; }
        public Vector3 MoveToPosWithAgent { get; set; }
        public float LayerWeightTarget { get; private set; }
        public float RemainingAgentDistance { get; private set; }
        public bool PathPending { get; private set; }
        public bool IsFreeLooking { get; private set; }

        public bool IsLocomoting
        {
            get { return LocomType != PredefinedLocomType.DeactivatedLayer; }
            private set { }
        }

        #endregion Properties

        #region Private

        private LocomotionProps cProps;
        private bool crouchBtnEnabled = false;
        private int switchToStyleOnCrouchDown = 0;
        private float lastAngle, lastVelX, lastVelY;
        private MoveType moveType;
        private Transform transform;
        private Rigidbody rb;
        private Vector2 smoothDeltaPosition;
        private Vector2 velocity;
        private NavMeshAgent agent;
        private Vector3 cDesiredDir;
        private Transform moveReference;
        private float cTorque = 0;
        private bool forceSprint = false;
        private Vector3 turnToPosition = Vector3.zero;
        private LayersWithDefValue<LocomotionParams> LocomTypeDict;
        private Animator animator;

        #endregion Private

        #region Starters

        public override void OnEnabled(Animator anim)
        {
            TriggS = new LayersWithDefValue<SystemTrigger>(new LocomotionSystemTriggers());
            AllowSprint = true;
            IsMovingWithAgent = false;
            Locomotion_Events = new LocomotionEvents();
            transform = userInput.transform;
            rb = userInput.GetComponent<Rigidbody>();
            agent = userInput.GetComponent<NavMeshAgent>();
            animator = userInput.GetComponent<Animator>();
            if (userInput.cameraRig)
                moveReference = userInput.cameraRig.FindChild("Move Reference");

            Transform TurnToReference = null;
            if (userInput && userInput.cameraRig)
                foreach (var trn in userInput.cameraRig.GetComponentsInChildren<Transform>())
                {
                    if (trn.name == "TurnTo Reference")
                        TurnToReference = trn;
                }
            if (!moveReference || !rb || !animator || (player.useStaticMovement && !TurnToReference))
                userInput.DisablePlayer("Needed reference not found:" + ToString());

            if (player.useStaticMovement)
            {
                TurnToTransform = TurnToReference;
                LocomTypeDict = new LayersWithDefValue<LocomotionParams>(new LocomoteStaticParams(TurnToReference, player.defaultLocomStyleIndex), "Locomotion");
            }
            else
                LocomTypeDict = new LayersWithDefValue<LocomotionParams>(new LocomoteFreeParams(player.defaultLocomStyleIndex), "Locomotion");
            LocomType = player.useStaticMovement ? PredefinedLocomType.StaticWithKeys : PredefinedLocomType.FreeWithKeys;
        }

        public override void OnStart()
        {
            IsWalking = player.startWalking;
            if (agent)
            {
                agent.updatePosition = false;
                agent.updateRotation = false;
            }

            SetLocomotionStyle(player.defaultLocomStyleIndex);
            if (cProps == null)
                userInput.DisablePlayer("Default currentLP not found by" + this);
        }

        #endregion Starters

        #region Overriders

        public void OverrideLocomoteStatic(LocomoteStaticParams param, short priority, string id)
        {
            if (param.TurnToObject == null)
            {
                Debug.Log("Turn to object is null, turned movement won't work...");
                return;
            }

            LocomTypeDict.Override(id, priority, param);
            if (LocomTypeDict.LastId == id)

                ToStatic(param);
        }

        private void ToStatic(LocomoteStaticParams param)
        {
            LayerWeightTarget = 1;
            TurnToTransform = param.TurnToObject;
            LocomType = PredefinedLocomType.StaticWithKeys;
            MoveToTransformWithAgent = null;
            StopLocomotionWithAgent();
            rb.isKinematic = false;
            SetLocomotionStyle(param.LocomStyleIndex);
        }

        public void OverrideToDeactivatedLayer(DeactivatedLocomLayerParams param, short priority, string id)
        {
            LocomTypeDict.Override(id, priority, param);
            if (LocomTypeDict.LastId == id)

                ToDeactivatedLayer(param);
        }

        private void ToDeactivatedLayer(DeactivatedLocomLayerParams param)
        {
            LayerWeightTarget = 0;
            StopLocomotionWithAgent();

            LocomType = PredefinedLocomType.DeactivatedLayer;
            TurnToTransform = null;
            MoveToTransformWithAgent = null;
            rb.isKinematic = param.DeactivateRigidbody;
            animator.SetFloat("Speed", 0);
            if (player.Animator.GetFloat("VelX") > .5f)
            {
                player.Animator.SetFloat("VelX", .5f);
            }
            else
            {
                player.Animator.SetFloat("VelX", 0f);
            }
            if (player.Animator.GetFloat("VelY") > .5f)
            {
                player.Animator.SetFloat("VelY", .5f);
            }
            else
            {
                player.Animator.SetFloat("VelY", 0f);
            }
        }

        public void OverrideLocomoteFree(LocomoteFreeParams param, short priority, string id)
        {
            LocomTypeDict.Override(id, priority, param);
            if (LocomTypeDict.LastId == id)
                ToFree(param);
        }

        private void ToFree(LocomoteFreeParams param)
        {
            LayerWeightTarget = 1;
            StopLocomotionWithAgent();
            rb.isKinematic = false;
            LocomType = PredefinedLocomType.FreeWithKeys;
            TurnToTransform = null;
            MoveToTransformWithAgent = null;
            if (param.LocomStyleIndex >= 0)
            {
                SetLocomotionStyle(param.LocomStyleIndex);
            }
            else
            {
                SetLocomotionStyle(player.defaultLocomStyleIndex);
                Debug.Log("No locomotion style found with this index, default will be used!");
            }

            if (player.Animator.GetFloat("VelX") > .5f)
            {
                player.Animator.SetFloat("VelX", .5f);
            }
            else
            {
                player.Animator.SetFloat("VelX", 0f);
            }
            if (player.Animator.GetFloat("VelY") > .5f)
            {
                player.Animator.SetFloat("VelY", .5f);
            }
            else
            {
                player.Animator.SetFloat("VelY", 0f);
            }
        }

        public void OverrideLocomoteWithAgentFreeToTransform(LocomoteWithAgentFreeToTransformParams param, short priority, string id)
        {
            LocomTypeDict.Override(id, priority, param);
            if (LocomTypeDict.LastId == id)

                ToAgentFreeToTransform(param);
        }

        private void ToAgentFreeToTransform(LocomoteWithAgentFreeToTransformParams param)
        {
            LayerWeightTarget = 1;
            LocomType = PredefinedLocomType.WithNavmesh;
            TurnToTransform = null;
            //rb.isKinematic = true;
            MoveToTransformWithAgent = param.MoveToTransform;
            agent.enabled = true;
            StartLocomotionWithAgent(param.MoveType, param.MoveToTransform.position);
            if (param.LocomStyleIndex >= 0)
            {
                SetLocomotionStyle(param.LocomStyleIndex);
            }
            else
            {
                SetLocomotionStyle(player.defaultLocomStyleIndex);
                Debug.Log("No locomotion style found with this index, default will be used!");
            }
        }

        public bool IsOverridenWithKey(string key)
        {
            return LocomTypeDict.IsOverridenWithKey(key);
        }

        public void ReleaseOverrideLocomoteType(string id)
        {
            if (LocomTypeDict.IsOverridenWithKey(id))
            {
                LocomTypeDict.Release(id);

                if (LocomTypeDict.LastValue.GetType() == typeof(LocomoteFreeParams))
                {
                    ToFree(LocomTypeDict.LastValue as LocomoteFreeParams);
                }
                else if (LocomTypeDict.LastValue.GetType() == typeof(LocomoteStaticParams))
                {
                    ToStatic(LocomTypeDict.LastValue as LocomoteStaticParams);
                }
                else if (LocomTypeDict.LastValue.GetType() == typeof(LocomoteWithAgentFreeToTransformParams))
                {
                    ToAgentFreeToTransform(LocomTypeDict.LastValue as LocomoteWithAgentFreeToTransformParams);
                }
                else if (LocomTypeDict.LastValue.GetType() == typeof(DeactivatedLocomLayerParams))
                {
                    ToDeactivatedLayer(LocomTypeDict.LastValue as DeactivatedLocomLayerParams);
                }
            }
            else
            {
                Debug.Log("No such key found in locomotion types :" + id);
            }
        }

        #endregion Overriders

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            UpdateLayerWeight(layerIndex);

            switch (LocomType)
            {
                case PredefinedLocomType.FreeWithKeys:
                    if (stateInfo.tagHash == ht_Locomotion)
                    {
                        if (!animator.IsInTransition(layerIndex))
                        {
                            JumpTrigger();
                            KeyTriggers();
                        }

                        LocomotionWithKeysFree(animator, layerIndex, false);
                    }
                    else if (stateInfo.tagHash == ht_Turning)
                    {
                    }
                    break;

                case PredefinedLocomType.StaticWithKeys:
                    if (stateInfo.tagHash == ht_Locomotion || stateInfo.tagHash == ht_Turning)
                    {
                        if (!animator.IsInTransition(layerIndex))
                        {
                            JumpTrigger();
                            KeyTriggers();
                        }

                        if (TurnToTransform)
                            turnToPosition = TurnToTransform.position;
                        LocomotionWithKeysStaticQ(animator, layerIndex, true);
                    }
                    break;

                case PredefinedLocomType.WithNavmesh:
                    if (MoveToTransformWithAgent)
                        MoveToPosWithAgent = MoveToTransformWithAgent.position;
                    LocomotionWithAgent(animator, layerIndex, MoveToPosWithAgent, false);
                    break;

                case PredefinedLocomType.DeactivatedLayer:

                    break;

                default:
                    break;
            }
        }

        public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (IsMovingWithAgent)
            {
                if (MoveToTransformWithAgent)
                    MoveToPosWithAgent = MoveToTransformWithAgent.position;
                LocomotionWithAgent(animator, layerIndex, MoveToPosWithAgent, false);

                Vector3 position = animator.rootPosition;
                position.y = agent.nextPosition.y;
            }
            else
            {
                animator.ApplyBuiltinRootMotion();
            }
        }

        private void SetLocomotionStyle(int index)
        {
            if (index < 0 || index >= player.locomotionStyles.Count ||
                (IsCrouching && player.locomotionStyles[index].crouchButtonEnabled && (player.locomotionStyles[index].switchToStyleIndexOnCrouchBtn < 0 || player.locomotionStyles[index].switchToStyleIndexOnCrouchBtn >= player.locomotionStyles.Count))
                )
            {
                Debug.Log("Skipping locomotion style set. Index not found :" + index);
                return;
            }
            if (!player.locomotionStyles[index].crouchButtonEnabled)
            {
                IsCrouching = false;
                crouchBtnEnabled = false;
            }
            else
                crouchBtnEnabled = true;

            LocomotionStyle lSOld = player.locomotionStyles.Find(x => x.locomotionProps == cProps);

            if (IsCrouching && player.locomotionStyles[index].crouchButtonEnabled && !player.locomotionStyles[index].isCrouchingStyle)
                index = player.locomotionStyles[index].switchToStyleIndexOnCrouchBtn;

            cProps = player.locomotionStyles[index].locomotionProps;
            switchToStyleOnCrouchDown = player.locomotionStyles[index].switchToStyleIndexOnCrouchBtn;
            player.Animator.SetInteger("LocomotionStyle", player.locomotionStyles[index].animLocomStyleParam);

            if (lSOld != null)
                Locomotion_Events.InvokeLocomotionStyleChanged(lSOld, player.locomotionStyles[index]);
        }

        #region Logic Functions

        private void UpdateLayerWeight(int layerIndex)
        {
            animator.SetLayerWeight(layerIndex, Mathf.Lerp(animator.GetLayerWeight(layerIndex), 0,
                (LayerWeightTarget < .5f ? LocomotionStyle.layerDisableSpeed : LocomotionStyle.layerEnableSpeed) * Time.deltaTime));
        }

        public int HasBindedLocomStyleToWeaponStyle(int weaponStyle)
        {
            for (int i = 0; i < player.locomotionStyles.Count; i++)
            {
                if (player.locomotionStyles[i].bindedWeaponStyles.Contains(weaponStyle))
                    return i;
            }
            return player.defaultLocomStyleIndex;
        }

        private void KeyTriggers()
        {
            if (userInput.WalkToggleDown && TriggS.LastValue.GetTrigger(LocomotionSystemTriggers.ct_ToggleWalk))
            {
                IsWalking = !IsWalking;
            }
            if (userInput.CrouchDown && crouchBtnEnabled && TriggS.LastValue.GetTrigger(LocomotionSystemTriggers.ct_ToggleWalk))
            {
                IsCrouching = !IsCrouching;
                SetLocomotionStyle(switchToStyleOnCrouchDown);
                userInput.m_CrouchDown = false;
            }
            if (userInput.FirstPersonLookDown && !IsFreeLooking && TriggS.LastValue.GetTrigger(LocomotionSystemTriggers.ct_FreeLook))
            {
                if (userInput.cameraRig.GetComponent<PlayerCamera>())
                {
                    userInput.cameraRig.GetComponent<PlayerCamera>().OverrideCamera(new FirstPersonCameraParams(), -3, c_overrideKey);

                    short priority = 1;
                    TriggS.Override(c_overrideKey, priority, new LocomotionSystemTriggers(false, false, true, false, false));
                    player.SmbWeapon.TriggS.Override(c_overrideKey, priority, new WeaponSystemTriggers(false));
                    player.SmbCover.TriggS.Override(c_overrideKey, priority, new CoverSystemTriggers(false));
                    player.SmbThrow.TriggS.Override(c_overrideKey, priority, new ThrowSystemTriggers(false));
                    player.SmbLookIK.OverrideToDeactivateLookAt(new DeactivatedLookAtParams(player.defaultLookAtIKStyleIndex), priority, c_overrideKey);
                    IsFreeLooking = true;
                }
            }
            else if (IsFreeLooking && (userInput.FirstPersonLookDown || !TriggS.LastValue.GetTrigger(LocomotionSystemTriggers.ct_FreeLook)))
            {
                userInput.cameraRig.GetComponent<PlayerCamera>().ReleaseOverride(c_overrideKey);
                TriggS.Release(c_overrideKey);
                player.SmbWeapon.TriggS.Release(c_overrideKey);
                player.SmbCover.TriggS.Release(c_overrideKey);
                player.SmbThrow.TriggS.Release(c_overrideKey);
                player.SmbLookIK.ReleaseOverrideLookAt(c_overrideKey);
                IsFreeLooking = false;
            }
        }

        private void JumpTrigger()
        {
            if (userInput.JumpDown && TriggS.LastValue.GetTrigger(LocomotionSystemTriggers.ct_FreeLook) && animator.GetBool("Grounded"))
            {
                animator.SetTrigger("Jump");
                player.SmbGroundedCheck.PauseSet = true;
                animator.SetBool("Grounded", false);
            }
        }

        private void LocomotionWithKeysFree(Animator animator, int layerIndex, bool turnTo)
        {
            Vector3 moveDirection = Vector3.zero; float targetAngle = 0; bool shouldMove = true;
            CalculateRefs(ref moveDirection, ref targetAngle, ref shouldMove, true, false, turnTo);
            FreeLocomotion(animator, layerIndex, moveDirection, targetAngle);
        }

        private void LocomotionWithKeysStaticQ(Animator animator, int layerIndex, bool turnTo)
        {
            Vector3 moveDirection = Vector3.zero; float targetAngle = 0; bool shouldMove = true;
            CalculateRefs(ref moveDirection, ref targetAngle, ref shouldMove, true, false, turnTo);
            FreeLocomotion(
                animator, layerIndex, moveDirection, targetAngle,
                false, false, true, true, turnTo, true, .15f, cProps.forceMaxRotationOnTurnTo);
        }

        public void StartLocomotionWithAgent(MoveType moveType1, Vector3 destination)
        {
            switch (moveType1)
            {
                case MoveType.Idle:
                    break;

                case MoveType.Walk:
                    IsWalking = true;
                    break;

                case MoveType.Run:
                    IsWalking = false;
                    break;

                case MoveType.Sprint:
                    forceSprint = true;
                    IsWalking = false;
                    break;

                default:
                    break;
            }
            if (!agent)
                return;
            moveType = moveType1;
            agent.enabled = true;
            agent.SetDestination(destination);
            rb.isKinematic = true;

            IsMovingWithAgent = true;

            agent.updateRotation = true;
        }

        private void LocomotionWithAgent(Animator animator, int layerIndex, Vector3 destination, bool turnTo = false)
        {
            if (!agent)
                return;
            agent.SetDestination(destination);
            Vector3 moveDirection = Vector3.zero; float targetAngle = 0; bool shouldMove = true;
            CalculateRefs(ref moveDirection, ref targetAngle, ref shouldMove, false, true, turnTo);
            FreeLocomotion(animator, layerIndex, moveDirection, targetAngle, true, forceSprint, shouldMove, true, turnTo);
            RemainingAgentDistance = agent.remainingDistance;
            PathPending = agent.pathPending;
        }

        public void StopLocomotionWithAgent()
        {
            if (!agent)
                return;
            if (agent.enabled)
                agent.Stop();

            agent.enabled = false;

            IsMovingWithAgent = false;
        }

        public void CalculateRefs(
            ref Vector3 targetMoveDirection,
            ref float targetAngle,
            ref bool shouldMove,
            bool useKeys = true,
            bool moveWithNavMesh = false,
            bool turnTo = false
            )
        {
            Vector3 stickDirection = Vector3.zero;
            Vector3 desiredDir = Vector3.zero;
            Vector3 moveRefForward = Vector3.zero;

            if (useKeys && TriggS.LastValue.GetTrigger(LocomotionSystemTriggers.ct_Move))
            {
                moveRefForward = moveReference.forward;
                moveRefForward.y = 0;
                stickDirection = new Vector3(userInput.Horizontal, 0, userInput.Vertical);
                shouldMove = true;
            }
            if (moveWithNavMesh)
            {
                #region Unity Manual Code

                Vector3 worldDeltaPosition = agent.nextPosition - transform.position;

                // Map 'worldDeltaPosition' to local space
                float dx = Vector3.Dot(transform.right, worldDeltaPosition);
                float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
                Vector2 deltaPosition = new Vector2(dx, dy);

                // Low-pass filter the deltaMove
                float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
                smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

                // Update velocity if delta time is safe
                if (Time.deltaTime > 1e-5f)
                    velocity = smoothDeltaPosition / Time.deltaTime;

                shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius;

                // Pull agent towards character[Modded pull multiplier]
                if (worldDeltaPosition.magnitude > agent.radius)
                    agent.nextPosition = transform.position + .01f * worldDeltaPosition;

                // Set transform's y to agent
                transform.position = new Vector3(transform.position.x, agent.nextPosition.y, transform.position.z);

                #endregion Unity Manual Code

                desiredDir = (-transform.position + new Vector3(agent.nextPosition.x, transform.position.y, agent.nextPosition.z)).normalized * 2;

                cDesiredDir = Vector3.Lerp(cDesiredDir, desiredDir, Time.deltaTime * cProps.agentDesiredDirSmooth);
                //stickDirection = desiredDir;
                stickDirection = cDesiredDir;
                moveRefForward = Vector3.zero;
            }

            Quaternion refShift1 = Quaternion.FromToRotation(Vector3.forward, moveRefForward);
            Quaternion refShift2 = new Quaternion(transform.rotation.x, transform.rotation.y * -1f, transform.rotation.z, transform.rotation.w);

            targetMoveDirection = refShift1 * stickDirection;

            Vector3 axisSign = Vector3.Cross(targetMoveDirection, transform.forward);
            targetAngle = Vector3.Angle(transform.forward, targetMoveDirection) * (axisSign.y >= 0 ? -1f : 1f);

            targetMoveDirection = refShift2 * targetMoveDirection;

            if (targetMoveDirection == Vector3.zero)
                targetAngle = 0;
        }

        private void FreeLocomotion(
            Animator animator,
            int layerIndex, Vector3 moveDirection,
            float targetAngle,
            bool dontUseUpliftingTorque = false,
            bool forceSprint = false,
            bool shouldMove = true,
            bool allowQuickTurn = true,
            bool turnTo = false,
            bool allowTorque = true,
            float stopQuickTurnBiggerThanSpeed = Mathf.Infinity,
            float forceTurntoRotationAtAngle = 0f
            )
        {
            animator.ResetTrigger("QuickTurn");
            float animTurnAngle = 0;
            allowTorque = !IsMovingWithAgent && allowTorque;

            bool turnTransiting = false;

            float targetTorq = 0f;
            if (moveType == MoveType.Walk)
                targetTorq = cProps.rotationTurnSpeedWalk;
            else if (moveType == MoveType.Run)
                targetTorq = cProps.rotationTurnSpeedRun;
            else
                targetTorq = cProps.rotationTurnSpeedSprint;

            if (cProps.useUpliftingTurnSpeed && !dontUseUpliftingTorque)
            {
                if (Mathf.Abs(targetAngle) > cProps.upliftingTurnStartAngle)
                    cTorque = Mathf.Lerp(cTorque, targetTorq, Time.deltaTime * cProps.upliftTurnUpSpeed);
                else
                    cTorque = Mathf.Lerp(cTorque, 0, Time.deltaTime * cProps.upliftTurnDownUpSpeed);
            }
            else
                cTorque = targetTorq;

            if (turnTo)
            {
                turnToPosition.y = transform.position.y;
                targetAngle = Vector3.Angle(transform.forward, (turnToPosition - transform.position).normalized);
                targetAngle = targetAngle * Mathf.Sign(Vector3.Dot(transform.right, (turnToPosition - transform.position).normalized));
            }

            float angleSmooth = Mathf.Lerp(0, targetAngle, moveDirection.magnitude * Time.deltaTime * cTorque);

            if (forceTurntoRotationAtAngle != 0 && forceTurntoRotationAtAngle < 180)
            {
                if (targetAngle > forceTurntoRotationAtAngle || targetAngle < -forceTurntoRotationAtAngle)
                {
                    Quaternion targRot = transform.rotation * Quaternion.Euler(0, targetAngle, 0);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targRot, Time.deltaTime * cProps.forceRotationSpeed);
                }
            }

            if (allowTorque && !(animator.GetNextAnimatorStateInfo(layerIndex).tagHash == ht_Turning)
                )
            {
                if (cProps.useTorqueRotation)
                    rb.AddTorque(transform.up * angleSmooth, ForceMode.VelocityChange);
                else
                    transform.rotation = Quaternion.Euler(0, angleSmooth, 0) * transform.rotation;
            }

            // Not tested & Not used (Turned to position movement with agent) - should work in theory
            else if (IsMovingWithAgent && turnTo)
            {
                float rotationSpeed = 3f;
                switch (moveType)
                {
                    case MoveType.Idle:
                        rotationSpeed = cProps.idleAgentTurnToSpeed;
                        break;

                    case MoveType.Walk:
                        rotationSpeed = cProps.walkAgentTurnToSpeed;
                        break;

                    case MoveType.Run:
                        rotationSpeed = cProps.runAgentTurnToSpeed;
                        break;

                    case MoveType.Sprint:
                        rotationSpeed = cProps.sprintAgentTurnToSpeed;
                        break;

                    default:
                        break;
                }
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(-transform.position + new Vector3(turnToPosition.x, transform.position.y, turnToPosition.z)), Time.deltaTime * rotationSpeed);
            }

            if ((animator.GetNextAnimatorStateInfo(layerIndex).tagHash == ht_Turning)
                || (animator.GetCurrentAnimatorStateInfo(layerIndex).tagHash == ht_Turning && animator.GetNextAnimatorStateInfo(layerIndex).tagHash == ht_Turning)
                || animator.GetCurrentAnimatorStateInfo(layerIndex).tagHash == ht_Turning
                )
            {
                turnTransiting = true;
                animTurnAngle = lastAngle;
                lastVelX = animator.GetFloat(cap_VelX);
                lastVelY = animator.GetFloat(cap_VelY);
            }
            else
            {
                animTurnAngle = targetAngle;
                lastAngle = animTurnAngle;
            }

            moveType = MoveType.Idle;
            if (animator.GetFloat(cap_Speed) > cProps.allowIdleToWalkAtSpeed)
                moveType = MoveType.Walk;
            if (!IsWalking && animator.GetFloat(cap_Speed) > cProps.allowWalkToRunAtSpeed)
                moveType = MoveType.Run;
            if ((userInput.SprintPress || forceSprint) && moveType == MoveType.Run && animator.GetFloat(cap_Speed) > cProps.allowRunToSprintAtSpeed && AllowSprint)
                moveType = MoveType.Sprint;

            float velocityLimit = 0; IsSprinting = false;
            switch (moveType)
            {
                case MoveType.Idle:
                    velocityLimit = cProps.walkVelocityLimit;
                    break;

                case MoveType.Walk:
                    velocityLimit = cProps.walkVelocityLimit;
                    if (animTurnAngle > -cProps.walkTurnAngleStart && animTurnAngle < cProps.walkTurnAngleStart && !turnTransiting)
                        animTurnAngle = 0;
                    if (turnTransiting)
                        animator.SetFloat(cap_Speed, cProps.walkTurnAngleThreshold);

                    if (IsMovingWithAgent)
                    {
                        agent.speed = cProps.agentWalkSpeed;
                        agent.angularSpeed = cProps.agentAngularSpeedWalk;
                    }
                    break;

                case MoveType.Run:
                    velocityLimit = cProps.runVelocityLimit;
                    if (animTurnAngle > -cProps.runTurnAngleStart && animTurnAngle < cProps.runTurnAngleStart && !turnTransiting)
                        animTurnAngle = 0;
                    if (turnTransiting)
                        animator.SetFloat(cap_Speed, cProps.runTurnAngleThreshold);
                    if (IsMovingWithAgent)
                    {
                        agent.speed = cProps.agentRunSpeed;
                        agent.angularSpeed = cProps.agentAngularSpeedRun;
                    }
                    break;

                case MoveType.Sprint:
                    IsSprinting = true;
                    velocityLimit = cProps.sprintVelocityLimit;
                    if (animTurnAngle > -cProps.sprintTurnAngleStart && animTurnAngle < cProps.sprintTurnAngleStart && !turnTransiting)
                        animTurnAngle = 0;
                    if (turnTransiting)
                        animator.SetFloat(cap_Speed, cProps.sprintTurnAngleThreshold);
                    if (IsMovingWithAgent)
                    {
                        agent.speed = cProps.agentSprintSpeed;
                        agent.angularSpeed = cProps.agentAngularSpeedSprint;
                    }
                    break;

                default:
                    break;
            }

            float xVelocity = moveDirection.x, yVelocity = moveDirection.z;
            // Set animator parameters
            if (xVelocity > 0)
                xVelocity = xVelocity > velocityLimit ? velocityLimit : xVelocity;
            else if (xVelocity < 0)
                xVelocity = -xVelocity > velocityLimit ? -velocityLimit : xVelocity;
            if (yVelocity > 0)
                yVelocity = yVelocity > velocityLimit ? velocityLimit : yVelocity;
            else if (yVelocity < 0)
                yVelocity = -yVelocity > velocityLimit ? -velocityLimit : yVelocity;

            if (turnTransiting)
            {
                xVelocity = lastVelX;
                yVelocity = lastVelY;
            }

            if (!shouldMove)
            {
                xVelocity = 0;
                yVelocity = 0;
            }

            animator.SetFloat(cap_VelX, xVelocity, cProps.animVelSmoothDamp, Time.deltaTime);
            animator.SetFloat(cap_VelY, yVelocity, cProps.animVelSmoothDamp, Time.deltaTime);

            float animVelX = animator.GetFloat(cap_VelX);
            float animVelY = animator.GetFloat(cap_VelY);
            float speed = Vector2.SqrMagnitude(new Vector2(animVelX, animVelY)) > velocityLimit ? velocityLimit : Vector2.SqrMagnitude(new Vector2(animVelX, animVelY));

            if (Mathf.Abs(animTurnAngle) >= cProps.minTurnAngleStartAbs)
            {
                animator.SetFloat(cap_Angle, animTurnAngle);
                if (!turnTransiting && allowQuickTurn && animator.GetFloat(cap_Speed) < stopQuickTurnBiggerThanSpeed)
                    animator.SetTrigger(cap_QuickTurn);
            }
            else
                animator.SetFloat(cap_Angle, 0);

            if (!turnTransiting)
                animator.SetFloat(cap_Speed, speed);
        }

        #endregion Logic Functions
    }
}