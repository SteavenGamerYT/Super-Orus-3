using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.UI
{
	public class DisplayMessageManager : MonoBehaviour
	{
		public UITable DisplayMessageRect;

		public NotificationToast MessagePrefab;

		private List<(float timestamp, float delay, string message, NotificationToast notification)> m_PendingMessages;

		private void Awake()
		{
			EventManager.AddListener<DisplayMessageEvent>(OnDisplayMessageEvent);
			m_PendingMessages = new List<(float, float, string, NotificationToast)>();
		}

		private void OnDisplayMessageEvent(DisplayMessageEvent evt)
		{
			NotificationToast component = Object.Instantiate(MessagePrefab, DisplayMessageRect.transform).GetComponent<NotificationToast>();
			m_PendingMessages.Add((Time.time, evt.DelayBeforeDisplay, evt.Message, component));
		}

		private void Update()
		{
			foreach (var pendingMessage in m_PendingMessages)
			{
				if (Time.time - pendingMessage.timestamp > pendingMessage.delay)
				{
					pendingMessage.notification.Initialize(pendingMessage.message);
					DisplayMessage(pendingMessage.notification);
				}
			}
			m_PendingMessages.RemoveAll(((float timestamp, float delay, string message, NotificationToast notification) x) => x.notification.Initialized);
		}

		private void DisplayMessage(NotificationToast notification)
		{
			DisplayMessageRect.UpdateTable(notification.gameObject);
		}

		private void OnDestroy()
		{
			EventManager.RemoveListener<DisplayMessageEvent>(OnDisplayMessageEvent);
		}
	}
}
