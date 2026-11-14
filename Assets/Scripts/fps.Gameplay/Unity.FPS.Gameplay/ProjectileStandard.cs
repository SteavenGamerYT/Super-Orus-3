using System;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
	public class ProjectileStandard : ProjectileBase
	{
		[Header("General")]
		[Tooltip("Radius of this projectile's collision detection")]
		public float Radius = 0.01f;

		[Tooltip("Transform representing the root of the projectile (used for accurate collision detection)")]
		public Transform Root;

		[Tooltip("Transform representing the tip of the projectile (used for accurate collision detection)")]
		public Transform Tip;

		[Tooltip("LifeTime of the projectile")]
		public float MaxLifeTime = 5f;

		[Tooltip("VFX prefab to spawn upon impact")]
		public GameObject ImpactVfx;

		[Tooltip("LifeTime of the VFX before being destroyed")]
		public float ImpactVfxLifetime = 5f;

		[Tooltip("Offset along the hit normal where the VFX will be spawned")]
		public float ImpactVfxSpawnOffset = 0.1f;

		[Tooltip("Clip to play on impact")]
		public AudioClip ImpactSfxClip;

		[Tooltip("Layers this projectile can collide with")]
		public LayerMask HittableLayers = -1;

		[Header("Movement")]
		[Tooltip("Speed of the projectile")]
		public float Speed = 20f;

		[Tooltip("Downward acceleration from gravity")]
		public float GravityDownAcceleration;

		[Tooltip("Distance over which the projectile will correct its course to fit the intended trajectory (used to drift projectiles towards center of screen in First Person view). At values under 0, there is no correction")]
		public float TrajectoryCorrectionDistance = -1f;

		[Tooltip("Determines if the projectile inherits the velocity that the weapon's muzzle had when firing")]
		public bool InheritWeaponVelocity;

		[Header("Damage")]
		[Tooltip("Damage of the projectile")]
		public float Damage = 40f;

		[Tooltip("Area of damage. Keep empty if you don<t want area damage")]
		public DamageArea AreaOfDamage;

		[Header("Debug")]
		[Tooltip("Color of the projectile radius debug view")]
		public Color RadiusColor = Color.cyan * 0.2f;

		private ProjectileBase m_ProjectileBase;

		private Vector3 m_LastRootPosition;

		private Vector3 m_Velocity;

		private bool m_HasTrajectoryOverride;

		private float m_ShootTime;

		private Vector3 m_TrajectoryCorrectionVector;

		private Vector3 m_ConsumedTrajectoryCorrectionVector;

		private List<Collider> m_IgnoredColliders;

		private const QueryTriggerInteraction k_TriggerInteraction = QueryTriggerInteraction.Collide;

		private void OnEnable()
		{
			m_ProjectileBase = GetComponent<ProjectileBase>();
			DebugUtility.HandleErrorIfNullGetComponent<ProjectileBase, ProjectileStandard>(m_ProjectileBase, this, gameObject);
			ProjectileBase projectileBase = m_ProjectileBase;
			projectileBase.OnShoot = (UnityAction)Delegate.Combine(projectileBase.OnShoot, new UnityAction(OnShoot));
			UnityEngine.Object.Destroy(gameObject, MaxLifeTime);
		}

		private new void OnShoot()
		{
			m_ShootTime = Time.time;
			m_LastRootPosition = Root.position;
			m_Velocity = transform.forward * Speed;
			m_IgnoredColliders = new List<Collider>();
			transform.position += m_ProjectileBase.InheritedMuzzleVelocity * Time.deltaTime;
			Collider[] componentsInChildren = m_ProjectileBase.Owner.GetComponentsInChildren<Collider>();
			m_IgnoredColliders.AddRange(componentsInChildren);
			PlayerWeaponsManager component = m_ProjectileBase.Owner.GetComponent<PlayerWeaponsManager>();
			if ((bool)component)
			{
				m_HasTrajectoryOverride = true;
				Vector3 vector = m_ProjectileBase.InitialPosition - component.WeaponCamera.transform.position;
				m_TrajectoryCorrectionVector = Vector3.ProjectOnPlane(-vector, component.WeaponCamera.transform.forward);
				if (TrajectoryCorrectionDistance == 0f)
				{
					transform.position += m_TrajectoryCorrectionVector;
					m_ConsumedTrajectoryCorrectionVector = m_TrajectoryCorrectionVector;
				}
				else if (TrajectoryCorrectionDistance < 0f)
				{
					m_HasTrajectoryOverride = false;
				}
				if (Physics.Raycast(component.WeaponCamera.transform.position, vector.normalized, out var hitInfo, vector.magnitude, HittableLayers, QueryTriggerInteraction.Collide) && IsHitValid(hitInfo))
				{
					OnHit(hitInfo.point, hitInfo.normal, hitInfo.collider);
				}
			}
		}

		private void Update()
		{
			transform.position += m_Velocity * Time.deltaTime;
			if (InheritWeaponVelocity)
			{
				transform.position += m_ProjectileBase.InheritedMuzzleVelocity * Time.deltaTime;
			}
			if (m_HasTrajectoryOverride && m_ConsumedTrajectoryCorrectionVector.sqrMagnitude < m_TrajectoryCorrectionVector.sqrMagnitude)
			{
				Vector3 vector = m_TrajectoryCorrectionVector - m_ConsumedTrajectoryCorrectionVector;
				Vector3 vector2 = (Root.position - m_LastRootPosition).magnitude / TrajectoryCorrectionDistance * m_TrajectoryCorrectionVector;
				vector2 = Vector3.ClampMagnitude(vector2, vector.magnitude);
				m_ConsumedTrajectoryCorrectionVector += vector2;
				if (m_ConsumedTrajectoryCorrectionVector.sqrMagnitude == m_TrajectoryCorrectionVector.sqrMagnitude)
				{
					m_HasTrajectoryOverride = false;
				}
				transform.position += vector2;
			}
			transform.forward = m_Velocity.normalized;
			if (GravityDownAcceleration > 0f)
			{
				m_Velocity += Vector3.down * GravityDownAcceleration * Time.deltaTime;
			}
			RaycastHit raycastHit = new RaycastHit
			{
				distance = float.PositiveInfinity
			};
			bool flag = false;
			Vector3 vector3 = Tip.position - m_LastRootPosition;
			RaycastHit[] array = Physics.SphereCastAll(m_LastRootPosition, Radius, vector3.normalized, vector3.magnitude, HittableLayers, QueryTriggerInteraction.Collide);
			for (int i = 0; i < array.Length; i++)
			{
				RaycastHit raycastHit2 = array[i];
				if (IsHitValid(raycastHit2) && raycastHit2.distance < raycastHit.distance)
				{
					flag = true;
					raycastHit = raycastHit2;
				}
			}
			if (flag)
			{
				if (raycastHit.distance <= 0f)
				{
					raycastHit.point = Root.position;
					raycastHit.normal = -transform.forward;
				}
				OnHit(raycastHit.point, raycastHit.normal, raycastHit.collider);
			}
			m_LastRootPosition = Root.position;
		}

		private bool IsHitValid(RaycastHit hit)
		{
			if ((bool)hit.collider.GetComponent<IgnoreHitDetection>())
			{
				return false;
			}
			if (hit.collider.isTrigger && hit.collider.GetComponent<Damageable>() == null)
			{
				return false;
			}
			if (m_IgnoredColliders != null && m_IgnoredColliders.Contains(hit.collider))
			{
				return false;
			}
			return true;
		}

		private void OnHit(Vector3 point, Vector3 normal, Collider collider)
		{
			if ((bool)AreaOfDamage)
			{
				AreaOfDamage.InflictDamageInArea(Damage, point, HittableLayers, QueryTriggerInteraction.Collide, m_ProjectileBase.Owner);
			}
			else
			{
				Damageable component = collider.GetComponent<Damageable>();
				if ((bool)component)
				{
					component.InflictDamage(Damage, isExplosionDamage: false, m_ProjectileBase.Owner);
				}
			}
			if ((bool)ImpactVfx)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(ImpactVfx, point + normal * ImpactVfxSpawnOffset, Quaternion.LookRotation(normal));
				if (ImpactVfxLifetime > 0f)
				{
					UnityEngine.Object.Destroy(gameObject.gameObject, ImpactVfxLifetime);
				}
			}
			if ((bool)ImpactSfxClip)
			{
				AudioUtility.CreateSFX(ImpactSfxClip, point, AudioUtility.AudioGroups.Impact, 1f, 3f);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = RadiusColor;
			Gizmos.DrawSphere(transform.position, Radius);
		}
	}
}
