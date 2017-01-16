using Player.Triggers;
using System.Linq;
using UnityEngine;

namespace Player
{
    public class ThrowCSMB : CustomPlayerSystemSMB
    {
        #region Const

        private const string c_overrideKey = "Throw csmb";

        private static readonly int h_Empty = Animator.StringToHash("Empty");
        private static readonly int h_PullOut = Animator.StringToHash("PullOut");
        private static readonly int h_ToReady = Animator.StringToHash("ToReady");
        private static readonly int h_ReadyIdle = Animator.StringToHash("ReadyIdle");
        private static readonly int h_Throw = Animator.StringToHash("Throw");

        private static readonly int cap_Throw = Animator.StringToHash("Throw");
        private static readonly int cap_ThrowDistance = Animator.StringToHash("Throw Distance");
        private static readonly int cap_ThrowStyle = Animator.StringToHash("Throw Style");

        #endregion Const

        #region Properties

        public bool UseMirrorThrow { get; set; }
        public ThrowingEvents Events { get; private set; }
        public int CurrentThrowableIndex { get; private set; }
        public int UpperBodyLayerTarget { get; private set; }
        public ThrowProps CThrowProps { get; private set; }
        public bool IsThrowing { get; private set; }
        public Transform TurnToReference { get; private set; }

        #endregion Properties

        #region Private

        private PlayerCamera playerCamera;
        private Transform leftHandHold;
        private Transform rightHandHold2;
        private Transform transform;
        private Animator animator;
        private Vector3 throwDir;
        private Exploder cExploderClone;
        private Exploder cExploderPrefab;
        private bool IsChangingThrowable = false;
        private int nextThrowableIndex;
        private Transform refTarget;
        private bool _isThrown = false;
        private bool _mirroredAnimDoubleHandOnCall_IsCalled = false;

        #endregion Private

        #region Public Checkers

        public int GetThrowableCount()
        {
            if (CurrentThrowableIndex == -1)
                return 0;
            else
                return player.throwableBag[CurrentThrowableIndex].haveCount;
        }

        public Sprite GetThrowableSprite()
        {
            if (CurrentThrowableIndex == -1)
                return null;
            else
                return player.throwableBag[CurrentThrowableIndex].throwablePrefab.hudImage;
        }

        public string GetThrowableName()
        {
            if (CurrentThrowableIndex == -1)
                return "";
            else
                return player.throwableBag[CurrentThrowableIndex].throwablePrefab.throwableName;
        }

        public bool HaveAnyThrowable()
        {
            return player.throwableBag.FirstOrDefault(x => x.haveCount > 0) == null ? false : true;
        }

        public int GetStyleOfIndex(int index)
        {
            if (index == -1 || player.throwableBag.Count >= index)
                return -1;
            return player.throwableBag[index].throwablePrefab.throwStyle;
        }

        public bool HaveAThrowableSelected()
        {
            return CurrentThrowableIndex != -1;
        }

        #endregion Public Checkers

        #region Animation Embedded

        public void HandOnBomb()
        {
            if (_mirroredAnimDoubleHandOnCall_IsCalled)
                return;

            _mirroredAnimDoubleHandOnCall_IsCalled = true;
            GameObject newExp = null;
            if (!UseMirrorThrow)
            {
                newExp = Instantiate(player.throwableBag[CurrentThrowableIndex].throwablePrefab.gameObject,
                leftHandHold.position, leftHandHold.rotation) as GameObject;
                newExp.transform.SetParent(leftHandHold);
            }
            else
            {
                newExp = Instantiate(player.throwableBag[CurrentThrowableIndex].throwablePrefab.gameObject,
                rightHandHold2.position, rightHandHold2.rotation) as GameObject;
                newExp.transform.SetParent(rightHandHold2);
            }
            if (newExp.GetComponent<Rigidbody>())
                newExp.GetComponent<Rigidbody>().isKinematic = true;
            if (newExp.GetComponent<Collider>())
                newExp.GetComponent<Collider>().enabled = false;
            cExploderClone = newExp.GetComponent<Exploder>();
            if (cExploderClone.GetComponent<LineRenderer>())
                cExploderClone.GetComponent<LineRenderer>().enabled = true;

            Events.InvokeOnPullOut(cExploderClone);

            cExploderClone.PlayRandomSoundAsWeaponChild(cExploderPrefab.sounds.pins, transform, true);
        }

