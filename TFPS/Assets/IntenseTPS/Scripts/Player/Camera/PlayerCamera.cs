using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public enum CamType { ThirdPerson, FirstPerson, Focus, ThirdPersonToFirstPersonTransition, ThirdPersonToFocusTransition }

    public class PlayerCamera : MonoBehaviour
    {
        private Vector2 _XYMouseSpeedMultiplier = new Vector2(250, 125);

        public Vector2 ignoreMouseBiggerThanXY = new Vector2(2.5f, 2.5f);

        [Space]
        public float wallDetectRadius = .4f;

        public float upRayFixFromFollow = .05f;
        public LayerMask wallDetectLayerMask;

        [Space]
        [Header("Third Person")]
        public bool dontFollowBone = false;

        public HumanBodyBones followBone = HumanBodyBones.Head;

        [Space]
        public Vector3 defaultCameraOffset = new Vector3(0, .5f, -3);

        public Vector3 defaultFocusModder;
        public bool useOffsetLerp = true;
        public float defaultOffsetLerpSpeed = 2f;
        public bool usePositionLerp = true;
        public float defaultPositionLerpSpeed = .5f;
        public bool useRotationLerp = false;
        public float defaultRotationLerpSpeed = 25f;
        public Vector2 defaultVerticalRotationLimitsMaxMin_TPS = new Vector2(30f, -55f);
        public float defaultFieldOfView = 60f;
        public bool useFieldOfViewLerp = true;
        public float defaultFieldOfViewLerpSpeed = .2f;

        [Space]
        public Vector2 startRotations;

        [Space]
        public Vector2 resetThirdPersonSpeed = new Vector2(2.5f, 2.5f);

        [Space]
        [Header("First Person Free Look")]
        public Vector2 ignoreFPSMouseBiggerThanXY = new Vector2(2.5f, 2.5f);

        public bool dontPositionToBone = false;
        public HumanBodyBones positionFreeLookBone = HumanBodyBones.Head;
        public Vector3 fpsLookCamPlayerOffset;
        public Vector2 limitFPSHorizontalAngle = new Vector2(-90, 90);
        public Vector2 limitFPSVerticalAngle = new Vector2(-90, 90);
        public float rotationTransitionLerpSpeed = 10f;
        public float positionTransitionLerpSpeed = 10f;

        [Space]
        public Vector2 resetThirdPersonMinSpeedHorVer = new Vector2(.5f, .5f);

        public Vector2 resetThirdPersonMaxSpeedHorVer = new Vector2(2f, 2f);

        #region Properties

        public Camera Cam { get; private set; }
        public CamType cameraType { get; private set; }
        public float HorizontalRotation { get; private set; }
        public float VerticalRotation { get; private set; }
        public Vector3 currentOffset { get; private set; }

        #endregion Properties

        #region Private

        private SetupAndUserInput userInput;
        private Animator plAnimator;
        private Transform moveReference;
        private Transform followTransform;
        private CapsuleCollider plCapCollider;
        private List<CameraShaker> camPosShakers;
        private List<CameraShaker> camRotShakers;
        private Vector3 camPosShake;
        private Vector3 camRotShake;
        private float fpsStartHorizontalRot, fpsCurHorizontalRot;
        private float fpsStartVerticalRot, fpsCurVerticalRot;
        private float lastVerticalRot, lastHorizontalRot;
        private PlayerAtts player;
        private Vector3 targetOffset;
        private Quaternion calculatedRotation;
        private Vector3 calculatedPosition;
        private Vector3 posVel;
        private bool resetting = false;
        private float fakeDirDirectionVert;
        private float fakeDirDirectionHoriz;
        private float horiDotWas = 0;
        private float verDotWas = 0;
        private ThirdPersonCameraParams currentTPModderParams;
        private FirstPersonCameraParams currentFPModderParams;
        private FocusCameraParams currentFocusModderParams;
        private Transform positionFreeLookTransform;
        private LayersWithDefValue<CameraModderParamsBase> camTypeDict;

        #endregion Private

        private void Awake()
        {
            camTypeDict = new LayersWithDefValue<CameraModderParamsBase>(new ThirdPersonCameraParams(), "Camera");
            cameraType = CamType.ThirdPerson;
        }

        private void Start()
        {
            plAnimator = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();

            if (plAnimator)
            {
                if (dontFollowBone)
                    followTransform = plAnimator.transform;
                else
                    followTransform = plAnimator.GetBoneTransform(followBone);
                if (dontPositionToBone)
                    positionFreeLookTransform = plAnimator.transform;
                else
                    positionFreeLookTransform = plAnimator.GetBoneTransform(positionFreeLookBone);
            }

            userInput = plAnimator.GetComponent<SetupAndUserInput>();
            player = userInput.GetComponent<PlayerAtts>();
            plCapCollider = userInput.GetComponent<CapsuleCollider>();
            Cam = transform.FindChild("Main Camera").GetComponent<Camera>();
            moveReference = transform.FindChild("Move Reference");

            if (!plAnimator || !followTransform || !userInput || !player || !plCapCollider || !Cam || !moveReference)
            {
                if (userInput)
                    userInput.DisablePlayer("Needed reference not found on Player Camera script");
                enabled = false;
            }

            currentTPModderParams = new ThirdPersonCameraParams();
            currentFPModderParams = new FirstPersonCameraParams();

            camPosShakers = new List<CameraShaker>();
            camRotShakers = new List<CameraShaker>();

            HorizontalRotation = startRotations.x;
            VerticalRotation = startRotations.y;
        }

        private void Update()
        {
            if (plAnimator)
            {
                if (dontFollowBone)
                    followTransform = plAnimator.transform;
                else
                    followTransform = plAnimator.GetBoneTransform(followBone);
                if (dontPositionToBone)
                    positionFreeLookTransform = plAnimator.transform;
                else
                    positionFreeLookTransform = plAnimator.GetBoneTransform(positionFreeLookBone);
            }
        }

        private void LateUpdate()
        {
            switch (cameraType)
            {
                case CamType.ThirdPerson:
                    ChangePointerOfCamModderWithID(camTypeDict.LastValue, camTypeDict.LastId); // only for debugging

                    if (userInput.CameraResetDown)
                    {
                        Vector3 cDir = (-new Vector3(moveReference.position.x, followTransform.position.y, moveReference.position.z) + followTransform.position).normalized;

                        // Horizontal
                        float horDot = Mathf.Sign(Vector3.Dot(Quaternion.Euler(0, 90, 0) * player.transform.forward, cDir));
                        float horAngle = Vector3.Angle(player.transform.forward, cDir);

                        fakeDirDirectionHoriz = horDot < 0 ? 1 : -1;
                        horiDotWas = horDot;

                        // Vertical
                        float verDot = Mathf.Sign(Vector3.Dot(cDir, moveReference.TransformDirection(Quaternion.Euler(90, 0, 0) * Vector3.forward)));
                        float verAngle = Vector3.Angle(cDir, moveReference.forward);

                        fakeDirDirectionVert = verDot < 0 ? 1 : -1;
                        verDotWas = verDot;

                        if (horAngle > 10f || verAngle > 10f)
                            resetting = true;
                    }
                    else if (resetting)
                    {
                        Vector3 cDir = (-new Vector3(moveReference.position.x, followTransform.position.y, moveReference.position.z) + followTransform.position).normalized;

                        float horDot = Mathf.Sign(Vector3.Dot(Quaternion.Euler(0, 90, 0) * player.transform.forward, cDir));
                        float horAngle = Vector3.Angle(player.transform.forward, cDir);
                        fakeDirDirectionHoriz = Mathf.Lerp(resetThirdPersonMinSpeedHorVer.x, resetThirdPersonMaxSpeedHorVer.x,
                            horAngle / 180
                            ) * -horDot;

                        if (horDot != horiDotWas)
                            fakeDirDirectionHoriz = 0;

                        float verDot = Mathf.Sign(Vector3.Dot(cDir, moveReference.TransformDirection(Quaternion.Euler(90, 0, 0) * Vector3.forward)));
                        float verAngle = Vector3.Angle(cDir, moveReference.forward);
                        fakeDirDirectionVert = Mathf.Lerp(resetThirdPersonMinSpeedHorVer.y, resetThirdPersonMaxSpeedHorVer.y,
                            verAngle / 90
                            ) * -verDot;
                        if (verDot != verDotWas)
                            fakeDirDirectionVert = 0;

                        if (userInput.AnyKeyDown() || (fakeDirDirectionHoriz == 0 && fakeDirDirectionVert == 0))
                        {
                            resetting = false;
                        }
                    }

                    if (!resetting)
                        GetMouse();
                    else
                        GetFakeMouse();

                    targetOffset = defaultCameraOffset + currentTPModderParams.AddedThirdPersonOffset;
                    if (useOffsetLerp)
                        currentOffset = Vector3.Lerp(
                            currentOffset, targetOffset, Time.deltaTime * (defaultOffsetLerpSpeed + currentTPModderParams.AddedOffsetLerpSpeed)
                            );
                    else
                        currentOffset = targetOffset;

                    VerticalRotation = ClampRotation(VerticalRotation,
                        -defaultVerticalRotationLimitsMaxMin_TPS.x - currentTPModderParams.AddedVerticalRotationLimitsMaxMin_TPS.x,
                        -defaultVerticalRotationLimitsMaxMin_TPS.y - currentTPModderParams.AddedVerticalRotationLimitsMaxMin_TPS.y
                        );
                    HorizontalRotation = ClampRotation(HorizontalRotation, -360, 360);

                    UpdateShakers();

                    Quaternion targetRotation = Quaternion.Euler(VerticalRotation + camRotShake.x, HorizontalRotation + camRotShake.y, camRotShake.z);

                    Vector3 dx = (-transform.position + followTransform.position).normalized; dx = dx == Vector3.zero ? Vector3.one : dx; // just in case
                    Quaternion lookRot = Quaternion.LookRotation(dx);
                    Vector3 targetPosition = targetRotation * currentOffset +
                        (followTransform.position +
                        lookRot * (defaultFocusModder + currentTPModderParams.AddedFocusModder + camPosShake)
                        );

                    if (usePositionLerp)
                        calculatedPosition = Vector3.SmoothDamp(
                            transform.position, targetPosition, ref posVel, (defaultPositionLerpSpeed + currentTPModderParams.AddedPositionLerpSpeed) * Time.deltaTime
                            );
                    else
                        calculatedPosition = targetPosition;

                    Collider[] cols = Physics.OverlapSphere(transform.position, wallDetectRadius, wallDetectLayerMask);

                    if (cols.Length > 0 ||
                         (Physics.Linecast(followTransform.position + Vector3.up * upRayFixFromFollow, transform.position,/* out wallHit,*/ wallDetectLayerMask))
                        )
                    {
                        RaycastHit hit;
                        if (Physics.Linecast(followTransform.position + Vector3.up * upRayFixFromFollow, transform.position, out hit, wallDetectLayerMask))
                        {
                            Cam.transform.localPosition = new Vector3(Cam.transform.localPosition.x, Cam.transform.localPosition.y,
                            Vector3.Distance(transform.position, hit.point)
                            );
                        }
                        else
                            Cam.transform.localPosition = Vector3.zero;
                    }
                    else
                        Cam.transform.localPosition = Vector3.zero;

                    if (useRotationLerp)
                        calculatedRotation = Quaternion.Lerp(transform.rotation, targetRotation, defaultRotationLerpSpeed + currentTPModderParams.AddedRotationLerpSpeed);
                    else
                        calculatedRotation = targetRotation;

                    transform.position = calculatedPosition;
                    transform.rotation = calculatedRotation;

                    {
                        Quaternion targetRotation1 = Quaternion.Euler(VerticalRotation, HorizontalRotation, 0);

                        Vector3 targetPosition1 =
                            targetRotation1 * -Vector3.forward +
                            (followTransform.position
                            );
                        moveReference.position = targetPosition1;
                        moveReference.rotation = targetRotation1;
                    }

                    float targetFieldOfView = defaultFieldOfView + currentTPModderParams.AddedFieldOfView;
                    if (useFieldOfViewLerp)
                        Cam.fieldOfView = Mathf.Lerp(Cam.fieldOfView, targetFieldOfView,
                            defaultFieldOfViewLerpSpeed + ((targetFieldOfView > Cam.fieldOfView) ? currentTPModderParams.AddedFieldOfViewLerpSpeedUp : currentTPModderParams.AddedFieldOfViewLerpSpeedDown) * Time.deltaTime
                            );
                    else
                        Cam.fieldOfView = targetFieldOfView;
                    break;

                case CamType.ThirdPersonToFirstPersonTransition:
                    transform.rotation = Quaternion.Lerp(transform.rotation, player.transform.rotation, Time.deltaTime * rotationTransitionLerpSpeed);
                    transform.position = Vector3.Lerp(transform.position, positionFreeLookTransform.TransformPoint(fpsLookCamPlayerOffset), Time.deltaTime * positionTransitionLerpSpeed);
                    HorizontalRotation = transform.rotation.eulerAngles.x;
                    if (Vector3.Distance(transform.position, positionFreeLookTransform.TransformPoint(fpsLookCamPlayerOffset)) < .009f)
                    {
                        cameraType = CamType.FirstPerson;
                        transform.rotation = player.transform.rotation;
                        transform.position = Vector3.Lerp(transform.position, positionFreeLookTransform.TransformPoint(fpsLookCamPlayerOffset), Time.deltaTime * positionTransitionLerpSpeed);
                        fpsStartHorizontalRot = fpsCurHorizontalRot = transform.rotation.eulerAngles.y;
                        fpsStartVerticalRot = fpsCurVerticalRot = transform.rotation.eulerAngles.x;
                    }
                    break;

                case CamType.FirstPerson:

                    transform.position = Vector3.Lerp(transform.position, positionFreeLookTransform.TransformPoint(fpsLookCamPlayerOffset), Time.deltaTime * positionTransitionLerpSpeed);

                    fpsCurHorizontalRot += (userInput.MouseX > ignoreFPSMouseBiggerThanXY.x ? ignoreFPSMouseBiggerThanXY.x : userInput.MouseX) * _XYMouseSpeedMultiplier.x * 0.02f;
                    fpsCurVerticalRot -= (userInput.MouseY > ignoreFPSMouseBiggerThanXY.y ? ignoreFPSMouseBiggerThanXY.y : userInput.MouseY) * _XYMouseSpeedMultiplier.y * 0.02f;

                    fpsCurHorizontalRot = Mathf.Clamp(fpsCurHorizontalRot, fpsStartHorizontalRot - limitFPSHorizontalAngle.y, fpsStartHorizontalRot + limitFPSHorizontalAngle.y);
                    fpsCurVerticalRot = Mathf.Clamp(fpsCurVerticalRot, fpsStartVerticalRot + limitFPSVerticalAngle.x, fpsStartVerticalRot - limitFPSVerticalAngle.x);

                    transform.rotation = Quaternion.Euler(fpsCurVerticalRot, fpsCurHorizontalRot, 0);
                    break;

                case CamType.ThirdPersonToFocusTransition:
                    cameraType = CamType.Focus;
                    break;

                case CamType.Focus:
                    Vector3 targetLerpToPos = currentFocusModderParams.IsLerpingToTransform
                        ? currentFocusModderParams.LerpToTransform.TransformPoint(currentFocusModderParams.FocusSerializedFields.LerpedTransformOffset)
                        : currentFocusModderParams.LerpToPosition;
                    Vector3 targetFocusPos = currentFocusModderParams.IsFocusingAtTransform
                        ? currentFocusModderParams.FocusTransform.TransformPoint(currentFocusModderParams.FocusSerializedFields.FocusedTransformOffset)
                        : currentFocusModderParams.FocusPosition;
                    Quaternion targetFocusRot = Quaternion.LookRotation((-transform.position + targetFocusPos).normalized);

                    transform.position = Vector3.Lerp(transform.position, targetLerpToPos, Time.deltaTime * currentFocusModderParams.FocusSerializedFields.LerpPosSpeed);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetFocusRot, Time.deltaTime * currentFocusModderParams.FocusSerializedFields.LerpRotSpeed);

                    if (Vector3.Distance(transform.position, targetLerpToPos) < .009f)
                    {
                        cameraType = CamType.Focus;
                    }
                    break;

                default:
                    break;
            }
        }

        private float ClampRotation(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }

        public void AddShakeCamera(CameraShaker shakerPos, CameraShaker shakerRot)
        {
            if (shakerPos != null)
                camPosShakers.Add(new CameraShaker(shakerPos));
            if (shakerRot != null)
                camRotShakers.Add(new CameraShaker(shakerRot));
        }

        private void UpdateShakers()
        {
            for (int i = 0; i < camPosShakers.Count; i++)
            {
                CameraShaker camShaker = camPosShakers[i];
                camPosShake -= camShaker.LastShake;
                Vector3 thisShake = camShaker.Shake();
                if (camShaker.HasEnded)
                    camPosShakers.RemoveAt(i);
                else
                    camPosShake += thisShake;
            }
            for (int i = 0; i < camRotShakers.Count; i++)
            {
                CameraShaker camShaker = camRotShakers[i];
                camRotShake -= camShaker.LastShake;
                Vector3 thisShake = camShaker.Shake();
                if (camShaker.HasEnded)
                    camRotShakers.RemoveAt(i);
                else
                    camRotShake += thisShake;
            }

            // Just in case
            if (camPosShakers.Count == 0)
                camPosShake = Vector3.zero;
            if (camRotShakers.Count == 0)
                camRotShake = Vector3.zero;
        }

        private void GetMouse()
        {
            HorizontalRotation += (userInput.MouseX > ignoreMouseBiggerThanXY.x ? ignoreMouseBiggerThanXY.x : userInput.MouseX) * _XYMouseSpeedMultiplier.x * 0.02f;
            VerticalRotation -= (userInput.MouseY > ignoreMouseBiggerThanXY.y ? ignoreMouseBiggerThanXY.y : userInput.MouseY) * _XYMouseSpeedMultiplier.y * 0.02f;
        }

        private void GetFakeMouse()
        {
            HorizontalRotation += fakeDirDirectionHoriz * resetThirdPersonSpeed.x * _XYMouseSpeedMultiplier.x * 0.02f;
            VerticalRotation -= fakeDirDirectionVert * resetThirdPersonSpeed.y * _XYMouseSpeedMultiplier.y * 0.02f;
        }

        public Vector2 GetVerticalLimitsMaxMin()
        {
            return new Vector2(currentTPModderParams.AddedVerticalRotationLimitsMaxMin_TPS.x + defaultVerticalRotationLimitsMaxMin_TPS.x,
                currentTPModderParams.AddedVerticalRotationLimitsMaxMin_TPS.y + defaultVerticalRotationLimitsMaxMin_TPS.y
                );
        }

        #region Cam Modifier Functions

        public void OverrideCamera(CameraModderParamsBase _params, short priority, string id)
        {
            if (camTypeDict.IsOverridenWithKey(id))
            {
                Debug.Log("Camera already overritten with this key (skipping camera override) :" + id.ToString());
                return;
            }
            CameraModderParamsBase x = camTypeDict.LastValue;
            camTypeDict.Override(id, priority, _params);

            if (id == camTypeDict.LastId)
                TransitTo(x, _params, true);
        }

        private void TransitTo(CameraModderParamsBase from, CameraModderParamsBase _toParams, bool isRelease)
        {
            if (from.GetType() == typeof(ThirdPersonCameraParams))
            {
                if (_toParams.GetType() == typeof(ThirdPersonCameraParams))
                    FromTpToTp(isRelease, _toParams as ThirdPersonCameraParams);
                else if (_toParams.GetType() == typeof(FirstPersonCameraParams))
                    FromTpToFp(isRelease, _toParams as FirstPersonCameraParams);
                else if (_toParams.GetType() == typeof(FocusCameraParams))
                    FromTpToFocus(isRelease, _toParams as FocusCameraParams);
                else
                    Debug.Log("Camera transition doesnt exists");
            }
            else if (from.GetType() == typeof(FirstPersonCameraParams))
            {
                if (_toParams.GetType() == typeof(ThirdPersonCameraParams))
                    FromFpToTp(isRelease, _toParams as ThirdPersonCameraParams);
                else if (_toParams.GetType() == typeof(FirstPersonCameraParams))
                    FromFpToFp(isRelease, _toParams as FirstPersonCameraParams);
                else
                    Debug.Log("Camera transition doesnt exists");
            }
            else if (from.GetType() == typeof(FocusCameraParams))
            {
                if (_toParams.GetType() == typeof(ThirdPersonCameraParams))
                    FromFocusToTp(isRelease, _toParams as ThirdPersonCameraParams);
                else
                    Debug.Log("Camera transition doesnt exists");
            }
        }

        private void FromTpToTp(bool isRelease, ThirdPersonCameraParams _nextParams)
        {
            cameraType = CamType.ThirdPerson;
            currentTPModderParams.Reset();
            currentTPModderParams += _nextParams;
        }

        private void FromTpToFp(bool isRelease, FirstPersonCameraParams _nextParams)
        {
            cameraType = CamType.ThirdPersonToFirstPersonTransition;
            currentFPModderParams.Reset();
            currentFPModderParams += _nextParams;

            lastVerticalRot = VerticalRotation;
            lastHorizontalRot = HorizontalRotation;
        }

        private void FromTpToFocus(bool isRelease, FocusCameraParams _nextParams)
        {
            cameraType = CamType.ThirdPersonToFocusTransition;
            currentFocusModderParams = _nextParams;
            lastVerticalRot = VerticalRotation;
            lastHorizontalRot = HorizontalRotation;
        }

        private void FromFocusToTp(bool isRelease, ThirdPersonCameraParams _nextParams)
        {
            FromFpToTp(isRelease, _nextParams);
        }

        private void FromFpToFp(bool isRelease, FirstPersonCameraParams _nextParams)
        {
            FromTpToFp(isRelease, _nextParams);
        }

        private void FromFpToTp(bool isRelease, ThirdPersonCameraParams _nextParams)
        {
            FromTpToTp(isRelease, _nextParams);
            VerticalRotation = lastVerticalRot;
            HorizontalRotation = lastHorizontalRot;
        }

        public void ReleaseOverride(string id)
        {
            if (!camTypeDict.Contains(id))
            {
                Debug.Log("No such key-id found, skipping Release camera... id: " + id);
                return;
            }

            CameraModderParamsBase x = camTypeDict.LastValue;
            camTypeDict.Release(id);
            TransitTo(x, camTypeDict.LastValue, true);
        }

        public void ChangePointerOfCamModderWithID(CameraModderParamsBase _newCamModderParam, string _modifierID)
        {
            if (!camTypeDict.IsOverridenWithKey(_modifierID))
            {
                Debug.Log("No such key-id found, skipping param change... id: " + _modifierID);
                return;
            }
            camTypeDict.Modify(_modifierID, _newCamModderParam);

            if (_newCamModderParam.GetType() == typeof(ThirdPersonCameraParams))
            {
                currentTPModderParams.Reset();
                currentTPModderParams += _newCamModderParam as ThirdPersonCameraParams;
            }
            else if (_newCamModderParam.GetType() == typeof(FirstPersonCameraParams))
            {
                currentFPModderParams.Reset();
                currentFPModderParams += _newCamModderParam as FirstPersonCameraParams;
            }
            else if (_newCamModderParam.GetType() == typeof(FocusCameraParams))
            {
                currentFocusModderParams = _newCamModderParam as FocusCameraParams;
            }
        }

        public bool IsOverridenWithKey(string _Id)
        {
            return camTypeDict.IsOverridenWithKey(_Id);
        }

        #endregion Cam Modifier Functions
    }
}