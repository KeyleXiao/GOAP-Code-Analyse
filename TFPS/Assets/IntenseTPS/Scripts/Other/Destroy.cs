using UnityEngine;

public class Destroy : MonoBehaviour
{
    public float destroyTime = 2;
    public float destroyTimeRandomize = 0;

    private float countToTime;

    private void Awake()
    {
        destroyTime += Random.value * destroyTimeRandomize;
    }

    private void Update()
    {
        countToTime += Time.deltaTime;
        if (countToTime >= destroyTime)
            Destroy(gameObject);
    }
}