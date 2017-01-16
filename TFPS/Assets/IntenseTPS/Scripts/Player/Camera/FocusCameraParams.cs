using UnityEngine;

namespace Player
{
    [System.Serializable]
    public class FocusCameraSerializedFields
    {
        public Vector3 FocusedTransformOffset;
        public Vector3 LerpedTransformOffset;
        public float LerpPosSpeed = 5;
        public float LerpRotSpeed = 5;
    }

    public class FocusCameraParams : CameraModderParamsBase
    {
        public FocusCameraSerializedFields FocusSerializedFields { get; set; }

        public Transform LerpToTransform { get; private set; }
        public Transform FocusTransform { get; private set; }
        public Vector3 LerpToPosition { get; private set; }
        public Vector3 FocusPosition { get; private set; }
        public bool IsLerpingToTransform { get; private set; }
        public bool IsFocusingAtTransform { get; private set; }

        public FocusCameraParams(FocusCameraSerializedFields _focusSFields, Transform _lerpToTransform, Transform _focusTransform)
        {
            IsLerpingToTransform = true;
            IsFocusingAtTransform = true;
            LerpToTransform = _lerpToTransform;
            FocusTransform = _focusTransform;
            FocusSerializedFields = _focusSFields;
        }

        public FocusCameraParams(FocusCameraSerializedFields _focusSFields, Transform _lerpToTransform, Vector3 _focusPosition)
        {
            IsLerpingToTransform = true;
            IsFocusingAtTransform = false;
            LerpToTransform = _lerpToTransform;
            FocusPosition = _focusPosition;
            FocusSerializedFields = _focusSFields;
        }

        public FocusCameraParams(FocusCameraSerializedFields _focusSFields, Vector3 _lerpToPosition, Vector3 _focusPosition)
        {
            IsLerpingToTransform = false;
            LerpToPosition = _lerpToPosition;
            FocusPosition = _focusPosition;
            FocusSerializedFields = _focusSFields;
        }
    }
}