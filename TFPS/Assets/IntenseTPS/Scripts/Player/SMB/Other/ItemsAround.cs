using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class ItemsAround : MonoBehaviour
    {
        public LayerMask layerMask;
        public List<Transform> collectableWeapons { get; private set; }
        public Transform BestPickable { get; private set; }

        private Transform playerCamTransform;

        public SupplyBox CAmmoBag { get; private set; }

        private void Start()
        {
            collectableWeapons = new List<Transform>();

            playerCamTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;

            if (!playerCamTransform)
            {
                Debug.Log("Needed reference not found in " + ToString());
                gameObject.SetActive(false);
            }
        }

        private void FixedUpdate()
        {
            if (collectableWeapons.Count > 0)
            {
                float lowestMagn = Mathf.Infinity;
                foreach (var item in collectableWeapons)
                {
                    if (!item || !item.GetComponent<Collider>().enabled)
                        continue;

                    Ray ray = new Ray(playerCamTransform.position, playerCamTransform.forward);
                    float magn = Vector3.Cross(ray.direction, item.position - ray.origin).magnitude;
                    if (magn < lowestMagn)
                    {
                        BestPickable = item;
                        lowestMagn = magn;
                    }
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (((1 << other.gameObject.layer) & layerMask) != 0)
            {
                // Weapon checks
                if (other.GetComponent<GunAtt>())
                {
                    if (!collectableWeapons.Contains(other.transform))
                        collectableWeapons.Add(other.transform);
                }

                if (other.GetComponent<SupplyBox>())
                    CAmmoBag = other.GetComponent<SupplyBox>();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<GunAtt>())
            {
                if (collectableWeapons.Contains(other.transform))
                    collectableWeapons.Remove(other.transform);
                if (collectableWeapons.Count == 0)
                    BestPickable = null;
            }

            if (other.GetComponent<SupplyBox>())
                CAmmoBag = null;
        }
    }
}