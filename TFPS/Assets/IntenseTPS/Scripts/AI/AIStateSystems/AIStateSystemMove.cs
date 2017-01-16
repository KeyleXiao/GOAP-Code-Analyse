using System.Collections.Generic;
using UnityEngine;

namespace StateSystems
{
    /// <summary>
    /// Class is used like an interface by <see cref="Shooter.StateSystems.AIShooterStateSystemMove"/>
    /// You can inherite from this class or <see cref="Shooter.StateSystems.AIShooterStateSystemMove"/> if necessary to use with different <see cref="MonoBehaviour"/>'s-Agent's
    /// </summary>
    public class AIStateSystemMove : AIStateSystem
    {
        public AIStateSystemMove(PatrolRoute patrolRoute)
        {
            PatrolRoute = patrolRoute;
            if (patrolRoute)
                PatrolPoints = patrolRoute.patrolPoints;
        }

        public PatrolRoute PatrolRoute { get; protected set; }
        public List<Vector3> PatrolPoints { get; protected set; }

        virtual public void SetTurnToPos(AIBrain ai, ET.TurnToType turnType, Vector3 pos)
        {
        }

        virtual public void SetTurnToPos(AIBrain ai, ET.TurnToType turnType)
        {
        }

        virtual public void StartTurning(AIBrain ai)
        {
        }

        virtual public void StopTurning(AIBrain ai)
        {
        }

        virtual public void SetTurnToPosNStartTurn(AIBrain ai, ET.TurnToType turnType, Vector3 pos)
        {
        }

        virtual public void SetTurnToPosNStartTurn(AIBrain ai, ET.TurnToType turnType)
        {
        }

        virtual public void SetMoveToPosition(AIBrain ai, ET.MoveType moveType, ET.MoveToType moveToType, Vector3 movePosition, bool usePath = false, NavMeshPath path = null)
        {
        }

        virtual public void SetMoveToPosition(AIBrain ai, ET.MoveType moveType, ET.MoveToType moveToType, bool usePath = false, NavMeshPath path = null)
        {
        }

        virtual public void SetMoveToPositionNStartMove(AIBrain ai, ET.MoveType moveType, ET.MoveToType moveToType, Vector3 movePosition, bool usePath = false, NavMeshPath path = null)
        {
        }

        virtual public void SetMoveToPositionNStartMove(AIBrain ai, ET.MoveType moveType, ET.MoveToType moveToType, bool usePath = false, NavMeshPath path = null)
        {
        }

        virtual public void StartMoving(AIBrain ai)
        {
        }

        virtual public void StopMoving(AIBrain ai)
        {
        }

        virtual public void Crouch(AIBrain ai)
        {
        }

        virtual public void Stand(AIBrain ai)
        {
        }

        virtual public bool ReachedDestination(AIBrain ai, float nearTolerance = 0)
        {
            return true;
        }
    }
}