using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
	[RequireComponent(typeof(AudioSource))]
	public class WeaponController : MonoBehaviour
	{
		[Header("Information")]
		[Tooltip("The name that will be displayed in the UI for this weapon")]
		public string WeaponName;

		[Tooltip("The image that will be displayed in the UI for this weapon")]
		public Sprite WeaponIcon;

		[Tooltip("Default data for the crosshair")]
		public CrosshairData CrosshairDataDefault;

		[Tooltip("Data for the crosshair when targeting an enemy")]
		public CrosshairData CrosshairDataTargetInSight;

		[Header("Internal References")]
		[Tooltip("The root object for the weapon, this is what will be deactivated when the weapon isn't active")]
		public GameObject WeaponRoot;

		[Tooltip("Tip of the weapon, where the projectiles are shot")]
		public Transform WeaponMuzzle;

		[Header("Shoot Parameters")]
		[Tooltip("The type of weapon wil affect how it shoots")]
		public WeaponShootType ShootType;

		[Tooltip("The projectile prefab")]
		public ProjectileBase ProjectilePrefab;

		[Tooltip("Minimum duration between two shots")]
		public float DelayBetweenShots = 0.5f;

		[Tooltip("Angle for the cone in which the bullets will be shot randomly (0 means no spread at all)")]
		public float BulletSpreadAngle;

		[Tooltip("Amount of bullets per shot")]
		public int BulletsPerShot = 1;

		[Tooltip("Force that will push back the weapon after each shot")]
		[Range(0f, 2f)]
		public float RecoilForce = 1f;

		[Tooltip("Ratio of the default FOV that this weapon applies while aiming")]
		[Range(0f, 1f)]
		public float AimZoomRatio = 1f;

		[Tooltip("Translation to apply to weapon arm when aiming with this weapon")]
		public Vector3 AimOffset;

		[Header("Ammo Parameters")]
		[Tooltip("Should the player manually reload")]
		public bool AutomaticReload = true;

		[Tooltip("Has physical clip on the weapon and ammo shells are ejected when firing")]
		public bool HasPhysicalBullets;

		[Tooltip("Number of bullets in a clip")]
		public int ClipSize = 30;

		[Tooltip("Bullet Shell Casing")]
		public GameObject ShellCasing;

		[Tooltip("Weapon Ejection Port for physical ammo")]
		public Transform EjectionPort;

		[Tooltip("Force applied on the shell")]
		[Range(0f, 5f)]
		public float ShellCasingEjectionForce = 2f;

		[Tooltip("Maximum number of shell that can be spawned before reuse")]
		[Range(1f, 30f)]
		public int ShellPoolSize = 1;

		[Tooltip("Amount of ammo reloaded per second")]
		public float AmmoReloadRate = 1f;

		[Tooltip("Delay after the last shot before starting to reload")]
		public float AmmoReloadDelay = 2f;

		[Tooltip("Maximum amount of ammo in the gun")]
		public int MaxAmmo = 8;

		[Header("Charging parameters (charging weapons only)")]
		[Tooltip("Trigger a shot when maximum charge is reached")]
		public bool AutomaticReleaseOnCharged;

		[Tooltip("Duration to reach maximum charge")]
		public float MaxChargeDuration = 2f;

		[Tooltip("Initial ammo used when starting to charge")]
		public float AmmoUsedOnStartCharge = 1f;

		[Tooltip("Additional ammo used when charge reaches its maximum")]
		public float AmmoUsageRateWhileCharging = 1f;

		[Header("Audio & Visual")]
		[Tooltip("Optional weapon animator for OnShoot animations")]
		public Animator WeaponAnimator;

		[Tooltip("Prefab of the muzzle flash")]
		public GameObject MuzzleFlashPrefab;

		[Tooltip("Unparent the muzzle flash instance on spawn")]
		public bool UnparentMuzzleFlash;

		[Tooltip("sound played when shooting")]
		public AudioClip ShootSfx;

		[Tooltip("Sound played when changing to this weapon")]
		public AudioClip ChangeWeaponSfx;

		[Tooltip("Continuous Shooting Sound")]
		public bool UseContinuousShootSound;

		public AudioClip ContinuousShootStartSfx;

		public AudioClip ContinuousShootLoopSfx;

		public AudioClip ContinuousShootEndSfx;

		private AudioSource m_ContinuousShootAudioSource;

		private bool m_WantsToShoot;

		public UnityAction OnShoot;

		private int m_CarriedPhysicalBullets;

		private float m_CurrentAmmo;

		private float m_LastTimeShot = float.NegativeInfinity;

		private Vector3 m_LastMuzzlePosition;

		private AudioSource m_ShootAudioSource;

		private const string k_AnimAttackParameter = "Attack";

		private Queue<Rigidbody> m_PhysicalAmmoPool;

		public float LastChargeTriggerTimestamp { get; private set; }

		public GameObject Owner { get; set; }

		public GameObject SourcePrefab { get; set; }

		public bool IsCharging { get; private set; }

		public float CurrentAmmoRatio { get; private set; }

		public bool IsWeaponActive { get; private set; }

		public bool IsCooling { get; private set; }

		public float CurrentCharge { get; private set; }

		public Vector3 MuzzleWorldVelocity { get; private set; }

		public bool IsReloading { get; private set; }

		public event Action OnShootProcessed;

		public float GetAmmoNeededToShoot()
		{
			return ((ShootType != WeaponShootType.Charge) ? 1f : Mathf.Max(1f, AmmoUsedOnStartCharge)) / (float)(MaxAmmo * BulletsPerShot);
		}

		public int GetCarriedPhysicalBullets()
		{
			return m_CarriedPhysicalBullets;
		}

		public int GetCurrentAmmo()
		{
			return Mathf.FloorToInt(m_CurrentAmmo);
		}

		private void Awake()
		{
			m_CurrentAmmo = MaxAmmo;
			m_CarriedPhysicalBullets = (HasPhysicalBullets ? ClipSize : 0);
			m_LastMuzzlePosition = WeaponMuzzle.position;
			m_ShootAudioSource = GetComponent<AudioSource>();
			DebugUtility.HandleErrorIfNullGetComponent<AudioSource, WeaponController>(m_ShootAudioSource, this, base.gameObject);
			if (UseContinuousShootSound)
			{
				m_ContinuousShootAudioSource = base.gameObject.AddComponent<AudioSource>();
				m_ContinuousShootAudioSource.playOnAwake = false;
				m_ContinuousShootAudioSource.clip = ContinuousShootLoopSfx;
				m_ContinuousShootAudioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponShoot);
				m_ContinuousShootAudioSource.loop = true;
			}
			if (HasPhysicalBullets)
			{
				m_PhysicalAmmoPool = new Queue<Rigidbody>(ShellPoolSize);
				for (int i = 0; i < ShellPoolSize; i++)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(ShellCasing, transform);
					gameObject.SetActive(value: false);
					m_PhysicalAmmoPool.Enqueue(gameObject.GetComponent<Rigidbody>());
				}
			}
		}

		public void AddCarriablePhysicalBullets(int count)
		{
			m_CarriedPhysicalBullets = Mathf.Max(m_CarriedPhysicalBullets + count, MaxAmmo);
		}

		private void ShootShell()
		{
			Rigidbody rigidbody = m_PhysicalAmmoPool.Dequeue();
			rigidbody.transform.position = EjectionPort.transform.position;
			rigidbody.transform.rotation = EjectionPort.transform.rotation;
			rigidbody.gameObject.SetActive(value: true);
			rigidbody.transform.SetParent(null);
			rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
			rigidbody.AddForce(rigidbody.transform.up * ShellCasingEjectionForce, ForceMode.Impulse);
			m_PhysicalAmmoPool.Enqueue(rigidbody);
		}

		private void PlaySFX(AudioClip sfx)
		{
			AudioUtility.CreateSFX(sfx, transform.position, AudioUtility.AudioGroups.WeaponShoot, 0f);
		}

		private void Reload()
		{
			if (m_CarriedPhysicalBullets > 0)
			{
				m_CurrentAmmo = Mathf.Min(m_CarriedPhysicalBullets, ClipSize);
			}
			IsReloading = false;
		}

		public void StartReloadAnimation()
		{
			if (m_CurrentAmmo < (float)m_CarriedPhysicalBullets)
			{
				GetComponent<Animator>().SetTrigger("Reload");
				IsReloading = true;
			}
		}

		private void Update()
		{
			UpdateAmmo();
			UpdateCharge();
			UpdateContinuousShootSound();
			if (Time.deltaTime > 0f)
			{
				MuzzleWorldVelocity = (WeaponMuzzle.position - m_LastMuzzlePosition) / Time.deltaTime;
				m_LastMuzzlePosition = WeaponMuzzle.position;
			}
		}

		private void UpdateAmmo()
		{
			if (AutomaticReload && m_LastTimeShot + AmmoReloadDelay < Time.time && m_CurrentAmmo < (float)MaxAmmo && !IsCharging)
			{
				m_CurrentAmmo += AmmoReloadRate * Time.deltaTime;
				m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo, 0f, MaxAmmo);
				IsCooling = true;
			}
			else
			{
				IsCooling = false;
			}
			if ((float)MaxAmmo == float.PositiveInfinity)
			{
				CurrentAmmoRatio = 1f;
			}
			else
			{
				CurrentAmmoRatio = m_CurrentAmmo / (float)MaxAmmo;
			}
		}

		private void UpdateCharge()
		{
			if (IsCharging && CurrentCharge < 1f)
			{
				float num = 1f - CurrentCharge;
				float num2 = 0f;
				num2 = ((!(MaxChargeDuration <= 0f)) ? (1f / MaxChargeDuration * Time.deltaTime) : num);
				num2 = Mathf.Clamp(num2, 0f, num);
				float num3 = num2 * AmmoUsageRateWhileCharging;
				if (num3 <= m_CurrentAmmo)
				{
					UseAmmo(num3);
					CurrentCharge = Mathf.Clamp01(CurrentCharge + num2);
				}
			}
		}

		private void UpdateContinuousShootSound()
		{
			if (!UseContinuousShootSound)
			{
				return;
			}
			if (m_WantsToShoot && m_CurrentAmmo >= 1f)
			{
				if (!m_ContinuousShootAudioSource.isPlaying)
				{
					m_ShootAudioSource.PlayOneShot(ShootSfx);
					m_ShootAudioSource.PlayOneShot(ContinuousShootStartSfx);
					m_ContinuousShootAudioSource.Play();
				}
			}
			else if (m_ContinuousShootAudioSource.isPlaying)
			{
				m_ShootAudioSource.PlayOneShot(ContinuousShootEndSfx);
				m_ContinuousShootAudioSource.Stop();
			}
		}

		public void ShowWeapon(bool show)
		{
			WeaponRoot.SetActive(show);
			if (show && (bool)ChangeWeaponSfx)
			{
				m_ShootAudioSource.PlayOneShot(ChangeWeaponSfx);
			}
			IsWeaponActive = show;
		}

		public void UseAmmo(float amount)
		{
			m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo - amount, 0f, MaxAmmo);
			m_CarriedPhysicalBullets -= Mathf.RoundToInt(amount);
			m_CarriedPhysicalBullets = Mathf.Clamp(m_CarriedPhysicalBullets, 0, MaxAmmo);
			m_LastTimeShot = Time.time;
		}

		public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
		{
			m_WantsToShoot = inputDown | inputHeld;
			switch (ShootType)
			{
			case WeaponShootType.Manual:
				if (inputDown)
				{
					return TryShoot();
				}
				return false;
			case WeaponShootType.Automatic:
				if (inputHeld)
				{
					return TryShoot();
				}
				return false;
			case WeaponShootType.Charge:
				if (inputHeld)
				{
					TryBeginCharge();
				}
				if (inputUp || (AutomaticReleaseOnCharged && CurrentCharge >= 1f))
				{
					return TryReleaseCharge();
				}
				return false;
			default:
				return false;
			}
		}

		private bool TryShoot()
		{
			if (m_CurrentAmmo >= 1f && m_LastTimeShot + DelayBetweenShots < Time.time)
			{
				HandleShoot();
				m_CurrentAmmo--;
				return true;
			}
			return false;
		}

		private bool TryBeginCharge()
		{
			if (!IsCharging && m_CurrentAmmo >= AmmoUsedOnStartCharge && Mathf.FloorToInt((m_CurrentAmmo - AmmoUsedOnStartCharge) * (float)BulletsPerShot) > 0 && m_LastTimeShot + DelayBetweenShots < Time.time)
			{
				UseAmmo(AmmoUsedOnStartCharge);
				LastChargeTriggerTimestamp = Time.time;
				IsCharging = true;
				return true;
			}
			return false;
		}

		private bool TryReleaseCharge()
		{
			if (IsCharging)
			{
				HandleShoot();
				CurrentCharge = 0f;
				IsCharging = false;
				return true;
			}
			return false;
		}

		private void HandleShoot()
		{
			int num = ((ShootType == WeaponShootType.Charge) ? Mathf.CeilToInt(CurrentCharge * (float)BulletsPerShot) : BulletsPerShot);
			for (int i = 0; i < num; i++)
			{
				Vector3 shotDirectionWithinSpread = GetShotDirectionWithinSpread(WeaponMuzzle);
				UnityEngine.Object.Instantiate(ProjectilePrefab, WeaponMuzzle.position, Quaternion.LookRotation(shotDirectionWithinSpread)).Shoot(this);
			}
			if (MuzzleFlashPrefab != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(MuzzleFlashPrefab, WeaponMuzzle.position, WeaponMuzzle.rotation, WeaponMuzzle.transform);
				if (UnparentMuzzleFlash)
				{
					gameObject.transform.SetParent(null);
				}
				else
				{
					gameObject.layer = WeaponMuzzle.gameObject.layer;
				}
				UnityEngine.Object.Destroy(gameObject, 2f);
			}
			if (HasPhysicalBullets)
			{
				ShootShell();
				m_CarriedPhysicalBullets--;
			}
			m_LastTimeShot = Time.time;
			if ((bool)ShootSfx && !UseContinuousShootSound)
			{
				m_ShootAudioSource.PlayOneShot(ShootSfx);
			}
			if ((bool)WeaponAnimator)
			{
				WeaponAnimator.SetTrigger("Attack");
			}
			OnShoot?.Invoke();
			OnShootProcessed?.Invoke();
		}

		public Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
		{
			float t = BulletSpreadAngle / 180f;
			return Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere, t);
		}
	}
}
