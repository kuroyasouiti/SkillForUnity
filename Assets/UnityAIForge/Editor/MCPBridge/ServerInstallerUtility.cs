using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;

namespace MCP.Editor
{
    internal static class ServerInstallerUtility
    {
        private static string _skillZipPath;

        public static string SkillZipPath
        {
            get
            {
                if (_skillZipPath == null)
                {
                    _skillZipPath = FindSkillZipPath();
                }
                return _skillZipPath;
            }
        }

        private static string GetEmbeddedSkillZipPath()
        {
            // Find this script's location and look for Unity-AI-Forge.zip in the parent directory
            var guids = AssetDatabase.FindAssets("ServerInstallerUtility t:Script");
            if (guids.Length > 0)
            {
                var scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                var scriptDir = Path.GetDirectoryName(scriptPath);
                var parentDir = Path.GetDirectoryName(Path.GetDirectoryName(scriptDir)); // Go up to Assets/UnityAIForge
                var zipPath = Path.Combine(parentDir, "Unity-AI-Forge.zip");

                if (File.Exists(zipPath))
                {
                    return zipPath;
                }
            }

            // Fallback to hardcoded path (for backward compatibility)
            var assetsPath = Application.dataPath;
            if (string.IsNullOrEmpty(assetsPath))
            {
                return null;
            }

            return Path.Combine(assetsPath, "UnityAIForge", "Unity-AI-Forge.zip");
        }

        private static string FindSkillZipPath()
        {
            var embeddedZipPath = GetEmbeddedSkillZipPath();
            if (!string.IsNullOrEmpty(embeddedZipPath) && File.Exists(embeddedZipPath))
            {
                return embeddedZipPath;
            }

            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrEmpty(projectRoot))
            {
                return null;
            }

            // .claude/skills/Unity-AI-Forge.zip を探す
            var skillZipPath = Path.Combine(projectRoot, ".claude", "skills", "Unity-AI-Forge.zip");
            if (File.Exists(skillZipPath))
            {
                return skillZipPath;
            }

            // 見つからない場合、プロジェクトルートを検索
            var rootZipPath = Path.Combine(projectRoot, "Unity-AI-Forge.zip");
            if (File.Exists(rootZipPath))
            {
                return rootZipPath;
            }

            return null;
        }

        public static void ResetCache()
        {
            _skillZipPath = null;
        }

        public static string GetSkillZipInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine("Skill Package Information:");
            info.AppendLine($"Project Root: {Path.GetDirectoryName(Application.dataPath)}");
            info.AppendLine($"Found Skill Package: {SkillZipPath ?? "Not Found"}");

            if (!string.IsNullOrEmpty(SkillZipPath))
            {
                var fileInfo = new FileInfo(SkillZipPath);
                info.AppendLine($"File Size: {fileInfo.Length / 1024} KB");
                info.AppendLine($"Last Modified: {fileInfo.LastWriteTime}");
            }

