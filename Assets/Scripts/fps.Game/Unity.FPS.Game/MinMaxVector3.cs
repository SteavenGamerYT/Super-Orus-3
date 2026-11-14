using System;
using UnityEngine;

namespace Unity.FPS.Game
{
	[Serializable]
	public struct MinMaxVector3
	{
		public Vector3 Min;

		public Vector3 Max;

		public Vector3 GetValueFromRatio(float ratio)
		{
			return Vector3.Lerp(Min, Max, ratio);
		}
	}
}
