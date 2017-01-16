using Actions;
using Sensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class AIToolMakerWindow : EditorWindow
{
    private Type type;
    private Vector2 scrollPos;
    private int tab;
    private EditorWindow window;
    private string currentSelectedPath;
    public List<Type> Types { get; set; }

    [MenuItem("Window/Intense AI/Tool Maker")]
    private static void Init()
    {
        EditorWindow.GetWindow(typeof(AIToolMakerWindow));
    }

    private void OnEnable()
    {
        window = EditorWindow.GetWindow(typeof(AIToolMakerWindow));
        EditorWindow.GetWindow(typeof(AIToolMakerWindow)).minSize = new Vector2(350, 200);
        GUIContent titleContent = new GUIContent("Tool Maker");
        window.titleContent = titleContent;
    }

    private void Update()
    {
        Repaint();
        currentSelectedPath = GetSelectedPath();
    }

    private void OnGUI()
    {
        tab = GUILayout.Toolbar(tab, new string[] { "Create Single", "Create Set" });
        currentSelectedPath = GetSelectedPath();
        switch (tab)
        {
            case 0:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Find All Scripts", EditorStyles.centeredGreyMiniLabel);
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Sensors", EditorStyles.miniButton))
                {
                    GetClasses(typeof(AISensorPolling));
                    AddClasses(typeof(AISensorRequest));
                    AddClasses(typeof(AISensorTrigger));
                    type = typeof(AISensor);
                }
                if (GUILayout.Button("Systems", EditorStyles.miniButton))
                {
                    GetClasses(typeof(AISystem));
                    type = typeof(AISystem);
                }
                if (GUILayout.Button("Actions", EditorStyles.miniButton))
                {
                    GetClasses(typeof(AIAction));
                    type = typeof(AIAction);
                }
                if (GUILayout.Button("Goals", EditorStyles.miniButton))
                {
                    GetClasses(typeof(AIGoal));
                    type = typeof(AIGoal);
                }

                GUILayout.EndHorizontal();

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(window.position.width), GUILayout.Height(window.position.height - 50));
                if (Types != null && Types.Count > 0)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("Selected save path:  " + currentSelectedPath + "/", EditorStyles.miniLabel);

                    GUILayout.Space(10);

                    EditorGUILayout.LabelField("Showing " + type.ToString() + "s -> (" + Types.Count + ")found", EditorStyles.centeredGreyMiniLabel);

                    EditorGUILayout.BeginVertical();
                    int i = 0;
                    foreach (var x in Types)
                    {
                        string typeName = x.ToString();
                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField(i + 1 + "." + typeName, EditorStyles.miniLabel);
                        if (GUILayout.Button(i + 1 + "." + "Create", GUILayout.Width(68), GUILayout.Height(15)))
                        {
                            CreateScriptableOfType(x);
                        }

                        EditorGUILayout.EndHorizontal();
                        i++;
                    }

                    GUILayout.Space(15);
                    EditorGUILayout.EndVertical();
                    if (GUILayout.Button("Create All", EditorStyles.miniButtonMid))
                    {
                        foreach (var x in Types)
                            CreateScriptableOfType(x);
                    }
                    GUILayout.Space(15);
                }

                EditorGUILayout.EndScrollView();

                break;

            case 1:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Select To Create Set", EditorStyles.centeredGreyMiniLabel);
                GUILayout.BeginVertical();

                if (GUILayout.Button("Sensor Set", EditorStyles.miniButton))
                {
                    type = typeof(AISensorSet);
                    CreateScriptableOfType(typeof(AISensorSet));
                }
                if (GUILayout.Button("System Set", EditorStyles.miniButton))
                {
                    type = typeof(AISystemSet);
                    CreateScriptableOfType(typeof(AISystemSet));
                }
                if (GUILayout.Button("Action Set", EditorStyles.miniButton))
                {
                    type = typeof(AIActionSet);
                    CreateScriptableOfType(typeof(AIActionSet));
                }
                if (GUILayout.Button("Goal Set", EditorStyles.miniButton))
                {
                    type = typeof(AIGoalSet);
                    CreateScriptableOfType(typeof(AIGoalSet));
                }

                GUILayout.EndVertical();

                GUILayout.Space(10);

                EditorGUILayout.LabelField("Selected save path:  " + currentSelectedPath + "/", EditorStyles.miniLabel);

                break;

            default:
                break;
        }

        if (GUI.changed)
            EditorUtility.SetDirty(window);
    }

    private void CreateScriptableOfType(Type type)
    {
        var asset = ScriptableObject.CreateInstance(type);

        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
            asset.GetInstanceID(),
            ScriptableObject.CreateInstance<NameEdit>(),
            string.Format("{0}.asset", type.ToString()),
            AssetPreview.GetMiniThumbnail(asset),
            null);
    }

    public static string GetSelectedPath()
    {
        string path = "Assets";

        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }

    public static void GetClasses(Type baseType)
    {
        var assembly = Assembly.Load(new AssemblyName("Assembly-CSharp"));
        var allScriptableSensors = assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(ScriptableObject)) && x.IsSubclassOf(baseType)).ToList();

        var window = EditorWindow.GetWindow<AIToolMakerWindow>(true);

        window.Types = new List<Type>(allScriptableSensors);
    }

    public static void AddClasses(Type baseType)
    {
        var assembly = Assembly.Load(new AssemblyName("Assembly-CSharp"));
        var allScriptableSensors = assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(ScriptableObject)) && x.IsSubclassOf(baseType)).ToList();

        var window = EditorWindow.GetWindow<AIToolMakerWindow>(true);

        window.Types.AddRange(allScriptableSensors);
    }
}

internal class NameEdit : UnityEditor.ProjectWindowCallback.EndNameEditAction
{
    #region implemented abstract members of EndNameEditAction

    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
    }

    #endregion implemented abstract members of EndNameEditAction
}