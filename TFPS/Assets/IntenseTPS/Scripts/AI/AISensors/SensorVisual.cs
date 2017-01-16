using Information;
using System.Collections.Generic;
using UnityEngine;

namespace Sensors
{
    /// <summary>
    /// Polling vision sensor which can classify visible objects that has a collider and can determine vision amount
    /// </summary>
    public class SensorVisual : AISensorPolling
    {
#if UNITY_EDITOR
        public bool showVisionShapes = true;

        [HideInInspector]
        public GUIStyle textGuiStyle = new GUIStyle();

        public Color color1 = Color.blue;
        public Color color2 = Color.green;
        public Color color3 = Color.green;
        public int labelsFontSize = 15;
        public Color labelsFontColor = Color.black;
        public float labelsDisableDistance = 80;
        public Font labelsFont;
#endif

        public List<string> fightableTags;

        public List<string> grenadeTags;

        public List<string> explosionTags;

        public LayerMask detectableColliderLayers;
        public LayerMask humanoidBodyPartsLayers;
        public float highSenseEllipseW = 54;
        public float highSenseEllipseH = 15;
        public float farViewDistance = 50;
        public float nearViewDistance = 19;
        public float fBehindDistance = 2;

        [Range(0, 180)]
        public float outerViewAngle = 160;

        [Range(0, 135)]
        public float innerViewAngle = 100;

        [Range(0, 180)]
        public float VerticalViewAngle = 145;

        public string headFixerTransformName = "SensorRotFixer";
        public Transform Head { get; private set; }
        public float iLevel_a = 100; // identify level
        public float iLevel_b = 90;
        public float iLevel_cd = 90;
        public float iLevel_ef = 35;
        public float iLevel_gh = 5;
        public float iLevel_i = 35;
        public bool useFalloff = true; // apply falloff outside of ellipse according to distance and-or angle with head/eyes
        public List<HumanoidRayChecks> humanoidRayChecks;
        public bool useMidAlignedRayForArmsLegs = true;

        private float ignoreFightablePercentageLesserThan = .01f;

        public override void OnStart(AIBrain ai)
        {
            Head = Checkers.FindInChilds(ai.Transform, headFixerTransformName);
            if (!Head)
                Debug.Log("HEAD NOT FOUND ON VISUAL SENSOR OF " + ai.Transform.name);
        }

