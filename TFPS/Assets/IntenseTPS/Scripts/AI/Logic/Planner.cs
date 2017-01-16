using Actions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Used to calculate an estimated valid plan, called by <see cref="AIBrain"/>
/// </summary>
public class Planner
{
	private List<AIAction> minCostPlan = new List<AIAction>();

	//private int letter = 0;

	//目标 - 目标所涉及到的行为
	private Dictionary<AIGoal, List<AIAction>> goalsToRelatedActions;

	public Planner(List<AIAction> _actions, List<AIGoal> _goals)
	{
		goalsToRelatedActions = new Dictionary<AIGoal, List<AIAction>>();
		//添加多个目标
		foreach (var goal in _goals)
			//给每个目标都添加 n条可以满足目标的行为
			goalsToRelatedActions.Add(goal, GetRelated(goal.goalStates, new List<AIAction>(), _actions));
	}


	public Queue<AIAction> CalculatePlan(AIBrain ai, List<AIAction> allActions, StateDictionary currentWorldState, List<AIGoal> allGoals, out AIGoal activeGoal)
	{

		// 挑选出当前可行的 行为然后计算消耗 加入可执行列表
		List<AIAction> applicapableActions = new List<AIAction>();
		foreach (AIAction action in allActions)
		{
			if (action.CanBeAddedToPlan(ai)) // to remove unnecessary action branch in tree
			{
				action.JustBeforePlan(ai);
				action.CalculateCost(ai); // Some actions can use dynamic cost based on confidence factors
				applicapableActions.Add(action);
			}
		}


		//按照消耗排序当前 可以执行的行为
		applicapableActions = applicapableActions.OrderBy(x => x.Cost).ToList();

		List<Node> goalMatchingNodes = new List<Node>();

		foreach (AIGoal goal in allGoals)
		{
			//跳过当前不能执行的方案
			if (!goal.IsApplicapable(ai))
			{
				goal.Applicapable = false;
				continue;
			}

			//更新目标的可执行状态
			goal.Applicapable = true;

			//最小消耗的行为列表
			minCostPlan = new List<AIAction>();

			//letter = 0;
			Node startNode = new Node(/*letter++ + ""*/);
			StateDictionary cWorldState = new StateDictionary(currentWorldState.conditions);//拷贝了一个状态
			List<AIAction> applicapableNRelatedActions = new List<AIAction>();

			// Creates paths including first lowest cost action path
			float maxCSoFar = Mathf.Infinity;


			//所有可用行为中 如果有涉及当前目标的行为 则加入列表
			foreach (var action in applicapableActions)
			{
				if (goalsToRelatedActions[goal].Contains(action))
					applicapableNRelatedActions.Add(action);
			}

			// 至此 已经按照消耗排序 并且筛选当前目标可用的行为
			// 
			CreateActionTree(/* 空节点 */startNode, /* 世界状态 */cWorldState,/* 当前目的 */ goal.goalStates, /* 当前目标牵涉到的行为 */applicapableNRelatedActions /*applicapableActions*/, goalMatchingNodes, ref maxCSoFar);


			if (minCostPlan.Count > 0)
			{
				Queue<AIAction> actionQ = new Queue<AIAction>();
				foreach (AIAction action in minCostPlan)
				{
					actionQ.Enqueue(action);
				}
				activeGoal = goal;
				goal.lastUsedAt = Time.time;
				return actionQ;
			}
			else
				continue;
		}
		activeGoal = null;
		return null;
	}

	private List<AIAction> GetRelated(StateDictionary sd, List<AIAction> relatedActionsSoFar, List<AIAction> allActions)
	{
		foreach (var kvp in sd.conditions)
		{
			foreach (var action in allActions)
			{
				foreach (var kvpPost in action.postEffects.conditions)
				{
					//找到同质化的行为(即抛出的信息相同的行为)
					if (kvpPost.Key == kvp.Key && kvpPost.Value.ToString() == kvp.Value.ToString())
						if (!relatedActionsSoFar.Contains(action))
						{
							relatedActionsSoFar.Add(action);
							GetRelated(action.preConditions, relatedActionsSoFar, allActions);
						}
				}
			}
		}
		return relatedActionsSoFar;
	}

	/// <summary>
	/// 创建行为列表
	/// </summary>
	/// <param name="root">空节点</param>
	/// <param name="cWorldState">世界状态.</param>
	/// <param name="goalState">当前目的(想要的最终改变)</param>
	/// <param name="allActions">当前目标牵涉到的行为</param>
	/// <param name="matchNodes">Match nodes.</param>
	/// <param name="minCostPlanSoFar">Minimum cost plan so far.</param>
	private void CreateActionTree(Node root, StateDictionary cWorldState, StateDictionary goalState, List<AIAction> allActions, List<Node> matchNodes, ref float minCostPlanSoFar)
	{
		foreach (AIAction action in allActions)
		{
			//当前行为的产生的消耗必须小于低于minCostPlanSoFar && 当前世界状态必须满足当前行为的前提条件
			if (root.cost + action.Cost < minCostPlanSoFar && action.CanApplyToWorld(cWorldState))
			{
				//创建一个世界状态的拷贝
				StateDictionary newWorldState = new StateDictionary(cWorldState.conditions);
				//把当前行为抛出的事件 合并(更新)到 世界状态里去
				StateDictionary.OverrideCombine(action.postEffects, newWorldState);
				//创建一个新的节点
				Node newNode = new Node(/*letter++ + "",*/ root.cost + action.Cost, root, action);

				// check to see if goal is satisfied
				//如果newWorldState包含所有的goalstate
				if (StateDictionary.ConditionsMatch(goalState, newWorldState))
				{
					matchNodes.Add(newNode);
					minCostPlanSoFar = newNode.cost;

					minCostPlan.Clear();
					Node tempNode = newNode;
					while (tempNode.parent != null)
					{
						//为什么要插入到第一个位置 ? 压入队列 从新到旧
						minCostPlan.Insert(0, tempNode.upperAction);
						tempNode = tempNode.parent;
					}

					continue;
				}
				else
				{
					//丢弃当前的行为 
					List<AIAction> newActionsList = new List<AIAction>(allActions);
					newActionsList.Remove(action);
					CreateActionTree(newNode, newWorldState, goalState, newActionsList, matchNodes, ref minCostPlanSoFar);
				}
			}
			else
				continue;
		}
		return;
	}
}