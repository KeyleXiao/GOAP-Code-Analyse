using Actions;
using Information;
using Sensors;
using StateSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIBrain
{
	#region Get

	//获取指定的状态
	public T GetStateSystem<T>() where T : AIStateSystem
	{
		return allStateSystems.OfType<T>().FirstOrDefault();
	}

	//获取感知
	public T GetSensor<T>() where T : AISensor
	{
		return allSensors.OfType<T>().FirstOrDefault();
	}

	//获取周边系统
	public T GetSystem<T>() where T : AISystem
	{
		return allSystems.OfType<T>().FirstOrDefault();
	}

	//当前状态
	public State currentState { get; private set; }


	//当前 target 
	public InformationAlive InfoCurrentTarget { get; set; }

	//InfoCurrentTarget 如果获取不到则返回零点 ，尝试获取位置获取不到返回上一个已知位置
	public Vector3 GetCurrentTargetPos()
	{
		if (InfoCurrentTarget == null)
			return Vector3.zero;

		return InfoCurrentTarget.transform ? InfoCurrentTarget.transform.position : InfoCurrentTarget.lastKnownPosition.Value;
	}

	//获取移动的位置
	public float GetDistanceFromCurrentTarget()
	{
		if (!HaveCurrentTarget())
			return 0;
		return Vector3.Distance(InfoCurrentTarget.lastKnownPosition.Value, Transform.position);
	}

	//当前ageng的prefba被生成
	public bool HaveCurrentTarget()
	{
		if (InfoCurrentTarget == null)
			return false;
		return true;
	}

	//获取当前武器上的属性脚本
	public GunAtt GetCurrentWeaponScript()
	{
		if (CurrentWeapon)
			return CurrentWeapon.GetComponent<GunAtt>();
		return null;
	}

	//当前是否有武器
	public bool HaveCurrentWeapon()
	{
		return CurrentWeapon ? true : false;
	}

	//所有的目标
	public List<AIGoal> AllGoals { get; private set; }
	//所有的计划
	public Queue<AIAction> Plan { get; private set; }
	//当前的武器
	public Transform CurrentWeapon { get; set; }
	//寻路
	public NavMeshAgent Agent { get; private set; }

	public Transform Transform { get; private set; }
	public Animator Animator { get; private set; }

	//当前世界状态
	public StateDictionary WorldState { get; set; }
	public AIMemory Memory { get; private set; }
	public AIStateSystemAnimator stateSystemAnimator { get; set; }

	#endregion Get

	#region Public

	public List<AISensor> allSensors;
	public List<AIAction> allActions;
	public List<AISystem> allSystems;
	public List<State> allStates;
	public List<AIStateSystem> allStateSystems;
	public GlobalEvents.OnDeadHandler onDead;

	#endregion Public

	#region Private

	private AIBrainProps brain;
	private AIGoal currentGoal;
	private List<AISensorPolling> pollingSensors;
	private int currentSensorRefreshIndex = 0;
	private float _tempForceToReEvaluateGoalsTime = 0;
	private float lastPlanTime = -Mathf.Infinity;
	private Planner planner;
	private bool planPending = false;

	#endregion Private

	public AIBrain(
		List<AISensor> _sensors,
		List<AISystem> _systems,
		List<AIAction> _actions,
		List<AIGoal> _goals,
		List<State> _states,
		List<AIStateSystem> _allStateSystems,
		AIBrainProps _brainProps,
		Transform _transform,
		AIMemory _memory,
		ref GlobalEvents.OnDeadHandler _onDead
		)
	{
		allSensors = _sensors;
		allSystems = _systems;
		allActions = _actions;
		AllGoals = _goals;
		allStates = _states;
		allStateSystems = _allStateSystems;

		brain = _brainProps;
		Memory = _memory;
		Transform = _transform;
		Animator = _transform.GetComponent<Animator>();
		Agent = _transform.GetComponent<NavMeshAgent>();

		onDead = _onDead;
		_onDead += OnDead;
		pollingSensors = allSensors.OfType<AISensorPolling>().ToList();
	}

	public void OnAwake()
	{
		MakeFakeStartWorld();
		stateSystemAnimator = GetStateSystem<AIStateSystemAnimator>();

		allSensors = allSensors.Distinct().ToList();
		foreach (var sensor in allSensors)
		{
			sensor.OnStart(this);
		}
		foreach (var sensor in pollingSensors)
		{
			sensor.LastWorkedUpdateTime = -sensor.updateInterval;
		}

		allActions = allActions.Distinct().ToList();
		foreach (var action in allActions)
		{
			action.LastUsedAt = -Mathf.Infinity;
			action.OnStart(this);
		}

		allSystems = allSystems.Distinct().ToList();
		foreach (var system in allSystems)
		{
			system.LastUpdateTime = -system.updateInterval;
			system.OnStart(this);
		}

		AllGoals = AllGoals.Distinct().ToList();
		foreach (var goal in AllGoals)
		{
			goal.lastUsedAt = -Mathf.Infinity;

			//goal.priority 确保这个数值位于 priorityRange的 x与y 之间 这部分代码可以优化掉 OnStart 内部已经调用了 Mathf.Clamp
			//goal.priority = (goal.priority > goal.priorityRange.y || goal.priority < goal.priorityRange.x) ?
			//	goal.priorityRange.y : goal.priority;
			goal.OnStart(this);
		}

		allStateSystems = allStateSystems.Distinct().ToList();
		foreach (var ss in allStateSystems)
			ss.OnStart(this);
		currentState = allStates.Find(x => x.StateType == ET.StateType.Idle);

		foreach (AIAction action in allActions)
			if (!action.NeededStatesExists(allStateSystems) || !action.NeededSensorsExists(allSensors))
			{
				action.DisabledCompletely = true;
#if UNITY_EDITOR
				Debug.Log(action.ToString() + "disabled, needed sensor or statesystem couldn't be found");
#endif
			}
			else
				action.DisabledCompletely = false;
		allActions.RemoveAll(x => x.DisabledCompletely);

		//初始化一个策划
		planner = new Planner(allActions, AllGoals);

		if (stateSystemAnimator == null)
		{
			brain.stopPlanning = true;
#if UNITY_EDITOR
			Debug.Log("Stopped Planning.");
			Debug.Log("No animation system found.");
#endif
		}
	}

	public void OnStart()
	{
	}

	public void OnUpdate()
	{
		if (!brain.stopPlanning)
		{
			stateSystemAnimator.ResetInterrupt(this);

			this.Memory.Items.ToString();

			bool needToReplanBySystem = UpdateSensorsSystemsNGoalPriorities();
			bool replanned = false;
			bool actionChanged = false;

			if (needToReplanBySystem || planPending || Plan == null || Plan.Count == 0)
			{
				if (Plan == null || Plan.Count == 0)
				{
					Replan();
					replanned = true;
					lastPlanTime = Time.time;
				}
				else if (Plan != null && Plan.Peek().IsInterruptableBySystems(this))
				{
					if (Time.time - lastPlanTime > brain.minPlanInterval || planPending)
					{
						lastPlanTime = Time.time;
						replanned = true;

						// deactivate then replan next update
						if (Plan.Peek().IsActivated)
							Plan.Peek().DeActivate(this);
						currentState.ExitState(this);

						Plan.Clear();

						stateSystemAnimator.InterruptAnimation(this);
					}
				}
				planPending = false;
			}

			if (!replanned && Plan != null && Plan.Count > 0) // if replanned, wait for ExitState on this update (to completely leave action)
			{
				if (!Plan.Peek().IsActivated) // action activation
				{
					actionChanged = true;
					if (Plan.Peek().CanActivate(this))
					{
#if UNITY_EDITOR
						if (brain.actionDebug)
							brain.ShowDebugMessage("Activated Action" + Plan.Peek());
#endif
						Plan.Peek().Activate(this);
						Plan.Peek().LastUsedAt = Time.time;
						Plan.Peek().IsActivated = true;
						currentState = allStates.Find(x => x.StateType == Plan.Peek().correspondingState);
						currentState.EnterState(this);

						stateSystemAnimator.InterruptAnimation(this);
					}
					else // can't activate this action
					{
						Plan.Clear();
						currentState.ExitState(this);
					}
				}
				else if (Plan.Peek().IsActivated)
				{
					if (Plan.Peek().IsStillValid(this))
					{
						Plan.Peek().OnUpdate(this);
						if (Plan.Peek().IsCompleted(this))
						{
#if UNITY_EDITOR
							if (brain.actionDebug)
								brain.ShowDebugMessage("Completed Action" + Plan.Peek());
#endif
							actionChanged = true;
							Plan.Peek().IsActivated = false;
							Plan.Peek().GeneralPostEffects(this);
							currentState.ExitState(this);
							stateSystemAnimator.InterruptAnimation(this);

							/*if (Plan.Peek().repeatType == EnumTypes.ActionType.Repetitive)
                            {
                            }
                            else */
							if (Plan.Peek().repeatType == ET.ActionType.Once)
								Plan.Dequeue();
						}
					}
					else
					{
#if UNITY_EDITOR
						if (brain.actionDebug)
							brain.ShowDebugMessage("Deactivated Action 'Not Valid' " + Plan.Peek());
#endif
						actionChanged = true;
						Plan.Peek().IsActivated = false;
						Plan.Peek().DeActivate(this);
						Plan.Clear();
						currentState.ExitState(this);

						stateSystemAnimator.InterruptAnimation(this);
					}
				}
			}

			if (!replanned && !actionChanged)
				currentState.Update(this);
		}
		else // Debug sensors-systems
		{
			UpdateSensorsSystemsNGoalPriorities();
			currentState = allStates.Find(x => x.StateType == ET.StateType.Idle); ;
			currentState.Update(this);
		}
	}

	public void OnAnimatorIK(int layerIndex)
	{
		currentState.OnAnimatorIK(layerIndex, this);
	}

	public void OnAnimatorMove()
	{
		currentState.OnAnimatorMove(this);
	}

	private void OnDead()
	{
		if (onDead != null)
			onDead();
	}

	private bool UpdateSensorsSystemsNGoalPriorities()
	{
		int startIndex = currentSensorRefreshIndex;
		for (int i = 0; i < brain.sensorUpdatePerFrame;)
		{
			float deltaTimeSinceLastWorkedTime = Time.time - pollingSensors[currentSensorRefreshIndex].LastWorkedUpdateTime;
			if (deltaTimeSinceLastWorkedTime > pollingSensors[currentSensorRefreshIndex].updateInterval)
			{
				pollingSensors[currentSensorRefreshIndex].DeltaTimeSinceLastWork = deltaTimeSinceLastWorkedTime;

#if UNITY_EDITOR
				System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
				sw.Start();
#endif
				if (pollingSensors[currentSensorRefreshIndex].OnUpdate(this))
				{
					pollingSensors[currentSensorRefreshIndex].LastWorkedUpdateTime = Time.time;
					i++;
				}
#if UNITY_EDITOR
				if (brain.sensorDebug)
				{
					sw.Stop();
					brain.ShowDebugMessage("Updated Sensor " + pollingSensors[currentSensorRefreshIndex] + "MS:" + sw.ElapsedMilliseconds);
				}
#endif
			}

			currentSensorRefreshIndex++;
			currentSensorRefreshIndex = currentSensorRefreshIndex % pollingSensors.Count;

			if (currentSensorRefreshIndex == startIndex)
				break;
		}

		Memory.Update();

		bool needToReplanBySystems = false;
		bool needToReevaluateGoalsBySystems = false;
		for (int i = 0; i < allSystems.Count; i++)
		{
			float deltaTime = Time.time - allSystems[i].LastUpdateTime;
			if (deltaTime > allSystems[i].updateInterval)
			{
#if UNITY_EDITOR
				System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
				sw.Start();
#endif
				allSystems[i].DeltaTime = deltaTime;
				bool ntReplan = false; bool ntReevaluateGoals = false; //safety
				allSystems[i].OnUpdate(ref ntReplan, ref ntReevaluateGoals, this);
				allSystems[i].LastUpdateTime = Time.time;
				if (needToReplanBySystems == false) needToReplanBySystems = ntReplan; //safety
				if (needToReevaluateGoalsBySystems == false) needToReevaluateGoalsBySystems = ntReevaluateGoals; //safety

#if UNITY_EDITOR
				sw.Stop();
				if (brain.systemDebug)
					brain.ShowDebugMessage("Updated System " + allSystems[i] + "MS:" + sw.ElapsedTicks);
#endif
			}
		}
		bool needToReEvaluteGoals = needToReevaluateGoalsBySystems || _tempForceToReEvaluateGoalsTime < 0;

		//强制重新评估Goals 倒计时
		_tempForceToReEvaluateGoalsTime -= Time.deltaTime;

		if (needToReEvaluteGoals)
		{
			//TODO: 这里我有一个疑问 如果重新评估了Goals 之前执行的Action怎么办？
			_tempForceToReEvaluateGoalsTime = brain.forceToCheckGoalPrioritiesInterval;

			//拷贝一个旧列表
			List<AIGoal> oldList = new List<AIGoal>(AllGoals);
			foreach (AIGoal goal in AllGoals)
			{
				goal.CalculatePriority(this);//重新计算优先级
			}


			// Order goals by priority, if order is changed replan to satisfy the most important goal
			// 按优先级排序目标，如果排序变更，重新策划最重要的目标
			AllGoals = AllGoals.OrderBy(x => x.priority).ToList();

			//检测比当前优先级 还 优先的 目标 是否可用
			bool goalsBeforeApplicapable = false;
			int index = AllGoals.FindIndex(x => x == currentGoal);
			for (int i = index - 1; i >= 0; i--)
			{
				if (AllGoals[i].IsApplicapable(this))
				{
					goalsBeforeApplicapable = true;
					break;
				}
			}

			for (int i = 0; i < AllGoals.Count; i++)
			{
				//如果优先级发生改变或出现了更优先且可用的
				if (AllGoals[i] != oldList[i] || goalsBeforeApplicapable) // priority changed or current goal is not the most important
				{
					needToReplanBySystems = true;
					planPending = true;
					break;
				}
			}
		}
		bool retVal = needToReplanBySystems;

		return retVal;
	}

	private void SelectCurrentActionsState()
	{
		currentState = allStates.Find(x => x.StateType == Plan.Peek().correspondingState);
	}

	private void Replan()
	{
#if UNITY_EDITOR
		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
		sw.Start();
#endif
		foreach (AIAction action in allActions)
			action.IsActivated = false;
		Plan = planner.CalculatePlan(this, allActions, WorldState, AllGoals, out currentGoal);
#if UNITY_EDITOR
		if (brain.plannerDebug)
		{
			sw.Stop();
			brain.ShowDebugMessage("Planned, Took " + sw.ElapsedMilliseconds + " ms to calculate a plan, Goal is " + currentGoal);
		}
#endif
		//if (Plan == null || Plan.Count == 0)
		//{
		//    Plan = new Queue<AIAction>();
		//    Plan.Enqueue(*Get Default Action*); // it is better to assign a default idle action if you have one
		//}
	}

	private void MakeFakeStartWorld()
	{
		WorldState = new StateDictionary();
		WorldState.Add(DS.killTarget, false);
		WorldState.Add(DS.haveSafePosition, false);
		WorldState.Add(DS.haveWeapon, true);
		WorldState.Add(DS.haveAmmo, true);
		WorldState.Add(DS.weaponArmed, false);
		WorldState.Add(DS.weaponLoaded, true);
		WorldState.Add(DS.targetIsDead, false);
		WorldState.Add(DS.haveTarget, false);
		WorldState.Add(DS.targetLost, false);
		WorldState.Add(DS.weaponAimed, false);
		WorldState.Add(DS.dangerExplosiveExists, false);
		WorldState.Add(DS.atSafePosition, false);
		WorldState.Add(DS.aiStatus, ET.AiStatus.Unknown);
		WorldState.Add(DS.aiAlertness, ET.AiAlertLevel.Relaxed);
		WorldState.Add(DS.takingBulletDamage, false);
		WorldState.Add(DS.bulletDodged, true);
		WorldState.Add(DS.haveFriendsAround, false);
		WorldState.Add(DS.isNearCurrentTarget, false);
	}
}

[System.Serializable]
public class AIBrainProps
{
	public bool stopPlanning = false;
	public int sensorUpdatePerFrame = 1;
	public float minPlanInterval = 2.5f;

	/// <summary>
	/// 强制重新评估Goals 倒计时 2 秒
	/// </summary>
	public float forceToCheckGoalPrioritiesInterval = 2f;
#if UNITY_EDITOR

	[Space]
	public bool actionDebug = false;

	public bool sensorDebug = false;
	public bool systemDebug = false;
	public bool plannerDebug = false;

	[System.NonSerialized]
	public int msgCount = 0;

	public void ShowDebugMessage(string msg)
	{
		Debug.Log(msgCount++ + ":" + msg + "  Frame:" + Time.frameCount);
	}

#endif
}