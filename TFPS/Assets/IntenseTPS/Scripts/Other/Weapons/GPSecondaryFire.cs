using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GunPart))]
public class GPSecondaryFire : MonoBehaviour
{
    public float damage;
    public ProjectileBase currentProjectilePrefab;
    public float firableBulletCaliber;
    public float reloadAnimParam;
    public float spreadAmount;
    public float fireSpeed;
    public float bulletPower;
    public float bulletForceToRigidbodys;
    public bool isShotGun;
    public int shrapnelCount;
    public float maxshrapnelSpread;
    public float maxBulletDistance;
    public float bodySpread;
    public GameObject muzzleFlashPrefab;
    public Transform muzzleFlashPosRot;
    public GameObject bulletTrailPrefab;
    public Transform bulletTrailExitPosRot;
    public float bulletTrailExitForce;
    public Transform spawn;
    public List<GameObject> fireSounds;
    public GameObject reloadSound;
    public bool isFiringProjectile;
    public GameObject projectilePrefab;
    public int currentClipCapacity;
    public int maxClipCapacity;
    public Animator animator;
    public Transform emptyShellPosition;
    public GameObject EmptyShellPrefab;
    public Vector3 emptyShellMinForce;
    public Vector3 emptyShellMaxForce;
    public float bulletSpeed;
}