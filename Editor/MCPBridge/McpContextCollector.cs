using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MCP.Editor
{
    internal static class McpContextCollector
    {
        private const int MaxAssets = 200;
        private static readonly string[] AssetTypeFilters =
        {
            "Script",
            "Prefab",
            "Material",
            "Scene",
            "ScriptableObject",
            "Shader",
        };

        public static Dictionary<string, object> BuildContextPayload()
        {
            return new Dictionary<string, object>
            {
                ["activeScene"] = BuildActiveSceneInfo(),
                ["hierarchy"] = BuildHierarchyTree(),
                ["selection"] = BuildSelectionInfo(),
                ["assets"] = BuildAssetIndex(),
                ["gitDiffSummary"] = TryCaptureGitStatus(),
                ["updatedAt"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };
        }

        private static Dictionary<string, object> BuildActiveSceneInfo()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                return null;
            }

            return new Dictionary<string, object>
            {
                ["name"] = scene.name,
                ["path"] = scene.path,
            };
        }

        private static Dictionary<string, object> BuildHierarchyTree()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                return null;
            }

            var roots = scene.GetRootGameObjects();
            var rootChildren = roots
                .Select(go => BuildHierarchyNode(go, 0))
                .Where(node => node != null)
                .ToList();

            return new Dictionary<string, object>
            {
                ["id"] = "scene-root",
                ["name"] = scene.name,
                ["type"] = "Scene",
                ["children"] = rootChildren,
            };
        }

        private static Dictionary<string, object> BuildHierarchyNode(GameObject go, int depth)
        {
            if (go == null)
            {
                return null;
            }

            var components = go.GetComponents<Component>()
                .Where(component => component != null)
                .Select(component => new Dictionary<string, object>
                {
                    ["type"] = component.GetType().FullName,
                    ["enabled"] = component is Behaviour behaviour ? behaviour.enabled : (bool?)null,
                })
                .ToList();

            var children = new List<object>();
            for (var i = 0; i < go.transform.childCount; i++)
            {
                var child = go.transform.GetChild(i);
                var node = BuildHierarchyNode(child.gameObject, depth + 1);
                if (node != null)
                {
                    children.Add(node);
                }
            }

            return new Dictionary<string, object>
            {
                ["id"] = go.GetInstanceID().ToString(),
                ["name"] = go.name,
                ["type"] = ResolveHierarchyType(go),
                ["components"] = components,
                ["children"] = children,
            };
        }

        private static string ResolveHierarchyType(GameObject go)
        {
            if (go.GetComponent<RectTransform>() != null)
            {
                return "UIElement";
            }

            if (PrefabUtility.IsAnyPrefabInstanceRoot(go))
            {
                return "PrefabInstance";
            }

            return "GameObject";
        }

        private static List<object> BuildSelectionInfo()
        {
            return Selection.objects
                .Where(obj => obj != null)
                .Select(obj => new Dictionary<string, object>
                {
                    ["name"] = obj.name,
                    ["type"] = obj.GetType().FullName,
                    ["path"] = AssetDatabase.GetAssetPath(obj),
                    ["guid"] = string.IsNullOrEmpty(AssetDatabase.GetAssetPath(obj))
                        ? null
                        : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj)),
                })
                .Cast<object>()
                .ToList();
        }

        private static List<object> BuildAssetIndex()
        {
            var guids = new List<string>(MaxAssets);
            var seen = new HashSet<string>();

            foreach (var type in AssetTypeFilters)
            {
                foreach (var guid in AssetDatabase.FindAssets($"t:{type}"))
                {
                    if (!seen.Add(guid))
                    {
                        continue;
                    }

                    guids.Add(guid);

                    if (guids.Count >= MaxAssets)
                    {
                        break;
                    }
                }

                if (guids.Count >= MaxAssets)
                {
                    break;
                }
            }

            var results = new List<object>(guids.Count);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var type = AssetDatabase.GetMainAssetTypeAtPath(path);
                results.Add(new Dictionary<string, object>
                {
                    ["guid"] = guid,
                    ["path"] = path,
                    ["type"] = type != null ? type.FullName : null,
                });
            }

            return results;
        }

        private static string TryCaptureGitStatus()
        {
            try
            {
                var projectRoot = Directory.GetCurrentDirectory();
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "status --short",
                    WorkingDirectory = projectRoot,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var process = Process.Start(processStartInfo);
                if (process == null)
                {
                    return null;
                }

                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(1500);

                if (process.ExitCode != 0)
                {
                    return null;
                }

                var sb = new StringBuilder();
                foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine(line.Trim());
                }

                return sb.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
