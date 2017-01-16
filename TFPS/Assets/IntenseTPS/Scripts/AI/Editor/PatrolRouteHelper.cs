using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PatrolRoute))]
public class PatrolRouteHelper : Editor
{
    private void OnEnable()
    {
        PatrolRoute patrolRoute = target as PatrolRoute;
        if (patrolRoute.patrolPoints == null)
            patrolRoute.patrolPoints = new List<Vector3>();
    }

    private void OnSceneGUI()
    {
        PatrolRoute patrolRoute = target as PatrolRoute;

        if (patrolRoute.patrolPoints.Count == 0)
            return;

        for (int i = 0; i < patrolRoute.patrolPoints.Count; i++)
        {
            Vector3 point = patrolRoute.patrolPoints[i];
            point = Handles.DoPositionHandle(point, Quaternion.identity);
            patrolRoute.patrolPoints[i] = Handles.PositionHandle(point, Quaternion.identity);

            Handles.color = Color.green;
            Handles.DrawWireDisc(point, Vector3.up, 1);

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }
    }

    public override void OnInspectorGUI()
    {
        PatrolRoute patrolRoute = target as PatrolRoute;

        if (patrolRoute.patrolPoints.Count == 0)
        {
            if (GUILayout.Button("Add Patrol Point") && patrolRoute.patrolPoints.Count == 0)
                patrolRoute.patrolPoints.Add(patrolRoute.transform.position + Vector3.right * 2);
            return;
        }

        for (int i = 0; i < patrolRoute.patrolPoints.Count; i++)
        {
            GUILayout.Label(patrolRoute.patrolPoints[i] + "");
            if (GUILayout.Button("Remove Point " + (i + 1), GUILayout.Width(150)))
            {
                patrolRoute.patrolPoints.RemoveAt(i);
            }
        }
        GUILayout.Space(10);

        if (GUILayout.Button("Add Patrol Point"))
            patrolRoute.patrolPoints.Add(patrolRoute.transform.position + Vector3.right * 2);

        GUILayout.Space(10);

        if (GUILayout.Button("Stick Points to Ground"))
            patrolRoute.StickToGround();

        GUILayout.Space(20);

        if (GUILayout.Button("Remove All Points"))
            patrolRoute.patrolPoints.Clear();

        EditorUtility.SetDirty(target);
    }
}