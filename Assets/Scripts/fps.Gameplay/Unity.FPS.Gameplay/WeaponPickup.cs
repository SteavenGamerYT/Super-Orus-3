using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
	public class WeaponPickup : Pickup
	{
		[Tooltip("The prefab for the weapon that will be added to the player on pickup")]
		public WeaponController WeaponPrefab;

		protected override void Start()
		{
			base.Start();
			Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren)
			{
				if (transform != base.transform)
				{
					transform.gameObject.layer = 0;
				}
			}
		}

		protected override void OnPicked(PlayerCharacterController byPlayer)
		{
			PlayerWeaponsManager component = byPlayer.GetComponent<PlayerWeaponsManager>();
			if ((bool)component && component.AddWeapon(WeaponPrefab))
			{
				if (component.GetActiveWeapon() == null)
				{
					component.SwitchWeapon(ascendingOrder: true);
				}
				PlayPickupFeedback();
				Object.Destroy(gameObject);
			}
		}
	}
}
