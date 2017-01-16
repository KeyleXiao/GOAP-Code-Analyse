using UnityEngine;

public class BulletTrailNoRigidbody : MonoBehaviour
{
    public float speed = 250;

    private void Update()
    {
        transform.position = transform.position + transform.forward * Time.deltaTime * speed;
    }
}