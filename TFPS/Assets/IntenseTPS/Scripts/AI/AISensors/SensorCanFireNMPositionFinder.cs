using Information;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sensors
{
    /// <summary>
    /// Finds positions on navmesh that agent can fire from and can move to
    /// </summary>
    public class SensorCanFireNMPositionFinder : AISensorRequest
    {
#if UNITY_EDITOR
        public bool showShapes = false;
#endif
        public Vector2 dodgeAnimAnglesInBetween = new Vector2(45, 135); // left(-), right(+) (must be in between 0-180)
        public float dodgeEstimateDistance = 1.1f;

        [System.NonSerialized]
        public float _delta = .09f;

        public int angleCheckCount = 3;
        public float lineCastHeightY = .5f;
        public LayerMask lineCastMask;

        public List<InformationNMCanFirePosition> RequestAllInfo(AIBrain ai)
        {
            List<InformationNMCanFirePosition> list = GetMovePositionS(ai);
            if (list.Count > 0)
                return list;
            return null;
        }

        public InformationNMCanFirePosition RequestInfo(AIBrain ai)
        {
            List<InformationNMCanFirePosition> list = GetMovePositionS(ai);
            if (list.Count > 0)
            {
                float maxInfo = list.OrderByDescending(x => x.OverallConfidence).First().OverallConfidence;
                List<InformationNMCanFirePosition> infos = list.FindAll(x => x.OverallConfidence == maxInfo);

                return infos[UnityEngine.Random.Range(0, infos.Count)];
            }
            return null;
        }

        private List<InformationNMCanFirePosition> GetMovePositionS(AIBrain ai)
        {
            if (!ai.HaveCurrentTarget())
                return new List<InformationNMCanFirePosition>();

            angleCheckCount = angleCheckCount <= 2 ? 1 : angleCheckCount;
            float[] checkAngles = new float[angleCheckCount * 2];
            if (angleCheckCount == 1)
            {
                checkAngles[0] = (dodgeAnimAnglesInBetween.y - dodgeAnimAnglesInBetween.x) / 2;
                checkAngles[1] = -checkAngles[0];
            }
            else
            {
                float step = (dodgeAnimAnglesInBetween.y - dodgeAnimAnglesInBetween.x) / (float)(angleCheckCount - 1);
                for (int i = 0; i < angleCheckCount; i++)
                {
                    checkAngles[i] = dodgeAnimAnglesInBetween.x + step * i;
                    checkAngles[angleCheckCount * 2 - i - 1] = -checkAngles[i];
                }
            }

            NavMeshHit hit;
            RaycastHit hitRc = new RaycastHit();
            List<InformationNMCanFirePosition> list = new List<InformationNMCanFirePosition>();
            foreach (float angle in checkAngles)
            {
                Vector3 startPos = ai.Transform.position + ai.Transform.TransformDirection(Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward) * dodgeEstimateDistance;

#if UNITY_EDITOR
                if (showShapes)
                {
                    bool hitb = NavMesh.SamplePosition(startPos, out hit, .1f, NavMesh.AllAreas);
                    hitb = !hitb; // dont give me warning
                    Debug.DrawLine(hit.position + Vector3.up * lineCastHeightY, ai.GetCurrentTargetPos() + Vector3.up * lineCastHeightY, Color.black);
                }
#endif

                if (NavMesh.SamplePosition(startPos, out hit, .1f, NavMesh.AllAreas) && hit.hit &&
                    Vector3.Distance(hit.position, ai.Transform.position) < (dodgeEstimateDistance + _delta) &&
                    Physics.Linecast(hit.position + Vector3.up * lineCastHeightY, ai.GetCurrentTargetPos() + Vector3.up * lineCastHeightY, out hitRc, lineCastMask) &&
                    hitRc.transform == ai.InfoCurrentTarget.transform // need to be able to keep shooting
                    )
                {
#if UNITY_EDITOR
                    if (showShapes)
                        Debug.DrawRay(hit.position, Vector3.up, Color.green);
#endif
                    list.Add(new InformationNMCanFirePosition(angle, 1, hit.position, 1));
                }
            }

            return list;
        }
    }
}