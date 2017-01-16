using Actions;
using Information;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum ShowDebugType
{
	MemoryItems,
	GoalSet,
	ActivePlan,
	WorldState
}

public class DebugPanel : MonoBehaviour
{
	private Button showHideButton;
	private Text showHideText;
	private bool isVisible = false;
	private Vector3 showHidePosition;
	private List<Text> tList;
	public GameObject textPrefab;
	public ShooterBehaviour shooterB;
	public Vector2 startSpace = Vector2.one;
	public float startSpaceOfTexts = 1f;
	public float spaceOfTexts = 1f;

	public ShowDebugType debugProperty = ShowDebugType.MemoryItems;

	private void Awake()
	{
		showHideButton = transform.Find("ShowHideButton").GetComponent<Button>();
		showHideText = showHideButton.transform.Find("Text").GetComponent<Text>();
		showHideButton.onClick.AddListener(
			() =>
			{
				isVisible = !isVisible;
			});

		tList = new List<Text>();
	}

	private void DestroyAll()
	{
	}

	private void Update()
	{
		if (!shooterB || !textPrefab || shooterB.enabled == false || !shooterB.gameObject.activeSelf)
			return;

		showHidePosition = showHideButton.GetComponent<RectTransform>().position;

		List<string> items = new List<string>();
		switch (debugProperty)
		{
			case ShowDebugType.MemoryItems:
				if (shooterB.ai == null || shooterB.ai.Memory.Items == null)
				{
					DestroyAll();
					return;
				}
				foreach (InformationP info in shooterB.ai.Memory.Items)
					items.Add(info.ToString());
				showHideText.text = "Memory";
				break;

			case ShowDebugType.GoalSet:
				if (shooterB == null)
				{
					DestroyAll();
					return;
				}
				foreach (AIGoal goal in shooterB.ai.AllGoals)
					items.Add(goal.ToString());
				showHideText.text = "GoalSet";
				break;

			case ShowDebugType.ActivePlan:
				if (shooterB.ai.Plan == null || shooterB.ai.Plan.Count == 0)
				{
					if (shooterB.ai.Plan == null) items.Add("Null Plan");
					else if (shooterB.ai.Plan.Count == 0) items.Add("Count = 0 plan");
				}
				else
				{
					foreach (AIAction action in shooterB.ai.Plan)
						items.Add(action.ToString().Split('.').Last());
					showHideText.text = "ActivePlan";
				}

				break;

			case ShowDebugType.WorldState:
				if (shooterB.ai == null || shooterB.ai.WorldState == null)
				{
					DestroyAll();
					return;
				}
				foreach (KeyValuePair<string, object> pair in shooterB.ai.WorldState.conditions)
				{
					items.Add("" + pair.Key + " :" + pair.Value);
				}
				showHideText.text = "WorldState";
				break;

			default:
				break;
		}

		if (tList.Count < items.Count)
		{
			int addCount = items.Count - tList.Count;
			for (int i = 0; i < addCount; i++)
			{
				GameObject nText = Instantiate(textPrefab);
				tList.Add(nText.GetComponent<Text>());
				nText.transform.SetParent(transform);

				nText.transform.localScale = Vector3.one;
			}
		}
		else if (tList.Count > items.Count)
		{
			int removeCount = tList.Count - items.Count;
			for (int i = 0; i < removeCount; i++)
			{
				GameObject removeGo = tList[tList.Count - 1].gameObject;
				Destroy(removeGo);
				tList.RemoveAt(tList.Count - 1);
			}
		}
		int w = 0;
		foreach (Text text in tList)
		{
			RectTransform rTransform = text.GetComponent<RectTransform>();
			rTransform.position = Vector3.up * (startSpace.y + spaceOfTexts * -w) + Vector3.right * startSpace.x + showHidePosition;
			text.text = w + 1 + "->  " + items[w];
			text.text = w + 1 < 10 ? "0" + text.text : text.text;
			w++;
		}
	}
}