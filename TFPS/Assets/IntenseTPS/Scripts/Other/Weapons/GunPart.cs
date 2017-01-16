using UnityEngine;

public class GunPart : MonoBehaviour
{
    public string partName;
    public string partInfo;
    public PartHolderTypes partHolderType;
    public GameObject onInstantiateAudioPrefab;
    public GameObject partPrefab;

    [HideInInspector]
    public Vector3 cloneLocalPos;

    [HideInInspector]
    public Quaternion cloneLocalRot;
}

[System.Serializable]
public class CompatibleWeaponOfPart
{
    [Header("Part Info")]
    public string compatibleWeaponName;

    public int usedPartHolderIndex;

    public Vector3 localPosition;
    public Quaternion localRotation;

    public GameObject prefab;

    public CompatibleWeaponOfPart(string _compatibleWeaponName, int _usedPartHolderIndex, Vector3 _localPos, Quaternion _localRot)
    {
        compatibleWeaponName = _compatibleWeaponName;
        usedPartHolderIndex = _usedPartHolderIndex;
        localPosition = _localPos;
        localRotation = _localRot;
    }
}

public enum PartHolderTypes
{
    SelectPartHolderType,
    Other, // Can hold Flashlight-Grips-SecondaryFire
    ClipHolder, // Can only hold clips
    SightHolder, // Can only hold Sights
    BarrelHolder, // Can only hold Barrels
    HandleHolder // Can only hold Handles
}