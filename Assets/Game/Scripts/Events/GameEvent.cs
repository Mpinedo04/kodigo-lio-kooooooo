using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // ScriptableObject event channel: systems raise it without knowing who listens.
    [CreateAssetMenu(menuName = "Code Lyoko/Game Event", fileName = "GameEvent")]
    public sealed class GameEvent : ScriptableObject
    {
        private readonly List<GameEventListener> listeners = new List<GameEventListener>();

        public event Action Raised;

        public void Raise()
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised();
            }

            Raised?.Invoke();
        }

        public void Register(GameEventListener listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        public void Unregister(GameEventListener listener)
        {
            listeners.Remove(listener);
        }
    }
}
