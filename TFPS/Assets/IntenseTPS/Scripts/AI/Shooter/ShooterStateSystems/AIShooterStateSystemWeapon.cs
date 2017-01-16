using StateSystems;
using System.Collections.Generic;
using UnityEngine;

namespace Shooter.StateSystems
{
    /// <summary>
    /// Used to handle <see cref="ShooterBehaviour"/>'s weapon functions
    /// Works in combination with <see cref="AIStateSystemAnimator"/>
    /// </summary>
    public class AIShooterStateSystemWeapon : AIStateSystemWeapon
    {
        private FireProps fireProps;
        public WeaponHandIKProps WHandIKProps { get; set; }

        private float nextFireTimer;

        private int randShotCountI;
        private float randWaitTimerF;
        private bool isFiring = false;

        public Transform LeftHandHold { get; private set; }
        public Transform RightHandHold { get; private set; }
        private Transform target;
        public Transform WeaponIK { get; private set; }
        private Transform weaponIKParent;
        private float randomTwistSign;

        public float weaponBodyBob;
        private float rightHandTarget, leftHandTarget;
        private Transform tempNewClip;
        private Vector3 fireTo = Vector3.zero;

        public AIShooterStateSystemWeapon(FireProps _fireProps, WeaponHandIKProps _whIKProps)
        {
            WHandIKProps = _whIKProps;
            fireProps = _fireProps;
        }

        public override void OnStart(AIBrain ai)
        {
            foreach (Transform t in ai.Transform.GetComponentsInChildren<Transform>())
                if (t.CompareTag("Target"))
                {
                    target = t;
                    t.GetComponent<AiTargetLogic>().ai = ai;
                    t.GetComponent<AiTargetLogic>().ssw = this;

                    weaponIKParent = target.parent;
                }
            foreach (Transform t in ai.Transform.GetComponentsInChildren<Transform>())
                if (t.CompareTag("RightHandHold"))
                    RightHandHold = t.FindChild("PosRotFixer");
            foreach (Transform t in ai.Transform.GetComponentsInChildren<Transform>())
                if (t.CompareTag("LeftHandHold"))
                    LeftHandHold = t.FindChild("PosRotFixer");
            foreach (Transform t in ai.Transform.GetComponentsInChildren<Transform>())
                if (t.CompareTag("WeaponIK"))
                    WeaponIK = t.FindChild("PosRotFixer");
            if (fireProps.weapons != null && fireProps.weapons.Count > 0)
            {
                ai.CurrentWeapon = fireProps.weapons[fireProps.startWeaponIndex > fireProps.weapons.Count - 1 ? 0 : fireProps.startWeaponIndex];
            }

            ShooterBehaviour sb = ai.Transform.GetComponent<ShooterBehaviour>();
            if (sb)
            {
                sb.onIsHandOnGun += OnIsHandOnGun;
                sb.onIsHandAwayFromGun += OnIsHandAwayFromGun;
                sb.onNewClipInLeftHand += OnNewClipInLeftHand;
                sb.onNewClipOffLeftHand += OnNewClipOffLeftHand;
                sb.onReloadDone += OnReloadDone;
            }
        }

        virtual public void OnIsHandOnGun(AIBrain ai)
        {
            GunAtt gunAtt = ai.GetCurrentWeaponScript();
            if (RightHandHold && ai.HaveCurrentWeapon() && gunAtt &&
                 gunAtt.GetFixer(ai.Transform.name, WeaponFixerTypes.AnimRightHand))
            {
                RightHandHold.localPosition = gunAtt.GetFixer(ai.Transform.name, WeaponFixerTypes.AnimRightHand).localPosition;
                RightHandHold.localRotation = gunAtt.GetFixer(ai.Transform.name, WeaponFixerTypes.AnimRightHand).localRotation;

                WeaponIK.localPosition = gunAtt.GetFixer(ai.Transform.name, WeaponFixerTypes.AimHipFire).localPosition;
                WeaponIK.localRotation = gunAtt.GetFixer(ai.Transform.name, WeaponFixerTypes.AimHipFire).localRotation;
            }

            ai.CurrentWeapon.SetParent(RightHandHold, false);
            ai.CurrentWeapon.localPosition = Vector3.zero;
            ai.CurrentWeapon.localRotation = Quaternion.identity;
        }

