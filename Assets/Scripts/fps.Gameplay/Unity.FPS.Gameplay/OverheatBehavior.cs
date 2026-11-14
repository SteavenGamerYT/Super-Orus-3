using System;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
	public class OverheatBehavior : MonoBehaviour
	{
		[Serializable]
		public struct RendererIndexData
		{
			public Renderer Renderer;

			public int MaterialIndex;

			public RendererIndexData(Renderer renderer, int index)
			{
				Renderer = renderer;
				MaterialIndex = index;
			}
		}

		[Header("Visual")]
		[Tooltip("The VFX to scale the spawn rate based on the ammo ratio")]
		public ParticleSystem SteamVfx;

		[Tooltip("The emission rate for the effect when fully overheated")]
		public float SteamVfxEmissionRateMax = 8f;

		[GradientUsage(true)]
		[Tooltip("Overheat color based on ammo ratio")]
		public Gradient OverheatGradient;

		[Tooltip("The material for overheating color animation")]
		public Material OverheatingMaterial;

		[Header("Sound")]
		[Tooltip("Sound played when a cell are cooling")]
		public AudioClip CoolingCellsSound;

		[Tooltip("Curve for ammo to volume ratio")]
		public AnimationCurve AmmoToVolumeRatioCurve;

		private WeaponController m_Weapon;

		private AudioSource m_AudioSource;

		private List<RendererIndexData> m_OverheatingRenderersData;

		private MaterialPropertyBlock m_OverheatMaterialPropertyBlock;

		private float m_LastAmmoRatio;

		private ParticleSystem.EmissionModule m_SteamVfxEmissionModule;

		private void Awake()
		{
			ParticleSystem.EmissionModule emission = SteamVfx.emission;
			emission.rateOverTimeMultiplier = 0f;
			m_OverheatingRenderersData = new List<RendererIndexData>();
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(includeInactive: true);
			foreach (Renderer renderer in componentsInChildren)
			{
				for (int j = 0; j < renderer.sharedMaterials.Length; j++)
				{
					if (renderer.sharedMaterials[j] == OverheatingMaterial)
					{
						m_OverheatingRenderersData.Add(new RendererIndexData(renderer, j));
					}
				}
			}
			m_OverheatMaterialPropertyBlock = new MaterialPropertyBlock();
			m_SteamVfxEmissionModule = SteamVfx.emission;
			m_Weapon = GetComponent<WeaponController>();
			DebugUtility.HandleErrorIfNullGetComponent<WeaponController, OverheatBehavior>(m_Weapon, this, gameObject);
			m_AudioSource = gameObject.AddComponent<AudioSource>();
			m_AudioSource.clip = CoolingCellsSound;
			m_AudioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponOverheat);
		}

		private void Update()
		{
			float currentAmmoRatio = m_Weapon.CurrentAmmoRatio;
			if (currentAmmoRatio != m_LastAmmoRatio)
			{
				m_OverheatMaterialPropertyBlock.SetColor("_EmissionColor", OverheatGradient.Evaluate(1f - currentAmmoRatio));
				foreach (RendererIndexData overheatingRenderersDatum in m_OverheatingRenderersData)
				{
					overheatingRenderersDatum.Renderer.SetPropertyBlock(m_OverheatMaterialPropertyBlock, overheatingRenderersDatum.MaterialIndex);
				}
				m_SteamVfxEmissionModule.rateOverTimeMultiplier = SteamVfxEmissionRateMax * (1f - currentAmmoRatio);
			}
			if ((bool)CoolingCellsSound)
			{
				if (!m_AudioSource.isPlaying && currentAmmoRatio != 1f && m_Weapon.IsWeaponActive && m_Weapon.IsCooling)
				{
					m_AudioSource.Play();
				}
				else if (m_AudioSource.isPlaying && (currentAmmoRatio == 1f || !m_Weapon.IsWeaponActive || !m_Weapon.IsCooling))
				{
					m_AudioSource.Stop();
					return;
				}
				m_AudioSource.volume = AmmoToVolumeRatioCurve.Evaluate(1f - currentAmmoRatio);
			}
			m_LastAmmoRatio = currentAmmoRatio;
		}
	}
}
