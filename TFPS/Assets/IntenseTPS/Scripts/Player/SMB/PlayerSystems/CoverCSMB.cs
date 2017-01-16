using Player.Triggers;
using UnityEngine;

namespace Player
{
    public class CoverCSMB : CustomPlayerSystemSMB
    {
        #region const/readonly

        private static readonly int h_Empty = Animator.StringToHash("Empty");
        private static readonly int h_ToCover = Animator.StringToHash("ToCover");
        private static readonly int h_CoverLocom = Animator.StringToHash("CoverLocomotion");
        private static readonly int ht_SwitchSide = Animator.StringToHash("SwitchSide");
        private static readonly int h_ToPeek = Animator.StringToHash("ToPeek");
        private static readonly int hTag_Peek = Animator.StringToHash("Peek");
        private static readonly int hTag_UpPeek = Animator.StringToHash("UpPeek");
        private static readonly int h_MovingToCamCover = Animator.StringToHash("MovingToCameraCover");

        private static readonly int hTrans_rightSwitchToLocom = Animator.StringToHash("SideSwitchFromRight -> CoverLocomotion");
        private static readonly int hTrans_leftSwitchToLocom = Animator.StringToHash("SideSwitchFromLeft -> CoverLocomotion");

        private const string cap_CoverSide = "CoverSide";
        private const string cap_Cover = "Cover";
        private const string cap_CrouchStand = "CrouchStand";
        private const string cap_Speed = "Speed";
        private const string cap_CoverSideSwitch = "CoverSideSwitch";
        private const string cap_EdgePeek = "Peek";
        private const string cap_UpPeek = "UpPeek";
        private const string cap_ToPeek = "ToPeek";
        private const string cap_Angle = "Angle";
        private const string cap_WeaponPull = "Weapon Pull";

        private const string c_OverrideKey = "Cover csmb";
        private const string c_OverrideKey_2 = "Cover csmb 2";

        #endregion const/readonly

        #region private

        private CoverTargetLogic coverChecker;
        private Vector3 calced_CoverNormal;
        private Vector3 calced_GroundPoint;
        private Animator animator;
        private Transform transform;
        private Transform moveReference;
        private bool switchSideOnCoverLocomStateExit = false;
        private PlayerCamera playerCamera;
        private GameObject tempObj;

        #endregion private

        #region Properties

        public CoveringEvents Events { get; private set; }
        public float LayerWeightTarget { get; private set; }
        public CoverProps CCoverProps { get; private set; }
        public Transform RefTarget { get; private set; }
        public bool IsEdgePeeking { get; private set; }

        public bool IsUpPeeking { get; private set; }

        public bool IsPeeking
        {
            get { return IsUpPeeking || IsEdgePeeking; }
            set { }
        }

        public bool IsInCover
        {
            get { return animator.GetInteger(cap_Cover) > 0; }
            set { }
        }

        #endregion Properties

        #region Starter

        public override void OnEnabled(Animator anim)
        {
            if (userInput.transform.FindChild("CoverChecker"))
                coverChecker = player.transform.FindChild("CoverChecker").GetComponent<CoverTargetLogic>();
            animator = userInput.GetComponent<Animator>();
            transform = userInput.transform;
            if (userInput.cameraRig)
            {
                moveReference = userInput.cameraRig.FindChild("Move Reference");
                playerCamera = userInput.cameraRig.GetComponent<PlayerCamera>();
            }

            foreach (Transform tr in userInput.cameraRig.GetComponentsInChildren<Transform>())
                if (tr.CompareTag("Target"))
                    RefTarget = tr;

            Events = new CoveringEvents();

            if (!coverChecker || !animator || !transform || !moveReference || !RefTarget || !playerCamera || player.defaultCoverStyleIndex >= player.coverStyles.Count)
                userInput.DisablePlayer("Reference not found on :" + ToString());

            TriggS = new LayersWithDefValue<SystemTrigger>(new CoverSystemTriggers());

            LayerWeightTarget = 0;
        }

        public override void OnStart()
        {
            SetCoverStyle(player.defaultCoverStyleIndex);
            coverChecker.StopUpdateForCameraCovers = true;
        }

        #endregion Starter

        #region Triggers

