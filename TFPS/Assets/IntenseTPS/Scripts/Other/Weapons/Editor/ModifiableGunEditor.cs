using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModifiableGun))]
public class ModifiableGunEditor : Editor
{
    private PartHolderTypes _partHolderType;
    private Transform _positionRotationHolder;
    private string _partHolderName;
    private bool _allowEmptyPart;

    private void OnEnable()
    {
        _partHolderType = PartHolderTypes.SelectPartHolderType;
        _positionRotationHolder = null;
        _partHolderName = "Enter a name for this part holder";
    }

    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            DrawDefaultInspector();
            return;
        }

        ModifiableGun modifiableGun = (ModifiableGun)target;

        GUILayout.Label("Warning : Don't manually add parts by dropping a prefab as a child to weapon\nUse \"Add clone part to weapon\" button to add parts instead.", EditorStyles.label);

        if (modifiableGun.partHolders == null)
            modifiableGun.partHolders = new List<PartHolder>();
        GUILayout.Space(3);
        if (modifiableGun.partHolders.Count > 0)
            GUILayout.Label("CURRENT PART HOLDERS", EditorStyles.centeredGreyMiniLabel);
        else
            GUILayout.Label("NO PART HOLDER FOUND...");
        GUILayout.Space(5);

        for (int i = 0; i < modifiableGun.partHolders.Count; i++)
        {
            var item = modifiableGun.partHolders[i];
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove Part Holder", EditorStyles.miniButtonLeft))
            {
                modifiableGun.partHolders.RemoveAt(i);
                continue;
            }
            GUILayout.Label("Type:", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label(item.partHolderType.ToString(), EditorStyles.label);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Can be empty :", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label((item.allowEmpty ? "Yes" : "No"));
            GUILayout.FlexibleSpace();

            GUILayout.Label(item.name);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);

            if (item.compatibleParts == null)
                item.compatibleParts = new List<CompatiblePart>();
            if (item.compatibleParts.Count > 0)
            {
                for (int j = 0; j < item.compatibleParts.Count; j++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
                    GUILayout.Label("Compatible Part No : " + (j + 1));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();

                    var compPart = item.compatibleParts[j];

                    compPart.prefab = EditorGUILayout.ObjectField("Part Prefab", compPart.prefab, typeof(GameObject), true) as GameObject;
                    GUILayout.Space(5);

                    if (compPart.prefab)
                    {
                        if (FindObjectsOfType<GameObject>().ToList().Find(x => x == compPart.prefab) != null || !compPart.prefab.activeSelf)
                        {
                            Debug.Log("Part prefab should be dragged from projects folder and it must be active.");
                            compPart.prefab = null;
                        }
                        if (!compPart.prefab.GetComponent<GunPart>())
                        {
                            Debug.Log("This is not a gun part. (GunPart script not found)");
                            compPart.prefab = null;
                        }
                        if (compPart.prefab.GetComponent<GunPart>().partHolderType != item.partHolderType)
                        {
                            Debug.Log("Part holder types does not match...");
                            compPart.prefab = null;
                        }
                    }
                    if (compPart.prefab)
                    {
                        if (GUILayout.Button("Add clone part to weapon"))
                        {
                            modifiableGun.CreateClonePart(compPart.prefab);
                        }
                    }

                    if (GUILayout.Button("Remove compatibility", EditorStyles.miniButtonRight))
                    {
                        item.compatibleParts.RemoveAt(j);
                        EditorGUILayout.EndHorizontal();
                        continue;
                    }
                    EditorGUILayout.EndHorizontal();

                    compPart.localPos = EditorGUILayout.Vector3Field("Local Position :", compPart.localPos);
                    compPart.localRot = EditorGUILayout.Vector3Field("Local Rotation :", compPart.localRot);
                    Transform trnsCpy = null;
                    trnsCpy = EditorGUILayout.ObjectField("Copy local pos/rot from transform:", trnsCpy, typeof(Transform), true) as Transform;
                    if (trnsCpy)
                    {
                        compPart.localPos = trnsCpy.localPosition;
                        compPart.localRot = trnsCpy.localRotation.eulerAngles;
                    }
                }
            }
            else
            {
                GUILayout.Label("No compatible part found for this holder");
            }
            GUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
            GUILayout.Label("Add new compatible part");
            if (GUILayout.Button("Add compatibility", EditorStyles.miniButtonRight))
            {
                item.compatibleParts.Add(new CompatiblePart(modifiableGun.GetComponent<GunAtt>().weaponName));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);

            GUILayout.Space(5);
        }
        GUILayout.Space(15);

        GUILayout.Label("Add new part holder");
        _partHolderName = EditorGUILayout.TextField(_partHolderName);
        _partHolderType = (PartHolderTypes)EditorGUILayout.EnumPopup("Select part holder type.", _partHolderType);
        _positionRotationHolder = EditorGUILayout.ObjectField("Pos-Rot Holder", _positionRotationHolder, typeof(Transform), true) as Transform;
        _allowEmptyPart = GUILayout.Toggle(_allowEmptyPart, "Can be empty?", (GUIStyle)"Radio");
        GUILayout.Space(5);

        if (GUILayout.Button("Add", EditorStyles.miniButtonMid))
        {
            // Single type attachments
            for (int i = 0; i < modifiableGun.partHolders.Count; i++)
            {
                var item = modifiableGun.partHolders[i];
                if ((_partHolderType == PartHolderTypes.ClipHolder && _partHolderType == item.partHolderType) ||
                    (_partHolderType == PartHolderTypes.SightHolder && _partHolderType == item.partHolderType) ||
                    (_partHolderType == PartHolderTypes.BarrelHolder && _partHolderType == item.partHolderType) ||
                    (_partHolderType == PartHolderTypes.HandleHolder && _partHolderType == item.partHolderType)
                    )
                {
                    Debug.Log("You can't add more than one type of" + _partHolderType.ToString());
                    return;
                }
            }
            if (_partHolderType == PartHolderTypes.SelectPartHolderType)
            {
                Debug.Log("Select a part holder type first.");
                return;
            }
            if (!_positionRotationHolder || _partHolderName == "")
            {
                Debug.Log("Can't add... Enter a name, and drop a pos-rot holder transform");
                return;
            }
            if (FindObjectsOfType<Transform>().ToList().Find(x => x == _positionRotationHolder) != null)
            {
                modifiableGun.partHolders.Add(new PartHolder(_partHolderName, _partHolderType, _positionRotationHolder, _allowEmptyPart));
                _partHolderName = "Enter a name for this part holder";
                _partHolderType = PartHolderTypes.SelectPartHolderType;
                _positionRotationHolder = null;
                _allowEmptyPart = false;
                Debug.Log("Part Holder Successfully added.");
                return;
            }
            else
            {
                Debug.Log("Transform needs to be in scene...");
                _positionRotationHolder = null;
                return;
            }
        }
    }
}