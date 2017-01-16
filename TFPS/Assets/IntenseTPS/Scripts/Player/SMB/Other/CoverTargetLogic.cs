using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class CoverTargetLogic : MonoBehaviour
    {
        #region Properties

        public bool HaveCoverCamera { get; private set; }
        public bool HaveCoverAround { get; private set; }

        public Vector3 CoverGroundPositionAround
        {
            get { return _coverGroundPositionAround; }
            private set { _coverGroundPositionAround = value; }
        }

        private Vector3 _coverWallNormalAround;

        public Vector3 CoverWallNormalAround
        {
            get { return _coverWallNormalAround; }
            private set { _coverWallNormalAround = value; }
        }

        public Vector3 CoverGroundPositionCamera
        {
            get { return _coverGroundPositionCamera; }
            private set { _coverGroundPositionCamera = value; }
        }

        private Vector3 _coverWallNormalCamera;

        public Vector3 CoverWallNormalCamera
        {
            get { return _coverWallNormalCamera; }
            private set { _coverWallNormalCamera = value; }
        }

        public bool StopUpdateForAroundCovers { get; set; }
        public bool StopUpdateForCameraCovers { get; set; }

        #endregion Properties

        #region Public

        public LayerMask rayMask;
        public float aroundPlRayStartUpHeight = .55f;
        public float rayLengthFromAround = 1.5f;
        public int startRayCountFromAround = 6;
        public int startRayCountFromCamera = 6;
        public float startRayRadius = .3f;

        [Space]
        public float afterHitBackDist = .1f;

        public float maxAllowedGroundNormalAngle = 25f;
        public float maxAllowedAngleVert = 60f;
        public float rayLengthFromCamera = 25f;
        public float minAllowedCameraCoverDistance = 1.5f;

        [Space]
        public List<OverlapSphereClass> overlapSpheres;

        #endregion Public

        #region Private

        private static LayerMask ray_Mask_static;
        private static Transform transform_static;
        private static Transform plTransform_static;
        private static Transform camTransform_static;
        private static List<OverlapSphereClass> overlapSpheres_static;
        private static float afterHitBackDist_static;
        private static float minCameraCoverDist_static;
        private Vector3 _coverGroundPositionAround;
        private Vector3 _coverGroundPositionCamera;

        #endregion Private

        private void Awake()
        {
            ray_Mask_static = rayMask;
            transform_static = transform;
            overlapSpheres_static = overlapSpheres;
            afterHitBackDist_static = afterHitBackDist;
            minCameraCoverDist_static = minAllowedCameraCoverDistance;

            camTransform_static = GameObject.FindGameObjectWithTag("MainCamera").transform;
            plTransform_static = GameObject.FindGameObjectWithTag("Player").transform;

            if (!camTransform_static || !plTransform_static)
            {
                Debug.Log("Needed reference not found." + ToString());
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
#if UNITY_EDITOR // If you modify any of these somewhere else, remove "#if UNITY_EDITOR"...
            // To be able to debug every value from inspector reset-update static fields
            ray_Mask_static = rayMask;
            overlapSpheres_static = overlapSpheres;
            afterHitBackDist_static = afterHitBackDist;
            minCameraCoverDist_static = minAllowedCameraCoverDistance;

#endif
            UpdateForCovers();
        }

        public bool RequestCover(Ray ray, ref Vector3 coverNormal, ref Vector3 groundHit, float rayLength, float maxAllowedAngleVert, float maxAllowedGroundNormalAngle)
        {
            List<SphereTesterClass> drawers = new List<SphereTesterClass>();
            SendRay(ray, rayLength, maxAllowedAngleVert, maxAllowedGroundNormalAngle, ref drawers);
            if (SendOverlaps(ref drawers) && ChooseCovers(ref drawers, ref coverNormal, ref groundHit))
            {
#if UNITY_EDITOR
                gizmosTester.Add(drawers[0]);
#endif
                return true;
            }
            return false;
        }

        public static float RequestCrouch(float characterHeight, float rayCount, float startHeight, float rayMaxDistance, Vector3 groundPoint, Vector3 wallNormal)
        {
            Vector3 startPos = groundPoint + Vector3.up * startHeight;

            int hitCount = 0;
            for (int i = 0; i < rayCount; i++)
            {
                Ray ray = new Ray(
                    startPos + Vector3.up * i * (characterHeight - startHeight) / rayCount,
                    -wallNormal
                    );
                if (Physics.Raycast(ray, rayMaxDistance, ray_Mask_static))
                    hitCount++;
            }

            return hitCount * 1 / rayCount;
        }

        private void UpdateForCovers()
        {
            Ray ray = new Ray(camTransform_static.position, camTransform_static.forward);

#if UNITY_EDITOR
            gizmosTester.Clear();
#endif

            // Ray checks from camera
            if (!StopUpdateForCameraCovers)
            {
                List<SphereTesterClass> drawers = new List<SphereTesterClass>();

                float step = 360 / ((float)startRayCountFromCamera);
                for (int i = 0; i < startRayCountFromCamera; i++)
                {
                    Vector3 dir = camTransform_static.transform.TransformDirection(
                        Quaternion.Euler(0, 0, i * step) * Vector3.right);
                    ray.origin = camTransform_static.position + dir * (startRayCountFromCamera == 1 ? 0 : startRayRadius);
                    SendRay(ray, rayLengthFromCamera, maxAllowedAngleVert, maxAllowedGroundNormalAngle, ref drawers);
                }
                if (SendOverlaps(ref drawers) && ChooseCovers(ref drawers, ref _coverWallNormalCamera, ref _coverGroundPositionCamera, true))
                {
#if UNITY_EDITOR
                    foreach (var item in drawers)
                        gizmosTester.Add(item);
#endif
                    HaveCoverCamera = true;

                    SendOverlaps(ref drawers);
                    ChooseCovers(ref drawers, ref _coverWallNormalCamera, ref _coverGroundPositionCamera, true);
                }
                else
                    HaveCoverCamera = false;
            }
            else
                HaveCoverCamera = false;

            // Ray checks from around player
            if (!StopUpdateForAroundCovers)
            {
                List<SphereTesterClass> drawers = new List<SphereTesterClass>();

                float step = 360 / ((float)startRayCountFromAround);
                ray.origin = plTransform_static.position + Vector3.up * aroundPlRayStartUpHeight;
                for (int i = 0; i < startRayCountFromAround; i++)
                {
                    ray.direction = Quaternion.Euler(0, step * i, 0) * plTransform_static.right;
                    SendRay(ray, rayLengthFromAround, maxAllowedAngleVert, maxAllowedGroundNormalAngle, ref drawers);
                }
                if (SendOverlaps(ref drawers) && ChooseCovers(ref drawers, ref _coverWallNormalAround, ref _coverGroundPositionAround))
                {
#if UNITY_EDITOR
                    foreach (var item in drawers)
                        gizmosTester.Add(item);
#endif
                    HaveCoverAround = true;
                }
                else
                    HaveCoverAround = false;
            }
            else
                HaveCoverAround = false;
        }

        private static bool ChooseCovers(
        ref List<SphereTesterClass> drawers, ref Vector3 coverWallNormalCamera, ref Vector3 coverGroundPositionCamera, bool isCameraCover = false
        )
        {
            if (drawers == null || drawers.Count == 0)
                return false;

            // Choose closest to player
            float minDistC = Mathf.Infinity;
            foreach (var item in drawers)
            {
                float dist = Vector3.Distance(plTransform_static.position, item.groundHit.point);
                if (isCameraCover && dist < minCameraCoverDist_static)
                    continue;
                if (dist < minDistC)
                {
                    minDistC = dist;
                    coverWallNormalCamera = item.normalXDirection;
                    coverGroundPositionCamera = item.groundHit.point;
                }
            }

            return true;
        }

        private static void SendRay(
            Ray ray, float rayLength, float maxAllowedAngleVert, float maxAllowedGroundNormalAngle,
            ref List<SphereTesterClass> drawers
            )
        {
            if (drawers == null)
                drawers = new List<SphereTesterClass>();

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayLength, ray_Mask_static))
            {
                transform_static.position = hit.point;
                transform_static.forward = hit.normal;
                Vector3 dirPointToPlWithPointY = (new Vector3(plTransform_static.position.x, hit.point.y, plTransform_static.position.z) - hit.point).normalized;

                Vector3 dirNormalrotXIgnored = Quaternion.AngleAxis(transform_static.eulerAngles.x * -1, transform_static.TransformDirection(Vector3.right))
                    * hit.normal;
                float angleVert = Vector3.Angle(dirPointToPlWithPointY, dirNormalrotXIgnored);

                if (angleVert > maxAllowedAngleVert)
                    return;

                Ray rayBFromHit = new Ray(hit.point + dirNormalrotXIgnored * afterHitBackDist_static, Vector3.down);

                RaycastHit toGroundHit;
                if (Physics.Raycast(rayBFromHit, out toGroundHit, 1.8f, ray_Mask_static) && (Vector3.Angle(Vector3.up, toGroundHit.normal) < maxAllowedGroundNormalAngle))
                {
                    drawers.Add(new SphereTesterClass());
                    drawers[drawers.Count - 1].groundHit = toGroundHit;
                    drawers[drawers.Count - 1].normalXDirection = dirNormalrotXIgnored;

                    foreach (var x in overlapSpheres_static)
                    {
                        drawers[drawers.Count - 1].overlaps.Add(x);
                    }
                }
            }
        }

        private static bool SendOverlaps
            (
             ref List<SphereTesterClass> drawers
            )
        {
            if (drawers == null)
                return false;

            for (int i = 0; i < drawers.Count; i++)
            {
                int check = 0;
                var groundHit = drawers[i];
                foreach (var sphere in groundHit.overlaps)
                {
                    Vector3 dir = Quaternion.Euler(0, sphere.angleAroundWallHit, 0) * groundHit.normalXDirection;
                    Vector3 centerOverlap = groundHit.groundHit.point + dir * sphere.sphereCenterDistFromWall + sphere.height * Vector3.up;

                    Collider[] colz = Physics.OverlapSphere(centerOverlap, sphere.sphereCastRadius, ray_Mask_static);
                    if ((sphere.shouldHit && colz.Length > 0) || (!sphere.shouldHit && colz.Length == 0))
                        check++;
                }
                if (check != groundHit.overlaps.Count)
                {
                    drawers.RemoveAt(i);
                    i--;
                }
            }
            if (drawers.Count == 0)
                return false;
            return true;
        }

