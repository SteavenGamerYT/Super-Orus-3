using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
	public class TakeScreenshot : MonoBehaviour
	{
		[Tooltip("Root of the screenshot panel in the menu")]
		public GameObject ScreenshotPanel;

		[Tooltip("Name for the screenshot file")]
		public string FileName = "Screenshot";

		[Tooltip("Image to display the screenshot in")]
		public RawImage PreviewImage;

		private CanvasGroup m_MenuCanvas;

		private Texture2D m_Texture;

		private bool m_TakeScreenshot;

		private bool m_ScreenshotTaken;

		private bool m_IsFeatureDisable;

		private const string k_ScreenshotPath = "Assets/";

		private string GetPath()
		{
			return "Assets/" + FileName + ".png";
		}

		private void Awake()
		{
			m_MenuCanvas = GetComponent<CanvasGroup>();
			ScreenshotPanel.SetActive(value: false);
			m_IsFeatureDisable = true;
		}

		private void Update()
		{
			PreviewImage.enabled = PreviewImage.texture != null;
			if (!m_IsFeatureDisable)
			{
				if (m_TakeScreenshot)
				{
					m_MenuCanvas.alpha = 0f;
					ScreenCapture.CaptureScreenshot(GetPath());
					m_TakeScreenshot = false;
					m_ScreenshotTaken = true;
				}
				else if (m_ScreenshotTaken)
				{
					LoadScreenshot();
					m_MenuCanvas.alpha = 1f;
					m_ScreenshotTaken = false;
				}
			}
		}

		public void OnTakeScreenshotButtonPressed()
		{
			m_TakeScreenshot = true;
		}

		private void LoadScreenshot()
		{
			if (File.Exists(GetPath()))
			{
				byte[] data = File.ReadAllBytes(GetPath());
				m_Texture = new Texture2D(2, 2);
				m_Texture.LoadImage(data);
				m_Texture.Apply();
				PreviewImage.texture = m_Texture;
			}
		}
	}
}
