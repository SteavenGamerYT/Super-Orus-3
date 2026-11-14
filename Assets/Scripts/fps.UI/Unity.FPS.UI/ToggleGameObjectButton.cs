using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.FPS.UI
{
	public class ToggleGameObjectButton : MonoBehaviour
	{
		public GameObject ObjectToToggle;

		public bool ResetSelectionAfterClick;

		private void Update()
		{
			if (ObjectToToggle.activeSelf && Input.GetButtonDown("Cancel"))
			{
				SetGameObjectActive(active: false);
			}
		}

		public void SetGameObjectActive(bool active)
		{
			ObjectToToggle.SetActive(active);
			if (ResetSelectionAfterClick)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
	}
}
