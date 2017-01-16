using System.Collections;
using UnityEngine;

/// <summary>
/// Use <see cref="Health"/> script to add apply damage to body parts (or add to a body part that has collider, manually )
/// </summary>
public class ApplyDamageScript : MonoBehaviour
{
    public Health health;
    public float damageMultiplier = 1;
    private bool isDead = false;

    private void Start()
    {
    }

    public delegate void DamageTakeHandler(
        Transform bodyPart,
        ET.DamageType damageType,
        float damageTaken,
        bool isDead,
        Vector3 direction,
        Vector3 hitPoint,
        float force
        );

    public event DamageTakeHandler onTakeDamage;

    public void TakeDamage(Transform bodyPart,
        ET.DamageType damageType,
        float damageTaken,
        bool isDead,
        Vector3 direction,
        Vector3 hitPoint,
        float force)
    {
        if (onTakeDamage != null)
            onTakeDamage(transform, damageType, damageTaken, isDead, direction, hitPoint, force);
    }

    public void ApplyDamage(float damage, Vector3 direction, Vector3 hitPoint, float force, ET.DamageType damageType)
    {
        if (!isDead)
        {
            health.health -= damage * damageMultiplier;

            if (health.health <= 0 && force > 0)
            {
                StartCoroutine(AddForceToRbs(direction, hitPoint, force));
                isDead = true;
            }
            TakeDamage(transform, damageType, damage, isDead, direction, hitPoint, force);
        }
    }

    public IEnumerator AddForceToRbs(Vector3 dir, Vector3 hitPoint, float force)
    {
        yield return null;
        foreach (Rigidbody rb in health.transform.GetComponentsInChildren<Rigidbody>())
            rb.AddForceAtPosition(dir * force, hitPoint);
    }
}