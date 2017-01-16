using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ProjectileBase))]
public class Projectile : MonoBehaviour
{
    public List<GameObject> explosionFxPrefabs;
    public float explosionDamage = 70f;
    public float maxDamageDistance = .3f;
    public float explosionForce = 30f;
    public float explosionRadius = 3f;
    public GameObject childAfterBurner;
    public bool setEnabledAtStart = false;
    public float exitForce = 300;
    private bool hasExploded = false;

    private void Start()
    {
        if (setEnabledAtStart)
            EnableProjectile();
    }

    public void EnableProjectile()
    {
        StartCoroutine(AddForeceNextFrame());
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().isKinematic = false;
        }
        if (childAfterBurner)
            childAfterBurner.SetActive(true);
        if (GetComponent<Collider>())
            GetComponent<Collider>().enabled = true;
    }

    public IEnumerator AddForeceNextFrame()
    {
        yield return null;
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().AddForce(transform.forward * exitForce);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!hasExploded)
        {
            hasExploded = true;
            if (explosionFxPrefabs.Count > 0)
            {
                Instantiate(explosionFxPrefabs[Random.Range(0, explosionFxPrefabs.Count)], transform.position, Quaternion.identity);
            }
            if (GetComponent<AudioSource>())
                GetComponent<AudioSource>().Stop(); // stop trail sound

            // get collisions to calculate hit effects
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider col in colliders)
            {
                // apply damage
                if (col.GetComponent<ApplyDamageScript>())
                {
                    Vector3 direction = (-transform.position + col.transform.position).normalized;
                    float distFromExplosion = Vector3.SqrMagnitude(transform.position - col.transform.position);
                    float thisDamage = explosionDamage / (distFromExplosion < maxDamageDistance ? 1 : distFromExplosion);
                    col.GetComponent<ApplyDamageScript>().ApplyDamage(thisDamage, direction, transform.position, explosionForce / distFromExplosion, ET.DamageType.Explosion);
                }

                if (!col.GetComponent<Rigidbody>())
                    continue;
                col.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRadius, 1, ForceMode.Impulse);
            }
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (hasExploded)
            Destroy(gameObject);
    }
}