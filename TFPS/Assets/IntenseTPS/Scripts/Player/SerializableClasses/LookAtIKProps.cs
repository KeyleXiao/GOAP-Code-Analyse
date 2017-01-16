using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [System.Serializable]
    public class LookAtIKProps
    {
        public float headIKSmooth = 3f;
        public float headIKBackSmooth = 3f;

        [Space]
        [Range(0, 180)]
        public float maxLookTAngleHorizontal = 75;

        [Range(0, 90)]
        public float maxLookTAngleVertical = 90;

        [Space]
        public float angleLerpSpeed = 4f;

        public float angleLerpBackSpeed = 4.5f;

        [Space]
        public float slawBodyWeight = 1;

        public float slawHeadWeight = 1;
        public float slawClamp = .5f;
    }

    [System.Serializable]
    public class LookAtIKStyle
    {
#if UNITY_EDITOR
        public string name = "Normal";
#endif
        public LookAtIKProps lookAtProps;
        public List<int> bindedWeaponStyles;
    }
}