using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
	public class ObjectiveKillEnemies : Objective
	{
		[Tooltip("Chose whether you need to kill every enemies or only a minimum amount")]
		public bool MustKillAllEnemies = true;

		[Tooltip("If MustKillAllEnemies is false, this is the amount of enemy kills required")]
		public int KillsToCompleteObjective = 5;

		[Tooltip("Start sending notification about remaining enemies when this amount of enemies is left")]
		public int NotificationEnemiesRemainingThreshold = 3;

		private int m_KillTotal;

		protected override void Start()
		{
			base.Start();
			EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);
			if (string.IsNullOrEmpty(Title))
			{
				Title = "Eliminate " + (MustKillAllEnemies ? "all the" : KillsToCompleteObjective.ToString()) + " enemies";
			}
			if (string.IsNullOrEmpty(Description))
			{
				Description = GetUpdatedCounterAmount();
			}
		}

		private void OnEnemyKilled(EnemyKillEvent evt)
		{
			if (!IsCompleted)
			{
				m_KillTotal++;
				if (MustKillAllEnemies)
				{
					KillsToCompleteObjective = evt.RemainingEnemyCount + m_KillTotal;
				}
				int num = (MustKillAllEnemies ? evt.RemainingEnemyCount : (KillsToCompleteObjective - m_KillTotal));
				switch (num)
				{
				case 0:
					CompleteObjective(string.Empty, GetUpdatedCounterAmount(), "Objective complete : " + Title);
					break;
				case 1:
				{
					string notificationText2 = ((NotificationEnemiesRemainingThreshold >= num) ? "One enemy left" : string.Empty);
					UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText2);
					break;
				}
				default:
				{
					string notificationText = ((NotificationEnemiesRemainingThreshold >= num) ? (num + " enemies to kill left") : string.Empty);
					UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
					break;
				}
				}
			}
		}

		private string GetUpdatedCounterAmount()
		{
			return m_KillTotal + " / " + KillsToCompleteObjective;
		}

		private void OnDestroy()
		{
			EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
		}
	}
}
