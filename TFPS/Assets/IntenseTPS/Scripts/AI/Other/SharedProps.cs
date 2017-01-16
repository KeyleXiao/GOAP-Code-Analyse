using UnityEngine;

/// <summary>
/// Used to communicate between agents's <see cref="AIMemory"/>'s that has different/same <see cref="MonoBehaviour"/>'s
/// </summary>
public class SharedProps : MonoBehaviour
{
    public AIMemory memory { get; set; }
}