using System;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
	[RequireComponent(typeof(PlayerInputHandler))]
	public class PlayerWeaponsManager : MonoBehaviour
	{
		public enum WeaponSwitchState
		{
			Up,
			Down,
			PutDownPrevious,
			PutUpNew
		}

		[Tooltip("List of weapon the player will start with")]
		public List<WeaponController> StartingWeapons = new List<WeaponController>();

		[Header("References")]
		[Tooltip("Secondary camera used to avoid seeing weapon go throw geometries")]
		public Camera WeaponCamera;

		[Tooltip("Parent transform where all weapon will be added in the hierarchy")]
		public Transform WeaponParentSocket;

		[Tooltip("Position for weapons when active but not actively aiming")]
		public Transform DefaultWeaponPosition;

		[Tooltip("Position for weapons when aiming")]
		public Transform AimingWeaponPosition;

		[Tooltip("Position for innactive weapons")]
		public Transform DownWeaponPosition;

		[Header("Weapon Bob")]
		[Tooltip("Frequency at which the weapon will move around in the screen when the player is in movement")]
		public float BobFrequency = 10f;

		[Tooltip("How fast the weapon bob is applied, the bigger value the fastest")]
		public float BobSharpness = 10f;

		[Tooltip("Distance the weapon bobs when not aiming")]
		public float DefaultBobAmount = 0.05f;

		[Tooltip("Distance the weapon bobs when aiming")]
		public float AimingBobAmount = 0.02f;

		[Header("Weapon Recoil")]
		[Tooltip("This will affect how fast the recoil moves the weapon, the bigger the value, the fastest")]
		public float RecoilSharpness = 50f;

		[Tooltip("Maximum distance the recoil can affect the weapon")]
		public float MaxRecoilDistance = 0.5f;

		[Tooltip("How fast the weapon goes back to it's original position after the recoil is finished")]
		public float RecoilRestitutionSharpness = 10f;

		[Header("Misc")]
		[Tooltip("Speed at which the aiming animatoin is played")]
		public float AimingAnimationSpeed = 10f;

		[Tooltip("Field of view when not aiming")]
		public float DefaultFov = 60f;

		[Tooltip("Portion of the regular FOV to apply to the weapon camera")]
		public float WeaponFovMultiplier = 1f;

		[Tooltip("Delay before switching weapon a second time, to avoid recieving multiple inputs from mouse wheel")]
		public float WeaponSwitchDelay = 1f;

		[Tooltip("Layer to set FPS weapon gameObjects to")]
		public LayerMask FpsWeaponLayer;

		public UnityAction<WeaponController> OnSwitchedToWeapon;

		public UnityAction<WeaponController, int> OnAddedWeapon;

		public UnityAction<WeaponController, int> OnRemovedWeapon;

		private WeaponController[] m_WeaponSlots = new WeaponController[9];

		private PlayerInputHandler m_InputHandler;

		private PlayerCharacterController m_PlayerCharacterController;

		private float m_WeaponBobFactor;

		private Vector3 m_LastCharacterPosition;

		private Vector3 m_WeaponMainLocalPosition;

		private Vector3 m_WeaponBobLocalPosition;

		private Vector3 m_WeaponRecoilLocalPosition;

		private Vector3 m_AccumulatedRecoil;

		private float m_TimeStartedWeaponSwitch;

		private WeaponSwitchState m_WeaponSwitchState;

		private int m_WeaponSwitchNewWeaponIndex;

		public bool IsAiming { get; private set; }

		public bool IsPointingAtEnemy { get; private set; }

		public int ActiveWeaponIndex { get; private set; }

		private void Start()
		{
			ActiveWeaponIndex = -1;
			m_WeaponSwitchState = WeaponSwitchState.Down;
			m_InputHandler = GetComponent<PlayerInputHandler>();
			DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, PlayerWeaponsManager>(m_InputHandler, this, gameObject);
			m_PlayerCharacterController = GetComponent<PlayerCharacterController>();
			DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterController, PlayerWeaponsManager>(m_PlayerCharacterController, this, gameObject);
			SetFov(DefaultFov);
			OnSwitchedToWeapon = (UnityAction<WeaponController>)Delegate.Combine(OnSwitchedToWeapon, new UnityAction<WeaponController>(OnWeaponSwitched));
			foreach (WeaponController startingWeapon in StartingWeapons)
			{
				AddWeapon(startingWeapon);
			}
			SwitchWeapon(ascendingOrder: true);
		}

		private void Update()
		{
			WeaponController activeWeapon = GetActiveWeapon();
			if (activeWeapon != null && activeWeapon.IsReloading)
			{
				return;
			}
			if (activeWeapon != null && m_WeaponSwitchState == WeaponSwitchState.Up)
			{
				if (!activeWeapon.AutomaticReload && m_InputHandler.GetReloadButtonDown() && activeWeapon.CurrentAmmoRatio < 1f)
				{
					IsAiming = false;
					activeWeapon.StartReloadAnimation();
					return;
				}
				IsAiming = m_InputHandler.GetAimInputHeld();
				if (activeWeapon.HandleShootInputs(m_InputHandler.GetFireInputDown(), m_InputHandler.GetFireInputHeld(), m_InputHandler.GetFireInputReleased()))
				{
					m_AccumulatedRecoil += Vector3.back * activeWeapon.RecoilForce;
					m_AccumulatedRecoil = Vector3.ClampMagnitude(m_AccumulatedRecoil, MaxRecoilDistance);
				}
			}
			if (!IsAiming && (activeWeapon == null || !activeWeapon.IsCharging) && (m_WeaponSwitchState == WeaponSwitchState.Up || m_WeaponSwitchState == WeaponSwitchState.Down))
			{
				int switchWeaponInput = m_InputHandler.GetSwitchWeaponInput();
				if (switchWeaponInput != 0)
				{
					bool ascendingOrder = switchWeaponInput > 0;
					SwitchWeapon(ascendingOrder);
				}
				else
				{
					switchWeaponInput = m_InputHandler.GetSelectWeaponInput();
					if (switchWeaponInput != 0 && GetWeaponAtSlotIndex(switchWeaponInput - 1) != null)
					{
						SwitchToWeaponIndex(switchWeaponInput - 1);
					}
				}
			}
			IsPointingAtEnemy = false;
			if ((bool)activeWeapon && Physics.Raycast(WeaponCamera.transform.position, WeaponCamera.transform.forward, out var hitInfo, 1000f, -1, QueryTriggerInteraction.Ignore) && hitInfo.collider.GetComponentInParent<Health>() != null)
			{
				IsPointingAtEnemy = true;
			}
		}

		private void LateUpdate()
		{
			UpdateWeaponAiming();
			UpdateWeaponBob();
			UpdateWeaponRecoil();
			UpdateWeaponSwitching();
			WeaponParentSocket.localPosition = m_WeaponMainLocalPosition + m_WeaponBobLocalPosition + m_WeaponRecoilLocalPosition;
		}

		public void SetFov(float fov)
		{
			m_PlayerCharacterController.PlayerCamera.fieldOfView = fov;
			WeaponCamera.fieldOfView = fov * WeaponFovMultiplier;
		}

		public void SwitchWeapon(bool ascendingOrder)
		{
			int newWeaponIndex = -1;
			int num = m_WeaponSlots.Length;
			for (int i = 0; i < m_WeaponSlots.Length; i++)
			{
				if (i != ActiveWeaponIndex && GetWeaponAtSlotIndex(i) != null)
				{
					int distanceBetweenWeaponSlots = GetDistanceBetweenWeaponSlots(ActiveWeaponIndex, i, ascendingOrder);
					if (distanceBetweenWeaponSlots < num)
					{
						num = distanceBetweenWeaponSlots;
						newWeaponIndex = i;
					}
				}
			}
			SwitchToWeaponIndex(newWeaponIndex);
		}

		public void SwitchToWeaponIndex(int newWeaponIndex, bool force = false)
		{
			if (!force && (newWeaponIndex == ActiveWeaponIndex || newWeaponIndex < 0))
			{
				return;
			}
			m_WeaponSwitchNewWeaponIndex = newWeaponIndex;
			m_TimeStartedWeaponSwitch = Time.time;
			if (GetActiveWeapon() == null)
			{
				m_WeaponMainLocalPosition = DownWeaponPosition.localPosition;
				m_WeaponSwitchState = WeaponSwitchState.PutUpNew;
				ActiveWeaponIndex = m_WeaponSwitchNewWeaponIndex;
				WeaponController weaponAtSlotIndex = GetWeaponAtSlotIndex(m_WeaponSwitchNewWeaponIndex);
				if (OnSwitchedToWeapon != null)
				{
					OnSwitchedToWeapon(weaponAtSlotIndex);
				}
			}
			else
			{
				m_WeaponSwitchState = WeaponSwitchState.PutDownPrevious;
			}
		}

		public WeaponController HasWeapon(WeaponController weaponPrefab)
		{
			for (int i = 0; i < m_WeaponSlots.Length; i++)
			{
				WeaponController weaponController = m_WeaponSlots[i];
				if (weaponController != null && weaponController.SourcePrefab == weaponPrefab.gameObject)
				{
					return weaponController;
				}
			}
			return null;
		}

		private void UpdateWeaponAiming()
		{
			if (m_WeaponSwitchState == WeaponSwitchState.Up)
			{
				WeaponController activeWeapon = GetActiveWeapon();
				if (IsAiming && (bool)activeWeapon)
				{
					m_WeaponMainLocalPosition = Vector3.Lerp(m_WeaponMainLocalPosition, AimingWeaponPosition.localPosition + activeWeapon.AimOffset, AimingAnimationSpeed * Time.deltaTime);
					SetFov(Mathf.Lerp(m_PlayerCharacterController.PlayerCamera.fieldOfView, activeWeapon.AimZoomRatio * DefaultFov, AimingAnimationSpeed * Time.deltaTime));
				}
				else
				{
					m_WeaponMainLocalPosition = Vector3.Lerp(m_WeaponMainLocalPosition, DefaultWeaponPosition.localPosition, AimingAnimationSpeed * Time.deltaTime);
					SetFov(Mathf.Lerp(m_PlayerCharacterController.PlayerCamera.fieldOfView, DefaultFov, AimingAnimationSpeed * Time.deltaTime));
				}
			}
		}

		private void UpdateWeaponBob()
		{
			if (Time.deltaTime > 0f)
			{
				Vector3 vector = (m_PlayerCharacterController.transform.position - m_LastCharacterPosition) / Time.deltaTime;
				float b = 0f;
				if (m_PlayerCharacterController.IsGrounded)
				{
					b = Mathf.Clamp01(vector.magnitude / (m_PlayerCharacterController.MaxSpeedOnGround * m_PlayerCharacterController.SprintSpeedModifier));
				}
				m_WeaponBobFactor = Mathf.Lerp(m_WeaponBobFactor, b, BobSharpness * Time.deltaTime);
				float num = (IsAiming ? AimingBobAmount : DefaultBobAmount);
				float bobFrequency = BobFrequency;
				float x = Mathf.Sin(Time.time * bobFrequency) * num * m_WeaponBobFactor;
				float f = (Mathf.Sin(Time.time * bobFrequency * 2f) * 0.5f + 0.5f) * num * m_WeaponBobFactor;
				m_WeaponBobLocalPosition.x = x;
				m_WeaponBobLocalPosition.y = Mathf.Abs(f);
				m_LastCharacterPosition = m_PlayerCharacterController.transform.position;
			}
		}

		private void UpdateWeaponRecoil()
		{
			if (m_WeaponRecoilLocalPosition.z >= m_AccumulatedRecoil.z * 0.99f)
			{
				m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, m_AccumulatedRecoil, RecoilSharpness * Time.deltaTime);
				return;
			}
			m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, Vector3.zero, RecoilRestitutionSharpness * Time.deltaTime);
			m_AccumulatedRecoil = m_WeaponRecoilLocalPosition;
		}

		private void UpdateWeaponSwitching()
		{
			float num = 0f;
			num = ((WeaponSwitchDelay != 0f) ? Mathf.Clamp01((Time.time - m_TimeStartedWeaponSwitch) / WeaponSwitchDelay) : 1f);
			if (num >= 1f)
			{
				if (m_WeaponSwitchState == WeaponSwitchState.PutDownPrevious)
				{
					WeaponController weaponAtSlotIndex = GetWeaponAtSlotIndex(ActiveWeaponIndex);
					if (weaponAtSlotIndex != null)
					{
						weaponAtSlotIndex.ShowWeapon(show: false);
					}
					ActiveWeaponIndex = m_WeaponSwitchNewWeaponIndex;
					num = 0f;
					WeaponController weaponAtSlotIndex2 = GetWeaponAtSlotIndex(ActiveWeaponIndex);
					if (OnSwitchedToWeapon != null)
					{
						OnSwitchedToWeapon(weaponAtSlotIndex2);
					}
					if ((bool)weaponAtSlotIndex2)
					{
						m_TimeStartedWeaponSwitch = Time.time;
						m_WeaponSwitchState = WeaponSwitchState.PutUpNew;
					}
					else
					{
						m_WeaponSwitchState = WeaponSwitchState.Down;
					}
				}
				else if (m_WeaponSwitchState == WeaponSwitchState.PutUpNew)
				{
					m_WeaponSwitchState = WeaponSwitchState.Up;
				}
			}
			if (m_WeaponSwitchState == WeaponSwitchState.PutDownPrevious)
			{
				m_WeaponMainLocalPosition = Vector3.Lerp(DefaultWeaponPosition.localPosition, DownWeaponPosition.localPosition, num);
			}
			else if (m_WeaponSwitchState == WeaponSwitchState.PutUpNew)
			{
				m_WeaponMainLocalPosition = Vector3.Lerp(DownWeaponPosition.localPosition, DefaultWeaponPosition.localPosition, num);
			}
		}

		public bool AddWeapon(WeaponController weaponPrefab)
		{
			if (HasWeapon(weaponPrefab) != null)
			{
				return false;
			}
			for (int i = 0; i < m_WeaponSlots.Length; i++)
			{
				if (m_WeaponSlots[i] == null)
				{
					WeaponController weaponController = UnityEngine.Object.Instantiate(weaponPrefab, WeaponParentSocket);
					weaponController.transform.localPosition = Vector3.zero;
					weaponController.transform.localRotation = Quaternion.identity;
					weaponController.Owner = gameObject;
					weaponController.SourcePrefab = weaponPrefab.gameObject;
					weaponController.ShowWeapon(show: false);
					int layer = Mathf.RoundToInt(Mathf.Log(FpsWeaponLayer.value, 2f));
					Transform[] componentsInChildren = weaponController.gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
					for (int j = 0; j < componentsInChildren.Length; j++)
					{
						componentsInChildren[j].gameObject.layer = layer;
					}
					m_WeaponSlots[i] = weaponController;
					if (OnAddedWeapon != null)
					{
						OnAddedWeapon(weaponController, i);
					}
					return true;
				}
			}
			if (GetActiveWeapon() == null)
			{
				SwitchWeapon(ascendingOrder: true);
			}
			return false;
		}

		public bool RemoveWeapon(WeaponController weaponInstance)
		{
			for (int i = 0; i < m_WeaponSlots.Length; i++)
			{
				if (m_WeaponSlots[i] == weaponInstance)
				{
					m_WeaponSlots[i] = null;
					if (OnRemovedWeapon != null)
					{
						OnRemovedWeapon(weaponInstance, i);
					}
					UnityEngine.Object.Destroy(weaponInstance.gameObject);
					if (i == ActiveWeaponIndex)
					{
						SwitchWeapon(ascendingOrder: true);
					}
					return true;
				}
			}
			return false;
		}

		public WeaponController GetActiveWeapon()
		{
			return GetWeaponAtSlotIndex(ActiveWeaponIndex);
		}

		public WeaponController GetWeaponAtSlotIndex(int index)
		{
			if (index >= 0 && index < m_WeaponSlots.Length)
			{
				return m_WeaponSlots[index];
			}
			return null;
		}

		private int GetDistanceBetweenWeaponSlots(int fromSlotIndex, int toSlotIndex, bool ascendingOrder)
		{
			int num = 0;
			num = ((!ascendingOrder) ? (-1 * (toSlotIndex - fromSlotIndex)) : (toSlotIndex - fromSlotIndex));
			if (num < 0)
			{
				num = m_WeaponSlots.Length + num;
			}
			return num;
		}

		private void OnWeaponSwitched(WeaponController newWeapon)
		{
			if (newWeapon != null)
			{
				newWeapon.ShowWeapon(show: true);
			}
		}
	}
}
