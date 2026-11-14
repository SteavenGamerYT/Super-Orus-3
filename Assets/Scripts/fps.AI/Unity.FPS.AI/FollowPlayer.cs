using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
	public class FollowPlayer : MonoBehaviour
	{
		private Transform m_PlayerTransform;

		private Vector3 m_OriginalOffset;

		private void Start()
		{
			ActorsManager actorsManager = Object.FindObjectOfType<ActorsManager>();
			if (actorsManager != null)
			{
				m_PlayerTransform = actorsManager.Player.transform;
				m_OriginalOffset = transform.position - m_PlayerTransform.position;
			}
			else
			{
				enabled = false;
			}
		}

		private void LateUpdate()
		{
			transform.position = m_PlayerTransform.position + m_OriginalOffset;
		}
	}
}
