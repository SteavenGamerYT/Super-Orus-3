using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
	public class HealthPickup : Pickup
	{
		[Header("Parameters")]
		[Tooltip("Amount of health to heal on pickup")]
		public float HealAmount;

		protected override void OnPicked(PlayerCharacterController player)
		{
			Health component = player.GetComponent<Health>();
			if ((bool)component && component.CanPickup())
			{
				component.Heal(HealAmount);
				PlayPickupFeedback();
				Object.Destroy(gameObject);
			}
		}
	}
}