        public void HandOffBomb()
        {
            if (_isThrown)
                return;
            _isThrown = true;
            ReleaseTriggers();
            UpperBodyLayerTarget = 0;
            if (cExploderClone)
            {
                ThrowCExploderClone();
            }
            cExploderClone = null;
        }

        #endregion Animation Embedded

        #region Starters

        public override void OnEnabled(Animator anim)
        {
            Events = new ThrowingEvents();
            TriggS = new LayersWithDefValue<SystemTrigger>(new ThrowSystemTriggers());

            transform = userInput.transform;
            playerCamera = userInput.cameraRig.GetComponent<PlayerCamera>();
            animator = transform.GetComponent<Animator>();

            foreach (Transform t in transform.GetComponentsInChildren<Transform>())
            {
                if (t.CompareTag("LeftHandHold"))
                    leftHandHold = t.FindChild("PosRotFixer");
                if (t.CompareTag("RightHandHold"))
                    rightHandHold2 = t.FindChild("PosRotFixer2");
            }

            foreach (Transform tr in userInput.cameraRig.GetComponentsInChildren<Transform>())
                if (tr.CompareTag("Target"))
                    refTarget = tr;
            if (userInput && userInput.cameraRig)
                foreach (var trn in userInput.cameraRig.GetComponentsInChildren<Transform>())
                {
                    if (trn.name == "TurnTo Reference")
                        TurnToReference = trn;
                }
            if (!rightHandHold2)
                Debug.Log("PosRotFixer2 not found, mirrored throw wont work properly...");
            if (!leftHandHold || !playerCamera || !animator)
            {
                userInput.DisablePlayer("Needed transform not found in " + ToString());
                return;
            }

            player.onIsHandOnBomb += HandOnBomb;
            player.onIsHandOffBomb += HandOffBomb;

            player.throwableBag.RemoveAll(x => x.haveCount <= 0 || x.throwablePrefab == null);

            CurrentThrowableIndex = player.defaultThrowableIndex >= player.throwableBag.Count ? -1 : player.defaultThrowableIndex;
            if (GetStyleOfIndex(CurrentThrowableIndex) != -1) SetThrowable(CurrentThrowableIndex);
            nextThrowableIndex = CurrentThrowableIndex;
        }

        public override void OnStart()
        {
        }

        #endregion Starters

        #region Triggers

        private bool IndicatorTrigger()
        {
            if (!TriggS.LastValue.GetTrigger(ThrowSystemTriggers.ct_IndicatorPress))
                return false;
            if (userInput.ThrowPress)
            {
                return true;
            }

            return false;
        }

        private bool ThrowTrigger()
        {
            if (!TriggS.LastValue.GetTrigger(ThrowSystemTriggers.ct_PullOut) || IsThrowing)
                return false;

            if (userInput.ThrowDown || userInput.ThrowPress)
            {
                if (!HaveAThrowableSelected() && player.throwableBag.Any(x => x.haveCount > 0))
                {
                    SetThrowable(player.throwableBag.FindIndex(x => x.haveCount > 0));
                }
                if (HaveAnyThrowable() && HaveAThrowableSelected() && animator.GetInteger(cap_Throw) <= 0/*&& !player.smbWeapon.IsAiming*/ )
                {
                    if (UseMirrorThrow)
                        animator.SetBool("MirrorThrow", true);
                    else
                        animator.SetBool("MirrorThrow", false);
                    _mirroredAnimDoubleHandOnCall_IsCalled = false;
                    player.SmbWeapon.TriggS.Override(c_overrideKey, 1, new WeaponSystemTriggers(false));
                    player.SmbWeapon.LeftHandEnabled = false;
                    Exploder expPrefab = player.throwableBag[CurrentThrowableIndex].throwablePrefab;

                    if (player.SmbLoco.IsLocomoting)
                        player.SmbLoco.OverrideLocomoteStatic(
                            new LocomoteStaticParams(TurnToReference, expPrefab.locomotionStyleIndex == -1 ? player.defaultLocomStyleIndex : expPrefab.locomotionStyleIndex), 1, c_overrideKey);
                    if (player.SmbCover.IsInCover)
                        player.SmbLookIK.OverrideLookAtTransform(
                            new LookAtTransformParams(refTarget, player.transform, expPrefab.lookIKStyleIndex == -1 ? player.defaultLookAtIKStyleIndex : expPrefab.lookIKStyleIndex), 1, c_overrideKey);

                    animator.SetInteger(cap_Throw, 1);
                    UpperBodyLayerTarget = 1;

                    SetThrowable(CurrentThrowableIndex);
                    IsThrowing = true;
                    _isThrown = false;

                    return true;
                }
                else
                {
                    Events.InvokeOnNoGrenadeTry();
                }
            }

            return false;
        }

