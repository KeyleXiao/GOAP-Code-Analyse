using Sensors;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShooterBehaviour))]
public class ShooterBehaviourEditor : Editor
{
    private ShooterBehaviour sb;
    private SensorVisual vs;
    private SensorHearing hr;

    private List<ScriptableTool> allScriptables;
    private List<Editor> editors;
    private List<bool> toggles;
    private bool editTools = false;

    private void OnEnable()
    {
        sb = (ShooterBehaviour)target;
        GetTools();

        if (sb.sensorSet != null)
        {
            vs = sb.sensorSet.sensorList.Find(x => x.GetType() == typeof(SensorVisual)) as SensorVisual;
            hr = sb.sensorSet.sensorList.Find(x => x.GetType() == typeof(SensorHearing)) as SensorHearing;
        }
    }

    private void GetTools()
    {
        if (allScriptables == null || allScriptables.Count == 0)
        {
            allScriptables = new List<ScriptableTool>();
            if (sb.sensorSet != null && sb.sensorSet.sensorList != null && sb.sensorSet.sensorList.Count > 0)
                foreach (var x in sb.sensorSet.sensorList)
                    allScriptables.Add(x);
            if (sb.actionSet != null && sb.actionSet.actionList != null && sb.actionSet.actionList.Count > 0)
                foreach (var x in sb.actionSet.actionList)
                    allScriptables.Add(x);
            if (sb.systemSet != null && sb.systemSet.systemList != null && sb.systemSet.systemList.Count > 0)
                foreach (var x in sb.systemSet.systemList)
                    allScriptables.Add(x);
            if (sb.goalSet != null && sb.goalSet.goalList != null && sb.goalSet.goalList.Count > 0)
                foreach (var x in sb.goalSet.goalList)
                    allScriptables.Add(x);
        }
        editors = new List<Editor>();
        toggles = new List<bool>();
    }

    public void OnSceneGUI()
    {
        #region Visual Sensor

        if (vs != null && vs.showVisionShapes)
        {
            Transform head = Checkers.FindInChilds(sb.transform, vs.headFixerTransformName);
            if (head)
                ShowVisualSensorShapes(head, vs);
        }

        #endregion Visual Sensor

        #region Hearing Sensor

        if (hr != null && hr.showHearingShapes)
        {
            Transform head = sb.transform;
            if (head)
                showHearingSensorShapes(head);
        }

        #endregion Hearing Sensor
    }

    public void showHearingSensorShapes(Transform head)
    {
        hr.highIntensityRadius = hr.highIntensityRadius > hr.maxHearingRadius ? hr.maxHearingRadius : hr.highIntensityRadius;

        Vector3 glHeadUpDir = head.transform.TransformDirection(-Vector3.right);
        Vector3 glHeadFwDir = head.transform.TransformDirection(Vector3.up);

        Handles.color = hr.color1;
        Handles.DrawSolidArc(head.position, glHeadUpDir, glHeadFwDir, 360, hr.highIntensityRadius);
        Handles.color = hr.color2;
        Handles.DrawWireArc(head.position, glHeadUpDir, glHeadFwDir, 360, hr.maxHearingRadius);
        Handles.DrawWireArc(head.position, glHeadFwDir, glHeadUpDir, 360, hr.maxHearingRadius);
        Handles.color = hr.color3;
        Handles.SphereCap(44, head.position, Quaternion.identity, hr.maxHearingRadius * 2);

        // labels
        hr.textGuiStyle.fontSize = hr.labelsFontSize;
        hr.textGuiStyle.normal.textColor = hr.labelsFontColor;
        if (hr.labelsFont) if (!hr.textGuiStyle.font) hr.textGuiStyle.font = hr.labelsFont;

        GUIContent content = new GUIContent("");
        if (Camera.current && (Vector3.Distance(Camera.current.transform.position, head.position) > hr.labelsDisableDistance)) hr.textGuiStyle.normal.textColor = new Color(0, 0, 0, 0);
        content.text = "h.a:" + hr.iLevel_a;
        Handles.Label(head.position - glHeadFwDir * (hr.highIntensityRadius - (hr.highIntensityRadius / 2)), content, hr.textGuiStyle);
        content.text = "h.b:" + hr.iLevel_b;
        Handles.Label(head.position - glHeadFwDir * (hr.maxHearingRadius - (hr.maxHearingRadius - hr.highIntensityRadius) / 2), content, hr.textGuiStyle);
    }

