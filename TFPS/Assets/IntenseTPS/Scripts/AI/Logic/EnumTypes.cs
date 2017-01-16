public class ET
{
    public enum TurnToType
    {
        ToCurrentTarget,
        ToPosition
    }

    public enum DamageType
    {
        BulletToBody,
        Melee,
        DirectToHealth,
        Explosion
    }

    public enum AiStatus
    {
        Safe,
        Dangerous,
        Idle,
        Unknown
    }

    public enum AiAlertLevel
    {
        Relaxed,
        Alerted,
        Aware
    }

    public enum LookAtType
    {
        ToCurrentTarget,
        ToPosition,
        Forward
    }

    [System.Serializable]
    public enum PatrolType
    {
        Sequenced,
        Random
    }

    public enum ActionType
    {
        Once,
        Repetitive,
    }

    public enum StateType
    {
        Idle,
        Move,
        Animate
    }

    public enum MoveType
    {
        Idle,
        Walk,
        Run,
        Sprint
    }

    public enum MoveToType
    {
        ToCurrentTarget,
        ToPosition
    }
}