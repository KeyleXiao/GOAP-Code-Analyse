using Player.Triggers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Player
{
    public class WeaponCSMB : CustomPlayerSystemSMB
    {
        #region Const

        private static readonly int h_Empty = Animator.StringToHash("Empty");
        private static readonly int h_ModifyWeapon = Animator.StringToHash("ModifyingWeapon");
        private static readonly int h_PullOutWeapon = Animator.StringToHash("PullOutWeapon");
        private static readonly int h_IdleWithWeapon = Animator.StringToHash("IdleWithWeapon");
        private static readonly int h_AimingWithWeapon = Animator.StringToHash("AimingWithWeapon");
        private static readonly int h_HolsterWeapon = Animator.StringToHash("HolsterWeapon");
        private static readonly int h_ReloadWeapon = Animator.StringToHash("ReloadWeapon");
        private static readonly int h_GunPartReload = Animator.StringToHash("GunPartReload");
        private static readonly int hT_IdleToAiming = Animator.StringToHash("IdleWithWeapon -> AimingWithWeapon");

        private const int c_RightArmLayer = 2;
        private const int c_LeftArmLayer = 3;
        private const int c_RightHandLayer = 5;
        private const int c_LeftHandLayer = 6;

        private const string c_overrideKey = "Weapon csmb";
        private const string c_overrideKey_Locomotion = "Weapon csmb Locomotion";
        private const string c_overrideKeyModifying = "Weapon csmb Modifying";
        private const string c_overrideKeyReload = "Weapon csmb Reloading";

        private const bool c_useQuickAimTurn = true;
        private const float c_disableLayersOnHolsterAtNormalizedTime = .3f;
        private const float c_autoUnAimTimerAfterFire1ButtonDownHipFireAim = 2f;

        private static readonly int cap_WeaponPull = Animator.StringToHash("Weapon Pull");
        private static readonly int cap_Aim = Animator.StringToHash("Aim");
        private static readonly int cap_Reload = Animator.StringToHash("Reload");
        private static readonly int cap_GunPartReload = Animator.StringToHash("GunPartReload");
        private static readonly int cap_WeaponStyle = Animator.StringToHash("Weapon Style");
        private static readonly int cap_ModifyWeapon = Animator.StringToHash("Modify Weapon");

        #endregion Const

        #region Properties

        public WeaponEvents Events { get; private set; }

        public bool IsAiming
        {
            get { return IsAimingSight || IsAimingHipFire; }
            private set { }
        }

        public bool IsAimingSight { get; private set; }
        public bool IsAimingHipFire { get; private set; }
        public Transform RefTarget { get; private set; }
        public Transform TurnToReference { get; private set; }

        public Transform RightHandHold { get; private set; }
        public Transform LeftHandHold { get; private set; }
        public Transform WeaponIK { get; private set; }
        public bool LeftHandEnabled { get; set; }
        public int NextWeaponIndex { get; set; }
        public bool IsChangingWeapon { get; private set; }
        public GunAtt cGunAtt { get; private set; }
        public float RightArmLayerTarget { get; private set; }
        public float LeftArmLayerTarget { get; private set; }
        public float RightHandIKTarget { get; private set; }
        public float LeftHandIKTarget { get; private set; }
        public FireProps CFireProps { get; private set; }

        #endregion Properties

        #region Private

        private TargetLogic targetLogic;
        private Transform weaponIKParent;
        private Transform transform;
        private Animator animator;
        private int stateShortNameHashOfOnStateEnter;
        private Transform tempNewClip;
        private int currentWeaponIndex;
        private float tmp_UnAimTimerAfterFire1Down = 0;
        private PlayerCamera playerCamera;
        private float weaponBodyBob;
        private float nextFireTimer;
        private float nextSecFireTimer;
        private float tapTimerCounter;
        private float waitOnAimTrigger;
        private float randomTwistSign;
        private Transform fireReference;
        private int ammoBagIndexFoundOnReloadDown;
        private bool blendedOutReloadAnimEventBug_onReloadDone = false;
        private bool blendedOutReloadAnimEventBug_onClipLeftHand = false;
        private ItemsAround itemChecker;
        private Quaternion weaponSpread;
        private int currentFocusedHolderIndex;
        private List<GunPart> partPrefabsForThisIndex;
        private Transform activeChildPart;
        private int activeIndex;
        private float projectilePrefabDamage;
        private bool blendedOutPullOutAnimEventBug_onIsHandOnGun;

        #endregion Private

        #region Public Checkers

        public Sprite GetCurrentWeaponSprite()
        {
            if (!cGunAtt || currentWeaponIndex == -1 || player.weapons.Count == 0)
                return null;
            return cGunAtt.hudSprite;
        }

        public string GetCurrentWeaponName()
        {
            if (!cGunAtt || currentWeaponIndex == -1 || player.weapons.Count == 0)
                return "";
            return cGunAtt.weaponName;
        }

        public Sprite GetCurrentBulletSprite()
        {
            if (!cGunAtt || currentWeaponIndex == -1 || player.weapons.Count == 0)
                return null;
            return cGunAtt.currentProjectilePrefab.hudSprite;
        }

        public string GetCurrentBulletName()
        {
            if (!cGunAtt || currentWeaponIndex == -1 || player.weapons.Count == 0)
                return "";
            return cGunAtt.currentProjectilePrefab.projectileName;
        }

        public Sprite GetSecCurrentBulletSprite()
        {
            if (!cGunAtt || !cGunAtt.GetComponent<ModifiableGun>() || !cGunAtt.GetComponent<ModifiableGun>().SecFireGp || currentWeaponIndex == -1 || player.weapons.Count == 0)
                return null;
            return cGunAtt.GetComponent<ModifiableGun>().SecFireGp.currentProjectilePrefab.hudSprite;
        }

        public string GetSecCurrentBulletName()
        {
            if (!cGunAtt || !cGunAtt.GetComponent<ModifiableGun>() || !cGunAtt.GetComponent<ModifiableGun>().SecFireGp || currentWeaponIndex == -1 || player.weapons.Count == 0)
                return "";
            return cGunAtt.GetComponent<ModifiableGun>().SecFireGp.currentProjectilePrefab.projectileName;
        }

        public int GetTotalAmmoCount()
        {
            if (!cGunAtt || currentWeaponIndex == -1 || player.weapons.Count == 0 || GetCurrentUsedBagIndex() == -1)
                return 0;
            return player.ammoBag[GetCurrentUsedBagIndex()].haveCount;
        }

        public int GetCurrentClip()
        {
            if (!cGunAtt || currentWeaponIndex == -1 || player.weapons.Count == 0)
                return 0;
            return cGunAtt.currentClipCapacity;
        }

        public int GetSecTotalAmmoCount()
        {
            if (!cGunAtt || !cGunAtt.GetComponent<ModifiableGun>() || !cGunAtt.GetComponent<ModifiableGun>().SecFireGp || !cGunAtt.GetComponent<ModifiableGun>().SecFireGp.currentProjectilePrefab || currentWeaponIndex == -1 || player.weapons.Count == 0 || GetCurrentUsedBagIndex() == -1)
                return 0;
            foreach (var bag in player.ammoBag)
            {
                if (bag.projectilePrefab == cGunAtt.GetComponent<ModifiableGun>().SecFireGp.currentProjectilePrefab)
                    return bag.haveCount;
            }
            return 0;
        }

        public int GetSecCurrentClip()
        {
            if (!cGunAtt || !cGunAtt.GetComponent<ModifiableGun>() || !cGunAtt.GetComponent<ModifiableGun>().SecFireGp || currentWeaponIndex == -1 || player.weapons.Count == 0)
                return 0;
            return cGunAtt.GetComponent<ModifiableGun>().SecFireGp.currentClipCapacity;
        }

        public ProjectileBase GetCurrentUsedProjectileOnAmmoBag()
        {
            if (player.weapons[currentWeaponIndex].GetComponent<GunAtt>().currentProjectilePrefab)
            {
                if (player.ammoBag.FirstOrDefault(x => x.projectilePrefab ==
                 player.weapons[currentWeaponIndex].GetComponent<GunAtt>().currentProjectilePrefab)
                != null)
                {
                    return player.ammoBag.FirstOrDefault(x => x.projectilePrefab ==
                player.weapons[currentWeaponIndex].GetComponent<GunAtt>().currentProjectilePrefab).projectilePrefab;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        public int GetCurrentUsedBagIndex()
        {
            if (GetCurrentUsedProjectileOnAmmoBag() != null)
                return player.ammoBag.FindIndex(x => x.projectilePrefab == GetCurrentUsedProjectileOnAmmoBag());
            return -1;
        }

        public GunAtt GetCurrentWeaponScript()
        {
            return cGunAtt;
        }

        public GunAtt GetWeaponScriptOf(int index)
        {
            if (index < player.weapons.Count)
                return player.weapons[index].GetComponent<GunAtt>();
            return null;
        }

        public int GetCGunStyle()
        {
            if (GetCurrentWeaponScript())
                return GetCurrentWeaponScript().gunStyle;
            return -1;
        }

        public bool HaveAnyWeapon()
        {
            player.weapons.RemoveAll(x => x == null);
            return player.weapons.Count > 0;
        }

        #endregion Public Checkers

        #region Animation Embedded Events

        private void OnIsHandOnGun()
        {
            if (!blendedOutPullOutAnimEventBug_onIsHandOnGun)
                blendedOutPullOutAnimEventBug_onIsHandOnGun = true;
            else
                return;
            cGunAtt.gameObject.SetActive(true);

            if (RightHandHold && cGunAtt && cGunAtt.GetFixer(player.transform.name, WeaponFixerTypes.AnimRightHand))
            {
                RightHandHold.localPosition = cGunAtt.GetFixer(player.transform.name, WeaponFixerTypes.AnimRightHand).localPosition;
                RightHandHold.localRotation = cGunAtt.GetFixer(player.transform.name, WeaponFixerTypes.AnimRightHand).localRotation;
            }

            cGunAtt.transform.SetParent(RightHandHold, false);
            cGunAtt.transform.localPosition = Vector3.zero;
            cGunAtt.transform.localRotation = Quaternion.identity;

            SetHandIKTarget(true, GetHandTarget(true, cGunAtt));

            if (player.useStaticMovement)
                player.SmbLoco.OverrideLocomoteStatic(new LocomoteStaticParams(TurnToReference, player.SmbLoco.HasBindedLocomStyleToWeaponStyle(GetCGunStyle())), 1, c_overrideKey_Locomotion);
            else
                player.SmbLoco.OverrideLocomoteFree(
                            new LocomoteFreeParams(player.SmbLoco.HasBindedLocomStyleToWeaponStyle(GetCGunStyle())), 1, c_overrideKey_Locomotion
                            );
        }

        private void OnIsHandAwayFromGun()
        {
            cGunAtt.transform.SetParent(null);
            cGunAtt.gameObject.SetActive(false);
            SetHandIKTarget(true, 0);
            Events.InvokeWeaponHolster(cGunAtt);
        }

        private void OnNewClipInLeftHand()
        {
            if (!blendedOutReloadAnimEventBug_onClipLeftHand)
                blendedOutReloadAnimEventBug_onClipLeftHand = true;
            else
                return;

            // instantiate new clip in left hand
            if (cGunAtt && cGunAtt.curClipPrefab)
            {
                // instantiate new clip
                tempNewClip = cGunAtt.InstantiateReturn(cGunAtt.curClipPrefab);
                tempNewClip.SetParent(LeftHandHold, true);

                tempNewClip.localPosition = Vector3.zero;
                tempNewClip.localRotation = Quaternion.identity;

                LeftHandHold.localPosition = cGunAtt.GetFixer(player.transform.name, WeaponFixerTypes.LeftHandClip).localPosition;
                LeftHandHold.localRotation = cGunAtt.GetFixer(player.transform.name, WeaponFixerTypes.LeftHandClip).localRotation;

                tempNewClip.GetComponent<Rigidbody>().isKinematic = true;
                tempNewClip.GetComponent<Collider>().enabled = false;

                cGunAtt.StartCoroutine(cGunAtt.FixClipPosInLHand(tempNewClip));

                return;
            }
        }

        private void OnNewClipOffLeftHand()
        {
            // new clip goes to weapon
            if (tempNewClip)
            {
                tempNewClip.SetParent(cGunAtt.transform);
                tempNewClip.localPosition = cGunAtt.clipDefLocalPos;
                tempNewClip.localRotation = cGunAtt.clipDefLocalRot;
                cGunAtt.curClipObject = tempNewClip;
            }
            tempNewClip = null;
        }

        private void OnReloadDone()
        {
            if (!blendedOutReloadAnimEventBug_onReloadDone)
                blendedOutReloadAnimEventBug_onReloadDone = true;
            else
                return;

            AmmoBag bag = player.ammoBag[ammoBagIndexFoundOnReloadDown];

            if (bag.haveCount < cGunAtt.maxClipCapacity - cGunAtt.currentClipCapacity)
            {
                cGunAtt.currentClipCapacity += bag.haveCount;
                bag.haveCount = 0;
            }
            else
            {
                bag.haveCount -= cGunAtt.maxClipCapacity - cGunAtt.currentClipCapacity;
                cGunAtt.currentClipCapacity = cGunAtt.maxClipCapacity;
            }

            Events.InvokeWeaponReloadDone(cGunAtt);
        }

        private void OnGunPartReloadDone()
        {
        }

        #endregion Animation Embedded Events

        #region Starter

        public override void OnEnabled(Animator anim)
        {
            Events = new WeaponEvents();
            TriggS = new LayersWithDefValue<SystemTrigger>(new WeaponSystemTriggers());

            LeftHandEnabled = true;
            transform = player.transform;
            animator = anim;

            foreach (Transform tr in userInput.cameraRig.GetComponentsInChildren<Transform>())
                if (tr.CompareTag("Target"))
                {
                    RefTarget = tr;
                    weaponIKParent = RefTarget.parent;
                    targetLogic = RefTarget.GetComponent<TargetLogic>();
                }

            if (player.transform.FindChild("ItemChecker"))
                itemChecker = player.transform.FindChild("ItemChecker").GetComponent<ItemsAround>();

            foreach (Transform t in transform.GetComponentsInChildren<Transform>())
                if (t.CompareTag("RightHandHold"))
                    RightHandHold = t.FindChild("PosRotFixer");
            foreach (Transform t in transform.GetComponentsInChildren<Transform>())
                if (t.CompareTag("LeftHandHold"))
                    LeftHandHold = t.FindChild("PosRotFixer");
            foreach (Transform t in transform.GetComponentsInChildren<Transform>())
                if (t.CompareTag("WeaponIK"))
                    WeaponIK = t.FindChild("PosRotFixer");

            playerCamera = userInput.cameraRig.GetComponent<PlayerCamera>();

            if (userInput && userInput.cameraRig)
                foreach (var trn in userInput.cameraRig.GetComponentsInChildren<Transform>())
                {
                    if (trn.name == "Fire Reference")
                        fireReference = trn;
                    if (trn.name == "TurnTo Reference")
                        TurnToReference = trn;
                }

            if (!RightHandHold || !TurnToReference || !LeftHandHold || !WeaponIK || !RefTarget || !weaponIKParent || !playerCamera || !fireReference || !targetLogic)
            {
                userInput.DisablePlayer("Needed transform not found in " + ToString());
            }
            if (!itemChecker)
                Debug.Log("Ttem Checker not found...Collecting won't work...");

            player.onIsHandOnGun += OnIsHandOnGun;
            player.onIsHandAwayFromGun += OnIsHandAwayFromGun;
            player.onNewClipInLeftHand += OnNewClipInLeftHand;
            player.onNewClipOffLeftHand += OnNewClipOffLeftHand;
            player.onReloadDone += OnReloadDone;
            player.onGunPartReloadDone += OnGunPartReloadDone;

            currentWeaponIndex = player.defaultWeaponIndex >= player.weapons.Count ? 0 : player.defaultWeaponIndex;
            NextWeaponIndex = currentWeaponIndex;

            foreach (GameObject go in player.weapons)
            {
                go.SetActive(false);
                if (go.GetComponent<Collider>())
                    go.GetComponent<Collider>().enabled = false;
                if (go.GetComponent<Rigidbody>())
                    go.GetComponent<Rigidbody>().isKinematic = true;
            }
            SetWeaponFireStyle(player.defaultWeaponFireStyleIndex);
            if (CFireProps == null)
                userInput.DisablePlayer("Default weapon fire style not found in list");

            if (HaveAnyWeapon())
            {
                SetWeapon(true);
                foreach (var modifiableGun in player.weapons)
                {
                    LearnGunParts(player, modifiableGun.GetComponent<ModifiableGun>());
                }
            }
            NextWeaponIndex = currentWeaponIndex;
        }

        #endregion Starter

        #region Logic Functions

        public void SetWeapon(bool changePlayerWeaponStyle = false)
        {
            if (NextWeaponIndex == -1)
            {
                Debug.Log("Next weapon index is -1. Cant set weapon");
                return;
            }
            if (NextWeaponIndex >= player.weapons.Count)
            {
                Debug.Log("Next weapon index is bigger than weapon count. Cant set weapon");
                return;
            }

            animator.SetFloat(cap_WeaponStyle, player.weapons[NextWeaponIndex].GetComponent<GunAtt>().gunStyle);

            IsChangingWeapon = false;

            cGunAtt = player.weapons[NextWeaponIndex].GetComponent<GunAtt>();

            projectilePrefabDamage = 0;
            if (cGunAtt.currentProjectilePrefab)
            {
                if (cGunAtt.currentProjectilePrefab.GetComponent<RaycastBullet>())
                    projectilePrefabDamage = cGunAtt.currentProjectilePrefab.GetComponent<RaycastBullet>().damage;
            }

            currentWeaponIndex = NextWeaponIndex;

            if (changePlayerWeaponStyle)
                SetWeaponFireStyle(cGunAtt.gunStyle);
        }

        public void SetWeaponFireStyle(int gunStyle)
        {
            foreach (var style in player.weaponFireStyles)
            {
                if (style.bindedWeaponStyles.Contains(gunStyle))
                {
                    CFireProps = style.fireProps;
                    return;
                }
            }
            CFireProps = player.weaponFireStyles[player.defaultWeaponFireStyleIndex].fireProps;
        }

        public void SetLayerWeightTargets(float _rightArmLayerTarget, float _leftArmLayerTarget)
        {
            RightArmLayerTarget = _rightArmLayerTarget;
            LeftArmLayerTarget = _leftArmLayerTarget;
        }

        public void SetLayerWeightAndHandIKTargets(float _leftHandLayerWeightTarget, float _rightHandLayerWeightTarget, float _leftHandIKTarget, float _rightHandIKTarget)
        {
            SetLayerWeightTargets(_rightHandLayerWeightTarget, _leftHandLayerWeightTarget);
            SetHandIKTarget(true, _leftHandIKTarget);
            SetHandIKTarget(false, _rightHandIKTarget);
        }

        public void SetLayerWeightAndHandIKTargets(float leftHandTargets, float rightHandTargets)
        {
            SetLayerWeightAndHandIKTargets(leftHandTargets, rightHandTargets, leftHandTargets, rightHandTargets);
        }

        public void SetHandIKTarget(bool isLeftHand = true, float target = 1)
        {
            if (isLeftHand)
            {
                LeftHandIKTarget = target;
            }
            else
            {
                RightHandIKTarget = target;
            }
        }

        private void UpdateLayerWeights()
        {
            float leftIKTarget = LeftHandIKTarget;
            leftIKTarget = LeftHandEnabled ? LeftHandIKTarget : 0;

            CFireProps.lHandAim = Mathf.Lerp(CheckEpsilon(CFireProps.lHandAim, leftIKTarget), leftIKTarget, Time.deltaTime * (leftIKTarget == 1 ? CFireProps.leftHandSmooth : CFireProps.leftHandBackSmooth));

            float leftLayerTarget = LeftArmLayerTarget;
            leftLayerTarget = LeftHandEnabled ? leftLayerTarget : 0;

            if (AtMinEpsilon(animator.GetLayerWeight(c_RightArmLayer)) && RightArmLayerTarget == 0)
                animator.SetLayerWeight(c_RightArmLayer, 0);
            else
                animator.SetLayerWeight(c_RightArmLayer,
                      Mathf.Lerp(animator.GetLayerWeight(c_RightArmLayer), RightArmLayerTarget, Time.deltaTime * (RightArmLayerTarget > .5f ? CFireProps.rightArmLayerEnableSpeed : CFireProps.rightArmLayerDisableSpeed)));
            if (AtMinEpsilon(animator.GetLayerWeight(c_LeftArmLayer)) && leftLayerTarget == 0)
                animator.SetLayerWeight(c_LeftArmLayer, 0);
            else
                animator.SetLayerWeight(c_LeftArmLayer,
                  Mathf.Lerp(animator.GetLayerWeight(c_LeftArmLayer), leftLayerTarget, Time.deltaTime * (leftLayerTarget > .5f ? CFireProps.leftArmLayerEnableSpeed : CFireProps.leftArmLayerDisableSpeed)));

            if (cGunAtt && cGunAtt.overrideRightHand)
            {
                animator.SetLayerWeight(c_RightHandLayer,
                    (IsAiming ||
                    animator.GetCurrentAnimatorStateInfo(c_RightArmLayer).shortNameHash == h_IdleWithWeapon ||
                    animator.GetCurrentAnimatorStateInfo(c_RightArmLayer).shortNameHash == h_ModifyWeapon ||
                    animator.GetCurrentAnimatorStateInfo(c_RightArmLayer).shortNameHash == h_ReloadWeapon) ? 1 : 0/*animator.GetLayerWeight(c_RightArmLayer)*/);
                animator.SetInteger("RightHand", cGunAtt.rightHandAnimNo);
            }
            else
            {
                animator.SetLayerWeight(c_RightHandLayer, 0);
            }

            if (cGunAtt)
            {
                if (animator.GetCurrentAnimatorStateInfo(c_RightArmLayer).shortNameHash == h_IdleWithWeapon)
                {
                    if (cGunAtt.overrideLeftHandOnIdle)
                    {
                        animator.SetLayerWeight(c_LeftHandLayer, animator.GetLayerWeight(c_LeftArmLayer));
                        animator.SetInteger("LeftHand", cGunAtt.leftHandAnimNoOnIdle);
                    }
                    else
                    {
                        animator.SetLayerWeight(c_LeftHandLayer, 0);
                    }
                }
                else if (animator.GetCurrentAnimatorStateInfo(c_RightArmLayer).shortNameHash == h_AimingWithWeapon)
                {
                    if (cGunAtt.overrideLeftHandOnAim)
                    {
                        animator.SetLayerWeight(c_LeftHandLayer, animator.GetLayerWeight(c_LeftArmLayer));
                        animator.SetInteger("LeftHand", cGunAtt.leftHandAnimNoOnAim);
                    }
                    else
                    {
                        animator.SetLayerWeight(c_LeftHandLayer, 0);
                    }
                }
                else
                {
                    animator.SetLayerWeight(c_LeftHandLayer, 0);
                }
            }
            else
            {
                animator.SetLayerWeight(c_LeftHandLayer, 0);
            }
        }

        public float GetHandTarget(bool isLeft, GunAtt gunAtt)
        {
            if (isLeft)
            {
                if (stateShortNameHashOfOnStateEnter == h_PullOutWeapon)
                    return gunAtt.enableLeftHandOnPullOut ? 1 : 0;
                else if (stateShortNameHashOfOnStateEnter == h_IdleWithWeapon)
                    return gunAtt.enableLeftHandOnIdle ? 1 : 0;
                else if (stateShortNameHashOfOnStateEnter == h_AimingWithWeapon)
                {
                    if (IsAimingHipFire)
                        return gunAtt.enableLeftHandOnHipFireAim ? 1 : 0;
                    else if (IsAimingSight)
                        return gunAtt.enableLeftHandOnAimSight ? 1 : 0;
                }
                else if (stateShortNameHashOfOnStateEnter == h_ReloadWeapon)
                    return gunAtt.enableLeftHandOnReload ? 1 : 0;
                else if (stateShortNameHashOfOnStateEnter == h_HolsterWeapon)
                    return gunAtt.enableLeftHandOnHolster ? 1 : 0;
                else if (stateShortNameHashOfOnStateEnter == h_ModifyWeapon)
                    return 0;
                else return 0;
            }
            else
            {
                if (stateShortNameHashOfOnStateEnter == h_IdleWithWeapon)
                    return gunAtt.enableRightHandOnIdle ? 1 : 0;
                else if (stateShortNameHashOfOnStateEnter == h_ModifyWeapon)
                    return 1;
            }

            return 1;
        }

        #endregion Logic Functions

        #region Triggers

        private void FlashLightTrigger()
        {
            if (!TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_Flashlight) || !cGunAtt || !cGunAtt.GetComponent<ModifiableGun>())
                return;
            if (userInput.FlashLightDown)
                cGunAtt.GetComponent<ModifiableGun>().FlashLightOnOff();
        }

        private bool WeaponPullOutTrigger()
        {
            if (userInput.HolsterDown && TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_Holster) && HaveAnyWeapon())
            {
                if (cGunAtt)
                {
                    animator.SetInteger(cap_WeaponPull, 1);
                    return true;
                }
            }
            return false;
        }

        private bool WeaponHolsterTrigger()
        {
            if (userInput.HolsterDown && TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_Holster))
            {
                if (cGunAtt)
                {
                    animator.SetInteger(cap_WeaponPull, 0);
                    return true;
                }
            }
            return false;
        }

        private bool WeaponDropTrigger(bool intentionalDrop = false, bool isWithWeapon = true)
        {
            if (!TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_Drop) || !HaveAnyWeapon())
                return false;

            if (userInput.DropDown || intentionalDrop)
            {
                GunAtt oldGunAtt = cGunAtt, newGunAtt = null;

                cGunAtt.gameObject.SetActive(true);
                cGunAtt.transform.position = player.transform.position + player.transform.forward * .5f;

                int oldIndex = player.weapons.FindIndex(x => x.gameObject);
                NextWeaponIndex = oldIndex == player.weapons.Count - 1 ? oldIndex - 1 : oldIndex;

                player.weapons.Remove(cGunAtt.gameObject);

                if (cGunAtt.GetComponent<Rigidbody>())
                {
                    cGunAtt.GetComponent<Rigidbody>().isKinematic = false;
                    cGunAtt.GetComponent<Rigidbody>().AddForce(transform.forward * 1);
                }
                if (cGunAtt.GetComponent<Collider>())
                    cGunAtt.GetComponent<Collider>().enabled = true;
                cGunAtt.transform.SetParent(null);

                if (isWithWeapon)
                {
                    if (player.SmbLoco.IsOverridenWithKey(c_overrideKey))
                        player.SmbLoco.ReleaseOverrideLocomoteType(c_overrideKey);
                    player.SmbLookIK.ReleaseOverrideLookAt(c_overrideKey);
                    animator.SetInteger(cap_WeaponPull, 2);
                }

                if (!HaveAnyWeapon())
                {
                    newGunAtt = null;
                    currentWeaponIndex = -1;
                    NextWeaponIndex = -1;
                    Events.InvokeWeaponDrop(oldGunAtt, newGunAtt);

                    return true;
                }
                SetLayerWeightAndHandIKTargets(0, 0);
                SetWeapon();
                Events.InvokeWeaponDrop(oldGunAtt, newGunAtt);
                return true;
            }
            return false;
        }

        private bool WeaponCollectTrigger()
        {
            if (!TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_Collect) || !itemChecker || itemChecker.collectableWeapons.Count == 0 ||
               (itemChecker.BestPickable && player.weapons.Contains(itemChecker.BestPickable.gameObject)))
                return false;

            if (player.weapons.Count < player.maxWeaponCarryCount && itemChecker.BestPickable && itemChecker.BestPickable.GetComponent<GunAtt>())
                player.TryPrintInformationText("Collecting Weapon" + itemChecker.BestPickable.name, .15f, "Press use button to collect :" + itemChecker.BestPickable.GetComponent<GunAtt>().weaponName);

            if (userInput.UseDown)
            {
                if (player.weapons.Count >= player.maxWeaponCarryCount)
                {
                    player.TryPrintInformationText("Max carry count reached", 2f, "You can't carry more weapon, drop your weapon to collect :" + itemChecker.BestPickable.GetComponent<GunAtt>().weaponName);
                    return false;
                }

                if (!HaveAnyWeapon())
                {
                    player.weapons.Add(itemChecker.BestPickable.gameObject);
                    NextWeaponIndex = 0;
                    currentWeaponIndex = 0;
                    SetWeapon();
                }
                else
                {
                    player.weapons.Insert(currentWeaponIndex + 1, itemChecker.BestPickable.gameObject);
                }
                LearnGunParts(player, itemChecker.BestPickable.GetComponent<ModifiableGun>());

                if (itemChecker.BestPickable.GetComponent<Collider>())
                    itemChecker.BestPickable.GetComponent<Collider>().enabled = false;
                if (itemChecker.BestPickable.GetComponent<Rigidbody>())
                    itemChecker.BestPickable.GetComponent<Rigidbody>().isKinematic = true;

                itemChecker.BestPickable.gameObject.SetActive(false);
                itemChecker.collectableWeapons.Remove(itemChecker.BestPickable);

                Events.InvokeWeaponCollect(itemChecker.BestPickable.GetComponent<GunAtt>());

                
                return true;
            }

            return false;
        }

        private void AmmoCollectTrigger()
        {
            if (!TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_Collect) || !itemChecker || itemChecker.collectableWeapons.Count == 0)
                return;
            foreach (var weaponTransform in itemChecker.collectableWeapons)
            {
                if (player.weapons.Contains(weaponTransform.gameObject) || weaponTransform.GetComponent<GunAtt>().currentClipCapacity <= 0)
                    continue;
                GunAtt gunAtt = weaponTransform.GetComponent<GunAtt>();
                gunAtt.currentClipCapacity -=
                    CollectAmmo(weaponTransform.GetComponent<GunAtt>().currentProjectilePrefab, weaponTransform.GetComponent<GunAtt>().currentClipCapacity);
            }
        }

        private void SupplyBoxAmmoCollectTrigger()
        {
            if (!TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_Collect) || !itemChecker || !itemChecker.CAmmoBag)
                return;

            foreach (var ammoBag in itemChecker.CAmmoBag.ammoBags)
            {
                if (ammoBag.haveCount > 0)
                {
                    if (userInput.UseDown)
                        CollectAmmo(ammoBag.projectilePrefab, ammoBag.haveCount);
                    AmmoBag plBag = player.ammoBag.Find(x => x.projectilePrefab == ammoBag.projectilePrefab);
                    if (plBag != null)
                    {
                        if (plBag.haveCount < plBag.maxCarryCount && ammoBag.haveCount > 0)
                            player.TryPrintInformationText("Press use to collect ammo", .3f, "Press use button to collect :" + ammoBag.projectilePrefab.projectileName);
                    }
                }
            }
            foreach (var throwableBag in itemChecker.CAmmoBag.throwableBags)
            {
                if (throwableBag.haveCount > 0)
                {
                    if (userInput.UseDown)
                        CollectThrowable(throwableBag.throwablePrefab, throwableBag.haveCount);
                    ThrowableBag plBag = player.throwableBag.Find(x => x.throwablePrefab == throwableBag.throwablePrefab);
                    if (plBag != null)
                    {
                        if (plBag.haveCount < plBag.maxCarryCount && throwableBag.haveCount > 0)
                            player.TryPrintInformationText("Press use to collect throwable", .3f, "Press use button to collect :" + throwableBag.throwablePrefab.throwableName);
                    }
                }
            }
        }

        private int CollectThrowable(Exploder throwablePrefab, int count)
        {
            if (player.throwableBag.Find(x => x.throwablePrefab == throwablePrefab) == null)
                player.throwableBag.Add(new ThrowableBag(throwablePrefab, 0, 15));

            ThrowableBag bag = player.throwableBag.Find(x => x.throwablePrefab == throwablePrefab);
            if (bag.haveCount >= bag.maxCarryCount)
            {
                player.TryPrintInformationText("Can't Collect Throwable", .5f, "Max carry capacity :" + throwablePrefab.throwableName);
                return 0;
            }
            if (count > bag.maxCarryCount - bag.haveCount)
            {
                count = count - (bag.maxCarryCount - bag.haveCount);
                bag.haveCount = bag.maxCarryCount;
                PlayCollectSound();

                Events.InvokeSupplyCollect();
                player.TryPrintInformationText("Collected Throwable" + throwablePrefab.throwableName, 2.5f, "Collected :" + count + ", " + throwablePrefab.throwableName);

                return count;
            }
            bag.haveCount += count;
            PlayCollectSound();
            Events.InvokeSupplyCollect();
            player.TryPrintInformationText("Collected Throwable" + throwablePrefab.throwableName, 2.5f, "Collected :" + count + ", " + throwablePrefab.throwableName);

            return count;
        }

        private int CollectAmmo(ProjectileBase projectilePrefab, int count)
        {
            if (player.ammoBag.Find(x => x.projectilePrefab == projectilePrefab) == null)
                player.ammoBag.Add(new AmmoBag(projectilePrefab, 0, 100));

            AmmoBag bag = player.ammoBag.Find(x => x.projectilePrefab == projectilePrefab);

            if (bag.haveCount >= bag.maxCarryCount)
            {
                player.TryPrintInformationText("Can't Collect Ammo", .5f, "Max carry capacity :" + projectilePrefab.projectileName);
                return 0;
            }
            if (count > bag.maxCarryCount - bag.haveCount)
            {
                count = count - (bag.maxCarryCount - bag.haveCount);
                bag.haveCount = bag.maxCarryCount;
                PlayCollectSound();
                Events.InvokeSupplyCollect();
                player.TryPrintInformationText("Collected Ammo" + projectilePrefab.projectileName, 2.5f, "Collected :" + count + ", " + projectilePrefab.projectileName);
                return count;
            }
            bag.haveCount += count;
            PlayCollectSound();
            Events.InvokeSupplyCollect();
            player.TryPrintInformationText("Collected Ammo" + projectilePrefab.projectileName, 2.5f, "Collected :" + count + ", " + projectilePrefab.projectileName);

            return count;
        }

        private void PlayCollectSound()
        {
            if (CFireProps.collectAmmoSoundPrefabs.Count > 0 && itemChecker.GetComponentsInChildren<AudioSource>().Length == 0)
            {
                GameObject go = Instantiate(CFireProps.collectAmmoSoundPrefabs[Random.Range(0, CFireProps.collectAmmoSoundPrefabs.Count)], transform.position, Quaternion.identity) as GameObject;
                go.transform.SetParent(itemChecker.transform, true);
            }
        }

        private bool WeaponSwitchTrigger(bool WithWeapon = true, bool intentionalNext = false)
        {
            if (!HaveAnyWeapon() || player.weapons.Count == 1 || IsChangingWeapon)
                return false;

            if (TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_SwitchWeapon))
            {
                if (userInput.MenuUpDown || intentionalNext)
                {
                    NextWeaponIndex = (currentWeaponIndex + 1) % player.weapons.Count;
                }
                else if (userInput.MenuDownDown)
                {
                    NextWeaponIndex = (currentWeaponIndex - 1) < 0 ? player.weapons.Count - 1 : (currentWeaponIndex - 1);
                }
            }
            else
                return false;

            if (NextWeaponIndex == currentWeaponIndex)
                return false;

            if (!WithWeapon)
            {
                SetWeapon();
                Events.InvokeWeaponSwitch(GetWeaponScriptOf(currentWeaponIndex), GetWeaponScriptOf(NextWeaponIndex));
                return true;
            }

            if (!IsChangingWeapon)
            {
                animator.SetInteger(cap_WeaponPull, 0);

                IsChangingWeapon = true;
                Events.InvokeWeaponSwitch(GetWeaponScriptOf(currentWeaponIndex), GetWeaponScriptOf(NextWeaponIndex));
                return true;
            }

            return false;
        }

        private bool AimTriggerOnIdle()
        {
            if (!IsAiming)
            {
                // HipFire Aim
                if (player.SmbLoco.IsLocomoting && !player.SmbCover.IsInCover && !IsAimingSight && !IsAimingHipFire && (userInput.Fire1Down || player.PressFire1Button) && TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_AimWeapon)
                    )
                {
                    if (!playerCamera.IsOverridenWithKey(c_overrideKey))
                        playerCamera.OverrideCamera(CFireProps.camModifiersHipFireAim, 1, c_overrideKey);
                    player.SmbLoco.OverrideLocomoteStatic(new LocomoteStaticParams(TurnToReference, player.SmbLoco.HasBindedLocomStyleToWeaponStyle(GetCGunStyle())), 0, c_overrideKey);
                    if (c_useQuickAimTurn)
                        player.transform.rotation = Quaternion.LookRotation((-player.transform.position + new Vector3(RefTarget.position.x, player.transform.position.y, RefTarget.position.z)).normalized);
                    player.SmbLookIK.OverrideLookAtTransform(new LookAtTransformParams(RefTarget, player.transform, player.SmbLookIK.HasBindedLookIKStyleToWeaponStyle(GetCGunStyle())), 1, c_overrideKey);

                    player.SmbLookIK.HorizontalLookAnglePlus = cGunAtt.bodyFixRight * CFireProps.lookIKRightMultiplier;
                    player.SmbLookIK.VerticalLookAnglePlus = cGunAtt.bodyFixUp * CFireProps.lookIKUpMultiplier;

                    animator.SetBool(cap_Aim, true);
                    Events.InvokeWeaponHipFireAim(cGunAtt);
                    IsAimingHipFire = true;
                    IsAimingSight = false;
                    tmp_UnAimTimerAfterFire1Down = c_autoUnAimTimerAfterFire1ButtonDownHipFireAim;
                    nextFireTimer = -1;
                    cGunAtt.PlayRandomSoundAsWeaponChild(cGunAtt.sounds.aims, transform);

                    WeaponIK.localPosition = cGunAtt.GetFixer(player.transform.name, WeaponFixerTypes.AimHipFire).localPosition;
                    WeaponIK.localRotation = cGunAtt.GetFixer(player.transform.name, WeaponFixerTypes.AimHipFire).localRotation;

                    return true;
                }
            }

            // Sight Aim
            if (!IsAimingSight && (userInput.Fire2Press || player.PressFire2Button) && TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_AimWeapon)
                )
            {
                if (!playerCamera.IsOverridenWithKey(c_overrideKey))
                    playerCamera.OverrideCamera((CameraModderParamsBase)CFireProps.camModifiersSightAim, 1, c_overrideKey);

                if (!player.SmbLoco.IsOverridenWithKey(c_overrideKey) && player.SmbLoco.IsLocomoting)
                {
                    player.SmbLoco.OverrideLocomoteStatic(new LocomoteStaticParams(TurnToReference, player.SmbLoco.HasBindedLocomStyleToWeaponStyle(GetCGunStyle())), 0, c_overrideKey);
                    if (c_useQuickAimTurn)
                        player.transform.rotation = Quaternion.LookRotation((-player.transform.position + new Vector3(RefTarget.position.x, player.transform.position.y, RefTarget.position.z)).normalized);
                }
                if (!player.SmbLookIK.IsOverridenWithKey(c_overrideKey) && !player.SmbCover.IsInCover)
                    player.SmbLookIK.OverrideLookAtTransform(new LookAtTransformParams(RefTarget, player.transform, player.SmbLookIK.HasBindedLookIKStyleToWeaponStyle(GetCGunStyle())), 1, c_overrideKey);

                player.SmbLookIK.HorizontalLookAnglePlus = cGunAtt.bodyFixRight * CFireProps.lookIKRightMultiplier;
                player.SmbLookIK.VerticalLookAnglePlus = cGunAtt.bodyFixUp * CFireProps.lookIKUpMultiplier;

                animator.SetBool(cap_Aim, true);
                Events.InvokeWeaponSightAim(cGunAtt);
                IsAimingSight = true;
                IsAimingHipFire = false;
                tmp_UnAimTimerAfterFire1Down = -1;
                nextFireTimer = -1;
                cGunAtt.PlayRandomSoundAsWeaponChild(cGunAtt.sounds.aims, transform);

                if (player.SmbCover.IsInCover)
                {
                    WeaponIK.localPosition = cGunAtt.GetFixer(player.transform.name, WeaponFixerTypes.AimCover).localPosition;
                    WeaponIK.localRotation = cGunAtt.GetFixer(player.transform.name, WeaponFixerTypes.AimCover).localRotation;
                }
                else
                {
                    WeaponIK.localPosition = cGunAtt.GetFixer(player.transform.name, WeaponFixerTypes.AimSight).localPosition;
                    WeaponIK.localRotation = cGunAtt.GetFixer(player.transform.name, WeaponFixerTypes.AimSight).localRotation;
                }

                return true;
            }

            // UnAiming
            else if ((!IsAimingHipFire && animator.GetBool(cap_Aim) && !(userInput.Fire2Press || player.PressFire2Button))
                    )
            {
                if (player.SmbLoco.IsOverridenWithKey(c_overrideKey))
                    player.SmbLoco.ReleaseOverrideLocomoteType(c_overrideKey);
                player.SmbLookIK.ReleaseOverrideLookAt(c_overrideKey);

                player.SmbLookIK.HorizontalLookAnglePlus = 0;
                player.SmbLookIK.VerticalLookAnglePlus = 0;

                animator.SetBool(cap_Aim, false);
                Events.InvokeWeaponUnAim(cGunAtt);
                IsAimingSight = false;
                IsAimingHipFire = false;

                if (playerCamera.IsOverridenWithKey(c_overrideKey))
                    playerCamera.ReleaseOverride(c_overrideKey);
                return true;
            }

            return false;
        }

        private bool UnAimTriggerOnAiming()
        {
            if (IsAiming)
            {
                // Debug
                player.SmbLookIK.HorizontalLookAnglePlus = cGunAtt.bodyFixRight * CFireProps.lookIKRightMultiplier + weaponBodyBob;
                player.SmbLookIK.VerticalLookAnglePlus = cGunAtt.bodyFixUp * CFireProps.lookIKUpMultiplier;
            }
            else
            {
                weaponBodyBob = 0;
            }
            weaponBodyBob = Mathf.Lerp(weaponBodyBob, 0, Time.deltaTime * cGunAtt.bodyRecoverSpeedInverse * CFireProps.bodyRecoverSpeedAgentMultiplier);

            if (((!(userInput.Fire2Press || player.PressFire2Button) && IsAimingSight) || (tmp_UnAimTimerAfterFire1Down < 0 && IsAimingHipFire) || !TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_AimWeapon) ||
                (cGunAtt.GetComponent<ModifiableGun>() && cGunAtt.GetComponent<ModifiableGun>().SecFireGp && cGunAtt.GetComponent<ModifiableGun>() && cGunAtt.GetComponent<ModifiableGun>().SecFireGp.currentClipCapacity <= 0 && HaveAmmoWithCaliber(cGunAtt.GetComponent<ModifiableGun>().SecFireGp.firableBulletCaliber))
                )
                )
            {
                OnUnAim();

                return true;
            }

            // Switch to sight aim
            if (userInput.Fire2Down && IsAimingHipFire && TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_AimWeapon)) // when Fire 2 down, override HipFireAim
            {
                Events.InvokeWeaponSightAim(cGunAtt);
                IsAimingSight = true;
                IsAimingHipFire = false;

                if (playerCamera.IsOverridenWithKey(c_overrideKey))
                    playerCamera.ChangePointerOfCamModderWithID(CFireProps.camModifiersSightAim, c_overrideKey);

                WeaponIK.localPosition = cGunAtt.GetFixer(player.transform.name, WeaponFixerTypes.AimSight).localPosition;
                WeaponIK.localRotation = cGunAtt.GetFixer(player.transform.name, WeaponFixerTypes.AimSight).localRotation;
            }
            else if ((userInput.Fire1Down || userInput.FirePress || player.PressFire1Button) && IsAimingHipFire && TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_AimWeapon)
                )
            {
                tmp_UnAimTimerAfterFire1Down = c_autoUnAimTimerAfterFire1ButtonDownHipFireAim;
            }

            return false;
        }

        private void OnUnAim()
        {
            if (player.SmbLoco.IsOverridenWithKey(c_overrideKey))
                player.SmbLoco.ReleaseOverrideLocomoteType(c_overrideKey);
            player.SmbLookIK.ReleaseOverrideLookAt(c_overrideKey);

            player.SmbLookIK.HorizontalLookAnglePlus = 0;
            player.SmbLookIK.VerticalLookAnglePlus = 0;

            if (playerCamera.IsOverridenWithKey(c_overrideKey))

                playerCamera.ReleaseOverride(c_overrideKey);

            IsAimingSight = false;
            IsAimingHipFire = false;
            animator.SetBool(cap_Aim, false);
            cGunAtt.PlayRandomSoundAsWeaponChild(cGunAtt.sounds.unAims, transform);
            Events.InvokeWeaponUnAim(cGunAtt);
        }

        private bool GunPartReloadTrigger()
        {
            animator.SetFloat(cap_GunPartReload, -1);
            if (!TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_Reload))
                return false;

            if (cGunAtt && cGunAtt.GetComponent<ModifiableGun>() && cGunAtt.GetComponent<ModifiableGun>().SecFireGp && cGunAtt.GetComponent<ModifiableGun>().SecFireGp.currentClipCapacity <= 0 &&
                HaveAmmoWithCaliber(cGunAtt.GetComponent<ModifiableGun>().SecFireGp.currentProjectilePrefab.bulletCaliber)
                )
            {
                ProjectileBase projectilePrefabToUse = null;
                // Does player has bullet prefab that this weapon used on last clip
                if (PlayerHaveBulletPrefab(cGunAtt.GetComponent<ModifiableGun>().SecFireGp.currentProjectilePrefab))
                    projectilePrefabToUse = cGunAtt.GetComponent<ModifiableGun>().SecFireGp.currentProjectilePrefab;
                // Else use first prefab that this weapon can use
                else
                    projectilePrefabToUse = player.ammoBag.First(x => x.projectilePrefab.bulletCaliber == cGunAtt.GetComponent<ModifiableGun>().SecFireGp.firableBulletCaliber && x.haveCount > 0).projectilePrefab;

                int secAmmoBagIndex = player.ammoBag.FindIndex(x => x.projectilePrefab == projectilePrefabToUse);

                animator.SetFloat(cap_GunPartReload, cGunAtt.GetComponent<ModifiableGun>().SecFireGp.reloadAnimParam);

                player.SmbCover.TriggS.Override(c_overrideKey, 1, new CoverSystemTriggers(true, true, false, false, false));

                player.SmbThrow.TriggS.Override(c_overrideKeyReload, 1, new ThrowSystemTriggers(false));

                if (IsAiming)
                    OnUnAim();

                if (player.ammoBag[secAmmoBagIndex].haveCount < cGunAtt.GetComponent<ModifiableGun>().SecFireGp.maxClipCapacity - cGunAtt.GetComponent<ModifiableGun>().SecFireGp.currentClipCapacity)
                {
                    cGunAtt.GetComponent<ModifiableGun>().SecFireGp.currentClipCapacity += player.ammoBag[secAmmoBagIndex].haveCount;
                    player.ammoBag[secAmmoBagIndex].haveCount = 0;
                }
                else
                {
                    player.ammoBag[secAmmoBagIndex].haveCount -= cGunAtt.GetComponent<ModifiableGun>().SecFireGp.maxClipCapacity - cGunAtt.GetComponent<ModifiableGun>().SecFireGp.currentClipCapacity;
                    cGunAtt.GetComponent<ModifiableGun>().SecFireGp.currentClipCapacity = cGunAtt.GetComponent<ModifiableGun>().SecFireGp.maxClipCapacity;
                }

                cGunAtt.GetComponent<ModifiableGun>().SecFireGp.currentClipCapacity = 1;

                Events.InvokeWeaponReloadDone(cGunAtt);

                return true;
            }

            return false;
        }

        private bool ReloadTrigger()
        {
            if (!TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_Reload))
                return false;

            if (userInput.ReloadDown)
            {
                if (HaveAmmoWithCaliber(cGunAtt.firableProjectileCaliber) && cGunAtt.currentClipCapacity < cGunAtt.maxClipCapacity)
                {
                    ProjectileBase projectilePrefabToUse = null;
                    // Does player has bullet prefab that this weapon used on last clip
                    if (cGunAtt.currentProjectilePrefab && PlayerHaveBulletPrefab(cGunAtt.currentProjectilePrefab))
                        projectilePrefabToUse = cGunAtt.currentProjectilePrefab;
                    // Else use first prefab that this weapon can use
                    else
                        projectilePrefabToUse = player.ammoBag.First(x => x.projectilePrefab.bulletCaliber == cGunAtt.firableProjectileCaliber && x.haveCount > 0).projectilePrefab;

                    ammoBagIndexFoundOnReloadDown = player.ammoBag.FindIndex(x => x.projectilePrefab == projectilePrefabToUse);
                    animator.SetTrigger(cap_Reload);

                    player.SmbCover.TriggS.Override(c_overrideKey, 1, new CoverSystemTriggers(true, true, false, false, false));

                    if (cGunAtt != null && cGunAtt.curClipObject && cGunAtt.curClipPrefab)
                    {
                        cGunAtt.curClipObject.SetParent(null);
                        if (cGunAtt.curClipObject.GetComponent<Rigidbody>())
                        {
                            cGunAtt.curClipObject.GetComponent<Rigidbody>().AddForce(transform.forward * 1f);
                            cGunAtt.curClipObject.GetComponent<Rigidbody>().isKinematic = false;
                        }
                        if (cGunAtt.curClipObject.GetComponent<Collider>())
                        {
                            cGunAtt.curClipObject.GetComponent<Collider>().enabled = true;
                            cGunAtt.curClipObject.GetComponent<Collider>().isTrigger = false;
                        }
                        if (cGunAtt.curClipObject.GetComponent<Destroy>())
                            cGunAtt.curClipObject.GetComponent<Destroy>().destroyTime = 30f;
                    }

                    player.SmbThrow.TriggS.Override(c_overrideKeyReload, 1, new ThrowSystemTriggers(false));

                    if (IsAiming)
                        OnUnAim();

                    return true;
                }

                if (!HaveAmmoWithCaliber(cGunAtt.firableProjectileCaliber))
                {
                    player.TryPrintInformationText("Can't reload info", 2f, "You have no ammo for this weapon.");
                }
            }

            return false;
        }

        private bool HaveAmmoWithCaliber(float style)
        {
            return player.ammoBag.Any(x => (x.projectilePrefab.bulletCaliber == style) &&
            x.haveCount > 0
            );
        }

        private bool PlayerHaveBulletPrefab(ProjectileBase prefab)
        {
            return player.ammoBag.Any(x => (x.projectilePrefab == prefab) && x.haveCount > 0);
        }

        private bool ModifyWeaponTrigger()
        {
            if (!TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_Modify))
                return false;
            if (userInput.ModifyWeaponDown && cGunAtt.GetComponent<ModifiableGun>() && cGunAtt.GetComponent<ModifiableGun>().partHolders != null && cGunAtt.GetComponent<ModifiableGun>().partHolders.Count > 0)
            {
                short priority = -1;
                player.SmbLoco.TriggS.Override(c_overrideKeyModifying, priority, new LocomotionSystemTriggers(false));
                player.SmbCover.TriggS.Override(c_overrideKeyModifying, priority, new CoverSystemTriggers(false));
                player.SmbThrow.TriggS.Override(c_overrideKeyModifying, priority, new ThrowSystemTriggers(false));
                player.SmbLookIK.OverrideToDeactivateLookAt(new DeactivatedLookAtParams(0), priority, c_overrideKeyModifying);
                playerCamera.OverrideCamera(new FocusCameraParams(CFireProps.focusCamModifierWeaponModify, transform, cGunAtt.transform), priority, c_overrideKeyModifying);
                animator.SetBool(cap_ModifyWeapon, true);
                return true;
            }
            return false;
        }

        private bool UnModifyWeaponTrigger()
        {
            if (!TriggS.LastValue.GetTrigger(WeaponSystemTriggers.ct_Modify) || userInput.ModifyWeaponDown)
            {
                player.SmbLoco.TriggS.Release(c_overrideKeyModifying);
                player.SmbCover.TriggS.Release(c_overrideKeyModifying);
                player.SmbThrow.TriggS.Release(c_overrideKeyModifying);
                player.SmbLookIK.ReleaseOverrideLookAt(c_overrideKeyModifying);
                playerCamera.ReleaseOverride(c_overrideKeyModifying);
                animator.SetBool(cap_ModifyWeapon, false);
                return true;
            }
            return false;
        }

        #endregion Triggers

        #region StateMachine Functions

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            stateShortNameHashOfOnStateEnter = stateInfo.shortNameHash;

            if (stateInfo.shortNameHash == h_Empty)
            {
                if (player.SmbLoco.IsOverridenWithKey(c_overrideKey_Locomotion))
                    player.SmbLoco.ReleaseOverrideLocomoteType(c_overrideKey_Locomotion);

                animator.SetInteger(cap_WeaponPull, 0);

                SetLayerWeightAndHandIKTargets(0, 0);
            }
            else if (stateInfo.shortNameHash == h_PullOutWeapon)
            {
                blendedOutPullOutAnimEventBug_onIsHandOnGun = false;
                SetWeapon(true);
                Events.InvokeWeaponPullOut(cGunAtt);

                float lHandTarget = GetHandTarget(true, cGunAtt);
                SetLayerWeightTargets(1, lHandTarget);
                SetHandIKTarget(true, 0);
                SetHandIKTarget(false, 0);

                cGunAtt.PlayRandomSoundAsWeaponChild(cGunAtt.sounds.pullOuts, transform);
            }
            else if (stateInfo.shortNameHash == h_IdleWithWeapon)
            {
                if (player.SmbCover.TriggS.IsOverridenWithKey(c_overrideKey))
                    player.SmbCover.TriggS.Release(c_overrideKey);

                float rightTarget = GetHandTarget(false, cGunAtt);
                float leftTarget = GetHandTarget(true, cGunAtt);

                SetLayerWeightTargets(rightTarget, leftTarget);
                SetHandIKTarget(false, 0);
                SetHandIKTarget(true, leftTarget);

                if (player.SmbThrow.TriggS.IsOverridenWithKey(c_overrideKeyReload))
                    player.SmbThrow.TriggS.Release(c_overrideKeyReload);
            }
            else if (stateInfo.shortNameHash == h_AimingWithWeapon)
            {
                OnStateEnterAimingFire();

                float lHandTget = GetHandTarget(true, cGunAtt);
                SetLayerWeightTargets(1, lHandTget);
                SetHandIKTarget(false, 1);
                SetHandIKTarget(true, lHandTget);
            }
            else if (stateInfo.shortNameHash == h_HolsterWeapon)
            {
                if (player.SmbLoco.IsOverridenWithKey(c_overrideKey_Locomotion))
                    player.SmbLoco.ReleaseOverrideLocomoteType(c_overrideKey_Locomotion);

                if (player.SmbCover.TriggS.IsOverridenWithKey(c_overrideKey))
                    player.SmbCover.TriggS.Release(c_overrideKey);

                if (IsChangingWeapon)
                    animator.SetInteger(cap_WeaponPull, 1);

                float lHandTget = GetHandTarget(true, cGunAtt);

                SetLayerWeightTargets(1, lHandTget);
                SetHandIKTarget(true, lHandTget);
                SetHandIKTarget(false, 0);

                cGunAtt.PlayRandomSoundAsWeaponChild(cGunAtt.sounds.holsters, transform);
            }
            else if (stateInfo.shortNameHash == h_ReloadWeapon)
            {
                float lHandTget = GetHandTarget(true, cGunAtt);
                SetLayerWeightTargets(1, lHandTget);

                SetHandIKTarget(true, 0);
                SetHandIKTarget(false, 0);

                blendedOutReloadAnimEventBug_onReloadDone = false;
                blendedOutReloadAnimEventBug_onClipLeftHand = false;

                cGunAtt.PlayRandomSoundAsWeaponChild(cGunAtt.sounds.reloads, transform);
            }
            else if (stateInfo.shortNameHash == h_GunPartReload)
            {
                SetLayerWeightTargets(1, 1);

                SetHandIKTarget(true, 0);
                SetHandIKTarget(false, 0);

                if (cGunAtt.GetComponent<ModifiableGun>().SecFireGp.reloadSound)
                {
                    GameObject goGPReload = Instantiate(cGunAtt.GetComponent<ModifiableGun>().SecFireGp.reloadSound, cGunAtt.transform.position, Quaternion.identity) as GameObject;
                    goGPReload.transform.SetParent(cGunAtt.transform, true);
                }
            }
            else if (stateInfo.shortNameHash == h_ModifyWeapon)
            {
                float rightTarget = GetHandTarget(false, cGunAtt);
                float leftTarget = GetHandTarget(true, cGunAtt);

                SetLayerWeightTargets(rightTarget, leftTarget);
                SetHandIKTarget(false, 0);
                SetHandIKTarget(true, 1);

                currentFocusedHolderIndex = 0;
                if (partPrefabsForThisIndex == null)
                    partPrefabsForThisIndex = new List<GunPart>();
                partPrefabsForThisIndex.Clear();

                partPrefabsForThisIndex = GetKnownPartPrefabsForWeaponIndex(cGunAtt.GetComponent<ModifiableGun>(), currentFocusedHolderIndex, player.learnedGunPartPrefabs);
                playerCamera.ChangePointerOfCamModderWithID(new FocusCameraParams(CFireProps.focusCamModifierWeaponModify, transform, cGunAtt.GetComponent<ModifiableGun>().partHolders[currentFocusedHolderIndex].holderTransform), c_overrideKeyModifying);
                activeChildPart = GetActiveChildGunPart(cGunAtt.GetComponent<ModifiableGun>(), currentFocusedHolderIndex);
                // Parts should be known by player by now, learn them if player does not know, just in case
                LearnGunParts(player, cGunAtt.GetComponent<ModifiableGun>());
                activeIndex = GetActiveIndexInKnownParts(partPrefabsForThisIndex, activeChildPart.GetComponent<GunPart>().partPrefab);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.shortNameHash == h_Empty)
            {
                if (animator.GetNextAnimatorStateInfo(layerIndex).shortNameHash != h_PullOutWeapon &&
                    WeaponPullOutTrigger())
                    return;
                if (!animator.IsInTransition(layerIndex) &&
                    (WeaponSwitchTrigger(false) || WeaponDropTrigger(false, false) || WeaponCollectTrigger())
                    )
                    return;
            }
            else if (stateInfo.shortNameHash == h_IdleWithWeapon)
            {
                if (!animator.IsInTransition(layerIndex))
                    if (WeaponHolsterTrigger() || WeaponSwitchTrigger() || ReloadTrigger() || WeaponDropTrigger() || ModifyWeaponTrigger() || GunPartReloadTrigger())
                        return;
                FlashLightTrigger();
                WeaponCollectTrigger();
                if ((!animator.IsInTransition(layerIndex) && AimTriggerOnIdle()) ||
                (animator.GetAnimatorTransitionInfo(layerIndex).nameHash == hT_IdleToAiming && AimTriggerOnIdle())
                )
                    return;
                HandleWeaponHandsUpdate();
            }
            else if (stateInfo.shortNameHash == h_AimingWithWeapon)
            {
                if (tmp_UnAimTimerAfterFire1Down > 0)
                    tmp_UnAimTimerAfterFire1Down -= Time.deltaTime;

                if (!animator.IsInTransition(layerIndex))
                    if (UnAimTriggerOnAiming() || ReloadTrigger())
                        return;
                if (!animator.IsInTransition(layerIndex))
                    OnStateUpdateAimingFire();
                FlashLightTrigger();

                HandleWeaponHandsUpdate();
            }
            else if (stateInfo.shortNameHash == h_HolsterWeapon)
            {
                if (stateInfo.normalizedTime > c_disableLayersOnHolsterAtNormalizedTime &&
                    animator.GetNextAnimatorStateInfo(layerIndex).shortNameHash != h_PullOutWeapon &&
                    !animator.IsInTransition(layerIndex))
                {
                    SetLayerWeightAndHandIKTargets(0, 0);
                }
            }
            else if (stateInfo.shortNameHash == h_ModifyWeapon)
            {
                if (UnModifyWeaponTrigger())
                    return;

                if ((partPrefabsForThisIndex.Count > 0 && cGunAtt.GetComponent<ModifiableGun>().partHolders[currentFocusedHolderIndex].allowEmpty) || (!cGunAtt.GetComponent<ModifiableGun>().partHolders[currentFocusedHolderIndex].allowEmpty && partPrefabsForThisIndex.Count > 1))
                {
                    if (userInput.MenuUpDown)
                    {
                        activeIndex++; activeIndex = activeIndex >= partPrefabsForThisIndex.Count ? (cGunAtt.GetComponent<ModifiableGun>().partHolders[currentFocusedHolderIndex].allowEmpty ? -1 : 0) : activeIndex;
                        if (activeChildPart) Destroy(activeChildPart.gameObject);
                        if (activeIndex != -1)
                        {
                            activeChildPart = cGunAtt.GetComponent<ModifiableGun>().CreateClonePart(partPrefabsForThisIndex[activeIndex].gameObject).transform;
                        }
                        cGunAtt.GetComponent<ModifiableGun>().StartCoroutine(cGunAtt.GetComponent<ModifiableGun>().ManageNextFrame());
                    }
                    else if (userInput.MenuDownDown)
                    {
                        activeIndex--; activeIndex = activeIndex < (cGunAtt.GetComponent<ModifiableGun>().partHolders[currentFocusedHolderIndex].allowEmpty ? -1 : 0) ? partPrefabsForThisIndex.Count - 1 : activeIndex;
                        if (activeChildPart) Destroy(activeChildPart.gameObject);
                        if (activeIndex != -1)
                        {
                            activeChildPart = cGunAtt.GetComponent<ModifiableGun>().CreateClonePart(partPrefabsForThisIndex[activeIndex].gameObject).transform;
                        }
                        cGunAtt.GetComponent<ModifiableGun>().StartCoroutine(cGunAtt.GetComponent<ModifiableGun>().ManageNextFrame());
                    }
                }
                if (userInput.MenuRightDown)
                {
                    currentFocusedHolderIndex = ++currentFocusedHolderIndex % cGunAtt.GetComponent<ModifiableGun>().partHolders.Count;
                    partPrefabsForThisIndex = GetKnownPartPrefabsForWeaponIndex(cGunAtt.GetComponent<ModifiableGun>(), currentFocusedHolderIndex, player.learnedGunPartPrefabs);
                    playerCamera.ChangePointerOfCamModderWithID(new FocusCameraParams(CFireProps.focusCamModifierWeaponModify, transform, cGunAtt.GetComponent<ModifiableGun>().partHolders[currentFocusedHolderIndex].holderTransform), c_overrideKeyModifying);
                    activeChildPart = GetActiveChildGunPart(cGunAtt.GetComponent<ModifiableGun>(), currentFocusedHolderIndex);
                }
                else if (userInput.MenuLeftDown)
                {
                    --currentFocusedHolderIndex; currentFocusedHolderIndex = currentFocusedHolderIndex < 0 ? cGunAtt.GetComponent<ModifiableGun>().partHolders.Count - 1 : currentFocusedHolderIndex;
                    partPrefabsForThisIndex = GetKnownPartPrefabsForWeaponIndex(cGunAtt.GetComponent<ModifiableGun>(), currentFocusedHolderIndex, player.learnedGunPartPrefabs);
                    playerCamera.ChangePointerOfCamModderWithID(new FocusCameraParams(CFireProps.focusCamModifierWeaponModify, transform, cGunAtt.GetComponent<ModifiableGun>().partHolders[currentFocusedHolderIndex].holderTransform), c_overrideKeyModifying);
                    activeChildPart = GetActiveChildGunPart(cGunAtt.GetComponent<ModifiableGun>(), currentFocusedHolderIndex);
                }
            }

            UpdateLayerWeights();
            AmmoCollectTrigger();
            SupplyBoxAmmoCollectTrigger();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.shortNameHash == h_Empty)
            {
            }
            else if (stateInfo.shortNameHash == h_PullOutWeapon)
            {
            }
            else if (stateInfo.shortNameHash == h_IdleWithWeapon)
            {
            }
            else if (stateInfo.shortNameHash == h_AimingWithWeapon)
            {
            }
            else if (stateInfo.shortNameHash == h_HolsterWeapon)
            {
            }
            else if (stateInfo.shortNameHash == h_ReloadWeapon)
            {
            }
        }

        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.shortNameHash == h_Empty)
            {
            }
            else if (stateInfo.shortNameHash == h_PullOutWeapon)
            {
            }
            else if (stateInfo.shortNameHash == h_IdleWithWeapon)
            {
                HandleWeaponHandsOnAnimatorIK();
            }
            else if (stateInfo.shortNameHash == h_AimingWithWeapon)
            {
                HandleWeaponHandsOnAnimatorIK();
            }
            else if (stateInfo.shortNameHash == h_HolsterWeapon)
            {
            }
            else if (stateInfo.shortNameHash == h_ModifyWeapon)
            {
                SimpleLeftHandFixForLocomStyleOnModifyWeapon();
            }
        }

        #endregion StateMachine Functions

        private void SimpleLeftHandFixForLocomStyleOnModifyWeapon()
        {
            animator.SetIKPosition(AvatarIKGoal.LeftHand, transform.TransformPoint(-.4f, .8f, 0));
            animator.SetIKRotation(AvatarIKGoal.LeftHand, Quaternion.identity);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, CFireProps.lHandAim);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, CFireProps.lHandAim);
        }

        private void OnStateEnterAimingFire()
        {
            waitOnAimTrigger = IsAimingHipFire ? cGunAtt.waitToStartFireOnHipFireAim : cGunAtt.waitToStartFireOnSightAim;
        }

        private void OnStateUpdateAimingFire()
        {
            if (waitOnAimTrigger < 0)
            {
                waitOnAimTrigger -= Time.deltaTime;
                return;
            }

            if ((((userInput.Fire1Down) && tapTimerCounter < 0) || ((userInput.FirePress || player.PressFire1Button) && nextFireTimer < 0))
                )
            {
                nextFireTimer = 1;
                tapTimerCounter = cGunAtt.tapFireMinTimer;

                Quaternion xQ = Quaternion.FromToRotation(
                            (-userInput.cameraRig.position + fireReference.position).normalized,
                            (-weaponIKParent.position + RefTarget.position).normalized
                            );
                animator.SetTrigger("Fire");

                if (cGunAtt.currentClipCapacity > 0)
                {
                    float bBob = 0;
                    if (cGunAtt.GetComponent<ModifiableGun>())
                    {
                        cGunAtt.GetComponent<ModifiableGun>().Fire(
                            player.transform, RefTarget,
                            fireReference.position,
                            ref bBob, ref randomTwistSign, xQ, CFireProps.rayCBulletLayerMask, projectilePrefabDamage
                            );
                    }
                    else
                    {
                        cGunAtt.Fire(
                            player.transform, RefTarget,
                            fireReference.position,
                            ref bBob, ref randomTwistSign, xQ, CFireProps.rayCBulletLayerMask, projectilePrefabDamage
                            );
                    }

                    Events.InvokeWeaponFire(cGunAtt);
                    weaponBodyBob += bBob * CFireProps.bodyBobAgentMultiplier;
                }
                else
                    // Dry shot
                    cGunAtt.PlayRandomSoundAsWeaponChild(cGunAtt.sounds.dryShots, transform);
            }
            else if (userInput.SecondaryFireDown && nextSecFireTimer < 0 && cGunAtt.GetComponent<ModifiableGun>() && cGunAtt.GetComponent<ModifiableGun>().SecFireGp)
            {
                nextSecFireTimer = 1;
                Quaternion xQ = Quaternion.FromToRotation(
                         (-userInput.cameraRig.position + fireReference.position).normalized,
                         (-weaponIKParent.position + RefTarget.position).normalized
                         );
                if (cGunAtt.GetComponent<ModifiableGun>().SecFireGp.currentClipCapacity > 0)
                {
                    float bBob = 0;
                    cGunAtt.GetComponent<ModifiableGun>().SecFire(
                        player.transform, RefTarget,
                            fireReference.position,
                            ref bBob, ref randomTwistSign, xQ, CFireProps.rayCBulletLayerMask
                        );
                    Events.InvokeWeaponFire(cGunAtt);
                    weaponBodyBob += bBob * CFireProps.bodyBobAgentMultiplier;
                }
            }

            if (cGunAtt.GetComponent<ModifiableGun>())
                nextSecFireTimer -= Time.deltaTime * cGunAtt.GetComponent<ModifiableGun>().SecFireSpeed;
            nextFireTimer -= Time.deltaTime * cGunAtt.fireSpeed;
            tapTimerCounter -= Time.deltaTime;
        }

        private void HandleWeaponHandsUpdate()
        {
            Vector3 targetLocalPos = RefTarget.localPosition;
            float spreadZ = (Mathf.Abs(RefTarget.localPosition.x) + Mathf.Abs(RefTarget.localPosition.y)) * randomTwistSign * CFireProps.visualHandSpreadAgentMultipliers.z;
            weaponSpread = Quaternion.Euler(new Vector3(-targetLocalPos.x * cGunAtt.spreadAxisMultipliers.x * CFireProps.visualHandSpreadAgentMultipliers.x, -targetLocalPos.y * cGunAtt.spreadAxisMultipliers.y * CFireProps.visualHandSpreadAgentMultipliers.y, spreadZ * cGunAtt.spreadAxisMultipliers.z));

            CFireProps.rHandAim = Mathf.Lerp(CheckEpsilon(CFireProps.rHandAim, RightHandIKTarget), RightHandIKTarget,
                (RightHandIKTarget == 1 ? CFireProps.rightHandSmooth : CFireProps.rightHandBackSmooth) * Time.deltaTime);
            CFireProps.rHandAimRot = Mathf.Lerp(CheckEpsilon(CFireProps.rHandAimRot, RightHandIKTarget), RightHandIKTarget,
                (RightHandIKTarget == 1 ? CFireProps.rightHandSmooth : CFireProps.rightHandBackSmooth) * Time.deltaTime);
        }

        private void HandleWeaponHandsOnAnimatorIK()
        {
            if (!cGunAtt)
                return;
            if (WeaponIK)
            {
                animator.SetIKPosition(AvatarIKGoal.RightHand, WeaponIK.position + WeaponIK.right * CFireProps.aimPositionFixer.x + WeaponIK.up * CFireProps.aimPositionFixer.y + WeaponIK.forward * CFireProps.aimPositionFixer.z);
                animator.SetIKRotation(AvatarIKGoal.RightHand, WeaponIK.rotation * Quaternion.Euler(CFireProps.aimRotationFixer) * weaponSpread);
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, CFireProps.rHandAim);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, CFireProps.rHandAimRot);
            }

            Transform leftHandleOfGun = cGunAtt.GetFixer(player.transform.name, WeaponFixerTypes.LeftHandle);
            if (leftHandleOfGun)
            {
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandleOfGun.transform.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandleOfGun.transform.rotation);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, CFireProps.lHandAim);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, CFireProps.lHandAim);
            }
        }

        public static void LearnGunParts(PlayerAtts player, ModifiableGun modifiableGun)
        {
            if (modifiableGun == null)
                return;
            foreach (GunPart gPart in modifiableGun.transform.GetComponentsInChildren<GunPart>())
                if (gPart.partPrefab)
                    if (!player.learnedGunPartPrefabs.Contains(gPart.partPrefab.GetComponent<GunPart>()))
                        player.learnedGunPartPrefabs.Add(gPart.partPrefab.GetComponent<GunPart>());
        }

        private static List<GunPart> GetKnownPartPrefabsForWeaponIndex(ModifiableGun modifiableGun, int holderIndex, List<GunPart> learnedGunParts)
        {
            if (modifiableGun.partHolders == null || holderIndex >= modifiableGun.partHolders.Count || modifiableGun.partHolders.Count == 0)
                return new List<GunPart>();

            List<GunPart> retValPrefabs = new List<GunPart>();

            foreach (var learnedPart in learnedGunParts)
                foreach (var compPart in modifiableGun.partHolders[holderIndex].compatibleParts)
                {
                    if (learnedPart.gameObject == compPart.prefab)
                        retValPrefabs.Add(learnedPart);
                }
            return retValPrefabs;
        }

        private static Transform GetActiveChildGunPart(ModifiableGun modifiableGun, int focusedIndex)
        {
            foreach (var pHolder in modifiableGun.partHolders[focusedIndex].compatibleParts)
            {
                foreach (var childGP in modifiableGun.GetComponentsInChildren<GunPart>())
                {
                    if (childGP.partPrefab == pHolder.prefab)
                        return childGP.transform;
                }
            }
            return null;
        }

        private static int GetActiveIndexInKnownParts(List<GunPart> knownPartPrefabs, GameObject partPrefab)
        {
            if (!partPrefab)
                return -1;
            for (int i = 0; i < knownPartPrefabs.Count; i++)
            {
                if (knownPartPrefabs[i].gameObject == partPrefab)
                    return i;
            }
            return -1;
        }

        public static float CheckEpsilon(float xFloat, float target)
        {
            if (target > .5f)
                xFloat = (target - xFloat < .01f) ? 1 : xFloat;
            else
                xFloat = (xFloat < .01f) ? 0 : xFloat;
            return xFloat;
        }

        public static bool AtMinEpsilon(float t)
        {
            if (t < .002f)
                return true;
            return false;
        }
    }
}