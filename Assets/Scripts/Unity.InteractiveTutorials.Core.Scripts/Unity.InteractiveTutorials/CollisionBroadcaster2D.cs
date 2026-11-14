using UnityEngine;

namespace Unity.InteractiveTutorials
{
	[RequireComponent(typeof(Collider2D))]
	public class CollisionBroadcaster2D : BaseCollisionBroadcaster
	{
		public delegate void EventHandler(CollisionBroadcaster2D sender);

		public static event EventHandler playerEnteredCollision;

		public static event EventHandler playerExitedCollision;

		public static event EventHandler playerEnteredTrigger;

		public static event EventHandler playerExitedTrigger;

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.gameObject.GetComponentInChildren<IPlayerAvatar>() != null && playerEnteredCollision != null)
			{
				playerEnteredCollision(this);
			}
		}

		private void OnCollisionExit2D(Collision2D collision)
		{
			if (collision.gameObject.GetComponentInChildren<IPlayerAvatar>() != null && playerExitedCollision != null)
			{
				playerExitedCollision(this);
			}
		}

		private void OnTriggerEnter2D(Collider2D collider)
		{
			if (collider.GetComponentInChildren<IPlayerAvatar>() != null && playerEnteredTrigger != null)
			{
				playerEnteredTrigger(this);
			}
		}

		private void OnTriggerExit2D(Collider2D collider)
		{
			if (collider.GetComponentInChildren<IPlayerAvatar>() != null && playerExitedTrigger != null)
			{
				playerExitedTrigger(this);
			}
		}
	}
}
