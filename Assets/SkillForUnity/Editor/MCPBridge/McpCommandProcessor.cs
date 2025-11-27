using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace MCP.Editor
{
    /// <summary>
    /// Processes MCP tool commands and executes corresponding Unity Editor operations.
    /// Supports management operations for scenes, GameObjects, components, and assets.
    /// </summary>
    internal static partial class McpCommandProcessor
    {
        /// <summary>
        /// Executes an MCP command and returns the result.
        /// </summary>
        /// <param name="command">The command to execute containing tool name and payload.</param>
        /// <returns>Result dictionary with operation-specific data.</returns>
        /// <exception cref="InvalidOperationException">Thrown when tool name is not supported.</exception>
        public static object Execute(McpIncomingCommand command)
        {
            return command.ToolName switch
            {
                "pingUnityEditor" => HandlePing(),
                "sceneManage" => HandleSceneManage(command.Payload),
                "gameObjectManage" => HandleGameObjectManage(command.Payload),
                "componentManage" => HandleComponentManage(command.Payload),
                "assetManage" => HandleAssetManage(command.Payload),
                "uguiRectAdjust" => HandleUguiRectAdjust(command.Payload),
                "uguiAnchorManage" => HandleUguiAnchorManage(command.Payload),
                "uguiManage" => HandleUguiManage(command.Payload),
                "uguiCreateFromTemplate" => HandleUguiCreateFromTemplate(command.Payload),
                "uguiLayoutManage" => HandleUguiLayoutManage(command.Payload),
                "uguiDetectOverlaps" => HandleUguiDetectOverlaps(command.Payload),
                "sceneQuickSetup" => HandleSceneQuickSetup(command.Payload),
                "gameObjectCreateFromTemplate" => HandleGameObjectCreateFromTemplate(command.Payload),
                "tagLayerManage" => HandleTagLayerManage(command.Payload),
                "prefabManage" => HandlePrefabManage(command.Payload),
                "scriptableObjectManage" => HandleScriptableObjectManage(command.Payload),
                "projectSettingsManage" => HandleProjectSettingsManage(command.Payload),
                "renderPipelineManage" => HandleRenderPipelineManage(command.Payload),
                "constantConvert" => HandleConstantConvert(command.Payload),
                "designPatternGenerate" => HandleDesignPatternGenerate(command.Payload),
                "scriptTemplateGenerate" => HandleScriptTemplateGenerate(command.Payload),
                "templateManage" => HandleTemplateManage(command.Payload),
                "menuHierarchyCreate" => HandleMenuHierarchyCreate(command.Payload),
                _ => throw new InvalidOperationException($"Unsupported tool name: {command.ToolName}"),
            };
        }

        /// <summary>
        /// Handles ping requests to verify Unity Editor connectivity.
        /// </summary>
        /// <returns>Dictionary containing Unity version, project name, and current timestamp.</returns>
        private static object HandlePing()
        {
            return new Dictionary<string, object>
            {
                ["editor"] = Application.unityVersion,
                ["project"] = Application.productName,
                ["time"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };
        }

        /// <summary>
        /// Handles scene management operations (create, load, save, delete, duplicate).
        /// </summary>
        /// <param name="payload">Operation parameters including 'operation' type and scene path.</param>
        /// <returns>Result dictionary with operation-specific data.</returns>
        /// <exception cref="InvalidOperationException">Thrown when operation is invalid or missing.</exception>
        // NOTE: Scene management operations have been moved to Scene/McpCommandProcessor.Scene.cs
        // This includes: HandleSceneManage, CreateScene, LoadScene, SaveScenes, DeleteScene,
        // DuplicateScene, InspectScene, and all build settings operations.


        /// <summary>
        /// Handles GameObject management operations (create, delete, move, rename, duplicate).
        /// </summary>
        /// <param name="payload">Operation parameters including 'operation' type and GameObject path.</param>
        /// <returns>Result dictionary with GameObject hierarchy path and instance ID.</returns>
        /// <exception cref="InvalidOperationException">Thrown when operation or required parameters are invalid.</exception>

        // NOTE: GameObject management operations have been moved to GameObject/McpCommandProcessor.GameObject.cs
        // This includes: HandleGameObjectManage, CreateGameObject, DeleteGameObject, MoveGameObject, RenameGameObject,
        // UpdateGameObject, DuplicateGameObject, InspectGameObject, FindMultipleGameObjects, DeleteMultipleGameObjects,
        // and InspectMultipleGameObjects.

        /// <summary>
        /// Handles component management operations (add, remove, update, inspect).
        /// Uses reflection to set component properties from the payload.
        /// Monitors compilation status and returns whether compilation was triggered.
        /// </summary>
        /// <param name="payload">Operation parameters including 'operation', 'gameObjectPath', 'componentType', and optional 'propertyChanges'.</param>
        /// <returns>Result dictionary with component type, GameObject path, and compilation status.</returns>
        /// <exception cref="InvalidOperationException">Thrown when GameObject or component type is not found.</exception>

        // NOTE: Component management operations have been moved to Component/McpCommandProcessor.Component.cs
        // This includes: HandleComponentManage, AddComponent, RemoveComponent, UpdateComponent, InspectComponent,
        // AddMultipleComponents, RemoveMultipleComponents, UpdateMultipleComponents, and InspectMultipleComponents.


        // NOTE: Asset management operations have been moved to Asset/McpCommandProcessor.Asset.cs
        // This includes: HandleAssetManage, CreateTextAsset, UpdateTextAsset, UpdateAssetImporter,
        // UpdateAsset, DeleteAsset, RenameAsset, DuplicateAsset, InspectAsset,
        // FindMultipleAssets, DeleteMultipleAssets, and InspectMultipleAssets.



        // NOTE: UI (UGUI) management operations have been moved to UI/McpCommandProcessor.UI.cs
        // This includes: HandleUguiRectAdjust, HandleUguiAnchorManage, HandleUguiManage,
        // HandleUguiCreateFromTemplate, HandleUguiLayoutManage, HandleUguiDetectOverlaps,
        // and all related UI helper methods (DetectRectOverlap, etc.).

        private static object HandleSceneQuickSetup(Dictionary<string, object> payload)


        // NOTE: Template operations have been moved to Template/McpCommandProcessor.Template.cs
        // This includes: HandleSceneQuickSetup, HandleGameObjectCreateFromTemplate, HandleDesignPatternGenerate,
        // HandleScriptTemplateGenerate, HandleTemplateManage, HandleMenuHierarchyCreate, and all related template methods.

        /// <summary>
        /// Inspects the current scene context including hierarchy, GameObjects, and components.
        /// Provides a comprehensive overview for Claude to understand the current state.
        /// </summary>
        /// <param name="payload">Options for what to include in the inspection.</param>
        /// <returns>Result dictionary with scene context information.</returns>
        private static object HandleContextInspect(Dictionary<string, object> payload)

        // NOTE: Context inspection and compilation management utilities have been moved to Utilities/McpCommandProcessor.Utilities.cs

        /// <summary>
        /// Handles tag and layer management operations.
        /// Supports setting/getting tags and layers on GameObjects,
        /// and adding/removing tags and layers from the project.
        /// </summary>
        /// <param name="payload">Operation parameters including 'operation' type.</param>
        /// <returns>Result dictionary with operation-specific data.</returns>

        // NOTE: Settings management operations have been moved to Settings/McpCommandProcessor.Settings.cs
        // This includes: HandleTagLayerManage, HandleProjectSettingsManage, HandleRenderPipelineManage,
        // HandleConstantConvert, and all related settings management methods.

        private static object HandleDesignPatternGenerate(Dictionary<string, object> payload)

        /// <summary>
        /// Detects if compilation has started after script operations.
        /// Waits for a short period and checks if EditorApplication.isCompiling becomes true.
        /// </summary>
        /// <param name="wasCompilingBefore">Whether compilation was already running before operations.</param>
        /// <param name="maxWaitSeconds">Maximum time to wait for compilation to start.</param>
        /// <returns>True if compilation started, false otherwise.</returns>
        private static bool DetectCompilationStart(bool wasCompilingBefore, float maxWaitSeconds = 1.5f)
        /// <summary>
        /// Waits for ongoing compilation to complete.
        /// Returns immediately if not compiling.
        /// </summary>
        /// <param name="maxWaitSeconds">Maximum time to wait for compilation to complete.</param>
        /// <returns>True if compilation completed successfully, false if timeout or still compiling.</returns>
        private static bool WaitForCompilationComplete(float maxWaitSeconds = 30f)
        /// <summary>
        /// Ensures no compilation is in progress before executing a tool operation.
        /// If compilation is in progress, waits for it to complete.
        /// </summary>
        /// <param name="toolName">Name of the tool being executed (for logging).</param>
        /// <param name="maxWaitSeconds">Maximum time to wait for compilation to complete.</param>
        /// <returns>Dictionary with status information if waiting occurred, null if no wait was needed.</returns>
        private static Dictionary<string, object> EnsureNoCompilationInProgress(string toolName, float maxWaitSeconds = 30f)
        #region Template Management

        /// <summary>
        /// Handles template management operations for customizing existing GameObjects.
        /// Supports operations: customize (add components/children to existing object), convertToPrefab (save as prefab).
        /// </summary>
        /// <param name="payload">Operation parameters including 'operation' type.</param>
        /// <returns>Result dictionary with operation-specific data.</returns>
        private static object HandleTemplateManage(Dictionary<string, object> payload)
        #endregion
    }
}