        private bool CoverCameraTrigger()
        {
            if (coverChecker.HaveCoverCamera && player && coverChecker.CoverWallNormalCamera != Vector3.zero && Vector3.Distance(transform.position, coverChecker.CoverGroundPositionCamera) > CCoverProps.minDistToGoToCamCover)
            {
                player.TryPrintWorldInformationText("CamCover", .5f, "Press 'Cover' button to move", coverChecker.CoverGroundPositionCamera, Quaternion.LookRotation(-coverChecker.CoverWallNormalCamera));
            }
            else
                return false;
            if (!TriggS.LastValue.GetTrigger(CoverSystemTriggers.ct_Cover) || !coverChecker.HaveCoverCamera)
                return false;
            if (userInput.CoverDown)
            {
                animator.SetInteger(cap_Cover, 2);
                if (!tempObj)
                {
                    tempObj = new GameObject();
                    tempObj.transform.name = "TempCoverObject";
                }
                tempObj.transform.position = coverChecker.CoverGroundPositionCamera;

                return true;
            }
            return false;
        }

        private bool CoverAroundTrigger(bool intentionalEnter = false)
        {
            if (!TriggS.LastValue.GetTrigger(CoverSystemTriggers.ct_Cover) || !coverChecker.HaveCoverAround || IsInCover)
                return false;

            if (userInput.CoverDown || intentionalEnter)
            {
                Events.InvokeOnCoverEnter();
                userInput.m_CoverDown = false;

                player.SmbLoco.OverrideToDeactivatedLayer(new DeactivatedLocomLayerParams(true), -1, c_OverrideKey);

                player.SmbWeapon.TriggS.Override(c_OverrideKey, 1, new WeaponSystemTriggers(false));
                player.SmbThrow.TriggS.Override(c_OverrideKey, 1, new ThrowSystemTriggers(false));

                LayerWeightTarget = 1;
                animator.SetFloat(cap_CrouchStand, 1);

                calced_CoverNormal = coverChecker.CoverWallNormalAround;
                calced_GroundPoint = coverChecker.CoverGroundPositionAround;

                animator.SetInteger(cap_Cover, 1);

                // Cover side is least angle side with player forward
                Vector3 fromPlToPointWPointY =
                    (-new Vector3(transform.position.x, calced_GroundPoint.y, transform.position.z) + calced_GroundPoint).normalized;
                float angle = Vector3.Dot(fromPlToPointWPointY, transform.right) * Vector3.Angle(fromPlToPointWPointY, transform.right);
                animator.SetFloat(cap_CoverSide, Mathf.Sign(angle));

                playerCamera.OverrideCamera(Mathf.Sign(angle) < 0 ? CCoverProps.cameraModifiersIdleLeft : CCoverProps.cameraModifiersIdleRight, -1, c_OverrideKey);

                player.GetComponent<Rigidbody>().isKinematic = true;
                return true;
            }
            return false;
        }

        private bool MoveTrigger()
        {
            if (!TriggS.LastValue.GetTrigger(CoverSystemTriggers.ct_Move) || IsEdgePeeking)
                return false;
            return true;
        }

        private bool ExitCoverTrigger(float angle, bool hasCurrentCover)
        {
            if (userInput.CoverDown || !hasCurrentCover)
            {
                Events.InvokeOnCoverExit();
                animator.SetInteger(cap_Cover, -1);
                LayerWeightTarget = 0;

                return true;
            }
            return false;
        }

        private bool UpPeekTrigger(float angle)
        {
            if (!TriggS.LastValue.GetTrigger(CoverSystemTriggers.ct_EdgePeek) || IsPeeking)
                return false;
            if (((userInput.Fire2Press || player.PressFire2Button) && animator.GetInteger(cap_WeaponPull) > 0) || player.SmbThrow.IsThrowing)
            {
                Events.InvokeOnUpPeek();

                animator.SetFloat(cap_Angle, angle);
                Ovr(false, animator.GetFloat(cap_CoverSide) < 0);
                animator.SetBool(cap_UpPeek, true);
                IsUpPeeking = true;
                return true;
            }
            return false;
        }

