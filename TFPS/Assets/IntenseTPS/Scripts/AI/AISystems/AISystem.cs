using System;
using UnityEngine;

public abstract class AISystem : ScriptableTool
{
    /// <summary>
    /// last update time of this system
    /// </summary>
    [NonSerialized]
    public float LastUpdateTime;

    /// <summary>
    /// delta time since last system update
    /// </summary>
    [NonSerialized]
    public float DeltaTime;

    [Tooltip("Update interval of this system, this should be low enough to catch world-memory changes")]
    public float updateInterval = .2f;

    /// <summary>
    /// Place to manage references
    /// </summary>
    virtual public void OnStart(AIBrain ai)
    {
    }

    /// <summary>
    /// Called to update this system with <see cref="updateInterval"/>
    /// </summary>
    public abstract void OnUpdate(ref bool needToReplan, ref bool needToReevaluateGoals, AIBrain ai);
}