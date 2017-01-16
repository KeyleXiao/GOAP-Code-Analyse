using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [System.Serializable]
    public class FireProps
    {
        [System.NonSerialized]
        public float rHandAimRot, rHandAim, lHandAim;

        public LayerMask rayCBulletLayerMask;

        public float rightHandSmooth = 3f;
        public float rightHandBackSmooth = 4f;
        public float leftHandSmooth = 4f;
        public float leftHandBackSmooth = 4.5f;

        public Vector2 immutedWeaponSpreadAgentMultiplier = Vector2.one;

        public float immutedSpreadChangeSpeed = 2f;
        public Vector3 visualHandSpreadAgentMultipliers = new Vector3(2, 2, 2);
        public float weaponSpreadRecoverAgentMultiplier = 1f;
        public Vector3 aimPositionFixer = new Vector3(.02f, .06f, -.12f);
        public Vector3 aimRotationFixer = new Vector3(2.38f, 22f, 0f);

        [Space]
        public float rightArmLayerEnableSpeed = 2f;

        public float rightArmLayerDisableSpeed = 2f;
        public float leftArmLayerEnableSpeed = 2f;
        public float leftArmLayerDisableSpeed = 2f;

        [Space]
        [Header("Third Person Camera Modifiers")]
        public ThirdPersonCameraParams camModifiersHipFireAim;

        public ThirdPersonCameraParams camModifiersSightAim;

        [Space]
        public float lookIKRightMultiplier = 0f;

        public float lookIKUpMultiplier = 0f;

        [Space]
        public float bodyBobAgentMultiplier = 1;

        public float bodyRecoverSpeedAgentMultiplier = 1;

        [Space]
        public FocusCameraSerializedFields focusCamModifierWeaponModify;

        [Space]
        public List<GameObject> collectAmmoSoundPrefabs;
    }

    [System.Serializable]
    public class WeaponFireStyle
    {
#if UNITY_EDITOR
        public string name = "Normal";
#endif
        public List<int> bindedWeaponStyles;
        public FireProps fireProps;
    }
}