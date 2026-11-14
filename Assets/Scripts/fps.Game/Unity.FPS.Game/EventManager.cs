using System;
using System.Collections.Generic;

namespace Unity.FPS.Game
{
	public static class EventManager
	{
		private static readonly Dictionary<Type, Action<GameEvent>> s_Events = new Dictionary<Type, Action<GameEvent>>();

		private static readonly Dictionary<Delegate, Action<GameEvent>> s_EventLookups = new Dictionary<Delegate, Action<GameEvent>>();

		public static void AddListener<T>(Action<T> evt) where T : GameEvent
		{
			if (!s_EventLookups.ContainsKey(evt))
			{
				Action<GameEvent> action = (GameEvent e) =>
				{
					evt((T)e);
				};
				s_EventLookups[evt] = action;
				if (s_Events.TryGetValue(typeof(T), out var value))
				{
					value = (s_Events[typeof(T)] = (Action<GameEvent>)Delegate.Combine(value, action));
				}
				else
				{
					s_Events[typeof(T)] = action;
				}
			}
		}

		public static void RemoveListener<T>(Action<T> evt) where T : GameEvent
		{
			if (!s_EventLookups.TryGetValue(evt, out var value))
			{
				return;
			}
			if (s_Events.TryGetValue(typeof(T), out var value2))
			{
				value2 = (Action<GameEvent>)Delegate.Remove(value2, value);
				if (value2 == null)
				{
					s_Events.Remove(typeof(T));
				}
				else
				{
					s_Events[typeof(T)] = value2;
				}
			}
			s_EventLookups.Remove(evt);
		}

		public static void Broadcast(GameEvent evt)
		{
			if (s_Events.TryGetValue(evt.GetType(), out var value))
			{
				value(evt);
			}
		}

		public static void Clear()
		{
			s_Events.Clear();
			s_EventLookups.Clear();
		}
	}
}
