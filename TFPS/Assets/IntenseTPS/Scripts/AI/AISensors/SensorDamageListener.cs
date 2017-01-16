using Information;
using UnityEngine;

namespace Sensors
{
    /// <summary>
    /// Listens applied damages to body and adds them to <see cref="AIMemory"/>, currently only listens bullet damages
    /// </summary>
    public class SensorDamageListener : AISensorTrigger
    {
        public float maxConfidenceDmgBulletToHumanoid = 25;

        private AIBrain ai;

        public override void OnStart(AIBrain _ai)
        {
            ai = _ai;
            // Listen to all take damage scripts of this agent
            foreach (Transform transform in ai.Transform.GetComponentsInChildren<Transform>())
                if (transform.GetComponent<ApplyDamageScript>())
                    transform.GetComponent<ApplyDamageScript>().onTakeDamage += OnDamageTake;
        }

        public void OnDamageTake(Transform bodyPart, ET.DamageType damageType, float damageTaken, bool isDead, Vector3 direction, Vector3 hitPoint, float force)
        {
            switch (damageType)
            {
                case ET.DamageType.BulletToBody:
                    ai.Memory.Add(
                        new InformationReceivedDamageBulletToHumanoid
                        (
                            damageTaken, maxConfidenceDmgBulletToHumanoid,
                            direction, 1,
                            force, 1,
                            bodyPart
                        )
                        );
                    break;

                case ET.DamageType.DirectToHealth:
                    break;

                default:
                    break;
            }
        }
    }
}