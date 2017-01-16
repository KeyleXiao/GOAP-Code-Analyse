using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sounds
{
    public List<GameObject> fireSounds;

    // Only player
    public List<GameObject> pullOuts;

    public List<GameObject> holsters;
    public List<GameObject> dryShots;
    public List<GameObject> reloads;
    public List<GameObject> aims;
    public List<GameObject> unAims;
}

[System.Serializable]
public class DefaultWeaponFixers
{
    public Transform animRightHand;
    public Transform aimSight;
    public Transform aimHipFire;
    public Transform aimCover;
    public Transform leftHandle;
    public Transform leftHandClip;
}

[System.Serializable]
public class WeaponFixersSingleType
{
    public string characterName;
    public Transform fixerTransform;

    public WeaponFixersSingleType(string _characterName, Transform _fixerTransform)
    {
        characterName = _characterName;
        fixerTransform = _fixerTransform;
    }

    public static Transform GetFixerForChar(List<WeaponFixersSingleType> list, string characterName)
    {
        foreach (var item in list)
        {
            if (item.characterName == characterName)
                return item.fixerTransform;
        }
        return null;
    }
}

public enum WeaponFixerTypes
{
    AnimRightHand, AimSight, AimHipFire, AimCover, LeftHandle, LeftHandClip
}

[System.Serializable]
public class CharacterWeaponFixer
{
    public string characterName;
    public DefaultWeaponFixers allFixHolders;
}

[System.Serializable]
public class WeaponPosRotFixer
{
    public Transform fixHolderTransform;
    public WeaponFixerTypes fixerType;
}

public class GunAtt : MonoBehaviour
{
    public Transform GetFixer(string characterName, WeaponFixerTypes fixerType)
    {
        foreach (var item0 in characterFixHolders)
        {
            if (/*item0.characterName == characterName*/ characterName.Contains(item0.characterName))
            {
                switch (fixerType)
                {
                    case WeaponFixerTypes.AnimRightHand:
                        return item0.allFixHolders.animRightHand;

                    case WeaponFixerTypes.AimSight:
                        return item0.allFixHolders.aimSight;

                    case WeaponFixerTypes.AimHipFire:
                        return item0.allFixHolders.aimHipFire;

                    case WeaponFixerTypes.AimCover:
                        return item0.allFixHolders.aimCover;

                    case WeaponFixerTypes.LeftHandle:
                        return item0.allFixHolders.leftHandle;

                    case WeaponFixerTypes.LeftHandClip:
                        return item0.allFixHolders.leftHandClip;
                }
            }
        }
        switch (fixerType)
        {
            case WeaponFixerTypes.AnimRightHand:
                return otherFixers.animRightHand;

            case WeaponFixerTypes.AimSight:
                return otherFixers.aimSight;

            case WeaponFixerTypes.AimHipFire:
                return otherFixers.aimHipFire;

            case WeaponFixerTypes.AimCover:
                return otherFixers.aimCover;

            case WeaponFixerTypes.LeftHandle:
                return otherFixers.leftHandle;

            case WeaponFixerTypes.LeftHandClip:
                return otherFixers.leftHandClip;

            default:
                return null;
        }
    }

    public List<WeaponFixersSingleType> GetFixersForType(WeaponFixerTypes fixerType)
    {
        List<WeaponFixersSingleType> fixersRetVal = new List<WeaponFixersSingleType>();
        foreach (var fixerForChar in characterFixHolders)
        {
            switch (fixerType)
            {
                case WeaponFixerTypes.AnimRightHand:
                    fixersRetVal.Add(new WeaponFixersSingleType(fixerForChar.characterName, fixerForChar.allFixHolders.animRightHand));
                    break;

                case WeaponFixerTypes.AimSight:
                    fixersRetVal.Add(new WeaponFixersSingleType(fixerForChar.characterName, fixerForChar.allFixHolders.aimSight));
                    break;

                case WeaponFixerTypes.AimHipFire:
                    fixersRetVal.Add(new WeaponFixersSingleType(fixerForChar.characterName, fixerForChar.allFixHolders.aimHipFire));
                    break;

                case WeaponFixerTypes.AimCover:
                    fixersRetVal.Add(new WeaponFixersSingleType(fixerForChar.characterName, fixerForChar.allFixHolders.aimCover));
                    break;

                case WeaponFixerTypes.LeftHandle:
                    fixersRetVal.Add(new WeaponFixersSingleType(fixerForChar.characterName, fixerForChar.allFixHolders.leftHandle));
                    break;

                case WeaponFixerTypes.LeftHandClip:
                    fixersRetVal.Add(new WeaponFixersSingleType(fixerForChar.characterName, fixerForChar.allFixHolders.leftHandClip));
                    break;

                default:
                    break;
            }
        }
        return fixersRetVal;
    }

