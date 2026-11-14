using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
	public class Compass : MonoBehaviour
	{
		public RectTransform CompasRect;

		public float VisibilityAngle = 180f;

		public float HeightDifferenceMultiplier = 2f;

		public float MinScale = 0.5f;

		public float DistanceMinScale = 50f;

		public float CompasMarginRatio = 0.8f;

		public GameObject MarkerDirectionPrefab;

		private Transform m_PlayerTransform;

		private Dictionary<Transform, CompassMarker> m_ElementsDictionnary = new Dictionary<Transform, CompassMarker>();

		private float m_WidthMultiplier;

		private float m_HeightOffset;

		private void Awake()
		{
			PlayerCharacterController playerCharacterController = Object.FindAnyObjectByType<PlayerCharacterController>();
			DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, Compass>(playerCharacterController, this);
			m_PlayerTransform = playerCharacterController.transform;
			m_WidthMultiplier = CompasRect.rect.width / VisibilityAngle;
			m_HeightOffset = (0f - CompasRect.rect.height) / 2f;
		}

		private void Update()
		{
			foreach (KeyValuePair<Transform, CompassMarker> item in m_ElementsDictionnary)
			{
				float t = 1f;
				float num = 0f;
				float num2;
				if (item.Value.IsDirection)
				{
					num2 = Vector3.SignedAngle(m_PlayerTransform.forward, item.Key.transform.localPosition.normalized, Vector3.up);
				}
				else
				{
					Vector3 normalized = (item.Key.transform.position - m_PlayerTransform.position).normalized;
					normalized = Vector3.ProjectOnPlane(normalized, Vector3.up);
					num2 = Vector3.SignedAngle(Vector3.ProjectOnPlane(m_PlayerTransform.forward, Vector3.up), normalized, Vector3.up);
					Vector3 vector = item.Key.transform.position - m_PlayerTransform.position;
					num = vector.y * HeightDifferenceMultiplier;
					num = Mathf.Clamp(num, (0f - CompasRect.rect.height) / 2f * CompasMarginRatio, CompasRect.rect.height / 2f * CompasMarginRatio);
					t = vector.magnitude / DistanceMinScale;
					t = Mathf.Clamp01(t);
				}
				if (num2 > (0f - VisibilityAngle) / 2f && num2 < VisibilityAngle / 2f)
				{
					item.Value.CanvasGroup.alpha = 1f;
					item.Value.CanvasGroup.transform.localPosition = new Vector2(m_WidthMultiplier * num2, num + m_HeightOffset);
					item.Value.CanvasGroup.transform.localScale = Vector3.one * Mathf.Lerp(1f, MinScale, t);
				}
				else
				{
					item.Value.CanvasGroup.alpha = 0f;
				}
			}
		}

		public void RegisterCompassElement(Transform element, CompassMarker marker)
		{
			marker.transform.SetParent(CompasRect);
			m_ElementsDictionnary.Add(element, marker);
		}

		public void UnregisterCompassElement(Transform element)
		{
			if (m_ElementsDictionnary.TryGetValue(element, out var value) && value.CanvasGroup != null)
			{
				Object.Destroy(value.CanvasGroup.gameObject);
			}
			m_ElementsDictionnary.Remove(element);
		}
	}
}
