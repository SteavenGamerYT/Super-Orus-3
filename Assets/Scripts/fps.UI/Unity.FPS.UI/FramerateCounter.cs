using TMPro;
using UnityEngine;

namespace Unity.FPS.UI
{
	public class FramerateCounter : MonoBehaviour
	{
		[Tooltip("Delay between updates of the displayed framerate value")]
		public float PollingTime = 0.5f;

		[Tooltip("The text field displaying the framerate")]
		public TextMeshProUGUI UIText;

		private float m_AccumulatedDeltaTime;

		private int m_AccumulatedFrameCount;

		private void Update()
		{
			m_AccumulatedDeltaTime += Time.deltaTime;
			m_AccumulatedFrameCount++;
			if (m_AccumulatedDeltaTime >= PollingTime)
			{
				int num = Mathf.RoundToInt((float)m_AccumulatedFrameCount / m_AccumulatedDeltaTime);
				UIText.text = num.ToString();
				m_AccumulatedDeltaTime = 0f;
				m_AccumulatedFrameCount = 0;
			}
		}
	}
}
