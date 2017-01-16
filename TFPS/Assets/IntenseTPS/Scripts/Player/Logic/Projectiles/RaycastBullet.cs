using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(ProjectileBase))]
    public class RaycastBullet : MonoBehaviour
    {
        public float damage = 25f;
    }
}