using TMPro;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
	public class ObjectiveToast : MonoBehaviour
	{
		[Header("References")]
		[Tooltip("Text content that will display the title")]
		public TextMeshProUGUI TitleTextContent;

		[Tooltip("Text content that will display the description")]
		public TextMeshProUGUI DescriptionTextContent;

		[Tooltip("Text content that will display the counter")]
		public TextMeshProUGUI CounterTextContent;

		[Tooltip("Rect that will display the description")]
		public RectTransform SubTitleRect;

		[Tooltip("Canvas used to fade in and out the content")]
		public CanvasGroup CanvasGroup;

		[Tooltip("Layout group containing the objective")]
		public HorizontalOrVerticalLayoutGroup LayoutGroup;

		[Header("Transitions")]
		[Tooltip("Delay before moving complete")]
		public float CompletionDelay;

		[Tooltip("Duration of the fade in")]
		public float FadeInDuration = 0.5f;

		[Tooltip("Duration of the fade out")]
		public float FadeOutDuration = 2f;

		[Header("Sound")]
		[Tooltip("Sound that will be player on initialization")]
		public AudioClip InitSound;

		[Tooltip("Sound that will be player on completion")]
		public AudioClip CompletedSound;

		[Header("Movement")]
		[Tooltip("Time it takes to move in the screen")]
		public float MoveInDuration = 0.5f;

		[Tooltip("Animation curve for move in, position in x over time")]
		public AnimationCurve MoveInCurve;

		[Tooltip("Time it takes to move out of the screen")]
		public float MoveOutDuration = 2f;

		[Tooltip("Animation curve for move out, position in x over time")]
		public AnimationCurve MoveOutCurve;

		private float m_StartFadeTime;

		private bool m_IsFadingIn;

		private bool m_IsFadingOut;

		private bool m_IsMovingIn;

		private bool m_IsMovingOut;

		private AudioSource m_AudioSource;

		public void Initialize(string titleText, string descText, string counterText, bool isOptionnal, float delay)
		{
			Canvas.ForceUpdateCanvases();
			TitleTextContent.text = titleText;
			DescriptionTextContent.text = descText;
			CounterTextContent.text = counterText;
			if ((bool)GetComponent<RectTransform>())
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
			}
			m_StartFadeTime = Time.time + delay;
			m_IsFadingIn = true;
			m_IsMovingIn = true;
		}

		public void Complete()
		{
			m_StartFadeTime = Time.time + CompletionDelay;
			m_IsFadingIn = false;
			m_IsMovingIn = false;
			PlaySound(CompletedSound);
			m_IsFadingOut = true;
			m_IsMovingOut = true;
		}

		private void Update()
		{
			float num = Time.time - m_StartFadeTime;
			SubTitleRect.gameObject.SetActive(!string.IsNullOrEmpty(DescriptionTextContent.text));
			if (m_IsFadingIn && !m_IsFadingOut)
			{
				if (num < FadeInDuration)
				{
					CanvasGroup.alpha = num / FadeInDuration;
				}
				else
				{
					CanvasGroup.alpha = 1f;
					m_IsFadingIn = false;
					PlaySound(InitSound);
				}
			}
			if (m_IsMovingIn && !m_IsMovingOut)
			{
				if (num < MoveInDuration)
				{
					LayoutGroup.padding.left = (int)MoveInCurve.Evaluate(num / MoveInDuration);
					if ((bool)GetComponent<RectTransform>())
					{
						LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
					}
				}
				else
				{
					LayoutGroup.padding.left = 0;
					if ((bool)GetComponent<RectTransform>())
					{
						LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
					}
					m_IsMovingIn = false;
				}
			}
			if (m_IsFadingOut)
			{
				if (num < FadeOutDuration)
				{
					CanvasGroup.alpha = 1f - num / FadeOutDuration;
				}
				else
				{
					CanvasGroup.alpha = 0f;
					m_IsFadingOut = false;
					Object.Destroy(gameObject);
				}
			}
			if (!m_IsMovingOut)
			{
				return;
			}
			if (num < MoveOutDuration)
			{
				LayoutGroup.padding.left = (int)MoveOutCurve.Evaluate(num / MoveOutDuration);
				if ((bool)GetComponent<RectTransform>())
				{
					LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
				}
			}
			else
			{
				m_IsMovingOut = false;
			}
		}

		private void PlaySound(AudioClip sound)
		{
			if ((bool)sound)
			{
				if (!m_AudioSource)
				{
					m_AudioSource = gameObject.AddComponent<AudioSource>();
					m_AudioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDObjective);
				}
				m_AudioSource.PlayOneShot(sound);
			}
		}
	}
}
