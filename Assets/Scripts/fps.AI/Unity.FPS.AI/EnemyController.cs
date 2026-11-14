using System;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
	[RequireComponent(typeof(Health), typeof(Actor), typeof(NavMeshAgent))]
	public class EnemyController : MonoBehaviour
	{
		[Serializable]
		public struct RendererIndexData
		{
			public Renderer Renderer;

			public int MaterialIndex;

			public RendererIndexData(Renderer renderer, int index)
			{
				Renderer = renderer;
				MaterialIndex = index;
			}
		}

		[Header("Parameters")]
		[Tooltip("The Y height at which the enemy will be automatically killed (if it falls off of the level)")]
		public float SelfDestructYHeight = -20f;

		[Tooltip("The distance at which the enemy considers that it has reached its current path destination point")]
		public float PathReachingRadius = 2f;

		[Tooltip("The speed at which the enemy rotates")]
		public float OrientationSpeed = 10f;

		[Tooltip("Delay after death where the GameObject is destroyed (to allow for animation)")]
		public float DeathDuration;

		[Header("Weapons Parameters")]
		[Tooltip("Allow weapon swapping for this enemy")]
		public bool SwapToNextWeapon;

		[Tooltip("Time delay between a weapon swap and the next attack")]
		public float DelayAfterWeaponSwap;

		[Header("Eye color")]
		[Tooltip("Material for the eye color")]
		public Material EyeColorMaterial;

		[Tooltip("The default color of the bot's eye")]
		[ColorUsage(true, true)]
		public Color DefaultEyeColor;

		[Tooltip("The attack color of the bot's eye")]
		[ColorUsage(true, true)]
		public Color AttackEyeColor;

		[Header("Flash on hit")]
		[Tooltip("The material used for the body of the hoverbot")]
		public Material BodyMaterial;

		[Tooltip("The gradient representing the color of the flash on hit")]
		[GradientUsage(true)]
		public Gradient OnHitBodyGradient;

		[Tooltip("The duration of the flash on hit")]
		public float FlashOnHitDuration = 0.5f;

		[Header("Sounds")]
		[Tooltip("Sound played when recieving damages")]
		public AudioClip DamageTick;

		[Header("VFX")]
		[Tooltip("The VFX prefab spawned when the enemy dies")]
		public GameObject DeathVfx;

		[Tooltip("The point at which the death VFX is spawned")]
		public Transform DeathVfxSpawnPoint;

		[Header("Loot")]
		[Tooltip("The object this enemy can drop when dying")]
		public GameObject LootPrefab;

		[Tooltip("The chance the object has to drop")]
		[Range(0f, 1f)]
		public float DropRate = 1f;

		[Header("Debug Display")]
		[Tooltip("Color of the sphere gizmo representing the path reaching range")]
		public Color PathReachingRangeColor = Color.yellow;

		[Tooltip("Color of the sphere gizmo representing the attack range")]
		public Color AttackRangeColor = Color.red;

		[Tooltip("Color of the sphere gizmo representing the detection range")]
		public Color DetectionRangeColor = Color.blue;

		public UnityAction onAttack;

		public UnityAction onDetectedTarget;

		public UnityAction onLostTarget;

		public UnityAction onDamaged;

		private List<RendererIndexData> m_BodyRenderers = new List<RendererIndexData>();

		private MaterialPropertyBlock m_BodyFlashMaterialPropertyBlock;

		private float m_LastTimeDamaged = float.NegativeInfinity;

		private RendererIndexData m_EyeRendererData;

		private MaterialPropertyBlock m_EyeColorMaterialPropertyBlock;

		private int m_PathDestinationNodeIndex;

		private EnemyManager m_EnemyManager;

		private ActorsManager m_ActorsManager;

		private Health m_Health;

		private Actor m_Actor;

		private Collider[] m_SelfColliders;

		private GameFlowManager m_GameFlowManager;

		private bool m_WasDamagedThisFrame;

		private float m_LastTimeWeaponSwapped = float.NegativeInfinity;

		private int m_CurrentWeaponIndex;

		private WeaponController m_CurrentWeapon;

		private WeaponController[] m_Weapons;

		private NavigationModule m_NavigationModule;

		public PatrolPath PatrolPath { get; set; }

		public GameObject KnownDetectedTarget => DetectionModule.KnownDetectedTarget;

		public bool IsTargetInAttackRange => DetectionModule.IsTargetInAttackRange;

		public bool IsSeeingTarget => DetectionModule.IsSeeingTarget;

		public bool HadKnownTarget => DetectionModule.HadKnownTarget;

		public NavMeshAgent NavMeshAgent { get; private set; }

		public DetectionModule DetectionModule { get; private set; }

		private void Start()
		{
			m_EnemyManager = UnityEngine.Object.FindObjectOfType<EnemyManager>();
			DebugUtility.HandleErrorIfNullFindObject<EnemyManager, EnemyController>(m_EnemyManager, this);
			m_ActorsManager = UnityEngine.Object.FindObjectOfType<ActorsManager>();
			DebugUtility.HandleErrorIfNullFindObject<ActorsManager, EnemyController>(m_ActorsManager, this);
			m_EnemyManager.RegisterEnemy(this);
			m_Health = GetComponent<Health>();
			DebugUtility.HandleErrorIfNullGetComponent<Health, EnemyController>(m_Health, this, gameObject);
			m_Actor = GetComponent<Actor>();
			DebugUtility.HandleErrorIfNullGetComponent<Actor, EnemyController>(m_Actor, this, gameObject);
			NavMeshAgent = GetComponent<NavMeshAgent>();
			if (NavMeshAgent != null && !NavMeshAgent.isOnNavMesh)
			{
				if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 15f, NavMesh.AllAreas))
				{
					NavMeshAgent.Warp(hit.position);
				}
			}
			m_SelfColliders = GetComponentsInChildren<Collider>();
			m_GameFlowManager = UnityEngine.Object.FindObjectOfType<GameFlowManager>();
			DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, EnemyController>(m_GameFlowManager, this);
			Health health = m_Health;
			health.OnDie = (UnityAction)Delegate.Combine(health.OnDie, new UnityAction(OnDie));
			Health health2 = m_Health;
			health2.OnDamaged = (UnityAction<float, GameObject>)Delegate.Combine(health2.OnDamaged, new UnityAction<float, GameObject>(OnDamaged));
			FindAndInitializeAllWeapons();
			GetCurrentWeapon().ShowWeapon(show: true);
			DetectionModule[] componentsInChildren = GetComponentsInChildren<DetectionModule>();
			DebugUtility.HandleErrorIfNoComponentFound<DetectionModule, EnemyController>(componentsInChildren.Length, this, gameObject);
			DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, EnemyController>(componentsInChildren.Length, this, gameObject);
			DetectionModule = componentsInChildren[0];
			DetectionModule detectionModule = DetectionModule;
			detectionModule.onDetectedTarget = (UnityAction)Delegate.Combine(detectionModule.onDetectedTarget, new UnityAction(OnDetectedTarget));
			DetectionModule detectionModule2 = DetectionModule;
			detectionModule2.onLostTarget = (UnityAction)Delegate.Combine(detectionModule2.onLostTarget, new UnityAction(OnLostTarget));
			onAttack = (UnityAction)Delegate.Combine(onAttack, new UnityAction(DetectionModule.OnAttack));
			NavigationModule[] componentsInChildren2 = GetComponentsInChildren<NavigationModule>();
			DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, EnemyController>(componentsInChildren.Length, this, gameObject);
			if (componentsInChildren2.Length != 0)
			{
				m_NavigationModule = componentsInChildren2[0];
				NavMeshAgent.speed = m_NavigationModule.MoveSpeed;
				NavMeshAgent.angularSpeed = m_NavigationModule.AngularSpeed;
				NavMeshAgent.acceleration = m_NavigationModule.Acceleration;
			}
			Renderer[] componentsInChildren3 = GetComponentsInChildren<Renderer>(includeInactive: true);
			foreach (Renderer renderer in componentsInChildren3)
			{
				for (int j = 0; j < renderer.sharedMaterials.Length; j++)
				{
					if (renderer.sharedMaterials[j] == EyeColorMaterial)
					{
						m_EyeRendererData = new RendererIndexData(renderer, j);
					}
					if (renderer.sharedMaterials[j] == BodyMaterial)
					{
						m_BodyRenderers.Add(new RendererIndexData(renderer, j));
					}
				}
			}
			m_BodyFlashMaterialPropertyBlock = new MaterialPropertyBlock();
			if (m_EyeRendererData.Renderer != null)
			{
				m_EyeColorMaterialPropertyBlock = new MaterialPropertyBlock();
				m_EyeColorMaterialPropertyBlock.SetColor("_EmissionColor", DefaultEyeColor);
				m_EyeRendererData.Renderer.SetPropertyBlock(m_EyeColorMaterialPropertyBlock, m_EyeRendererData.MaterialIndex);
			}
		}

		private void Update()
		{
			EnsureIsWithinLevelBounds();
			DetectionModule.HandleTargetDetection(m_Actor, m_SelfColliders);
			Color value = OnHitBodyGradient.Evaluate((Time.time - m_LastTimeDamaged) / FlashOnHitDuration);
			m_BodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", value);
			foreach (RendererIndexData bodyRenderer in m_BodyRenderers)
			{
				bodyRenderer.Renderer.SetPropertyBlock(m_BodyFlashMaterialPropertyBlock, bodyRenderer.MaterialIndex);
			}
			m_WasDamagedThisFrame = false;
		}

		private void EnsureIsWithinLevelBounds()
		{
			if (transform.position.y < SelfDestructYHeight)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}

		private void OnLostTarget()
		{
			onLostTarget();
			if (m_EyeRendererData.Renderer != null)
			{
				m_EyeColorMaterialPropertyBlock.SetColor("_EmissionColor", DefaultEyeColor);
				m_EyeRendererData.Renderer.SetPropertyBlock(m_EyeColorMaterialPropertyBlock, m_EyeRendererData.MaterialIndex);
			}
		}

		private void OnDetectedTarget()
		{
			onDetectedTarget();
			if (m_EyeRendererData.Renderer != null)
			{
				m_EyeColorMaterialPropertyBlock.SetColor("_EmissionColor", AttackEyeColor);
				m_EyeRendererData.Renderer.SetPropertyBlock(m_EyeColorMaterialPropertyBlock, m_EyeRendererData.MaterialIndex);
			}
		}

		public void OrientTowards(Vector3 lookPosition)
		{
			Vector3 normalized = Vector3.ProjectOnPlane(lookPosition - transform.position, Vector3.up).normalized;
			if (normalized.sqrMagnitude != 0f)
			{
				Quaternion b = Quaternion.LookRotation(normalized);
				transform.rotation = Quaternion.Slerp(transform.rotation, b, Time.deltaTime * OrientationSpeed);
			}
		}

		private bool IsPathValid()
		{
			if ((bool)PatrolPath)
			{
				return PatrolPath.PathNodes.Count > 0;
			}
			return false;
		}

		public void ResetPathDestination()
		{
			m_PathDestinationNodeIndex = 0;
		}

		public void SetPathDestinationToClosestNode()
		{
			if (IsPathValid())
			{
				int num = 0;
				for (int i = 0; i < PatrolPath.PathNodes.Count; i++)
				{
					if (PatrolPath.GetDistanceToNode(transform.position, i) < PatrolPath.GetDistanceToNode(transform.position, num))
					{
						num = i;
					}
				}
				m_PathDestinationNodeIndex = num;
			}
			else
			{
				m_PathDestinationNodeIndex = 0;
			}
		}

		public Vector3 GetDestinationOnPath()
		{
			if (IsPathValid())
			{
				return PatrolPath.GetPositionOfPathNode(m_PathDestinationNodeIndex);
			}
			return transform.position;
		}

		public void SetNavDestination(Vector3 destination)
		{
			if (NavMeshAgent != null && NavMeshAgent.isOnNavMesh)
			{
				NavMeshAgent.SetDestination(destination);
			}
		}

		public void UpdatePathDestination(bool inverseOrder = false)
		{
			if (IsPathValid() && (transform.position - GetDestinationOnPath()).magnitude <= PathReachingRadius)
			{
				m_PathDestinationNodeIndex = (inverseOrder ? (m_PathDestinationNodeIndex - 1) : (m_PathDestinationNodeIndex + 1));
				if (m_PathDestinationNodeIndex < 0)
				{
					m_PathDestinationNodeIndex += PatrolPath.PathNodes.Count;
				}
				if (m_PathDestinationNodeIndex >= PatrolPath.PathNodes.Count)
				{
					m_PathDestinationNodeIndex -= PatrolPath.PathNodes.Count;
				}
			}
		}

		private void OnDamaged(float damage, GameObject damageSource)
		{
			if ((bool)damageSource && !damageSource.GetComponent<EnemyController>())
			{
				DetectionModule.OnDamaged(damageSource);
				onDamaged?.Invoke();
				m_LastTimeDamaged = Time.time;
				if ((bool)DamageTick && !m_WasDamagedThisFrame)
				{
					AudioUtility.CreateSFX(DamageTick, transform.position, AudioUtility.AudioGroups.DamageTick, 0f);
				}
				m_WasDamagedThisFrame = true;
			}
		}

		private void OnDie()
		{
			UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate(DeathVfx, DeathVfxSpawnPoint.position, Quaternion.identity), 5f);
			m_EnemyManager.UnregisterEnemy(this);
			if (TryDropItem())
			{
				UnityEngine.Object.Instantiate(LootPrefab, transform.position, Quaternion.identity);
			}
			UnityEngine.Object.Destroy(gameObject, DeathDuration);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = PathReachingRangeColor;
			Gizmos.DrawWireSphere(transform.position, PathReachingRadius);
			if (DetectionModule != null)
			{
				Gizmos.color = DetectionRangeColor;
				Gizmos.DrawWireSphere(transform.position, DetectionModule.DetectionRange);
				Gizmos.color = AttackRangeColor;
				Gizmos.DrawWireSphere(transform.position, DetectionModule.AttackRange);
			}
		}

		public void OrientWeaponsTowards(Vector3 lookPosition)
		{
			for (int i = 0; i < m_Weapons.Length; i++)
			{
				Vector3 normalized = (lookPosition - m_Weapons[i].WeaponRoot.transform.position).normalized;
				m_Weapons[i].transform.forward = normalized;
			}
		}

		public bool TryAtack(Vector3 enemyPosition)
		{
			if (m_GameFlowManager.GameIsEnding)
			{
				return false;
			}
			OrientWeaponsTowards(enemyPosition);
			if (m_LastTimeWeaponSwapped + DelayAfterWeaponSwap >= Time.time)
			{
				return false;
			}
			bool flag = GetCurrentWeapon().HandleShootInputs(inputDown: false, inputHeld: true, inputUp: false);
			if (flag && onAttack != null)
			{
				onAttack();
				if (SwapToNextWeapon && m_Weapons.Length > 1)
				{
					int currentWeapon = (m_CurrentWeaponIndex + 1) % m_Weapons.Length;
					SetCurrentWeapon(currentWeapon);
				}
			}
			return flag;
		}

		public bool TryDropItem()
		{
			if (DropRate == 0f || LootPrefab == null)
			{
				return false;
			}
			if (DropRate == 1f)
			{
				return true;
			}
			return UnityEngine.Random.value <= DropRate;
		}

		private void FindAndInitializeAllWeapons()
		{
			if (m_Weapons == null)
			{
				m_Weapons = GetComponentsInChildren<WeaponController>();
				DebugUtility.HandleErrorIfNoComponentFound<WeaponController, EnemyController>(m_Weapons.Length, this, gameObject);
				for (int i = 0; i < m_Weapons.Length; i++)
				{
					m_Weapons[i].Owner = gameObject;
				}
			}
		}

		public WeaponController GetCurrentWeapon()
		{
			FindAndInitializeAllWeapons();
			if (m_CurrentWeapon == null)
			{
				SetCurrentWeapon(0);
			}
			DebugUtility.HandleErrorIfNullGetComponent<WeaponController, EnemyController>(m_CurrentWeapon, this, gameObject);
			return m_CurrentWeapon;
		}

		private void SetCurrentWeapon(int index)
		{
			m_CurrentWeaponIndex = index;
			m_CurrentWeapon = m_Weapons[m_CurrentWeaponIndex];
			if (SwapToNextWeapon)
			{
				m_LastTimeWeaponSwapped = Time.time;
			}
			else
			{
				m_LastTimeWeaponSwapped = float.NegativeInfinity;
			}
		}
	}
}
