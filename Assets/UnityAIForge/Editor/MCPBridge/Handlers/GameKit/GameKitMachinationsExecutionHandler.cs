using System;
using System.Collections.Generic;
using MCP.Editor.Base;
using UnityAIForge.GameKit;
using UnityEditor;
using UnityEngine;

namespace MCP.Editor.Handlers.GameKit
{
    /// <summary>
    /// GameKit Machinations Execution handler: execute diagram logic on resource managers.
    /// Allows manual execution of flows, converters, and trigger checks.
    /// </summary>
    public class GameKitMachinationsExecutionHandler : BaseCommandHandler
    {
        private static readonly string[] Operations = { "processFlows", "executeConverter", "checkTriggers", "addController" };

        public override string Category => "gamekitMachinationsExecution";

        public override IEnumerable<string> SupportedOperations => Operations;

        protected override bool RequiresCompilationWait(string operation) => false;

        protected override object ExecuteOperation(string operation, Dictionary<string, object> payload)
        {
            return operation switch
            {
                "processFlows" => ProcessFlows(payload),
                "executeConverter" => ExecuteConverter(payload),
                "checkTriggers" => CheckTriggers(payload),
                "addController" => AddController(payload),
                _ => throw new InvalidOperationException($"Unsupported Machinations Execution operation: {operation}"),
            };
        }

        #region Process Flows

        private object ProcessFlows(Dictionary<string, object> payload)
        {
            var managerId = GetString(payload, "managerId");
            var deltaTime = GetFloat(payload, "deltaTime", Time.deltaTime);
            var flowId = GetString(payload, "flowId"); // Optional: process specific flow

            if (string.IsNullOrEmpty(managerId))
            {
                throw new InvalidOperationException("managerId is required for processFlows.");
            }

            var manager = FindManagerById(managerId);
            if (manager == null)
            {
                throw new InvalidOperationException($"Manager with ID '{managerId}' not found.");
            }

            var resourceManager = manager.GetComponent<GameKitResourceManager>();
            if (resourceManager == null)
            {
                throw new InvalidOperationException($"Manager '{managerId}' does not have a GameKitResourceManager component.");
            }

            if (resourceManager.MachinationsAsset == null)
            {
                throw new InvalidOperationException($"Manager '{managerId}' does not have a Machinations asset assigned.");
            }

            // Process flows
            if (string.IsNullOrEmpty(flowId))
            {
                // Process all flows
                resourceManager.ProcessFlows(deltaTime);
            }
            else
            {
                // Process specific flow
                resourceManager.ProcessFlow(flowId, deltaTime);
            }

            return CreateSuccessResponse(
                ("managerId", managerId),
                ("processed", true),
                ("deltaTime", deltaTime)
            );
        }

        #endregion

        #region Execute Converter

        private object ExecuteConverter(Dictionary<string, object> payload)
        {
            var managerId = GetString(payload, "managerId");
            var converterId = GetString(payload, "converterId");
            var amount = GetFloat(payload, "amount", 1f);

            if (string.IsNullOrEmpty(managerId))
            {
                throw new InvalidOperationException("managerId is required for executeConverter.");
            }

            if (string.IsNullOrEmpty(converterId))
            {
                throw new InvalidOperationException("converterId is required for executeConverter.");
            }

            var manager = FindManagerById(managerId);
            if (manager == null)
            {
                throw new InvalidOperationException($"Manager with ID '{managerId}' not found.");
            }

            var resourceManager = manager.GetComponent<GameKitResourceManager>();
            if (resourceManager == null)
            {
                throw new InvalidOperationException($"Manager '{managerId}' does not have a GameKitResourceManager component.");
            }

            if (resourceManager.MachinationsAsset == null)
            {
                throw new InvalidOperationException($"Manager '{managerId}' does not have a Machinations asset assigned.");
            }

            // Execute converter
            bool success = resourceManager.ExecuteConverter(converterId, amount);

            return CreateSuccessResponse(
                ("managerId", managerId),
                ("converterId", converterId),
                ("success", success),
                ("amount", amount)
            );
        }

        #endregion

        #region Check Triggers

        private object CheckTriggers(Dictionary<string, object> payload)
        {
            var managerId = GetString(payload, "managerId");
            var triggerName = GetString(payload, "triggerName"); // Optional: check specific trigger