        virtual public void OnIsHandAwayFromGun(AIBrain ai)
        {
            ai.CurrentWeapon.SetParent(null);
            ai.CurrentWeapon.position = new Vector3(0, -500, 0);
            ai.GetStateSystem<AIStateSystemAnimator>().DisableLayer(ai, 1, true, false);
        }

        virtual public void OnNewClipInLeftHand(AIBrain ai)
        {
            GunAtt gunAtt = ai.GetCurrentWeaponScript();
            // instantiate new clip in left hand
            if (gunAtt && gunAtt.curClipObject && gunAtt.curClipPrefab)
            {
                // instantiate new clip
                tempNewClip = gunAtt.InstantiateReturn(gunAtt.curClipPrefab);
                tempNewClip.SetParent(LeftHandHold, true);

                tempNewClip.localPosition = Vector3.zero;
                tempNewClip.localRotation = Quaternion.identity;

                LeftHandHold.localPosition = gunAtt.GetFixer(ai.Transform.name, WeaponFixerTypes.LeftHandClip).localPosition;
                LeftHandHold.localRotation = gunAtt.GetFixer(ai.Transform.name, WeaponFixerTypes.LeftHandClip).localRotation;

                tempNewClip.GetComponent<Rigidbody>().isKinematic = true;
                tempNewClip.GetComponent<Collider>().enabled = false;

                gunAtt.StartCoroutine(gunAtt.FixClipPosInLHand(tempNewClip));

                return;
            }
        }

        virtual public void OnNewClipOffLeftHand(AIBrain ai)
        {
            // new clip goes to weapon
            if (tempNewClip)
            {
                GunAtt gunAtt = ai.GetCurrentWeaponScript();

                tempNewClip.SetParent(gunAtt.transform);
                tempNewClip.localPosition = gunAtt.clipDefLocalPos;
                tempNewClip.localRotation = gunAtt.clipDefLocalRot;

                gunAtt.curClipObject = tempNewClip;
            }
            tempNewClip = null;
        }

        virtual public void OnReloadDone(AIBrain ai)
        {
            ai.GetCurrentWeaponScript().currentClipCapacity = ai.GetCurrentWeaponScript().maxClipCapacity;
        }

        public override void ArmWeapon(AIBrain ai)
        {
            ai.stateSystemAnimator.AnimateInteger(
                ai, "Draw", ai.GetCurrentWeaponScript().gunStyle, true, false, "", "LowIdle", 1);
        }

        public override bool IsWeaponArmingFinished(AIBrain ai)
        {
            if (ai.stateSystemAnimator.IsStartedAnimationFinished("", "LowIdle"))
            {
                // Just in case
                GunAtt gunAtt = ai.GetCurrentWeaponScript();
                if (RightHandHold && ai.HaveCurrentWeapon() && gunAtt &&
                    gunAtt.GetFixer(ai.Transform.name, WeaponFixerTypes.AnimRightHand))
                {
                    RightHandHold.localPosition = gunAtt.GetFixer(ai.Transform.name, WeaponFixerTypes.AnimRightHand).localPosition;
                    RightHandHold.localRotation = gunAtt.GetFixer(ai.Transform.name, WeaponFixerTypes.AnimRightHand).localRotation;

                    WeaponIK.localPosition = gunAtt.GetFixer(ai.Transform.name, WeaponFixerTypes.AimHipFire).localPosition;
                    WeaponIK.localRotation = gunAtt.GetFixer(ai.Transform.name, WeaponFixerTypes.AimHipFire).localRotation;
                }

                ai.CurrentWeapon.SetParent(RightHandHold, false);
                ai.CurrentWeapon.localPosition = Vector3.zero;
                ai.CurrentWeapon.localRotation = Quaternion.identity;

                return true;
            }

            return false; ;
        }

        public override void DisArmWeapon(AIBrain ai)
        {
            ReleaseLeftHandFromWeapon(ai);
            ai.stateSystemAnimator.AnimateInteger(
                ai, "Draw", 0, true, false, "Empty", "", 1);
        }

