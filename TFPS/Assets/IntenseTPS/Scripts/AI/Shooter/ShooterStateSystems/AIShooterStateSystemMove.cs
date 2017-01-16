using StateSystems;
using UnityEngine;

namespace Shooter.StateSystems
{
    /// <summary>
    /// Used to move to and/or turn to a <see cref="NavMesh"/> position with root motion
    /// </summary>
    public class AIShooterStateSystemMove : AIStateSystemMove
    {
        private bool shouldTurnToPosition = false;
        private Vector3 turnToPosition;
        private ET.TurnToType turnToType;
        private Vector3 moveToPosition;
        private ET.MoveType moveType;
        private ET.MoveToType moveToType;
        private bool usePath = false;
        private NavMeshPath path;

        private Vector2 smoothDeltaPosition;
        private Vector2 velocity;
        private bool crouching;
        private bool moving = false;

        private ShooterMoveProps moveProps;

        public AIShooterStateSystemMove(ShooterMoveProps _moveProps, PatrolRoute patrolRoute) : base(patrolRoute)
        {
            moveProps = _moveProps;
        }

        public override void OnStart(AIBrain ai)
        {
            // Root motion movement
            ai.Agent.updatePosition = false;
            StopTurning(ai);
        }

        #region Turning Functs

        public override bool ReachedDestination(AIBrain ai, float nearTolerance = 0)
        {
            if (ai.Agent.remainingDistance < ai.Agent.radius + nearTolerance && !ai.Agent.pathPending)
                return true;
            return false;
        }

        public override void SetTurnToPos(AIBrain ai, ET.TurnToType _turnToType, Vector3 pos)
        {
            turnToPosition = pos;
            turnToType = _turnToType;
        }

        public override void SetTurnToPos(AIBrain ai, ET.TurnToType _turnToType)
        {
            turnToType = _turnToType;
        }

        public override void StartTurning(AIBrain ai)
        {
            shouldTurnToPosition = true;
        }

        public override void StopTurning(AIBrain ai)
        {
            shouldTurnToPosition = false;
        }

        public override void SetTurnToPosNStartTurn(AIBrain ai, ET.TurnToType _turnToType, Vector3 pos)
        {
            SetTurnToPos(ai, _turnToType, pos);
            StartTurning(ai);
        }

        public override void SetTurnToPosNStartTurn(AIBrain ai, ET.TurnToType _turnToType)
        {
            SetTurnToPos(ai, _turnToType);
            StartTurning(ai);
        }

        #endregion Turning Functs

        #region Crouch-Stand

        public override void Crouch(AIBrain ai)
        {
            crouching = true;
        }

        public override void Stand(AIBrain ai)
        {
            crouching = false;
        }

        #endregion Crouch-Stand

        #region Move Functs

        public override void SetMoveToPosition(AIBrain ai, ET.MoveType _moveType, ET.MoveToType _moveToType, bool _usePath = false, NavMeshPath _path = null)
        {
            moveType = _moveType;
            moveToType = _moveToType;
            if (_usePath)
            {
                usePath = _usePath;
                path = _path;
            }
        }

        public override void SetMoveToPosition(AIBrain ai, ET.MoveType moveType, ET.MoveToType moveToType, Vector3 _movePosition, bool usePath = false, NavMeshPath path = null)
        {
            SetMoveToPosition(ai, moveType, moveToType, usePath, path);
            moveToPosition = _movePosition;
        }

        public override void SetMoveToPositionNStartMove(AIBrain ai, ET.MoveType moveType, ET.MoveToType moveToType, bool usePath = false, NavMeshPath path = null)
        {
            SetMoveToPosition(ai, moveType, moveToType, usePath, path);
            StartMoving(ai);
        }

        public override void SetMoveToPositionNStartMove(AIBrain ai, ET.MoveType moveType, ET.MoveToType moveToType, Vector3 movePosition, bool usePath = false, NavMeshPath path = null)
        {
            SetMoveToPosition(ai, moveType, moveToType, movePosition, usePath, path);
            StartMoving(ai);
        }

        public override void StartMoving(AIBrain ai)
        {
            moving = true;
        }

        public override void StopMoving(AIBrain ai)
        {
            moving = false;
        }

        #endregion Move Functs

        #region State Functs

        public override void OnActionActivate(AIBrain ai, ET.StateType stateType)
        {
            switch (stateType)
            {
                case ET.StateType.Idle:
                    break;

                case ET.StateType.Move:
                    if (usePath)
                        ai.Agent.SetPath(path);
                    else
                        ai.Agent.destination = moveToPosition;
                    break;

                case ET.StateType.Animate:
                    break;

                default:
                    break;
            }
        }

