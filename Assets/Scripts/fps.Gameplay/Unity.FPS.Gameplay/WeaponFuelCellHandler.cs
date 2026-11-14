using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
	[RequireComponent(typeof(WeaponController))]
	public class WeaponFuelCellHandler : MonoBehaviour
	{
		[Tooltip("Retract All Fuel Cells Simultaneously")]
		public bool SimultaneousFuelCellsUsage;

		[Tooltip("List of GameObjects representing the fuel cells on the weapon")]
		public GameObject[] FuelCells;

		[Tooltip("Cell local position when used")]
		public Vector3 FuelCellUsedPosition;

		[Tooltip("Cell local position before use")]
		public Vector3 FuelCellUnusedPosition = new Vector3(0f, -0.1f, 0f);

		private WeaponController m_Weapon;

		private bool[] m_FuelCellsCooled;

		private void Start()
		{
			m_Weapon = GetComponent<WeaponController>();
			DebugUtility.HandleErrorIfNullGetComponent<WeaponController, WeaponFuelCellHandler>(m_Weapon, this, gameObject);
			m_FuelCellsCooled = new bool[FuelCells.Length];
			for (int i = 0; i < m_FuelCellsCooled.Length; i++)
			{
				m_FuelCellsCooled[i] = true;
			}
		}

		private void Update()
		{
			if (SimultaneousFuelCellsUsage)
			{
				for (int i = 0; i < FuelCells.Length; i++)
				{
					FuelCells[i].transform.localPosition = Vector3.Lerp(FuelCellUsedPosition, FuelCellUnusedPosition, m_Weapon.CurrentAmmoRatio);
				}
				return;
			}
			for (int j = 0; j < FuelCells.Length; j++)
			{
				float num = FuelCells.Length;
				float a = (float)j / num;
				float b = (float)(j + 1) / num;
				float value = Mathf.InverseLerp(a, b, m_Weapon.CurrentAmmoRatio);
				value = Mathf.Clamp01(value);
				FuelCells[j].transform.localPosition = Vector3.Lerp(FuelCellUsedPosition, FuelCellUnusedPosition, value);
			}
		}
	}
}