        public override bool IsWeaponDisArmingFinished(AIBrain ai)
        {
            if (ai.stateSystemAnimator.IsStartedAnimationFinished("Empty", ""))
                return true;
            return false;
        }

        public override void AimWeapon(AIBrain ai, bool _hipFire = false)
        {
            ai.stateSystemAnimator.AnimateBool(
                ai, "Aim", true, true, false, "", "ReadyIdle", 1
                );

            ai.stateSystemAnimator.EnableLayer(ai, 1, true, false);

            rightHandTarget = 1;
            leftHandTarget = 1;
            if (!_hipFire)
            {
                WeaponIK.localPosition = ai.GetCurrentWeaponScript().GetFixer(ai.Transform.name, WeaponFixerTypes.AimSight).localPosition;
                WeaponIK.localRotation = ai.GetCurrentWeaponScript().GetFixer(ai.Transform.name, WeaponFixerTypes.AimSight).localRotation;
            }
            else
            {
                WeaponIK.localPosition = ai.GetCurrentWeaponScript().GetFixer(ai.Transform.name, WeaponFixerTypes.AimHipFire).localPosition;
                WeaponIK.localRotation = ai.GetCurrentWeaponScript().GetFixer(ai.Transform.name, WeaponFixerTypes.AimHipFire).localRotation;
            }
        }

        public override bool WeaponAimingFinished(AIBrain ai)
        {
            return ai.stateSystemAnimator.IsStartedAnimationFinished("", "ReadyIdle") && ai.GetStateSystem<AIStateSystemLookAt>().GetHeadIKWeight() > .8f;
        }

        public override void UnAimWeapon(AIBrain ai)
        {
            rightHandTarget = 0;
            ai.stateSystemAnimator.AnimateBool(
                ai, "Aim", false, true, false, "", "LowIdle", 1
                );
        }

        public override bool WeaponUnAimingFinished(AIBrain ai)
        {
            return ai.stateSystemAnimator.IsStartedAnimationFinished("", "LowIdle");
        }

        public override void HitMelee(AIBrain ai)
        {
            ai.stateSystemAnimator.AnimateFloat(
                ai, "MeleeHit", .5f, false, false, "", "Locomotion", 0);
        }

        public override bool IsMeleeHitEnded(AIBrain ai)
        {
            return ai.stateSystemAnimator.IsStartedAnimationFinished("", "Locomotion");
        }

        public override void ReleaseLeftHandFromWeapon(AIBrain ai)
        {
            leftHandTarget = 0;
        }

        public override void HoldWeaponWithLeftHand(AIBrain ai)
        {
            leftHandTarget = 1;
        }

        public override bool WeaponReloadFinished(AIBrain ai)
        {
            bool retval = ai.stateSystemAnimator.IsStartedAnimationFinished("", "LowIdle");

            if (retval)
                HoldWeaponWithLeftHand(ai);
            return retval;
        }

        public override void ReloadWeapon(AIBrain ai)
        {
            ai.stateSystemAnimator.AnimateTrigger(
                ai, "Reload",
                false, false, "", "LowIdle", 1);
            ReleaseLeftHandFromWeapon(ai);

            // Drop current clip object from weapon if there is
            GunAtt gunAtt = ai.GetCurrentWeaponScript();
            if (gunAtt != null && gunAtt.curClipObject && gunAtt.curClipPrefab)
            {
                gunAtt.curClipObject.SetParent(null);
                if (gunAtt.curClipObject.GetComponent<Rigidbody>())
                {
                    gunAtt.curClipObject.GetComponent<Rigidbody>().AddForce(ai.Transform.forward * 1f);
                    gunAtt.curClipObject.GetComponent<Rigidbody>().isKinematic = false;
                }
                if (gunAtt.curClipObject.GetComponent<Collider>())
                {
                    gunAtt.curClipObject.GetComponent<Collider>().enabled = true;
                    gunAtt.curClipObject.GetComponent<Collider>().isTrigger = false;
                }
                if (gunAtt.curClipObject.GetComponent<Destroy>())
                    gunAtt.curClipObject.GetComponent<Destroy>().destroyTime = 30f;
            }
        }