        public override void OnUpdate(AIBrain ai, ET.StateType stateType)
        {
            switch (stateType)
            {
                case ET.StateType.Idle:
                    ai.Agent.Stop();
                    DontMoveWithAgent(ai);
                    break;

                case ET.StateType.Move:
                    ai.Agent.Resume();
                    if (moving) Move(ai); else DontMoveWithAgent(ai);
                    break;

                case ET.StateType.Animate:
                    ai.Agent.Stop();
                    DontMoveWithAgent(ai);
                    break;

                default:
                    break;
            }
        }

        public override void OnActionExit(AIBrain ai, ET.StateType stateType)
        {
            usePath = false;
        }

        public override void OnAnimatorMove(AIBrain ai, ET.StateType stateType)
        {
            Vector3 position = ai.Animator.rootPosition;
            position.y = ai.Agent.nextPosition.y;
            ai.Transform.position = position;
        }

        #endregion State Functs

        public void DontMoveWithAgent(AIBrain ai)
        {
            NavMeshAgent agent = ai.Agent;
            if (agent)
            {
                Transform transform = ai.Transform;
                Animator animator = ai.Animator;

                agent.destination = ai.Transform.position;

                bool turnedMovement = false;
                if (shouldTurnToPosition)
                {
                    agent.updateRotation = false; turnedMovement = true;

                    switch (turnToType)
                    {
                        case ET.TurnToType.ToCurrentTarget:
                            turnToPosition = ai.GetCurrentTargetPos();
                            break;

                        case ET.TurnToType.ToPosition:
                            // already set in function
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    agent.updateRotation = true;
                    turnedMovement = false;
                }

                #region Unity Manual Code

                Vector3 worldDeltaPosition = agent.nextPosition - transform.position;

                // Map 'worldDeltaPosition' to local space
                float dx = Vector3.Dot(transform.right, worldDeltaPosition);
                float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
                Vector2 deltaPosition = new Vector2(dx, dy);

                // Low-pass filter the deltaMove
                float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
                smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

                // Update velocity if delta time is safe
                if (Time.deltaTime > 1e-5f)
                    velocity = smoothDeltaPosition / Time.deltaTime;

                bool shouldMove = velocity.magnitude > 0.1f && agent.remainingDistance > agent.radius;

                // Move agent to transform
                if (worldDeltaPosition.magnitude > agent.radius)
                    agent.nextPosition = transform.position + 0f * worldDeltaPosition;

                // Set transform's y to agent
                transform.position = new Vector3(transform.position.x, agent.nextPosition.y, transform.position.z);

                #endregion Unity Manual Code

                Vector3 desiredDir = (-transform.position + new Vector3(agent.nextPosition.x, transform.position.y, agent.nextPosition.z)).normalized * 2;

                float angle = 0;
                if (turnedMovement)
                {
                    angle = Vector3.Angle(transform.forward, (turnToPosition - transform.position).normalized);
                    angle = angle * Mathf.Abs(Vector3.Dot(transform.right, (turnToPosition - transform.position).normalized));
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(-transform.position + new Vector3(turnToPosition.x, transform.position.y, turnToPosition.z)), Time.deltaTime * moveProps.idleTurnToSmooth);
                }

                float speedAnim = Vector2.SqrMagnitude(new Vector2(ai.Animator.GetFloat("VelX"), ai.Animator.GetFloat("VelY")));
                if (turnedMovement)
                {
                    if (speedAnim < moveProps.legsStopTurnAtSqrM && Mathf.Abs(angle) > moveProps.legsStartTurnAtAngle)
                    {
                        ai.Animator.SetFloat("LegsAngle", angle * moveProps.legsTurnAngleMult, moveProps.legsTurnAngleDamp, Time.deltaTime);
                        ai.Animator.SetBool("LegTurn", true);
                        ai.GetStateSystem<AIStateSystemAnimator>().EnableLayer(ai, 3, true, true);
                    }
                    else
                    {
                        ai.Animator.SetFloat("LegsAngle", 0, moveProps.legsTurnAngleDamp, Time.deltaTime);
                        ai.Animator.SetBool("LegTurn", false);
                        ai.GetStateSystem<AIStateSystemAnimator>().DisableLayer(ai, 3, true, true);
                    }
                }
                else
                {
                    ai.Animator.SetFloat("LegsAngle", 0, moveProps.legsTurnAngleDamp, Time.deltaTime);
                    ai.Animator.SetBool("LegTurn", false);
                    ai.GetStateSystem<AIStateSystemAnimator>().DisableLayer(ai, 3, true, true);
                }

                Quaternion refShift = new Quaternion(transform.rotation.x, transform.rotation.y * -1f, transform.rotation.z, transform.rotation.w);
                Vector3 moveDirection = refShift * desiredDir;

                float locomotionDamp = moveProps.velocityAnimDamp;

                ET.MoveType moveType = ET.MoveType.Walk;

                float velocityLimit = moveProps.animatorWalkSpeed;

                switch (moveType)
                {
                    case ET.MoveType.Walk:
                        velocityLimit = moveProps.animatorWalkSpeed;
                        agent.speed = moveProps.agentWalkSpeed;
                        agent.angularSpeed = moveProps.agentAngularSpeedWalk;
                        break;

                    case ET.MoveType.Run:
                        velocityLimit = moveProps.animatorRunSpeed;
                        agent.speed = moveProps.agentRunSpeed;
                        agent.angularSpeed = moveProps.agentAngularSpeedRun;
                        break;

                    case ET.MoveType.Sprint:
                        velocityLimit = moveProps.animatorSprintSpeed;
                        agent.speed = moveProps.agentSprintSpeed;
                        agent.angularSpeed = moveProps.agentAngularSpeedSprint;
                        break;

                    default:
                        break;
                }
                float xVelocity = moveDirection.x, yVelocity = moveDirection.z;
                // Limit velocity
                if (xVelocity > 0)
                    xVelocity = xVelocity > velocityLimit ? velocityLimit : xVelocity;
                else if (xVelocity < 0)
                    xVelocity = -xVelocity > velocityLimit ? -velocityLimit : xVelocity;
                if (yVelocity > 0)
                    yVelocity = yVelocity > velocityLimit ? velocityLimit : yVelocity;
                else if (yVelocity < 0)
                    yVelocity = -yVelocity > velocityLimit ? -velocityLimit : yVelocity;

                if (!shouldMove)
                {
                    xVelocity = 0;
                    yVelocity = 0;
                }

                animator.SetFloat("VelX", xVelocity, locomotionDamp, Time.deltaTime);
                animator.SetFloat("VelY", yVelocity, locomotionDamp, Time.deltaTime);
                animator.SetFloat("CrouchStand", Mathf.Clamp01(Mathf.Lerp(animator.GetFloat("CrouchStand"), crouching ? 0 : 1, Time.deltaTime * moveProps.crouchStandSmooth)));
            }
        }

