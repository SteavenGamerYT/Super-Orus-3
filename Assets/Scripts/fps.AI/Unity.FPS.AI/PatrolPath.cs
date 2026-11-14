using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.AI
{
	public class PatrolPath : MonoBehaviour
	{
		[Tooltip("Enemies that will be assigned to this path on Start")]
		public List<EnemyController> EnemiesToAssign = new List<EnemyController>();

		[Tooltip("The Nodes making up the path")]
		public List<Transform> PathNodes = new List<Transform>();

		private void Start()
		{
			foreach (EnemyController item in EnemiesToAssign)
			{
				item.PatrolPath = this;
			}
		}

		public float GetDistanceToNode(Vector3 origin, int destinationNodeIndex)
		{
			if (destinationNodeIndex < 0 || destinationNodeIndex >= PathNodes.Count || PathNodes[destinationNodeIndex] == null)
			{
				return -1f;
			}
			return (PathNodes[destinationNodeIndex].position - origin).magnitude;
		}

		public Vector3 GetPositionOfPathNode(int nodeIndex)
		{
			if (nodeIndex < 0 || nodeIndex >= PathNodes.Count || PathNodes[nodeIndex] == null)
			{
				return Vector3.zero;
			}
			return PathNodes[nodeIndex].position;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.cyan;
			for (int i = 0; i < PathNodes.Count; i++)
			{
				int num = i + 1;
				if (num >= PathNodes.Count)
				{
					num -= PathNodes.Count;
				}
				Gizmos.DrawLine(PathNodes[i].position, PathNodes[num].position);
				Gizmos.DrawSphere(PathNodes[i].position, 0.1f);
			}
		}
	}
}