        private bool EdgePeekTrigger(float angle)
        {
            if (!TriggS.LastValue.GetTrigger(CoverSystemTriggers.ct_EdgePeek) || IsPeeking)
                return false;
            float animAngle = 0;
            bool isAngleInBound = IsAngleInBounds(angle, CCoverProps.edgePeekAngleTolerance,
                CCoverProps.oppositeDirMaxAngle, animator.GetFloat(cap_CoverSide) < 0, ref animAngle);
            if (isAngleInBound && (((userInput.Fire2Press || player.PressFire2Button) && animator.GetInteger(cap_WeaponPull) > 0)) || player.SmbThrow.IsThrowing)
            {
                Events.InvokeOnEdgePeek();
                animator.SetFloat(cap_Angle, animAngle);
                Ovr(true, animator.GetFloat(cap_CoverSide) < 0);
                animator.SetBool(cap_EdgePeek, true);
                IsEdgePeeking = true;
                return true;
            }
            return false;
        }

        private bool UnUpPeekTrigger(float angle)
        {
            if ((!TriggS.LastValue.GetTrigger(CoverSystemTriggers.ct_UpPeek) || !(userInput.Fire2Press || player.PressFire2Button || player.SmbThrow.IsThrowing)/* || !isAngleInBound*/) && IsUpPeeking)
            {
                Events.InvokeOnUnUpPeek();
                player.SmbWeapon.TriggS.Release(c_OverrideKey_2);
                player.SmbThrow.TriggS.Release(c_OverrideKey_2);
                player.SmbLookIK.ReleaseOverrideLookAt(c_OverrideKey_2);
                animator.ResetTrigger(cap_ToPeek);
                animator.SetBool(cap_UpPeek, false);
                IsUpPeeking = false;
                if (playerCamera.IsOverridenWithKey(c_OverrideKey))
                    playerCamera.ChangePointerOfCamModderWithID(animator.GetFloat(cap_CoverSide) < 0 ? CCoverProps.cameraModifiersIdleLeft : CCoverProps.cameraModifiersIdleRight, c_OverrideKey);
                if (playerCamera.IsOverridenWithKey(c_OverrideKey_2))
                    playerCamera.ReleaseOverride(c_OverrideKey_2);
                return true;
            }
            return false;
        }

        private bool UnEdgePeekTrigger(float angle)
        {
            float animAngle = 0;
            bool isAngleInBound = IsAngleInBounds(angle, CCoverProps.edgePeekAngleTolerance,
                CCoverProps.oppositeDirMaxAngle, animator.GetFloat(cap_CoverSide) < 0, ref animAngle);
            if (player.SmbThrow.IsThrowing)
                return false;
            if ((!TriggS.LastValue.GetTrigger(CoverSystemTriggers.ct_EdgePeek) || !(userInput.Fire2Press || player.PressFire2Button) || !isAngleInBound) && IsEdgePeeking)
            {
                Events.InvokeOnUnEdgePeek();
                player.SmbWeapon.TriggS.Release(c_OverrideKey_2);
                player.SmbThrow.TriggS.Release(c_OverrideKey_2);
                player.SmbLookIK.ReleaseOverrideLookAt(c_OverrideKey_2);
                animator.ResetTrigger(cap_ToPeek);
                animator.SetBool(cap_EdgePeek, false);
                IsEdgePeeking = false;
                playerCamera.ChangePointerOfCamModderWithID(animator.GetFloat(cap_CoverSide) < 0 ? CCoverProps.cameraModifiersIdleLeft : CCoverProps.cameraModifiersIdleRight, c_OverrideKey);
                if (playerCamera.IsOverridenWithKey(c_OverrideKey_2))
                    playerCamera.ReleaseOverride(c_OverrideKey_2);
                return true;
            }
            return false;
        }

        #endregion Triggers

