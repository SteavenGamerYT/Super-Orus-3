using UnityEngine;

namespace Unity.FPS.Gameplay
{
	public class JetpackPickup : Pickup
	{
		protected override void OnPicked(PlayerCharacterController byPlayer)
		{
			Jetpack component = byPlayer.GetComponent<Jetpack>();
			if ((bool)component && component.TryUnlock())
			{
				PlayPickupFeedback();
				Object.Destroy(gameObject);
			}
		}
	}
}
