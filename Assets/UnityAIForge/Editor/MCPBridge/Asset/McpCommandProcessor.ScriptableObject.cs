using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MCP.Editor
{
    internal static partial class McpCommandProcessor
    {
        #region ScriptableObject Management

        private static object HandleScriptableObjectManage(Dictionary<string, object> payload)
        {
            var operation = EnsureValue(GetString(payload, "operation"), "operation");

            // Check if compilation is in progress and wait if necessary (except for read-only operations)
            Dictionary<string, object> compilationWaitInfo = null;
            if (operation != "inspect" && operation != "list" && operation != "findByType")
            {
                compilationWaitInfo = EnsureNoCompilationInProgress("scriptableObjectManage", maxWaitSeconds: 30f);
            }

            var result = operation switch
            {
                "create" => CreateScriptableObject(payload),
                "inspect" => InspectScriptableObject(payload),
                "update" => UpdateScriptableObject(payload),
                "delete" => DeleteScriptableObject(payload),
                "duplicate" => DuplicateScriptableObject(payload),
                "list" => ListScriptableObjects(payload),
                "findByType" => FindScriptableObjectsByType(payload),
                _ => throw new InvalidOperationException($"Unknown scriptableObjectManage operation: {operation}"),
            };

            // Add compilation wait info if we waited
            if (compilationWaitInfo != null && result is Dictionary<string, object> resultDict)
            {
                resultDict["compilationWait"] = compilationWaitInfo;
            }

            return result;
        }

        private static object CreateScriptableObject(Dictionary<string, object> payload)
        {
            var typeName = EnsureValue(GetString(payload, "typeName"), "typeName");
            var assetPath = EnsureValue(GetString(payload, "assetPath"), "assetPath");

            // Validate asset path
            ValidateAssetPath(assetPath, "Assets/", ".asset");

            // Ensure parent directory exists
            EnsureDirectoryExists(assetPath);

            // Check if file already exists
            if (File.Exists(assetPath))
            {
                throw new InvalidOperationException($"Asset already exists: {assetPath}");
            }

            // Find and validate the type
            var type = ResolveType(typeName);

            // Verify it's a ScriptableObject type
            if (!typeof(ScriptableObject).IsAssignableFrom(type))
            {
                throw new InvalidOperationException(
                    $"Type {typeName} is not a ScriptableObject. " +
                    $"Found type: {type.FullName}"
                );
            }

            // Check if type is abstract
            if (type.IsAbstract)
            {
                throw new InvalidOperationException(
                    $"Cannot create instance of abstract type {typeName}. " +
                    $"Use a concrete derived type instead."
                );
            }

            // Create the ScriptableObject instance
            var instance = ScriptableObject.CreateInstance(type);
            if (instance == null)
            {
                throw new InvalidOperationException($"Failed to create instance of {typeName}");
            }

            // Apply initial property values if provided with improved error handling
            var result = new Dictionary<string, object>
            {
                ["assetPath"] = assetPath,
                ["typeName"] = type.FullName,
            };

            if (payload.TryGetValue("properties", out var propertiesObj) && propertiesObj is Dictionary<string, object> properties)
            {
                var appliedProperties = new List<string>();
                var failedProperties = new Dictionary<string, string>();

                foreach (var kvp in properties)
                {
                    try
                    {
                        ApplyPropertyToObject(instance, kvp.Key, kvp.Value);
                        appliedProperties.Add(kvp.Key);
                    }
                    catch (Exception ex)
                    {
                        failedProperties[kvp.Key] = ex.Message;
                    }
                }

                if (appliedProperties.Count > 0)
                {
                    result["appliedProperties"] = appliedProperties;
                }

                if (failedProperties.Count > 0)
                {
                    result["failedProperties"] = failedProperties;
                    result["warning"] = $"{failedProperties.Count} properties failed to apply";
                }
            }

            // Create the asset
            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            result["guid"] = guid;
            result["message"] = $"ScriptableObject created successfully at {assetPath}";

            return result;
        }

        private static object InspectScriptableObject(Dictionary<string, object> payload)
        {
            // Resolve asset path (supports both path and GUID)
            var assetPath = ResolveAssetPath(
                GetString(payload, "assetPath"),
                GetString(payload, "assetGuid")
            );

            // Load the ScriptableObject
            var scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (scriptableObject == null)
            {
                throw new InvalidOperationException($"ScriptableObject not found at {assetPath}");
            }

            var type = scriptableObject.GetType();
            var guid = AssetDatabase.AssetPathToGUID(assetPath);

            var result = new Dictionary<string, object>
            {
                ["assetPath"] = assetPath,
                ["guid"] = guid,
                ["typeName"] = type.FullName,
                ["name"] = scriptableObject.name,
            };

            // Get properties
            var includeProperties = GetBool(payload, "includeProperties", true);
            if (includeProperties)
            {
                var propertyFilter = GetStringList(payload, "propertyFilter");
                var properties = SerializeObjectProperties(scriptableObject, propertyFilter);
                result["properties"] = properties;
            }

            return result;
        }

        private static object UpdateScriptableObject(Dictionary<string, object> payload)
        {
            // Resolve asset path (supports both path and GUID)
            var assetPath = ResolveAssetPath(
                GetString(payload, "assetPath"),
                GetString(payload, "assetGuid")
            );

            // Load the ScriptableObject
            var scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (scriptableObject == null)
            {
                throw new InvalidOperationException($"ScriptableObject not found at {assetPath}");
            }

            // Apply property changes with improved error handling
            if (!payload.TryGetValue("properties", out var propertiesObj) || !(propertiesObj is Dictionary<string, object> properties))
            {
                throw new InvalidOperationException("properties parameter is required for update operation");
            }

            var changedProperties = new List<string>();
            var failedProperties = new Dictionary<string, string>();

            foreach (var kvp in properties)
            {
                try
                {
                    ApplyPropertyToObject(scriptableObject, kvp.Key, kvp.Value);
                    changedProperties.Add(kvp.Key);
                }
                catch (Exception ex)
                {
                    failedProperties[kvp.Key] = ex.Message;
                }
            }

            // If all properties failed, throw an exception
            if (failedProperties.Count > 0 && changedProperties.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Failed to apply all properties: {string.Join(", ", failedProperties.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}"
                );
            }

            // Mark as dirty and save
            EditorUtility.SetDirty(scriptableObject);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var result = new Dictionary<string, object>
            {
                ["assetPath"] = assetPath,
                ["guid"] = AssetDatabase.AssetPathToGUID(assetPath),
                ["changedProperties"] = changedProperties,
                ["message"] = $"ScriptableObject updated successfully",
            };

            // Include failed properties if any
            if (failedProperties.Count > 0)
            {
                result["failedProperties"] = failedProperties;
                result["warning"] = $"{failedProperties.Count} properties failed to update";
            }

            return result;
        }

        private static object DeleteScriptableObject(Dictionary<string, object> payload)
        {
            // Resolve asset path (supports both path and GUID)
            var assetPath = ResolveAssetPath(
                GetString(payload, "assetPath"),
                GetString(payload, "assetGuid")
            );

            // Verify it's a ScriptableObject
            var scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (scriptableObject == null)
            {
                throw new InvalidOperationException($"ScriptableObject not found at {assetPath}");
            }

            // Delete the asset
            if (!AssetDatabase.DeleteAsset(assetPath))
            {
                throw new InvalidOperationException($"Failed to delete asset at {assetPath}");
            }

            AssetDatabase.Refresh();

            return new Dictionary<string, object>
            {
                ["assetPath"] = assetPath,
                ["message"] = "ScriptableObject deleted successfully",
            };
        }

        private static object DuplicateScriptableObject(Dictionary<string, object> payload)
        {
            // Resolve source asset path (supports both path and GUID)
            var sourceAssetPath = ResolveAssetPath(
                GetString(payload, "sourceAssetPath"),
                GetString(payload, "sourceAssetGuid"),
                "sourceAssetPath"
            );

            var destinationAssetPath = EnsureValue(GetString(payload, "destinationAssetPath"), "destinationAssetPath");

            // Validate destination path
            ValidateAssetPath(destinationAssetPath, "Assets/", ".asset");

            // Verify source is a ScriptableObject
            var sourceScriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(sourceAssetPath);
            if (sourceScriptableObject == null)
            {
                throw new InvalidOperationException($"ScriptableObject not found at {sourceAssetPath}");
            }

            // Ensure parent directory exists
            EnsureDirectoryExists(destinationAssetPath);

            // Check if destination already exists
            if (File.Exists(destinationAssetPath))
            {
                throw new InvalidOperationException($"Asset already exists at destination: {destinationAssetPath}");
            }

            // Copy the asset
            if (!AssetDatabase.CopyAsset(sourceAssetPath, destinationAssetPath))
            {
                throw new InvalidOperationException($"Failed to duplicate asset from {sourceAssetPath} to {destinationAssetPath}");
            }

            AssetDatabase.Refresh();

            var guid = AssetDatabase.AssetPathToGUID(destinationAssetPath);

            return new Dictionary<string, object>
            {
                ["sourceAssetPath"] = sourceAssetPath,
                ["destinationAssetPath"] = destinationAssetPath,
                ["guid"] = guid,
                ["message"] = "ScriptableObject duplicated successfully",
            };
        }

        private static object ListScriptableObjects(Dictionary<string, object> payload)
        {
            var searchPath = GetString(payload, "searchPath", "Assets");
            var typeName = GetString(payload, "typeName");
            var maxResults = GetInt(payload, "maxResults", 1000);  // デフォルト1000件
            var offset = GetInt(payload, "offset", 0);  // ページネーション用

            // Find all ScriptableObject assets
            var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { searchPath });
            
            var results = new List<Dictionary<string, object>>();
            var processedCount = 0;
            var skippedCount = 0;

            foreach (var guid in guids)
            {
                // Skip items before offset
                if (skippedCount < offset)
                {
                    skippedCount++;
                    continue;
                }

                // Stop if we've reached maxResults
                if (processedCount >= maxResults)
                {
                    break;
                }

                var path = AssetDatabase.GUIDToAssetPath(guid);
                var scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                
                if (scriptableObject == null)
                    continue;

                var type = scriptableObject.GetType();

                // Filter by type if specified
                if (!string.IsNullOrEmpty(typeName))
                {
                    if (type.FullName != typeName && type.Name != typeName)
                        continue;
                }

                results.Add(new Dictionary<string, object>
                {
                    ["assetPath"] = path,
                    ["guid"] = guid,
                    ["name"] = scriptableObject.name,
                    ["typeName"] = type.FullName,
                });

                processedCount++;
            }

            return new Dictionary<string, object>
            {
                ["count"] = results.Count,
                ["totalFound"] = guids.Length,
                ["offset"] = offset,
                ["maxResults"] = maxResults,
                ["hasMore"] = (offset + processedCount) < guids.Length,
                ["scriptableObjects"] = results,
                ["searchPath"] = searchPath,
            };
        }

        private static object FindScriptableObjectsByType(Dictionary<string, object> payload)
        {
            var typeName = EnsureValue(GetString(payload, "typeName"), "typeName");
            var searchPath = GetString(payload, "searchPath", "Assets");
            var includeProperties = GetBool(payload, "includeProperties", false);
            var maxResults = GetInt(payload, "maxResults", 1000);  // デフォルト1000件
            var offset = GetInt(payload, "offset", 0);  // ページネーション用

            // Find and validate the type
            var type = ResolveType(typeName);

            // Verify it's a ScriptableObject type
            if (!typeof(ScriptableObject).IsAssignableFrom(type))
            {
                throw new InvalidOperationException(
                    $"Type {typeName} is not a ScriptableObject. " +
                    $"Found type: {type.FullName}, " +
                    $"Base type: {type.BaseType?.FullName ?? "None"}"
                );
            }

            // Warn if type is abstract
            var isAbstract = type.IsAbstract;

            // Find all ScriptableObject assets
            var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { searchPath });
            
            var results = new List<Dictionary<string, object>>();
            var processedCount = 0;
            var skippedCount = 0;
            var totalMatched = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                
                if (scriptableObject == null)
                    continue;

                var objectType = scriptableObject.GetType();

                // Check if type matches (exact match or derived type)
                if (!type.IsAssignableFrom(objectType))
                    continue;

                totalMatched++;

                // Skip items before offset
                if (skippedCount < offset)
                {
                    skippedCount++;
                    continue;
                }

                // Stop if we've reached maxResults
                if (processedCount >= maxResults)
                {
                    continue;  // Still count total matches
                }

                var info = new Dictionary<string, object>
                {
                    ["assetPath"] = path,
                    ["guid"] = guid,
                    ["name"] = scriptableObject.name,
                    ["typeName"] = objectType.FullName,
                };

                if (includeProperties)
                {
                    var propertyFilter = GetStringList(payload, "propertyFilter");
                    var properties = SerializeObjectProperties(scriptableObject, propertyFilter);
                    info["properties"] = properties;
                }

                results.Add(info);
                processedCount++;
            }

            var result = new Dictionary<string, object>
            {
                ["count"] = results.Count,
                ["totalMatched"] = totalMatched,
                ["offset"] = offset,
                ["maxResults"] = maxResults,
                ["hasMore"] = (offset + processedCount) < totalMatched,
                ["typeName"] = type.FullName,
                ["scriptableObjects"] = results,
                ["searchPath"] = searchPath,
            };

            if (isAbstract)
            {
                result["note"] = $"Searching for abstract type {typeName}. Results include all derived types.";
            }

            return result;
        }

        #endregion
    }
}

