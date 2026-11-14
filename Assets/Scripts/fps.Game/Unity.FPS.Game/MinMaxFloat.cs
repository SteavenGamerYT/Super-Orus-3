using System;
using UnityEngine;

namespace Unity.FPS.Game
{
	[Serializable]
	public struct MinMaxFloat
	{
		public float Min;

		public float Max;

		public float GetValueFromRatio(float ratio)
		{
			return Mathf.Lerp(Min, Max, ratio);
		}
	}
}