            return info.ToString();
        }

        private static void CopyDirectoryRecursive(string sourceDir, string destDir)
        {
            // Create destination directory
            Directory.CreateDirectory(destDir);

            // Copy files
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(destDir, fileName);
                File.Copy(file, destFile, overwrite: true);
            }

            // Copy subdirectories
            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(dir);
                var destSubDir = Path.Combine(destDir, dirName);
                CopyDirectoryRecursive(dir, destSubDir);
            }
        }

        public static bool InstallSkillPackage(string destinationPath, out string message)
        {
            if (string.IsNullOrEmpty(SkillZipPath))
            {
                message = "Skill package (Unity-AI-Forge.zip) not found. Please build the skill package first.";
                return false;
            }

            if (!File.Exists(SkillZipPath))
            {
                message = $"Skill package file not found at: {SkillZipPath}";
                return false;
            }

            string tempExtractPath = null;

            try
            {
                // If destination exists, delete it first
                if (Directory.Exists(destinationPath))
                {
                    Directory.Delete(destinationPath, recursive: true);
                }

                // Ensure parent directory exists
                var parentDir = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
                {
                    Directory.CreateDirectory(parentDir);
                }

                // Extract to temporary directory first
                tempExtractPath = Path.Combine(Path.GetTempPath(), "Unity-AI-Forge_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempExtractPath);
                ZipFile.ExtractToDirectory(SkillZipPath, tempExtractPath);

                // Find the Unity-AI-Forge directory inside the extracted content
                var skillDir = Path.Combine(tempExtractPath, "Unity-AI-Forge");
                if (Directory.Exists(skillDir))
                {
                    // Copy the Unity-AI-Forge directory to the destination
                    CopyDirectoryRecursive(skillDir, destinationPath);
                }
                else
                {
                    // If Unity-AI-Forge subdirectory doesn't exist, copy the temp directory contents
                    CopyDirectoryRecursive(tempExtractPath, destinationPath);
                }

                // Create or update .mcp.json after successful installation
                CreateMcpJsonFile(destinationPath, out string mcpJsonMessage);

                message = $"Skill package extracted to: {destinationPath}\n{mcpJsonMessage}";
                return true;
            }
            catch (Exception ex)
            {
                message = $"Failed to install skill package: {ex.Message}";
                return false;
            }
            finally
            {
                // Clean up temporary directory
                if (!string.IsNullOrEmpty(tempExtractPath) && Directory.Exists(tempExtractPath))
                {
                    try
                    {
                        Directory.Delete(tempExtractPath, recursive: true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        public static bool HasPyProject(string path)
        {
            // For skill packages, we don't need pyproject.toml
            // Check if it's a directory with skill.yml or a zip file
            if (File.Exists(path) && path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (Directory.Exists(path))
            {
                var skillYml = Path.Combine(path, "skill.yml");
                return File.Exists(skillYml);
            }

            return false;
        }

        public static bool TryUninstall(string path, out string message, bool force = false)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                message = "Install path is empty.";
                return false;
            }

            string normalized;
            try
            {
                normalized = Path.GetFullPath(path.Trim());
            }
            catch (Exception ex)
            {
                message = $"Invalid install path: {ex.Message}";
                return false;
            }

            if (!File.Exists(normalized) && !Directory.Exists(normalized))
            {
                message = $"Install path not found: {normalized}";
                return false;
            }

            // Safety check: don't delete system directories
            var root = Path.GetPathRoot(normalized);
            if (!string.IsNullOrEmpty(root))
            {
                var trimmedPath = normalized.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var trimmedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (string.Equals(trimmedPath, trimmedRoot, StringComparison.OrdinalIgnoreCase))
                {
                    message = $"Refusing to delete root directory: {normalized}";
                    return false;
                }
            }

            try
            {
                if (File.Exists(normalized))
                {
                    File.Delete(normalized);
                    message = $"Removed skill package: {normalized}";
                }
                else if (Directory.Exists(normalized))
                {
                    FileUtil.DeleteFileOrDirectory(normalized);
                    message = $"Removed skill directory: {normalized}";
                }
                else
                {
                    message = $"Path not found: {normalized}";
                    return false;
                }

                // Remove from .mcp.json
                RemoveFromMcpJson(normalized, out string mcpJsonMessage);
                if (!string.IsNullOrEmpty(mcpJsonMessage))
                {
                    message += "\n" + mcpJsonMessage;
                }

                return true;
            }
            catch (Exception ex)
            {
                message = $"Failed to remove install: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Get the default installation path for the local .claude/skills directory
        /// </summary>
        public static string GetLocalSkillsPath()
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrEmpty(projectRoot))
            {
                return null;
            }

            return Path.Combine(projectRoot, ".claude", "skills", "Unity-AI-Forge");
        }

        /// <summary>
        /// Get the default installation path for the global ~/.claude/skills directory
        /// </summary>
        public static string GetGlobalSkillsPath()
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrEmpty(homeDir))
            {
                return null;
            }

            return Path.Combine(homeDir, ".claude", "skills", "Unity-AI-Forge");
        }

        /// <summary>
        /// Create or update .mcp.json file for Claude Code auto-start
        /// </summary>
        /// <param name="destinationPath">Installation destination path to determine local or global installation</param>
        /// <param name="message">Result message</param>
        private static bool CreateMcpJsonFile(string destinationPath, out string message)
        {
            try
            {
                // Determine if this is a local or global installation
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var isGlobalInstall = !string.IsNullOrEmpty(homeDir) &&
                                     destinationPath.StartsWith(homeDir, StringComparison.OrdinalIgnoreCase);

                // Determine .mcp.json path based on installation type
                string mcpJsonPath;
                string skillDirectory;

                if (isGlobalInstall)
                {
                    // Global installation: ~/.claude/mcp.json
                    // Path is relative to ~/.claude directory
                    mcpJsonPath = Path.Combine(homeDir, ".claude", "mcp.json");
                    skillDirectory = "skills/Unity-AI-Forge";
                }
                else
                {
                    // Local installation: project root/.mcp.json
                    // Path is relative to project root directory
                    var projectRoot = Path.GetDirectoryName(Application.dataPath);
                    if (string.IsNullOrEmpty(projectRoot))
                    {
                        message = ".mcp.json: Failed to determine project root directory";
                        return false;
                    }
                    mcpJsonPath = Path.Combine(projectRoot, ".mcp.json");
                    skillDirectory = ".claude/skills/Unity-AI-Forge";
                }

                // Load existing .mcp.json or create new structure
                var mcpConfig = new Dictionary<string, object>();
                var mcpServers = new Dictionary<string, object>();

                if (File.Exists(mcpJsonPath))
                {
                    try
                    {
                        var existingContent = File.ReadAllText(mcpJsonPath);
                        var existingConfig = MiniJson.Deserialize(existingContent) as Dictionary<string, object>;

                        if (existingConfig != null)
                        {
                            mcpConfig = existingConfig;

                            // Extract existing mcpServers
                            if (mcpConfig.ContainsKey("mcpServers") && mcpConfig["mcpServers"] is Dictionary<string, object> servers)
                            {
                                mcpServers = servers;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to parse existing .mcp.json, will create new one: {ex.Message}");
                    }
                }

                // Create or update skillforunity server configuration
                var skillforunityConfig = new Dictionary<string, object>
                {
                    ["command"] = "uv",
                    ["args"] = new List<object>
                    {
                        "run",
                        "--directory",
                        skillDirectory,
                        "src/main.py",
                        "--transport",
                        "stdio"
                    },
                    ["env"] = new Dictionary<string, object>
                    {
                        ["MCP_SERVER_TRANSPORT"] = "stdio",
                        ["MCP_LOG_LEVEL"] = "info"
                    }
                };

                // Update or add skillforunity configuration
                mcpServers["skillforunity"] = skillforunityConfig;
                mcpConfig["mcpServers"] = mcpServers;

                // Ensure parent directory exists
                var mcpJsonDir = Path.GetDirectoryName(mcpJsonPath);
                if (!string.IsNullOrEmpty(mcpJsonDir) && !Directory.Exists(mcpJsonDir))
                {
                    Directory.CreateDirectory(mcpJsonDir);
                }

                // Serialize and write to file with pretty formatting
                var jsonContent = MiniJson.Serialize(mcpConfig);
                var formattedJson = FormatJson(jsonContent);
                File.WriteAllText(mcpJsonPath, formattedJson);

                var installType = isGlobalInstall ? "global" : "project";
                message = $".mcp.json: Updated {installType} configuration at {mcpJsonPath}";
                return true;
            }
            catch (Exception ex)
            {
                message = $".mcp.json: Failed to create/update configuration file: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Format JSON string with proper indentation
        /// </summary>
        private static string FormatJson(string json)
        {
            var indent = 0;
            var formatted = new System.Text.StringBuilder();
            var inString = false;
            var escape = false;

            foreach (var ch in json)
            {
                if (escape)
                {
                    formatted.Append(ch);
                    escape = false;
                    continue;
                }

                if (ch == '\\' && inString)
                {
                    formatted.Append(ch);
                    escape = true;
                    continue;
                }

                if (ch == '"')
                {
                    formatted.Append(ch);
                    inString = !inString;
                    continue;
                }

                if (inString)
                {
                    formatted.Append(ch);
                    continue;
                }

                switch (ch)
                {
                    case '{':
                    case '[':
                        formatted.Append(ch);
                        formatted.Append('\n');
                        indent++;
                        formatted.Append(new string(' ', indent * 2));
                        break;
                    case '}':
                    case ']':
                        formatted.Append('\n');
                        indent--;
                        formatted.Append(new string(' ', indent * 2));
                        formatted.Append(ch);
                        break;
                    case ',':
                        formatted.Append(ch);
                        formatted.Append('\n');
                        formatted.Append(new string(' ', indent * 2));
                        break;
                    case ':':
                        formatted.Append(ch);
                        formatted.Append(' ');
                        break;
                    case ' ':
                    case '\n':
                    case '\r':
                    case '\t':
                        // Skip whitespace
                        break;
                    default:
                        formatted.Append(ch);
                        break;
                }
            }

            return formatted.ToString();
        }

        /// <summary>
        /// Remove Unity-AI-Forge entry from .mcp.json configuration file
        /// </summary>
        private static bool RemoveFromMcpJson(string destinationPath, out string message)
        {
            try
            {
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var isGlobalInstall = destinationPath.StartsWith(homeDir, StringComparison.OrdinalIgnoreCase);

                string mcpJsonPath;
                if (isGlobalInstall)
                {
                    // Global installation: ~/.claude/mcp.json
                    mcpJsonPath = Path.Combine(homeDir, ".claude", "mcp.json");
                }
                else
                {
                    // Local installation: project root/.mcp.json
                    var projectRoot = Path.GetDirectoryName(Application.dataPath);
                    if (string.IsNullOrEmpty(projectRoot))
                    {
                        message = ".mcp.json: Failed to determine project root directory";
                        return false;
                    }
                    mcpJsonPath = Path.Combine(projectRoot, ".mcp.json");
                }

                // Check if .mcp.json exists
                if (!File.Exists(mcpJsonPath))
                {
                    message = ".mcp.json: File not found, nothing to remove";
                    return true; // Not an error, just nothing to do
                }

                // Load existing .mcp.json
                string existingJson = File.ReadAllText(mcpJsonPath);
                var config = MiniJson.Deserialize(existingJson) as Dictionary<string, object>;

                if (config == null || !config.ContainsKey("mcpServers"))
                {
                    message = ".mcp.json: No mcpServers found, nothing to remove";
                    return true;
                }

                var servers = config["mcpServers"] as Dictionary<string, object>;
                if (servers == null || !servers.ContainsKey("skillforunity"))
                {
                    message = ".mcp.json: skillforunity entry not found, nothing to remove";
                    return true;
                }

                // Remove skillforunity entry
                servers.Remove("skillforunity");

                // If no servers left, delete the .mcp.json file
                if (servers.Count == 0)
                {
                    File.Delete(mcpJsonPath);
                    message = ".mcp.json: Removed file (no other servers configured)";
                    Debug.Log(message);
                    return true;
                }

                // Otherwise, update the .mcp.json file with remaining servers
                config["mcpServers"] = servers;
                string updatedJson = MiniJson.Serialize(config);
                string formattedJson = FormatJson(updatedJson);

                File.WriteAllText(mcpJsonPath, formattedJson);
                message = ".mcp.json: Removed skillforunity entry";
                Debug.Log(message);
                return true;
            }
            catch (Exception ex)
            {
                message = $".mcp.json: Failed to remove entry - {ex.Message}";
                Debug.LogWarning(message);
                return false;
            }
        }
    }
}
