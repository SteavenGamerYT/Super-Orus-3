using TMPro;
using UnityEngine;

namespace Unity.FPS.UI
{
	public class NotificationToast : MonoBehaviour
	{
		[Tooltip("Text content that will display the notification text")]
		public TextMeshProUGUI TextContent;

		[Tooltip("Canvas used to fade in and out the content")]
		public CanvasGroup CanvasGroup;

		[Tooltip("How long it will stay visible")]
		public float VisibleDuration;

		[Tooltip("Duration of the fade in")]
		public float FadeInDuration = 0.5f;

		[Tooltip("Duration of the fade out")]
		public float FadeOutDuration = 2f;

		private float m_InitTime;

		public bool Initialized { get; private set; }

		public float TotalRunTime => VisibleDuration + FadeInDuration + FadeOutDuration;

		public void Initialize(string text)
		{
			TextContent.text = text;
			m_InitTime = Time.time;
			Initialized = true;
		}

		private void Update()
		{
			if (Initialized)
			{
				float num = Time.time - m_InitTime;
				if (num < FadeInDuration)
				{
					CanvasGroup.alpha = num / FadeInDuration;
					return;
				}
				if (num < FadeInDuration + VisibleDuration)
				{
					CanvasGroup.alpha = 1f;
					return;
				}
				if (num < FadeInDuration + VisibleDuration + FadeOutDuration)
				{
					CanvasGroup.alpha = 1f - (num - FadeInDuration - VisibleDuration) / FadeOutDuration;
					return;
				}
				CanvasGroup.alpha = 0f;
				Object.Destroy(gameObject);
			}
		}
	}
}
