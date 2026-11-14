using System;
using UnityEngine;

namespace Unity.FPS.Game
{
	[Serializable]
	public struct CrosshairData
	{
		[Tooltip("The image that will be used for this weapon's crosshair")]
		public Sprite CrosshairSprite;

		[Tooltip("The size of the crosshair image")]
		public int CrosshairSize;

		[Tooltip("The color of the crosshair image")]
		public Color CrosshairColor;
	}
}