        public void Move(AIBrain ai)
        {
            NavMeshAgent agent = ai.Agent;
            if (agent)
            {
                Transform transform = ai.Transform;
                Animator animator = ai.Animator;
                if (!usePath)
                {
                    switch (moveToType)
                    {
                        case ET.MoveToType.ToCurrentTarget:
                            agent.destination = ai.GetCurrentTargetPos();
                            break;

                        case ET.MoveToType.ToPosition:
                            agent.destination = moveToPosition;
                            break;

                        default:
                            break;
                    }
                }
                bool turnedMovement = false;
                if (shouldTurnToPosition)
                {
                    agent.updateRotation = false; turnedMovement = true;

                    switch (turnToType)
                    {
                        case ET.TurnToType.ToCurrentTarget:
                            turnToPosition = ai.GetCurrentTargetPos();
                            break;

                        case ET.TurnToType.ToPosition:
                            // already set in function
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    agent.updateRotation = true;
                    turnedMovement = false;
                }

                #region Unity Manual Code

                Vector3 worldDeltaPosition = agent.nextPosition - transform.position;

                // Map 'worldDeltaPosition' to local space
                float dx = Vector3.Dot(transform.right, worldDeltaPosition);
                float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
                Vector2 deltaPosition = new Vector2(dx, dy);

                // Low-pass filter the deltaMove
                float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
                smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

                // Update velocity if delta time is safe
                if (Time.deltaTime > 1e-5f)
                    velocity = smoothDeltaPosition / Time.deltaTime;

                bool shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius;

                // Pull agent towards character[Modded pull multiplier]
                if (worldDeltaPosition.magnitude > agent.radius)
                    agent.nextPosition = transform.position + .01f * worldDeltaPosition;

                // Set transform's y to agent
                transform.position = new Vector3(transform.position.x, agent.nextPosition.y, transform.position.z);

                #endregion Unity Manual Code

                Vector3 desiredDir = (-transform.position + new Vector3(agent.nextPosition.x, transform.position.y, agent.nextPosition.z)).normalized * 2;

                float angle = 0;
                if (turnedMovement)
                {
                    angle = Vector3.Angle(transform.forward, (turnToPosition - transform.position).normalized);
                    angle = angle * Mathf.Abs(Vector3.Dot(transform.right, (turnToPosition - transform.position).normalized));
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(-transform.position + new Vector3(turnToPosition.x, transform.position.y, turnToPosition.z)), Time.deltaTime * moveProps.idleTurnToSmooth);
                }

                float speedAnim = Vector2.SqrMagnitude(new Vector2(ai.Animator.GetFloat("VelX"), ai.Animator.GetFloat("VelY")));
                if (turnedMovement)
                {
                    if (speedAnim < moveProps.legsStopTurnAtSqrM && Mathf.Abs(angle) > moveProps.legsStartTurnAtAngle)
                    {
                        ai.Animator.SetFloat("LegsAngle", angle * moveProps.legsTurnAngleMult, moveProps.legsTurnAngleDamp, Time.deltaTime);
                        ai.Animator.SetBool("LegTurn", true);
                        ai.GetStateSystem<AIStateSystemAnimator>().EnableLayer(ai, 3, true, true);
                    }
                    else
                    {
                        ai.Animator.SetFloat("LegsAngle", 0, moveProps.legsTurnAngleDamp, Time.deltaTime);
                        ai.Animator.SetBool("LegTurn", false);
                        ai.GetStateSystem<AIStateSystemAnimator>().DisableLayer(ai, 3, true, true);
                    }
                }
                else
                {
                    ai.Animator.SetFloat("LegsAngle", 0, moveProps.legsTurnAngleDamp, Time.deltaTime);
                    ai.Animator.SetBool("LegTurn", false);
                    ai.GetStateSystem<AIStateSystemAnimator>().DisableLayer(ai, 3, true, true);
                }

                Quaternion refShift = new Quaternion(transform.rotation.x, transform.rotation.y * -1f, transform.rotation.z, transform.rotation.w);
                Vector3 moveDirection = refShift * desiredDir;

                float locomotionDamp = moveProps.velocityAnimDamp;
                float velocityLimit = moveProps.animatorWalkSpeed;

                switch (moveType)
                {
                    case ET.MoveType.Walk:
                        velocityLimit = moveProps.animatorWalkSpeed;
                        agent.speed = moveProps.agentWalkSpeed;
                        agent.angularSpeed = moveProps.agentAngularSpeedWalk;
                        break;

                    case ET.MoveType.Run:
                        velocityLimit = moveProps.animatorRunSpeed;
                        agent.speed = moveProps.agentRunSpeed;
                        agent.angularSpeed = moveProps.agentAngularSpeedRun;
                        break;

                    case ET.MoveType.Sprint:
                        velocityLimit = moveProps.animatorSprintSpeed;
                        agent.speed = moveProps.agentSprintSpeed;
                        agent.angularSpeed = moveProps.agentAngularSpeedSprint;
                        break;

                    default:
                        break;
                }
                float xVelocity = moveDirection.x, yVelocity = moveDirection.z;
                // Limit velocity
                if (xVelocity > 0)
                    xVelocity = xVelocity > velocityLimit ? velocityLimit : xVelocity;
                else if (xVelocity < 0)
                    xVelocity = -xVelocity > velocityLimit ? -velocityLimit : xVelocity;
                if (yVelocity > 0)
                    yVelocity = yVelocity > velocityLimit ? velocityLimit : yVelocity;
                else if (yVelocity < 0)
                    yVelocity = -yVelocity > velocityLimit ? -velocityLimit : yVelocity;
                if (!shouldMove)
                {
                    xVelocity = 0;
                    yVelocity = 0;
                }

                animator.SetFloat("VelX", xVelocity, locomotionDamp, Time.deltaTime);
                animator.SetFloat("VelY", yVelocity, locomotionDamp, Time.deltaTime);
                animator.SetFloat("CrouchStand", Mathf.Clamp01(Mathf.Lerp(animator.GetFloat("CrouchStand"), crouching ? 0 : 1, Time.deltaTime * moveProps.crouchStandSmooth)));
            }
        }
    }

    [System.Serializable]
    public class ShooterMoveProps
    {
        public float velocityAnimDamp = .25f;
        public float idleTurnToSmooth = 1.4f;
        public float crouchStandSmooth = 5f;

        [UnityEngine.Space]
        [Range(0, 1)]
        public float animatorWalkSpeed = .4f;

        public float agentWalkSpeed = 3.15f;
        public float agentAngularSpeedWalk = 70;

        [UnityEngine.Space]
        [Range(0, 1)]
        public float animatorRunSpeed = .8f;

        public float agentRunSpeed = 5.5f;
        public float agentAngularSpeedRun = 120f;

        [UnityEngine.Space]
        [Range(0, 1)]
        public float animatorSprintSpeed = 1f;

        public float agentSprintSpeed = 7;
        public float agentAngularSpeedSprint = 120;

        [UnityEngine.Space]
        public float legsStopTurnAtSqrM = .1f;

        public float legsStartTurnAtAngle = 5f;

        public float legsTurnAngleMult = 1.5f;
        public float legsTurnAngleDamp = 0.1f;
    }
}