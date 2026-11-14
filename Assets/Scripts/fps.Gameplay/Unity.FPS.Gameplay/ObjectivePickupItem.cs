using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
	public class ObjectivePickupItem : Objective
	{
		[Tooltip("Item to pickup to complete the objective")]
		public GameObject ItemToPickup;

		protected override void Start()
		{
			base.Start();
			EventManager.AddListener<PickupEvent>(OnPickupEvent);
		}

		private void OnPickupEvent(PickupEvent evt)
		{
			if (!IsCompleted && !(ItemToPickup != evt.Pickup))
			{
				CompleteObjective(string.Empty, string.Empty, "Objective complete : " + Title);
				if ((bool)gameObject)
				{
					Object.Destroy(gameObject);
				}
			}
		}

		private void OnDestroy()
		{
			EventManager.RemoveListener<PickupEvent>(OnPickupEvent);
		}
	}
}
