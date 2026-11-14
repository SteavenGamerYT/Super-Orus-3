using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
	[RequireComponent(typeof(Rigidbody), typeof(Collider))]
	public class Pickup : MonoBehaviour
	{
		[Tooltip("Frequency at which the item will move up and down")]
		public float VerticalBobFrequency = 1f;

		[Tooltip("Distance the item will move up and down")]
		public float BobbingAmount = 1f;

		[Tooltip("Rotation angle per second")]
		public float RotatingSpeed = 360f;

		[Tooltip("Sound played on pickup")]
		public AudioClip PickupSfx;

		[Tooltip("VFX spawned on pickup")]
		public GameObject PickupVfxPrefab;

		private Collider m_Collider;

		private Vector3 m_StartPosition;

		private bool m_HasPlayedFeedback;

		public Rigidbody PickupRigidbody { get; private set; }

		protected virtual void Start()
		{
			PickupRigidbody = GetComponent<Rigidbody>();
			DebugUtility.HandleErrorIfNullGetComponent<Rigidbody, Pickup>(PickupRigidbody, this, gameObject);
			m_Collider = GetComponent<Collider>();
			DebugUtility.HandleErrorIfNullGetComponent<Collider, Pickup>(m_Collider, this, gameObject);
			PickupRigidbody.isKinematic = true;
			m_Collider.isTrigger = true;
			m_StartPosition = transform.position;
		}

		private void Update()
		{
			float num = (Mathf.Sin(Time.time * VerticalBobFrequency) * 0.5f + 0.5f) * BobbingAmount;
			transform.position = m_StartPosition + Vector3.up * num;
			transform.Rotate(Vector3.up, RotatingSpeed * Time.deltaTime, Space.Self);
		}

		private void OnTriggerEnter(Collider other)
		{
			PlayerCharacterController component = other.GetComponent<PlayerCharacterController>();
			if (component != null)
			{
				OnPicked(component);
				PickupEvent pickupEvent = Events.PickupEvent;
				pickupEvent.Pickup = gameObject;
				EventManager.Broadcast(pickupEvent);
			}
		}

		protected virtual void OnPicked(PlayerCharacterController playerController)
		{
			PlayPickupFeedback();
		}

		public void PlayPickupFeedback()
		{
			if (!m_HasPlayedFeedback)
			{
				if ((bool)PickupSfx)
				{
					AudioUtility.CreateSFX(PickupSfx, transform.position, AudioUtility.AudioGroups.Pickup, 0f);
				}
				if ((bool)PickupVfxPrefab)
				{
					Object.Instantiate(PickupVfxPrefab, transform.position, Quaternion.identity);
				}
				m_HasPlayedFeedback = true;
			}
		}
	}
}
