using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillForUnity.GameKit
{
    /// <summary>
    /// GameKit Manager component: manages game flow, resources, and state.
    /// </summary>
    [AddComponentMenu("SkillForUnity/GameKit/Manager")]
    public class GameKitManager : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string managerId;
        [SerializeField] private ManagerType managerType;
        
        [Header("Turn-Based Settings")]
        [SerializeField] private List<string> turnPhases = new List<string>();
        [SerializeField] private int currentPhaseIndex = 0;
        
        [Header("Resource Pool")]
        [SerializeField] private List<ResourceEntry> resources = new List<ResourceEntry>();
        
        [Header("Persistence")]
        [SerializeField] private bool persistent = false;

        public string ManagerId => managerId;
        public ManagerType Type => managerType;
        public bool IsPersistent => persistent;

        private void Awake()
        {
            if (persistent)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public void Initialize(string id, ManagerType type, bool isPersistent)
        {
            managerId = id;
            managerType = type;
            persistent = isPersistent;
        }

        #region Turn-Based

        public void AddTurnPhase(string phaseName)
        {
            if (!turnPhases.Contains(phaseName))
            {
                turnPhases.Add(phaseName);
            }
        }

        public string GetCurrentPhase()
        {
            if (turnPhases.Count == 0) return null;
            return turnPhases[currentPhaseIndex];
        }

        public void NextPhase()
        {
            if (turnPhases.Count == 0) return;
            currentPhaseIndex = (currentPhaseIndex + 1) % turnPhases.Count;
            Debug.Log($"[GameKitManager] Advanced to phase: {GetCurrentPhase()}");
        }

        #endregion

        #region Resource Pool

        public void SetResource(string resourceName, float amount)
        {
            var resource = resources.Find(r => r.name == resourceName);
            if (resource != null)
            {
                resource.amount = amount;
            }
            else
            {
                resources.Add(new ResourceEntry { name = resourceName, amount = amount });
            }
        }

        public float GetResource(string resourceName, float defaultValue = 0f)
        {
            var resource = resources.Find(r => r.name == resourceName);
            return resource != null ? resource.amount : defaultValue;
        }

        public bool ConsumeResource(string resourceName, float amount)
        {
            var resource = resources.Find(r => r.name == resourceName);
            if (resource != null && resource.amount >= amount)
            {
                resource.amount -= amount;
                return true;
            }
            return false;
        }

        public void AddResource(string resourceName, float amount)
        {
            var resource = resources.Find(r => r.name == resourceName);
            if (resource != null)
            {
                resource.amount += amount;
            }
            else
            {
                resources.Add(new ResourceEntry { name = resourceName, amount = amount });
            }
        }

        public Dictionary<string, float> GetAllResources()
        {
            var result = new Dictionary<string, float>();
            foreach (var resource in resources)
            {
                result[resource.name] = resource.amount;
            }
            return result;
        }

        #endregion

        [Serializable]
        public class ResourceEntry
        {
            public string name;
            public float amount;
        }

        public enum ManagerType
        {
            TurnBased,
            Realtime,
            ResourcePool,
            EventHub,
            StateManager
        }
    }
}

