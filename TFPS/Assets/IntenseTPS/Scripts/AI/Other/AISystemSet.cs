using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to mix <see cref="AISystem"/>'s and create an <see cref="AISystem"/> set
/// </summary>
public class AISystemSet : ScriptableObject
{
    public List<AISystem> systemList;
}