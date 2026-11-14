using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
	public class ObjectiveHUDManager : MonoBehaviour
	{
		[Tooltip("UI panel containing the layoutGroup for displaying objectives")]
		public RectTransform ObjectivePanel;

		[Tooltip("Prefab for the primary objectives")]
		public GameObject PrimaryObjectivePrefab;

		[Tooltip("Prefab for the primary objectives")]
		public GameObject SecondaryObjectivePrefab;

		private Dictionary<Objective, ObjectiveToast> m_ObjectivesDictionnary;

		private void Awake()
		{
			m_ObjectivesDictionnary = new Dictionary<Objective, ObjectiveToast>();
			EventManager.AddListener<ObjectiveUpdateEvent>(OnUpdateObjective);
			Objective.OnObjectiveCreated += RegisterObjective;
			Objective.OnObjectiveCompleted += UnregisterObjective;
		}

		public void RegisterObjective(Objective objective)
		{
			GameObject gameObject = Object.Instantiate(objective.IsOptional ? SecondaryObjectivePrefab : PrimaryObjectivePrefab, ObjectivePanel);
			if (!objective.IsOptional)
			{
				gameObject.transform.SetSiblingIndex(0);
			}
			ObjectiveToast component = gameObject.GetComponent<ObjectiveToast>();
			DebugUtility.HandleErrorIfNullGetComponent<ObjectiveToast, ObjectiveHUDManager>(component, this, gameObject.gameObject);
			component.Initialize(objective.Title, objective.Description, "", objective.IsOptional, objective.DelayVisible);
			m_ObjectivesDictionnary.Add(objective, component);
			LayoutRebuilder.ForceRebuildLayoutImmediate(ObjectivePanel);
		}

		public void UnregisterObjective(Objective objective)
		{
			if (m_ObjectivesDictionnary.TryGetValue(objective, out var value) && value != null)
			{
				value.Complete();
			}
			m_ObjectivesDictionnary.Remove(objective);
		}

		private void OnUpdateObjective(ObjectiveUpdateEvent evt)
		{
			if (m_ObjectivesDictionnary.TryGetValue(evt.Objective, out var value) && value != null)
			{
				Canvas.ForceUpdateCanvases();
				if (!string.IsNullOrEmpty(evt.DescriptionText))
				{
					value.DescriptionTextContent.text = evt.DescriptionText;
				}
				if (!string.IsNullOrEmpty(evt.CounterText))
				{
					value.CounterTextContent.text = evt.CounterText;
				}
				if ((bool)value.GetComponent<RectTransform>())
				{
					LayoutRebuilder.ForceRebuildLayoutImmediate(value.GetComponent<RectTransform>());
				}
			}
		}

		private void OnDestroy()
		{
			EventManager.AddListener<ObjectiveUpdateEvent>(OnUpdateObjective);
			Objective.OnObjectiveCreated -= RegisterObjective;
			Objective.OnObjectiveCompleted -= UnregisterObjective;
		}
	}
}
