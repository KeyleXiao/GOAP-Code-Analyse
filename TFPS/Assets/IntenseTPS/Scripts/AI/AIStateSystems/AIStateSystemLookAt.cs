using UnityEngine;

namespace StateSystems
{
    /// <summary>
    /// Class is used like an interface by <see cref="Shooter.StateSystems.AIShooterStateSystemLookAt"/>
    /// You can inherite from this class or <see cref="Shooter.StateSystems.AIShooterStateSystemLookAt"/> if necessary to use with different <see cref="MonoBehaviour"/>'s-Agent's
    /// </summary>
    public class AIStateSystemLookAt : AIStateSystem
    {
        // IK Functions
        virtual public void SetLookAtPosition(AIBrain ai, Vector3 position, ET.LookAtType lookAtType) { }

        virtual public void SetLookAtPosNStartLook(AIBrain ai, Vector3 position, ET.LookAtType lookAtType)
        {
        }

        virtual public void SetLookAtPosition(AIBrain ai, ET.LookAtType lookAtType)
        {
        }

        virtual public void SetLookAtPosNStartLook(AIBrain ai, ET.LookAtType lookAtType)
        {
        }

        virtual public void StartLooking(AIBrain ai)
        {
        }

        virtual public void StopLooking(AIBrain ai)
        {
        }

        virtual public float GetHeadIKWeight()
        {
            return 0;
        }
    }
}