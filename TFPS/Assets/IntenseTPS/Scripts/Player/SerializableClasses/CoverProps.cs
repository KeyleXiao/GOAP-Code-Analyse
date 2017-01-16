using UnityEngine;

namespace Player
{
    [System.Serializable]
    public class CoverProps
    {
        public float layerEnableSpeed;
        public float layerDisableSpeed;
        public float toCoverPositionLerpSpeed = 3f;
        public float toCoverRotationLerpSpeed = 5f;

        [Space]
        public float distFromWall = .5f;

        public float yRotationFixer;

        [Space]
        public float startMoveAtStickAngle = 30f;

        public float exitCoverAtAngleMax = 15f;
        public float animSpeedDamp = .05f;

        [Space]
        public float rayLength;
        public float sideRayDist;

        [Space]
        public float characterHeight = 1.8f;

        public int crouchRayCheckCount = 5;
        public float crouchCheckStartHeight = .63f;
        public float crouchCheckRayMaxDist = .37f;
        public float animParamDamp = .2f;

        [Space]
        [Range(0, 90)]
        public float edgePeekAngleTolerance = 25f;

        public float oppositeDirMaxAngle = 90;

        [Space]
        public float minDistToGoToCamCover = 1.5f;

        public float toCameraCoverStopDistance = .4f;

        [Space]
        public ThirdPersonCameraParams cameraModifiersIdleLeft;

        public ThirdPersonCameraParams cameraModifiersIdleRight;

        public ThirdPersonCameraParams cameraModifiersEdgePeekLeft;
        public ThirdPersonCameraParams cameraModifiersEdgePeekRight;

        public ThirdPersonCameraParams cameraModifiersUpPeekLeft;
        public ThirdPersonCameraParams cameraModifiersUpPeekRight;
    }

    [System.Serializable]
    public class CoverStyle
    {
        public CoverProps coverProps;
    }
}