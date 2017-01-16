using UnityEngine;

namespace Player
{
    #region Override Params

    public class LookAtParams
    {
        public int LookAtStyleIndex { get; private set; }

        public LookAtParams(int _lookAtStyleIndex)
        {
            LookAtStyleIndex = _lookAtStyleIndex;
        }
    }

    public class LookAtTransformParams : LookAtParams
    {
        public Transform LookAtTransform { get; private set; }
        public Transform ReferenceTransform { get; private set; }

        public LookAtTransformParams(Transform _lookAtTransform, Transform _referenceTransform, int _lookAtStyleIndex) : base(_lookAtStyleIndex)
        {
            LookAtTransform = _lookAtTransform;
            ReferenceTransform = _referenceTransform;
        }
    }

    public class LookAtPositionParams : LookAtParams
    {
        public Vector3 LookAtPosition { get; private set; }
        public Transform ReferenceTransform { get; private set; }

        public LookAtPositionParams(Vector3 _lookAtPosition, Transform _referenceTransform, int _lookAtStyleIndex) : base(_lookAtStyleIndex)
        {
            LookAtPosition = _lookAtPosition;
            ReferenceTransform = _referenceTransform;
        }
    }

    public class DeactivatedLookAtParams : LookAtParams
    {
        public DeactivatedLookAtParams(int _lookAtStyleIndex) : base(_lookAtStyleIndex)
        {
        }
    }

    #endregion Override Params

    public class LookIKCSMB : CustomPlayerSystemSMB
    {
        #region Properties

        public float HorizontalLookAnglePlus { get; set; }
        public float VerticalLookAnglePlus { get; set; }

        public LookIKEvents LookEvents { get; private set; }

        private bool lookEnabled;

        public bool IsLookingAt
        {
            get { return lookEnabled; }
            private set { lookEnabled = value; }
        }

        public bool IsLookAtIKActive()
        {
            return !(AtMinEpsilon(headAim) && headIKTarget == 0);
        }

        #endregion Properties

        #region Private

        private Vector3 lookAtPosition;
        private float headIKTarget;
        private float headAim = 0;
        private LookAtIKProps cLookProps;
        private Vector3 headPos;
        private Transform currentReferenceTransform;
        private Vector2 tAngles; // Horizontal & Vertical
        private Vector3 realLookPos;
        private Transform lookAtTransform;
        private LayersWithDefValue<LookAtParams> LookAtTypeDict;

        #endregion Private

        #region Starters

        public override void OnEnabled(Animator anim)
        {
            LookAtTypeDict = new LayersWithDefValue<LookAtParams>(new LookAtParams(player.defaultLookAtIKStyleIndex), "LookIK");
            LookEvents = new LookIKEvents();
        }

        public override void OnStart()
        {
            IsLookingAt = false;
            SetLookAtIKStyle(player.defaultLookAtIKStyleIndex);
            if (cLookProps == null)
                userInput.DisablePlayer("Default cLookProps not found by" + this);
        }

        #endregion Starters

        #region Overriders

        public void OverrideLookAtTransform(LookAtTransformParams param, short priority, string id)
        {
            if (param.LookAtTransform == null)
            {
                Debug.Log("Lookat object is null, looking at failed");
                return;
            }
            if (param.ReferenceTransform == null)
            {
                Debug.Log("Reference look transform is null, looking at failed");
                return;
            }
            LookAtTypeDict.Override(id, priority, param);
            if (LookAtTypeDict.LastId == id)
                ToLookAtTransform(param);
        }

        public bool IsOverridenWithKey(string key)
        {
            return LookAtTypeDict.IsOverridenWithKey(key);
        }

        private void ToLookAtTransform(LookAtTransformParams param)
        {
            lookAtTransform = param.LookAtTransform;
            currentReferenceTransform = param.ReferenceTransform;
            StartToLookAt(param.LookAtTransform);
            if (param.LookAtStyleIndex >= 0)
            {
                SetLookAtIKStyle(param.LookAtStyleIndex);
            }
            else
            {
                SetLookAtIKStyle(player.defaultLookAtIKStyleIndex);
            }
        }

