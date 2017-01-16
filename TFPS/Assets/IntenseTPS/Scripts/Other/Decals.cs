using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TagProps
{
    public string tagName;
    public bool copyAllFromOtherTag = false;
    public string otherTagName;
    public float bulletPowerDecreaser;
    public List<GameObject> bulletHoles;
    public List<GameObject> bulletHitEffects;
    public List<GameObject> bulletHitSounds;
    public List<GameObject> colliderHitSounds;
    public List<GameObject> footStepFx;
}

public class Decals : MonoBehaviour
{
    public List<TagProps> TagList;

    public GameObject getRandomFootStepFx(string tag)
    {
        TagProps list = TagList.Find(x => x.tagName == tag);
        if (list == null || list.footStepFx.Count == 0)
            return null;
        return list.footStepFx[Random.Range(0, list.footStepFx.Count)];
    }

    public void GetAllNormalShot(string tag, ref GameObject decal, ref GameObject fx, ref GameObject hitSound, ref float piercePower)
    {
        TagProps tagProps = TagList.Find(x => x.tagName == tag);
        if (tagProps == null)
            return;
        if (tagProps.copyAllFromOtherTag)
            tagProps = TagList.Find(x => x.tagName == tagProps.otherTagName);

        if (tagProps != null)
        {
            if (tagProps.bulletHitEffects.Count > 0)
                fx = tagProps.bulletHitEffects[Random.Range(0, tagProps.bulletHitEffects.Count)];
            if (tagProps.bulletHoles.Count > 0)
                decal = tagProps.bulletHoles[Random.Range(0, tagProps.bulletHoles.Count)];
            if (tagProps.bulletHitSounds.Count > 0)
                hitSound = tagProps.bulletHitSounds[Random.Range(0, tagProps.bulletHitSounds.Count)];
            piercePower = tagProps.bulletPowerDecreaser;
        }
    }
}