        public override void StartFiring(AIBrain ai)
        {
            GunAtt gunAtt = ai.GetCurrentWeaponScript();
#if UNITY_EDITOR
            if (!gunAtt)
            {
                Debug.Log("No GunAtt script is attached to weapon");
                return;
            }
#endif
            randShotCountI = Mathf.Max(1,
                (int)(.1f * gunAtt.maxClipCapacity *
                ((int)Random.Range((fireProps.randShotCount.x *
                fireProps.randShotCountAgentMultiplier.x), (fireProps.randShotCount.y * fireProps.randShotCountAgentMultiplier.y)))));

            randWaitTimerF = -1f;
            isFiring = true;
        }

        private void FireWithTimer(AIBrain ai)
        {
            if (isFiring)
            {
                GunAtt gunAtt = ai.GetCurrentWeaponScript();
#if UNITY_EDITOR
                if (!gunAtt)
                {
                    Debug.Log("No GunAtt script is attached to weapon");
                    return;
                }
#endif
                if (ai.InfoCurrentTarget != null && ai.InfoCurrentTarget.GetFireToPosition() != Vector3.zero)
                    fireTo = ai.InfoCurrentTarget.GetFireToPosition();
                if (randWaitTimerF < 0 && randShotCountI > 0 && nextFireTimer < 0)
                {
                    float bBob = 0;

                    Vector3 fw = weaponIKParent.forward;
                    Vector3 to = (-weaponIKParent.position + target.position).normalized;
                    Quaternion xQ = Quaternion.FromToRotation(fw, to);

                    gunAtt.Fire(ai.Transform, target, fireTo, ref bBob, ref randomTwistSign, xQ, fireProps.rayCBulletLayerMask);
                    //bBob = Mathf.Sign(target.localPosition.x) * bBob;
                    weaponBodyBob += bBob;
                    nextFireTimer = 1;
                    randShotCountI--;
                }
                if (randShotCountI <= 0)
                {
                    randShotCountI = Mathf.Max(1, (int)(.1f * gunAtt.maxClipCapacity *
                        (Random.Range((fireProps.randShotCount.x * fireProps.randShotCountAgentMultiplier.x),
                        (fireProps.randShotCount.y * fireProps.randShotCountAgentMultiplier.y)))));
                    randWaitTimerF = Random.Range(fireProps.randWaitTimer.x, fireProps.randWaitTimer.y);
                }
                randWaitTimerF -= Time.deltaTime;
                nextFireTimer -= Time.deltaTime * gunAtt.fireSpeed;
            }
        }

        public override void StopFiring(AIBrain ai)
        {
            isFiring = false;
        }

        public override bool HaveAmmoOnClip(AIBrain ai)
        {
            GunAtt gunAtt = ai.GetCurrentWeaponScript();
            if (!gunAtt)
            {
#if UNITY_EDITOR
                Debug.Log("No GunAtt script is attached to weapon");
                return false;
#endif
            }

            return gunAtt.currentClipCapacity > 0;
        }

        public override void OnActionActivate(AIBrain ai, ET.StateType stateType)
        {
        }

        public override void OnUpdate(AIBrain ai, ET.StateType stateType)
        {
            FireWithTimer(ai);
            HandleWeaponHandsUpdate(ai);
        }

        public override void OnAnimatorIK(AIBrain ai, int layerIndex, ET.StateType stateType)
        {
            HandleWeaponHandsOnAnimatorIK(ai);
        }

        public override void OnActionExit(AIBrain ai, ET.StateType stateType)
        {
        }

