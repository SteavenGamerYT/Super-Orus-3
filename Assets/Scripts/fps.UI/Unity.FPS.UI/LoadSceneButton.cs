using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Unity.FPS.UI
{
	public class LoadSceneButton : MonoBehaviour
	{
		public string SceneName = "";

		private void Update()
		{
			if (EventSystem.current.currentSelectedGameObject == gameObject && Input.GetButtonDown("Submit"))
			{
				LoadTargetScene();
			}
		}

		public void LoadTargetScene()
		{
			SceneManager.LoadScene(SceneName);
		}
	}
}
