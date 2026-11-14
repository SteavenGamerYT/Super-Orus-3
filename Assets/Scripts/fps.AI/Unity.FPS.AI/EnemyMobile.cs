using System;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
	[RequireComponent(typeof(EnemyController))]
	public class EnemyMobile : MonoBehaviour
	{
		public enum AIState
		{
			Patrol,
			Follow,
			Attack
		}

		public Animator Animator;

		[Tooltip("Fraction of the enemy's attack range at which it will stop moving towards target while attacking")]
		[Range(0f, 1f)]
		public float AttackStopDistanceRatio = 0.5f;

		[Tooltip("The random hit damage effects")]
		public ParticleSystem[] RandomHitSparks;

		public ParticleSystem[] OnDetectVfx;

		public AudioClip OnDetectSfx;

		[Header("Sound")]
		public AudioClip MovementSound;

		public MinMaxFloat PitchDistortionMovementSpeed;

		private EnemyController m_EnemyController;

		private AudioSource m_AudioSource;

		private const string k_AnimMoveSpeedParameter = "MoveSpeed";

		private const string k_AnimAttackParameter = "Attack";

		private const string k_AnimAlertedParameter = "Alerted";

		private const string k_AnimOnDamagedParameter = "OnDamaged";

		public AIState AiState { get; private set; }

		private void Start()
		{
			m_EnemyController = GetComponent<EnemyController>();
			DebugUtility.HandleErrorIfNullGetComponent<EnemyController, EnemyMobile>(m_EnemyController, this, gameObject);
			EnemyController enemyController = m_EnemyController;
			enemyController.onAttack = (UnityAction)Delegate.Combine(enemyController.onAttack, new UnityAction(OnAttack));
			EnemyController enemyController2 = m_EnemyController;
			enemyController2.onDetectedTarget = (UnityAction)Delegate.Combine(enemyController2.onDetectedTarget, new UnityAction(OnDetectedTarget));
			EnemyController enemyController3 = m_EnemyController;
			enemyController3.onLostTarget = (UnityAction)Delegate.Combine(enemyController3.onLostTarget, new UnityAction(OnLostTarget));
			m_EnemyController.SetPathDestinationToClosestNode();
			EnemyController enemyController4 = m_EnemyController;
			enemyController4.onDamaged = (UnityAction)Delegate.Combine(enemyController4.onDamaged, new UnityAction(OnDamaged));
			AiState = AIState.Patrol;
			m_AudioSource = GetComponent<AudioSource>();
			DebugUtility.HandleErrorIfNullGetComponent<AudioSource, EnemyMobile>(m_AudioSource, this, gameObject);
			m_AudioSource.clip = MovementSound;
			m_AudioSource.Play();
		}

		private void Update()
		{
			UpdateAiStateTransitions();
			UpdateCurrentAiState();
			float magnitude = (m_EnemyController.NavMeshAgent != null && m_EnemyController.NavMeshAgent.isOnNavMesh)
				? m_EnemyController.NavMeshAgent.velocity.magnitude
				: 0f;
			Animator.SetFloat("MoveSpeed", magnitude);
			float speed = (m_EnemyController.NavMeshAgent != null && m_EnemyController.NavMeshAgent.speed > 0f)
				? m_EnemyController.NavMeshAgent.speed
				: 1f;
			m_AudioSource.pitch = Mathf.Lerp(PitchDistortionMovementSpeed.Min, PitchDistortionMovementSpeed.Max, magnitude / speed);
		}

		private void UpdateAiStateTransitions()
		{
			switch (AiState)
			{
			case AIState.Follow:
				if (m_EnemyController.IsSeeingTarget && m_EnemyController.IsTargetInAttackRange)
				{
					AiState = AIState.Attack;
					m_EnemyController.SetNavDestination(transform.position);
				}
				break;
			case AIState.Attack:
				if (!m_EnemyController.IsTargetInAttackRange)
				{
					AiState = AIState.Follow;
				}
				break;
			}
		}

		private void UpdateCurrentAiState()
		{
			switch (AiState)
			{
			case AIState.Patrol:
				m_EnemyController.UpdatePathDestination();
				m_EnemyController.SetNavDestination(m_EnemyController.GetDestinationOnPath());
				break;
			case AIState.Follow:
				m_EnemyController.SetNavDestination(m_EnemyController.KnownDetectedTarget.transform.position);
				m_EnemyController.OrientTowards(m_EnemyController.KnownDetectedTarget.transform.position);
				m_EnemyController.OrientWeaponsTowards(m_EnemyController.KnownDetectedTarget.transform.position);
				break;
			case AIState.Attack:
				if (Vector3.Distance(m_EnemyController.KnownDetectedTarget.transform.position, m_EnemyController.DetectionModule.DetectionSourcePoint.position) >= AttackStopDistanceRatio * m_EnemyController.DetectionModule.AttackRange)
				{
					m_EnemyController.SetNavDestination(m_EnemyController.KnownDetectedTarget.transform.position);
				}
				else
				{
					m_EnemyController.SetNavDestination(transform.position);
				}
				m_EnemyController.OrientTowards(m_EnemyController.KnownDetectedTarget.transform.position);
				m_EnemyController.TryAtack(m_EnemyController.KnownDetectedTarget.transform.position);
				break;
			}
		}

		private void OnAttack()
		{
			Animator.SetTrigger("Attack");
		}

		private void OnDetectedTarget()
		{
			if (AiState == AIState.Patrol)
			{
				AiState = AIState.Follow;
			}
			for (int i = 0; i < OnDetectVfx.Length; i++)
			{
				OnDetectVfx[i].Play();
			}
			if ((bool)OnDetectSfx)
			{
				AudioUtility.CreateSFX(OnDetectSfx, transform.position, AudioUtility.AudioGroups.EnemyDetection, 1f);
			}
			Animator.SetBool("Alerted", value: true);
		}

		private void OnLostTarget()
		{
			if (AiState == AIState.Follow || AiState == AIState.Attack)
			{
				AiState = AIState.Patrol;
			}
			for (int i = 0; i < OnDetectVfx.Length; i++)
			{
				OnDetectVfx[i].Stop();
			}
			Animator.SetBool("Alerted", value: false);
		}

		private void OnDamaged()
		{
			if (RandomHitSparks.Length != 0)
			{
				int num = UnityEngine.Random.Range(0, RandomHitSparks.Length - 1);
				RandomHitSparks[num].Play();
			}
			Animator.SetTrigger("OnDamaged");
		}
	}
}