        #region StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.shortNameHash == h_Empty)
            {
                player.SmbThrow.UseMirrorThrow = false;
                if (animator.GetInteger(cap_Cover) == -1)
                    ExitCover();
            }
            else if (stateInfo.shortNameHash == h_ToCover)
            {
                player.SmbWeapon.TriggS.Modify(c_OverrideKey, new WeaponSystemTriggers(true, true, true, false, true, true, true, false, false));
                player.SmbThrow.TriggS.Modify(c_OverrideKey, new ThrowSystemTriggers(true, true, true));
                coverChecker.StopUpdateForAroundCovers = true;
                transform.forward = calced_CoverNormal;
            }
            else if (stateInfo.shortNameHash == h_CoverLocom)
            {
                coverChecker.StopUpdateForCameraCovers = false;
                userInput.GetComponent<Rigidbody>().isKinematic = true;
            }
            else if (stateInfo.tagHash == hTag_Peek)
            {
                animator.SetFloat(cap_Speed, 0/*, 0, Time.deltaTime*/);
            }
            else if (stateInfo.tagHash == hTag_UpPeek)
            {
                animator.SetFloat(cap_Speed, 0);
            }
            else if (stateInfo.shortNameHash == h_MovingToCamCover)
            {
                ExitCover();
                player.SmbLoco.OverrideLocomoteWithAgentFreeToTransform(new LocomoteWithAgentFreeToTransformParams(tempObj.transform, MoveType.Run, player.defaultLocomStyleIndex),
                   -3, c_OverrideKey_2);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            UpdateLayerWeight(layerIndex);
            if (stateInfo.shortNameHash == h_Empty)
            {
                if (!animator.IsInTransition(0))
                    animator.SetFloat(cap_CrouchStand, 0, CCoverProps.animParamDamp, Time.deltaTime);

                if (!animator.IsInTransition(0) && CoverAroundTrigger())
                    return;
            }
            else if (stateInfo.shortNameHash == h_ToCover)
            {
                float crouchStand = CoverTargetLogic.RequestCrouch(CCoverProps.characterHeight, CCoverProps.crouchRayCheckCount, CCoverProps.crouchCheckStartHeight,
                    CCoverProps.crouchCheckRayMaxDist,
                    calced_GroundPoint, calced_CoverNormal
                    );
                animator.SetFloat(cap_CrouchStand, crouchStand, CCoverProps.animParamDamp, Time.deltaTime);
            }
            else if (stateInfo.shortNameHash == h_CoverLocom)
            {
                // Current Pos Check (with a fixed small distance to the opposite side)
                Vector3 coverNormalFromCPos = Vector3.zero;
                Vector3 coverGroundHitFromCPos = Vector3.zero;
                bool hasCoverCPos = coverChecker.RequestCover(
                    new Ray(transform.position + Vector3.up * coverChecker.aroundPlRayStartUpHeight, -calced_CoverNormal),
                    ref coverNormalFromCPos, ref coverGroundHitFromCPos,
                    CCoverProps.rayLength, coverChecker.maxAllowedAngleVert, coverChecker.maxAllowedGroundNormalAngle
                    );
                if (hasCoverCPos)
                {
                    calced_CoverNormal = coverNormalFromCPos;
                    calced_GroundPoint = coverGroundHitFromCPos;
                }

                // Cover exit check & stick angle
                float stickAngleWorld = GetStickAngle();
                if (!animator.IsInTransition(layerIndex) && (CoverCameraTrigger() || ExitCoverTrigger(stickAngleWorld, hasCoverCPos)))
                    return;

                // Next Pos Check
                Ray rayNext = new Ray(coverGroundHitFromCPos + Vector3.up * coverChecker.aroundPlRayStartUpHeight +
                    Quaternion.Euler(0, 90 * Mathf.Sign(animator.GetFloat(cap_CoverSide)), 0) * calced_CoverNormal * CCoverProps.sideRayDist, -calced_CoverNormal);
                Vector3 coverNormalFromNextPos = Vector3.zero;
                Vector3 coverGroundHitNextPos = Vector3.zero;
                bool hasCoverNextPos = coverChecker.RequestCover(
                    rayNext, ref coverNormalFromNextPos, ref coverGroundHitNextPos,
                    CCoverProps.rayLength, coverChecker.maxAllowedAngleVert, coverChecker.maxAllowedGroundNormalAngle
                    );

                // Camera look angle / Peek triggers
                float angle = GetAngle();

                if (!animator.IsInTransition(layerIndex))
                {
                    if (animator.GetFloat(cap_CoverSide) < 0)
                        player.SmbThrow.UseMirrorThrow = true;
                    else
                        player.SmbThrow.UseMirrorThrow = false;
                    if (!hasCoverNextPos && userInput.Vertical < .1f && EdgePeekTrigger(angle))
                        return;
                    else if (UpPeekTrigger(angle))
                        return;
                }

                SetCrouchStand();

                if (MoveTrigger() && (!animator.IsInTransition(layerIndex) || animator.GetAnimatorTransitionInfo(layerIndex).nameHash == hTrans_leftSwitchToLocom ||
                    animator.GetAnimatorTransitionInfo(layerIndex).nameHash == hTrans_rightSwitchToLocom)
                    && Mathf.Abs(stickAngleWorld) > CCoverProps.startMoveAtStickAngle)
                {
                    stickAngleWorld = Mathf.Sign(stickAngleWorld);
                    // Switch side
                    if (stickAngleWorld != Mathf.Sign(animator.GetFloat(cap_CoverSide)))
                    {
                        SetSideSwitchTrigger();
                        return;
                    }

                    if (hasCoverNextPos)
                        animator.SetFloat(cap_Speed, Mathf.Abs(stickAngleWorld), CCoverProps.animSpeedDamp, Time.deltaTime);
                    else
                        animator.SetFloat(cap_Speed, 0, CCoverProps.animSpeedDamp, Time.deltaTime);
                }
                else
                    animator.SetFloat(cap_Speed, 0, CCoverProps.animSpeedDamp, Time.deltaTime);
            }
            else if (stateInfo.tagHash == ht_SwitchSide)
            {
                animator.SetFloat(cap_Speed, 0, CCoverProps.animSpeedDamp, Time.deltaTime);
            }
            else if (stateInfo.shortNameHash == h_ToPeek)
            {
                animator.SetFloat(cap_Speed, 0, CCoverProps.animSpeedDamp, Time.deltaTime);
            }
            else if (stateInfo.tagHash == hTag_Peek)
            {
                float angle = GetAngle();
                if (UnEdgePeekTrigger(angle))
                    return;

                float animAngle = 0;
                IsAngleInBounds(angle, CCoverProps.edgePeekAngleTolerance,
                    CCoverProps.oppositeDirMaxAngle, animator.GetFloat(cap_CoverSide) < 0, ref animAngle);
                if (!animator.IsInTransition(layerIndex))
                    animator.SetFloat(cap_Angle, animAngle);
            }
            else if (stateInfo.tagHash == hTag_UpPeek)
            {
                float angle = GetAngle();
                UnUpPeekTrigger(angle);

                if (!animator.IsInTransition(layerIndex))
                    animator.SetFloat(cap_Angle, angle);
                if (!animator.IsInTransition(layerIndex))
                    animator.SetFloat(cap_CoverSide, Mathf.Sign(angle));
            }
            else if (stateInfo.shortNameHash == h_MovingToCamCover)
            {
                // Exit cover completely
                if (!animator.IsInTransition(layerIndex) && userInput.AnyKeyDown())
                {
                    player.SmbLoco.ReleaseOverrideLocomoteType(c_OverrideKey_2);
                    animator.SetInteger(cap_Cover, -1);
                    LayerWeightTarget = 0;
                    return;
                }
                if (!animator.IsInTransition(layerIndex) && animator.GetInteger(cap_Cover) > 0 && player.SmbLoco.RemainingAgentDistance < CCoverProps.toCameraCoverStopDistance)
                {
                    if (coverChecker.HaveCoverAround && CoverAroundTrigger(true))
                        player.SmbLoco.ReleaseOverrideLocomoteType(c_OverrideKey_2);
                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.shortNameHash == h_CoverLocom)
            {
                if (switchSideOnCoverLocomStateExit)
                {
                    animator.SetFloat(cap_CoverSide, -animator.GetFloat(cap_CoverSide));
                    playerCamera.ChangePointerOfCamModderWithID(Mathf.Sign(animator.GetFloat(cap_CoverSide)) < 0 ? CCoverProps.cameraModifiersIdleLeft : CCoverProps.cameraModifiersIdleRight, c_OverrideKey);

                    switchSideOnCoverLocomStateExit = false;
                }
                transform.forward = Quaternion.Euler(0,
                    CCoverProps.yRotationFixer * Mathf.Sign(animator.GetFloat(cap_CoverSide)), 0) * calced_CoverNormal;
                transform.position = calced_GroundPoint + calced_CoverNormal * CCoverProps.distFromWall;
                coverChecker.StopUpdateForCameraCovers = true;
            }
            else if (stateInfo.tagHash == ht_SwitchSide)
            {
                playerCamera.ChangePointerOfCamModderWithID(Mathf.Sign(animator.GetFloat(cap_CoverSide)) < 0 ? CCoverProps.cameraModifiersIdleLeft : CCoverProps.cameraModifiersIdleRight, c_OverrideKey);
            }
        }

        public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.shortNameHash == h_ToCover)
            {
                Vector3 lerpPos = calced_GroundPoint + calced_CoverNormal * CCoverProps.distFromWall;

                transform.position = Vector3.Lerp(transform.position, lerpPos, Time.deltaTime * CCoverProps.toCoverPositionLerpSpeed);
                transform.forward = Vector3.Lerp(transform.forward,
                    Quaternion.Euler(0,
                    CCoverProps.yRotationFixer * Mathf.Sign(animator.GetFloat(cap_CoverSide)), 0) * calced_CoverNormal,
                    Time.deltaTime * CCoverProps.toCoverRotationLerpSpeed);
            }
            else if (stateInfo.shortNameHash == h_CoverLocom)
            {
                transform.forward = Vector3.Lerp(transform.forward,
                    Quaternion.Euler(0,
                    CCoverProps.yRotationFixer * Mathf.Sign(animator.GetFloat(cap_CoverSide)), 0) * calced_CoverNormal,
                    Time.deltaTime * CCoverProps.toCoverRotationLerpSpeed);

                Vector3 lerpPos = calced_GroundPoint + calced_CoverNormal * CCoverProps.distFromWall;
                transform.position = Vector3.Lerp(transform.position, lerpPos, Time.deltaTime * CCoverProps.toCoverPositionLerpSpeed);
                animator.ApplyBuiltinRootMotion();
            }
            else if (stateInfo.tagHash == ht_SwitchSide)
            {
                Vector3 lerpPos = calced_GroundPoint + calced_CoverNormal * CCoverProps.distFromWall;

                transform.position = Vector3.Lerp(transform.position, lerpPos, Time.deltaTime * CCoverProps.toCoverPositionLerpSpeed);
                transform.forward = Vector3.Lerp(transform.forward,
                    Quaternion.Euler(0,
                    CCoverProps.yRotationFixer * Mathf.Sign(animator.GetFloat(cap_CoverSide)), 0) * calced_CoverNormal,
                    Time.deltaTime * CCoverProps.toCoverRotationLerpSpeed);
            }
            else
            {
                animator.ApplyBuiltinRootMotion();
            }
        }

