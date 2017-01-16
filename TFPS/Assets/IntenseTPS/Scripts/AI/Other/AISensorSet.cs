using Sensors;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to mix <see cref="AISensor"/>'s and create an <see cref="AISensor"/> set
/// </summary>
public class AISensorSet : ScriptableObject
{
    public List<AISensor> sensorList;
}