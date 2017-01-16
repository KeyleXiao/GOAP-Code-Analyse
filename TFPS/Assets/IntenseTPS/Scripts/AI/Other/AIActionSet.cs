using Actions;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to mix <see cref="AIAction"/>'s and create an <see cref="AIAction"/> set
/// </summary>
public class AIActionSet : ScriptableObject
{
    public List<AIAction> actionList;
}