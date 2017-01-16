using StateSystems;
using System;
using UnityEngine;

namespace Shooter.StateSystems
{
    /// <summary>
    /// Uses IK system of unity to look at a target, has parameters to modify for different agent models (Only horizontal for now).
    /// </summary>
    public class AIShooterStateSystemLookAt : AIStateSystemLookAt
    {
        private Vector3 lookAtPosition;
        private ET.LookAtType lookAtType;
        private float headIKTarget;
        public LookIKProps LookIKProps { get; set; }

        public override float GetHeadIKWeight()
        {
            return LookIKProps.headAim;
        }

        public AIShooterStateSystemLookAt(LookIKProps _lookIKProps)
        {
            LookIKProps = _lookIKProps;
        }

        public override void OnStart(AIBrain ai)
        {
            foreach (Transform t in ai.Transform.GetComponentsInChildren<Transform>())
                if (t.CompareTag("Target"))
                {
                    t.GetComponent<AiTargetLogic>().ssl = this;
                }
        }

        public override void SetLookAtPosNStartLook(AIBrain ai, Vector3 position, ET.LookAtType _lookAtType)
        {
            SetLookAtPosition(ai, position, lookAtType);
            StartLooking(ai);
        }

        public override void SetLookAtPosition(AIBrain ai, Vector3 position, ET.LookAtType _lookAtType = ET.LookAtType.Forward)
        {
            lookAtPosition = position;
            lookAtType = _lookAtType;
        }

        public override void SetLookAtPosNStartLook(AIBrain ai, ET.LookAtType lookAtType)
        {
            SetLookAtPosition(ai, lookAtType);
            StartLooking(ai);
        }

        public override void SetLookAtPosition(AIBrain ai, ET.LookAtType _lookAtType)
        {
            lookAtType = _lookAtType;
        }

        public override void StartLooking(AIBrain ai)
        {
            headIKTarget = 1;
        }

        public override void StopLooking(AIBrain ai)
        {
            headIKTarget = 0;
        }

        public override void OnUpdate(AIBrain ai, ET.StateType stateType)
        {
            SolveLookIKUpdate(ai);
        }

        public override void OnAnimatorIK(AIBrain ai, int layerIndex, ET.StateType stateType)
        {
            SolveLookIKOnAnimatorIK(ai);
        }

        public void SolveLookIKUpdate(AIBrain ai)
        {
            LookIKProps.headAim = Mathf.Lerp(CheckEpsilon(LookIKProps.headAim, headIKTarget), headIKTarget, (headIKTarget == 1 ? LookIKProps.headIKSmooth : LookIKProps.headIKBackSmooth) * Time.deltaTime);

            Vector3 lookPos = Vector3.zero;
            switch (lookAtType)
            {
                case ET.LookAtType.ToCurrentTarget:
                    lookPos = ai.GetCurrentTargetPos();
                    break;

                case ET.LookAtType.ToPosition:
                    lookPos = lookAtPosition;
                    break;

                case ET.LookAtType.Forward:
                    lookPos = ai.Transform.position + ai.Transform.forward * 5f;
                    break;

                default:
                    break;
            }

            LookIKProps.midPos = ai.Transform.position + Vector3.up * LookIKProps.lookStartHeight;
            Vector3 mPosToTDir = -LookIKProps.midPos + new Vector3(lookPos.x, LookIKProps.midPos.y, lookPos.z);
            Vector3 mPosToCDir = LookIKProps.midPos.y * (Quaternion.AngleAxis(LookIKProps.tAngle, Vector3.up) * ai.Transform.TransformDirection(Vector3.forward));

            Vector3 mPosToZeroDir = LookIKProps.midPos.y * (Quaternion.AngleAxis(0, Vector3.up) * ai.Transform.TransformDirection(Vector3.forward));

            //Debug.DrawRay(LookIKProps.midPos, mPosToCDir * LookIKProps.fwSize, Color.red);
            float cAngleBw = Vector3.Angle(mPosToTDir, mPosToZeroDir);
            cAngleBw = cAngleBw * Mathf.Sign(Vector3.Dot(Quaternion.AngleAxis(90, Vector3.up) * mPosToZeroDir, mPosToTDir));

            float aimingAnglePlus = 0f, weaponBodyBob = 0;
            if (ai.GetStateSystem<AIShooterStateSystemWeapon>() != null /*&& ai.WorldState.GetValue(DictionaryStrings.weaponAimed) == true.ToString()*/ && ai.HaveCurrentWeapon())
            {
                aimingAnglePlus = ai.GetCurrentWeaponScript().bodyFixRight * LookIKProps.aimingWeaponHorFixMultiplier;
                weaponBodyBob = ai.GetStateSystem<AIShooterStateSystemWeapon>().weaponBodyBob;
            }
            if (headIKTarget == 0)
            {
                LookIKProps.tAngle = Mathf.Lerp(LookIKProps.tAngle, 0, Time.deltaTime * LookIKProps.tAngleLerpBackSpeed);
            }
            else
                LookIKProps.tAngle = Mathf.Lerp(LookIKProps.tAngle, cAngleBw + aimingAnglePlus + weaponBodyBob * LookIKProps.aiWeaponBodyBobMultiplier, Time.deltaTime * LookIKProps.tAngleLerpSpeed);
            LookIKProps.tAngle = Mathf.Clamp(LookIKProps.tAngle, -LookIKProps.maxLookTAngle, LookIKProps.maxLookTAngle);

            LookIKProps.realLookPos = LookIKProps.midPos + mPosToCDir.normalized * LookIKProps.fwSize;
        }

        public void SolveLookIKOnAnimatorIK(AIBrain ai)
        {
            ai.Animator.SetLookAtPosition(LookIKProps.realLookPos);
            ai.Animator.SetLookAtWeight(LookIKProps.headAim, 1, 1, 0, .5f);
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
}

[System.Serializable]
public class LookIKProps
{
    public float headIKSmooth = 3f;
    public float headIKBackSmooth = 3f;
    public float aiWeaponBodyBobMultiplier = 15f;
    public float weaponBodyRecoverSpeedMultiplier = 50f;

    [Space]
    [System.NonSerialized]
    public float tAngle;

    public float maxLookTAngle = 75;

    [NonSerialized]
    public Vector3 midPos;

    public float lookStartHeight = 1.5f;
    public float fwSize = 1.35f;
    public float tAngleLerpSpeed = 4f;
    public float tAngleLerpBackSpeed = 4.5f;
    public float aimingWeaponHorFixMultiplier = 1.83f;
    public float headAim;

    [NonSerialized]
    public Vector3 realLookPos;
}