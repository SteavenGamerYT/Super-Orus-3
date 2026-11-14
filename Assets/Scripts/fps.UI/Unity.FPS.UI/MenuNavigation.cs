using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
	public class MenuNavigation : MonoBehaviour
	{
		public Selectable DefaultSelection;

		private void Start()
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			EventSystem.current.SetSelectedGameObject(null);
		}

		private void LateUpdate()
		{
			if (EventSystem.current.currentSelectedGameObject == null && (Input.GetButtonDown("Submit") || Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f))
			{
				EventSystem.current.SetSelectedGameObject(DefaultSelection.gameObject);
			}
		}
	}
}
