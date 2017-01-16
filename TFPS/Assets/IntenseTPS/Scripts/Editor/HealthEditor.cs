using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Health))]
public class HealthEditor : Editor
{
    public Transform characterModel;
    private Health health;

    public override void OnInspectorGUI()
    {
        health = (Health)target;

        DrawDefaultInspector();

        if (!health.gameObject.activeSelf)
        {
            GUILayout.Label("Cant modify on inactive gameobject");
            return;
        }

        characterModel = EditorGUILayout.ObjectField("Drop Character Model hips here", characterModel, typeof(Transform), true) as Transform;

        if (!characterModel)
        {
            GUILayout.Label("You need to select character model root first", EditorStyles.boldLabel);
            return;
        }

        if (characterModel)
        {
            if (GUILayout.Button("Get ragdoll colliders & rigidbodys"))
            {
                health.rbzRagdoll = characterModel.GetComponentsInChildren<Rigidbody>();
                if (health.rbzRagdoll.Length == 0)
                    return;

                health.colzRagdoll = characterModel.GetComponentsInChildren<Collider>();
                if (health.colzRagdoll.Length == 0)
                    return;
            }
        }

        if (health.colzRagdoll != null)
        {
            foreach (var item in health.colzRagdoll)
            {
                if (item == null)
                {
                    health.colzRagdoll = null;
                }
            }
        }

        if (health.colzRagdoll == null)
        {
            GUILayout.Label("You need to get colliders first", EditorStyles.boldLabel);
            return;
        }
        if (health.colzRagdoll.Length == 0)
        {
            GUILayout.Label("No collider found", EditorStyles.boldLabel);
            return;
        }

        //if (GUILayout.Button("Get ragdoll colliders & rigidbodys"))
        //{
        //    health.rbzRagdoll = characterModel.GetComponentsInChildren<Rigidbody>();
        //    if (health.rbzRagdoll.Length == 0)
        //        return;

        //    health.colzRagdoll = characterModel.GetComponentsInChildren<Collider>();
        //    if (health.colzRagdoll.Length == 0)
        //        return;
        //}
        else if (GUILayout.Button("Clear all"))
        {
            if (health.colzRagdoll != null)
            {
                foreach (Collider col in health.colzRagdoll)
                {
                    if (col.gameObject.GetComponent<ApplyDamageScript>())
                        DestroyImmediate(col.gameObject.GetComponent<ApplyDamageScript>());
                }
            }

            health.rbzRagdoll = null;
            health.colzRagdoll = null;
        }
        else if (GUILayout.Button("Add apply damage script to colliders"))
        {
            if (health.colzRagdoll != null)
            {
                foreach (Collider col in health.colzRagdoll)
                {
                    if (!col.GetComponent<ApplyDamageScript>())
                        col.gameObject.AddComponent<ApplyDamageScript>();
                    ApplyDamageScript appDamageScr = col.gameObject.GetComponent<ApplyDamageScript>();
                    appDamageScr.health = health;
                }
            }
        }
        else if (GUILayout.Button("Remove all apply damage script from colliders"))
        {
            if (health.colzRagdoll != null)
            {
                foreach (Collider col in health.colzRagdoll)
                {
                    if (col.GetComponent<ApplyDamageScript>())
                        DestroyImmediate(col.GetComponent<ApplyDamageScript>());
                }
            }
        }
        else if (GUILayout.Button("Set all rigidbodys to kinematic"))
        {
            if (health.rbzRagdoll != null)
            {
                foreach (Rigidbody rb in health.rbzRagdoll)
                    rb.isKinematic = true;
            }
        }
        else if (GUILayout.Button("Set all colliders to Trigger"))
        {
            if (health.colzRagdoll != null)
            {
                foreach (Collider col in health.colzRagdoll)
                    col.isTrigger = true;
            }
        }

        if (health.colzRagdoll != null)
        {
            foreach (Collider col in health.colzRagdoll)
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.Label(col.transform.name.ToUpper(), EditorStyles.boldLabel);

                if (col.GetComponent<ApplyDamageScript>())
                {
                    if (GUILayout.Button("Remove Damage Script"))
                    {
                        DestroyImmediate(col.GetComponent<ApplyDamageScript>());
                        GUILayout.EndHorizontal();
                        continue;
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label("ApplyDamage not found", EditorStyles.label);
                    if (GUILayout.Button("Add Damage Script"))
                    {
                        col.gameObject.AddComponent<ApplyDamageScript>();
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}