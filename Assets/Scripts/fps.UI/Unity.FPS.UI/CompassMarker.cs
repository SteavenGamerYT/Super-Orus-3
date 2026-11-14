using System;
using TMPro;
using Unity.FPS.AI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
	public class CompassMarker : MonoBehaviour
	{
		[Tooltip("Main marker image")]
		public Image MainImage;

		[Tooltip("Canvas group for the marker")]
		public CanvasGroup CanvasGroup;

		[Header("Enemy element")]
		[Tooltip("Default color for the marker")]
		public Color DefaultColor;

		[Tooltip("Alternative color for the marker")]
		public Color AltColor;

		[Header("Direction element")]
		[Tooltip("Use this marker as a magnetic direction")]
		public bool IsDirection;

		[Tooltip("Text content for the direction")]
		public TextMeshProUGUI TextContent;

		private EnemyController m_EnemyController;

		public void Initialize(CompassElement compassElement, string textDirection)
		{
			if (IsDirection && (bool)TextContent)
			{
				TextContent.text = textDirection;
				return;
			}
			m_EnemyController = compassElement.transform.GetComponent<EnemyController>();
			if ((bool)m_EnemyController)
			{
				EnemyController enemyController = m_EnemyController;
				enemyController.onDetectedTarget = (UnityAction)Delegate.Combine(enemyController.onDetectedTarget, new UnityAction(DetectTarget));
				EnemyController enemyController2 = m_EnemyController;
				enemyController2.onLostTarget = (UnityAction)Delegate.Combine(enemyController2.onLostTarget, new UnityAction(LostTarget));
				LostTarget();
			}
		}

		public void DetectTarget()
		{
			MainImage.color = AltColor;
		}

		public void LostTarget()
		{
			MainImage.color = DefaultColor;
		}
	}
}
