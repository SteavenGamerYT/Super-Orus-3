using UnityEngine;

namespace Unity.InteractiveTutorials
{
	[SelectionBase]
	[ExecuteInEditMode]
	public class SelectionRoot : MonoBehaviour
	{
		private void Update()
		{
			transform.position = Vector3.zero;
			transform.rotation = Quaternion.identity;
			transform.localScale = Vector3.one;
		}
	}
}