        #endregion StateMachineBehaviour

        #region Logic Functions

        private void Ovr(bool isEdgePeek, bool isLeftPeek)
        {
            if (!player.SmbThrow.IsThrowing)
                player.SmbWeapon.TriggS.Override(c_OverrideKey_2, 0, new WeaponSystemTriggers(false, false, false, true, true, false, false, false, false));
            else
                player.SmbWeapon.TriggS.Override(c_OverrideKey_2, 0, new WeaponSystemTriggers(false));
            if (player.SmbThrow.IsThrowing)
                player.SmbThrow.TriggS.Override(c_OverrideKey_2, 0, new ThrowSystemTriggers(true, false, true));
            else
                player.SmbThrow.TriggS.Override(c_OverrideKey_2, 0, new ThrowSystemTriggers(false));

            ThirdPersonCameraParams modder = isEdgePeek ? (isLeftPeek ? CCoverProps.cameraModifiersEdgePeekLeft : CCoverProps.cameraModifiersEdgePeekRight) :
                (isLeftPeek ? CCoverProps.cameraModifiersUpPeekLeft : CCoverProps.cameraModifiersUpPeekRight);
            if (!playerCamera.IsOverridenWithKey(c_OverrideKey_2))
                playerCamera.OverrideCamera(modder, -2, c_OverrideKey_2);

            player.SmbLookIK.OverrideLookAtTransform(new LookAtTransformParams(RefTarget, player.Animator.GetBoneTransform(HumanBodyBones.Hips), player.SmbLookIK.HasBindedLookIKStyleToWeaponStyle(player.SmbWeapon.GetCGunStyle())), -2, c_OverrideKey_2);
        }