        public void OverrideLookAtPosition(LookAtPositionParams param, short priority, string id)
        {
            if (param.ReferenceTransform == null)
            {
                Debug.Log("Reference look transform is null, looking at failed");
                return;
            }
            LookAtTypeDict.Override(id, priority, param);
            if (LookAtTypeDict.LastId == id)
                ToLookAtPosition(param);
        }

        private void ToLookAtPosition(LookAtPositionParams param)
        {
            currentReferenceTransform = param.ReferenceTransform;
            lookAtTransform = null;
            StartToLookAt(param.LookAtPosition);
            if (param.LookAtStyleIndex >= 0)
            {
                SetLookAtIKStyle(param.LookAtStyleIndex);
            }
            else
            {
                SetLookAtIKStyle(player.defaultLookAtIKStyleIndex);
            }
        }

        public void OverrideToDeactivateLookAt(DeactivatedLookAtParams param, short priority, string id)
        {
            LookAtTypeDict.Override(id, priority, param);
            if (LookAtTypeDict.LastId == id)
                ToDeactivatedLookAt(param);
        }

        private void ToDeactivatedLookAt(DeactivatedLookAtParams param)
        {
            lookAtTransform = null;
            StopLookAt();
            if (param.LookAtStyleIndex >= 0)
            {
                SetLookAtIKStyle(param.LookAtStyleIndex);
            }
            else
            {
                SetLookAtIKStyle(player.defaultLookAtIKStyleIndex);
            }
        }

        public void ReleaseOverrideLookAt(string id)
        {
            if (LookAtTypeDict.IsOverridenWithKey(id))
            {
                LookAtTypeDict.Release(id);
                if (LookAtTypeDict.LastValue.GetType() == typeof(LookAtTransformParams))
                {
                    ToLookAtTransform(LookAtTypeDict.LastValue as LookAtTransformParams);
                }
                else if (LookAtTypeDict.LastValue.GetType() == typeof(LookAtPositionParams))
                {
                    ToLookAtPosition(LookAtTypeDict.LastValue as LookAtPositionParams);
                }
                else if (LookAtTypeDict.LastValue.GetType() == typeof(DeactivatedLookAtParams))
                {
                    ToDeactivatedLookAt(LookAtTypeDict.LastValue as DeactivatedLookAtParams);
                }
                else if (LookAtTypeDict.LastValue.GetType() == typeof(LookAtParams))
                {
                    StopLookAt();
                    SetLookAtIKStyle(LookAtTypeDict.LastValue.LookAtStyleIndex);
                }
            }
        }

        #endregion Overriders

