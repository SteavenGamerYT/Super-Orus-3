namespace Unity.FPS.Game
{
	public static class Events
	{
		public static ObjectiveUpdateEvent ObjectiveUpdateEvent = new ObjectiveUpdateEvent();

		public static AllObjectivesCompletedEvent AllObjectivesCompletedEvent = new AllObjectivesCompletedEvent();

		public static GameOverEvent GameOverEvent = new GameOverEvent();

		public static PlayerDeathEvent PlayerDeathEvent = new PlayerDeathEvent();

		public static EnemyKillEvent EnemyKillEvent = new EnemyKillEvent();

		public static PickupEvent PickupEvent = new PickupEvent();

		public static AmmoPickupEvent AmmoPickupEvent = new AmmoPickupEvent();

		public static DamageEvent DamageEvent = new DamageEvent();

		public static DisplayMessageEvent DisplayMessageEvent = new DisplayMessageEvent();
	}
}