        private bool IsAngleInBounds(float angle, float toleranceAngleUp, float oppositeDirMaxAngle, bool isLeft, ref float animAngle)
        {
            float sign = Mathf.Sign(angle);
            float abs = Mathf.Abs(angle);

            if (isLeft)
            {
                if (sign > 0)
                {
                    if (abs < oppositeDirMaxAngle)
                    {
                        animAngle = abs;
                        return true;
                    }
                    else
                    {
                        animAngle = -(180 + 180 - abs);
                        if (180 - toleranceAngleUp < abs)
                            return true;
                        return false;
                    }
                }
                else
                {
                    animAngle = angle;
                    return true;
                }
            }
            else
            {
                if (sign < 0)
                {
                    if (abs < oppositeDirMaxAngle)
                    {
                        animAngle = -abs;
                        return true;
                    }
                    else
                    {
                        animAngle = 180 + 180 - abs;
                        if (180 - toleranceAngleUp < abs)
                            return true;
                        return false;
                    }
                }
                else
                {
                    animAngle = angle;
                    return true;
                }
            }
        }

        private float GetAngle()
        {
            Vector3 dir = (-transform.position + new Vector3(RefTarget.position.x, transform.position.y, RefTarget.position.z)).normalized;
            float dot = Mathf.Sign(Vector3.Dot(Quaternion.Euler(0, 90, 0) * calced_CoverNormal, dir));
            float angle = Vector3.Angle(dir, calced_CoverNormal);

            return angle * dot;
        }

