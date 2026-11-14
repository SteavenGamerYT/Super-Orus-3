using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;

namespace Unity.FPS.Game
{
	public static class MeshCombineUtility
	{
		public class RenderBatchData
		{
			public class MeshAndTrs
			{
				public Mesh Mesh;

				public Matrix4x4 Trs;

				public MeshAndTrs(Mesh m, Matrix4x4 t)
				{
					Mesh = m;
					Trs = t;
				}
			}

			public Material Material;

			public int SubmeshIndex;

			public ShadowCastingMode ShadowMode;

			public bool ReceiveShadows;

			public MotionVectorGenerationMode MotionVectors;

			public List<MeshAndTrs> MeshesWithTrs = new List<MeshAndTrs>();
		}

		public enum RendererDisposeMethod
		{
			DestroyGameObject,
			DestroyRendererAndFilter,
			DisableGameObject,
			DisableRenderer
		}

		public static void Combine(List<MeshRenderer> renderers, RendererDisposeMethod disposeMethod, string newObjectName)
		{
			int count = renderers.Count;
			List<RenderBatchData> list = new List<RenderBatchData>();
			for (int i = 0; i < count; i++)
			{
				MeshRenderer meshRenderer = renderers[i];
				if (meshRenderer == null)
				{
					continue;
				}
				MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
				if (component == null)
				{
					continue;
				}
				Mesh sharedMesh = component.sharedMesh;
				if (sharedMesh == null)
				{
					continue;
				}
				Transform component2 = meshRenderer.GetComponent<Transform>();
				Material[] sharedMaterials = meshRenderer.sharedMaterials;
				for (int j = 0; j < sharedMesh.subMeshCount; j++)
				{
					if (!(sharedMaterials[j] == null))
					{
						int existingRenderBatch = GetExistingRenderBatch(list, sharedMaterials[j], meshRenderer, j);
						if (existingRenderBatch >= 0)
						{
							list[existingRenderBatch].MeshesWithTrs.Add(new RenderBatchData.MeshAndTrs(sharedMesh, Matrix4x4.TRS(component2.position, component2.rotation, component2.lossyScale)));
							continue;
						}
						RenderBatchData renderBatchData = new RenderBatchData();
						renderBatchData.Material = sharedMaterials[j];
						renderBatchData.SubmeshIndex = j;
						renderBatchData.ShadowMode = meshRenderer.shadowCastingMode;
						renderBatchData.ReceiveShadows = meshRenderer.receiveShadows;
						renderBatchData.MeshesWithTrs.Add(new RenderBatchData.MeshAndTrs(sharedMesh, Matrix4x4.TRS(component2.position, component2.rotation, component2.lossyScale)));
						list.Add(renderBatchData);
					}
				}
				ProBuilderMesh component3 = meshRenderer.GetComponent<ProBuilderMesh>();
				if ((bool)component3)
				{
					Object.Destroy(component3);
				}
				switch (disposeMethod)
				{
				case RendererDisposeMethod.DestroyGameObject:
					if (Application.isPlaying)
					{
						Object.Destroy(meshRenderer.gameObject);
					}
					else
					{
						Object.DestroyImmediate(meshRenderer.gameObject);
					}
					break;
				case RendererDisposeMethod.DestroyRendererAndFilter:
					if (Application.isPlaying)
					{
						Object.Destroy(meshRenderer);
						Object.Destroy(component);
					}
					else
					{
						Object.DestroyImmediate(meshRenderer);
						Object.DestroyImmediate(component);
					}
					break;
				case RendererDisposeMethod.DisableGameObject:
					meshRenderer.gameObject.SetActive(value: false);
					break;
				case RendererDisposeMethod.DisableRenderer:
					meshRenderer.enabled = false;
					break;
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				RenderBatchData renderBatchData2 = list[k];
				Mesh mesh = new Mesh();
				mesh.indexFormat = IndexFormat.UInt32;
				CombineInstance[] array = new CombineInstance[renderBatchData2.MeshesWithTrs.Count];
				for (int l = 0; l < renderBatchData2.MeshesWithTrs.Count; l++)
				{
					array[l].subMeshIndex = renderBatchData2.SubmeshIndex;
					array[l].mesh = renderBatchData2.MeshesWithTrs[l].Mesh;
					array[l].transform = renderBatchData2.MeshesWithTrs[l].Trs;
				}
				mesh.CombineMeshes(array);
				mesh.RecalculateBounds();
				GameObject gameObject = new GameObject(newObjectName);
				gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
				MeshRenderer meshRenderer2 = gameObject.AddComponent<MeshRenderer>();
				meshRenderer2.sharedMaterial = renderBatchData2.Material;
				meshRenderer2.shadowCastingMode = renderBatchData2.ShadowMode;
			}
		}

		private static int GetExistingRenderBatch(List<RenderBatchData> renderBatches, Material mat, MeshRenderer ren, int submeshIndex)
		{
			for (int i = 0; i < renderBatches.Count; i++)
			{
				RenderBatchData renderBatchData = renderBatches[i];
				if (renderBatchData.Material == mat && renderBatchData.SubmeshIndex == submeshIndex && renderBatchData.ShadowMode == ren.shadowCastingMode && renderBatchData.ReceiveShadows == ren.receiveShadows)
				{
					return i;
				}
			}
			return -1;
		}
	}
}