        public override bool OnUpdate(AIBrain ai)
        {
            bool sensorUpdated = false;
            Collider[] cols = Physics.OverlapSphere(Head.position, farViewDistance, detectableColliderLayers);
            if (cols.Length > 0)
                sensorUpdated = true;

            foreach (Collider col in cols)
            {
                #region HumanoidAlive Fightable

                if (col.GetComponent<Animator>() && col.GetComponent<Health>() && col.GetComponent<Health>().isHuman && Checkers.IsOneOfTags(col.transform, fightableTags))
                {
                    var visibleBonePositions = new List<Attribute<Vector3>>();
                    float visionPerc = GetHumanoidVisibilityPercentage(col.transform, ref visibleBonePositions);
                    if (visionPerc * 100 > ignoreFightablePercentageLesserThan && visibleBonePositions.Count > 0)
                    {
                        InformationAliveHuman infoHFightable = ai.Memory.GetAliveWithTransform<InformationAliveHuman>(col.transform);
                        if (infoHFightable == null)
                        {
                            infoHFightable = new InformationAliveHuman
                            (
                                col.transform.ToString(),
                                col.transform,
                                col.transform.position,
                                visionPerc,
                                col.transform.GetComponent<Health>().health,
                                col.transform.GetComponent<Health>().health / col.transform.GetComponent<Health>().maxHealth,
                                visibleBonePositions
                            );
                            ai.Memory.Add(infoHFightable);
                        }
                        else
                        {
                            infoHFightable.Update
                            (
                                col.transform.position,
                                visionPerc,
                                col.transform.GetComponent<Health>().health,
                                col.transform.GetComponent<Health>().health / col.transform.GetComponent<Health>().maxHealth,
                                visibleBonePositions
                            );
                        }
                    }
                }

                #endregion HumanoidAlive Fightable

                #region General OtherAlive Information

                else if (Checkers.IsOneOfTags(col.transform, fightableTags))
                {
                    bool haveVision = false;
                    RaycastHit hit;
                    if (
                        Physics.Linecast(Head.position, col.transform.position, out hit, detectableColliderLayers) &&
                        hit.transform == col.transform
                        )
                    {
                        haveVision = true;
                    }
                    if (
                        col.GetComponent<Health>()
                        )
                    {
                        float visionPerc = haveVision ? GetVisibilityPercentage(col.transform.position) : 0;
                        InformationAlive infoFtbl = ai.Memory.GetAliveWithTransform<InformationAliveOther>(col.transform);

                        if (infoFtbl == null) // create new
                        {
                            infoFtbl = new InformationAliveOther
                                (
                                    col.transform.ToString(),
                                    col.transform,
                                    col.transform.position,
                                    visionPerc,
                                    col.transform.GetComponent<Health>().health,
                                    col.transform.GetComponent<Health>().health / col.transform.GetComponent<Health>().maxHealth
                                );
                            ai.Memory.Add(infoFtbl);
                        }
                        else // update
                        {
                            infoFtbl.Update
                                (
                                    col.transform.position,
                                    visionPerc,
                                    col.transform.GetComponent<Health>().health,
                                    col.transform.GetComponent<Health>().health / col.transform.GetComponent<Health>().maxHealth
                                );
                        }
                    }
                }

                #endregion General OtherAlive Information

                #region Grenade Information

                else if (Checkers.IsOneOfTags(col.transform, grenadeTags))
                {
                    bool haveVision = false;
                    RaycastHit hit;
                    if (
                        Physics.Linecast(Head.position, col.transform.position, out hit, detectableColliderLayers) &&
                        hit.transform == col.transform
                        )
                    {
                        haveVision = true;
                    }
                    float visionPerc = haveVision ? GetVisibilityPercentage(col.transform.position) : 0;
                    InformationIncomingGrenade infoIncomingGrenade = ai.Memory.GetDangerWithTransform<InformationIncomingGrenade>(col.transform);

                    // Distance Confidence
                    float distanceFromGrenade = Vector3.Distance(ai.Memory.GameObject.transform.position, col.transform.position);
                    float distanceConfidence = 1 - distanceFromGrenade / 15f;
                    distanceConfidence = Mathf.Clamp01(distanceConfidence);

                    // Direction with velocity Confidence
                    Vector3 dir = Vector3.zero; float dirConfidence = 0;
                    if (col.GetComponent<Rigidbody>())
                    {
                        dir = col.GetComponent<Rigidbody>().velocity.normalized;
                        dirConfidence = 1 - Vector3.Angle((Head.position - col.transform.position).normalized, dir) / 180;
                    }

                    if (infoIncomingGrenade == null) // create new
                    {
                        infoIncomingGrenade = new InformationIncomingGrenade
                            (
                                col.transform.ToString(),
                                col.transform,
                                col.transform.position,
                                visionPerc,
                                dir,
                                dirConfidence,
                                distanceFromGrenade,
                                distanceConfidence
                            );
                        ai.Memory.Add(infoIncomingGrenade);
                    }
                    else // update
                    {
                        infoIncomingGrenade.Update
                            (
                                col.transform.position,
                                visionPerc,
                                dir,
                                dirConfidence,
                                distanceFromGrenade,
                                distanceConfidence
                            );
                    }
                }

                #endregion Grenade Information

                #region Explosion Information

                else if (Checkers.IsOneOfTags(col.transform, explosionTags))
                {
                    bool haveVision = false;
                    RaycastHit hit;
                    if (
                        Physics.Linecast(Head.position, col.transform.position, out hit, detectableColliderLayers) &&
                        hit.transform == col.transform
                        )
                    {
                        haveVision = true;
                    }
                    float visionPerc = haveVision ? GetVisibilityPercentage(col.transform.position) : 0;
                    InformationExplosion infoExplosion = ai.Memory.GetDangerWithTransform<InformationExplosion>(col.transform);

                    // Distance Confidence
                    float distanceFromGrenade = Vector3.Distance(ai.Memory.GameObject.transform.position, col.transform.position);
                    float distanceConfidence = 1 - distanceFromGrenade / 15f;
                    distanceConfidence = Mathf.Clamp01(distanceConfidence);

                    if (infoExplosion == null) // create new
                    {
                        infoExplosion = new InformationExplosion
                            (
                                col.transform.ToString(),
                                col.transform,
                                col.transform.position,
                                visionPerc,
                                distanceFromGrenade,
                                distanceConfidence
                            );
                        ai.Memory.Add(infoExplosion);
                    }
                    else // update
                    {
                        infoExplosion.Update
                            (
                                col.transform.position,
                                visionPerc,
                                distanceFromGrenade,
                                distanceConfidence
                            );
                    }
                }

                #endregion Explosion Information
            }

            return sensorUpdated;
        }

