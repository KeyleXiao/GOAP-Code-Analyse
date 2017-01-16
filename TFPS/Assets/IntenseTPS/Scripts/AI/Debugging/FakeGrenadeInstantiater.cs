using System.Collections;
using UnityEngine;

public class FakeGrenadeInstantiater : MonoBehaviour
{
    public bool on = false;
    public float interval = 2f;
    public float explodeTime = 2f;
    public float forwardForce = 5f;
    public GameObject grenadePrefab;
    public GameObject explosionPrefab;

    private float _tempInterval;

    private void Start()
    {
    }

    private void Update()
    {
        if (!grenadePrefab || !on)
            return;

        if (_tempInterval < 0)
        {
            _tempInterval = interval;
            GameObject go = Instantiate(grenadePrefab, transform.position + transform.forward * .3f, transform.rotation) as GameObject;
            if (go.GetComponent<Rigidbody>())
                go.GetComponent<Rigidbody>().AddForce(transform.forward * forwardForce);
            StartCoroutine(FakeExplode(go.transform, explodeTime));
        }

        _tempInterval -= Time.deltaTime;
    }

    public IEnumerator FakeExplode(Transform transfrm, float explodeTime)
    {
        yield return new WaitForSeconds(explodeTime);
        if (explosionPrefab)
            Instantiate(explosionPrefab, transfrm.position, Quaternion.identity);
        Destroy(transfrm.gameObject);
    }
}