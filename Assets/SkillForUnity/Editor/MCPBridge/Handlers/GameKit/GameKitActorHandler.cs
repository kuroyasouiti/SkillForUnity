using System;
using System.Collections.Generic;
using MCP.Editor.Base;
using SkillForUnity.GameKit;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MCP.Editor.Handlers.GameKit
{
    /// <summary>
    /// GameKit Actor handler: create and manage game actors with behavior profiles and control modes.
    /// </summary>
    public class GameKitActorHandler : BaseCommandHandler
    {
        private static readonly string[] Operations = { "create", "update", "inspect", "delete" };

        public override string Category => "gamekitActor";

        public override IEnumerable<string> SupportedOperations => Operations;

        protected override bool RequiresCompilationWait(string operation) => false;

        protected override object ExecuteOperation(string operation, Dictionary<string, object> payload)
        {
            return operation switch
            {
                "create" => CreateActor(payload),
                "update" => UpdateActor(payload),
                "inspect" => InspectActor(payload),
                "delete" => DeleteActor(payload),
                _ => throw new InvalidOperationException($"Unsupported GameKit Actor operation: {operation}"),
            };
        }

        #region Create Actor

        private object CreateActor(Dictionary<string, object> payload)
        {
            var actorId = GetString(payload, "actorId") ?? $"Actor_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var parentPath = GetString(payload, "parentPath");
            var behaviorStr = GetString(payload, "behaviorProfile") ?? "2dLinear";
            var controlStr = GetString(payload, "controlMode") ?? "directController";

            // Create GameObject
            var actorGo = new GameObject(actorId);
            Undo.RegisterCreatedObjectUndo(actorGo, "Create GameKit Actor");

            if (!string.IsNullOrEmpty(parentPath))
            {
                var parent = ResolveGameObject(parentPath);
                actorGo.transform.SetParent(parent.transform, false);
            }

            // Set position
            if (payload.TryGetValue("position", out var posObj) && posObj is Dictionary<string, object> posDict)
            {
                actorGo.transform.position = GetVector3FromDict(posDict, Vector3.zero);
            }

            // Add GameKitActor component
            var actor = Undo.AddComponent<GameKitActor>(actorGo);
            var behavior = ParseBehaviorProfile(behaviorStr);
            var control = ParseControlMode(controlStr);
            actor.Initialize(actorId, behavior, control);

            // Apply behavior-specific components
            ApplyBehaviorComponents(actorGo, behavior);

            // Apply control-specific components
            ApplyControlComponents(actorGo, control);

            // Set stats
            if (payload.TryGetValue("stats", out var statsObj) && statsObj is Dictionary<string, object> statsDict)
            {
                foreach (var kvp in statsDict)
                {
                    actor.SetStat(kvp.Key, Convert.ToSingle(kvp.Value));
                }
            }

            // Add abilities
            if (payload.TryGetValue("abilities", out var abilitiesObj) && abilitiesObj is List<object> abilitiesList)
            {
                foreach (var ability in abilitiesList)
                {
                    actor.AddAbility(ability.ToString());
                }
            }

            // Add weapons
            if (payload.TryGetValue("weaponLoadout", out var weaponsObj) && weaponsObj is List<object> weaponsList)
            {
                foreach (var weapon in weaponsList)
                {
                    actor.AddWeapon(weapon.ToString());
                }
            }

            // Load sprite or model
            LoadVisuals(actorGo, payload, behavior);

            EditorSceneManager.MarkSceneDirty(actorGo.scene);
            return CreateSuccessResponse(("actorId", actorId), ("path", BuildGameObjectPath(actorGo)));
        }

        private void ApplyBehaviorComponents(GameObject go, GameKitActor.BehaviorProfile behavior)
        {
            switch (behavior)
            {
                case GameKitActor.BehaviorProfile.TwoDPhysics:
                    Undo.AddComponent<Rigidbody2D>(go);
                    Undo.AddComponent<BoxCollider2D>(go);
                    break;

                case GameKitActor.BehaviorProfile.TwoDTileGrid:
                    // Add custom tile grid movement component (placeholder)
                    break;

                case GameKitActor.BehaviorProfile.ThreeDCharacterController:
                    Undo.AddComponent<CharacterController>(go);
                    break;

                case GameKitActor.BehaviorProfile.ThreeDPhysics:
                    Undo.AddComponent<Rigidbody>(go);
                    Undo.AddComponent<CapsuleCollider>(go);
                    break;

                case GameKitActor.BehaviorProfile.ThreeDNavMesh:
                    var navAgent = Undo.AddComponent<UnityEngine.AI.NavMeshAgent>(go);
                    break;
            }
        }

        private void ApplyControlComponents(GameObject go, GameKitActor.ControlMode control)
        {
            switch (control)
            {
                case GameKitActor.ControlMode.DirectController:
                    // Add input handling component (placeholder)
                    break;

                case GameKitActor.ControlMode.AIAutonomous:
                    // Add AI component (placeholder)
                    break;

                case GameKitActor.ControlMode.UICommand:
                    // Add command receiver component (placeholder)
                    break;
            }
        }

        private void LoadVisuals(GameObject go, Dictionary<string, object> payload, GameKitActor.BehaviorProfile behavior)
        {
            if (behavior == GameKitActor.BehaviorProfile.TwoDLinear ||
                behavior == GameKitActor.BehaviorProfile.TwoDPhysics ||
                behavior == GameKitActor.BehaviorProfile.TwoDTileGrid)
            {
                // 2D sprite
                var spritePath = GetString(payload, "spritePath");
                if (!string.IsNullOrEmpty(spritePath))
                {
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                    if (sprite != null)
                    {
                        var spriteRenderer = Undo.AddComponent<SpriteRenderer>(go);
                        spriteRenderer.sprite = sprite;
                    }
                }
                else
                {
                    // Add default sprite renderer
                    Undo.AddComponent<SpriteRenderer>(go);
                }
            }
            else
            {
                // 3D model
                var modelPath = GetString(payload, "modelPath");
                if (!string.IsNullOrEmpty(modelPath))
                {
                    var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                    if (model != null)
                    {
                        var instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
                        if (instance != null)
                        {
                            Undo.RegisterCreatedObjectUndo(instance, "Instantiate Model");
                            instance.transform.SetParent(go.transform, false);
                        }
                    }
                }
            }
        }

        #endregion

        #region Update Actor

        private object UpdateActor(Dictionary<string, object> payload)
        {
            var actorId = GetString(payload, "actorId");
            if (string.IsNullOrEmpty(actorId))
            {
                throw new InvalidOperationException("actorId is required for update.");
            }

            var actor = FindActorById(actorId);
            if (actor == null)
            {
                throw new InvalidOperationException($"Actor with ID '{actorId}' not found.");
            }

            Undo.RecordObject(actor, "Update GameKit Actor");

            // Update stats
            if (payload.TryGetValue("stats", out var statsObj) && statsObj is Dictionary<string, object> statsDict)
            {
                foreach (var kvp in statsDict)
                {
                    actor.SetStat(kvp.Key, Convert.ToSingle(kvp.Value));
                }
            }

            // Update abilities
            if (payload.TryGetValue("abilities", out var abilitiesObj) && abilitiesObj is List<object> abilitiesList)
            {
                foreach (var ability in abilitiesList)
                {
                    actor.AddAbility(ability.ToString());
                }
            }

            // Update weapons
            if (payload.TryGetValue("weaponLoadout", out var weaponsObj) && weaponsObj is List<object> weaponsList)
            {
                foreach (var weapon in weaponsList)
                {
                    actor.AddWeapon(weapon.ToString());
                }
            }

            EditorSceneManager.MarkSceneDirty(actor.gameObject.scene);
            return CreateSuccessResponse(("actorId", actorId), ("path", BuildGameObjectPath(actor.gameObject)));
        }

        #endregion

        #region Inspect Actor

        private object InspectActor(Dictionary<string, object> payload)
        {
            var actorId = GetString(payload, "actorId");
            if (string.IsNullOrEmpty(actorId))
            {
                throw new InvalidOperationException("actorId is required for inspect.");
            }

            var actor = FindActorById(actorId);
            if (actor == null)
            {
                return CreateSuccessResponse(("found", false), ("actorId", actorId));
            }

            var info = new Dictionary<string, object>
            {
                { "found", true },
                { "actorId", actor.ActorId },
                { "path", BuildGameObjectPath(actor.gameObject) },
                { "behaviorProfile", actor.Behavior.ToString() },
                { "controlMode", actor.Control.ToString() },
                { "stats", actor.GetAllStats() }
            };

            return CreateSuccessResponse(("actor", info));
        }

        #endregion

        #region Delete Actor

        private object DeleteActor(Dictionary<string, object> payload)
        {
            var actorId = GetString(payload, "actorId");
            if (string.IsNullOrEmpty(actorId))
            {
                throw new InvalidOperationException("actorId is required for delete.");
            }

            var actor = FindActorById(actorId);
            if (actor == null)
            {
                throw new InvalidOperationException($"Actor with ID '{actorId}' not found.");
            }

            var scene = actor.gameObject.scene;
            Undo.DestroyObjectImmediate(actor.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);

            return CreateSuccessResponse(("actorId", actorId), ("deleted", true));
        }

        #endregion

        #region Helpers

        private GameKitActor FindActorById(string actorId)
        {
            var actors = UnityEngine.Object.FindObjectsByType<GameKitActor>(FindObjectsSortMode.None);
            foreach (var actor in actors)
            {
                if (actor.ActorId == actorId)
                {
                    return actor;
                }
            }
            return null;
        }

        private GameKitActor.BehaviorProfile ParseBehaviorProfile(string str)
        {
            return str.ToLowerInvariant() switch
            {
                "2dlinear" => GameKitActor.BehaviorProfile.TwoDLinear,
                "2dphysics" => GameKitActor.BehaviorProfile.TwoDPhysics,
                "2dtilegrid" => GameKitActor.BehaviorProfile.TwoDTileGrid,
                "3dcharactercontroller" => GameKitActor.BehaviorProfile.ThreeDCharacterController,
                "3dphysics" => GameKitActor.BehaviorProfile.ThreeDPhysics,
                "3dnavmesh" => GameKitActor.BehaviorProfile.ThreeDNavMesh,
                _ => GameKitActor.BehaviorProfile.TwoDLinear
            };
        }

        private GameKitActor.ControlMode ParseControlMode(string str)
        {
            return str.ToLowerInvariant() switch
            {
                "directcontroller" => GameKitActor.ControlMode.DirectController,
                "aiautonomous" => GameKitActor.ControlMode.AIAutonomous,
                "uicommand" => GameKitActor.ControlMode.UICommand,
                "scripttriggeronly" => GameKitActor.ControlMode.ScriptTriggerOnly,
                _ => GameKitActor.ControlMode.DirectController
            };
        }

        private Vector3 GetVector3FromDict(Dictionary<string, object> dict, Vector3 fallback)
        {
            float x = dict.TryGetValue("x", out var xObj) ? Convert.ToSingle(xObj) : fallback.x;
            float y = dict.TryGetValue("y", out var yObj) ? Convert.ToSingle(yObj) : fallback.y;
            float z = dict.TryGetValue("z", out var zObj) ? Convert.ToSingle(zObj) : fallback.z;
            return new Vector3(x, y, z);
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

