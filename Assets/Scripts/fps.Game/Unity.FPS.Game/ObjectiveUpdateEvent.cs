namespace Unity.FPS.Game
{
	public class ObjectiveUpdateEvent : GameEvent
	{
		public Objective Objective;

		public string DescriptionText;

		public string CounterText;

		public bool IsComplete;

		public string NotificationText;
	}
}
