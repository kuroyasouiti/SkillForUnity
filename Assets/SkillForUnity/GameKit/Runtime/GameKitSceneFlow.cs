using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkillForUnity.GameKit
{
    /// <summary>
    /// GameKit SceneFlow component: manages scene transitions with state machine and additive loading.
    /// </summary>
    [AddComponentMenu("SkillForUnity/GameKit/SceneFlow")]
    public class GameKitSceneFlow : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string flowId;
        
        [Header("Scenes")]
        [SerializeField] private List<SceneDefinition> scenes = new List<SceneDefinition>();
        
        [Header("Transitions")]
        [SerializeField] private List<SceneTransition> transitions = new List<SceneTransition>();
        
        [Header("Shared Scene Groups")]
        [SerializeField] private List<SharedSceneGroup> sharedGroups = new List<SharedSceneGroup>();
        
        [Header("Current State")]
        [SerializeField] private string currentSceneName;
        [SerializeField] private List<string> loadedSharedScenes = new List<string>();

        private static GameKitSceneFlow instance;

        public string FlowId => flowId;
        public string CurrentScene => currentSceneName;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Initialize(string id)
        {
            flowId = id;
        }

        public void AddScene(string name, string scenePath, SceneLoadMode loadMode, string[] sharedGroupNames)
        {
            scenes.Add(new SceneDefinition
            {
                name = name,
                scenePath = scenePath,
                loadMode = loadMode,
                sharedGroupNames = new List<string>(sharedGroupNames)
            });
        }

        public void AddTransition(string trigger, string fromScene, string toScene)
        {
            transitions.Add(new SceneTransition
            {
                trigger = trigger,
                fromScene = fromScene,
                toScene = toScene
            });
        }

        public void AddSharedGroup(string groupName, string[] scenePaths)
        {
            sharedGroups.Add(new SharedSceneGroup
            {
                groupName = groupName,
                scenePaths = new List<string>(scenePaths)
            });
        }

        public void TriggerTransition(string triggerName)
        {
            foreach (var transition in transitions)
            {
                if (transition.trigger == triggerName)
                {
                    // Check if transition is valid from current scene
                    if (string.IsNullOrEmpty(transition.fromScene) || transition.fromScene == currentSceneName)
                    {
                        StartCoroutine(TransitionToScene(transition.toScene));
                        return;
                    }
                }
            }

            Debug.LogWarning($"[GameKitSceneFlow] No valid transition found for trigger: {triggerName}");
        }

        private IEnumerator TransitionToScene(string targetSceneName)
        {
            Debug.Log($"[GameKitSceneFlow] Transitioning from '{currentSceneName}' to '{targetSceneName}'");

            // Find target scene definition
            var targetScene = scenes.Find(s => s.name == targetSceneName);
            if (targetScene == null)
            {
                Debug.LogError($"[GameKitSceneFlow] Scene '{targetSceneName}' not found in flow");
                yield break;
            }

            // Unload current scene if exists
            if (!string.IsNullOrEmpty(currentSceneName))
            {
                var currentScene = scenes.Find(s => s.name == currentSceneName);
                if (currentScene != null && currentScene.loadMode == SceneLoadMode.Additive)
                {
                    yield return SceneManager.UnloadSceneAsync(currentScene.scenePath);
                }
            }

            // Unload old shared scenes
            foreach (var sharedScenePath in loadedSharedScenes)
            {
                yield return SceneManager.UnloadSceneAsync(sharedScenePath);
            }
            loadedSharedScenes.Clear();

            // Load target scene
            if (targetScene.loadMode == SceneLoadMode.Single)
            {
                yield return SceneManager.LoadSceneAsync(targetScene.scenePath, LoadSceneMode.Single);
            }
            else
            {
                yield return SceneManager.LoadSceneAsync(targetScene.scenePath, LoadSceneMode.Additive);
            }

            currentSceneName = targetSceneName;

            // Load shared scenes for target
            foreach (var groupName in targetScene.sharedGroupNames)
            {
                var group = sharedGroups.Find(g => g.groupName == groupName);
                if (group != null)
                {
                    foreach (var scenePath in group.scenePaths)
                    {
                        yield return SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
                        loadedSharedScenes.Add(scenePath);
                    }
                }
            }

            Debug.Log($"[GameKitSceneFlow] Transition complete. Current scene: {currentSceneName}");
        }

        public static void Transition(string triggerName)
        {
            if (instance != null)
            {
                instance.TriggerTransition(triggerName);
            }
            else
            {
                Debug.LogError("[GameKitSceneFlow] No SceneFlow instance found");
            }
        }

        [Serializable]
        public class SceneDefinition
        {
            public string name;
            public string scenePath;
            public SceneLoadMode loadMode;
            public List<string> sharedGroupNames = new List<string>();
        }

        [Serializable]
        public class SceneTransition
        {
            public string trigger;
            public string fromScene;
            public string toScene;
        }

        [Serializable]
        public class SharedSceneGroup
        {
            public string groupName;
            public List<string> scenePaths = new List<string>();
        }

        public enum SceneLoadMode
        {
            Single,
            Additive
        }
    }
}