    public void ShowVisualSensorShapes(Transform head, SensorVisual vision)
    {
        vision.farViewDistance = vision.farViewDistance < vision.highSenseEllipseH ? vision.highSenseEllipseH : vision.farViewDistance;
        vision.innerViewAngle = vision.innerViewAngle > vision.outerViewAngle ? vision.outerViewAngle : vision.innerViewAngle;
        vision.highSenseEllipseH = vision.highSenseEllipseH < 3 ? 3 : vision.highSenseEllipseH;

        Vector3 glHeadUpDir = head.transform.TransformDirection(-Vector3.right);
        Vector3 glHeadFwDir = head.transform.TransformDirection(Vector3.up);
        Vector3 glHeadLeftDir = head.transform.TransformDirection(Vector3.forward);

        float a = vision.highSenseEllipseH / 2;
        float b = a * Mathf.Tan(vision.highSenseEllipseW / 2 * Mathf.Deg2Rad);
        float c = Mathf.Sqrt(a * a - b * b);

        Vector3 vPos = head.position;
        Vector3 vDir = head.TransformDirection(Vector3.up);
        Vector3 F1 = vPos + vDir * (a - vision.fBehindDistance - c);
        Vector3 F2 = vPos + vDir * (a - vision.fBehindDistance + c);
        Vector3 center = (F1 + F2) / 2;

        //Debug.DrawRay(F1, Vector3.up * .3f, Color.green);
        ////Handles.Label(F1, "F1");
        //Debug.DrawRay(F2, Vector3.up * .3f, Color.green);
        ////Handles.Label(F2, "F2");
        //Debug.DrawRay(vPos, Vector3.up * .3f, Color.green);
        //Handles.Label(vPos, "vPos");
        //Debug.DrawRay(center, Vector3.up * .3f, Color.white);

        // labels
        vision.textGuiStyle.fontSize = vision.labelsFontSize;
        vision.textGuiStyle.normal.textColor = vision.labelsFontColor;
        if (vision.labelsFont) if (!vision.textGuiStyle.font) vision.textGuiStyle.font = vision.labelsFont;

        GUIContent content = new GUIContent("");
        if (Camera.current && (Vector3.Distance(Camera.current.transform.position, head.position) > vision.labelsDisableDistance)) vision.textGuiStyle.normal.textColor = new Color(0, 0, 0, 0);
        content.text = "a:" + vision.iLevel_a;
        Handles.Label(center, content, vision.textGuiStyle);
        content.text = "b:" + vision.iLevel_b;
        if (vision.fBehindDistance > 0)
            Handles.Label(head.position - vDir * vision.fBehindDistance / 2, content, vision.textGuiStyle);

        Vector3 dirTemp = Quaternion.AngleAxis(-vision.innerViewAngle * 4 / 10, glHeadUpDir) * vDir;
        content.text = "c:" + vision.iLevel_cd;
        Handles.Label(head.position + dirTemp * (vision.nearViewDistance - 3), content, vision.textGuiStyle);
        dirTemp = Quaternion.AngleAxis(vision.innerViewAngle * 4 / 10, glHeadUpDir) * vDir;
        content.text = "d:" + vision.iLevel_cd;
        Handles.Label(head.position + dirTemp * (vision.nearViewDistance - 3), content, vision.textGuiStyle);
        dirTemp = Quaternion.AngleAxis(-vision.outerViewAngle * 4 / 10, glHeadUpDir) * vDir;
        content.text = "e:" + vision.iLevel_ef;
        Handles.Label(head.position + dirTemp * (vision.nearViewDistance - 3), content, vision.textGuiStyle);
        dirTemp = Quaternion.AngleAxis(vision.outerViewAngle * 4 / 10, glHeadUpDir) * vDir;
        content.text = "f:" + vision.iLevel_ef;
        Handles.Label(head.position + dirTemp * (vision.nearViewDistance - 3), content, vision.textGuiStyle);
        dirTemp = Quaternion.AngleAxis(-vision.outerViewAngle * 4 / 10, glHeadUpDir) * vDir;
        content.text = "g:" + vision.iLevel_gh;
        Handles.Label(head.position + dirTemp * (vision.farViewDistance - (vision.farViewDistance - vision.nearViewDistance) / 2), content, vision.textGuiStyle);
        dirTemp = Quaternion.AngleAxis(vision.outerViewAngle * 4 / 10, glHeadUpDir) * vDir;
        content.text = "h:" + vision.iLevel_gh;
        Handles.Label(head.position + dirTemp * (vision.farViewDistance - (vision.farViewDistance - vision.nearViewDistance) / 2), content, vision.textGuiStyle);
        content.text = "i:" + vision.iLevel_i;
        Handles.Label(head.position + glHeadFwDir * ((vision.farViewDistance - (vision.farViewDistance - vision.nearViewDistance) / 2)), content, vision.textGuiStyle);

        DrawEllipse(center + vDir * a, center - vDir * a, b, head.forward); // high intensity ellipse

        Handles.color = vision.color1;
        Handles.DrawSolidArc(vPos, head.TransformDirection(-Vector3.right), vDir, vision.outerViewAngle / 2, vision.farViewDistance);
        Handles.DrawSolidArc(vPos, -head.TransformDirection(Vector3.left), vDir, vision.outerViewAngle / 2, vision.farViewDistance);

        Handles.color = vision.color2;
        Handles.DrawSolidArc(vPos, head.TransformDirection(-Vector3.right), vDir, vision.innerViewAngle / 2, vision.nearViewDistance);
        Handles.DrawSolidArc(vPos, -head.TransformDirection(-Vector3.right), vDir, vision.innerViewAngle / 2, vision.nearViewDistance);

        Handles.color = vision.color2;
        Handles.DrawSolidArc(vPos, head.TransformDirection(-Vector3.right), vDir, vision.outerViewAngle / 2, vision.nearViewDistance);
        Handles.DrawSolidArc(vPos, -head.TransformDirection(-Vector3.right), vDir, vision.outerViewAngle / 2, vision.nearViewDistance);

        Handles.color = vision.color2;
        Handles.DrawSolidArc(vPos, head.TransformDirection(-Vector3.right), vDir, vision.innerViewAngle / 2, vision.farViewDistance);
        Handles.DrawSolidArc(vPos, -head.TransformDirection(-Vector3.right), vDir, vision.innerViewAngle / 2, vision.farViewDistance);

        // vertical view angle
        Handles.color = vision.color3;
        Handles.DrawSolidArc(vPos, glHeadLeftDir, glHeadFwDir, vision.VerticalViewAngle / 2, vision.farViewDistance);
        Handles.DrawSolidArc(vPos, glHeadLeftDir, glHeadFwDir, -vision.VerticalViewAngle / 2, vision.farViewDistance);
        Handles.color = Color.gray;
        Handles.DrawDottedLine(vPos, vPos + vDir * (vision.farViewDistance - vision.fBehindDistance), 8);
    }

