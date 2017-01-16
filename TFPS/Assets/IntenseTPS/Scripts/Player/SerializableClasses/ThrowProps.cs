using UnityEngine;

namespace Player
{
    [System.Serializable]
    public class ThrowProps
    {
        public float layerEnableSpeed = 5f;
        public float layerDisableSpeed = 5f;

        [Space]
        public float throwParamLerpSpeed = 3f;

        [Space]
        public ThirdPersonCameraParams cameraProps;
    }

    [System.Serializable]
    public class ThrowStyle
    {
        public ThrowProps throwProps;
    }
}