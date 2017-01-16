using Actions;
using Sensors;
using Shooter.StateSystems;
using StateSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class ShooterBehaviour : MonoBehaviour
{
	[SerializeField]
	private FootStepSoundsAndFx footStepFx;

	private FootPlanting footSoundFx;

	[SerializeField]
	private AIBrainProps brainProps;

	[Header("WeaponStateSystem")]
	[SerializeField]
	private FireProps fireProps;

	public WeaponHandIKProps weaponHandIKProps;

	[Header("LookStateSystem")]
	[SerializeField]
	private LookIKProps lookIKProps;

	[Header("AnimatorStateSystem")]
	public ShooterAnimatorSystemProps animatorProps;

	[Header("MovementStateSystem")]
	public ShooterMoveProps movementProps;

	[Space]
	public PatrolRoute patrolRoute;

	[NonSerialized]
	public AIBrain ai;

	[NonSerialized]// serialize to debug melee hit animation if needed
	public Transform testTransform;

	[Space]
	public AISensorSet sensorSet;

	public AISystemSet systemSet;
	public AIActionSet actionSet;
	public AIGoalSet goalSet;
	private List<State> states = new List<State>();
	private List<AIStateSystem> stateSystems = new List<AIStateSystem>();

	[Space]
	public LayerMask friendMask;

	private Health health;
	private bool isDead = false;

	private void OnEnable()
	{
		CustomSoldierSMB[] allSMBs = GetComponent<Animator>().GetBehaviours<CustomSoldierSMB>();
		for (int i = 0; i < allSMBs.Length; i++)
		{
			allSMBs[i].shooter = this;
			allSMBs[i].Init(GetComponent<Animator>());
		}
	}

	public StateSystems.AIStateSystem stateSysTest;

	private void Awake()
	{
		health = GetComponent<Health>();

		stateSystems.Add(new AIShooterStateSystemAnimator(animatorProps, GetComponent<Animator>()));
		stateSystems.Add(new AIShooterStateSystemLookAt(lookIKProps));
		stateSystems.Add(new AIShooterStateSystemMove(movementProps, patrolRoute));
		stateSystems.Add(new AIShooterStateSystemWeapon(fireProps, weaponHandIKProps));

		states.Add(new IdleState(stateSystems.ToArray()));
		states.Add(new AnimateState(stateSystems.ToArray()));
		states.Add(new MoveState(stateSystems.ToArray()));

		sensorSet.sensorList.RemoveAll(x => x == null);
		systemSet.systemList.RemoveAll(x => x == null);
		actionSet.actionList.RemoveAll(x => x == null);
		goalSet.goalList.RemoveAll(x => x == null);

		if (sensorSet == null || systemSet == null || goalSet == null || actionSet == null)
		{
#if UNITY_EDITOR
			Debug.Log("You need to fill all sets");
#endif
			this.enabled = false;
			return;
		}

		List<AISensor> cloneSensors = new List<AISensor>();
		foreach (AISensor sensor in sensorSet.sensorList)
			cloneSensors.Add(UnityEngine.Object.Instantiate(sensor) as AISensor);
		List<AIAction> cloneActions = new List<AIAction>();
		foreach (AIAction action in actionSet.actionList)
			cloneActions.Add(UnityEngine.Object.Instantiate(action) as AIAction);
		List<AISystem> cloneSystems = new List<AISystem>();
		foreach (AISystem system in systemSet.systemList)
			cloneSystems.Add(UnityEngine.Object.Instantiate(system) as AISystem);
		List<AIGoal> cloneGoals = new List<AIGoal>();
		foreach (AIGoal goal in goalSet.goalList)
			cloneGoals.Add(UnityEngine.Object.Instantiate(goal) as AIGoal);

		AIMemoryShooter memory = new AIMemoryShooter(gameObject, 150, 2, 25, friendMask);
		GetComponent<SharedProps>().memory = memory;

		//初始化大脑
		ai = new AIBrain(
			cloneSensors,
			cloneSystems,
			cloneActions,
			cloneGoals,
			states,
			stateSystems,
			brainProps,
			transform,
			memory,
			ref onAgentDead
			);

		ai.OnAwake();

		footSoundFx = new FootPlanting(footStepFx, GetComponent<Animator>());
	}

	private void Start()
	{
		Health.SwitchRagdoll(false, transform.GetComponentsInChildren<Rigidbody>(), transform.GetComponentsInChildren<Collider>());
		ai.OnStart();
	}

	private void Update()
	{
		if (health.health <= 0 && !isDead)
		{
			isDead = true;
			OnDead();
		}

		if (!isDead)
			ai.OnUpdate();
	}

	private void OnAnimatorIK(int layerIndex)
	{
		if (!isDead)
		{
			ai.OnAnimatorIK(layerIndex);
			footSoundFx.FootPlantOnAnimatorIK(layerIndex);
		}
	}

	public void OnAnimatorMove()
	{
		if (!isDead)
			ai.OnAnimatorMove();
	}

	public event GlobalEvents.OnDeadHandler onAgentDead;

	public void OnDead()
	{
		Health.SwitchRagdoll(true, health.rbzRagdoll, health.colzRagdoll);

		GetComponent<Animator>().enabled = false;
		GetComponent<NavMeshAgent>().enabled = false;
		GetComponent<Rigidbody>().isKinematic = true;
		GetComponent<Collider>().isTrigger = true;
		if (ai.CurrentWeapon) StartCoroutine(FlyingWeapon(.3f, ai.CurrentWeapon));

		if (onAgentDead != null)
			onAgentDead();

		gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
	}

	public IEnumerator FlyingWeapon(float waitFSecs, Transform weapon)
	{
		yield return new WaitForSeconds(waitFSecs);
		if (weapon)
		{
			weapon.SetParent(null);
			if (weapon.GetComponent<BoxCollider>())
			{
				weapon.GetComponent<BoxCollider>().enabled = true;
				ai.CurrentWeapon.GetComponent<BoxCollider>().isTrigger = false;
			}
			if (weapon.GetComponent<Rigidbody>())
			{
				weapon.GetComponent<Rigidbody>().isKinematic = false;
				weapon.GetComponent<Rigidbody>().useGravity = true;
			}
		}
	}

	#region Animation Embedded

	public delegate void AnimTrgFuncHandler(AIBrain ai);

	public event AnimTrgFuncHandler onNewClipInLeftHand;

	public event AnimTrgFuncHandler onReloadDone;

	public event AnimTrgFuncHandler onNewClipOffLeftHand;

	public event AnimTrgFuncHandler onIsHandOnGun;

	public event AnimTrgFuncHandler onIsHandAwayFromGun;

	public void NewClipInLeftHand()
	{
		if (onNewClipInLeftHand != null)
			onNewClipInLeftHand(ai);
	}

	public void NewClipOffLeftHand()
	{
		if (onNewClipOffLeftHand != null)
			onNewClipOffLeftHand(ai);
	}

	public void ReloadDone()
	{
		if (onReloadDone != null)
			onReloadDone(ai);
	}

	public void IsHandOnGun()
	{
		if (onIsHandOnGun != null)
			onIsHandOnGun(ai);
	}

	public void IsHandAwayFromGun()
	{
		if (onIsHandAwayFromGun != null)
			onIsHandAwayFromGun(ai);
	}

	#endregion Animation Embedded
}