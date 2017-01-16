using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [System.Serializable]
    public class LocomotionProps
    {
        public float animVelSmoothDamp = .15f;
        [Space]
        public bool useUpliftingTurnSpeed = true;
        public float upliftTurnUpSpeed = 1.8f;
        public float upliftTurnDownUpSpeed = 4f;
        public float upliftingTurnStartAngle = 3f;
        public float rotationTurnSpeedWalk = 3.5f; // Rotation Speed
        public float rotationTurnSpeedRun = 6f; // Rotation Speed
        public float rotationTurnSpeedSprint = 7f; // Rotation Speed
        public bool useTorqueRotation = false;
        [Space]
        public float forceMaxRotationOnTurnTo = 50;
        public float forceRotationSpeed = 7f;
        [Space]
        public float walkVelocityLimit = .5f;
        public float runVelocityLimit = .8f;
        public float sprintVelocityLimit = 1f;
        [Space]
        public float allowIdleToWalkAtSpeed = .15f;
        public float allowWalkToRunAtSpeed = .3f;
        public float allowRunToSprintAtSpeed = .75f;
        [Space]
        public float walkTurnAngleStart = 170;
        public float runTurnAngleStart = 130;
        public float sprintTurnAngleStart = 130;
        [Space]
        public float walkTurnAngleThreshold = .3f;
        public float runTurnAngleThreshold = .6f;
        public float sprintTurnAngleThreshold = 1f;
        [Space]
        public float minTurnAngleStartAbs = 45f;
        [Space]
        public float agentDesiredDirSmooth = 3f;
        public float agentWalkSpeed = 3.15f;
        public float agentAngularSpeedWalk = 120f;
        public float agentRunSpeed = 5.5f;
        public float agentAngularSpeedRun = 120f;
        public float agentSprintSpeed = 7f;
        public float agentAngularSpeedSprint = 120f;
        [Space]
        public float idleAgentTurnToSpeed = 1.5f;
        public float walkAgentTurnToSpeed = 2f;
        public float runAgentTurnToSpeed = 2f;
        public float sprintAgentTurnToSpeed = 2f;
    }

    [System.Serializable]
    public class LocomotionStyle
    {
#if UNITY_EDITOR
        public string name = "Normal";
#endif
        public LocomotionProps locomotionProps;
        public int animLocomStyleParam = 0;
        public List<int> bindedWeaponStyles;
        public bool crouchButtonEnabled = true;
        public bool isCrouchingStyle = false;
        public int switchToStyleIndexOnCrouchBtn = -1;
        public static float layerEnableSpeed = 5f;
        public static float layerDisableSpeed = 5f;
    }
}