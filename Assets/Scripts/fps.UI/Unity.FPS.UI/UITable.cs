using UnityEngine;

namespace Unity.FPS.UI
{
	public class UITable : MonoBehaviour
	{
		[Tooltip("How much space should there be between items?")]
		public float Offset;

		[Tooltip("Add new the new items below existing items.")]
		public bool Down;

		public void UpdateTable(GameObject newItem)
		{
			if (newItem != null)
			{
				newItem.GetComponent<RectTransform>().localScale = Vector3.one;
			}
			float num = 0f;
			for (int i = 0; i < transform.childCount; i++)
			{
				RectTransform component = transform.GetChild(i).GetComponent<RectTransform>();
				Vector2 sizeDelta = component.sizeDelta;
				num += (Down ? ((0f - (1f - component.pivot.y)) * sizeDelta.y) : ((1f - component.pivot.y) * sizeDelta.y));
				if (i != 0)
				{
					num += (Down ? (0f - Offset) : Offset);
				}
				Vector2 zero = Vector2.zero;
				zero.y = num;
				zero.x = 0f;
				component.anchoredPosition = zero;
			}
		}
	}
}
