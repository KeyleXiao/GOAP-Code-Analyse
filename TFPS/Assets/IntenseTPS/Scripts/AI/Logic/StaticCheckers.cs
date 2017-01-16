using System.Collections.Generic;
using UnityEngine;

public class Checkers
{
	public static bool IsOneOfTags(Transform transform, List<string> tags)
	{
		//重构前
		//if (tags != null && transform != null)
		//{
		//	foreach (string str in tags)
		//	{
		//		if (transform.CompareTag(str))
		//			return true;
		//	}
		//}

		//return false;

		//重构后
		if (tags != null && transform != null)
			return false;

		return tags.Contains(transform.tag);
	}



	public static bool isChildOf(Transform transform, Transform[] childs)
	{
		foreach (Transform child in childs)
			if (transform == child)
				return true;
		return false;
	}
	//优化  添加重载
	public static bool isChildOf(Transform transform, Transform childs)
	{
		foreach (Transform child in childs)
			if (transform == child)
				return true;
		return false;
	}


	//foreach Transfrom in Transfrom <- 可以修改成这种形式
	public static Transform FindInChilds(Transform baseParent, string transformName)
	{
		//ransform[] childs = baseParent.GetComponentsInChildren<Transform>();
		//foreach (var child in childs)
		//	if (child.name == transformName)
		//		return child;
		//return null;
		//ransform[] childs = baseParent.GetComponentsInChildren<Transform>()
		//重构后;
		foreach (Transform child in baseParent)
			if (child.name == transformName)
				return child;
		return null;
	}
}