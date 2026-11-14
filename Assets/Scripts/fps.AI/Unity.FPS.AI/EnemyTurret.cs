using System;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
	[RequireComponent(typeof(EnemyController))]
	public class EnemyTurret : MonoBehaviour
	{
		public enum AIState
		{
			Idle,
			Attack
		}

		public Transform TurretPivot;

		public Transform TurretAimPoint;

		public Animator Animator;

		public float AimRotationSharpness = 5f;

		public float LookAtRotationSharpness = 2.5f;

		public float DetectionFireDelay = 1f;

		public float AimingTransitionBlendTime = 1f;

		[Tooltip("The random hit damage effects")]
		public ParticleSystem[] RandomHitSparks;

		public ParticleSystem[] OnDetectVfx;

		public AudioClip OnDetectSfx;

		private EnemyController m_EnemyController;

		private Health m_Health;

		private Quaternion m_RotationWeaponForwardToPivot;

		private float m_TimeStartedDetection;

		private float m_TimeLostDetection;

		private Quaternion m_PreviousPivotAimingRotation;

		private Quaternion m_PivotAimingRotation;

		private const string k_AnimOnDamagedParameter = "OnDamaged";

		private const string k_AnimIsActiveParameter = "IsActive";

		public AIState AiState { get; private set; }

		private void Start()
		{
			m_Health = GetComponent<Health>();
			DebugUtility.HandleErrorIfNullGetComponent<Health, EnemyTurret>(m_Health, this, gameObject);
			Health health = m_Health;
			health.OnDamaged = (UnityAction<float, GameObject>)Delegate.Combine(health.OnDamaged, new UnityAction<float, GameObject>(OnDamaged));
			m_EnemyController = GetComponent<EnemyController>();
			DebugUtility.HandleErrorIfNullGetComponent<EnemyController, EnemyTurret>(m_EnemyController, this, gameObject);
			EnemyController enemyController = m_EnemyController;
			enemyController.onDetectedTarget = (UnityAction)Delegate.Combine(enemyController.onDetectedTarget, new UnityAction(OnDetectedTarget));
			EnemyController enemyController2 = m_EnemyController;
			enemyController2.onLostTarget = (UnityAction)Delegate.Combine(enemyController2.onLostTarget, new UnityAction(OnLostTarget));
			m_RotationWeaponForwardToPivot = Quaternion.Inverse(m_EnemyController.GetCurrentWeapon().WeaponMuzzle.rotation) * TurretPivot.rotation;
			AiState = AIState.Idle;
			m_TimeStartedDetection = float.NegativeInfinity;
			m_PreviousPivotAimingRotation = TurretPivot.rotation;
		}

		private void Update()
		{
			UpdateCurrentAiState();
		}

		private void LateUpdate()
		{
			UpdateTurretAiming();
		}

		private void UpdateCurrentAiState()
		{
			if (AiState == AIState.Attack)
			{
				bool flag = Time.time > m_TimeStartedDetection + DetectionFireDelay;
				Quaternion b = Quaternion.LookRotation((m_EnemyController.KnownDetectedTarget.transform.position - TurretAimPoint.position).normalized) * m_RotationWeaponForwardToPivot;
				m_PivotAimingRotation = Quaternion.Slerp(m_PreviousPivotAimingRotation, b, (flag ? AimRotationSharpness : LookAtRotationSharpness) * Time.deltaTime);
				if (flag)
				{
					Vector3 vector = m_PivotAimingRotation * Quaternion.Inverse(m_RotationWeaponForwardToPivot) * Vector3.forward;
					m_EnemyController.TryAtack(TurretAimPoint.position + vector);
				}
			}
		}

		private void UpdateTurretAiming()
		{
			if (AiState == AIState.Attack)
			{
				TurretPivot.rotation = m_PivotAimingRotation;
			}
			else
			{
				TurretPivot.rotation = Quaternion.Slerp(m_PivotAimingRotation, TurretPivot.rotation, (Time.time - m_TimeLostDetection) / AimingTransitionBlendTime);
			}
			m_PreviousPivotAimingRotation = TurretPivot.rotation;
		}

		private void OnDamaged(float dmg, GameObject source)
		{
			if (RandomHitSparks.Length != 0)
			{
				int num = UnityEngine.Random.Range(0, RandomHitSparks.Length - 1);
				RandomHitSparks[num].Play();
			}
			Animator.SetTrigger("OnDamaged");
		}

		private void OnDetectedTarget()
		{
			if (AiState == AIState.Idle)
			{
				AiState = AIState.Attack;
			}
			for (int i = 0; i < OnDetectVfx.Length; i++)
			{
				OnDetectVfx[i].Play();
			}
			if ((bool)OnDetectSfx)
			{
				AudioUtility.CreateSFX(OnDetectSfx, transform.position, AudioUtility.AudioGroups.EnemyDetection, 1f);
			}
			Animator.SetBool("IsActive", value: true);
			m_TimeStartedDetection = Time.time;
		}

		private void OnLostTarget()
		{
			if (AiState == AIState.Attack)
			{
				AiState = AIState.Idle;
			}
			for (int i = 0; i < OnDetectVfx.Length; i++)
			{
				OnDetectVfx[i].Stop();
			}
			Animator.SetBool("IsActive", value: false);
			m_TimeLostDetection = Time.time;
		}
	}
}