        private void HandleWeaponHandsUpdate(AIBrain ai)
        {
            GunAtt gunAtt = ai.GetCurrentWeaponScript();
            if (gunAtt)
            {
                Vector3 targetLocalPos = target.localPosition;
                float spreadZ = (Mathf.Abs(target.localPosition.x) + Mathf.Abs(target.localPosition.y)) * randomTwistSign * WHandIKProps.weaponSpreadAgentMultipliers.z;
                WHandIKProps.weaponSpread = Quaternion.Euler(new Vector3(-targetLocalPos.x * gunAtt.spreadAxisMultipliers.x * WHandIKProps.weaponSpreadAgentMultipliers.x, -targetLocalPos.y * gunAtt.spreadAxisMultipliers.y * WHandIKProps.weaponSpreadAgentMultipliers.y, spreadZ * gunAtt.spreadAxisMultipliers.z));

                WHandIKProps.rHandAim = Mathf.Lerp(CheckEpsilon(WHandIKProps.rHandAim, rightHandTarget), rightHandTarget,
                    (rightHandTarget == 1 ? WHandIKProps.rightHandSmooth : WHandIKProps.rightHandBackSmooth) * Time.deltaTime);
                WHandIKProps.rHandAimRot = Mathf.Lerp(CheckEpsilon(WHandIKProps.rHandAimRot, rightHandTarget), rightHandTarget,
                    (rightHandTarget == 1 ? WHandIKProps.rightHandSmooth : WHandIKProps.rightHandBackSmooth) * Time.deltaTime);

                WHandIKProps.lHandAim = Mathf.Lerp(CheckEpsilon(WHandIKProps.lHandAim, leftHandTarget), leftHandTarget, Time.deltaTime * (leftHandTarget == 1 ? WHandIKProps.leftHandSmooth : WHandIKProps.leftHandBackSmooth));
            }
        }

        private void HandleWeaponHandsOnAnimatorIK(AIBrain ai)
        {
            if (WeaponIK)
            {
                ai.Animator.SetIKPosition(AvatarIKGoal.RightHand, WeaponIK.position + WeaponIK.right * WHandIKProps.aimPositionFixer.x + WeaponIK.up * WHandIKProps.aimPositionFixer.y + WeaponIK.forward * WHandIKProps.aimPositionFixer.z);
                ai.Animator.SetIKRotation(AvatarIKGoal.RightHand, WeaponIK.rotation * Quaternion.Euler(WHandIKProps.aimRotationFixer) * WHandIKProps.weaponSpread);
                ai.Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, WHandIKProps.rHandAim);
                ai.Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, WHandIKProps.rHandAimRot);
            }

            Transform leftHandleOfGun = ai.GetCurrentWeaponScript() && ai.GetCurrentWeaponScript().GetFixer(ai.Transform.name, WeaponFixerTypes.LeftHandle) ? ai.GetCurrentWeaponScript().GetFixer(ai.Transform.name, WeaponFixerTypes.LeftHandle) : null;
            if (leftHandleOfGun)
            {
                ai.Animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandleOfGun.transform.position);
                ai.Animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandleOfGun.transform.rotation);
                ai.Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, WHandIKProps.lHandAim);
                ai.Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, WHandIKProps.lHandAim);
            }
        }

        public static float CheckEpsilon(float xFloat, float target)
        {
            if (target > .5f) // target = 1
                xFloat = (target - xFloat < .01f) ? 1 : xFloat;
            else
                xFloat = (xFloat < .01f) ? 0 : xFloat;
            return xFloat;
        }
    }

    [System.Serializable]
    public class WeaponHandIKProps
    {
        [System.NonSerialized]
        public float rHandAim, rHandAimRot, lHandAim, headAim;

        public float rightHandSmooth = 3f;
        public float rightHandBackSmooth = 4f;
        public float leftHandSmooth = 4f;
        public float leftHandBackSmooth = 4.5f;

        [Space]
        public Vector2 immutedWeaponSpreadAgentMultiplier = Vector2.one;

        public float immutedSpreadChangeSpeed = 2f;
        public Vector3 weaponSpreadAgentMultipliers = Vector3.one;
        public float weaponSpreadRecoverAgentMultiplier = 1f;
        public Vector3 aimPositionFixer = new Vector3(.02f, .06f, -.12f);
        public Vector3 aimRotationFixer = new Vector3(2.38f, 22f, 0f);

        [System.NonSerialized]
        public Quaternion weaponSpread;
    }

    [System.Serializable]
    public class FireProps
    {
        // Fire Props
        public Vector2 randShotCount = new Vector2(.5f, 4f); // Set this considering every weapon has 10 maxClipCapacity

        public Vector2 randShotCountAgentMultiplier = new Vector2(1, 1);
        public Vector2 randWaitTimer = new Vector2(.2f, 1f);
        public Vector2 waitTimerAgentMultiplier = new Vector2(1, 1);

        public List<Transform> weapons;
        public int startWeaponIndex = 0;
        public LayerMask rayCBulletLayerMask;
    }
}