        private bool SwitchThrowableTrigger(bool isWithThrowable = true, bool intentionalNext = false)
        {
            if (!TriggS.LastValue.GetTrigger(ThrowSystemTriggers.ct_Switch_Throwable) || player.throwableBag.Count <= 1 || !HaveAnyThrowable())
                return false;

            if (userInput.MenuLeftDown || intentionalNext)
            {
                nextThrowableIndex = (CurrentThrowableIndex + 1) % player.throwableBag.Count;
            }
            else if (userInput.MenuRightDown)
            {
                nextThrowableIndex = (CurrentThrowableIndex - 1) < 0 ? player.throwableBag.Count - 1 : (CurrentThrowableIndex - 1);
            }
            if (nextThrowableIndex == CurrentThrowableIndex)
                return false;

            if (!isWithThrowable)
            {
                SetThrowable(nextThrowableIndex);
                Events.InvokeOnSwitch(player.throwableBag[CurrentThrowableIndex].throwablePrefab, player.throwableBag[nextThrowableIndex].throwablePrefab);
                return true;
            }

            if (!IsChangingThrowable)
            {
                animator.SetInteger(cap_Throw, 2);
                IsChangingThrowable = true;

                if (cExploderClone)
                {
                    Destroy(cExploderClone.gameObject);
                }

                Events.InvokeOnSwitch(player.throwableBag[CurrentThrowableIndex].throwablePrefab, player.throwableBag[nextThrowableIndex].throwablePrefab);

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
                _mirroredAnimDoubleHandOnCall_IsCalled = false;

                player.SmbWeapon.LeftHandEnabled = true;

                UpperBodyLayerTarget = 0;

                ReleaseTriggers();
            }
            else if (stateInfo.shortNameHash == h_PullOut)
            {
                SetThrowable(nextThrowableIndex);
                if (cExploderPrefab.leftHandHoldPositionRotation && !UseMirrorThrow)
                {
                    leftHandHold.localRotation = cExploderPrefab.leftHandHoldPositionRotation.localRotation;
                    leftHandHold.localPosition = cExploderPrefab.leftHandHoldPositionRotation.localPosition;
                }
                else if (cExploderPrefab.rightHandHoldPositionRotation && UseMirrorThrow)
                {
                    rightHandHold2.localRotation = cExploderPrefab.rightHandHoldPositionRotation.localRotation;
                    rightHandHold2.localPosition = cExploderPrefab.rightHandHoldPositionRotation.localPosition;
                }

                if (cExploderPrefab)
                    cExploderPrefab.PlayRandomSoundAsWeaponChild(cExploderPrefab.sounds.pulls, transform, true, transform);
            }
            else if (stateInfo.shortNameHash == h_ToReady)
            {
                animator.SetInteger(cap_Throw, 1);
            }
            else if (stateInfo.shortNameHash == h_ReadyIdle)
            {
                throwDir = Quaternion.AngleAxis(-cExploderClone.additionalThrowAngleHorizontal, playerCamera.transform.right) * playerCamera.transform.forward;

                if (IndicatorTrigger() /*|| 1 == 1*/)
                {
                    if (!playerCamera.IsOverridenWithKey(c_overrideKey))
                        if (!player.SmbCover.IsInCover)
                            playerCamera.OverrideCamera(CThrowProps.cameraProps, 1, c_overrideKey);
                        else if (playerCamera.IsOverridenWithKey(c_overrideKey))
                            playerCamera.ChangePointerOfCamModderWithID(CThrowProps.cameraProps, c_overrideKey);
                }
            }
            else if (stateInfo.shortNameHash == h_Throw)
            {
                if (cExploderClone)
                    cExploderClone.PlayRandomSoundAsWeaponChild(cExploderPrefab.sounds.throws, transform, transform);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            SetLayerWeights(layerIndex);
            if (stateInfo.shortNameHash == h_Empty)
            {
                if (UseMirrorThrow)
                    animator.SetBool("MirrorThrow", true);
                else
                    animator.SetBool("MirrorThrow", false);
                if (!animator.IsInTransition(layerIndex) && ThrowTrigger())
                    return;
                if (!animator.IsInTransition(layerIndex) && SwitchThrowableTrigger(false))
                {
                    return;
                }
            }
            else if (stateInfo.shortNameHash == h_PullOut)
            {
            }
            else if (stateInfo.shortNameHash == h_ToReady)
            {
            }
            else if (stateInfo.shortNameHash == h_ReadyIdle)
            {
                if (!cExploderClone && animator.GetInteger(cap_Throw) != 2)
                {
                    // Unintentional Explosion
                    IsThrowing = false;
                    animator.SetInteger(cap_Throw, 0);
                    CalculateNextThrowable();
                    Events.InvokeThrowableExit(null);
                    return;
                }

                if (!animator.IsInTransition(layerIndex) && SwitchThrowableTrigger(true))
                {
                    return;
                }

                if (IndicatorTrigger()/* || 1 == 1*/) // uncomment to debug
                {
                    Vector3 targThrowDir = Quaternion.AngleAxis(-cExploderClone.additionalThrowAngleHorizontal, userInput.cameraRig.up) * (-transform.position + refTarget.position).normalized;

                    throwDir = targThrowDir;

                    SetThrowDistance();

                    if (cExploderClone.showEstimatedRoute)
                        cExploderClone.lineRendEnabled = true;
                    cExploderClone.FireStrength = Mathf.Lerp(
                        cExploderClone.throwForceMinMax.x, cExploderClone.throwForceMinMax.y,
                        animator.GetFloat(cap_ThrowDistance));
                    cExploderClone.FireDir = throwDir;
                }
                else /*if (1 == 0)*/ // uncomment to debug
                {
                    animator.SetInteger(cap_Throw, -1);
                    cExploderClone.lineRendEnabled = false;
                }

                SetThrowDistance();
            }
            else if (stateInfo.shortNameHash == h_Throw)
            {
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.shortNameHash == h_Empty)
            {
            }
            else if (stateInfo.shortNameHash == h_PullOut)
            {
            }
            else if (stateInfo.shortNameHash == h_ToReady)
            {
            }
            else if (stateInfo.shortNameHash == h_ReadyIdle)
            {
            }
            else if (stateInfo.shortNameHash == h_Throw)
            {
            }
        }

        #endregion StateMachineBehaviour

        #region Logic Functions

        private void SetThrowable(int index)
        {
            if (player.throwableBag.Count <= index)
            {
                Debug.Log("Wrong index throwable");
                return;
            }
            IsChangingThrowable = false;

            cExploderPrefab = player.throwableBag[index].throwablePrefab;

            CurrentThrowableIndex = index;
            nextThrowableIndex = CurrentThrowableIndex;

            CThrowProps = (cExploderPrefab.throwStyleIndex == -1 ||
                cExploderPrefab.throwStyleIndex >= player.throwingStyles.Count)
                ? player.throwingStyles[player.defaultThrowStyleIndex].throwProps : player.throwingStyles[cExploderPrefab.throwStyleIndex].throwProps;

            animator.SetFloat(cap_ThrowStyle, cExploderPrefab.throwStyle);
        }

        private void CalculateNextThrowable()
        {
            player.throwableBag[CurrentThrowableIndex].haveCount--;
            if (player.throwableBag[CurrentThrowableIndex].haveCount <= 0)
            {
                player.throwableBag.RemoveAt(CurrentThrowableIndex);
                if (!HaveAnyThrowable())
                {
                    CurrentThrowableIndex = -1;
                    nextThrowableIndex = -1;
                }
                else
                {
                    SwitchThrowableTrigger(false, true);
                }
            }
        }

        private void SetLayerWeights(int layerIndex)
        {
            if (AtMinEpsilon(animator.GetLayerWeight(layerIndex)) && UpperBodyLayerTarget == 0)
                animator.SetLayerWeight(layerIndex, 0);
            else
                animator.SetLayerWeight(layerIndex,
                      Mathf.Lerp(animator.GetLayerWeight(layerIndex), UpperBodyLayerTarget,
                      Time.deltaTime * (UpperBodyLayerTarget < .5f ? CThrowProps.layerDisableSpeed : CThrowProps.layerEnableSpeed)));
        }

        private void ReleaseTriggers()
        {
            if (player.SmbLoco.IsOverridenWithKey(c_overrideKey))
                player.SmbLoco.ReleaseOverrideLocomoteType(c_overrideKey);
            if (player.SmbLookIK.IsOverridenWithKey(c_overrideKey))
                player.SmbLookIK.ReleaseOverrideLookAt(c_overrideKey);
            if (playerCamera.IsOverridenWithKey(c_overrideKey))
                playerCamera.ReleaseOverride(c_overrideKey);
            if (player.SmbWeapon.TriggS.IsOverridenWithKey(c_overrideKey))
                player.SmbWeapon.TriggS.Release(c_overrideKey);
        }

        private void ThrowCExploderClone()
        {
            if (cExploderClone)
            {
                IsThrowing = false;

                // Throw grenade
                cExploderClone.transform.SetParent(null, true);
                Rigidbody rb = cExploderClone.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.isKinematic = false;
                    rb.GetComponent<Collider>().enabled = true;
                    rb.velocity = throwDir * Mathf.Lerp(
                        cExploderClone.throwForceMinMax.x, cExploderClone.throwForceMinMax.y,
                        animator.GetFloat(cap_ThrowDistance)) / (cExploderClone.landingFireForceDivisor == 0 ? 1 : cExploderClone.landingFireForceDivisor);
                }

                if (cExploderClone.GetComponent<LineRenderer>())
                {
                    cExploderClone.GetComponent<LineRenderer>().SetVertexCount(2);
                    cExploderClone.GetComponent<LineRenderer>().enabled = false;
                }

                cExploderClone.gameObject.AddComponent<Destroy>();
                cExploderClone.GetComponent<Destroy>().destroyTime = cExploderClone.GetComponent<Exploder>().modelDestroyTime;

                CalculateNextThrowable();
                Events.InvokeOnThrow(cExploderClone);
                Events.InvokeThrowableExit(cExploderClone);
            }
        }

        private void SetThrowDistance()
        {
            animator.SetFloat(cap_ThrowDistance,
                Mathf.Lerp(animator.GetFloat(cap_ThrowDistance), (-playerCamera.VerticalRotation - playerCamera.GetVerticalLimitsMaxMin().y) /
                            (playerCamera.GetVerticalLimitsMaxMin().x - playerCamera.GetVerticalLimitsMaxMin().y), Time.deltaTime * CThrowProps.throwParamLerpSpeed)
                );
        }

        public static bool AtMinEpsilon(float t)
        {
            if (t < .002f)
                return true;
            return false;
        }

        public static bool AtMaxEpsilon(float t)
        {
            if (t < .998f)
                return true;
            return false;
        }

        #endregion Logic Functions
    }
}