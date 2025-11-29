using System;
using System.Collections.Generic;
using MCP.Editor.Base;
using UnityAIForge.GameKit;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MCP.Editor.Handlers.GameKit
{
    /// <summary>
    /// GameKit UI Command handler: create command panels with buttons.
    /// </summary>
    public class GameKitUICommandHandler : BaseCommandHandler
    {
        private static readonly string[] Operations = { "createCommandPanel", "addCommand", "inspect", "delete" };

        public override string Category => "gamekitUICommand";

        public override IEnumerable<string> SupportedOperations => Operations;

        protected override bool RequiresCompilationWait(string operation) => false;

        protected override object ExecuteOperation(string operation, Dictionary<string, object> payload)
        {
            return operation switch
            {
                "createCommandPanel" => CreateCommandPanel(payload),
                "addCommand" => AddCommand(payload),
                "inspect" => InspectCommandPanel(payload),
                "delete" => DeleteCommandPanel(payload),
                _ => throw new InvalidOperationException($"Unsupported GameKit UI Command operation: {operation}"),
            };
        }

        #region Create Command Panel

        private object CreateCommandPanel(Dictionary<string, object> payload)
        {
            var panelId = GetString(payload, "panelId") ?? $"CommandPanel_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var canvasPath = GetString(payload, "canvasPath");
            var targetActorId = GetString(payload, "targetActorId");
            var layout = GetString(payload, "layout") ?? "vertical";

            if (string.IsNullOrEmpty(canvasPath))
            {
                throw new InvalidOperationException("canvasPath is required for createCommandPanel.");
            }

            var canvas = ResolveGameObject(canvasPath);

            // Create panel GameObject
            var panelGo = new GameObject(panelId, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(panelGo, "Create Command Panel");
            panelGo.transform.SetParent(canvas.transform, false);

            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.sizeDelta = new Vector2(0, 100);
            panelRect.anchoredPosition = Vector2.zero;

            // Add background image
            var panelImage = Undo.AddComponent<Image>(panelGo);
            panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Add layout group
            AddLayoutGroup(panelGo, layout);

            // Add GameKitUICommand component
            var uiCommand = Undo.AddComponent<GameKitUICommand>(panelGo);
            uiCommand.Initialize(panelId, targetActorId);

            // Create command buttons if specified
            if (payload.TryGetValue("commands", out var commandsObj) && commandsObj is List<object> commandsList)
            {
                var buttonSize = GetButtonSize(payload);
                foreach (var commandObj in commandsList)
                {
                    if (commandObj is Dictionary<string, object> commandDict)
                    {
                        var name = GetStringFromDict(commandDict, "name");
                        var label = GetStringFromDict(commandDict, "label") ?? name;
                        CreateCommandButton(panelGo, uiCommand, name, label, buttonSize);
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(panelGo.scene);
            return CreateSuccessResponse(("panelId", panelId), ("path", BuildGameObjectPath(panelGo)));
        }

        private void AddLayoutGroup(GameObject go, string layout)
        {
            switch (layout.ToLowerInvariant())
            {
                case "horizontal":
                    var hLayout = Undo.AddComponent<HorizontalLayoutGroup>(go);
                    hLayout.spacing = 10;
                    hLayout.padding = new RectOffset(10, 10, 10, 10);
                    hLayout.childAlignment = TextAnchor.MiddleCenter;
                    hLayout.childControlWidth = false;
                    hLayout.childControlHeight = false;
                    break;

                case "vertical":
                    var vLayout = Undo.AddComponent<VerticalLayoutGroup>(go);
                    vLayout.spacing = 10;
                    vLayout.padding = new RectOffset(10, 10, 10, 10);
                    vLayout.childAlignment = TextAnchor.UpperCenter;
                    vLayout.childControlWidth = false;
                    vLayout.childControlHeight = false;
                    break;

                case "grid":
                    var gLayout = Undo.AddComponent<GridLayoutGroup>(go);
                    gLayout.spacing = new Vector2(10, 10);
                    gLayout.padding = new RectOffset(10, 10, 10, 10);
                    gLayout.cellSize = new Vector2(100, 50);
                    break;
            }
        }

        private Vector2 GetButtonSize(Dictionary<string, object> payload)
        {
            if (payload.TryGetValue("buttonSize", out var sizeObj) && sizeObj is Dictionary<string, object> sizeDict)
            {
                float width = sizeDict.TryGetValue("width", out var wObj) ? Convert.ToSingle(wObj) : 100f;
                float height = sizeDict.TryGetValue("height", out var hObj) ? Convert.ToSingle(hObj) : 50f;
                return new Vector2(width, height);
            }
            return new Vector2(100, 50);
        }

        private void CreateCommandButton(GameObject parent, GameKitUICommand uiCommand, string commandName, string label, Vector2 size)
        {
            // Create button GameObject
            var buttonGo = new GameObject(commandName, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(buttonGo, "Create Command Button");
            buttonGo.transform.SetParent(parent.transform, false);

            var buttonRect = buttonGo.GetComponent<RectTransform>();
            buttonRect.sizeDelta = size;

            var buttonImage = Undo.AddComponent<Image>(buttonGo);
            buttonImage.color = Color.white;

            var button = Undo.AddComponent<Button>(buttonGo);

            // Create text child
            var textGo = new GameObject("Text", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(textGo, "Create Button Text");
            textGo.transform.SetParent(buttonGo.transform, false);

            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var text = Undo.AddComponent<Text>(textGo);
            text.text = label;
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;

            // Register button with UICommand component
            Undo.RecordObject(uiCommand, "Register Button");
            uiCommand.RegisterButton(commandName, button);
        }

        #endregion

        #region Add Command

        private object AddCommand(Dictionary<string, object> payload)
        {
            var panelId = GetString(payload, "panelId");
            if (string.IsNullOrEmpty(panelId))
            {
                throw new InvalidOperationException("panelId is required for addCommand.");
            }

            var uiCommand = FindUICommandById(panelId);
            if (uiCommand == null)
            {
                throw new InvalidOperationException($"Command panel with ID '{panelId}' not found.");
            }

            if (payload.TryGetValue("commands", out var commandsObj) && commandsObj is List<object> commandsList)
            {
                var buttonSize = GetButtonSize(payload);
                foreach (var commandObj in commandsList)
                {
                    if (commandObj is Dictionary<string, object> commandDict)
                    {
                        var name = GetStringFromDict(commandDict, "name");
                        var label = GetStringFromDict(commandDict, "label") ?? name;
                        CreateCommandButton(uiCommand.gameObject, uiCommand, name, label, buttonSize);
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(uiCommand.gameObject.scene);
            return CreateSuccessResponse(("panelId", panelId), ("path", BuildGameObjectPath(uiCommand.gameObject)));
        }

        #endregion

        #region Inspect

        private object InspectCommandPanel(Dictionary<string, object> payload)
        {
            var panelId = GetString(payload, "panelId");
            if (string.IsNullOrEmpty(panelId))
            {
                throw new InvalidOperationException("panelId is required for inspect.");
            }

            var uiCommand = FindUICommandById(panelId);
            if (uiCommand == null)
            {
                return CreateSuccessResponse(("found", false), ("panelId", panelId));
            }

            var info = new Dictionary<string, object>
            {
                { "found", true },
                { "panelId", uiCommand.PanelId },
                { "path", BuildGameObjectPath(uiCommand.gameObject) },
                { "targetActorId", uiCommand.TargetActorId }
            };

            return CreateSuccessResponse(("commandPanel", info));
        }

        #endregion

        #region Delete

        private object DeleteCommandPanel(Dictionary<string, object> payload)
        {
            var panelId = GetString(payload, "panelId");
            if (string.IsNullOrEmpty(panelId))
            {
                throw new InvalidOperationException("panelId is required for delete.");
            }

            var uiCommand = FindUICommandById(panelId);
            if (uiCommand == null)
            {
                throw new InvalidOperationException($"Command panel with ID '{panelId}' not found.");
            }

            var scene = uiCommand.gameObject.scene;
            Undo.DestroyObjectImmediate(uiCommand.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);

            return CreateSuccessResponse(("panelId", panelId), ("deleted", true));
        }

        #endregion

        #region Helpers

        private GameKitUICommand FindUICommandById(string panelId)
        {
            var uiCommands = UnityEngine.Object.FindObjectsByType<GameKitUICommand>(FindObjectsSortMode.None);
            foreach (var uiCommand in uiCommands)
            {
                if (uiCommand.PanelId == panelId)
                {
                    return uiCommand;
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

