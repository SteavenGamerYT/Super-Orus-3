using UnityEngine;

namespace Unity.FPS.Game
{
	public class EnemyKillEvent : GameEvent
	{
		public GameObject Enemy;

		public int RemainingEnemyCount;
	}
}
