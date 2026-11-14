using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
	public class PlayerHealthBar : MonoBehaviour
	{
		[Tooltip("Image component dispplaying current health")]
		public Image HealthFillImage;

		private Health m_PlayerHealth;

		private void Start()
		{
			PlayerCharacterController playerCharacterController = Object.FindAnyObjectByType<PlayerCharacterController>();
			DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, PlayerHealthBar>(playerCharacterController, this);
			m_PlayerHealth = playerCharacterController.GetComponent<Health>();
			DebugUtility.HandleErrorIfNullGetComponent<Health, PlayerHealthBar>(m_PlayerHealth, this, playerCharacterController.gameObject);
		}

		private void Update()
		{
			HealthFillImage.fillAmount = m_PlayerHealth.CurrentHealth / m_PlayerHealth.MaxHealth;
		}
	}
}
