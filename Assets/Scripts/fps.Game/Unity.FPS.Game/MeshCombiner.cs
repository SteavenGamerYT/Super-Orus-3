using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
	public class MeshCombiner : MonoBehaviour
	{
		public List<GameObject> CombineParents = new List<GameObject>();

		[Header("Grid parameters")]
		public bool UseGrid;

		public Vector3 GridCenter;

		public Vector3 GridExtents = new Vector3(10f, 10f, 10f);

		public Vector3Int GridResolution = new Vector3Int(2, 2, 2);

		public Color GridPreviewColor = Color.green;

		private void Start()
		{
			Combine();
		}

		public void Combine()
		{
			List<MeshRenderer> list = new List<MeshRenderer>();
			foreach (GameObject combineParent in CombineParents)
			{
				list.AddRange(combineParent.GetComponentsInChildren<MeshRenderer>());
			}
			if (UseGrid)
			{
				for (int i = 0; i < GetGridCellCount(); i++)
				{
					if (GetGridCellBounds(i, out var bounds))
					{
						CombineAllInBounds(bounds, list);
					}
				}
			}
			else
			{
				MeshCombineUtility.Combine(list, MeshCombineUtility.RendererDisposeMethod.DestroyRendererAndFilter, "Level_Combined");
			}
		}

		private void CombineAllInBounds(Bounds bounds, List<MeshRenderer> validRenderers)
		{
			List<MeshRenderer> list = new List<MeshRenderer>();
			for (int num = validRenderers.Count - 1; num >= 0; num--)
			{
				MeshRenderer meshRenderer = validRenderers[num];
				if (bounds.Intersects(meshRenderer.bounds))
				{
					list.Add(meshRenderer);
					validRenderers.Remove(meshRenderer);
				}
			}
			if (list.Count > 0)
			{
				MeshCombineUtility.Combine(list, MeshCombineUtility.RendererDisposeMethod.DestroyRendererAndFilter, "Level_Combined");
			}
		}

		private int GetGridCellCount()
		{
			return GridResolution.x * GridResolution.y * GridResolution.z;
		}

		public bool GetGridCellBounds(int index, out Bounds bounds)
		{
			bounds = default;
			if (index < 0 || index >= GetGridCellCount())
			{
				return false;
			}
			int num = index / (GridResolution.y * GridResolution.z);
			int num2 = index / GridResolution.z % GridResolution.y;
			int num3 = index % GridResolution.z;
			Vector3 vector = GridCenter - GridExtents * 0.5f;
			Vector3 size = new Vector3(GridExtents.x / (float)GridResolution.x, GridExtents.y / (float)GridResolution.y, GridExtents.z / (float)GridResolution.z);
			Vector3 center = vector + new Vector3((float)num * size.x + size.x * 0.5f, (float)num2 * size.y + size.y * 0.5f, (float)num3 * size.z + size.z * 0.5f);
			bounds.center = center;
			bounds.size = size;
			return true;
		}

		private void OnDrawGizmosSelected()
		{
			if (!UseGrid)
			{
				return;
			}
			Gizmos.color = GridPreviewColor;
			for (int i = 0; i < GetGridCellCount(); i++)
			{
				if (GetGridCellBounds(i, out var bounds))
				{
					Gizmos.DrawWireCube(bounds.center, bounds.size);
				}
			}
		}
	}
}
