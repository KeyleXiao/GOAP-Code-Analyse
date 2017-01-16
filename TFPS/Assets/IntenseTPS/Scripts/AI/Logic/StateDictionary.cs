using Actions;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to hold world state and preconditions and post effects
/// </summary>
public class StateDictionary
{
	public Dictionary<string, object> conditions;


	//比较比较两组状态 是否一样
	public static bool IsEqual(StateDictionary a, StateDictionary b)
	{
		if (a.conditions.Keys.Count != b.conditions.Keys.Count)
			return false;
		int equalCount = 0;
		foreach (KeyValuePair<string, object> aPair in a.conditions)
		{
			foreach (KeyValuePair<string, object> bPair in b.conditions)
			{
				if (aPair.Key == bPair.Key && aPair.Value == bPair.Value)
					equalCount++;
			}
		}
		return equalCount == a.conditions.Keys.Count;
	}

	//创建一组状态  默认构造
	public StateDictionary()
	{
		conditions = new Dictionary<string, object>();
	}

	//copy一组构造 深拷贝
	public StateDictionary(Dictionary<string, object> dict)
	{
		conditions = new Dictionary<string, object>(dict);
	}

	//添加一组状态
	public void Add(string key, object value)
	{
		Add(new KeyValuePair<string, object>(key, value));
	}

	//添加一组状态
	public void Add(KeyValuePair<string, object> pair)
	{
		if (!conditions.ContainsKey(pair.Key))
		{
			conditions.Add(pair.Key, pair.Value);
		}
		else
			Debug.Log("You tried to add and existing key to dictionary" + pair.ToString());
	}

	//设置一组Key
	public void SetKey(string str, object value)
	{
		if (conditions.ContainsKey(str))
		{
			conditions[str] = value;
		}
		else
			Debug.Log("You have tried to set a key that doesnt exist");
	}

	//设置或添加key
	public void SetOrAddKey(string key, object value)
	{
		if (conditions.ContainsKey(key))
		{
			conditions[key] = value;
		}
		else
		{
			Add(key, value);
		}
	}

	//移除
	public void RemoveKeyIfExists(string key)
	{
		if (conditions.ContainsKey(key))
		{
			conditions.Remove(key);
		}
	}

	////存在
	//public bool ContainsKey(string key)
	//{
	//	return conditions.ContainsKey(key);
	//}

	//public bool HaveKey(string key)
	//{
	//	if (conditions.ContainsKey(key))
	//		return true;
	//	else
	//		return false;
	//}

	//public string GetValue(string key)
	//{
	//	if (conditions.ContainsKey(key))
	//	{
	//		return conditions[key].ToString();
	//	}

	//	return "";
	//}

	//比较
	public bool CompareKey(string key, object value)
	{
		if (conditions.ContainsKey(key) && conditions[key].ToString() == value.ToString())
			return true;
		return false;
	}

	/// <summary>
	/// 将A状态 覆盖进 B状态
	/// </summary>
	/// <param name="overrider">A状态</param>
	/// <param name="dictionary">B状态</param>
	public static void OverrideCombine(StateDictionary overrider, StateDictionary dictionary)
	{
		foreach (KeyValuePair<string, object> pair in overrider.conditions)
			dictionary.conditions[pair.Key] = pair.Value;
	}


	/// <summary>
	/// 相当于equals 匹配整个列表 ,worldState必须要包含所有的conditions
	/// </summary>
	/// <returns><c>true</c>, if match was conditionsed, <c>false</c> otherwise.</returns>
	/// <param name="conditions">Conditions.</param>
	/// <param name="worldState">World state.</param>
	public static bool ConditionsMatch(StateDictionary conditions, StateDictionary worldState)
	{
		foreach (KeyValuePair<string, object> pair in conditions.conditions)
		{
			//if (worldState.ContainsKey(pair.Key))
			//{
			if (worldState.conditions[pair.Key].ToString() != conditions.conditions[pair.Key].ToString())
				return false;
			//}
			//else
			//    Debug.Log("Warning, Make sure you added condition to worldstate, Not Added Condition key: " + pair.Key);
		}
		return true;
	}


	//Action列表是否能满足目标 这个判断有bug,没有存在的意义
	//public static bool CanSatisfyGoalByActions(StateDictionary goalSd, List<AIAction> actions)
	//{
	//	bool[] checkedConditions = new bool[goalSd.conditions.Count];

	//	//foreach (AIAction action in actions)
	//	//{
	//	//	int i = 0;
	//	//	foreach (KeyValuePair<string, object> goalPair in goalSd.conditions)
	//	//	{
	//	//		if (checkedConditions[i])
	//	//		{
	//	//			i++;
	//	//			continue;
	//	//		}
	//	//		foreach (KeyValuePair<string, object> postEffectPair in action.postEffects.conditions)
	//	//		{
	//	//			if (goalPair.Key == postEffectPair.Key && goalPair.Value.ToString() == postEffectPair.Value.ToString())
	//	//				checkedConditions[i] = true;
	//	//		}
	//	//		i++;
	//	//	}
	//	//}

	//	if (checkedConditions.Length == goalSd.conditions.Count)
	//		return true;
	//	return false;
	//}
}