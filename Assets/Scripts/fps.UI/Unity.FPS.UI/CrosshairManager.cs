using System;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
	public class CrosshairManager : MonoBehaviour
	{
		public Image CrosshairImage;

		public Sprite NullCrosshairSprite;

		public float CrosshairUpdateshrpness = 5f;

		private PlayerWeaponsManager m_WeaponsManager;

		private bool m_WasPointingAtEnemy;

		private RectTransform m_CrosshairRectTransform;

		private CrosshairData m_CrosshairDataDefault;

		private CrosshairData m_CrosshairDataTarget;

		private CrosshairData m_CurrentCrosshair;

		private void Start()
		{
			m_WeaponsManager = UnityEngine.Object.FindAnyObjectByType<PlayerWeaponsManager>();
			DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, CrosshairManager>(m_WeaponsManager, this);
			OnWeaponChanged(m_WeaponsManager.GetActiveWeapon());
			PlayerWeaponsManager weaponsManager = m_WeaponsManager;
			weaponsManager.OnSwitchedToWeapon = (UnityAction<WeaponController>)Delegate.Combine(weaponsManager.OnSwitchedToWeapon, new UnityAction<WeaponController>(OnWeaponChanged));
		}

		private void Update()
		{
			UpdateCrosshairPointingAtEnemy(force: false);
			m_WasPointingAtEnemy = m_WeaponsManager.IsPointingAtEnemy;
		}

		private void UpdateCrosshairPointingAtEnemy(bool force)
		{
			if (!(m_CrosshairDataDefault.CrosshairSprite == null))
			{
				if ((force || !m_WasPointingAtEnemy) && m_WeaponsManager.IsPointingAtEnemy)
				{
					m_CurrentCrosshair = m_CrosshairDataTarget;
					CrosshairImage.sprite = m_CurrentCrosshair.CrosshairSprite;
					m_CrosshairRectTransform.sizeDelta = m_CurrentCrosshair.CrosshairSize * Vector2.one;
				}
				else if ((force || m_WasPointingAtEnemy) && !m_WeaponsManager.IsPointingAtEnemy)
				{
					m_CurrentCrosshair = m_CrosshairDataDefault;
					CrosshairImage.sprite = m_CurrentCrosshair.CrosshairSprite;
					m_CrosshairRectTransform.sizeDelta = m_CurrentCrosshair.CrosshairSize * Vector2.one;
				}
				CrosshairImage.color = Color.Lerp(CrosshairImage.color, m_CurrentCrosshair.CrosshairColor, Time.deltaTime * CrosshairUpdateshrpness);
				m_CrosshairRectTransform.sizeDelta = Mathf.Lerp(m_CrosshairRectTransform.sizeDelta.x, m_CurrentCrosshair.CrosshairSize, Time.deltaTime * CrosshairUpdateshrpness) * Vector2.one;
			}
		}

		private void OnWeaponChanged(WeaponController newWeapon)
		{
			if ((bool)newWeapon)
			{
				CrosshairImage.enabled = true;
				m_CrosshairDataDefault = newWeapon.CrosshairDataDefault;
				m_CrosshairDataTarget = newWeapon.CrosshairDataTargetInSight;
				m_CrosshairRectTransform = CrosshairImage.GetComponent<RectTransform>();
				DebugUtility.HandleErrorIfNullGetComponent<RectTransform, CrosshairManager>(m_CrosshairRectTransform, this, CrosshairImage.gameObject);
			}
			else if ((bool)NullCrosshairSprite)
			{
				CrosshairImage.sprite = NullCrosshairSprite;
			}
			else
			{
				CrosshairImage.enabled = false;
			}
			UpdateCrosshairPointingAtEnemy(force: true);
		}
	}
}
