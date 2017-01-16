using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GunAtt))]
public class ModifiableGun : MonoBehaviour
{
    public List<PartHolder> partHolders;
    private Decals decals;
    private List<WeaponFixersSingleType> emptyLeftHandles;
    private List<WeaponFixersSingleType> defClipFixHolders;
    private GPFlashlight flashLight;
    private GameObject muzzleFlashPrefab;
    private Transform muzzleFlashPosRot;
    private float spreadDecreaser = 0;
    private float barrelSpreadDecreaser = 0;
    private float damageIncreaser = 0;
    private float powerIncreaser = 0;
    private float forceToRigidbodyIncrease = 0;
    private float exitForceIncrease = 0;
    private List<GameObject> fireSounds;
    private Vector3 defClipPos;
    private Quaternion defClipRot;
    private int defLeftHandAnimNoOnAim;
    private int defLeftHandAnimNoOnIdle;

    public GPSecondaryFire SecFireGp { get; private set; }
    public float SecFireSpeed { get; private set; }

    private void Awake()
    {
        decals = GameObject.FindGameObjectWithTag("EnvFx").GetComponent<Decals>();
        GetDefault();
        ManageParts();
    }

    private void GetDefault()
    {
        GunAtt gunAtt = GetComponent<GunAtt>();

        // Handle
        emptyLeftHandles = GetComponent<GunAtt>().GetFixersForType(WeaponFixerTypes.LeftHandle);

        // Clip
        if (gunAtt.curClipObject)
        {
            defClipPos = gunAtt.curClipObject.localPosition;
            defClipRot = gunAtt.curClipObject.localRotation;
        }
        defLeftHandAnimNoOnAim = gunAtt.leftHandAnimNoOnAim;
        defLeftHandAnimNoOnIdle = gunAtt.leftHandAnimNoOnIdle;

        defClipFixHolders = gunAtt.GetFixersForType(WeaponFixerTypes.LeftHandClip);
    }

    public IEnumerator ManageNextFrame()
    {
        yield return new WaitForEndOfFrame();
        ManageParts();
    }

