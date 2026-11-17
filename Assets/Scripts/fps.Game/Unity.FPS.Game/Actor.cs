using UnityEngine;

namespace Unity.FPS.Game
{
	public class Actor : MonoBehaviour
	{
		[Tooltip("Represents the affiliation (or team) of the actor. Actors of the same affiliation are friendly to each other")]
		public int Affiliation;

		[Tooltip("Represents point where other actors will aim when they attack this actor")]
		public Transform AimPoint;

		private ActorsManager m_ActorsManager;

		private void Start()
		{
			m_ActorsManager = Object.FindAnyObjectByType<ActorsManager>();
			DebugUtility.HandleErrorIfNullFindObject<ActorsManager, Actor>(m_ActorsManager, this);
			if (!m_ActorsManager.Actors.Contains(this))
			{
				m_ActorsManager.Actors.Add(this);
			}
		}

		private void OnDestroy()
		{
			if ((bool)m_ActorsManager)
			{
				m_ActorsManager.Actors.Remove(this);
			}
		}
	}
}