        public float GetHumanoidVisibilityPercentage(Transform target, ref List<Attribute<Vector3>> visibleBonePositions)
        {
            // Humanoid and ragdoll rigged enemy visibility percentage check
            Transform[] allChilds = target.GetComponentsInChildren<Transform>();
            Health health = target.GetComponent<Health>();
            RaycastHit hit;

            float sum = 0;
            foreach (HumanoidRayChecks humanPartRayCheck in humanoidRayChecks)
                if (humanPartRayCheck.check)
                    sum += humanPartRayCheck.visionMultiplier;
            float k = 1 / (sum == 0 ? .001f : sum);
            sum = 0;
            foreach (HumanoidRayChecks humanPartRayCheck in humanoidRayChecks)
            {
                if (humanPartRayCheck.check && health.BodySensorRayParts.ContainsKey(humanPartRayCheck.bone))
                {
                    Vector3 toPos = health.BodySensorRayParts[humanPartRayCheck.bone].position;
                    toPos = useMidAlignedRayForArmsLegs ? ModForMiddleOfBodyPart(humanPartRayCheck.bone, toPos, health.BodySensorRayParts) : toPos;
                    if (Physics.Linecast(Head.position, toPos, out hit, humanoidBodyPartsLayers) && Checkers.isChildOf(hit.transform, allChilds)/* && hit.transform == health.BodySensorRayParts[humanPartRayCheck.bone]*/)
                    {
                        var newPos = new Attribute<Vector3>(); newPos.Set(toPos, humanPartRayCheck.hitEasiness);
                        visibleBonePositions.Add(newPos);
                        sum += k * humanPartRayCheck.visionMultiplier * (GetVisibilityPercentage(toPos) / 100);
                    }
                }
            }
            return sum;
        }

        public float GetVisibilityPercentage(Vector3 position)
        {
            float retVal = 0;
            // is target back of player
            Vector3 glHeadFwDir = Head.transform.TransformDirection(Vector3.up);
            Vector3 targetToThisDir = (position - Head.position).normalized;
            bool isTargetBack = Vector3.Dot(targetToThisDir, glHeadFwDir) < 0 ? true : false;
            float toEyeDistance = Vector3.Distance(Head.position, position);
            // Determine the horizontal angle
            Vector3 toTargetDirWithThisY = (new Vector3(position.x, Head.position.y, position.z) - Head.position).normalized;
            float horizontalAngle = Vector3.Angle(Head.transform.TransformDirection(Vector3.up), toTargetDirWithThisY);
            float verticalAngle = Vector3.Angle(-Head.position + position, toTargetDirWithThisY);

            // if is target in the ellipse
            bool isInEllipse = IsTargetInsideEllipse(position);

            if (isInEllipse && verticalAngle <= VerticalViewAngle / 2)
            {
                if (isTargetBack)
                    retVal = iLevel_b;
                else if (!isTargetBack)
                    retVal = iLevel_a;
            }
            else if (isTargetBack) // assuming outer and inner view angles can't be bigger than 180
                retVal = 0;
            else
            {
                if (verticalAngle > VerticalViewAngle / 2 || horizontalAngle > outerViewAngle / 2 || toEyeDistance > farViewDistance)
                    retVal = 0;
                else if (verticalAngle <= VerticalViewAngle / 2)
                {
                    if (horizontalAngle <= outerViewAngle / 2)
                    {
                        if (!useFalloff)
                        {
                            if (horizontalAngle <= innerViewAngle / 2 && toEyeDistance < nearViewDistance)
                                retVal = iLevel_cd;
                            else if (horizontalAngle <= innerViewAngle / 2 && toEyeDistance > nearViewDistance)
                                retVal = iLevel_i;
                            else if (horizontalAngle > innerViewAngle / 2 && toEyeDistance < nearViewDistance)
                                retVal = iLevel_ef;
                            else if (horizontalAngle > innerViewAngle / 2 && toEyeDistance > nearViewDistance)
                                retVal = iLevel_gh;
                        }
                        else
                        {
                            if (horizontalAngle <= innerViewAngle / 2 && toEyeDistance < nearViewDistance) // falloff with angle ( near distance-inner circle )
                                retVal = iLevel_cd - ((iLevel_cd - iLevel_ef) * (horizontalAngle) / (innerViewAngle / 2));
                            else if (horizontalAngle <= innerViewAngle / 2 && toEyeDistance > nearViewDistance)  // falloff with angle+distance ( far distance-outer circle )
                            {
                                float distFoff = iLevel_i - ((iLevel_i) * (toEyeDistance - nearViewDistance) / (farViewDistance - nearViewDistance));
                                retVal = distFoff - ((distFoff - iLevel_gh) * (horizontalAngle) / (innerViewAngle / 2));
                            }
                            else if (horizontalAngle > innerViewAngle / 2 && toEyeDistance < nearViewDistance) // falloff with angle ( near distance-inner circle )
                                retVal = iLevel_ef - ((iLevel_ef) * (horizontalAngle - innerViewAngle / 2) / (outerViewAngle / 2 - innerViewAngle / 2));
                            else if (horizontalAngle > innerViewAngle / 2 && toEyeDistance > nearViewDistance) // falloff with angle+distance ( far distance-outer circle )
                            {
                                float distFoff = iLevel_gh - ((iLevel_gh) * (toEyeDistance - nearViewDistance) / (farViewDistance - nearViewDistance));
                                retVal = distFoff - ((distFoff) * (horizontalAngle - innerViewAngle / 2) / (outerViewAngle / 2 - innerViewAngle / 2));
                            }
                        }
                    }
                }
            }
            return retVal;
        }

