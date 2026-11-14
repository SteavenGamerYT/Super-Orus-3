using System;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[AddComponentMenu("")]
public class SceneObjectGUIDComponent : MonoBehaviour
{
	[SerializeField]
	private string m_Id;

	[NonSerialized]
	private bool m_Registered;

	public string id => m_Id;

	public void Awake()
	{
		if (!Application.isEditor)
		{
			UnityEngine.Object.Destroy(this);
		}
		if (string.IsNullOrEmpty(m_Id))
		{
			m_Id = Guid.NewGuid().ToString();
		}
		else if (UnityEngine.Object.FindObjectsOfType<SceneObjectGUIDComponent>().Any((SceneObjectGUIDComponent c) => c.m_Id == m_Id && c != this))
		{
			m_Id = Guid.NewGuid().ToString();
		}
		hideFlags |= HideFlags.HideInInspector;
		Register();
	}

	private void Register()
	{
		if (!m_Registered && Application.isEditor)
		{
			SceneObjectGUIDManager.instance.Register(this);
			m_Registered = true;
		}
	}

	private void OnValidate()
	{
		Register();
	}

	private void OnDestroy()
	{
		if (SceneObjectGUIDManager.IsInitiated)
		{
			SceneObjectGUIDManager.instance.Deregister(this);
		}
	}
}
