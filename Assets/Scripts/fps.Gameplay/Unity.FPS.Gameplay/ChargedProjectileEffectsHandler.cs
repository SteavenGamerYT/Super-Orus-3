using System;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
	public class ChargedProjectileEffectsHandler : MonoBehaviour
	{
		[Tooltip("Object that will be affected by charging scale & color changes")]
		public GameObject ChargingObject;

		[Tooltip("Scale of the charged object based on charge")]
		public MinMaxVector3 Scale;

		[Tooltip("Color of the charged object based on charge")]
		public MinMaxColor Color;

		private MeshRenderer[] m_AffectedRenderers;

		private ProjectileBase m_ProjectileBase;

		private void OnEnable()
		{
			m_ProjectileBase = GetComponent<ProjectileBase>();
			DebugUtility.HandleErrorIfNullGetComponent<ProjectileBase, ChargedProjectileEffectsHandler>(m_ProjectileBase, this, gameObject);
			ProjectileBase projectileBase = m_ProjectileBase;
			projectileBase.OnShoot = (UnityAction)Delegate.Combine(projectileBase.OnShoot, new UnityAction(OnShoot));
			m_AffectedRenderers = ChargingObject.GetComponentsInChildren<MeshRenderer>();
			MeshRenderer[] affectedRenderers = m_AffectedRenderers;
			foreach (MeshRenderer obj in affectedRenderers)
			{
				obj.sharedMaterial = UnityEngine.Object.Instantiate(obj.sharedMaterial);
			}
		}

		private void OnShoot()
		{
			ChargingObject.transform.localScale = Scale.GetValueFromRatio(m_ProjectileBase.InitialCharge);
			MeshRenderer[] affectedRenderers = m_AffectedRenderers;
			for (int i = 0; i < affectedRenderers.Length; i++)
			{
				affectedRenderers[i].sharedMaterial.SetColor("_Color", Color.GetValueFromRatio(m_ProjectileBase.InitialCharge));
			}
		}
	}
}
