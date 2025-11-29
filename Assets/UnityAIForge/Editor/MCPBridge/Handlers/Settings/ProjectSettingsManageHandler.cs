using System;
using System.Collections.Generic;
using MCP.Editor.Base;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace MCP.Editor.Handlers.Settings
{
    /// <summary>
    /// プロジェクト設定管理のコマンドハンドラー。
    /// Player, Quality, Time, Physics, Audio, Editor設定の読み書きをサポート。
    /// </summary>
    public class ProjectSettingsManageHandler : BaseCommandHandler
    {
        public override string Category => "projectSettingsManage";
        
        public override IEnumerable<string> SupportedOperations => new[]
        {
            "read",
            "write",
            "list",
        };
        
        public ProjectSettingsManageHandler() : base()
        {
        }
        
        protected override object ExecuteOperation(string operation, Dictionary<string, object> payload)
        {
            return operation switch
            {
                "read" => ReadProjectSettings(payload),
                "write" => WriteProjectSettings(payload),
                "list" => ListProjectSettings(payload),
                _ => throw new InvalidOperationException($"Unknown operation: {operation}")
            };
        }
        
        protected override bool RequiresCompilationWait(string operation)
        {
            // Read and list operations don't require compilation wait
            return operation != "read" && operation != "list";
        }
        
        #region Main Operations
        
        private object ReadProjectSettings(Dictionary<string, object> payload)
        {
            var category = GetString(payload, "category");
            if (string.IsNullOrEmpty(category))
            {
                throw new InvalidOperationException("category is required");
            }
            
            var property = GetString(payload, "property");
            
            var result = new Dictionary<string, object>
            {
                ["category"] = category,
            };
            
            switch (category.ToLower())
            {
                case "player":
                    result["settings"] = ReadPlayerSettings(property);
                    break;
                case "quality":
                    result["settings"] = ReadQualitySettings(property);
                    break;
                case "time":
                    result["settings"] = ReadTimeSettings(property);
                    break;
                case "physics":
                    result["settings"] = ReadPhysicsSettings(property);
                    break;
                case "audio":
                    result["settings"] = ReadAudioSettings(property);
                    break;
                case "editor":
                    result["settings"] = ReadEditorSettings(property);
                    break;
                case "tagslayers":
                    result["settings"] = ReadTagsAndLayers(property);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown settings category: {category}");
            }
            
            return result;
        }
        
        private object WriteProjectSettings(Dictionary<string, object> payload)
        {
            var category = GetString(payload, "category");
            if (string.IsNullOrEmpty(category))
            {
                throw new InvalidOperationException("category is required");
            }
            
            var property = GetString(payload, "property");
            if (string.IsNullOrEmpty(property))
            {
                throw new InvalidOperationException("property is required");
            }
            
            if (!payload.TryGetValue("value", out var value))
            {
                throw new InvalidOperationException("value is required for write operation");
            }
            
            switch (category.ToLower())
            {
                case "player":
                    WritePlayerSettings(property, value);
                    break;
                case "quality":
                    WriteQualitySettings(property, value);
                    break;
                case "time":
                    WriteTimeSettings(property, value);
                    break;
                case "physics":
                    WritePhysicsSettings(property, value);
                    break;
                case "audio":
                    WriteAudioSettings(property, value);
                    break;
                case "editor":
                    WriteEditorSettings(property, value);
                    break;
                case "tagslayers":
                    WriteTagsAndLayers(property, value);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown settings category: {category}");
            }
            
            return new Dictionary<string, object>
            {
                ["category"] = category,
                ["property"] = property,
                ["value"] = value,
                ["message"] = "Settings updated successfully",
            };
        }
        
        private object ListProjectSettings(Dictionary<string, object> payload)
        {
            var category = GetString(payload, "category");
            
            if (string.IsNullOrEmpty(category))
            {
                // Return all available categories
                return new Dictionary<string, object>
                {
                    ["categories"] = new List<string>
                    {
                        "player",
                        "quality",
                        "time",
                        "physics",
                        "audio",
                        "editor",
                    },
                };
            }
            
            // Return available properties for the specified category
            var properties = category.ToLower() switch
            {
                "player" => new List<string>
                {
                    "companyName", "productName", "version", "bundleVersion",
                    "defaultScreenWidth", "defaultScreenHeight", "runInBackground",
                    "displayResolutionDialog", "defaultIsFullScreen", "defaultIsNativeResolution",
                    "allowFullscreenSwitch", "captureSingleScreen", "resizableWindow",
                },
                "quality" => new List<string>
                {
                    "names", "currentLevel", "pixelLightCount", "shadowDistance",
                    "shadowResolution", "shadowProjection", "shadowCascades", "vSyncCount",
                    "antiAliasing", "softParticles", "realtimeReflectionProbes",
                },
                "time" => new List<string>
                {
                    "fixedDeltaTime", "maximumDeltaTime", "timeScale", "maximumParticleDeltaTime",
                    "captureDeltaTime",
                },
                "physics" => new List<string>
                {
                    "gravity", "defaultSolverIterations", "defaultSolverVelocityIterations",
                    "bounceThreshold", "sleepThreshold", "defaultContactOffset",
                    "queriesHitTriggers", "queriesHitBackfaces", "autoSimulation",
                },
                "audio" => new List<string>
                {
                    "dspBufferSize", "sampleRate", "speakerMode", "numRealVoices",
                    "numVirtualVoices",
                },
                "editor" => new List<string>
                {
                    "serializationMode", "spritePackerMode", "etcTextureCompressorBehavior",
                    "lineEndingsForNewScripts", "defaultBehaviorMode", "prefabRegularEnvironment",
                },
                "tagslayers" => new List<string>
                {
                    "tags",
                    "layers",
                    "addTag",
                    "removeTag",
                    "addLayer",
                    "removeLayer",
                },
                _ => throw new InvalidOperationException($"Unknown settings category: {category}"),
            };
            
            return new Dictionary<string, object>
            {
                ["category"] = category,
                ["properties"] = properties,
            };
        }
        
        #endregion

        #region Tags & Layers

        private object ReadTagsAndLayers(string property)
        {
            var tags = InternalEditorUtility.tags;
            var layers = InternalEditorUtility.layers;

            if (string.IsNullOrEmpty(property))
            {
                return new Dictionary<string, object>
                {
                    ["tags"] = tags,
                    ["layers"] = layers,
                };
            }

            return property.ToLower() switch
            {
                "tags" => tags,
                "layers" => layers,
                _ => throw new InvalidOperationException($"Unknown tagsLayers property: {property}"),
            };
        }

        private void WriteTagsAndLayers(string property, object value)
        {
            var stringValue = value?.ToString();
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                throw new InvalidOperationException("A non-empty string value is required for tag/layer operations.");
            }

            switch (property.ToLower())
            {
                case "addtag":
                    AddTag(stringValue);
                    break;
                case "removetag":
                    RemoveTag(stringValue);
                    break;
                case "addlayer":
                    AddLayer(stringValue);
                    break;
                case "removelayer":
                    RemoveLayer(stringValue);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported tagsLayers property: {property}");
            }
        }

        private void AddTag(string tag)
        {
            if (Array.Exists(InternalEditorUtility.tags, existing => existing == tag))
            {
                throw new InvalidOperationException($"Tag '{tag}' already exists.");
            }

            var tagManager = GetTagManagerSerializedObject();
            var tagsProp = tagManager.FindProperty("tags");
            tagsProp.arraySize++;
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        private void RemoveTag(string tag)
        {
            var tagManager = GetTagManagerSerializedObject();
            var tagsProp = tagManager.FindProperty("tags");
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    tagsProp.DeleteArrayElementAtIndex(i);
                    tagManager.ApplyModifiedProperties();
                    AssetDatabase.SaveAssets();
                    return;
                }
            }

            throw new InvalidOperationException($"Tag '{tag}' does not exist.");
        }

        private void AddLayer(string layerName)
        {
            foreach (var layer in InternalEditorUtility.layers)
            {
                if (layer == layerName)
                {
                    throw new InvalidOperationException($"Layer '{layerName}' already exists.");
                }
            }

            var tagManager = GetTagManagerSerializedObject();
            var layersProp = tagManager.FindProperty("layers");
            for (int i = 8; i < layersProp.arraySize; i++)
            {
                var sp = layersProp.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(sp.stringValue))
                {
                    sp.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    AssetDatabase.SaveAssets();
                    return;
                }
            }

            throw new InvalidOperationException("Maximum number of layers reached. Remove an existing layer before adding a new one.");
        }

        private void RemoveLayer(string layerNameOrIndex)
        {
            var tagManager = GetTagManagerSerializedObject();
            var layersProp = tagManager.FindProperty("layers");

            bool FoundByName()
            {
                for (int i = 8; i < layersProp.arraySize; i++)
                {
                    var sp = layersProp.GetArrayElementAtIndex(i);
                    if (sp.stringValue == layerNameOrIndex)
                    {
                        sp.stringValue = string.Empty;
                        tagManager.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                        return true;
                    }
                }
                return false;
            }

            if (FoundByName())
            {
                return;
            }

            if (int.TryParse(layerNameOrIndex, out var index))
            {
                if (index < 8 || index >= layersProp.arraySize)
                {
                    throw new InvalidOperationException("Layer index must be between 8 and 31 (user layers).");
                }

                layersProp.GetArrayElementAtIndex(index).stringValue = string.Empty;
                tagManager.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                return;
            }

            throw new InvalidOperationException($"Layer '{layerNameOrIndex}' does not exist.");
        }

        private SerializedObject GetTagManagerSerializedObject()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                throw new InvalidOperationException("Unable to load TagManager asset.");
            }

            return new SerializedObject(assets[0]);
        }

        #endregion
        
        #region PlayerSettings Read/Write
        
        private object ReadPlayerSettings(string property)
        {
            if (string.IsNullOrEmpty(property))
            {
                return new Dictionary<string, object>
                {
                    ["companyName"] = PlayerSettings.companyName,
                    ["productName"] = PlayerSettings.productName,
                    ["version"] = PlayerSettings.bundleVersion,
                    ["defaultScreenWidth"] = PlayerSettings.defaultScreenWidth,
                    ["defaultScreenHeight"] = PlayerSettings.defaultScreenHeight,
                    ["runInBackground"] = PlayerSettings.runInBackground,
                };
            }
            
            return property.ToLower() switch
            {
                "companyname" => PlayerSettings.companyName,
                "productname" => PlayerSettings.productName,
                "version" or "bundleversion" => PlayerSettings.bundleVersion,
                "defaultscreenwidth" => PlayerSettings.defaultScreenWidth,
                "defaultscreenheight" => PlayerSettings.defaultScreenHeight,
                "runinbackground" => PlayerSettings.runInBackground,
                "fullscreenmode" => PlayerSettings.fullScreenMode.ToString(),
                "defaultisnativeresolution" => PlayerSettings.defaultIsNativeResolution,
                "allowfullscreenswitch" => PlayerSettings.allowFullscreenSwitch,
                "resizablewindow" => PlayerSettings.resizableWindow,
                _ => throw new InvalidOperationException($"Unknown PlayerSettings property: {property}"),
            };
        }
        
        private void WritePlayerSettings(string property, object value)
        {
            switch (property.ToLower())
            {
                case "companyname":
                    PlayerSettings.companyName = value.ToString();
                    break;
                case "productname":
                    PlayerSettings.productName = value.ToString();
                    break;
                case "version":
                case "bundleversion":
                    PlayerSettings.bundleVersion = value.ToString();
                    break;
                case "defaultscreenwidth":
                    PlayerSettings.defaultScreenWidth = Convert.ToInt32(value);
                    break;
                case "defaultscreenheight":
                    PlayerSettings.defaultScreenHeight = Convert.ToInt32(value);
                    break;
                case "runinbackground":
                    PlayerSettings.runInBackground = Convert.ToBoolean(value);
                    break;
                case "fullscreenmode":
                    if (Enum.TryParse<FullScreenMode>(value.ToString(), true, out var mode))
                    {
                        PlayerSettings.fullScreenMode = mode;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid FullScreenMode value: {value}");
                    }
                    break;
                case "defaultisnativeresolution":
                    PlayerSettings.defaultIsNativeResolution = Convert.ToBoolean(value);
                    break;
                case "allowfullscreenswitch":
                    PlayerSettings.allowFullscreenSwitch = Convert.ToBoolean(value);
                    break;
                case "resizablewindow":
                    PlayerSettings.resizableWindow = Convert.ToBoolean(value);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown or readonly PlayerSettings property: {property}");
            }
        }
        
        #endregion
        
        #region QualitySettings Read/Write
        
        private object ReadQualitySettings(string property)
        {
            if (string.IsNullOrEmpty(property))
            {
                return new Dictionary<string, object>
                {
                    ["names"] = QualitySettings.names,
                    ["currentLevel"] = QualitySettings.GetQualityLevel(),
                    ["pixelLightCount"] = QualitySettings.pixelLightCount,
                    ["shadowDistance"] = QualitySettings.shadowDistance,
                    ["vSyncCount"] = QualitySettings.vSyncCount,
                    ["antiAliasing"] = QualitySettings.antiAliasing,
                };
            }
            
            return property.ToLower() switch
            {
                "names" => QualitySettings.names,
                "currentlevel" => QualitySettings.GetQualityLevel(),
                "pixellightcount" => QualitySettings.pixelLightCount,
                "shadowdistance" => QualitySettings.shadowDistance,
                "shadowresolution" => QualitySettings.shadowResolution.ToString(),
                "shadowprojection" => QualitySettings.shadowProjection.ToString(),
                "shadowcascades" => QualitySettings.shadowCascades,
                "vsynccount" => QualitySettings.vSyncCount,
                "antialiasing" => QualitySettings.antiAliasing,
                "softparticles" => QualitySettings.softParticles,
                "realtimereflectionprobes" => QualitySettings.realtimeReflectionProbes,
                _ => throw new InvalidOperationException($"Unknown QualitySettings property: {property}"),
            };
        }
        
        private void WriteQualitySettings(string property, object value)
        {
            switch (property.ToLower())
            {
                case "currentlevel":
                    QualitySettings.SetQualityLevel(Convert.ToInt32(value));
                    break;
                case "pixellightcount":
                    QualitySettings.pixelLightCount = Convert.ToInt32(value);
                    break;
                case "shadowdistance":
                    QualitySettings.shadowDistance = Convert.ToSingle(value);
                    break;
                case "shadowcascades":
                    QualitySettings.shadowCascades = Convert.ToInt32(value);
                    break;
                case "vsynccount":
                    QualitySettings.vSyncCount = Convert.ToInt32(value);
                    break;
                case "antialiasing":
                    QualitySettings.antiAliasing = Convert.ToInt32(value);
                    break;
                case "softparticles":
                    QualitySettings.softParticles = Convert.ToBoolean(value);
                    break;
                case "realtimereflectionprobes":
                    QualitySettings.realtimeReflectionProbes = Convert.ToBoolean(value);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown or readonly QualitySettings property: {property}");
            }
        }
        
        #endregion
        
        #region TimeSettings Read/Write
        
        private object ReadTimeSettings(string property)
        {
            if (string.IsNullOrEmpty(property))
            {
                return new Dictionary<string, object>
                {
                    ["fixedDeltaTime"] = Time.fixedDeltaTime,
                    ["maximumDeltaTime"] = Time.maximumDeltaTime,
                    ["timeScale"] = Time.timeScale,
                    ["maximumParticleDeltaTime"] = Time.maximumParticleDeltaTime,
                };
            }
            
            return property.ToLower() switch
            {
                "fixeddeltatime" => Time.fixedDeltaTime,
                "maximumdeltatime" => Time.maximumDeltaTime,
                "timescale" => Time.timeScale,
                "maximumparticledeltatime" => Time.maximumParticleDeltaTime,
                "capturedeltatime" => Time.captureDeltaTime,
                _ => throw new InvalidOperationException($"Unknown TimeSettings property: {property}"),
            };
        }
        
        private void WriteTimeSettings(string property, object value)
        {
            switch (property.ToLower())
            {
                case "fixeddeltatime":
                    Time.fixedDeltaTime = Convert.ToSingle(value);
                    break;
                case "maximumdeltatime":
                    Time.maximumDeltaTime = Convert.ToSingle(value);
                    break;
                case "timescale":
                    Time.timeScale = Convert.ToSingle(value);
                    break;
                case "maximumparticledeltatime":
                    Time.maximumParticleDeltaTime = Convert.ToSingle(value);
                    break;
                case "capturedeltatime":
                    Time.captureDeltaTime = Convert.ToSingle(value);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown TimeSettings property: {property}");
            }
        }
        
        #endregion
        
        #region PhysicsSettings Read/Write
        
        private object ReadPhysicsSettings(string property)
        {
            if (string.IsNullOrEmpty(property))
            {
                return new Dictionary<string, object>
                {
                    ["gravity"] = new Dictionary<string, object>
                    {
                        ["x"] = Physics.gravity.x,
                        ["y"] = Physics.gravity.y,
                        ["z"] = Physics.gravity.z,
                    },
                    ["defaultSolverIterations"] = Physics.defaultSolverIterations,
                    ["defaultSolverVelocityIterations"] = Physics.defaultSolverVelocityIterations,
                    ["bounceThreshold"] = Physics.bounceThreshold,
                    ["sleepThreshold"] = Physics.sleepThreshold,
                    ["queriesHitTriggers"] = Physics.queriesHitTriggers,
                };
            }
            
            return property.ToLower() switch
            {
                "gravity" => new Dictionary<string, object>
                {
                    ["x"] = Physics.gravity.x,
                    ["y"] = Physics.gravity.y,
                    ["z"] = Physics.gravity.z,
                },
                "defaultsolveriterations" => Physics.defaultSolverIterations,
                "defaultsolvervelocityiterations" => Physics.defaultSolverVelocityIterations,
                "bouncethreshold" => Physics.bounceThreshold,
                "sleepthreshold" => Physics.sleepThreshold,
                "defaultcontactoffset" => Physics.defaultContactOffset,
                "querieshittriggers" => Physics.queriesHitTriggers,
                "querieshitbackfaces" => Physics.queriesHitBackfaces,
                "autosimulation" => Physics.autoSimulation,
                _ => throw new InvalidOperationException($"Unknown PhysicsSettings property: {property}"),
            };
        }
        
        private void WritePhysicsSettings(string property, object value)
        {
            switch (property.ToLower())
            {
                case "gravity":
                    if (value is Dictionary<string, object> gravityDict)
                    {
                        var x = gravityDict.ContainsKey("x") ? Convert.ToSingle(gravityDict["x"]) : Physics.gravity.x;
                        var y = gravityDict.ContainsKey("y") ? Convert.ToSingle(gravityDict["y"]) : Physics.gravity.y;
                        var z = gravityDict.ContainsKey("z") ? Convert.ToSingle(gravityDict["z"]) : Physics.gravity.z;
                        Physics.gravity = new Vector3(x, y, z);
                    }
                    break;
                case "defaultsolveriterations":
                    Physics.defaultSolverIterations = Convert.ToInt32(value);
                    break;
                case "defaultsolvervelocityiterations":
                    Physics.defaultSolverVelocityIterations = Convert.ToInt32(value);
                    break;
                case "bouncethreshold":
                    Physics.bounceThreshold = Convert.ToSingle(value);
                    break;
                case "sleepthreshold":
                    Physics.sleepThreshold = Convert.ToSingle(value);
                    break;
                case "defaultcontactoffset":
                    Physics.defaultContactOffset = Convert.ToSingle(value);
                    break;
                case "querieshittriggers":
                    Physics.queriesHitTriggers = Convert.ToBoolean(value);
                    break;
                case "querieshitbackfaces":
                    Physics.queriesHitBackfaces = Convert.ToBoolean(value);
                    break;
                case "autosimulation":
                    Physics.autoSimulation = Convert.ToBoolean(value);
                    break;
                case "simulationmode":
                    if (Enum.TryParse<SimulationMode>(value.ToString(), true, out var simMode))
                    {
                        Physics.simulationMode = simMode;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid SimulationMode value: {value}");
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unknown PhysicsSettings property: {property}");
            }
        }
        
        #endregion
        
        #region AudioSettings Read/Write
        
        private object ReadAudioSettings(string property)
        {
            if (string.IsNullOrEmpty(property))
            {
                var config = AudioSettings.GetConfiguration();
                return new Dictionary<string, object>
                {
                    ["dspBufferSize"] = config.dspBufferSize,
                    ["sampleRate"] = config.sampleRate,
                    ["speakerMode"] = config.speakerMode.ToString(),
                    ["numRealVoices"] = config.numRealVoices,
                    ["numVirtualVoices"] = config.numVirtualVoices,
                };
            }
            
            var audioConfig = AudioSettings.GetConfiguration();
            return property.ToLower() switch
            {
                "dspbuffersize" => audioConfig.dspBufferSize,
                "samplerate" => audioConfig.sampleRate,
                "speakermode" => audioConfig.speakerMode.ToString(),
                "numrealvoices" => audioConfig.numRealVoices,
                "numvirtualvoices" => audioConfig.numVirtualVoices,
                _ => throw new InvalidOperationException($"Unknown AudioSettings property: {property}"),
            };
        }
        
        private void WriteAudioSettings(string property, object value)
        {
            var config = AudioSettings.GetConfiguration();
            var modified = false;
            
            switch (property.ToLower())
            {
                case "dspbuffersize":
                    config.dspBufferSize = Convert.ToInt32(value);
                    modified = true;
                    break;
                case "samplerate":
                    config.sampleRate = Convert.ToInt32(value);
                    modified = true;
                    break;
                case "speakermode":
                    if (Enum.TryParse<AudioSpeakerMode>(value.ToString(), out var mode))
                    {
                        config.speakerMode = mode;
                        modified = true;
                    }
                    break;
                case "numrealvoices":
                    config.numRealVoices = Convert.ToInt32(value);
                    modified = true;
                    break;
                case "numvirtualvoices":
                    config.numVirtualVoices = Convert.ToInt32(value);
                    modified = true;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown AudioSettings property: {property}");
            }
            
            if (modified)
            {
                AudioSettings.Reset(config);
            }
        }
        
        #endregion
        
        #region EditorSettings Read/Write
        
        private object ReadEditorSettings(string property)
        {
            if (string.IsNullOrEmpty(property))
            {
                return new Dictionary<string, object>
                {
                    ["serializationMode"] = EditorSettings.serializationMode.ToString(),
                    ["spritePackerMode"] = EditorSettings.spritePackerMode.ToString(),
                    ["lineEndingsForNewScripts"] = EditorSettings.lineEndingsForNewScripts.ToString(),
                    ["defaultBehaviorMode"] = EditorSettings.defaultBehaviorMode.ToString(),
                };
            }
            
            return property.ToLower() switch
            {
                "serializationmode" => EditorSettings.serializationMode.ToString(),
                "spritepackermode" => EditorSettings.spritePackerMode.ToString(),
                "lineendingsfornewscripts" => EditorSettings.lineEndingsForNewScripts.ToString(),
                "defaultbehaviormode" => EditorSettings.defaultBehaviorMode.ToString(),
                "prefabregularenvironment" => EditorSettings.prefabRegularEnvironment?.name,
                _ => throw new InvalidOperationException($"Unknown EditorSettings property: {property}"),
            };
        }
        
        private void WriteEditorSettings(string property, object value)
        {
            switch (property.ToLower())
            {
                case "serializationmode":
                    if (Enum.TryParse<SerializationMode>(value.ToString(), out var serMode))
                    {
                        EditorSettings.serializationMode = serMode;
                    }
                    break;
                case "spritepackermode":
                    if (Enum.TryParse<SpritePackerMode>(value.ToString(), out var spriteMode))
                    {
                        EditorSettings.spritePackerMode = spriteMode;
                    }
                    break;
                case "lineendingsfornewscripts":
                    if (Enum.TryParse<LineEndingsMode>(value.ToString(), out var lineMode))
                    {
                        EditorSettings.lineEndingsForNewScripts = lineMode;
                    }
                    break;
                case "defaultbehaviormode":
                    if (Enum.TryParse<EditorBehaviorMode>(value.ToString(), out var behaviorMode))
                    {
                        EditorSettings.defaultBehaviorMode = behaviorMode;
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unknown or readonly EditorSettings property: {property}");
            }
        }
        
        #endregion
    }
}

