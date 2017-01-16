using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerAtts : MonoBehaviour
    {
        public LayerMask groundLayer;

        [Space]
        public bool startWalking;

        public bool useStaticMovement = false;

        [Space]
        public List<LocomotionStyle> locomotionStyles;

        public List<WeaponFireStyle> weaponFireStyles;
        public List<LookAtIKStyle> lookAtIKStyles;
        public List<ThrowStyle> throwingStyles;
        public List<CoverStyle> coverStyles;
        public JumpProps jumpProps;

        [Space]
        public int defaultLocomStyleIndex = 0;

        public int defaultWeaponFireStyleIndex = 0;
        public int defaultLookAtIKStyleIndex = 0;
        public int defaultThrowStyleIndex = 0;
        public int defaultCoverStyleIndex = 0;

        [Space]
        [SerializeField]
        private FootStepSoundsAndFx footStepFx;

        [Space]
        public List<GameObject> weapons;

        public int defaultWeaponIndex = 0;
        public int maxWeaponCarryCount = 2;
        public List<AmmoBag> ammoBag;

        [Space]
        public List<ThrowableBag> throwableBag;

        public int defaultThrowableIndex = 0;

        [Space]
        public List<GunPart> learnedGunPartPrefabs;

        [Space]
        public FocusCameraSerializedFields focusCamModifierOnDead;

        [Space]
        public bool PressFire2Button;

        public bool PressFire1Button;

        #region Properties

        public LocomotionCSMB SmbLoco { get; private set; }
        public WeaponCSMB SmbWeapon { get; private set; }
        public LookIKCSMB SmbLookIK { get; private set; }
        public ThrowCSMB SmbThrow { get; private set; }
        public CoverCSMB SmbCover { get; private set; }
        public GroundedCheckSMB SmbGroundedCheck { get; private set; }
        public bool IsDead { get; private set; }
        public Animator Animator { get; private set; }
        public PlayerPhysics playerPhysic { get; private set; }

        #endregion Properties

        private Health health;
        private FootPlanting footSoundFx;

        public delegate void DeadEventHandler();

        public event DeadEventHandler onPlayerDead;

        private void Awake()
        {
            Animator = GetComponent<Animator>();

            SmbLoco = Animator.GetBehaviour<LocomotionCSMB>();

            SmbWeapon = Animator.GetBehaviour<WeaponCSMB>();

            SmbLookIK = Animator.GetBehaviour<LookIKCSMB>();

            SmbCover = Animator.GetBehaviour<CoverCSMB>();
            SmbThrow = Animator.GetBehaviour<ThrowCSMB>();
            SmbGroundedCheck = Animator.GetBehaviour<GroundedCheckSMB>();

            footSoundFx = new FootPlanting(footStepFx, GetComponent<Animator>());
            health = GetComponent<Health>();

            playerPhysic = new PlayerPhysics(GetComponent<Rigidbody>(), this);
        }

        private void Update()
        {
            if (health.health <= 0 && !IsDead)
            {
                IsDead = true;
                InvokePlayerDead();
            }
        }

        private void FixedUpdate()
        {
            playerPhysic.FixedUpdate();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!IsDead)
                footSoundFx.FootPlantOnAnimatorIK(layerIndex);
        }

        private void InvokePlayerDead()
        {
            Health.SwitchRagdoll(true, transform.GetComponentsInChildren<Rigidbody>(), transform.GetComponentsInChildren<Collider>());
            Animator.enabled = false;
            GetComponent<NavMeshAgent>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Collider>().isTrigger = true;
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            GetComponent<SetupAndUserInput>().cameraRig.GetComponent<PlayerCamera>().OverrideCamera(
                new FocusCameraParams(focusCamModifierOnDead, transform, transform), -short.MaxValue, "PlayerDead"
                );
            if (onPlayerDead != null)
                onPlayerDead();
        }

        public void TryPrintInformationText(string id, float timer, string text)
        {
            Canvas3D cnvs3d = Object.FindObjectOfType(typeof(Canvas3D)) as Canvas3D;
            if (cnvs3d)
            {
                cnvs3d.PrintInformationText(id, timer, text);
            }
        }

        public void TryPrintWorldInformationText(string id, float timer, string text, Vector3 pos, Quaternion rot)
        {
            Canvas3D cnvs3d = Object.FindObjectOfType(typeof(Canvas3D)) as Canvas3D;
            if (cnvs3d)
            {
                cnvs3d.PrintWorldInformationText(id, timer, text, pos, rot);
            }
        }

        #region Animation Embedded

        public delegate void AnimTrgFuncHandler();

        #region Weapon

        public event AnimTrgFuncHandler onNewClipInLeftHand;

        public event AnimTrgFuncHandler onReloadDone;

        public event AnimTrgFuncHandler onNewClipOffLeftHand;

        public event AnimTrgFuncHandler onIsHandOnGun;

        public event AnimTrgFuncHandler onIsHandAwayFromGun;

        public event AnimTrgFuncHandler onGunPartReloadDone;

        public void NewClipInLeftHand()
        {
            if (onNewClipInLeftHand != null)
                onNewClipInLeftHand();
        }

        public void NewClipOffLeftHand()
        {
            if (onNewClipOffLeftHand != null)
                onNewClipOffLeftHand();
        }

        public void ReloadDone()
        {
            if (onReloadDone != null)
                onReloadDone();
        }

        public void IsHandOnGun()
        {
            if (onIsHandOnGun != null)
                onIsHandOnGun();
        }

        public void IsHandAwayFromGun()
        {
            if (onIsHandAwayFromGun != null)
                onIsHandAwayFromGun();
        }

        public void GunPartReloadDone()
        {
            if (onGunPartReloadDone != null)
                onGunPartReloadDone();
        }

        #endregion Weapon

        #region Throwables

        public AnimTrgFuncHandler onIsHandOnBomb;
        public AnimTrgFuncHandler onIsHandOffBomb;

        public void IsHandOnBomb()
        {
            if (onIsHandOnBomb != null)
                onIsHandOnBomb();
        }

        public void IsHandOffBomb()
        {
            if (onIsHandOffBomb != null)
                onIsHandOffBomb();
        }

        #endregion Throwables

        #endregion Animation Embedded
    }
}