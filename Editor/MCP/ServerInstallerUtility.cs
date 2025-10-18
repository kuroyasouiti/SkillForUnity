using System;
using System.IO;
using UnityEngine;

namespace MCP.Editor
{
    internal static class ServerInstallerUtility
    {
        private static readonly string[] IgnoredDirectories =
        {
            "node_modules",
            "dist",
            ".git",
            ".venv",
            "__pycache__"
        };

        public static string TemplatePath => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "mcp-server"));

        public static bool InstallTemplate(string destinationPath, out string message)
        {
            if (!Directory.Exists(TemplatePath))
            {
                message = $"Template server not found: {TemplatePath}";
                return false;
            }

            try
            {
                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }

                CopyDirectory(new DirectoryInfo(TemplatePath), new DirectoryInfo(destinationPath));
                EnsureEnvFile(destinationPath);
                message = $"Template copied to {destinationPath}";
                return true;
            }
            catch (Exception ex)
            {
                message = $"Failed to install template: {ex.Message}";
                return false;
            }
        }

        public static bool HasPyProject(string path)
        {
            var pyProject = Path.Combine(path, "pyproject.toml");
            return File.Exists(pyProject);
        }

        private static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (var dir in source.GetDirectories())
            {
                if (Array.Exists(IgnoredDirectories, name => string.Equals(name, dir.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var targetSubDir = target.CreateSubdirectory(dir.Name);
                CopyDirectory(dir, targetSubDir);
            }

            foreach (var file in source.GetFiles())
            {
                if (string.Equals(file.Extension, ".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var targetFilePath = Path.Combine(target.FullName, file.Name);
                file.CopyTo(targetFilePath, overwrite: true);
            }
        }

        private static void EnsureEnvFile(string rootPath)
        {
            var examplePath = Path.Combine(rootPath, ".env.example");
            if (!File.Exists(examplePath))
            {
                return;
            }

            var envPath = Path.Combine(rootPath, ".env");
            if (File.Exists(envPath))
            {
                return;
            }

            File.Copy(examplePath, envPath);
        }
    }
}
