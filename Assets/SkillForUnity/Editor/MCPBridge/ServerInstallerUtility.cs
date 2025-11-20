using System;
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
            // Find this script's location and look for SkillForUnity.zip in the parent directory
            var guids = AssetDatabase.FindAssets("ServerInstallerUtility t:Script");
            if (guids.Length > 0)
            {
                var scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                var scriptDir = Path.GetDirectoryName(scriptPath);
                var parentDir = Path.GetDirectoryName(Path.GetDirectoryName(scriptDir)); // Go up to Assets/SkillForUnity
                var zipPath = Path.Combine(parentDir, "SkillForUnity.zip");

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

            return Path.Combine(assetsPath, "SkillForUnity", "SkillForUnity.zip");
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

            // .claude/skills/SkillForUnity.zip を探す
            var skillZipPath = Path.Combine(projectRoot, ".claude", "skills", "SkillForUnity.zip");
            if (File.Exists(skillZipPath))
            {
                return skillZipPath;
            }

            // 見つからない場合、プロジェクトルートを検索
            var rootZipPath = Path.Combine(projectRoot, "SkillForUnity.zip");
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

        public static bool InstallSkillPackage(string destinationPath, out string message)
        {
            if (string.IsNullOrEmpty(SkillZipPath))
            {
                message = "Skill package (SkillForUnity.zip) not found. Please build the skill package first.";
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
                tempExtractPath = Path.Combine(Path.GetTempPath(), "SkillForUnity_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempExtractPath);
                ZipFile.ExtractToDirectory(SkillZipPath, tempExtractPath);

                // Find the SkillForUnity directory inside the extracted content
                var skillDir = Path.Combine(tempExtractPath, "SkillForUnity");
                if (Directory.Exists(skillDir))
                {
                    // Move the SkillForUnity directory to the destination
                    Directory.Move(skillDir, destinationPath);
                }
                else
                {
                    // If SkillForUnity subdirectory doesn't exist, move the temp directory itself
                    Directory.Move(tempExtractPath, destinationPath);
                    tempExtractPath = null; // Prevent deletion since we moved it
                }

                message = $"Skill package extracted to: {destinationPath}";
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

            return Path.Combine(projectRoot, ".claude", "skills", "SkillForUnity");
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

            return Path.Combine(homeDir, ".claude", "skills", "SkillForUnity");
        }
    }
}