    public void ManageParts()
    {
        GunAtt gunAtt = GetComponent<GunAtt>();
        flashLight = null;

        spreadDecreaser = 0;
        damageIncreaser = 0;
        powerIncreaser = 0;
        forceToRigidbodyIncrease = 0;
        barrelSpreadDecreaser = 0;
        SecFireSpeed = .1f;
        gunAtt.SetFixersForType(emptyLeftHandles, WeaponFixerTypes.LeftHandle);
        gunAtt.SetFixersForType(defClipFixHolders, WeaponFixerTypes.LeftHandClip);
        fireSounds = gunAtt.sounds.fireSounds;
        SecFireGp = null;
        muzzleFlashPrefab = null;
        muzzleFlashPosRot = null;

        gunAtt.clipDefLocalPos = defClipPos;
        gunAtt.clipDefLocalRot = defClipRot;

        gunAtt.leftHandAnimNoOnAim = defLeftHandAnimNoOnAim;
        gunAtt.leftHandAnimNoOnIdle = defLeftHandAnimNoOnIdle;

        GunPart[] childGunParts = transform.GetComponentsInChildren<GunPart>();

        foreach (var childGPart in childGunParts)
        {
            if (childGPart.GetComponent<GPBarrel>())
            {
                if (childGPart.GetComponent<GPBarrel>().overrideFireSounds)
                    fireSounds = childGPart.GetComponent<GPBarrel>().fireSounds;
                powerIncreaser += childGPart.GetComponent<GPBarrel>().bulletPowerIncrease;
                damageIncreaser += childGPart.GetComponent<GPBarrel>().damageIncrease;
                barrelSpreadDecreaser += childGPart.GetComponent<GPBarrel>().spreadDecrease;
                forceToRigidbodyIncrease += childGPart.GetComponent<GPBarrel>().bulletForceToRigidbodyIncrease;
                exitForceIncrease += childGPart.GetComponent<GPBarrel>().exitForceIncrease;
                muzzleFlashPosRot = childGPart.GetComponent<GPBarrel>().childMuzzleFlashPosRot;
                muzzleFlashPrefab = childGPart.GetComponent<GPBarrel>().muzzleFlashPrefab;
            }
            if (childGPart.GetComponent<GPClip>())
            {
                if (childGPart.GetComponent<GunPart>().partPrefab)
                    gunAtt.curClipPrefab = childGPart.GetComponent<GunPart>().partPrefab.transform;

                gunAtt.SetFixersForType(childGPart.GetComponent<GPClip>().leftHandClipFixers, WeaponFixerTypes.LeftHandClip);

                gunAtt.curClipObject = childGPart.transform;
                gunAtt.maxClipCapacity = childGPart.GetComponent<GPClip>().clipCapacity;

                gunAtt.currentClipCapacity = childGPart.GetComponent<GPClip>().clipCapacity;

                gunAtt.clipDefLocalPos = childGPart.transform.localPosition;
                gunAtt.clipDefLocalRot = childGPart.transform.localRotation;

                gunAtt.DisableRbAndCol(childGPart.transform);
            }
            if (childGPart.GetComponent<GPFlashlight>())
            {
                flashLight = childGPart.GetComponent<GPFlashlight>();
            }
            else if (childGPart.GetComponent<GPGrip>())
            {
                spreadDecreaser += childGPart.GetComponent<GPGrip>().spreadDecrease;

                gunAtt.SetFixersForType(childGPart.GetComponent<GPGrip>().leftHandleFixers, WeaponFixerTypes.LeftHandle);

                gunAtt.leftHandAnimNoOnIdle = childGPart.GetComponent<GPGrip>().overrideLeftHandAnimNoOnIdle >= 0 ? childGPart.GetComponent<GPGrip>().overrideLeftHandAnimNoOnIdle : defLeftHandAnimNoOnIdle;
                gunAtt.leftHandAnimNoOnAim = childGPart.GetComponent<GPGrip>().overrideLeftHandAnimNoOnAim >= 0 ? childGPart.GetComponent<GPGrip>().overrideLeftHandAnimNoOnAim : defLeftHandAnimNoOnAim;
            }
            else if (childGPart.GetComponent<GPHandle>())
            {
                spreadDecreaser += childGPart.GetComponent<GPHandle>().spreadDecrease;
                powerIncreaser += childGPart.GetComponent<GPHandle>().bulletPowerIncrease;
            }
            if (childGPart.GetComponent<GPSecondaryFire>())
            {
                SecFireGp = childGPart.GetComponent<GPSecondaryFire>();
                SecFireSpeed = childGPart.GetComponent<GPSecondaryFire>().fireSpeed;
            }
            if (childGPart.GetComponent<GPSight>())
            {
                spreadDecreaser += childGPart.GetComponent<GPSight>().spreadDecrease;
            }
            if (childGPart.GetComponent<GPSpecial>())
            {
                spreadDecreaser += childGPart.GetComponent<GPSpecial>().spreadDecrease;
                powerIncreaser += childGPart.GetComponent<GPSpecial>().powerIncrease;
                damageIncreaser += childGPart.GetComponent<GPSpecial>().damageIncrease;
            }
        }
    }

