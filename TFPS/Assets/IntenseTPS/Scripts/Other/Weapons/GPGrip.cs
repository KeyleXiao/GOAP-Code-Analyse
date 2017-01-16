using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GunPart))]
public class GPGrip : MonoBehaviour
{
    public List<WeaponFixersSingleType> leftHandleFixers;
    public int overrideLeftHandAnimNoOnIdle = -1;
    public int overrideLeftHandAnimNoOnAim = -1;
    public float spreadDecrease;
}