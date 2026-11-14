using UnityEngine;

namespace Unity.FPS.Game
{
	public class TimedSelfDestruct : MonoBehaviour
	{
		public float LifeTime = 1f;

		private float m_SpawnTime;

		private void Awake()
		{
			m_SpawnTime = Time.time;
		}

		private void Update()
		{
			if (Time.time > m_SpawnTime + LifeTime)
			{
				Object.Destroy(gameObject);
			}
		}
	}
}