    public static void DrawEllipse(Vector3 p1, Vector3 p2, float height, Vector3 up)
    {
        Quaternion quat = Quaternion.identity;
        int halfPoints = 25;
        int totalPoints = halfPoints * 2;
        Vector3[] points1 = new Vector3[halfPoints];
        Vector3[] points2 = new Vector3[halfPoints];
        //Vector3 midPoint = (p1 + ((p2 - p1)) * 0.5f);
        Vector3 tmp = Vector3.zero;
        Vector3 firstPoint = Vector3.zero;
        quat *= Quaternion.LookRotation(p2 - p1, up);
        for (int i = 0; i < totalPoints; i++)
        {
            tmp = Vector3.zero;
            // build the coordinates in arbitrary space.
            tmp.z = -Mathf.Cos(((float)i / totalPoints) * (Mathf.PI * 2)) * (Vector3.Distance(p1, p2) * 0.5f);
            tmp.y = Mathf.Sin(((float)i / totalPoints) * (Mathf.PI * 2)) * height;
            if (i == 0) firstPoint = tmp;
            tmp -= firstPoint;
            // modify the point for correct orientation.
            tmp = (quat * tmp);
            // push to the arrays (split to show outward and return halves)
            if (i < halfPoints)
            {
                points1[i] = tmp + p1;
            }
            else
            {
                points2[i - halfPoints] = tmp + p1;
            }
        }
        //draw the results.
        Handles.color = Color.black;
        Handles.DrawPolyLine(points1);
        Handles.DrawLine(points1[0], points2[points2.Length - 1]);
        Handles.DrawLine(points1[points1.Length - 1], points2[0]);
        Handles.DrawPolyLine(points2);

        //DrawPath(points1, Color.cyan, false);
        //DrawPath(points2, Color.red, false);
        //Debug.DrawLine(p1, p2, Color.black);
        //DrawCross(points1[0], 0.2f, Color.cyan);
        //DrawCross(points2[0], 0.2f, Color.red);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (vs != null)
            if (vs.showVisionShapes && Checkers.FindInChilds(sb.transform, vs.headFixerTransformName) == null)
                EditorGUILayout.LabelField("Head not found, try renaming head fixer string of vision sensor");

        // Editing All
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        if (editors.Count != allScriptables.Count)
        {
            editors.Clear();
            toggles.Clear();
            for (int i = 0; i < allScriptables.Count; i++)
            {
                editors.Add(CreateEditor(allScriptables[i]));
                toggles.Add(false);
            }
        }

        if (Application.isPlaying)
            return;
        if (GUILayout.Button("Quick Edit Tools(Permanent Asset Edit!)", EditorStyles.miniButtonMid))
            editTools = !editTools;
        if (editTools)
        {
            for (int i = 0; i < editors.Count; i++)
            {
                EditorGUI.indentLevel = 0;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(allScriptables[i].name.Split('.').Last().ToString());
                toggles[i] = EditorGUILayout.Toggle(toggles[i]);
                EditorGUILayout.EndHorizontal();

                if (toggles[i])
                {
                    EditorGUI.indentLevel = 1;
                    editors[i].DrawDefaultInspector();
                }
            }
        }
        if (GUI.changed) { EditorUtility.SetDirty(target); }
    }
}