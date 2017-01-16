using Information;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sensors
{
    /// <summary>
    /// Finds possible safe positions on <see cref="NavMesh"/> and adds them to memory when there is a target selected by <see cref="AISystemTargetSelector"/>
    /// </summary>
    public class SensorSafePositionFinder : AISensorPolling
    {
        public bool enabled = true;
#if UNITY_EDITOR
        public bool showShapes = false;
#endif
        public int maxMemoryAddPerUpdate = 5;
        public Vector2 randomStartDistance = new Vector2(.15f, 1f);
        public int angleCheckCount = 3;
        public int checkAlongAngleCount = 5;

        //public int aroundPointCount = 4; // not used atm
        public float stepSize = 2.8f;

        public Vector2 rayHeightMinMaxVertical = new Vector2(.5f, 1.5f);
        public int rayCheckCountVertical = 2;
        public float rayToTargetHeight = .5f;
        public float rayMaxDistance = 30f;
        public float maxDistanceFromTargetForMaxConfidence = 25f; // min is considered 0
        public float maxDistanceFromSelfForMinConfidence = 25f; // min is considered 0

        [System.NonSerialized]
        public List<string> fightableTags;

        public LayerMask lineCastMask;

        public override bool OnUpdate(AIBrain ai)
        {
            if (!enabled)
                return false;
            bool sensorUpdated = false;

            if (FindSafePositions(ai))
            {
                sensorUpdated = true;
            }
            return sensorUpdated;
        }

        private bool FindSafePositions(AIBrain ai)
        {
            if (!ai.HaveCurrentTarget())
                return false;

            Transform transform = ai.Transform;

            List<Vector3> samplePositions = new List<Vector3>();
            samplePositions.Clear();

            float randDirAngle = Random.Range(0, 360);

            for (int randAngleCounter = 0; randAngleCounter < angleCheckCount; randAngleCounter++)
            {
                randDirAngle += 360 / angleCheckCount;
                float randStartDist = Random.Range(randomStartDistance.x, randomStartDistance.y);

                Vector3 startPos = transform.position + Quaternion.Euler(0, randDirAngle, 0) * transform.forward * randStartDist;
                Vector3 startDir = Quaternion.Euler(0, randDirAngle, 0) * transform.forward * Random.Range(randomStartDistance.x, randomStartDistance.y);

                NavMeshHit hit;

                Vector3 lastPoint = transform.position;
                for (int t = 0; t < checkAlongAngleCount; t++)
                {
                    startPos = startPos + startDir * t * stepSize;
                    startDir = (startPos - lastPoint).normalized;
                    if (NavMesh.SamplePosition(startPos, out hit, 10f, NavMesh.AllAreas))
                    {
                        samplePositions.Add(hit.position);
#if UNITY_EDITOR
                        if (showShapes)
                            Debug.DrawRay(hit.position, Vector3.up * 1f, Color.black);
#endif
                    }
                    lastPoint = hit.position;
                }
            }

            #region Check For Safe Position

            if (ai.InfoCurrentTarget.transform)
            {
                List<InformationSafePosition> infos = new List<InformationSafePosition>();

                rayCheckCountVertical = rayCheckCountVertical <= 0 ? 1 : rayCheckCountVertical;
                maxDistanceFromTargetForMaxConfidence = maxDistanceFromTargetForMaxConfidence <= 0 ? 1 : maxDistanceFromTargetForMaxConfidence;

                float yStep = (rayHeightMinMaxVertical.y - rayHeightMinMaxVertical.x) / (rayCheckCountVertical - 1);
                for (int i = 0; i < samplePositions.Count; i++)
                {
                    // Vertical Rays
                    Vector3 samplePosition = samplePositions[i];
                    int rayHitToSafeCount = 0;
                    for (int j = 0; j < rayCheckCountVertical; j++)
                    {
                        float height = rayHeightMinMaxVertical.x + j * yStep;
                        RaycastHit hit;
#if UNITY_EDITOR
                        if (showShapes)
                        {
                            Debug.DrawLine(samplePosition + Vector3.up * height, ai.GetCurrentTargetPos() + Vector3.up * rayToTargetHeight, Color.red);
                            Debug.DrawRay(ai.GetCurrentTargetPos() + Vector3.up * rayToTargetHeight, Vector3.up * 15f, Color.gray);
                        }
#endif
                        if (Physics.Linecast(samplePosition + Vector3.up * height, ai.GetCurrentTargetPos() + Vector3.up * rayToTargetHeight, out hit, lineCastMask))
                        {
                            if (!Checkers.IsOneOfTags(hit.transform, fightableTags) && !Checkers.isChildOf(hit.transform, ai.Transform.GetComponentsInChildren<Transform>()) && hit.transform != transform)
                                rayHitToSafeCount++;
                        }
                    }
                    if (rayHitToSafeCount >= 1)
                    {
                        float distFromSelf = Vector3.Distance(ai.Transform.position, samplePosition);
                        float distSelfConf = 1 - Mathf.Clamp01(distFromSelf / maxDistanceFromSelfForMinConfidence);

                        float distFromTarget = Vector3.Distance(ai.GetCurrentTargetPos(), samplePosition);
                        float distTargetConf = Mathf.Clamp01(distFromTarget / maxDistanceFromTargetForMaxConfidence);

                        InformationSafePosition infoSafePos = new InformationSafePosition
                            (
                            samplePosition, (rayHitToSafeCount / (float)rayCheckCountVertical),
                            distFromSelf, distSelfConf,
                            distFromTarget, distTargetConf
                            );
                        infos.Add(infoSafePos);
                    }
                }

                infos = infos.OrderByDescending(x => x.OverallConfidence).ToList();
                for (int i = 0; i < infos.Count && i < maxMemoryAddPerUpdate; i++)
                    ai.Memory.Add(infos[i]);
            }

            #endregion Check For Safe Position

            return true;
        }
    }
}