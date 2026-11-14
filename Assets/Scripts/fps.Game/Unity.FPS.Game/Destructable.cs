using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
	public class Destructable : MonoBehaviour
	{
		private Health m_Health;

		private void Start()
		{
			m_Health = GetComponent<Health>();
			DebugUtility.HandleErrorIfNullGetComponent<Health, Destructable>(m_Health, this, gameObject);
			Health health = m_Health;
			health.OnDie = (UnityAction)Delegate.Combine(health.OnDie, new UnityAction(OnDie));
			Health health2 = m_Health;
			health2.OnDamaged = (UnityAction<float, GameObject>)Delegate.Combine(health2.OnDamaged, new UnityAction<float, GameObject>(OnDamaged));
		}

		private void OnDamaged(float damage, GameObject damageSource)
		{
		}

		private void OnDie()
		{
			UnityEngine.Object.Destroy(gameObject);
		}
	}
}
