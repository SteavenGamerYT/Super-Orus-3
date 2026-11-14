using UnityEngine;

namespace Unity.InteractiveTutorials
{
	[RequireComponent(typeof(Collider))]
	public class CollisionBroadcaster3D : BaseCollisionBroadcaster
	{
		public delegate void EventHandler(CollisionBroadcaster3D sender);

		public static event EventHandler playerEnteredCollision;

		public static event EventHandler playerExitedCollision;

		public static event EventHandler playerEnteredTrigger;

		public static event EventHandler playerExitedTrigger;

		private void OnCollisionEnter(Collision collision)
		{
			if (collision.gameObject.GetComponentInChildren<IPlayerAvatar>() != null && playerEnteredCollision != null)
			{
				playerEnteredCollision(this);
			}
		}

		private void OnCollisionExit(Collision collision)
		{
			if (collision.gameObject.GetComponentInChildren<IPlayerAvatar>() != null && playerExitedCollision != null)
			{
				playerExitedCollision(this);
			}
		}

		private void OnTriggerEnter(Collider collider)
		{
			if (collider.GetComponent<IPlayerAvatar>() != null && playerEnteredTrigger != null)
			{
				playerEnteredTrigger(this);
			}
		}

		private void OnTriggerExit(Collider collider)
		{
			if (collider.GetComponentInChildren<IPlayerAvatar>() != null && playerExitedTrigger != null)
			{
				playerExitedTrigger(this);
			}
		}
	}
}
