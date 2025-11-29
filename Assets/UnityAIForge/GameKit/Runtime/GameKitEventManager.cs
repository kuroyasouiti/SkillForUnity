using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityAIForge.GameKit
{
    /// <summary>
    /// GameKit Event Manager: event hub for game-wide events.
    /// Automatically added by GameKitManager when ManagerType.EventHub is selected.
    /// </summary>
    [AddComponentMenu("")]
    public class GameKitEventManager : MonoBehaviour
    {
        [Header("Registered Events")]
        [SerializeField] private List<string> registeredEventNames = new List<string>();
        
        private Dictionary<string, UnityEvent> eventDictionary = new Dictionary<string, UnityEvent>();
        
        [Header("Settings")]
        [Tooltip("Log event triggers for debugging")]
        [SerializeField] private bool logEvents = false;

        private void Awake()
        {
            eventDictionary = new Dictionary<string, UnityEvent>();
        }

        public void RegisterListener(string eventName, UnityAction callback)
        {
            if (!eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] = new UnityEvent();
                if (!registeredEventNames.Contains(eventName))
                {
                    registeredEventNames.Add(eventName);
                }
            }
            eventDictionary[eventName].AddListener(callback);
        }

        public void UnregisterListener(string eventName, UnityAction callback)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName].RemoveListener(callback);
            }
        }

        public void TriggerEvent(string eventName)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                if (logEvents)
                    Debug.Log($"[GameKitEventManager] Triggered event: {eventName}");
                eventDictionary[eventName]?.Invoke();
            }
            else if (logEvents)
            {
                Debug.LogWarning($"[GameKitEventManager] Event '{eventName}' not found");
            }
        }

        public void ClearEvent(string eventName)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName].RemoveAllListeners();
                eventDictionary.Remove(eventName);
                registeredEventNames.Remove(eventName);
            }
        }

        public void ClearAllEvents()
        {
            foreach (var evt in eventDictionary.Values)
            {
                evt.RemoveAllListeners();
            }
            eventDictionary.Clear();
            registeredEventNames.Clear();
        }

        public bool HasEvent(string eventName)
        {
            return eventDictionary.ContainsKey(eventName);
        }

        public List<string> GetAllEventNames()
        {
            return new List<string>(registeredEventNames);
        }
    }
}

