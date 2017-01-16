using UnityEngine;

public class AIGoal : ScriptableTool
{
    /// <summary>
    /// Shows this goal's last used time, set by <see cref="Planner"/>.
    /// </summary>
    [System.NonSerialized]
    public float lastUsedAt = -Mathf.Infinity;

    /// <summary>
    /// Used by <see cref="Planner"/> to calculate an estimated valid plan
    /// </summary>
    [System.NonSerialized]
    public StateDictionary goalStates = new StateDictionary();

    /// <summary>
    /// This <see cref="AIGoal"/>'s current priority, you can set start priority using inspector (Priority will be clamped to <see cref="priorityRange"/> on <see cref="AIBrain"/> start)
    /// </summary>
    public float priority;

    /// <summary>
    /// You can use this serializable field to clamp <see cref="priority"/>
    /// </summary>
    public Vector2 priorityRange = new Vector2(0, 1);

    /// <summary>
    /// Set by <see cref="Planner"/> when this is applicapable
    /// </summary>
    [System.NonSerialized]
    public bool Applicapable;

    /// <summary>
    /// Called by <see cref="AIBrain"/> on start
    /// </summary>
    /// <param name="ai"></param>
    virtual public void OnStart(AIBrain ai)
    {
        priority = Mathf.Clamp(priority, priorityRange.x, priorityRange.y);
    }

    /// <summary>
    /// Called by both h<see cref="AIBrain"/> and <see cref="Planner"/> to check if this can be used as a <see cref="AIGoal"/> by <see cref="Planner"/>
    /// </summary>
    /// <param name="ai"></param>
    /// <returns></returns>
    public virtual bool IsApplicapable(AIBrain ai)
    {
        return true;
    }

    /// <summary>
    /// Called by <see cref="AIBrain"/> to check priority of goals
    /// </summary>
    /// <param name="ai"></param>
    public virtual void CalculatePriority(AIBrain ai) { }

    public override string ToString()
    {
        return string.Format("{0} | Priority = {1:0.00} | Range = {2:0}-{3:0}",
            this.GetType().ToString(), priority, priorityRange.x, priorityRange.y);
    }
}