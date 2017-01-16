using Information;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sensors
{
    /// <summary>
    /// Returns positions around a given <see cref="Vector3"/> on <see cref="NavMesh"/>
    /// </summary>
    public class SensorRandPosAroundGtor : AISensorRequest
    {
#if UNITY_EDITOR
        public bool showShapes = false;
#endif
        public int randPositionGenerateCount = 5;
        public Vector2 randomStartDistance = new Vector2(.15f, 1f);
        public int angleCheckCount = 3;
        public int checkAlongAngleCount = 5;
        public Vector2 stepSizeMinMax = new Vector2(2.5f, 3.5f);
        public float maxDistanceFromTargetForMaxConfidence = 25f; // min is considered 0
        public float maxDistanceFromSelfForMinConfidence = 25f; // min is considered 0

        public List<InformationRandGoToCheckPosition> RequestAllInfo(Vector3 randAroundPoint)
        {
            List<InformationRandGoToCheckPosition> list = GetRandPositions(randAroundPoint);

            if (list != null)
            {
                if (list.Count > randPositionGenerateCount)
                {
                    var retVal = new List<InformationRandGoToCheckPosition>();
                    for (int i = 0; i < randPositionGenerateCount; i++)
                    {
                        retVal.Add(list[Random.Range(0, list.Count)]);
                    }
                    return retVal;
                }
                else
                    return list;
            }
            return null;
        }

        public InformationRandGoToCheckPosition RequestInfo(Vector3 randAroundPoint)
        {
            var list = GetRandPositions(randAroundPoint);
            if (list.Count > 0)
            {
                float maxInfo = list.OrderByDescending(x => x.OverallConfidence).First().OverallConfidence;
                List<InformationRandGoToCheckPosition> infos = list.FindAll(x => x.OverallConfidence == maxInfo);

                return infos[UnityEngine.Random.Range(0, infos.Count)];
            }
            return null;
        }

        public List<InformationRandGoToCheckPosition> GetRandPositions(Vector3 randAroundPoint)
        {
            List<Vector3> samplePositions = new List<Vector3>();

            float randDirAngle = Random.Range(0, 360);

            for (int randAngleCounter = 0; randAngleCounter < angleCheckCount; randAngleCounter++)
            {
                randDirAngle += 360 / angleCheckCount;
                float randStartDist = Random.Range(randomStartDistance.x, randomStartDistance.y);

                Vector3 startPos = randAroundPoint + Quaternion.Euler(0, randDirAngle, 0) * Vector3.forward * randStartDist;
                Vector3 startDir = Quaternion.Euler(0, randDirAngle, 0) * Vector3.forward * Random.Range(randomStartDistance.x, randomStartDistance.y);

                NavMeshHit hit;

                Vector3 lastPoint = randAroundPoint;
                for (int t = 0; t < checkAlongAngleCount; t++)
                {
                    startPos = startPos + startDir * t * Random.Range(stepSizeMinMax.x, stepSizeMinMax.y);
                    startDir = (startPos - lastPoint).normalized;
                    if (NavMesh.SamplePosition(startPos, out hit, 10f, NavMesh.AllAreas))
                    {
                        samplePositions.Add(hit.position);
#if UNITY_EDITOR
                        if (showShapes)
                            Debug.DrawRay(hit.position, Vector3.up * 1f, Color.black);
#endif
                        //float dotRandAngle = Random.Range(0, 360);
                        //for (int aroundPointCounter = 0; aroundPointCounter < aroundPointCount; aroundPointCounter++) // Not used atm
                        //{
                        //    dotRandAngle += 360 * aroundPointCounter / aroundPointCount;
                        //}
                    }
                    lastPoint = hit.position;
                }
            }
            List<InformationRandGoToCheckPosition> infos = new List<InformationRandGoToCheckPosition>();
            foreach (Vector3 samplePosition in samplePositions)
            {
                float distFromSelf = 1;//Vector3.Distance(blackboard.transform.position, samplePosition);
                float distSelfConf = Mathf.Clamp01(distFromSelf / maxDistanceFromSelfForMinConfidence);

                float distFromTarget = Vector3.Distance(randAroundPoint, samplePosition);
                float distTargetConf = Mathf.Clamp01(distFromTarget / maxDistanceFromTargetForMaxConfidence);

                var infoRandAround = new InformationRandGoToCheckPosition(
                    samplePosition, 0,
                    distFromSelf, distSelfConf,
                    distFromTarget, distTargetConf
                    );
                infos.Add(infoRandAround);
            }
#if UNITY_EDITOR
            if (showShapes)
                foreach (Vector3 position in samplePositions)
                {
                    Debug.DrawRay(position, Vector3.up, Color.blue);
                }
#endif
            if (infos.Count > 0)
                return infos;
            else
                return null;
        }
    }
}