            if (string.IsNullOrEmpty(managerId))
            {
                throw new InvalidOperationException("managerId is required for checkTriggers.");
            }

            var manager = FindManagerById(managerId);
            if (manager == null)
            {
                throw new InvalidOperationException($"Manager with ID '{managerId}' not found.");
            }

            var resourceManager = manager.GetComponent<GameKitResourceManager>();
            if (resourceManager == null)
            {
                throw new InvalidOperationException($"Manager '{managerId}' does not have a GameKitResourceManager component.");
            }

            if (resourceManager.MachinationsAsset == null)
            {
                throw new InvalidOperationException($"Manager '{managerId}' does not have a Machinations asset assigned.");
            }

            // Check triggers
            if (string.IsNullOrEmpty(triggerName))
            {
                // Check all triggers
                resourceManager.CheckTriggers();
                return CreateSuccessResponse(
                    ("managerId", managerId),
                    ("checked", "all")
                );
            }
            else
            {
                // Check specific trigger
                bool triggered = resourceManager.CheckTrigger(triggerName);
                return CreateSuccessResponse(
                    ("managerId", managerId),
                    ("triggerName", triggerName),
                    ("triggered", triggered)
                );
            }
        }

        #endregion

        #region Add Controller

        private object AddController(Dictionary<string, object> payload)
        {
            var managerId = GetString(payload, "managerId");
            var autoProcessFlows = GetBool(payload, "autoProcessFlows", true);
            var autoProcessConverters = GetBool(payload, "autoProcessConverters", false);
            var autoCheckTriggers = GetBool(payload, "autoCheckTriggers", true);
            var timeScale = GetFloat(payload, "timeScale", 1f);

            if (string.IsNullOrEmpty(managerId))
            {
                throw new InvalidOperationException("managerId is required for addController.");
            }

            var manager = FindManagerById(managerId);
            if (manager == null)
            {
                throw new InvalidOperationException($"Manager with ID '{managerId}' not found.");
            }

            var resourceManager = manager.GetComponent<GameKitResourceManager>();
            if (resourceManager == null)
            {
                throw new InvalidOperationException($"Manager '{managerId}' does not have a GameKitResourceManager component.");
            }

            // Check if controller already exists
            var existingController = manager.GetComponent<GameKitMachinationsController>();
            if (existingController != null)
            {
                Debug.LogWarning($"[GameKitMachinationsExecution] Controller already exists on manager '{managerId}'");
                return CreateSuccessResponse(
                    ("managerId", managerId),
                    ("added", false),
                    ("message", "Controller already exists")
                );
            }

            // Add controller
            var controller = Undo.AddComponent<GameKitMachinationsController>(manager.gameObject);

            // Configure settings using reflection
            var autoFlowsField = typeof(GameKitMachinationsController).GetField("autoProcessFlows",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            autoFlowsField?.SetValue(controller, autoProcessFlows);

            var autoConvertersField = typeof(GameKitMachinationsController).GetField("autoProcessConverters",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            autoConvertersField?.SetValue(controller, autoProcessConverters);

            var autoTriggersField = typeof(GameKitMachinationsController).GetField("autoCheckTriggers",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            autoTriggersField?.SetValue(controller, autoCheckTriggers);

            var timeScaleField = typeof(GameKitMachinationsController).GetField("timeScale",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            timeScaleField?.SetValue(controller, timeScale);

            EditorUtility.SetDirty(controller);

            return CreateSuccessResponse(
                ("managerId", managerId),
                ("added", true),
                ("autoProcessFlows", autoProcessFlows),
                ("autoCheckTriggers", autoCheckTriggers)
            );
        }

        #endregion

        #region Helpers

        private GameKitManager FindManagerById(string managerId)
        {
            var managers = UnityEngine.Object.FindObjectsByType<GameKitManager>(FindObjectsSortMode.None);
            foreach (var manager in managers)
            {
                if (manager.ManagerId == managerId)
                {
                    return manager;
                }
            }
            return null;
        }

        private float GetFloat(Dictionary<string, object> dict, string key, float defaultValue = 0f)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return Convert.ToSingle(value);
            }
            return defaultValue;
        }

        #endregion
    }
}

