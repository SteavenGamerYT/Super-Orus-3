using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
	public class Health : MonoBehaviour
	{
		[Tooltip("Maximum amount of health")]
		public float MaxHealth = 10f;

		[Tooltip("Health ratio at which the critical health vignette starts appearing")]
		public float CriticalHealthRatio = 0.3f;

		public UnityAction<float, GameObject> OnDamaged;

		public UnityAction<float> OnHealed;

		public UnityAction OnDie;

		private bool m_IsDead;

		public float CurrentHealth { get; set; }

		public bool Invincible { get; set; }

		public bool CanPickup()
		{
			return CurrentHealth < MaxHealth;
		}

		public float GetRatio()
		{
			return CurrentHealth / MaxHealth;
		}

		public bool IsCritical()
		{
			return GetRatio() <= CriticalHealthRatio;
		}

		private void Start()
		{
			CurrentHealth = MaxHealth;
		}

		public void Heal(float healAmount)
		{
			float currentHealth = CurrentHealth;
			CurrentHealth += healAmount;
			CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
			float num = CurrentHealth - currentHealth;
			if (num > 0f)
			{
				OnHealed?.Invoke(num);
			}
		}

		public void TakeDamage(float damage, GameObject damageSource)
		{
			if (!Invincible)
			{
				float currentHealth = CurrentHealth;
				CurrentHealth -= damage;
				CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
				float num = currentHealth - CurrentHealth;
				if (num > 0f)
				{
					OnDamaged?.Invoke(num, damageSource);
				}
				HandleDeath();
			}
		}

		public void Kill()
		{
			CurrentHealth = 0f;
			OnDamaged?.Invoke(MaxHealth, null);
			HandleDeath();
		}

		private void HandleDeath()
		{
			if (!m_IsDead && CurrentHealth <= 0f)
			{
				m_IsDead = true;
				OnDie?.Invoke();
			}
		}
	}
}
