using System;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
	public class StanceHUD : MonoBehaviour
	{
		[Tooltip("Image component for the stance sprites")]
		public Image StanceImage;

		[Tooltip("Sprite to display when standing")]
		public Sprite StandingSprite;

		[Tooltip("Sprite to display when crouching")]
		public Sprite CrouchingSprite;

		private void Start()
		{
			PlayerCharacterController playerCharacterController = UnityEngine.Object.FindAnyObjectByType<PlayerCharacterController>();
			DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, StanceHUD>(playerCharacterController, this);
			playerCharacterController.OnStanceChanged = (UnityAction<bool>)Delegate.Combine(playerCharacterController.OnStanceChanged, new UnityAction<bool>(OnStanceChanged));
			OnStanceChanged(playerCharacterController.IsCrouching);
		}

		private void OnStanceChanged(bool crouched)
		{
			StanceImage.sprite = (crouched ? CrouchingSprite : StandingSprite);
		}
	}
}