    public void SetFixersForType(List<WeaponFixersSingleType> fixers, WeaponFixerTypes fixerType)
    {
        foreach (var item in characterFixHolders)
        {
            foreach (var item1 in fixers)
            {
                if (item.characterName == item1.characterName)
                {
                    switch (fixerType)
                    {
                        case WeaponFixerTypes.AnimRightHand:
                            item.allFixHolders.animRightHand = item1.fixerTransform;
                            break;

                        case WeaponFixerTypes.AimSight:
                            item.allFixHolders.aimSight = item1.fixerTransform;
                            break;

                        case WeaponFixerTypes.AimHipFire:
                            item.allFixHolders.aimHipFire = item1.fixerTransform;
                            break;

                        case WeaponFixerTypes.AimCover:
                            item.allFixHolders.aimCover = item1.fixerTransform;
                            break;

                        case WeaponFixerTypes.LeftHandle:
                            item.allFixHolders.leftHandle = item1.fixerTransform;
                            break;

                        case WeaponFixerTypes.LeftHandClip:
                            item.allFixHolders.leftHandClip = item1.fixerTransform;
                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }

    public List<CharacterWeaponFixer> characterFixHolders;
    public DefaultWeaponFixers otherFixers;

    [Space]
    // Seperated left right hands-arms (overriding) are only for player
    public bool overrideRightHand = false;

    public int rightHandAnimNo = 0;

    [Space]
    public bool overrideLeftHandOnIdle = false;

    public int leftHandAnimNoOnIdle = 0;
    public bool overrideLeftHandOnAim = false;
    public int leftHandAnimNoOnAim = 0;

    [Space]
    [Header("Pull Out weapon")]
    public bool enableLeftHandOnPullOut = true;

    [Header("Idle with weapon")]
    public bool enableRightHandOnIdle = true;

    public bool enableLeftHandOnIdle = true;

    [Header("Aim with weapon")]
    public bool enableLeftHandOnAimSight = true;

    public bool enableLeftHandOnHipFireAim = true;

    [Header("Reload weapon")]
    public bool enableLeftHandOnReload = true;

    [Header("Holster weapon")]
    public bool enableLeftHandOnHolster = true;

    public float firableProjectileCaliber;
    public ProjectileBase currentProjectilePrefab; // Only for player
    public Sprite hudSprite; // Only for player

    [Space]
    public float waitToStartFireOnHipFireAim = .5f;

    public float waitToStartFireOnSightAim = .5f;
    public float tapFireMinTimer = .2f;
    public Sprite crosshair2D; // Leave it empty to use default 2D Crosshair with this weapon
    public int crosshairCount = 4;

    [Space]
    public string weaponName = "";

    public float damage = 30;
    public float fireSpeed = 10;
    public float spreadAmount = 4;
    public Vector2 immutedSpreadMax = Vector2.one;
    public float spreadRecoverSpeed = .08f;
    public Vector3 spreadAxisMultipliers;
    public float bodySpread = 1f;
    public float bodyRecoverSpeedInverse = 55f;
    public float bodyFixRight = 5.38f;
    public float bodyFixUp = 5.38f;
    public int maxClipCapacity;
    public int currentClipCapacity;
    public int gunStyle;
    public float bulletSpeed = 600;
    public float maxBulletDistance = 200f;
    public float bulletPower = .1f;
    public float bulletForceToRigidbodys = 30f;
    public bool isShotGun = false;
    public int shrapnelCount = 6;
    public float maxshrapnelSpread = 3f;
    public bool isFiringProjectile = false;
    public Transform spawn;
    public GameObject projectilePrefab;
    public bool firesCurClipObject = false;
    public GameObject muzzleFlashPrefab;
    public Transform muzzleFlashPosRot;
    public Sounds sounds;
    public Animator animator;
    public GameObject EmptyShellPrefab;
    public Transform emptyShellPosition;
    public Vector3 emptyShellMinForce;
    public Vector3 emptyShellMaxForce;
    public GameObject bulletTrailPrefab;
    public Transform bulletTrailExitPosRot;
    public float bulletTrailExitForce = 450;
    public Transform curClipObject;
    public Transform curClipPrefab;

    [System.NonSerialized]
    public Vector3 clipDefLocalPos;

    [System.NonSerialized]
    public Quaternion clipDefLocalRot;

    private Decals decals;

    private void Awake()
    {
        decals = GameObject.FindGameObjectWithTag("EnvFx").GetComponent<Decals>();

        if (isFiringProjectile && firesCurClipObject)
        {
            currentClipCapacity = 1;
            maxClipCapacity = 1;
        }
        if (curClipObject)
        {
            clipDefLocalPos = curClipObject.localPosition;
            clipDefLocalRot = curClipObject.localRotation;

            DisableRbAndCol(curClipObject);
        }
    }

    public void DisableRbAndCol(Transform trn)
    {
        if (trn.GetComponent<Rigidbody>())
            trn.GetComponent<Rigidbody>().isKinematic = true;
        if (trn.GetComponent<Collider>())
            trn.GetComponent<Collider>().enabled = false;
    }

    public void PlayRandomSoundAsWeaponChild(List<GameObject> availableSounds, Transform owner)
    {
        if (availableSounds.Count > 0)
        {
            int randFireSound = Random.Range(0, availableSounds.Count);
            GameObject randSound = Instantiate(availableSounds[randFireSound], transform.position, transform.rotation) as GameObject;

            if (randSound)
            {
                randSound.transform.SetParent(transform);
                AudioEventMonoB audioEventMonoB = randSound.GetComponent<AudioEventMonoB>();
                if (audioEventMonoB)
                    audioEventMonoB.owner = owner;
            }
        }
    }

    public void Fire(
        Transform owner,
        Transform target,
        Vector3 firingToPos,
        ref float _weaponBodyBob,
        ref float _randomHandTwistSign,
        Quaternion xq,
        LayerMask bulletMask,
        float rayCastProjectileDamage = 0
        )
    {
        GunAtt gunAtt = this;
        currentClipCapacity -= 1;

        float xSpread = 0, ySpread = 0, bodySpread = 0;
        bodySpread = gunAtt.bodySpread;
        xSpread = (float)(Random.value - 0.5) * (gunAtt.spreadAmount);
        ySpread = (float)(Random.Range(.35f, 1f) - 0.5) * (gunAtt.spreadAmount);

        if (gunAtt.sounds.fireSounds.Count > 0)
        {
            int randFireSound = Random.Range(0, gunAtt.sounds.fireSounds.Count);
            GameObject fireSound = Instantiate(gunAtt.sounds.fireSounds[randFireSound], gunAtt.transform.position, gunAtt.transform.rotation) as GameObject;
            if (fireSound)
            {
                fireSound.transform.SetParent(gunAtt.transform);

                AudioEventMonoB audioEventMonoB = fireSound.GetComponent<AudioEventMonoB>();
                if (audioEventMonoB)
                    audioEventMonoB.owner = owner;
            }
        }

        if (animator)
        {
            animator.SetTrigger("Fire");
        }

        // empty shell
        if (gunAtt.emptyShellPosition && gunAtt.EmptyShellPrefab)
        {
            Vector3 randomEmptyShellForce = new Vector3(Random.Range(gunAtt.emptyShellMinForce.x, gunAtt.emptyShellMaxForce.x), Random.Range(gunAtt.emptyShellMinForce.y, gunAtt.emptyShellMaxForce.y), Random.Range(gunAtt.emptyShellMinForce.z, gunAtt.emptyShellMaxForce.z));
            GameObject goEmptyShell = Instantiate(gunAtt.EmptyShellPrefab, gunAtt.emptyShellPosition.position, gunAtt.emptyShellPosition.rotation) as GameObject;
            goEmptyShell.GetComponent<Rigidbody>().AddForce(gunAtt.emptyShellPosition.rotation * randomEmptyShellForce, ForceMode.Impulse);
            goEmptyShell.transform.SetParent(decals.transform, true);
        }

        // muzzle flash
        if (gunAtt.muzzleFlashPrefab)
        {
            GameObject goMuzzle = Instantiate(gunAtt.muzzleFlashPrefab, gunAtt.muzzleFlashPosRot.position, gunAtt.muzzleFlashPosRot.rotation) as GameObject;
            goMuzzle.transform.SetParent(gunAtt.transform, true);
        }

        #region Raycast Fire

        if (!gunAtt.isFiringProjectile)
        {
            if (!gunAtt.isShotGun)
            {
                Vector3 bulletDir = xq * (firingToPos - gunAtt.spawn.position).normalized;
                RaycastHit[] hits = Physics.RaycastAll(gunAtt.spawn.position, bulletDir, gunAtt.maxBulletDistance, bulletMask); // raycastAll - get hits

                SortArray(ref hits); // sort raycast hits by distance for calculations
                ManageHits(hits, gunAtt.bulletPower, bulletDir, gunAtt.bulletForceToRigidbodys, gunAtt.damage + rayCastProjectileDamage, gunAtt.bulletSpeed, gunAtt.bulletTrailExitForce, gunAtt.bulletTrailPrefab, gunAtt.bulletTrailExitPosRot, bulletDir, bulletMask);
            }
            else
            {
                for (int i = 0; i < shrapnelCount; i++)
                {
                    Vector3 bulletDir = xq * Quaternion.Euler(Random.Range(-maxshrapnelSpread, maxshrapnelSpread), Random.Range(-maxshrapnelSpread, maxshrapnelSpread), 0) * (firingToPos - gunAtt.spawn.position).normalized;
                    RaycastHit[] hits = Physics.RaycastAll(gunAtt.spawn.position, bulletDir, gunAtt.maxBulletDistance, bulletMask); // raycastAll - get hits

                    SortArray(ref hits); // sort raycast hits by distance
                    ManageHits(hits, gunAtt.bulletPower, bulletDir, gunAtt.bulletForceToRigidbodys, gunAtt.damage + rayCastProjectileDamage, gunAtt.bulletSpeed, gunAtt.bulletTrailExitForce, gunAtt.bulletTrailPrefab, gunAtt.bulletTrailExitPosRot, bulletDir, bulletMask);
                }
            }
        }

        #endregion Raycast Fire

        #region Projectile Fire

        else
        {
            if (firesCurClipObject && curClipObject.GetComponent<Projectile>())
            {
                curClipObject.SetParent(null);
                curClipObject.forward = xq * (firingToPos - gunAtt.spawn.position).normalized;

                curClipObject.GetComponent<Projectile>().EnableProjectile();
            }
            else
            {
                Instantiate(gunAtt.projectilePrefab, gunAtt.spawn.position, xq);
            }
        }

        #endregion Projectile Fire

        // Apply Spreads after Fire
        target.localPosition = new Vector3(xSpread, ySpread, 0) + target.localPosition;
        _weaponBodyBob = bodySpread;
        _randomHandTwistSign = Random.Range(-1, 1);
    }

    private void ManageHits(RaycastHit[] hits, float bulletPower, Vector3 bulletDirection, float forceToRigidBodys,
       float damage, float bulletSpeed, float bulletTrailExitForce /*if it has rigidbody*/,
       GameObject trailPrefab, Transform trailExitPosRot, Vector3 bulletDir, LayerMask bulletMask
       )
    {
        #region No Hit Fake Shot-return

        if (hits.Length == 0) // Fake shot when nothing is hit
        {
            if (trailExitPosRot && trailPrefab)
            {
                Vector3 exitPos = trailExitPosRot.position;
                GameObject trail;
                trail = Instantiate(trailPrefab, exitPos, Quaternion.LookRotation(
                    /*-plAtts.target.position + exitPos*/ /*trailExitPosRot.forward*/
                    bulletDir
                    )) as GameObject;
                trail.transform.SetParent(decals.transform, true);
                if (trailPrefab.GetComponent<Rigidbody>())
                    trail.GetComponent<Rigidbody>().velocity = -trail.transform.forward * bulletTrailExitForce;
            }
            return;
        }

        #endregion No Hit Fake Shot-return

        int hitIndex = 0;
        GameObject tempBulletTrail = null;
        float thisBulletPower = bulletPower = bulletPower <= 0 ? .01f : bulletPower; // make sure we hit a collider
        foreach (RaycastHit hit in hits)    // all raycast hits
        {
            GameObject decal = null, fx = null, hitSound = null; float thisPierceDecrease = 0;
            float timeToTravel = 0;

            Vector3 exitPos = transform.position;
            if (thisBulletPower <= 0)   // if bullet have no power to hit next collider - raycast back & exit foreach
            {
                #region Destroy trail & Backward raycast

                // Find the time needed to get to last hit point
                timeToTravel = Vector3.Distance(hits[hitIndex - 1].point, exitPos) / bulletSpeed;
                // Destroy bullet trail if bullet lost power on last collider hit
                StartCoroutine(DestroyCoroutine(timeToTravel, tempBulletTrail));

                // BW raycast
                if (hitIndex > 0)
                {
                    RaycastHit[] hitsBW = Physics.RaycastAll(hits[hitIndex - 1].point, -bulletDirection, Vector3.Distance(hits[0].point, hits[hitIndex - 1].point), bulletMask);
                    if (hitsBW.Length > 1)
                    {
                        SortArray(ref hitsBW);
                        foreach (RaycastHit hitBW in hitsBW)
                        {
                            decals.GetAllNormalShot(hitBW.transform.tag, ref decal, ref fx, ref hitSound, ref thisPierceDecrease);

                            // Instantiate them delayed
                            float timeToTravelBW = Vector3.Distance(hitBW.point, exitPos) / bulletSpeed;
                            if (decal != null)
                                StartCoroutine(InstantiateCoroutine(
                                    timeToTravelBW, decal, hitBW.point + (hitBW.normal * .04f), Quaternion.LookRotation(hitBW.normal) * Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360))), hitBW.transform));

                            if (fx != null)
                                StartCoroutine(InstantiateCoroutine(
                                    timeToTravelBW, fx, hitBW.point + (hitBW.normal * .07f), Quaternion.LookRotation(hitBW.normal) * Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360))), hitBW.transform, true));

                            if (hitSound != null)
                                StartCoroutine(InstantiateCoroutine(timeToTravelBW, hitSound, hitBW.point, Quaternion.identity, hitBW.transform, true));
                        }
                    }
                }

                #endregion Destroy trail & Backward raycast

                break;
            }

