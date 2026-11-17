using System.Linq;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
	public class DetectionModule : MonoBehaviour
	{
		[Tooltip("The point representing the source of target-detection raycasts for the enemy AI")]
		public Transform DetectionSourcePoint;

		[Tooltip("The max distance at which the enemy can see targets")]
		public float DetectionRange = 20f;

		[Tooltip("The max distance at which the enemy can attack its target")]
		public float AttackRange = 10f;

		[Tooltip("Time before an enemy abandons a known target that it can't see anymore")]
		public float KnownTargetTimeout = 4f;

		[Tooltip("Optional animator for OnShoot animations")]
		public Animator Animator;

		public UnityAction onDetectedTarget;

		public UnityAction onLostTarget;

		protected float TimeLastSeenTarget = float.NegativeInfinity;

		private ActorsManager m_ActorsManager;

		private const string k_AnimAttackParameter = "Attack";

		private const string k_AnimOnDamagedParameter = "OnDamaged";

		public GameObject KnownDetectedTarget { get; private set; }

		public bool IsTargetInAttackRange { get; private set; }

		public bool IsSeeingTarget { get; private set; }

		public bool HadKnownTarget { get; private set; }

		protected virtual void Start()
		{
			m_ActorsManager = Object.FindAnyObjectByType<ActorsManager>();
			DebugUtility.HandleErrorIfNullFindObject<ActorsManager, DetectionModule>(m_ActorsManager, this);
		}

		public virtual void HandleTargetDetection(Actor actor, Collider[] selfColliders)
		{
			if ((bool)KnownDetectedTarget && !IsSeeingTarget && Time.time - TimeLastSeenTarget > KnownTargetTimeout)
			{
				KnownDetectedTarget = null;
			}
			float num = DetectionRange * DetectionRange;
			IsSeeingTarget = false;
			float num2 = float.PositiveInfinity;
			foreach (Actor actor2 in m_ActorsManager.Actors)
			{
				if (actor2.Affiliation == actor.Affiliation)
				{
					continue;
				}
				float sqrMagnitude = (actor2.transform.position - DetectionSourcePoint.position).sqrMagnitude;
				if (!(sqrMagnitude < num) || !(sqrMagnitude < num2))
				{
					continue;
				}
				RaycastHit[] array = Physics.RaycastAll(DetectionSourcePoint.position, (actor2.AimPoint.position - DetectionSourcePoint.position).normalized, DetectionRange, -1, QueryTriggerInteraction.Ignore);
				RaycastHit raycastHit = new RaycastHit
				{
					distance = float.PositiveInfinity
				};
				bool flag = false;
				RaycastHit[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					RaycastHit raycastHit2 = array2[i];
					if (!selfColliders.Contains(raycastHit2.collider) && raycastHit2.distance < raycastHit.distance)
					{
						raycastHit = raycastHit2;
						flag = true;
					}
				}
				if (flag && raycastHit.collider.GetComponentInParent<Actor>() == actor2)
				{
					IsSeeingTarget = true;
					num2 = sqrMagnitude;
					TimeLastSeenTarget = Time.time;
					KnownDetectedTarget = actor2.AimPoint.gameObject;
				}
			}
			IsTargetInAttackRange = KnownDetectedTarget != null && Vector3.Distance(transform.position, KnownDetectedTarget.transform.position) <= AttackRange;
			if (!HadKnownTarget && KnownDetectedTarget != null)
			{
				OnDetect();
			}
			if (HadKnownTarget && KnownDetectedTarget == null)
			{
				OnLostTarget();
			}
			HadKnownTarget = KnownDetectedTarget != null;
		}

		public virtual void OnLostTarget()
		{
			onLostTarget?.Invoke();
		}

		public virtual void OnDetect()
		{
			onDetectedTarget?.Invoke();
		}

		public virtual void OnDamaged(GameObject damageSource)
		{
			TimeLastSeenTarget = Time.time;
			KnownDetectedTarget = damageSource;
			if ((bool)Animator)
			{
				Animator.SetTrigger("OnDamaged");
			}
		}

		public virtual void OnAttack()
		{
			if ((bool)Animator)
			{
				Animator.SetTrigger("Attack");
			}
		}
	}
}