        #region StateMachine Updates

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (lookEnabled || (!lookEnabled && !AtMinEpsilon(headAim)))
            {
                if (lookAtTransform)
                    lookAtPosition = lookAtTransform.position;

                headAim = Mathf.Lerp(CheckEpsilon(headAim, headIKTarget), headIKTarget,
                    (headIKTarget == 1 ? cLookProps.headIKSmooth : cLookProps.headIKBackSmooth) * Time.deltaTime);
                headPos = animator.GetBoneTransform(HumanBodyBones.Head).position;

                Vector3 fromHeadToLookAtNoXRot = (-headPos + new Vector3(lookAtPosition.x, headPos.y, lookAtPosition.z)).normalized;
                Vector3 mPosToCDir = headPos.y * (Quaternion.AngleAxis(tAngles.x, Vector3.up) * currentReferenceTransform.TransformDirection(Vector3.forward));
                Vector3 mPosToZeroDir = headPos.y * (Quaternion.AngleAxis(0, Vector3.up) * currentReferenceTransform.TransformDirection(Vector3.forward));
                float targetAngleHorizontal = Vector3.Angle(fromHeadToLookAtNoXRot, mPosToZeroDir);
                targetAngleHorizontal = targetAngleHorizontal * Mathf.Sign(Vector3.Dot(Quaternion.AngleAxis(90, Vector3.up) * mPosToZeroDir, fromHeadToLookAtNoXRot));

                float bottomEdgeSize = Vector3.Distance(headPos, new Vector3(lookAtPosition.x, headPos.y, lookAtPosition.z));
                Vector3 lookPosWOY = headPos + mPosToCDir.normalized * bottomEdgeSize;

                #region Vertical

                Vector3 fromHeadToLookAt = (lookAtPosition - headPos).normalized;

                float targetAngleVertical = Vector3.Angle(fromHeadToLookAt, fromHeadToLookAtNoXRot);

                float dot = Mathf.Sign(Vector3.Dot(fromHeadToLookAt, -Vector3.up));

                float plusYHeadPos = -bottomEdgeSize * Mathf.Sin(tAngles.y * Mathf.Deg2Rad) / Mathf.Sin((90 - tAngles.y) * Mathf.Deg2Rad);
                targetAngleVertical *= dot;

                #endregion Vertical

                if (headIKTarget == 0)
                {
                    tAngles = Vector2.Lerp(tAngles, Vector2.zero, Time.deltaTime * cLookProps.angleLerpBackSpeed);
                }
                else
                {
                    tAngles = Vector2.Lerp(tAngles,
                        new Vector2(targetAngleHorizontal + HorizontalLookAnglePlus,
                        targetAngleVertical + VerticalLookAnglePlus),
                        Time.deltaTime * cLookProps.angleLerpSpeed);
                }

                tAngles.x = Mathf.Clamp(tAngles.x, -cLookProps.maxLookTAngleHorizontal, cLookProps.maxLookTAngleHorizontal);
                tAngles.y = Mathf.Clamp(tAngles.y, -cLookProps.maxLookTAngleVertical, cLookProps.maxLookTAngleVertical);

                realLookPos = lookPosWOY + Vector3.up * plusYHeadPos;
            }
            else if (AtMinEpsilon(headAim) && !lookEnabled)
            {
                headAim = 0;
            }
        }

        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetLookAtPosition(realLookPos);
            animator.SetLookAtWeight(headAim, cLookProps.slawBodyWeight, cLookProps.slawHeadWeight, 0, cLookProps.slawClamp);
        }

        #endregion StateMachine Updates

        #region Logic Functions

        public void SetLookAtIKStyle(int styleIndex)
        {
            if (styleIndex < player.lookAtIKStyles.Count)
            {
                cLookProps = player.lookAtIKStyles[styleIndex].lookAtProps;
            }
        }

        public void StartToLookAt(Vector3 lookAtPos)
        {
            LookEvents.InvokeStartToLookAtPosition(lookAtPos);
            headIKTarget = 1;
            lookEnabled = true;
            SetLookAtPosition(lookAtPos);
        }

        public void StartToLookAt(Transform lookAtTransform)
        {
            LookEvents.InvokeStartToLookAtObject(lookAtTransform);
            this.lookAtTransform = lookAtTransform;
            StartToLookAt(lookAtTransform.position);
        }

        public void SetLookAtPosition(Vector3 lookAtPos)
        {
            lookAtPosition = lookAtPos;
        }

        public void SetLookAtPosition(Transform trnsfrm)
        {
            lookAtTransform = trnsfrm;
            SetLookAtPosition(trnsfrm.position);
        }

        public void StopLookAt()
        {
            LookEvents.InvokeStopLookingAt();
            lookEnabled = false;
            lookAtTransform = null;
            headIKTarget = 0;
        }

        public int HasBindedLookIKStyleToWeaponStyle(int weaponStyle)
        {
            int i = 0;
            foreach (LookAtIKStyle style in player.lookAtIKStyles)
            {
                if (style.bindedWeaponStyles.Contains(weaponStyle))
                    return i;
                i++;
            }
            return -1;
        }

        public static float CheckEpsilon(float xFloat, float target)
        {
            if (target > .5f) // target = 1
                xFloat = (target - xFloat < .01f) ? 1 : xFloat;
            else
                xFloat = (xFloat < .01f) ? 0 : xFloat;
            return xFloat;
        }

        public static bool AtMinEpsilon(float t)
        {
            if (t < .02f)
                return true;
            return false;
        }

        #endregion Logic Functions
    }
}