            #region Forward raycast

            decals.GetAllNormalShot(hit.transform.tag, ref decal, ref fx, ref hitSound, ref thisPierceDecrease);

            // Instantiate them delayed
            timeToTravel = Vector3.Distance(hit.point, exitPos) / bulletSpeed;
            if (decal != null)
                StartCoroutine(InstantiateCoroutine(
                    timeToTravel, decal, hit.point + (hit.normal * .04f), Quaternion.LookRotation(hit.normal) * Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360))), hit.transform, true));

            if (fx != null)
                StartCoroutine(InstantiateCoroutine(
                    timeToTravel, fx, hit.point + (hit.normal * .07f), Quaternion.LookRotation(hit.normal) * Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360))), hit.transform, true));

            if (hitSound != null)
                StartCoroutine(InstantiateCoroutine(timeToTravel, hitSound, hit.point, Quaternion.identity, hit.transform, true));

            // bullet trail at first hit
            if (trailExitPosRot && trailPrefab && hitIndex == 0)
            {
                Vector3 trailExitPos = trailExitPosRot.position;
                GameObject trail;
                trail = Instantiate(trailPrefab, trailExitPos, Quaternion.LookRotation(bulletDir)) as GameObject;

                if (trailPrefab.GetComponent<Rigidbody>())
                    trail.GetComponent<Rigidbody>().velocity = trail.transform.forward * bulletTrailExitForce;
                tempBulletTrail = trail;
                trail.transform.SetParent(decals.transform, true);
            }

            // add force to objects that bullet hit, if they have rigidbody
            StartCoroutine(AddBulletForceToRigidbodys(timeToTravel, hit, forceToRigidBodys * thisBulletPower / bulletPower, bulletDirection));

            // Apply damage
            StartCoroutine(ApplyingDamageDelayed(timeToTravel, hit.collider.gameObject, damage * thisBulletPower / bulletPower, bulletDirection, hit.point, forceToRigidBodys * thisBulletPower / bulletPower));

            #endregion Forward raycast

            hitIndex++;
            thisBulletPower -= thisPierceDecrease;
        }
    }

    public IEnumerator ApplyingDamageDelayed(float waitForSecs, GameObject goToTakeDamage, float damage, Vector3 direction, Vector3 hitPoint, float rbForcePower)
    {
        yield return new WaitForSeconds(waitForSecs);
        // addforce if it has rigidbody
        if (goToTakeDamage && goToTakeDamage.GetComponent<ApplyDamageScript>())
        {
            goToTakeDamage.GetComponent<ApplyDamageScript>().ApplyDamage(damage, direction, hitPoint, rbForcePower, ET.DamageType.BulletToBody);
        }
    }

    public IEnumerator InstantiateCoroutine(float waitForSecs, GameObject prefab, Vector3 pos, Quaternion rot, Transform parent, bool setAsParent = true)
    {
        yield return new WaitForSeconds(waitForSecs);
        GameObject newObj = Instantiate(prefab, pos, rot) as GameObject;
        if (setAsParent && parent)
            newObj.transform.SetParent(parent, true);
    }

    public Transform InstantiateReturn(Transform prefab)
    {
        return Instantiate(prefab);
    }

    public IEnumerator FixClipPosInLHand(Transform clip)
    {
        yield return null;
        clip.localPosition = Vector3.zero;
        clip.localRotation = Quaternion.Euler(Vector3.zero);
    }

    public IEnumerator DestroyCoroutine(float waitForSecs, GameObject objToDestroy)
    {
        yield return new WaitForSeconds(waitForSecs);
        Destroy(objToDestroy);
    }

    public IEnumerator AddBulletForceToRigidbodys(float waitForSecs, RaycastHit hit, float force, Vector3 forceDir)
    {
        yield return new WaitForSeconds(waitForSecs);
        // addforce if it has rigidbody
        if (hit.collider && hit.collider.GetComponent<Rigidbody>())
        {
            hit.collider.GetComponent<Rigidbody>().AddForce((forceDir).normalized * force);
        }
    }

    public void SortArray(ref RaycastHit[] hits)
    {
        RaycastHit tempRcH;

        for (int i = 0; i <= hits.Length - 1; i++)
        {
            for (int j = 1; j <= hits.Length - 1; j++)
            {
                if (hits[j - 1].distance > hits[j].distance)
                {
                    tempRcH = hits[j - 1];
                    hits[j - 1] = hits[j];
                    hits[j] = tempRcH;
                }
            }
        }
    }
}