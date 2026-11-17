using UnityEngine;
using UnityEngine.Audio;

namespace Unity.FPS.Game
{
	public class AudioUtility
	{
		public enum AudioGroups
		{
			DamageTick,
			Impact,
			EnemyDetection,
			Pickup,
			WeaponShoot,
			WeaponOverheat,
			WeaponChargeBuildup,
			WeaponChargeLoop,
			HUDVictory,
			HUDObjective,
			EnemyAttack
		}

		private static AudioManager s_AudioManager;

		public static void CreateSFX(AudioClip clip, Vector3 position, AudioGroups audioGroup, float spatialBlend, float rolloffDistanceMin = 1f)
		{
			GameObject gameObject = new GameObject();
			gameObject.transform.position = position;
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.clip = clip;
			audioSource.spatialBlend = spatialBlend;
			audioSource.minDistance = rolloffDistanceMin;
			audioSource.Play();
			audioSource.outputAudioMixerGroup = GetAudioGroup(audioGroup);
			gameObject.AddComponent<TimedSelfDestruct>().LifeTime = clip.length;
		}

		public static AudioMixerGroup GetAudioGroup(AudioGroups group)
		{
			if (s_AudioManager == null)
			{
				s_AudioManager = Object.FindAnyObjectByType<AudioManager>();
			}
			AudioMixerGroup[] array = s_AudioManager.FindMatchingGroups(group.ToString());
			if (array.Length != 0)
			{
				return array[0];
			}
			Debug.LogWarning("Didn't find audio group for " + group);
			return null;
		}

		public static void SetMasterVolume(float value)
		{
			if (s_AudioManager == null)
			{
				s_AudioManager = Object.FindAnyObjectByType<AudioManager>();
			}
			if (value <= 0f)
			{
				value = 0.001f;
			}
			float value2 = Mathf.Log10(value) * 20f;
			s_AudioManager.SetFloat("MasterVolume", value2);
		}

		public static float GetMasterVolume()
		{
			if (s_AudioManager == null)
			{
				s_AudioManager = Object.FindAnyObjectByType<AudioManager>();
			}
			s_AudioManager.GetFloat("MasterVolume", out var value);
			return Mathf.Pow(10f, value / 20f);
		}
	}
}
