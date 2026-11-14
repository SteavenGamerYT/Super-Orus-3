using System;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
	public class FeedbackFlashHUD : MonoBehaviour
	{
		[Header("References")]
		[Tooltip("Image component of the flash")]
		public Image FlashImage;

		[Tooltip("CanvasGroup to fade the damage flash, used when recieving damage end healing")]
		public CanvasGroup FlashCanvasGroup;

		[Tooltip("CanvasGroup to fade the critical health vignette")]
		public CanvasGroup VignetteCanvasGroup;

		[Header("Damage")]
		[Tooltip("Color of the damage flash")]
		public Color DamageFlashColor;

		[Tooltip("Duration of the damage flash")]
		public float DamageFlashDuration;

		[Tooltip("Max alpha of the damage flash")]
		public float DamageFlashMaxAlpha = 1f;

		[Header("Critical health")]
		[Tooltip("Max alpha of the critical vignette")]
		public float CriticaHealthVignetteMaxAlpha = 0.8f;

		[Tooltip("Frequency at which the vignette will pulse when at critical health")]
		public float PulsatingVignetteFrequency = 4f;

		[Header("Heal")]
		[Tooltip("Color of the heal flash")]
		public Color HealFlashColor;

		[Tooltip("Duration of the heal flash")]
		public float HealFlashDuration;

		[Tooltip("Max alpha of the heal flash")]
		public float HealFlashMaxAlpha = 1f;

		private bool m_FlashActive;

		private float m_LastTimeFlashStarted = float.NegativeInfinity;

		private Health m_PlayerHealth;

		private GameFlowManager m_GameFlowManager;

		private void Start()
		{
			PlayerCharacterController playerCharacterController = UnityEngine.Object.FindAnyObjectByType<PlayerCharacterController>();
			DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, FeedbackFlashHUD>(playerCharacterController, this);
			m_PlayerHealth = playerCharacterController.GetComponent<Health>();
			DebugUtility.HandleErrorIfNullGetComponent<Health, FeedbackFlashHUD>(m_PlayerHealth, this, playerCharacterController.gameObject);
			m_GameFlowManager = UnityEngine.Object.FindAnyObjectByType<GameFlowManager>();
			DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, FeedbackFlashHUD>(m_GameFlowManager, this);
			Health playerHealth = m_PlayerHealth;
			playerHealth.OnDamaged = (UnityAction<float, GameObject>)Delegate.Combine(playerHealth.OnDamaged, new UnityAction<float, GameObject>(OnTakeDamage));
			Health playerHealth2 = m_PlayerHealth;
			playerHealth2.OnHealed = (UnityAction<float>)Delegate.Combine(playerHealth2.OnHealed, new UnityAction<float>(OnHealed));
		}

		private void Update()
		{
			if (m_PlayerHealth.IsCritical())
			{
				VignetteCanvasGroup.gameObject.SetActive(value: true);
				float num = (1f - m_PlayerHealth.CurrentHealth / m_PlayerHealth.MaxHealth / m_PlayerHealth.CriticalHealthRatio) * CriticaHealthVignetteMaxAlpha;
				if (m_GameFlowManager.GameIsEnding)
				{
					VignetteCanvasGroup.alpha = num;
				}
				else
				{
					VignetteCanvasGroup.alpha = (Mathf.Sin(Time.time * PulsatingVignetteFrequency) / 2f + 0.5f) * num;
				}
			}
			else
			{
				VignetteCanvasGroup.gameObject.SetActive(value: false);
			}
			if (m_FlashActive)
			{
				float num2 = (Time.time - m_LastTimeFlashStarted) / DamageFlashDuration;
				if (num2 < 1f)
				{
					float alpha = DamageFlashMaxAlpha * (1f - num2);
					FlashCanvasGroup.alpha = alpha;
				}
				else
				{
					FlashCanvasGroup.gameObject.SetActive(value: false);
					m_FlashActive = false;
				}
			}
		}

		private void ResetFlash()
		{
			m_LastTimeFlashStarted = Time.time;
			m_FlashActive = true;
			FlashCanvasGroup.alpha = 0f;
			FlashCanvasGroup.gameObject.SetActive(value: true);
		}

		private void OnTakeDamage(float dmg, GameObject damageSource)
		{
			ResetFlash();
			FlashImage.color = DamageFlashColor;
		}

		private void OnHealed(float amount)
		{
			ResetFlash();
			FlashImage.color = HealFlashColor;
		}
	}
}
