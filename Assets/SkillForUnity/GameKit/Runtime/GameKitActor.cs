using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillForUnity.GameKit
{
    /// <summary>
    /// GameKit Actor component: stores metadata and provides runtime behavior for game actors.
    /// </summary>
    [AddComponentMenu("SkillForUnity/GameKit/Actor")]
    public class GameKitActor : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string actorId;
        
        [Header("Behavior")]
        [SerializeField] private BehaviorProfile behaviorProfile;
        [SerializeField] private ControlMode controlMode;
        
        [Header("Stats")]
        [SerializeField] private List<StatEntry> stats = new List<StatEntry>();
        
        [Header("Abilities & Equipment")]
        [SerializeField] private List<string> abilities = new List<string>();
        [SerializeField] private List<string> weaponLoadout = new List<string>();

        public string ActorId => actorId;
        public BehaviorProfile Behavior => behaviorProfile;
        public ControlMode Control => controlMode;

        public void Initialize(string id, BehaviorProfile behavior, ControlMode control)
        {
            actorId = id;
            behaviorProfile = behavior;
            controlMode = control;
        }

        public void SetStat(string statName, float value)
        {
            var stat = stats.Find(s => s.name == statName);
            if (stat != null)
            {
                stat.value = value;
            }
            else
            {
                stats.Add(new StatEntry { name = statName, value = value });
            }
        }

        public float GetStat(string statName, float defaultValue = 0f)
        {
            var stat = stats.Find(s => s.name == statName);
            return stat != null ? stat.value : defaultValue;
        }

        public void AddAbility(string abilityName)
        {
            if (!abilities.Contains(abilityName))
            {
                abilities.Add(abilityName);
            }
        }

        public void AddWeapon(string weaponName)
        {
            if (!weaponLoadout.Contains(weaponName))
            {
                weaponLoadout.Add(weaponName);
            }
        }

        public bool HasAbility(string abilityName)
        {
            return abilities.Contains(abilityName);
        }

        public Dictionary<string, float> GetAllStats()
        {
            var result = new Dictionary<string, float>();
            foreach (var stat in stats)
            {
                result[stat.name] = stat.value;
            }
            return result;
        }

        [Serializable]
        public class StatEntry
        {
            public string name;
            public float value;
        }

        public enum BehaviorProfile
        {
            TwoDLinear,
            TwoDPhysics,
            TwoDTileGrid,
            ThreeDCharacterController,
            ThreeDPhysics,
            ThreeDNavMesh
        }

        public enum ControlMode
        {
            DirectController,
            AIAutonomous,
            UICommand,
            ScriptTriggerOnly
        }
    }
}

