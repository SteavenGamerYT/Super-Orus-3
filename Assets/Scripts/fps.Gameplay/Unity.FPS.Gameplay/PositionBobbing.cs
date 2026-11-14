using UnityEngine;

namespace Unity.FPS.Gameplay
{
	public class PositionBobbing : MonoBehaviour
	{
		[Tooltip("Frequency at which the item will move up and down")]
		public float VerticalBobFrequency = 1f;

		[Tooltip("Distance the item will move up and down")]
		public float BobbingAmount = 0.5f;

		private Vector3 m_StartPosition;

		private void Start()
		{
			m_StartPosition = transform.position;
		}

		private void Update()
		{
			float num = (Mathf.Sin(Time.time * VerticalBobFrequency) * 0.5f + 0.5f) * BobbingAmount;
			transform.position = m_StartPosition + Vector3.up * num;
		}
	}
}