#if UNITY_EDITOR
        private List<SphereTesterClass> gizmosTester = new List<SphereTesterClass>();

        private void OnDrawGizmosSelected()
        {
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawSphere(transform.position, 1);
            if (!Application.isPlaying)
                return;
            foreach (var groundHit in gizmosTester)
            {
                foreach (var sphere in groundHit.overlaps)
                {
                    // Vector3 dir = mainCam.transform.TransformDirection(
                    //Quaternion.Euler(0, 0, sphere.angleAroundWallHit) * Vector3.down);

                    Vector3 dir = Quaternion.Euler(0, sphere.angleAroundWallHit, 0) * groundHit.normalXDirection;
                    Vector3 centerOverlap = groundHit.groundHit.point + dir * sphere.sphereCenterDistFromWall + sphere.height * Vector3.up;

                    Gizmos.DrawSphere(centerOverlap, sphere.sphereCastRadius);

                    if (Physics.OverlapSphere(centerOverlap, sphere.sphereCastRadius, rayMask).Length > 0)
                    {
                        Gizmos.color = Color.green;
                        Color color = Color.green; color.a = .2f;
                        Gizmos.color = color;
                        Gizmos.DrawSphere(centerOverlap, sphere.sphereCastRadius);
                    }
                    else
                    {
                        Color color = Color.black; color.a = .2f;
                        Gizmos.color = color;
                        Gizmos.DrawSphere(centerOverlap, sphere.sphereCastRadius);
                    }
                }
            }

            //Color color1 = Color.white; color1.a = .2f;
            //Gizmos.color = color1;
            //Gizmos.DrawSphere(plTransform.position + Vector3.up * aroundPlRayStartUpHeight, aroundPlayerCheckRadius);
        }

#endif

        #region PrivateClasses

        public class SphereTesterClass
        {
            public List<OverlapSphereClass> overlaps;
            public RaycastHit groundHit;
            public Vector3 normalXDirection;

            public SphereTesterClass()
            {
                overlaps = new List<OverlapSphereClass>();
            }
        }

        [System.Serializable]
        public class OverlapSphereClass
        {
            public float sphereCenterDistFromWall = .2f;
            public float angleAroundWallHit = 45f;
            public float sphereCastRadius = .1f;
            public float height = .5f;
            public bool shouldHit = true;
        }

        #endregion PrivateClasses
    }
}