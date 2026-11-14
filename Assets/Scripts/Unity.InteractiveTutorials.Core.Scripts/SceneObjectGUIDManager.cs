using System.Collections.Generic;

public class SceneObjectGUIDManager
{
	private static SceneObjectGUIDManager m_Instance;

	private Dictionary<string, SceneObjectGUIDComponent> components = new Dictionary<string, SceneObjectGUIDComponent>();

	public static SceneObjectGUIDManager instance
	{
		get
		{
			if (!IsInitiated)
			{
				m_Instance = new SceneObjectGUIDManager();
			}
			return m_Instance;
		}
		set
		{
			m_Instance = value;
		}
	}

	public static bool IsInitiated => m_Instance != null;

	public void Register(SceneObjectGUIDComponent component)
	{
		components.Add(component.id, component);
	}

	public bool Contains(string id)
	{
		return components.ContainsKey(id);
	}

	public void Deregister(SceneObjectGUIDComponent component)
	{
		components.Remove(component.id);
	}

	public SceneObjectGUIDComponent GetComponent(string id)
	{
		if (components.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}
}
