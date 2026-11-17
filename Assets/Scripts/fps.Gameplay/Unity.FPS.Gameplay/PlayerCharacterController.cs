using System;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
	[RequireComponent(typeof(CharacterController), typeof(PlayerInputHandler), typeof(AudioSource))]
	public class PlayerCharacterController : MonoBehaviour
	{
		[Header("References")]
		[Tooltip("Reference to the main camera used for the player")]
		public Camera PlayerCamera;

		[Tooltip("Audio source for footsteps, jump, etc...")]
		public AudioSource AudioSource;

		[Header("General")]
		[Tooltip("Force applied downward when in the air")]
		public float GravityDownForce = 20f;

		[Tooltip("Physic layers checked to consider the player grounded")]
		public LayerMask GroundCheckLayers = -1;

		[Tooltip("distance from the bottom of the character controller capsule to test for grounded")]
		public float GroundCheckDistance = 0.05f;

		[Header("Movement")]
		[Tooltip("Max movement speed when grounded (when not sprinting)")]
		public float MaxSpeedOnGround = 10f;

		[Tooltip("Sharpness for the movement when grounded, a low value will make the player accelerate and decelerate slowly, a high value will do the opposite")]
		public float MovementSharpnessOnGround = 15f;

		[Tooltip("Max movement speed when crouching")]
		[Range(0f, 1f)]
		public float MaxSpeedCrouchedRatio = 0.5f;

		[Tooltip("Max movement speed when not grounded")]
		public float MaxSpeedInAir = 10f;

		[Tooltip("Acceleration speed when in the air")]
		public float AccelerationSpeedInAir = 25f;

		[Tooltip("Multiplicator for the sprint speed (based on grounded speed)")]
		public float SprintSpeedModifier = 2f;

		[Tooltip("Height at which the player dies instantly when falling off the map")]
		public float KillHeight = -50f;

		[Header("Rotation")]
		[Tooltip("Rotation speed for moving the camera")]
		public float RotationSpeed = 200f;

		[Range(0.1f, 1f)]
		[Tooltip("Rotation speed multiplier when aiming")]
		public float AimingRotationMultiplier = 0.4f;

		[Header("Jump")]
		[Tooltip("Force applied upward when jumping")]
		public float JumpForce = 9f;

		[Header("Stance")]
		[Tooltip("Ratio (0-1) of the character height where the camera will be at")]
		public float CameraHeightRatio = 0.9f;

		[Tooltip("Height of character when standing")]
		public float CapsuleHeightStanding = 1.8f;

		[Tooltip("Height of character when crouching")]
		public float CapsuleHeightCrouching = 0.9f;

		[Tooltip("Speed of crouching transitions")]
		public float CrouchingSharpness = 10f;

		[Header("Audio")]
		[Tooltip("Amount of footstep sounds played when moving one meter")]
		public float FootstepSfxFrequency = 1f;

		[Tooltip("Amount of footstep sounds played when moving one meter while sprinting")]
		public float FootstepSfxFrequencyWhileSprinting = 1f;

		[Tooltip("Sound played for footsteps")]
		public AudioClip FootstepSfx;

		[Tooltip("Sound played when jumping")]
		public AudioClip JumpSfx;

		[Tooltip("Sound played when landing")]
		public AudioClip LandSfx;

		[Tooltip("Sound played when taking damage froma fall")]
		public AudioClip FallDamageSfx;

		[Header("Fall Damage")]
		[Tooltip("Whether the player will recieve damage when hitting the ground at high speed")]
		public bool RecievesFallDamage;

		[Tooltip("Minimun fall speed for recieving fall damage")]
		public float MinSpeedForFallDamage = 10f;

		[Tooltip("Fall speed for recieving th emaximum amount of fall damage")]
		public float MaxSpeedForFallDamage = 30f;

		[Tooltip("Damage recieved when falling at the mimimum speed")]
		public float FallDamageAtMinSpeed = 10f;

		[Tooltip("Damage recieved when falling at the maximum speed")]
		public float FallDamageAtMaxSpeed = 50f;

		public UnityAction<bool> OnStanceChanged;

		private Health m_Health;

		private PlayerInputHandler m_InputHandler;

		private CharacterController m_Controller;

		private PlayerWeaponsManager m_WeaponsManager;

		private Actor m_Actor;

		private Vector3 m_GroundNormal;

		private Vector3 m_LatestImpactSpeed;

		private float m_LastTimeJumped;

		private float m_CameraVerticalAngle;

		private float m_FootstepDistanceCounter;

		private float m_TargetCharacterHeight;

		private const float k_JumpGroundingPreventionTime = 0.2f;

		private const float k_GroundCheckDistanceInAir = 0.07f;

		public Vector3 CharacterVelocity { get; set; }

		public bool IsGrounded { get; private set; }

		public bool HasJumpedThisFrame { get; private set; }

		public bool IsDead { get; private set; }

		public bool IsCrouching { get; private set; }

		public float RotationMultiplier
		{
			get
			{
				if (m_WeaponsManager.IsAiming)
				{
					return AimingRotationMultiplier;
				}
				return 1f;
			}
		}

		private void Awake()
		{
			ActorsManager actorsManager = UnityEngine.Object.FindAnyObjectByType<ActorsManager>();
			if (actorsManager != null)
			{
				actorsManager.SetPlayer(gameObject);
			}
		}

		private void Start()
		{
			m_Controller = GetComponent<CharacterController>();
			DebugUtility.HandleErrorIfNullGetComponent<CharacterController, PlayerCharacterController>(m_Controller, this, gameObject);
			m_InputHandler = GetComponent<PlayerInputHandler>();
			DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, PlayerCharacterController>(m_InputHandler, this, gameObject);
			m_WeaponsManager = GetComponent<PlayerWeaponsManager>();
			DebugUtility.HandleErrorIfNullGetComponent<PlayerWeaponsManager, PlayerCharacterController>(m_WeaponsManager, this, gameObject);
			m_Health = GetComponent<Health>();
			DebugUtility.HandleErrorIfNullGetComponent<Health, PlayerCharacterController>(m_Health, this, gameObject);
			m_Actor = GetComponent<Actor>();
			DebugUtility.HandleErrorIfNullGetComponent<Actor, PlayerCharacterController>(m_Actor, this, gameObject);
			m_Controller.enableOverlapRecovery = true;
			Health health = m_Health;
			health.OnDie = (UnityAction)Delegate.Combine(health.OnDie, new UnityAction(OnDie));
			SetCrouchingState(crouched: false, ignoreObstructions: true);
			UpdateCharacterHeight(force: true);
		}

		private void Update()
		{
			if (!IsDead && transform.position.y < KillHeight)
			{
				m_Health.Kill();
			}
			HasJumpedThisFrame = false;
			bool isGrounded = IsGrounded;
			GroundCheck();
			if (IsGrounded && !isGrounded)
			{
				float num = (0f - Mathf.Min(CharacterVelocity.y, m_LatestImpactSpeed.y) - MinSpeedForFallDamage) / (MaxSpeedForFallDamage - MinSpeedForFallDamage);
				if (RecievesFallDamage && num > 0f)
				{
					float damage = Mathf.Lerp(FallDamageAtMinSpeed, FallDamageAtMaxSpeed, num);
					m_Health.TakeDamage(damage, null);
					AudioSource.PlayOneShot(FallDamageSfx);
				}
				else
				{
					AudioSource.PlayOneShot(LandSfx);
				}
			}
			if (m_InputHandler.GetCrouchInputDown())
			{
				SetCrouchingState(!IsCrouching, ignoreObstructions: false);
			}
			UpdateCharacterHeight(force: false);
			HandleCharacterMovement();
		}

		private void OnDie()
		{
			IsDead = true;
			m_WeaponsManager.SwitchToWeaponIndex(-1, force: true);
			EventManager.Broadcast(Events.PlayerDeathEvent);
		}

		private void GroundCheck()
		{
			float maxDistance = (IsGrounded ? (m_Controller.skinWidth + GroundCheckDistance) : 0.07f);
			IsGrounded = false;
			m_GroundNormal = Vector3.up;
			if (!(Time.time >= m_LastTimeJumped + 0.2f) || !Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(m_Controller.height), m_Controller.radius, Vector3.down, out var hitInfo, maxDistance, GroundCheckLayers, QueryTriggerInteraction.Ignore))
			{
				return;
			}
			m_GroundNormal = hitInfo.normal;
			if (Vector3.Dot(hitInfo.normal, transform.up) > 0f && IsNormalUnderSlopeLimit(m_GroundNormal))
			{
				IsGrounded = true;
				if (hitInfo.distance > m_Controller.skinWidth)
				{
					m_Controller.Move(Vector3.down * hitInfo.distance);
				}
			}
		}

		private void HandleCharacterMovement()
		{
			transform.Rotate(new Vector3(0f, m_InputHandler.GetLookInputsHorizontal() * RotationSpeed * RotationMultiplier, 0f), Space.Self);
			m_CameraVerticalAngle += m_InputHandler.GetLookInputsVertical() * RotationSpeed * RotationMultiplier;
			m_CameraVerticalAngle = Mathf.Clamp(m_CameraVerticalAngle, -89f, 89f);
			PlayerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0f, 0f);
			bool flag = m_InputHandler.GetSprintInputHeld();
			if (flag)
			{
				flag = SetCrouchingState(crouched: false, ignoreObstructions: false);
			}
			float num = (flag ? SprintSpeedModifier : 1f);
			Vector3 vector = transform.TransformVector(m_InputHandler.GetMoveInput());
			if (IsGrounded)
			{
				Vector3 vector2 = vector * MaxSpeedOnGround * num;
				if (IsCrouching)
				{
					vector2 *= MaxSpeedCrouchedRatio;
				}
				vector2 = GetDirectionReorientedOnSlope(vector2.normalized, m_GroundNormal) * vector2.magnitude;
				CharacterVelocity = Vector3.Lerp(CharacterVelocity, vector2, MovementSharpnessOnGround * Time.deltaTime);
				if (IsGrounded && m_InputHandler.GetJumpInputDown() && SetCrouchingState(crouched: false, ignoreObstructions: false))
				{
					CharacterVelocity = new Vector3(CharacterVelocity.x, 0f, CharacterVelocity.z);
					CharacterVelocity += Vector3.up * JumpForce;
					AudioSource.PlayOneShot(JumpSfx);
					m_LastTimeJumped = Time.time;
					HasJumpedThisFrame = true;
					IsGrounded = false;
					m_GroundNormal = Vector3.up;
				}
				float num2 = (flag ? FootstepSfxFrequencyWhileSprinting : FootstepSfxFrequency);
				if (m_FootstepDistanceCounter >= 1f / num2)
				{
					m_FootstepDistanceCounter = 0f;
					AudioSource.PlayOneShot(FootstepSfx);
				}
				m_FootstepDistanceCounter += CharacterVelocity.magnitude * Time.deltaTime;
			}
			else
			{
				CharacterVelocity += vector * AccelerationSpeedInAir * Time.deltaTime;
				float y = CharacterVelocity.y;
				Vector3 vector3 = Vector3.ProjectOnPlane(CharacterVelocity, Vector3.up);
				vector3 = Vector3.ClampMagnitude(vector3, MaxSpeedInAir * num);
				CharacterVelocity = vector3 + Vector3.up * y;
				CharacterVelocity += Vector3.down * GravityDownForce * Time.deltaTime;
			}
			Vector3 capsuleBottomHemisphere = GetCapsuleBottomHemisphere();
			Vector3 capsuleTopHemisphere = GetCapsuleTopHemisphere(m_Controller.height);
			m_Controller.Move(CharacterVelocity * Time.deltaTime);
			m_LatestImpactSpeed = Vector3.zero;
			if (Physics.CapsuleCast(capsuleBottomHemisphere, capsuleTopHemisphere, m_Controller.radius, CharacterVelocity.normalized, out var hitInfo, CharacterVelocity.magnitude * Time.deltaTime, -1, QueryTriggerInteraction.Ignore))
			{
				m_LatestImpactSpeed = CharacterVelocity;
				CharacterVelocity = Vector3.ProjectOnPlane(CharacterVelocity, hitInfo.normal);
			}
		}

		private bool IsNormalUnderSlopeLimit(Vector3 normal)
		{
			return Vector3.Angle(transform.up, normal) <= m_Controller.slopeLimit;
		}

		private Vector3 GetCapsuleBottomHemisphere()
		{
			return transform.position + transform.up * m_Controller.radius;
		}

		private Vector3 GetCapsuleTopHemisphere(float atHeight)
		{
			return transform.position + transform.up * (atHeight - m_Controller.radius);
		}

		public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
		{
			Vector3 rhs = Vector3.Cross(direction, transform.up);
			return Vector3.Cross(slopeNormal, rhs).normalized;
		}

		private void UpdateCharacterHeight(bool force)
		{
			if (force)
			{
				m_Controller.height = m_TargetCharacterHeight;
				m_Controller.center = Vector3.up * m_Controller.height * 0.5f;
				PlayerCamera.transform.localPosition = Vector3.up * m_TargetCharacterHeight * CameraHeightRatio;
				m_Actor.AimPoint.transform.localPosition = m_Controller.center;
			}
			else if (m_Controller.height != m_TargetCharacterHeight)
			{
				m_Controller.height = Mathf.Lerp(m_Controller.height, m_TargetCharacterHeight, CrouchingSharpness * Time.deltaTime);
				m_Controller.center = Vector3.up * m_Controller.height * 0.5f;
				PlayerCamera.transform.localPosition = Vector3.Lerp(PlayerCamera.transform.localPosition, Vector3.up * m_TargetCharacterHeight * CameraHeightRatio, CrouchingSharpness * Time.deltaTime);
				m_Actor.AimPoint.transform.localPosition = m_Controller.center;
			}
		}

		private bool SetCrouchingState(bool crouched, bool ignoreObstructions)
		{
			if (crouched)
			{
				m_TargetCharacterHeight = CapsuleHeightCrouching;
			}
			else
			{
				if (!ignoreObstructions)
				{
					Collider[] array = Physics.OverlapCapsule(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(CapsuleHeightStanding), m_Controller.radius, -1, QueryTriggerInteraction.Ignore);
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i] != m_Controller)
						{
							return false;
						}
					}
				}
				m_TargetCharacterHeight = CapsuleHeightStanding;
			}
			if (OnStanceChanged != null)
			{
				OnStanceChanged(crouched);
			}
			IsCrouching = crouched;
			return true;
		}
	}
}
