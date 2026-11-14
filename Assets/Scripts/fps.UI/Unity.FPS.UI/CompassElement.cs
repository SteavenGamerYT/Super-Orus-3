using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.UI
{
	public class CompassElement : MonoBehaviour
	{
		[Tooltip("The marker on the compass for this element")]
		public CompassMarker CompassMarkerPrefab;

		[Tooltip("Text override for the marker, if it's a direction")]
		public string TextDirection;

		private Compass m_Compass;

		private void Awake()
		{
			m_Compass = Object.FindAnyObjectByType<Compass>();
			DebugUtility.HandleErrorIfNullFindObject<Compass, CompassElement>(m_Compass, this);
			CompassMarker compassMarker = Object.Instantiate(CompassMarkerPrefab);
			compassMarker.Initialize(this, TextDirection);
			m_Compass.RegisterCompassElement(transform, compassMarker);
		}

		private void OnDestroy()
		{
			m_Compass.UnregisterCompassElement(transform);
		}
	}
}
