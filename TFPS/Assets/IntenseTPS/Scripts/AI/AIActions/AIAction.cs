using Sensors;
using StateSystems;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Actions
{
	public abstract class AIAction : ScriptableTool
	{
		[NonSerialized]
		public float LastUsedAt;

		private List<Type> NeededStateSystems { get; set; }
		private List<Type> NeededSensors { get; set; }

		[NonSerialized]
		public bool DisabledCompletely;

		[NonSerialized]
		public bool IsActivated;

		/// <summary>
		/// This action's cost to be used by planner
		/// </summary>
		public float cost = 1;

		public float Cost
		{
			get { return cost; }
			protected set { cost = value <= 0 ? .001f : value; } // cost must be bigger than 0 to planner to work properly
		}

		/// <summary>
		/// Used by <see cref="AIBrain"/>  | Repetitive : Repeat action after complete | Once : Move on to next action after complete
		/// </summary>
		public ET.ActionType repeatType;

		/// <summary>
		/// <see cref="Planner"/>  uses the <see cref="preConditions"/> to calculate a plan // You can override behaviours in <see cref="GeneralPostEffects(AIBrain)"/> function
		/// </summary>
		public StateDictionary preConditions = new StateDictionary();

		/// <summary>
		/// <see cref="Planner"/> uses the <see cref="postEffects"/> to calculate a plan // You can override behaviours in <see cref="GeneralPostEffects(AIBrain)"/> function
		/// </summary>
		public StateDictionary postEffects = new StateDictionary();

		/// <summary>
		/// Enum state representation of states
		/// </summary>
		[NonSerialized]
		public ET.StateType correspondingState;

		/// <summary>
		/// Called <see cref="AIBrain"/> on start
		/// </summary>
		/// <param name="ai"></param>
		virtual public void OnStart(AIBrain ai)
		{
		}

		/// <summary>
		/// You can use to add needed state systems in <see cref="OnStart(AIBrain)"/> for precaution so that it will give u a warning if needed state systems are not provided
		/// </summary>
		/// <typeparam name="T"></typeparam>
		protected void AddNeededStateSystem<T>() where T : AIStateSystem
		{
			if (NeededStateSystems == null)
				NeededStateSystems = new List<Type>();
			NeededStateSystems.Add(typeof(T));
		}

		public bool NeededStatesExists(List<AIStateSystem> allStateSystems)
		{
			if (NeededStateSystems != null)
			{
				foreach (var ss in NeededStateSystems)
				{
					if (allStateSystems.Find(x => (x.GetType() == ss || x.GetType().IsSubclassOf(ss))) == null)
					{
#if UNITY_EDITOR
						Debug.Log(ss.ToString() + "Not Found");
#endif
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// You can use to add needed sensors in <see cref="OnStart(AIBrain)"/> for precaution so that it will give u a warning if needed sensors are not provided
		/// </summary>
		/// <typeparam name="T"></typeparam>
		protected void AddNeededSensor<T>() where T : AISensor
		{
			if (NeededSensors == null)
				NeededSensors = new List<Type>();
			NeededSensors.Add(typeof(T));
		}

		public bool NeededSensorsExists(List<AISensor> allSensors)
		{
			if (NeededSensors != null)
			{
				foreach (var sensr in NeededSensors)
				{
					if (allSensors.Find(x => (x.GetType() == sensr || x.GetType().IsSubclassOf(sensr))) == null)
						return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Called by <see cref="AIBrain"/> to activate an action
		/// </summary>
		/// <param name="ai"></param>
		abstract public void Activate(AIBrain ai);

		/// <summary>
		/// Called by <see cref="AIBrain"/> every update to check if action is still valid, if not <see cref="DeActivate(AIBrain)"/> function is called
		/// </summary>
		/// <param name="ai"></param>
		/// <returns></returns>
		virtual public bool IsStillValid(AIBrain ai) { return true; }

		/// <summary>
		/// Called by <see cref="AIBrain"/> every update. Same as <see cref="IsStillValid(AIBrain)"/>, but this is called after it.
		/// </summary>
		/// <param name="ai"></param>
		virtual public void OnUpdate(AIBrain ai)
		{
		}

		/// <summary>
		/// Called by <see cref="AIBrain"/> to check if the action is completed
		/// </summary>
		/// <param name="ai"></param>
		/// <returns></returns>
		abstract public bool IsCompleted(AIBrain ai);

		/// <summary>
		/// Called by <see cref="Planner"/> to calculate an estimated valid plan
		/// 世界状态 必须要满足当前行为的前提条件
		/// </summary>
		/// <param name="worldState"></param>
		/// <returns></returns>
		virtual public bool CanApplyToWorld(StateDictionary worldState)
		{
			foreach (KeyValuePair<string, object> pair in preConditions.conditions)
			{
				//if (worldState.conditions.ContainsKey(pair.Key))
				//{
				if (worldState.conditions[pair.Key].ToString() != pair.Value.ToString())
					return false;
				//}
				//else
				//    Debug.Log("Make Sure World state contains all states");
			}
			return true;
		}

		/// <summary>
		/// Override if necessary (dynamic action cost) | Called by <see cref="Planner"/> just before calculating a plan
		/// </summary>
		virtual public void CalculateCost(AIBrain ai) { }

		/// <summary>
		/// Called by <see cref="Planner"/> just before plan calculation, same as <see cref="CalculateCost(AIBrain)"/>, but this is called earlier.
		/// </summary>
		virtual public void JustBeforePlan(AIBrain ai) { }

		/// <summary>
		/// Called by <see cref="Planner"/> just before calculating a plan(before creating action tree) to see if action will be taken into account or not
		/// </summary>
		/// <returns></returns>
		virtual public bool CanBeAddedToPlan(AIBrain ai) { return true; }

		/// <summary>
		/// Called by <see cref="AIBrain"/> just before activating this action to check if it can be activated
		/// </summary>
		/// <returns></returns>
		virtual public bool CanActivate(AIBrain ai) { return true; }

		/// <summary>
		/// Called by <see cref="AIBrain"/> after <see cref="IsCompleted(AIBrain)"/> returned true
		/// </summary>
		virtual public void GeneralPostEffects(AIBrain ai)
		{
			//Update world satae && must once
			if (repeatType != ET.ActionType.Repetitive)
				StateDictionary.OverrideCombine(postEffects, ai.WorldState);
		}

		/// <summary>
		/// Called by <see cref="AIBrain"/> when this action is not valid or more important goal is applicapable or a <see cref="AISystem"/> told brain that it should replan
		/// </summary>
		virtual public void DeActivate(AIBrain ai) { }

		/// <summary>
		/// Called by <see cref="AIBrain"/> to check if this action can be interrupted when a more important goal is applicapable or a <see cref="AISystem"/> told brain that it should replan
		/// </summary>
		/// <returns></returns>
		virtual public bool IsInterruptableBySystems(AIBrain ai) { return true; }
	}
}