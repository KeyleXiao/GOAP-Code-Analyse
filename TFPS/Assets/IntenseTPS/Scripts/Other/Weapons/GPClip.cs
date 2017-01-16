using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GunPart))]
public class GPClip : MonoBehaviour
{
    public int clipCapacity;
    public List<WeaponFixersSingleType> leftHandClipFixers;
}