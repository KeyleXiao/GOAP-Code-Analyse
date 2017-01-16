using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class Exploder : MonoBehaviour
    {
        [System.Serializable]
        public class ThrowableSounds
        {
            public List<GameObject> pulls;
            public List<GameObject> pins;
            public List<GameObject> throws;
            public List<GameObject> explosions;
            public List<GameObject> colliderHits;
        }

        public string throwableName;

        [Space]
        public int throwStyle = 1;

        public int locomotionStyleIndex = -1; // Leave at -1 to use default locomotion style with this exploder
        public int lookIKStyleIndex = -1; // Leave at -1 to use default lookIK style with this exploder
        public int throwStyleIndex = -1; // Leave at -1 to use default throw style with this exploder

        [Space]
        public Sprite hudImage;

        public ThrowableSounds sounds;
        public Vector2 throwForceMinMax = new Vector2(100, 500);
        public float additionalThrowAngleHorizontal = 25;
        public bool canExplodeOtherBombs = true; // explodes other bombs in the explosionRadius if their canExplodeByAnotherBombs = true
        public bool canExplodeByOtherBombs = true;
        public float otherBombMinExplodeTime = .05f; // delayed explosions by distance
        public float otherBombMaxExplodeTime = .3f;  // delayed explosions by distance
        public float minExplodeVelocity = 3; // you can set this zero to explode the grenade on any hit with any speed // or set very high to prevent exploding on hit
        public float explosionTimer = 10f; // if you dont use timer set this value very high
        public bool isSticky = false;
        public bool destroyOnExplosion = true;
        public bool showEstimatedRoute = true; // requires line renderer
        public bool shakesCam = false;
        public float shakeFeelDistance = 20f;
        public CameraShaker cameraPositionShaker;
        public CameraShaker cameraRotationShaker;
        public float modelDestroyTime = 120;
        public GameObject explosionFx;
        public bool circularExplosionFx = false;
        public int circularFxCount = 20;
        public float circularMaxForce = 5;
        public float minRelativeVelMagnHitSound = 2;
        public float minIntervalHitSoundTime = .3f;
        public float explosionForce = 30f;
        public float explosionMultiplier = 1;
        public float explosionRadius = 3f;
        public float explosionDamage = 70f;
        public float maxDamageDistance = .3f;
        public Transform leftHandHoldPositionRotation;
        public Transform rightHandHoldPositionRotation;

        [Space]
        public int lineRend_segmentCount = 20;

        public float lineRend_segmentScale = 1;
        public Color lineRend_nextColor;
        public float landingFireForceDivisor; // Use this to get the proper landing position (the same with trajectory simulation)
        public Collider _hitObject { get; private set; }
        public float FireStrength { get; set; }
        public Vector3 FireDir { get; set; }

        [System.NonSerialized]
        public bool lineRendEnabled = false;

        [System.NonSerialized]
        public LineRenderer lineRenderer;

        [System.NonSerialized]
        public float _tempTimer;

        [System.NonSerialized]
        public bool timerAlreadySet = false;

        private Transform playerT;
        private bool exploded = false;
        private bool isColHitSoundPlaying = false;

        private void Start()
        {
            _tempTimer = explosionTimer;
            lineRenderer = GetComponent<LineRenderer>();
            //tpsCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<TPSCamera>();
            playerT = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void Update()
        {
            _tempTimer -= Time.deltaTime;
            if (exploded)
                if (destroyOnExplosion)
                    Destroy(gameObject);
            if (exploded)
                return;
            Explode(null);
        }

        private void FixedUpdate()
        {
            if (lineRendEnabled && showEstimatedRoute)
            {
                // Unity landing position finder
                SimulatePath();
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            // coll hit sounds
            if (sounds.colliderHits.Count > 0 && /*emptyAudioSourcePrefab &&*/ !isColHitSoundPlaying)
                collHitSound(other.relativeVelocity.magnitude, other.contacts[0].point);

            if (exploded)
                return;
            Explode(other);
        }

        private void OnCollisionStay(Collision other)
        {
            // coll hit sounds
            if (sounds.colliderHits.Count > 0 && /*emptyAudioSourcePrefab &&*/ !isColHitSoundPlaying)
                collHitSound(other.relativeVelocity.magnitude, other.contacts[0].point);

            if (exploded)
                return;
            Explode(other);
        }

        public void Explode(Collision other, bool explodeNow = false)
        {
            if ((other != null && other.relativeVelocity.magnitude > minExplodeVelocity) /*|| (explodesOnAnyHit && other != null) */|| _tempTimer < 0 || explodeNow)
            {
                exploded = true;

                // Camera shake
                if (shakesCam)
                {
                    float distFromPlayer = Vector3.Distance(playerT.position, transform.position);
                    if (distFromPlayer < shakeFeelDistance)
                    {
                        float perc = 1 - distFromPlayer / shakeFeelDistance;
                        GameObject goCam = GameObject.FindGameObjectWithTag("MainCamera");
                        if (goCam && goCam.transform.parent && goCam.transform.parent.GetComponent<PlayerCamera>())
                            goCam.transform.parent.GetComponent<PlayerCamera>().AddShakeCamera(
                                cameraPositionShaker * perc, cameraRotationShaker * perc);
                    }
                }

                // explosion fx
                if (explosionFx)
                {
                    if (!circularExplosionFx)
                        Instantiate(explosionFx, transform.position, Quaternion.identity);
                    else
                    {
                        Vector3 contactPosition = transform.position; // if this is timed explosion
                        Vector3 contactNormal = transform.up;
                        if (other != null) // if hit something, not timed
                        {
                            contactPosition = other.contacts[0].point;
                            contactNormal = other.contacts[0].normal;
                        }
                        for (int i = 0; i < circularFxCount; i++)
                        {
                            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, contactNormal);
                            rotation = rotation * Quaternion.Euler(Vector3.up * i * (360 / circularFxCount));
                            GameObject fxGo = Instantiate(explosionFx, contactPosition + contactNormal * .5f, rotation) as GameObject;
                            fxGo.transform.position = fxGo.transform.position + fxGo.transform.forward * .1f;
                            fxGo.GetComponent<Rigidbody>().velocity = fxGo.transform.forward * Random.Range(0, circularMaxForce) + fxGo.transform.up * -1f;
                        }
                    }
                }

                PlayRandomSoundAsWeaponChild(sounds.explosions, transform, false);

                Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
                foreach (Collider col in colliders)
                {
                    // apply damage
                    if (col.GetComponent<ApplyDamageScript>())
                    {
                        Vector3 direction = (-transform.position + col.transform.position).normalized;
                        float distFromExplosion = Vector3.SqrMagnitude(transform.position - col.transform.position);
                        float thisDamage = explosionDamage / (distFromExplosion < maxDamageDistance ? 1 : distFromExplosion);

                        col.GetComponent<ApplyDamageScript>().ApplyDamage(
                            thisDamage, direction, transform.position, explosionForce / distFromExplosion, ET.DamageType.Explosion);
                    }

                    Rigidbody rb = col.GetComponent<Rigidbody>();
                    if (rb)
                        rb.AddExplosionForce(explosionForce * explosionMultiplier, transform.position, 10 * explosionMultiplier, 1 * explosionMultiplier, ForceMode.Impulse);

                    Exploder exploder = col.GetComponent<Exploder>();
                    if (exploder && !exploder.timerAlreadySet && canExplodeOtherBombs && exploder.canExplodeByOtherBombs && !exploder.exploded)
                    {
                        float timer = otherBombMinExplodeTime + (otherBombMaxExplodeTime - otherBombMinExplodeTime) * Vector3.Distance(transform.position, col.transform.position) / explosionRadius;
                        exploder._tempTimer = timer < exploder._tempTimer ? timer : exploder._tempTimer;
                    }
                }
            }

            if (other != null && isSticky)
            {
                GetComponent<Rigidbody>().isKinematic = true;
            }
        }

        private void collHitSound(float hitRelativeVelocity, Vector3 hitPoint)
        {
            if (hitRelativeVelocity > minRelativeVelMagnHitSound)
            {
                PlayRandomSoundAsWeaponChild(sounds.colliderHits, transform, true);
                isColHitSoundPlaying = true;
                StartCoroutine(IntervalHitSound());
            }
        }

        private IEnumerator IntervalHitSound()
        {
            yield return new WaitForSeconds(minIntervalHitSoundTime);
            isColHitSoundPlaying = false;
        }

        public void PlayRandomSoundAsWeaponChild(List<GameObject> availableSounds, Transform owner, bool setAsParent = true, Transform alternateParent = null)
        {
            if (availableSounds.Count > 0)
            {
                int rand = Random.Range(0, availableSounds.Count);
                GameObject randSound = Instantiate(availableSounds[rand], transform.position, transform.rotation) as GameObject;
                if (randSound)
                {
                    if (setAsParent && !alternateParent)
                        randSound.transform.SetParent(transform);
                    else if (setAsParent && alternateParent)
                        randSound.transform.SetParent(alternateParent);
                    randSound.transform.localPosition = Vector3.zero;

                    AudioEventMonoB audioEventMonoB = randSound.GetComponent<AudioEventMonoB>();
                    if (audioEventMonoB)
                        audioEventMonoB.owner = owner;
                }
            }
        }

        private void SimulatePath()
        {
            if (!lineRenderer)
                return;

            Vector3[] segments = new Vector3[lineRend_segmentCount];

            // The first line point is wherever the player's cannon, etc is
            segments[0] = transform.position;

            // The initial velocity
            Vector3 segVelocity = /*transform.up*/FireDir * FireStrength * Time.deltaTime;

            // reset our hit object
            _hitObject = null;

            for (int i = 1; i < lineRend_segmentCount; i++)
            {
                // Time it takes to traverse one segment of length segScale (careful if velocity is zero)
                float segTime = (segVelocity.sqrMagnitude != 0) ? lineRend_segmentScale / segVelocity.magnitude : 0;

                // Add velocity from gravity for this segment's timestep
                segVelocity = segVelocity + Physics.gravity * segTime;

                // Check to see if we're going to hit a physics object
                RaycastHit hit;
                if (Physics.Raycast(segments[i - 1], segVelocity, out hit, lineRend_segmentScale))
                {
                    // remember who we hit
                    _hitObject = hit.collider;

                    // set next position to the position where we hit the physics object
                    segments[i] = segments[i - 1] + segVelocity.normalized * hit.distance;
                    // correct ending velocity, since we didn't actually travel an entire segment
                    segVelocity = segVelocity - Physics.gravity * (lineRend_segmentScale - hit.distance) / segVelocity.magnitude;
                    // flip the velocity to simulate a bounce
                    segVelocity = Vector3.Reflect(segVelocity, hit.normal);

                    /*
                     * Here you could check if the object hit by the Raycast had some property - was
                     * sticky, would cause the ball to explode, or was another ball in the air for
                     * instance. You could then end the simulation by setting all further points to
                     * this last point and then breaking this for loop.
                     */
                }
                // If our raycast hit no objects, then set the next position to the last one plus v*t
                else
                {
                    segments[i] = segments[i - 1] + segVelocity * segTime;
                }
            }

            // At the end, apply our simulations to the LineRenderer

            // Set the colour of our path to the colour of the next ball
            Color startColor = lineRend_nextColor;
            Color endColor = startColor;
            startColor.a = 1;
            endColor.a = 0;
            lineRenderer.SetColors(startColor, endColor);

            lineRenderer.SetVertexCount(lineRend_segmentCount);
            for (int i = 0; i < lineRend_segmentCount; i++)
                lineRenderer.SetPosition(i, segments[i]);
        }
    }
}