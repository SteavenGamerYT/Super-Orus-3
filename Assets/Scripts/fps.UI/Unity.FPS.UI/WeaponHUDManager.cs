using System;
using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
	public class WeaponHUDManager : MonoBehaviour
	{
		[Tooltip("UI panel containing the layoutGroup for displaying weapon ammo")]
		public RectTransform AmmoPanel;

		[Tooltip("Prefab for displaying weapon ammo")]
		public GameObject AmmoCounterPrefab;

		private PlayerWeaponsManager m_PlayerWeaponsManager;

		private List<AmmoCounter> m_AmmoCounters = new List<AmmoCounter>();

		private void Start()
		{
			m_PlayerWeaponsManager = UnityEngine.Object.FindAnyObjectByType<PlayerWeaponsManager>();
			DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, WeaponHUDManager>(m_PlayerWeaponsManager, this);
			WeaponController activeWeapon = m_PlayerWeaponsManager.GetActiveWeapon();
			if ((bool)activeWeapon)
			{
				AddWeapon(activeWeapon, m_PlayerWeaponsManager.ActiveWeaponIndex);
				ChangeWeapon(activeWeapon);
			}
			PlayerWeaponsManager playerWeaponsManager = m_PlayerWeaponsManager;
			playerWeaponsManager.OnAddedWeapon = (UnityAction<WeaponController, int>)Delegate.Combine(playerWeaponsManager.OnAddedWeapon, new UnityAction<WeaponController, int>(AddWeapon));
			PlayerWeaponsManager playerWeaponsManager2 = m_PlayerWeaponsManager;
			playerWeaponsManager2.OnRemovedWeapon = (UnityAction<WeaponController, int>)Delegate.Combine(playerWeaponsManager2.OnRemovedWeapon, new UnityAction<WeaponController, int>(RemoveWeapon));
			PlayerWeaponsManager playerWeaponsManager3 = m_PlayerWeaponsManager;
			playerWeaponsManager3.OnSwitchedToWeapon = (UnityAction<WeaponController>)Delegate.Combine(playerWeaponsManager3.OnSwitchedToWeapon, new UnityAction<WeaponController>(ChangeWeapon));
		}

		private void AddWeapon(WeaponController newWeapon, int weaponIndex)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(AmmoCounterPrefab, AmmoPanel);
			AmmoCounter component = gameObject.GetComponent<AmmoCounter>();
			DebugUtility.HandleErrorIfNullGetComponent<AmmoCounter, WeaponHUDManager>(component, this, gameObject.gameObject);
			component.Initialize(newWeapon, weaponIndex);
			m_AmmoCounters.Add(component);
		}

		private void RemoveWeapon(WeaponController newWeapon, int weaponIndex)
		{
			int num = -1;
			for (int i = 0; i < m_AmmoCounters.Count; i++)
			{
				if (m_AmmoCounters[i].WeaponCounterIndex == weaponIndex)
				{
					num = i;
					UnityEngine.Object.Destroy(m_AmmoCounters[i].gameObject);
				}
			}
			if (num >= 0)
			{
				m_AmmoCounters.RemoveAt(num);
			}
		}

		private void ChangeWeapon(WeaponController weapon)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(AmmoPanel);
		}
	}
}
