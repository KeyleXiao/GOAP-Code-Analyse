using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GunPart))]
public class GPBarrel : MonoBehaviour
{
    public float bulletPowerIncrease;
    public float bulletForceToRigidbodyIncrease;
    public float damageIncrease;
    public float spreadDecrease;
    public float exitForceIncrease;
    public GameObject muzzleFlashPrefab;
    public Transform childMuzzleFlashPosRot;
    public bool overrideFireSounds = false;
    public List<GameObject> fireSounds;
}