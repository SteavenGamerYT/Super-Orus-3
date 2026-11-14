using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
	public class AmmoPickup : Pickup
	{
		[Tooltip("Weapon those bullets are for")]
		public WeaponController Weapon;

		[Tooltip("Number of bullets the player gets")]
		public int BulletCount = 30;

		protected override void OnPicked(PlayerCharacterController byPlayer)
		{
			PlayerWeaponsManager component = byPlayer.GetComponent<PlayerWeaponsManager>();
			if ((bool)component)
			{
				WeaponController weaponController = component.HasWeapon(Weapon);
				if (weaponController != null)
				{
					weaponController.AddCarriablePhysicalBullets(BulletCount);
					AmmoPickupEvent ammoPickupEvent = Events.AmmoPickupEvent;
					ammoPickupEvent.Weapon = weaponController;
					EventManager.Broadcast(ammoPickupEvent);
					PlayPickupFeedback();
					Object.Destroy(gameObject);
				}
			}
		}
	}
}
