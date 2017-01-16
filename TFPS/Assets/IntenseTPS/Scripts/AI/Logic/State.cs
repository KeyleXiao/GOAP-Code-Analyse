using StateSystems;/// <summary>
/// Used to execute <see cref="AIStateSystem"/>'s
/// </summary>
public abstract class State{
    protected AIStateSystem[] sSystems;
    public ET.StateType StateType { get; protected set; }    public State(AIStateSystem[] _stateSystems)
    {
        sSystems = _stateSystems;
    }

    public virtual void EnterState(AIBrain ai)
    {
        foreach (var sSystem in sSystems)
            sSystem.OnActionActivate(ai, StateType);
    }
    public virtual void Update(AIBrain ai)
    {
        foreach (var sSystem in sSystems)
            sSystem.OnUpdate(ai, StateType);
    }
    public virtual void ExitState(AIBrain ai)
    {
        foreach (var sSystem in sSystems)
            sSystem.OnActionExit(ai, StateType);
    }    public virtual void OnAnimatorIK(int layerNo, AIBrain ai)
    {
        foreach (var sSystem in sSystems)
            sSystem.OnAnimatorIK(ai, layerNo, StateType);
    }
    public virtual void OnAnimatorMove(AIBrain ai)
    {
        foreach (var sSystem in sSystems)
            sSystem.OnAnimatorMove(ai, StateType);
    }}