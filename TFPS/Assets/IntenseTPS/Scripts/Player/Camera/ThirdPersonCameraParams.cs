using UnityEngine;

namespace Player
{
    [System.Serializable]
    public class ThirdPersonCameraParams : CameraModderParamsBase
    {
        public Vector3 AddedThirdPersonOffset;
        public Vector3 AddedFocusModder;
        public float AddedOffsetLerpSpeed;
        public float AddedPositionLerpSpeed;
        public float AddedRotationLerpSpeed;
        public Vector2 AddedVerticalRotationLimitsMaxMin_TPS;
        public float AddedFieldOfView;
        public float AddedFieldOfViewLerpSpeedUp;
        public float AddedFieldOfViewLerpSpeedDown;

        public ThirdPersonCameraParams()
        {
            AddedThirdPersonOffset = Vector3.zero;
            AddedFocusModder = Vector3.zero;
            AddedOffsetLerpSpeed = 0;
            AddedPositionLerpSpeed = 0;
            AddedRotationLerpSpeed = 0;
            AddedVerticalRotationLimitsMaxMin_TPS = Vector3.zero;
            AddedFieldOfView = 0;
            AddedFieldOfViewLerpSpeedUp = 0;
            AddedFieldOfViewLerpSpeedDown = 0;
        }

        public override void Reset()
        {
            AddedThirdPersonOffset = Vector3.zero;
            AddedFocusModder = Vector3.zero;
            AddedOffsetLerpSpeed = 0;
            AddedPositionLerpSpeed = 0;
            AddedRotationLerpSpeed = 0;
            AddedVerticalRotationLimitsMaxMin_TPS = Vector3.zero;
            AddedFieldOfView = 0;
            AddedFieldOfViewLerpSpeedUp = 0;
            AddedFieldOfViewLerpSpeedDown = 0;
        }

        public static ThirdPersonCameraParams operator +(ThirdPersonCameraParams modifier1ReturnedAddTo, ThirdPersonCameraParams modifier2AddThis)
        {
            modifier1ReturnedAddTo.AddedThirdPersonOffset += modifier2AddThis.AddedThirdPersonOffset;
            modifier1ReturnedAddTo.AddedFocusModder += modifier2AddThis.AddedFocusModder;
            modifier1ReturnedAddTo.AddedOffsetLerpSpeed += modifier2AddThis.AddedOffsetLerpSpeed;
            modifier1ReturnedAddTo.AddedPositionLerpSpeed += modifier2AddThis.AddedPositionLerpSpeed;
            modifier1ReturnedAddTo.AddedRotationLerpSpeed += modifier2AddThis.AddedRotationLerpSpeed;
            modifier1ReturnedAddTo.AddedVerticalRotationLimitsMaxMin_TPS += modifier2AddThis.AddedVerticalRotationLimitsMaxMin_TPS;
            modifier1ReturnedAddTo.AddedFieldOfView += modifier2AddThis.AddedFieldOfView;
            modifier1ReturnedAddTo.AddedFieldOfViewLerpSpeedUp += modifier2AddThis.AddedFieldOfViewLerpSpeedUp;
            modifier1ReturnedAddTo.AddedFieldOfViewLerpSpeedDown += modifier2AddThis.AddedFieldOfViewLerpSpeedDown;

            return modifier1ReturnedAddTo;
        }
    }
}