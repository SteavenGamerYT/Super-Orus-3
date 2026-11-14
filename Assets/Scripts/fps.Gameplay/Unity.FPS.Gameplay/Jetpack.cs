using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
	[RequireComponent(typeof(AudioSource))]
	public class Jetpack : MonoBehaviour
	{
		[Header("References")]
		[Tooltip("Audio source for jetpack sfx")]
		public AudioSource AudioSource;

		[Tooltip("Particles for jetpack vfx")]
		public ParticleSystem[] JetpackVfx;

		[Header("Parameters")]
		[Tooltip("Whether the jetpack is unlocked at the begining or not")]
		public bool IsJetpackUnlockedAtStart;

		[Tooltip("The strength with which the jetpack pushes the player up")]
		public float JetpackAcceleration = 7f;

		[Range(0f, 1f)]
		[Tooltip("This will affect how much using the jetpack will cancel the gravity value, to start going up faster. 0 is not at all, 1 is instant")]
		public float JetpackDownwardVelocityCancelingFactor = 1f;

		[Header("Durations")]
		[Tooltip("Time it takes to consume all the jetpack fuel")]
		public float ConsumeDuration = 1.5f;

		[Tooltip("Time it takes to completely refill the jetpack while on the ground")]
		public float RefillDurationGrounded = 2f;

		[Tooltip("Time it takes to completely refill the jetpack while in the air")]
		public float RefillDurationInTheAir = 5f;

		[Tooltip("Delay after last use before starting to refill")]
		public float RefillDelay = 1f;

		[Header("Audio")]
		[Tooltip("Sound played when using the jetpack")]
		public AudioClip JetpackSfx;

		private bool m_CanUseJetpack;

		private PlayerCharacterController m_PlayerCharacterController;

		private PlayerInputHandler m_InputHandler;

		private float m_LastTimeOfUse;

		public UnityAction<bool> OnUnlockJetpack;

		public float CurrentFillRatio { get; private set; }

		public bool IsJetpackUnlocked { get; private set; }

		public bool IsPlayergrounded()
		{
			return m_PlayerCharacterController.IsGrounded;
		}

		private void Start()
		{
			IsJetpackUnlocked = IsJetpackUnlockedAtStart;
			m_PlayerCharacterController = GetComponent<PlayerCharacterController>();
			DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterController, Jetpack>(m_PlayerCharacterController, this, gameObject);
			m_InputHandler = GetComponent<PlayerInputHandler>();
			DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, Jetpack>(m_InputHandler, this, gameObject);
			CurrentFillRatio = 1f;
			AudioSource.clip = JetpackSfx;
			AudioSource.loop = true;
		}

		private void Update()
		{
			if (IsPlayergrounded())
			{
				m_CanUseJetpack = false;
			}
			else if (!m_PlayerCharacterController.HasJumpedThisFrame && m_InputHandler.GetJumpInputDown())
			{
				m_CanUseJetpack = true;
			}
			if (m_CanUseJetpack && IsJetpackUnlocked && CurrentFillRatio > 0f && m_InputHandler.GetJumpInputHeld())
			{
				m_LastTimeOfUse = Time.time;
				float jetpackAcceleration = JetpackAcceleration;
				jetpackAcceleration += m_PlayerCharacterController.GravityDownForce;
				if (m_PlayerCharacterController.CharacterVelocity.y < 0f)
				{
					jetpackAcceleration += (0f - m_PlayerCharacterController.CharacterVelocity.y) / Time.deltaTime * JetpackDownwardVelocityCancelingFactor;
				}
				m_PlayerCharacterController.CharacterVelocity += Vector3.up * jetpackAcceleration * Time.deltaTime;
				CurrentFillRatio -= Time.deltaTime / ConsumeDuration;
				for (int i = 0; i < JetpackVfx.Length; i++)
				{
					ParticleSystem.EmissionModule emission = JetpackVfx[i].emission;
					emission.enabled = true;
				}
				if (!AudioSource.isPlaying)
				{
					AudioSource.Play();
				}
			}
			else
			{
				if (IsJetpackUnlocked && Time.time - m_LastTimeOfUse >= RefillDelay)
				{
					float num = 1f / (m_PlayerCharacterController.IsGrounded ? RefillDurationGrounded : RefillDurationInTheAir);
					CurrentFillRatio += Time.deltaTime * num;
				}
				for (int j = 0; j < JetpackVfx.Length; j++)
				{
					ParticleSystem.EmissionModule emission2 = JetpackVfx[j].emission;
					emission2.enabled = false;
				}
				CurrentFillRatio = Mathf.Clamp01(CurrentFillRatio);
				if (AudioSource.isPlaying)
				{
					AudioSource.Stop();
				}
			}
		}

		public bool TryUnlock()
		{
			if (IsJetpackUnlocked)
			{
				return false;
			}
			OnUnlockJetpack(arg0: true);
			IsJetpackUnlocked = true;
			m_LastTimeOfUse = Time.time;
			return true;
		}
	}
}
