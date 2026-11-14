using System;
using UnityEngine;

namespace Unity.FPS.Game
{
	[Serializable]
	public struct MinMaxColor
	{
		[ColorUsage(true, true)]
		public Color Min;

		[ColorUsage(true, true)]
		public Color Max;

		public Color GetValueFromRatio(float ratio)
		{
			return Color.Lerp(Min, Max, ratio);
		}
	}
}
