using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
	public class TeleportPlayer : MonoBehaviour
	{
		public KeyCode ActivateKey = KeyCode.F12;

		private PlayerCharacterController m_PlayerCharacterController;

		private void Awake()
		{
			m_PlayerCharacterController = Object.FindAnyObjectByType<PlayerCharacterController>();
			DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, TeleportPlayer>(m_PlayerCharacterController, this);
		}

		private void Update()
		{
			if (Input.GetKeyDown(ActivateKey))
			{
				m_PlayerCharacterController.transform.SetPositionAndRotation(transform.position, transform.rotation);
				Health component = m_PlayerCharacterController.GetComponent<Health>();
				if ((bool)component)
				{
					component.Heal(999f);
				}
			}
		}
	}
}
