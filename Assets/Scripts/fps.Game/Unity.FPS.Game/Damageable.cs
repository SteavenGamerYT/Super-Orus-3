using UnityEngine;

namespace Unity.FPS.Game
{
	public class Damageable : MonoBehaviour
	{
		[Tooltip("Multiplier to apply to the received damage")]
		public float DamageMultiplier = 1f;

		[Range(0f, 1f)]
		[Tooltip("Multiplier to apply to self damage")]
		public float SensibilityToSelfdamage = 0.5f;

		public Health Health { get; private set; }

		private void Awake()
		{
			Health = GetComponent<Health>();
			if (!Health)
			{
				Health = GetComponentInParent<Health>();
			}
		}

		public void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource)
		{
			if ((bool)Health)
			{
				float num = damage;
				if (!isExplosionDamage)
				{
					num *= DamageMultiplier;
				}
				if (Health.gameObject == damageSource)
				{
					num *= SensibilityToSelfdamage;
				}
				Health.TakeDamage(num, damageSource);
			}
		}
	}
}
