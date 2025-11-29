using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkillForUnity.GameKit
{
    /// <summary>
    /// GameKit Interaction component: handles triggers and executes declarative actions.
    /// </summary>
    [AddComponentMenu("SkillForUnity/GameKit/Interaction")]
    public class GameKitInteraction : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string interactionId;
        
        [Header("Trigger Settings")]
        [SerializeField] private TriggerType triggerType;
        
        [Header("Actions")]
        [SerializeField] private List<InteractionAction> actions = new List<InteractionAction>();
        
        [Header("Conditions")]
        [SerializeField] private List<InteractionCondition> conditions = new List<InteractionCondition>();

        public string InteractionId => interactionId;
        public TriggerType Trigger => triggerType;

        public void Initialize(string id, TriggerType trigger)
        {
            interactionId = id;
            triggerType = trigger;
        }

        public void AddAction(ActionType type, string target, string parameter)
        {
            actions.Add(new InteractionAction
            {
                type = type,
                target = target,
                parameter = parameter
            });
        }

        public void AddCondition(ConditionType type, string value)
        {
            conditions.Add(new InteractionCondition
            {
                type = type,
                value = value
            });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggerType == TriggerType.Trigger && EvaluateConditions(other.gameObject))
            {
                ExecuteActions(other.gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggerType == TriggerType.Trigger && EvaluateConditions(other.gameObject))
            {
                ExecuteActions(other.gameObject);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (triggerType == TriggerType.Collision && EvaluateConditions(collision.gameObject))
            {
                ExecuteActions(collision.gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (triggerType == TriggerType.Collision && EvaluateConditions(collision.gameObject))
            {
                ExecuteActions(collision.gameObject);
            }
        }

        private void Update()
        {
            if (triggerType == TriggerType.Input && Input.GetKeyDown(KeyCode.E))
            {
                if (EvaluateConditions(gameObject))
                {
                    ExecuteActions(gameObject);
                }
            }
        }

        private bool EvaluateConditions(GameObject other)
        {
            if (conditions.Count == 0) return true;

            foreach (var condition in conditions)
            {
                switch (condition.type)
                {
                    case ConditionType.Tag:
                        if (!other.CompareTag(condition.value))
                            return false;
                        break;
                    
                    case ConditionType.Layer:
                        if (other.layer != LayerMask.NameToLayer(condition.value))
                            return false;
                        break;
                    
                    case ConditionType.Distance:
                        if (float.TryParse(condition.value, out float maxDistance))
                        {
                            if (Vector3.Distance(transform.position, other.transform.position) > maxDistance)
                                return false;
                        }
                        break;
                }
            }

            return true;
        }

        private void ExecuteActions(GameObject other)
        {
            foreach (var action in actions)
            {
                switch (action.type)
                {
                    case ActionType.SpawnPrefab:
                        SpawnPrefab(action.target, action.parameter);
                        break;
                    
                    case ActionType.DestroyObject:
                        DestroyObject(action.target);
                        break;
                    
                    case ActionType.PlaySound:
                        PlaySound(action.target);
                        break;
                    
                    case ActionType.SendMessage:
                        SendMessageToTarget(action.target, action.parameter);
                        break;
                    
                    case ActionType.ChangeScene:
                        ChangeScene(action.target);
                        break;
                }
            }
        }

        private void SpawnPrefab(string prefabPath, string positionStr)
        {
            var prefab = UnityEngine.Resources.Load<GameObject>(prefabPath);
            if (prefab != null)
            {
                Vector3 position = transform.position;
                if (!string.IsNullOrEmpty(positionStr))
                {
                    var parts = positionStr.Split(',');
                    if (parts.Length == 3 &&
                        float.TryParse(parts[0], out float x) &&
                        float.TryParse(parts[1], out float y) &&
                        float.TryParse(parts[2], out float z))
                    {
                        position = new Vector3(x, y, z);
                    }
                }
                Instantiate(prefab, position, Quaternion.identity);
                Debug.Log($"[GameKitInteraction] Spawned prefab: {prefabPath}");
            }
        }

        private void DestroyObject(string targetPath)
        {
            if (targetPath == "self")
            {
                Destroy(gameObject);
            }
            else
            {
                var target = GameObject.Find(targetPath);
                if (target != null)
                {
                    Destroy(target);
                    Debug.Log($"[GameKitInteraction] Destroyed object: {targetPath}");
                }
            }
        }

        private void PlaySound(string audioClipPath)
        {
            var clip = UnityEngine.Resources.Load<AudioClip>(audioClipPath);
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
                Debug.Log($"[GameKitInteraction] Played sound: {audioClipPath}");
            }
        }

        private void SendMessageToTarget(string targetPath, string message)
        {
            var target = GameObject.Find(targetPath);
            if (target != null)
            {
                target.SendMessage(message, SendMessageOptions.DontRequireReceiver);
                Debug.Log($"[GameKitInteraction] Sent message '{message}' to {targetPath}");
            }
        }

        private void ChangeScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
            Debug.Log($"[GameKitInteraction] Loading scene: {sceneName}");
        }

        [Serializable]
        public class InteractionAction
        {
            public ActionType type;
            public string target;
            public string parameter;
        }

        [Serializable]
        public class InteractionCondition
        {
            public ConditionType type;
            public string value;
        }

        public enum TriggerType
        {
            Collision,
            Trigger,
            Raycast,
            Proximity,
            Input
        }

        public enum ActionType
        {
            SpawnPrefab,
            DestroyObject,
            PlaySound,
            SendMessage,
            ChangeScene
        }

        public enum ConditionType
        {
            Tag,
            Layer,
            Distance,
            Custom
        }
    }
}

