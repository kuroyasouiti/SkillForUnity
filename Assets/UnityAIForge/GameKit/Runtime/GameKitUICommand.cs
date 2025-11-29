using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityAIForge.GameKit
{
    /// <summary>
    /// GameKit UI Command Hub: bridges UI controls to GameKitActor's UnityEvents.
    /// Acts as a central hub for translating UI interactions into actor commands.
    /// </summary>
    [AddComponentMenu("SkillForUnity/GameKit/UI Command Hub")]
    public class GameKitUICommand : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string panelId;
        
        [Header("Target Actor")]
        [Tooltip("Reference to the target actor (preferred over actorId)")]
        [SerializeField] private GameKitActor targetActor;
        
        [Tooltip("Target actor ID (fallback if actor reference is not set)")]
        [SerializeField] private string targetActorId;
        
        [Header("Command Bindings")]
        [SerializeField] private List<UICommandBinding> commandBindings = new List<UICommandBinding>();

        [Header("Settings")]
        [Tooltip("Cache actor reference on Start for better performance")]
        [SerializeField] private bool cacheActorReference = true;
        
        [Tooltip("Log command execution for debugging")]
        [SerializeField] private bool logCommands = false;

        private Dictionary<string, UICommandBinding> bindingLookup;
        private bool isInitialized = false;

        public string PanelId => panelId;
        public string TargetActorId => targetActorId;
        public GameKitActor TargetActor => targetActor;

        /// <summary>
        /// Command types that map to GameKitActor's UnityEvents
        /// </summary>
        public enum CommandType
        {
            Move,           // Maps to OnMoveInput (Vector3)
            Jump,           // Maps to OnJumpInput (void)
            Action,         // Maps to OnActionInput (string)
            Look,           // Maps to OnLookInput (Vector2)
            Custom          // Custom command via SendMessage
        }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (isInitialized)
                return;

            // Build lookup dictionary for faster command execution
            bindingLookup = new Dictionary<string, UICommandBinding>();
            foreach (var binding in commandBindings)
            {
                if (!string.IsNullOrEmpty(binding.commandName))
                {
                    bindingLookup[binding.commandName] = binding;
                }
            }

            // Cache actor reference if enabled
            if (cacheActorReference && targetActor == null && !string.IsNullOrEmpty(targetActorId))
            {
                targetActor = FindActorById(targetActorId);
            }

            isInitialized = true;
        }

        /// <summary>
        /// Initialize the UI command hub with panel ID and target actor.
        /// </summary>
        public void Initialize(string id, string actorId)
        {
            panelId = id;
            targetActorId = actorId;
            targetActor = null; // Clear cached reference
            isInitialized = false;
            Initialize();
        }

        /// <summary>
        /// Register a UI button with a command binding.
        /// </summary>
        public void RegisterButton(string commandName, Button button, CommandType commandType = CommandType.Action, string commandParam = null)
        {
            if (!isInitialized)
                Initialize();

            var binding = new UICommandBinding
            {
                commandName = commandName,
                commandType = commandType,
                button = button,
                commandParameter = commandParam
            };

            commandBindings.Add(binding);
            bindingLookup[commandName] = binding;

            // Wire up button click to command execution
            button.onClick.AddListener(() => ExecuteCommand(commandName));
        }

        /// <summary>
        /// Register a UI button with direction (for movement commands).
        /// </summary>
        public void RegisterDirectionalButton(string commandName, Button button, Vector3 direction)
        {
            if (!isInitialized)
                Initialize();

            var binding = new UICommandBinding
            {
                commandName = commandName,
                commandType = CommandType.Move,
                button = button,
                moveDirection = direction
            };

            commandBindings.Add(binding);
            bindingLookup[commandName] = binding;

            button.onClick.AddListener(() => ExecuteCommand(commandName));
        }

        /// <summary>
        /// Execute a command by name.
        /// </summary>
        public void ExecuteCommand(string commandName)
        {
            if (!isInitialized)
                Initialize();

            // Get binding
            if (!bindingLookup.TryGetValue(commandName, out var binding))
            {
                Debug.LogWarning($"[GameKitUICommand] Command '{commandName}' not found in bindings");
                return;
            }

            // Get or find target actor
            var actor = GetTargetActor();
            if (actor == null)
            {
                Debug.LogWarning($"[GameKitUICommand] Target actor not found for command '{commandName}'");
                return;
            }

            // Execute command based on type
            switch (binding.commandType)
            {
                case CommandType.Move:
                    actor.SendMoveInput(binding.moveDirection);
                    if (logCommands)
                        Debug.Log($"[GameKitUICommand] Move command: {binding.moveDirection}");
                    break;

                case CommandType.Jump:
                    actor.SendJumpInput();
                    if (logCommands)
                        Debug.Log($"[GameKitUICommand] Jump command");
                    break;

                case CommandType.Action:
                    string actionParam = binding.commandParameter ?? commandName;
                    actor.SendActionInput(actionParam);
                    if (logCommands)
                        Debug.Log($"[GameKitUICommand] Action command: {actionParam}");
                    break;

                case CommandType.Look:
                    actor.SendLookInput(binding.lookDirection);
                    if (logCommands)
                        Debug.Log($"[GameKitUICommand] Look command: {binding.lookDirection}");
                    break;

                case CommandType.Custom:
                    // Send custom message for backward compatibility
                    actor.gameObject.SendMessage($"OnCommand_{commandName}", SendMessageOptions.DontRequireReceiver);
                    if (logCommands)
                        Debug.Log($"[GameKitUICommand] Custom command: {commandName}");
                    break;
            }
        }

        /// <summary>
        /// Execute a move command with direction.
        /// </summary>
        public void ExecuteMoveCommand(Vector3 direction)
        {
            var actor = GetTargetActor();
            if (actor != null)
            {
                actor.SendMoveInput(direction);
                if (logCommands)
                    Debug.Log($"[GameKitUICommand] Move: {direction}");
            }
        }

        /// <summary>
        /// Execute a jump command.
        /// </summary>
        public void ExecuteJumpCommand()
        {
            var actor = GetTargetActor();
            if (actor != null)
            {
                actor.SendJumpInput();
                if (logCommands)
                    Debug.Log($"[GameKitUICommand] Jump");
            }
        }

        /// <summary>
        /// Execute an action command with parameter.
        /// </summary>
        public void ExecuteActionCommand(string actionName)
        {
            var actor = GetTargetActor();
            if (actor != null)
            {
                actor.SendActionInput(actionName);
                if (logCommands)
                    Debug.Log($"[GameKitUICommand] Action: {actionName}");
            }
        }

        /// <summary>
        /// Execute a look command with direction.
        /// </summary>
        public void ExecuteLookCommand(Vector2 direction)
        {
            var actor = GetTargetActor();
            if (actor != null)
            {
                actor.SendLookInput(direction);
                if (logCommands)
                    Debug.Log($"[GameKitUICommand] Look: {direction}");
            }
        }

        /// <summary>
        /// Set target actor by reference.
        /// </summary>
        public void SetTargetActor(GameKitActor actor)
        {
            targetActor = actor;
            if (actor != null)
            {
                targetActorId = actor.ActorId;
            }
        }

        /// <summary>
        /// Set target actor by ID.
        /// </summary>
        public void SetTargetActor(string actorId)
        {
            targetActorId = actorId;
            if (cacheActorReference)
            {
                targetActor = FindActorById(actorId);
            }
            else
            {
                targetActor = null; // Force lookup on next command
            }
        }

        /// <summary>
        /// Clear all command bindings.
        /// </summary>
        public void ClearBindings()
        {
            // Remove all button listeners
            foreach (var binding in commandBindings)
            {
                if (binding.button != null)
                {
                    binding.button.onClick.RemoveAllListeners();
                }
            }

            commandBindings.Clear();
            bindingLookup?.Clear();
        }

        private GameKitActor GetTargetActor()
        {
            // Use cached reference if available
            if (targetActor != null)
                return targetActor;

            // Find by ID if not cached
            if (!string.IsNullOrEmpty(targetActorId))
            {
                targetActor = FindActorById(targetActorId);
                return targetActor;
            }

            return null;
        }

        private GameKitActor FindActorById(string actorId)
        {
            if (string.IsNullOrEmpty(actorId))
                return null;

            var actors = FindObjectsByType<GameKitActor>(FindObjectsSortMode.None);
            foreach (var actor in actors)
            {
                if (actor.ActorId == actorId)
                {
                    return actor;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all registered command names.
        /// </summary>
        public List<string> GetCommandNames()
        {
            var names = new List<string>();
            foreach (var binding in commandBindings)
            {
                if (!string.IsNullOrEmpty(binding.commandName))
                {
                    names.Add(binding.commandName);
                }
            }
            return names;
        }

        /// <summary>
        /// Check if a command is registered.
        /// </summary>
        public bool HasCommand(string commandName)
        {
            if (!isInitialized)
                Initialize();
            return bindingLookup.ContainsKey(commandName);
        }

        [Serializable]
        public class UICommandBinding
        {
            [Tooltip("Unique command identifier")]
            public string commandName;
            
            [Tooltip("Type of command (maps to GameKitActor events)")]
            public CommandType commandType = CommandType.Action;
            
            [Tooltip("UI button that triggers this command")]
            public Button button;
            
            [Tooltip("Direction for Move commands")]
            public Vector3 moveDirection = Vector3.zero;
            
            [Tooltip("Direction for Look commands")]
            public Vector2 lookDirection = Vector2.zero;
            
            [Tooltip("Parameter string for Action commands")]
            public string commandParameter;
        }
    }
}

