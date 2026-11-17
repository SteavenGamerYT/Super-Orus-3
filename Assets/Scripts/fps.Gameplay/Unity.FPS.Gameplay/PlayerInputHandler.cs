using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
	public class PlayerInputHandler : MonoBehaviour
	{
		[Tooltip("Sensitivity multiplier for moving the camera around")]
		public float LookSensitivity = 1f;

		[Tooltip("Additional sensitivity multiplier for WebGL")]
		public float WebglLookSensitivityMultiplier = 0.25f;

		[Tooltip("Limit to consider an input when using a trigger on a controller")]
		public float TriggerAxisThreshold = 0.4f;

		[Tooltip("Used to flip the vertical input axis")]
		public bool InvertYAxis;

		[Tooltip("Used to flip the horizontal input axis")]
		public bool InvertXAxis;

		private GameFlowManager m_GameFlowManager;

		private PlayerCharacterController m_PlayerCharacterController;

		private bool m_FireInputWasHeld;

		private void Start()
		{
			m_PlayerCharacterController = GetComponent<PlayerCharacterController>();
			DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterController, PlayerInputHandler>(m_PlayerCharacterController, this, gameObject);
			m_GameFlowManager = Object.FindAnyObjectByType<GameFlowManager>();
			DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, PlayerInputHandler>(m_GameFlowManager, this);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		private void LateUpdate()
		{
			m_FireInputWasHeld = GetFireInputHeld();
		}

		public bool CanProcessInput()
		{
			if (Cursor.lockState == CursorLockMode.Locked)
			{
				return !m_GameFlowManager.GameIsEnding;
			}
			return false;
		}

		public Vector3 GetMoveInput()
		{
			if (CanProcessInput())
			{
				return Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")), 1f);
			}
			return Vector3.zero;
		}

		public float GetLookInputsHorizontal()
		{
			return GetMouseOrStickLookAxis("Mouse X", "Look X");
		}

		public float GetLookInputsVertical()
		{
			return GetMouseOrStickLookAxis("Mouse Y", "Look Y");
		}

		public bool GetJumpInputDown()
		{
			if (CanProcessInput())
			{
				return Input.GetButtonDown("Jump");
			}
			return false;
		}

		public bool GetJumpInputHeld()
		{
			if (CanProcessInput())
			{
				return Input.GetButton("Jump");
			}
			return false;
		}

		public bool GetFireInputDown()
		{
			if (GetFireInputHeld())
			{
				return !m_FireInputWasHeld;
			}
			return false;
		}

		public bool GetFireInputReleased()
		{
			if (!GetFireInputHeld())
			{
				return m_FireInputWasHeld;
			}
			return false;
		}

		public bool GetFireInputHeld()
		{
			if (CanProcessInput())
			{
				if (Input.GetAxis("Gamepad Fire") != 0f)
				{
					return Input.GetAxis("Gamepad Fire") >= TriggerAxisThreshold;
				}
				return Input.GetButton("Fire");
			}
			return false;
		}

		public bool GetAimInputHeld()
		{
			if (CanProcessInput())
			{
				if (Input.GetAxis("Gamepad Aim") == 0f)
				{
					return Input.GetButton("Aim");
				}
				return Input.GetAxis("Gamepad Aim") > 0f;
			}
			return false;
		}

		public bool GetSprintInputHeld()
		{
			if (CanProcessInput())
			{
				return Input.GetButton("Sprint");
			}
			return false;
		}

		public bool GetCrouchInputDown()
		{
			if (CanProcessInput())
			{
				return Input.GetButtonDown("Crouch");
			}
			return false;
		}

		public bool GetCrouchInputReleased()
		{
			if (CanProcessInput())
			{
				return Input.GetButtonUp("Crouch");
			}
			return false;
		}

		public bool GetReloadButtonDown()
		{
			if (CanProcessInput())
			{
				return Input.GetButtonDown("Reload");
			}
			return false;
		}

		public int GetSwitchWeaponInput()
		{
			if (CanProcessInput())
			{
				string axisName = ((Input.GetAxis("Gamepad Switch") != 0f) ? "Gamepad Switch" : "Mouse ScrollWheel");
				if (Input.GetAxis(axisName) > 0f)
				{
					return -1;
				}
				if (Input.GetAxis(axisName) < 0f)
				{
					return 1;
				}
				if (Input.GetAxis("NextWeapon") > 0f)
				{
					return -1;
				}
				if (Input.GetAxis("NextWeapon") < 0f)
				{
					return 1;
				}
			}
			return 0;
		}

		public int GetSelectWeaponInput()
		{
			if (CanProcessInput())
			{
				if (Input.GetKeyDown(KeyCode.Alpha1))
				{
					return 1;
				}
				if (Input.GetKeyDown(KeyCode.Alpha2))
				{
					return 2;
				}
				if (Input.GetKeyDown(KeyCode.Alpha3))
				{
					return 3;
				}
				if (Input.GetKeyDown(KeyCode.Alpha4))
				{
					return 4;
				}
				if (Input.GetKeyDown(KeyCode.Alpha5))
				{
					return 5;
				}
				if (Input.GetKeyDown(KeyCode.Alpha6))
				{
					return 6;
				}
				if (Input.GetKeyDown(KeyCode.Alpha7))
				{
					return 7;
				}
				if (Input.GetKeyDown(KeyCode.Alpha8))
				{
					return 8;
				}
				if (Input.GetKeyDown(KeyCode.Alpha9))
				{
					return 9;
				}
				return 0;
			}
			return 0;
		}

		private float GetMouseOrStickLookAxis(string mouseInputName, string stickInputName)
		{
			if (CanProcessInput())
			{
				bool flag = Input.GetAxis(stickInputName) != 0f;
				float num = (flag ? Input.GetAxis(stickInputName) : Input.GetAxisRaw(mouseInputName));
				if (InvertYAxis)
				{
					num *= -1f;
				}
				num *= LookSensitivity;
				if (flag)
				{
					return num * Time.deltaTime;
				}
				return num * 0.01f;
			}
			return 0f;
		}
	}
}