    public void Fire
    (
    Transform owner,
    Transform target,
    Vector3 firingToPos,
    ref float _weaponBodyBob,
    ref float _randomHandTwistSign,
    Quaternion xq,
    LayerMask bulletMask,
    float projectileDamage = 0
    )
    {
        GunAtt gunAtt = GetComponent<GunAtt>();
        gunAtt.currentClipCapacity -= 1;

        float xSpread = 0, ySpread = 0, bodySpread = 0;
        bodySpread = gunAtt.bodySpread;
        xSpread = (float)(Random.value - 0.5) * (gunAtt.spreadAmount - spreadDecreaser - barrelSpreadDecreaser);
        ySpread = (float)(Random.Range(.35f, 1f) - 0.5) * (gunAtt.spreadAmount - spreadDecreaser);

        if (fireSounds.Count > 0)
        {
            int randFireSound = Random.Range(0, fireSounds.Count);
            GameObject fireSound = Instantiate(fireSounds[randFireSound], transform.position, transform.rotation) as GameObject;
            if (fireSound)
            {
                fireSound.transform.SetParent(transform);
                AudioEventMonoB audioEventMonoB = fireSound.GetComponent<AudioEventMonoB>();
                if (audioEventMonoB)
                    audioEventMonoB.owner = owner;
            }
        }

        if (gunAtt.animator)
        {
            gunAtt.animator.SetTrigger("Fire");
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
        if (muzzleFlashPrefab && muzzleFlashPosRot)
        {
            GameObject goMuzzle = Instantiate(muzzleFlashPrefab, muzzleFlashPosRot.position, muzzleFlashPosRot.rotation) as GameObject;
            goMuzzle.transform.SetParent(gunAtt.transform, true);
        }

        #region Raycast Fire

        if (!gunAtt.isFiringProjectile)
        {
            if (!gunAtt.isShotGun)
            {
                Vector3 bulletDir = xq * (firingToPos - gunAtt.spawn.position).normalized;
                RaycastHit[] hits = Physics.RaycastAll(gunAtt.spawn.position, bulletDir, gunAtt.maxBulletDistance, bulletMask); // raycastAll - get hits

                gunAtt.SortArray(ref hits); // sort raycast hits by distance
                ManageHits(hits, gunAtt.bulletPower + powerIncreaser, bulletDir, gunAtt.bulletForceToRigidbodys + forceToRigidbodyIncrease, gunAtt.damage + damageIncreaser + (projectileDamage),
                    gunAtt.bulletSpeed, gunAtt.bulletTrailExitForce, gunAtt.bulletTrailPrefab, gunAtt.bulletTrailExitPosRot, bulletDir, bulletMask);
            }
            else
            {
                for (int i = 0; i < gunAtt.shrapnelCount; i++)
                {
                    Vector3 bulletDir = xq * Quaternion.Euler(Random.Range(-gunAtt.maxshrapnelSpread, gunAtt.maxshrapnelSpread), Random.Range(-gunAtt.maxshrapnelSpread, gunAtt.maxshrapnelSpread), 0) * (firingToPos - gunAtt.spawn.position).normalized;
                    RaycastHit[] hits = Physics.RaycastAll(gunAtt.spawn.position, bulletDir, gunAtt.maxBulletDistance, bulletMask); // raycastAll - get hits

                    gunAtt.SortArray(ref hits); // sort raycast hits by distance
                    ManageHits(hits, gunAtt.bulletPower + powerIncreaser, bulletDir, gunAtt.bulletForceToRigidbodys + forceToRigidbodyIncrease, gunAtt.damage + damageIncreaser + (projectileDamage), gunAtt.bulletSpeed, gunAtt.bulletTrailExitForce, gunAtt.bulletTrailPrefab, gunAtt.bulletTrailExitPosRot, bulletDir, bulletMask);
                }
            }
        }

        #endregion Raycast Fire

        #region Projectile Fire

        else
        {
            if (gunAtt.firesCurClipObject && gunAtt.curClipObject.GetComponent<Projectile>())
            {
                gunAtt.curClipObject.SetParent(null);
                gunAtt.curClipObject.rotation = xq;
                if (gunAtt.curClipObject.GetComponent<Rigidbody>())
                    GetComponent<Rigidbody>().AddForce(transform.forward * (gunAtt.curClipObject.GetComponent<Projectile>().exitForce + exitForceIncrease));
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

    public void SecFire
  (
  Transform owner,
  Transform target,
  Vector3 firingToPos,
  ref float _weaponBodyBob,
  ref float _randomHandTwistSign,
  Quaternion xq,
  LayerMask bulletMask
  )
    {
        if (!SecFireGp)
            return;

        GunAtt gunAtt = GetComponent<GunAtt>();
        SecFireGp.currentClipCapacity -= 1;

        float xSpread = 0, ySpread = 0, bodySpread = 0;
        bodySpread = SecFireGp.bodySpread;
        xSpread = (float)(Random.value - 0.5) * (SecFireGp.spreadAmount - spreadDecreaser);
        ySpread = (float)(Random.Range(.35f, 1f) - 0.5) * (SecFireGp.spreadAmount - spreadDecreaser);

        if (SecFireGp.fireSounds.Count > 0)
        {
            int randFireSound = Random.Range(0, SecFireGp.fireSounds.Count);
            GameObject fireSound = Instantiate(SecFireGp.fireSounds[randFireSound], transform.position, transform.rotation) as GameObject;
            if (fireSound)
            {
                fireSound.transform.SetParent(transform);

                AudioEventMonoB audioEventMonoB = fireSound.GetComponent<AudioEventMonoB>();
                if (audioEventMonoB)
                    audioEventMonoB.owner = owner;
            }
        }

        if (SecFireGp.animator)
        {
            SecFireGp.animator.SetTrigger("Fire");
        }

        // empty shell
        if (SecFireGp.emptyShellPosition && SecFireGp.EmptyShellPrefab)
        {
            Vector3 randomEmptyShellForce = new Vector3(Random.Range(SecFireGp.emptyShellMinForce.x, SecFireGp.emptyShellMaxForce.x), Random.Range(SecFireGp.emptyShellMinForce.y, SecFireGp.emptyShellMaxForce.y), Random.Range(SecFireGp.emptyShellMinForce.z, SecFireGp.emptyShellMaxForce.z));
            GameObject goEmptyShell = Instantiate(SecFireGp.EmptyShellPrefab, SecFireGp.emptyShellPosition.position, SecFireGp.emptyShellPosition.rotation) as GameObject;
            goEmptyShell.GetComponent<Rigidbody>().AddForce(SecFireGp.emptyShellPosition.rotation * randomEmptyShellForce, ForceMode.Impulse);
            goEmptyShell.transform.SetParent(decals.transform, true);
        }

        // muzzle flash
        if (SecFireGp.muzzleFlashPrefab)
        {
            GameObject goMuzzle = Instantiate(SecFireGp.muzzleFlashPrefab, SecFireGp.muzzleFlashPosRot.position, SecFireGp.muzzleFlashPosRot.rotation) as GameObject;
            goMuzzle.transform.SetParent(gunAtt.transform, true);
        }

        #region Raycast Fire

        if (!SecFireGp.isFiringProjectile)
        {
            if (!SecFireGp.isShotGun)
            {
                Vector3 bulletDir = xq * (firingToPos - SecFireGp.spawn.position).normalized;
                RaycastHit[] hits = Physics.RaycastAll(gunAtt.spawn.position, bulletDir, gunAtt.maxBulletDistance, bulletMask); // raycastAll - get hits

                gunAtt.SortArray(ref hits); // sort raycast hits by distance
                ManageHits(hits, SecFireGp.bulletPower, bulletDir, SecFireGp.bulletForceToRigidbodys, SecFireGp.damage + (SecFireGp.currentProjectilePrefab.GetComponent<RaycastBullet>().damage),
                    SecFireGp.bulletSpeed, SecFireGp.bulletTrailExitForce, SecFireGp.bulletTrailPrefab, SecFireGp.bulletTrailExitPosRot, bulletDir, bulletMask);
            }
            else
            {
                for (int i = 0; i < SecFireGp.shrapnelCount; i++)
                {
                    Vector3 bulletDir = xq * Quaternion.Euler(Random.Range(-SecFireGp.maxshrapnelSpread, SecFireGp.maxshrapnelSpread), Random.Range(-SecFireGp.maxshrapnelSpread, SecFireGp.maxshrapnelSpread), 0) * (firingToPos - SecFireGp.spawn.position).normalized;
                    //Debug.DrawRay(gunAtt.spawn.position, bulletDir * 55, Color.red);
                    RaycastHit[] hits = Physics.RaycastAll(SecFireGp.spawn.position, bulletDir, SecFireGp.maxBulletDistance, bulletMask); // raycastAll - get hits

                    gunAtt.SortArray(ref hits); // sort raycast hits
                    ManageHits(hits, SecFireGp.bulletPower, bulletDir, SecFireGp.bulletForceToRigidbodys, SecFireGp.damage + (SecFireGp.currentProjectilePrefab.GetComponent<RaycastBullet>().damage), SecFireGp.bulletSpeed, SecFireGp.bulletTrailExitForce, SecFireGp.bulletTrailPrefab, SecFireGp.bulletTrailExitPosRot, bulletDir, bulletMask);
                }
            }
        }

        #endregion Raycast Fire

        #region Projectile Fire

        else
        {
            //if (SecFireGp.firesCurClipObject && SecFireGp.curClipObject.GetComponent<Projectile>())
            //{
            //    gunAtt.curClipObject.SetParent(null);
            //    gunAtt.curClipObject.forward = xq * (firingToPos - gunAtt.spawn.position).normalized;

            //    gunAtt.curClipObject.GetComponent<Projectile>().EnableProjectile();
            //}
            //else
            {
                Instantiate(SecFireGp.projectilePrefab, gunAtt.spawn.position, xq);
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

        GunAtt gunAtt = GetComponent<GunAtt>();
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
                StartCoroutine(gunAtt.DestroyCoroutine(timeToTravel, tempBulletTrail));

                // BW raycast
                if (hitIndex > 0)
                {
                    RaycastHit[] hitsBW = Physics.RaycastAll(hits[hitIndex - 1].point, -bulletDirection, Vector3.Distance(hits[0].point, hits[hitIndex - 1].point), bulletMask);
                    if (hitsBW.Length > 1)
                    {
                        gunAtt.SortArray(ref hitsBW);
                        foreach (RaycastHit hitBW in hitsBW)
                        {
                            decals.GetAllNormalShot(hitBW.transform.tag, ref decal, ref fx, ref hitSound, ref thisPierceDecrease);

                            // Instantiate them delayed
                            float timeToTravelBW = Vector3.Distance(hitBW.point, exitPos) / bulletSpeed;
                            if (decal != null)
                                StartCoroutine(gunAtt.InstantiateCoroutine(
                                    timeToTravelBW, decal, hitBW.point + (hitBW.normal * .04f), Quaternion.LookRotation(hitBW.normal) * Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360))), hitBW.transform));

                            if (fx != null)
                                StartCoroutine(gunAtt.InstantiateCoroutine(
                                    timeToTravelBW, fx, hitBW.point + (hitBW.normal * .07f), Quaternion.LookRotation(hitBW.normal) * Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360))), hitBW.transform, true));

                            if (hitSound != null)
                                StartCoroutine(gunAtt.InstantiateCoroutine(timeToTravelBW, hitSound, hitBW.point, Quaternion.identity, hitBW.transform, true));
                        }
                    }
                }

                #endregion Destroy trail & Backward raycast

                break;
            }