        private void SetCrouchStand()
        {
            float crouchStand = CoverTargetLogic.RequestCrouch(CCoverProps.characterHeight, CCoverProps.crouchRayCheckCount, CCoverProps.crouchCheckStartHeight,
                    CCoverProps.crouchCheckRayMaxDist,
                    calced_GroundPoint, calced_CoverNormal
                    );
            animator.SetFloat(cap_CrouchStand, crouchStand, CCoverProps.animParamDamp, Time.deltaTime);
        }

        private void SetSideSwitchTrigger()
        {
            animator.SetTrigger(cap_CoverSideSwitch);
            switchSideOnCoverLocomStateExit = true;
        }

        private void ExitCover()
        {
            if (player.SmbLoco.IsOverridenWithKey(c_OverrideKey))
                player.SmbLoco.ReleaseOverrideLocomoteType(c_OverrideKey);
            if (playerCamera.IsOverridenWithKey(c_OverrideKey))
                playerCamera.ReleaseOverride(c_OverrideKey);

            if (player.SmbWeapon.TriggS.IsOverridenWithKey(c_OverrideKey))
                player.SmbWeapon.TriggS.Release(c_OverrideKey);
            if (player.SmbThrow.TriggS.IsOverridenWithKey(c_OverrideKey))
                player.SmbThrow.TriggS.Release(c_OverrideKey);

            coverChecker.StopUpdateForAroundCovers = false;
        }

        public float GetStickAngle(
        )
        {
            float targetAngle;
            Vector3 targetMoveDirection;

            Vector3 stickDirection = Vector3.zero;
            Vector3 moveRefForward = Vector3.zero;

            moveRefForward = moveReference.forward; moveRefForward.y = 0;
            stickDirection = new Vector3(userInput.Horizontal, 0, userInput.Vertical);

            Quaternion refShift1 = Quaternion.FromToRotation(Vector3.forward, moveRefForward);
            Quaternion refShift2 = new Quaternion(transform.rotation.x, transform.rotation.y * -1f, transform.rotation.z, transform.rotation.w);

            targetMoveDirection = refShift1 * stickDirection;

            Vector3 axisSign = Vector3.Cross(targetMoveDirection, transform.forward);
            targetAngle = Vector3.Angle(transform.forward, targetMoveDirection) * (axisSign.y >= 0 ? -1f : 1f);

            targetMoveDirection = refShift2 * targetMoveDirection;
            if (targetMoveDirection == Vector3.zero)
                targetAngle = 0;

            return targetAngle;
        }

        private void SetCoverStyle(int index)
        {
            if (index >= player.coverStyles.Count)
            {
                Debug.Log("Cant set cover style");
                return;
            }
            CCoverProps = player.coverStyles[index].coverProps;
        }

        private void UpdateLayerWeight(int layerIndex)
        {
            float cLayerWeight = animator.GetLayerWeight(layerIndex);
            animator.SetLayerWeight(layerIndex, Mathf.Lerp(cLayerWeight, LayerWeightTarget,
                    (LayerWeightTarget < .5f ? CCoverProps.layerDisableSpeed : CCoverProps.layerEnableSpeed) * Time.deltaTime));
        }

        #endregion Logic Functions
    }
}