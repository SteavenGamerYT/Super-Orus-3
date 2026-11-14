using System;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
	public class ProjectileChargeParameters : MonoBehaviour
	{
		public MinMaxFloat Damage;

		public MinMaxFloat Radius;

		public MinMaxFloat Speed;

		public MinMaxFloat GravityDownAcceleration;

		public MinMaxFloat AreaOfEffectDistance;

		private ProjectileBase m_ProjectileBase;

		private void OnEnable()
		{
			m_ProjectileBase = GetComponent<ProjectileBase>();
			DebugUtility.HandleErrorIfNullGetComponent<ProjectileBase, ProjectileChargeParameters>(m_ProjectileBase, this, gameObject);
			ProjectileBase projectileBase = m_ProjectileBase;
			projectileBase.OnShoot = (UnityAction)Delegate.Combine(projectileBase.OnShoot, new UnityAction(OnShoot));
		}

		private void OnShoot()
		{
			ProjectileStandard component = GetComponent<ProjectileStandard>();
			if ((bool)component)
			{
				component.Damage = Damage.GetValueFromRatio(m_ProjectileBase.InitialCharge);
				component.Radius = Radius.GetValueFromRatio(m_ProjectileBase.InitialCharge);
				component.Speed = Speed.GetValueFromRatio(m_ProjectileBase.InitialCharge);
				component.GravityDownAcceleration = GravityDownAcceleration.GetValueFromRatio(m_ProjectileBase.InitialCharge);
			}
		}
	}
}