            #region Forward raycast

            decals.GetAllNormalShot(hit.transform.tag, ref decal, ref fx, ref hitSound, ref thisPierceDecrease);

            // to look realistic Instantiate them delayed
            timeToTravel = Vector3.Distance(hit.point, exitPos) / bulletSpeed;
            if (decal != null)
                StartCoroutine(gunAtt.InstantiateCoroutine(
                    timeToTravel, decal, hit.point + (hit.normal * .04f), Quaternion.LookRotation(hit.normal) * Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360))), hit.transform, true));

            if (fx != null)
                StartCoroutine(gunAtt.InstantiateCoroutine(
                    timeToTravel, fx, hit.point + (hit.normal * .07f), Quaternion.LookRotation(hit.normal) * Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360))), hit.transform, true));

            if (hitSound != null)
                StartCoroutine(gunAtt.InstantiateCoroutine(timeToTravel, hitSound, hit.point, Quaternion.identity, hit.transform, true));

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
            StartCoroutine(gunAtt.AddBulletForceToRigidbodys(timeToTravel, hit, forceToRigidBodys * thisBulletPower / bulletPower, bulletDirection));

            // Apply damage
            StartCoroutine(gunAtt.ApplyingDamageDelayed(timeToTravel, hit.collider.gameObject, damage * thisBulletPower / bulletPower, bulletDirection, hit.point, forceToRigidBodys * thisBulletPower / bulletPower));

            #endregion Forward raycast

            hitIndex++;
            thisBulletPower -= thisPierceDecrease;
        }
    }

    // Use this to Instantiate parts (Bc reference to prefab need to be assigned in clone part)
    public GameObject CreateClonePart(GameObject prefab)
    {
        foreach (var partHolder in partHolders)
        {
            foreach (var compPart in partHolder.compatibleParts)
            {
                if (compPart.prefab == prefab)
                {
                    GameObject cloneGo = Instantiate(compPart.prefab);
                    cloneGo.transform.SetParent(transform, true);
                    cloneGo.transform.localPosition = compPart.localPos;
                    cloneGo.transform.localRotation = Quaternion.Euler(compPart.localRot);
                    cloneGo.GetComponent<GunPart>().partPrefab = prefab;
                    prefab.GetComponent<GunPart>().cloneLocalPos = compPart.localPos;
                    prefab.GetComponent<GunPart>().cloneLocalRot = Quaternion.Euler(compPart.localRot);

                    return cloneGo;
                }
            }
        }
        Debug.Log("Error");
        return null;
    }

    public void FlashLightOnOff()
    {
        if (flashLight && flashLight.childFlashlight)
        {
            if (flashLight.childFlashlight.activeSelf)
            {
                flashLight.childFlashlight.SetActive(false);
                Instantiate(flashLight.turnOffAudioPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                flashLight.childFlashlight.SetActive(true);
                Instantiate(flashLight.turnOnAudioPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}

[System.Serializable]
public class PartHolder
{
    public string name;
    public PartHolderTypes partHolderType;
    public Transform holderTransform;
    public bool allowEmpty;
    public List<CompatiblePart> compatibleParts;

    public PartHolder(string _name, PartHolderTypes _type, Transform _holderTransform, bool _allowEmpty)
    {
        name = _name;
        partHolderType = _type;
        holderTransform = _holderTransform;
        allowEmpty = _allowEmpty;
        compatibleParts = new List<CompatiblePart>();
    }
}

[System.Serializable]
public class CompatiblePart
{
    public Vector3 localPos;
    public Vector3 localRot;
    public GameObject prefab;
    public string compatibleWeaponName;

    public CompatiblePart(string _compWeapName)
    {
        compatibleWeaponName = _compWeapName;
    }
}