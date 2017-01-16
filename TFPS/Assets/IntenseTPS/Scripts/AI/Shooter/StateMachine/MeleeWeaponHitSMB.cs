using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to modify hit animations by normalized time of animation
/// </summary>
[System.Serializable]
public class MeleeHit
{
    // Note that unity state machine behaviour serialization don't work as expected in 'play mode'
    public float lookSpeed = 1f;

    public float lookBackSpeed = 1.5f;
    public Vector2 lookInBetweenNTime = new Vector2(.3f, .5f);
    public float bodyWeight = .5f, headWeight = .1f, clampWeight = .7f;
    public float lookUpDist = 1f;
    public Vector2 damageApplyInBwNTime = new Vector2(.3f, .8f);
    public bool useCheatTurn = true;
    public Vector2 cheatInBetweenNTime = new Vector2(.2f, .7f);
    public float meleeHitParameter;
    public Vector3 rHandOverlapSpherePosFix;
    public float overlapSphereRadius = .3f;
    public float maxHorizontalAngleToLookAt = 120f;
}

public class MeleeWeaponHitSMB : CustomSoldierSMB
{
    public List<MeleeHit> meleeHitAnimations;

    private Vector3 lookPos = Vector3.zero;
    private float lookAim = 0;
    private float normalizeTime;
    private MeleeHit mHit;
    private Transform rHHParent;
    private Vector3 spherePos;
    private float horAngle;

    public bool testMode = false;

    public override void Init(Animator anim)
    {
        Transform[] childs = shooter.transform.GetComponentsInChildren<Transform>();
        foreach (Transform child in childs)
            if (child.CompareTag("RightHandHold"))
            {
                rHHParent = child;
                break;
            }
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        int hitF = Random.Range(0, meleeHitAnimations.Count);
        mHit = meleeHitAnimations[hitF];
        animator.SetFloat("MeleeHit", mHit.meleeHitParameter);
        lookAim = 0;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        spherePos = rHHParent.TransformPoint(mHit.rHandOverlapSpherePosFix);
        if (testMode)
        {
            Debug.DrawRay(spherePos, rHHParent.right * mHit.overlapSphereRadius, Color.red);
            Debug.DrawRay(spherePos, rHHParent.up * mHit.overlapSphereRadius, Color.green);
            Debug.DrawRay(spherePos, rHHParent.forward * mHit.overlapSphereRadius, Color.blue);
        }

        if (shooter.testTransform && testMode)
        {
            lookPos = shooter.testTransform.position + Vector3.up * mHit.lookUpDist;
            Vector3 noYTargetPos = shooter.testTransform.position;
            noYTargetPos.y = 0;
            Vector3 dir = (noYTargetPos - shooter.transform.position).normalized;
            horAngle = Vector3.Angle(shooter.transform.forward, dir) * Mathf.Sign(Vector3.Dot(dir, shooter.transform.forward));
        }
        else if (shooter.ai.HaveCurrentTarget())
        {
            lookPos = shooter.ai.GetCurrentTargetPos() + Vector3.up * mHit.lookUpDist;
            Vector3 noYTargetPos = shooter.ai.GetCurrentTargetPos();
            noYTargetPos.y = 0;
            Vector3 dir = (noYTargetPos - shooter.transform.position).normalized;
            horAngle = Vector3.Angle(shooter.transform.forward, dir) * Mathf.Sign(Vector3.Dot(dir, shooter.transform.forward));
        }

        if (mHit.useCheatTurn)
        {
            if (normalizeTime > mHit.cheatInBetweenNTime.x && normalizeTime < mHit.cheatInBetweenNTime.y)
            {
                shooter.ai.GetStateSystem<StateSystems.AIStateSystemMove>().SetTurnToPosNStartTurn(shooter.ai, ET.TurnToType.ToPosition,
                    testMode ? shooter.testTransform.position : shooter.ai.GetCurrentTargetPos());
            }
            else
                shooter.ai.GetStateSystem<StateSystems.AIStateSystemMove>().StopTurning(shooter.ai);
        }

        if (normalizeTime > mHit.damageApplyInBwNTime.x && normalizeTime < mHit.damageApplyInBwNTime.y)
        {
            //Collider cols = Physics.OverlapSphere() // Applying damage. // not finished atm
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!testMode)
            animator.SetFloat("MeleeHit", 0);
        shooter.ai.GetStateSystem<StateSystems.AIStateSystemMove>().StopTurning(shooter.ai);
    }

    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetLookAtPosition(lookPos + Vector3.up);
        normalizeTime = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f);

        if (normalizeTime > mHit.lookInBetweenNTime.x && normalizeTime < mHit.lookInBetweenNTime.y &&
            Mathf.Abs(horAngle) < mHit.maxHorizontalAngleToLookAt
            )
            lookAim = Mathf.Lerp(lookAim, 1, Time.deltaTime * mHit.lookSpeed);
        else
            lookAim = Mathf.Lerp(lookAim, 0, Time.deltaTime * mHit.lookBackSpeed);
        animator.SetLookAtWeight(lookAim, mHit.bodyWeight, mHit.headWeight, .5f, mHit.clampWeight);
    }
}