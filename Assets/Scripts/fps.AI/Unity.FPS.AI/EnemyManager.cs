using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
	public class EnemyManager : MonoBehaviour
	{
		public List<EnemyController> Enemies { get; private set; }

		public int NumberOfEnemiesTotal { get; private set; }

		public int NumberOfEnemiesRemaining => Enemies.Count;

		private void Awake()
		{
			Enemies = new List<EnemyController>();
		}

		public void RegisterEnemy(EnemyController enemy)
		{
			Enemies.Add(enemy);
			NumberOfEnemiesTotal++;
		}

		public void UnregisterEnemy(EnemyController enemyKilled)
		{
			int remainingEnemyCount = NumberOfEnemiesRemaining - 1;
			EnemyKillEvent enemyKillEvent = Events.EnemyKillEvent;
			enemyKillEvent.Enemy = enemyKilled.gameObject;
			enemyKillEvent.RemainingEnemyCount = remainingEnemyCount;
			EventManager.Broadcast(enemyKillEvent);
			Enemies.Remove(enemyKilled);
		}
	}
}
