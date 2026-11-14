using System;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.UI
{
	public class NotificationHUDManager : MonoBehaviour
	{
		[Tooltip("UI panel containing the layoutGroup for displaying notifications")]
		public RectTransform NotificationPanel;

		[Tooltip("Prefab for the notifications")]
		public GameObject NotificationPrefab;

		private void Awake()
		{
			PlayerWeaponsManager playerWeaponsManager = UnityEngine.Object.FindAnyObjectByType<PlayerWeaponsManager>();
			DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, NotificationHUDManager>(playerWeaponsManager, this);
			playerWeaponsManager.OnAddedWeapon = (UnityAction<WeaponController, int>)Delegate.Combine(playerWeaponsManager.OnAddedWeapon, new UnityAction<WeaponController, int>(OnPickupWeapon));
			Jetpack jetpack = UnityEngine.Object.FindAnyObjectByType<Jetpack>();
			DebugUtility.HandleErrorIfNullFindObject<Jetpack, NotificationHUDManager>(jetpack, this);
			jetpack.OnUnlockJetpack = (UnityAction<bool>)Delegate.Combine(jetpack.OnUnlockJetpack, new UnityAction<bool>(OnUnlockJetpack));
			EventManager.AddListener<ObjectiveUpdateEvent>(OnObjectiveUpdateEvent);
		}

		private void OnObjectiveUpdateEvent(ObjectiveUpdateEvent evt)
		{
			if (!string.IsNullOrEmpty(evt.NotificationText))
			{
				CreateNotification(evt.NotificationText);
			}
		}

		private void OnPickupWeapon(WeaponController weaponController, int index)
		{
			if (index != 0)
			{
				CreateNotification("Picked up weapon : " + weaponController.WeaponName);
			}
		}

		private void OnUnlockJetpack(bool unlock)
		{
			CreateNotification("Jetpack unlocked");
		}

		public void CreateNotification(string text)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(NotificationPrefab, NotificationPanel);
			gameObject.transform.SetSiblingIndex(0);
			NotificationToast component = gameObject.GetComponent<NotificationToast>();
			if ((bool)component)
			{
				component.Initialize(text);
			}
		}

		private void OnDestroy()
		{
			EventManager.RemoveListener<ObjectiveUpdateEvent>(OnObjectiveUpdateEvent);
		}
	}
}
