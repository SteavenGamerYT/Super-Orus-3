using System;
using UnityEngine;

namespace Unity.FPS.Game
{
	public abstract class Objective : MonoBehaviour
	{
		[Tooltip("Name of the objective that will be shown on screen")]
		public string Title;

		[Tooltip("Short text explaining the objective that will be shown on screen")]
		public string Description;

		[Tooltip("Whether the objective is required to win or not")]
		public bool IsOptional;

		[Tooltip("Delay before the objective becomes visible")]
		public float DelayVisible;

		public bool IsCompleted { get; private set; }

		public static event Action<Objective> OnObjectiveCreated;

		public static event Action<Objective> OnObjectiveCompleted;

		public bool IsBlocking()
		{
			if (!IsOptional)
			{
				return !IsCompleted;
			}
			return false;
		}

		protected virtual void Start()
		{
			OnObjectiveCreated?.Invoke(this);
			DisplayMessageEvent displayMessageEvent = Events.DisplayMessageEvent;
			displayMessageEvent.Message = Title;
			displayMessageEvent.DelayBeforeDisplay = 0f;
			EventManager.Broadcast(displayMessageEvent);
		}

		public void UpdateObjective(string descriptionText, string counterText, string notificationText)
		{
			ObjectiveUpdateEvent objectiveUpdateEvent = Events.ObjectiveUpdateEvent;
			objectiveUpdateEvent.Objective = this;
			objectiveUpdateEvent.DescriptionText = descriptionText;
			objectiveUpdateEvent.CounterText = counterText;
			objectiveUpdateEvent.NotificationText = notificationText;
			objectiveUpdateEvent.IsComplete = IsCompleted;
			EventManager.Broadcast(objectiveUpdateEvent);
		}

		public void CompleteObjective(string descriptionText, string counterText, string notificationText)
		{
			IsCompleted = true;
			ObjectiveUpdateEvent objectiveUpdateEvent = Events.ObjectiveUpdateEvent;
			objectiveUpdateEvent.Objective = this;
			objectiveUpdateEvent.DescriptionText = descriptionText;
			objectiveUpdateEvent.CounterText = counterText;
			objectiveUpdateEvent.NotificationText = notificationText;
			objectiveUpdateEvent.IsComplete = IsCompleted;
			EventManager.Broadcast(objectiveUpdateEvent);
			OnObjectiveCompleted?.Invoke(this);
		}
	}
}