        public Vector3 ModForMiddleOfBodyPart(HumanBodyBones boneEnum, Vector3 pos, Dictionary<HumanBodyBones, Transform> dict)
        {
            Vector3 retVal = pos;
            switch (boneEnum)
            {
                //case HumanBodyBones.Hips:
                //    break;
                //case HumanBodyBones.LeftUpperLeg:
                //    break;
                //case HumanBodyBones.RightUpperLeg:
                //    break;
                case HumanBodyBones.LeftLowerLeg:
                    retVal = (pos + ((dict[HumanBodyBones.LeftFoot]) ? dict[HumanBodyBones.LeftFoot].position : pos)) / 2;
                    break;

                case HumanBodyBones.RightLowerLeg:
                    retVal = (pos + ((dict[HumanBodyBones.RightFoot]) ? dict[HumanBodyBones.RightFoot].position : pos)) / 2;
                    break;

                case HumanBodyBones.Spine:
                    retVal = (pos + ((dict[HumanBodyBones.Head]) ? dict[HumanBodyBones.Head].position : pos)) / 2;
                    break;
                //case HumanBodyBones.Chest:
                //    break;
                case HumanBodyBones.LeftUpperArm:
                    retVal = (pos + ((dict[HumanBodyBones.LeftLowerArm]) ? dict[HumanBodyBones.LeftLowerArm].position : pos)) / 2;
                    break;

                case HumanBodyBones.RightUpperArm:
                    retVal = (pos + ((dict[HumanBodyBones.RightLowerArm]) ? dict[HumanBodyBones.RightLowerArm].position : pos)) / 2;
                    break;

                case HumanBodyBones.LeftLowerArm:
                    retVal = (pos + ((dict[HumanBodyBones.LeftHand]) ? dict[HumanBodyBones.LeftHand].position : pos)) / 2;
                    break;

                case HumanBodyBones.RightLowerArm:
                    retVal = (pos + ((dict[HumanBodyBones.RightHand]) ? dict[HumanBodyBones.RightHand].position : pos)) / 2;
                    break;

                default:
                    break;
            }
            return retVal;
        }

        private bool IsTargetInsideEllipse(Vector3 position)
        {
            Vector3 glHeadUpDir = Head.transform.TransformDirection(-Vector3.right);
            Vector3 glHeadFwDir = Head.transform.TransformDirection(Vector3.up);
            Vector3 glHeadLeftDir = Head.transform.TransformDirection(Vector3.forward);

            float a = highSenseEllipseH / 2;
            float b = a * Mathf.Tan(highSenseEllipseW / 2 * Mathf.Deg2Rad);
            float c = Mathf.Sqrt(a * a - b * b);

            Vector3 vPos = Head.position;
            float angle01 = Vector3.Angle((-Head.position + new Vector3(position.x, Head.position.y, position.z)),
                -Head.position + position);
            Vector3 targetToThisDir = (position - Head.position).normalized;
            float dot = Mathf.Sign(Vector3.Dot(targetToThisDir, glHeadUpDir));
            Vector3 vDir = (Quaternion.AngleAxis(dot * angle01, glHeadLeftDir) * glHeadFwDir);
            Vector3 F1 = vPos + vDir * (a - fBehindDistance - c);
            Vector3 F2 = vPos + vDir * (a - fBehindDistance + c);
            float distFromPoints = Vector3.Distance(position, F1) + Vector3.Distance(position, F2);
            if (distFromPoints < 2 * a)
                return true;
            return false;
        }
    }

    [System.Serializable]
    public class HumanoidRayChecks
    {
        public bool check = true;
        public HumanBodyBones bone;

        [Range(0, 1)]
        public float visionMultiplier = 1;

        [Range(0, 1)]
        public float hitEasiness = 1; // collider width can be a parameter
    }
}