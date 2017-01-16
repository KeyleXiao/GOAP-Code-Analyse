using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InGameTransformSaver))]
public class InGameTransformSaveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        InGameTransformSaver j = (InGameTransformSaver)target;

        Transform t = j.GetComponent<Transform>();

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Save to temporary"))
            {
                SaveData(t.gameObject);
            }
        }

        if (!Application.isPlaying)
        {
            if (GUILayout.Button("Load from temporary"))
            {
                LoadData(t.gameObject);
            }
        }
    }

    private string GetInstanceFileName(GameObject baseObject)
    {
        return System.IO.Path.GetTempPath() + baseObject.name + "_" + baseObject.GetInstanceID() + ".InGameTransformSaveEditor.txt";
    }

    public void SaveData(GameObject baseObject)
    {
        List<string> saveData = new List<string>();

        saveData.Add(this.GetInstanceID().ToString());

        saveData.Add(baseObject.transform.localPosition.x.ToString());
        saveData.Add(baseObject.transform.localPosition.y.ToString());
        saveData.Add(baseObject.transform.localPosition.z.ToString());

        saveData.Add(baseObject.transform.localRotation.eulerAngles.x.ToString());
        saveData.Add(baseObject.transform.localRotation.eulerAngles.y.ToString());
        saveData.Add(baseObject.transform.localRotation.eulerAngles.z.ToString());

        saveData.Add(baseObject.transform.localScale.x.ToString());
        saveData.Add(baseObject.transform.localScale.y.ToString());
        saveData.Add(baseObject.transform.localScale.z.ToString());

        System.IO.File.WriteAllLines(GetInstanceFileName(baseObject), saveData.ToArray());
    }

    public void LoadData(GameObject baseObject)
    {
        string[] lines = System.IO.File.ReadAllLines(GetInstanceFileName(baseObject));
        if (lines.Length > 0)
        {
            baseObject.transform.localPosition = new Vector3(System.Convert.ToSingle(lines[1]), System.Convert.ToSingle(lines[2]), System.Convert.ToSingle(lines[3]));
            baseObject.transform.localRotation = Quaternion.Euler(System.Convert.ToSingle(lines[4]), System.Convert.ToSingle(lines[5]), System.Convert.ToSingle(lines[6]));
            baseObject.transform.localScale = new Vector3(System.Convert.ToSingle(lines[7]), System.Convert.ToSingle(lines[8]), System.Convert.ToSingle(lines[9]));
            System.IO.File.Delete(GetInstanceFileName(baseObject));
        }
    }
}