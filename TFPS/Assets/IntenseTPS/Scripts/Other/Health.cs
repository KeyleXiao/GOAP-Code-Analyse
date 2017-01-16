using System;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float health = 100;
    public float maxHealth = 100;
    public bool isRegenerative = false;
    public float regenerateRate = 20f;
    public Rigidbody[] rbzRagdoll;
    public Collider[] colzRagdoll;
    private Animator animator;
    private float _tempTimer;
    [NonSerialized]
    public bool isHuman;

    private Dictionary<HumanBodyBones, Transform> bodySensorRayParts;

    public Dictionary<HumanBodyBones, Transform> BodySensorRayParts
    {
        get
        {
            return this.bodySensorRayParts;
        }
    }

    private void Awake()
    {
        GetBoneTransforms();
        _tempTimer = 1;
    }

    private void Update()
    {
        if (isRegenerative)
        {
            if(health < maxHealth)
            {
                if(_tempTimer < 0)
                {
                    _tempTimer = 1;
                    health += regenerateRate;
                    health = health > maxHealth ? maxHealth : health;
                }
            }
            _tempTimer -= Time.deltaTime;
        }
    }

    public static void SwitchRagdoll(bool onOff, Rigidbody[] rbzRagdoll, Collider[] colzRagdoll)
    {
        foreach (Rigidbody rb in rbzRagdoll)
            rb.isKinematic = !onOff;
        foreach (Collider col in colzRagdoll)
            col.isTrigger = !onOff;
    }

    public void GetBoneTransforms()
    {
        isHuman = GetComponent<Animator>() && GetComponent<Animator>().isHuman;
        if (isHuman)
        {
            animator = GetComponent<Animator>();
            bodySensorRayParts = new Dictionary<HumanBodyBones, Transform>();
            foreach (HumanBodyBones humanBodyBone in Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (animator.GetBoneTransform(humanBodyBone))
                    bodySensorRayParts.Add(humanBodyBone, animator.GetBoneTransform(humanBodyBone));
            }
        }
    }
}