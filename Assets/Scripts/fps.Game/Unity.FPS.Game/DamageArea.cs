using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
	public class DamageArea : MonoBehaviour
	{
		[Tooltip("Area of damage when the projectile hits something")]
		public float AreaOfEffectDistance = 5f;

		[Tooltip("Damage multiplier over distance for area of effect")]
		public AnimationCurve DamageRatioOverDistance;

		[Header("Debug")]
		[Tooltip("Color of the area of effect radius")]
		public Color AreaOfEffectColor = Color.red * 0.5f;

		public void InflictDamageInArea(float damage, Vector3 center, LayerMask layers, QueryTriggerInteraction interaction, GameObject owner)
		{
			Dictionary<Health, Damageable> dictionary = new Dictionary<Health, Damageable>();
			Collider[] array = Physics.OverlapSphere(center, AreaOfEffectDistance, layers, interaction);
			for (int i = 0; i < array.Length; i++)
			{
				Damageable component = array[i].GetComponent<Damageable>();
				if ((bool)component)
				{
					Health componentInParent = component.GetComponentInParent<Health>();
					if ((bool)componentInParent && !dictionary.ContainsKey(componentInParent))
					{
						dictionary.Add(componentInParent, component);
					}
				}
			}
			foreach (Damageable value in dictionary.Values)
			{
				float num = Vector3.Distance(value.transform.position, transform.position);
				value.InflictDamage(damage * DamageRatioOverDistance.Evaluate(num / AreaOfEffectDistance), isExplosionDamage: true, owner);
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = AreaOfEffectColor;
			Gizmos.DrawSphere(transform.position, AreaOfEffectDistance);
		}
	}
}
