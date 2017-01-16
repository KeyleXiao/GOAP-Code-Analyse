using UnityEngine;

public class FakeDamager : MonoBehaviour
{
    public bool isEnabled = true;
    public ET.DamageType damageType;
    public float interval = 1f;
    public Vector2 randDamageBetween = new Vector2(3, 15);

    private float lastDamageAt = 0;

    private void Start()
    {
    }

    private void Update()
    {
        if (isEnabled)
        {
            switch (damageType)
            {
                case ET.DamageType.BulletToBody:
                    if (Time.time - lastDamageAt > interval && GetComponent<Health>())
                    {
                        ApplyDamageScript[] allParts = transform.GetComponentsInChildren<ApplyDamageScript>();
                        if (allParts.Length > 0)
                        {
                            allParts[Random.Range(0, allParts.Length - 1)].ApplyDamage(
                                Random.Range(randDamageBetween.x, randDamageBetween.y),
                                Vector3.zero,
                                Vector3.zero,
                                30,
                                damageType
                                );
                            lastDamageAt = Time.time;
                        }
                    }
                    break;

                case ET.DamageType.DirectToHealth:
                    if (Time.time - lastDamageAt > interval && GetComponent<Health>())
                    {
                        GetComponent<Health>().health -= Random.Range(randDamageBetween.x, randDamageBetween.y);
                        lastDamageAt = Time.time;
                    }
                    break;

                default:
                    break;
            }
        }
        else
            lastDamageAt = Time.time;
    }
}