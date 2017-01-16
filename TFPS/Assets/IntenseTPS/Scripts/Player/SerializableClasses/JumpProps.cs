using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Player
{
    [System.Serializable]
    public class JumpProps
    {
        public float idleJumpUpForce = 75;
        public float runJumpUpForce = 65;
        public bool useTorqueRotation = false;
        public float airRotationSpeed = 10;
        public float airControlForce = 500;
        public float airDownForce = 10f;
        public float airDownForceStartVelocity = -1;
        public float velocityParamSmooth = 1f;
        
    }
}
