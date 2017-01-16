using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to mix <see cref="AIGoal"/>'s and create an <see cref="AIGoal"/> set
/// </summary>
public class AIGoalSet : ScriptableObject
{
    public List<AIGoal> goalList;
}