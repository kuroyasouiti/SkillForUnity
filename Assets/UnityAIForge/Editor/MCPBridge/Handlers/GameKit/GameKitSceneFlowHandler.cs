using System;
using System.Collections.Generic;
using System.Linq;
using MCP.Editor.Base;
using UnityAIForge.GameKit;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MCP.Editor.Handlers.GameKit
{
    /// <summary>
    /// GameKit SceneFlow handler: manage scene transitions and flow.
    /// </summary>
    public class GameKitSceneFlowHandler : BaseCommandHandler
    {
        private static readonly string[] Operations = { "create", "update", "inspect", "delete", "transition" };

        public override string Category => "gamekitSceneFlow";

        public override IEnumerable<string> SupportedOperations => Operations;

        protected override bool RequiresCompilationWait(string operation) => false;

        protected override object ExecuteOperation(string operation, Dictionary<string, object> payload)
        {
            return operation switch
            {
                "create" => CreateSceneFlow(payload),
                "update" => UpdateSceneFlow(payload),
                "inspect" => InspectSceneFlow(payload),
                "delete" => DeleteSceneFlow(payload),
                "transition" => TriggerTransition(payload),
                _ => throw new InvalidOperationException($"Unsupported GameKit SceneFlow operation: {operation}"),
            };
        }

        #region Create SceneFlow

        private object CreateSceneFlow(Dictionary<string, object> payload)
        {
            var flowId = GetString(payload, "flowId") ?? $"SceneFlow_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var managerScenePath = GetString(payload, "managerScenePath");

            // Create SceneFlow GameObject
            var flowGo = new GameObject(flowId);
            Undo.RegisterCreatedObjectUndo(flowGo, "Create SceneFlow");

            var sceneFlow = Undo.AddComponent<GameKitSceneFlow>(flowGo);
            sceneFlow.Initialize(flowId);

            // Add scenes
            if (payload.TryGetValue("scenes", out var scenesObj) && scenesObj is List<object> scenesList)
            {
                foreach (var sceneObj in scenesList)
                {
                    if (sceneObj is Dictionary<string, object> sceneDict)
                    {
                        var name = GetStringFromDict(sceneDict, "name");
                        var scenePath = GetStringFromDict(sceneDict, "scenePath");
                        var loadModeStr = GetStringFromDict(sceneDict, "loadMode") ?? "additive";
                        var loadMode = loadModeStr.ToLowerInvariant() == "single" 
                            ? GameKitSceneFlow.SceneLoadMode.Single 
                            : GameKitSceneFlow.SceneLoadMode.Additive;

                        var sharedScenePaths = new List<string>();
                        if (sceneDict.TryGetValue("sharedScenePaths", out var sharedScenesObj) && sharedScenesObj is List<object> scenePathsList)
                        {
                            sharedScenePaths = scenePathsList.Select(p => p.ToString()).ToList();
                        }
                        // Support legacy "sharedGroups" for backward compatibility (treated as scene paths)
                        else if (sceneDict.TryGetValue("sharedGroups", out var sharedGroupsObj) && sharedGroupsObj is List<object> groupsList)
                        {
                            sharedScenePaths = groupsList.Select(g => g.ToString()).ToList();
                        }

                        sceneFlow.AddScene(name, scenePath, loadMode, sharedScenePaths.ToArray());
                    }
                }
            }

            // Add transitions (now scene-centric: each scene defines its own transitions)
            if (payload.TryGetValue("transitions", out var transitionsObj) && transitionsObj is List<object> transitionsList)
            {
                foreach (var transitionObj in transitionsList)
                {
                    if (transitionObj is Dictionary<string, object> transitionDict)
                    {
                        var fromScene = GetStringFromDict(transitionDict, "fromScene");
                        var trigger = GetStringFromDict(transitionDict, "trigger");
                        var toScene = GetStringFromDict(transitionDict, "toScene");
                        
                        if (string.IsNullOrEmpty(fromScene))
                        {
                            Debug.LogWarning("[GameKitSceneFlowHandler] Transition missing 'fromScene'. Transitions must be added to a specific scene.");
                            continue;
                        }
                        
                        sceneFlow.AddTransition(fromScene, trigger, toScene);
                    }
                }
            }

            // Legacy support: Add shared scene groups (deprecated - use sharedScenePaths in scene definition instead)
            if (payload.TryGetValue("sharedSceneGroups", out var sharedGroupsDict) && sharedGroupsDict is Dictionary<string, object> groupsDict)
            {
                Debug.LogWarning("[GameKitSceneFlowHandler] 'sharedSceneGroups' is deprecated. Use 'sharedScenePaths' in scene definitions instead.");
                // For backward compatibility, we could add these to scenes, but it's better to just warn
            }

            EditorSceneManager.MarkSceneDirty(flowGo.scene);
            return CreateSuccessResponse(("flowId", flowId), ("path", BuildGameObjectPath(flowGo)));
        }

        #endregion

        #region Update SceneFlow

        private object UpdateSceneFlow(Dictionary<string, object> payload)
        {
            var flowId = GetString(payload, "flowId");
            if (string.IsNullOrEmpty(flowId))
            {
                throw new InvalidOperationException("flowId is required for update.");
            }

            var sceneFlow = FindSceneFlowById(flowId);
            if (sceneFlow == null)
            {
                throw new InvalidOperationException($"SceneFlow with ID '{flowId}' not found.");
            }

            Undo.RecordObject(sceneFlow, "Update SceneFlow");

            // Add new scenes
            if (payload.TryGetValue("scenes", out var scenesObj) && scenesObj is List<object> scenesList)
            {
                foreach (var sceneObj in scenesList)
                {
                    if (sceneObj is Dictionary<string, object> sceneDict)
                    {
                        var name = GetStringFromDict(sceneDict, "name");
                        var scenePath = GetStringFromDict(sceneDict, "scenePath");
                        var loadModeStr = GetStringFromDict(sceneDict, "loadMode") ?? "additive";
                        var loadMode = loadModeStr.ToLowerInvariant() == "single"
                            ? GameKitSceneFlow.SceneLoadMode.Single
                            : GameKitSceneFlow.SceneLoadMode.Additive;

                        var sharedScenePaths = new List<string>();
                        if (sceneDict.TryGetValue("sharedScenePaths", out var sharedScenesObj) && sharedScenesObj is List<object> scenePathsList)
                        {
                            sharedScenePaths = scenePathsList.Select(p => p.ToString()).ToList();
                        }
                        // Support legacy "sharedGroups" for backward compatibility
                        else if (sceneDict.TryGetValue("sharedGroups", out var sharedGroupsObj2) && sharedGroupsObj2 is List<object> groupsList)
                        {
                            sharedScenePaths = groupsList.Select(g => g.ToString()).ToList();
                        }

                        sceneFlow.AddScene(name, scenePath, loadMode, sharedScenePaths.ToArray());
                    }
                }
            }

            // Add new transitions
            if (payload.TryGetValue("transitions", out var transitionsObj) && transitionsObj is List<object> transitionsList)
            {
                foreach (var transitionObj in transitionsList)
                {
                    if (transitionObj is Dictionary<string, object> transitionDict)
                    {
                        var fromScene = GetStringFromDict(transitionDict, "fromScene");
                        var trigger = GetStringFromDict(transitionDict, "trigger");
                        var toScene = GetStringFromDict(transitionDict, "toScene");
                        
                        if (string.IsNullOrEmpty(fromScene))
                        {
                            Debug.LogWarning("[GameKitSceneFlowHandler] Transition missing 'fromScene'. Transitions must be added to a specific scene.");
                            continue;
                        }
                        
                        sceneFlow.AddTransition(fromScene, trigger, toScene);
                    }
                }
            }

            // Legacy support: Add shared scene groups (deprecated)
            if (payload.TryGetValue("sharedSceneGroups", out var sharedGroupsDict2) && sharedGroupsDict2 is Dictionary<string, object> groupsDict)
            {
                Debug.LogWarning("[GameKitSceneFlowHandler] 'sharedSceneGroups' is deprecated. Use 'sharedScenePaths' in scene definitions instead.");
            }

            EditorSceneManager.MarkSceneDirty(sceneFlow.gameObject.scene);
            return CreateSuccessResponse(("flowId", flowId), ("path", BuildGameObjectPath(sceneFlow.gameObject)));
        }

        #endregion

        #region Inspect SceneFlow

        private object InspectSceneFlow(Dictionary<string, object> payload)
        {
            var flowId = GetString(payload, "flowId");
            if (string.IsNullOrEmpty(flowId))
            {
                throw new InvalidOperationException("flowId is required for inspect.");
            }

            var sceneFlow = FindSceneFlowById(flowId);
            if (sceneFlow == null)
            {
                return CreateSuccessResponse(("found", false), ("flowId", flowId));
            }

            var info = new Dictionary<string, object>
            {
                { "found", true },
                { "flowId", sceneFlow.FlowId },
                { "path", BuildGameObjectPath(sceneFlow.gameObject) },
                { "currentScene", sceneFlow.CurrentScene }
            };

            return CreateSuccessResponse(("sceneFlow", info));
        }

        #endregion

        #region Delete SceneFlow

        private object DeleteSceneFlow(Dictionary<string, object> payload)
        {
            var flowId = GetString(payload, "flowId");
            if (string.IsNullOrEmpty(flowId))
            {
                throw new InvalidOperationException("flowId is required for delete.");
            }

            var sceneFlow = FindSceneFlowById(flowId);
            if (sceneFlow == null)
            {
                throw new InvalidOperationException($"SceneFlow with ID '{flowId}' not found.");
            }

            var scene = sceneFlow.gameObject.scene;
            Undo.DestroyObjectImmediate(sceneFlow.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);

            return CreateSuccessResponse(("flowId", flowId), ("deleted", true));
        }

        #endregion

        #region Trigger Transition

        private object TriggerTransition(Dictionary<string, object> payload)
        {
            var triggerName = GetString(payload, "triggerName");
            if (string.IsNullOrEmpty(triggerName))
            {
                throw new InvalidOperationException("triggerName is required for transition.");
            }

            // This operation is meant for runtime, but we can log it in editor
            Debug.Log($"[GameKitSceneFlow] Transition trigger '{triggerName}' would be executed at runtime.");

            return CreateSuccessResponse(("triggerName", triggerName), ("note", "Transition will execute at runtime"));
        }

        #endregion

        #region Helpers

        private GameKitSceneFlow FindSceneFlowById(string flowId)
        {
            var sceneFlows = UnityEngine.Object.FindObjectsByType<GameKitSceneFlow>(FindObjectsSortMode.None);
            foreach (var sceneFlow in sceneFlows)
            {
                if (sceneFlow.FlowId == flowId)
                {
                    return sceneFlow;
                }
            }
            return null;
        }

        private string GetStringFromDict(Dictionary<string, object> dict, string key)
        {
            return dict.TryGetValue(key, out var value) ? value?.ToString() : null;
        }

        private string BuildGameObjectPath(GameObject go)
        {
            var path = go.name;
            var current = go.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }

        #endregion
    }
}

