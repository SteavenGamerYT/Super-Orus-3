using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
	[RequireComponent(typeof(Collider))]
	public class ObjectiveReachPoint : Objective
	{
		[Tooltip("Visible transform that will be destroyed once the objective is completed")]
		public Transform DestroyRoot;

		private void Awake()
		{
			if (DestroyRoot == null)
			{
				DestroyRoot = transform;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!IsCompleted && other.GetComponent<PlayerCharacterController>() != null)
			{
				CompleteObjective(string.Empty, string.Empty, "Objective complete : " + Title);
				Object.Destroy(DestroyRoot.gameObject);
			}
		}
	}
}
