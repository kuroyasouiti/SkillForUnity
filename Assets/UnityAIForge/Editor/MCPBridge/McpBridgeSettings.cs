using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MCP.Editor
{
    [FilePath("ProjectSettings/McpBridgeSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class McpBridgeSettings : ScriptableSingleton<McpBridgeSettings>
    {
        private static bool _hideFlagsInitialized;

        [SerializeField] private string serverHost = "127.0.0.1";
        [SerializeField] private int serverPort = 7070;
        [SerializeField] private string bridgeToken = string.Empty;
        [SerializeField] private bool autoConnectOnLoad = true;
        [SerializeField] private float contextPushIntervalSeconds = 5f;
        [SerializeField] private string serverInstallPath = string.Empty;

        static McpBridgeSettings()
        {
            // Ensure hideFlags is set on the main thread
            EditorApplication.delayCall += EnsureHideFlagsOnMainThread;
        }

        public static McpBridgeSettings Instance
        {
            get
            {
                var instance = ScriptableSingleton<McpBridgeSettings>.instance;
                // hideFlags is applied on the main thread via EnsureHideFlagsOnMainThread
                return instance;
            }
        }

        private static void EnsureHideFlagsOnMainThread()
        {
            if (_hideFlagsInitialized)
            {
                return;
            }

            var inst = ScriptableSingleton<McpBridgeSettings>.instance;
            inst.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            _hideFlagsInitialized = true;
        }

        public string ServerHost
        {
            get => serverHost;
            set
            {
                var normalized = NormalizeHost(value);
                if (serverHost == normalized)
                {
                    return;
                }

                serverHost = normalized;
                SaveSettings();
            }
        }

        public int ServerPort
        {
            get => serverPort;
            set
            {
                if (serverPort == value)
                {
                    return;
                }

                var clamped = Mathf.Max(1, value);
                if (serverPort == clamped)
                {
                    return;
                }

                serverPort = clamped;
                SaveSettings();
            }
        }

        public string ServerInstallPath
        {
            get
            {
                var path = string.IsNullOrEmpty(serverInstallPath) ? DefaultServerInstallPath : serverInstallPath;
                return NormalizeInstallPath(path);
            }
            set
            {
                var normalized = NormalizeInstallPath(value);
                if (serverInstallPath == normalized)
                {
                    return;
                }

                serverInstallPath = normalized;
                SaveSettings();
            }
        }

        public string DefaultServerInstallPath
        {
            get
            {
                // Use McpServerManager's default path
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(userProfile, "Unity-AI-Forge");
            }
        }

        /// <summary>
        /// Gets or sets the bridge authentication token.
        /// Priority: 1) Environment variable MCP_BRIDGE_TOKEN, 2) Token file (.mcp_bridge_token), 3) Stored value.
        /// Use environment variables in CI/CD or shared environments to avoid committing secrets.
        /// </summary>
        public string BridgeToken
        {
            get
            {
                // First check environment variable
                var envToken = Environment.GetEnvironmentVariable("MCP_BRIDGE_TOKEN");
                if (!string.IsNullOrWhiteSpace(envToken))
                {
                    return envToken;
                }

                // Next try shared token file at project root
                var tokenFromFile = LoadTokenFromFile();
                if (!string.IsNullOrWhiteSpace(tokenFromFile))
                {
                    if (bridgeToken != tokenFromFile)
                    {
                        bridgeToken = tokenFromFile;
                        SaveSettings();
                    }
                    return tokenFromFile;
                }

                // Fallback to stored value
                return bridgeToken;
            }
            set
            {
                if (bridgeToken == value)
                {
                    return;
                }

                bridgeToken = value;
                SaveSettings();
            }
        }

        /// <summary>
        /// Gets the stored (non-environment) token value for display purposes.
        /// Returns masked version to avoid accidental exposure in logs/UI.
        /// </summary>
        public string BridgeTokenMasked
        {
            get
            {
                if (string.IsNullOrEmpty(bridgeToken))
                {
                    return string.Empty;
                }

                if (bridgeToken.Length <= 8)
                {
                    return new string('*', bridgeToken.Length);
                }

                return bridgeToken.Substring(0, 4) + new string('*', bridgeToken.Length - 8) + bridgeToken.Substring(bridgeToken.Length - 4);
            }
        }

        /// <summary>
        /// Checks if token is loaded from environment variable.
        /// </summary>
        public bool IsTokenFromEnvironment => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MCP_BRIDGE_TOKEN"));

        public bool AutoConnectOnLoad
        {
            get => autoConnectOnLoad;
            set
            {
                if (autoConnectOnLoad == value)
                {
                    return;
                }

                autoConnectOnLoad = value;
                SaveSettings();
            }
        }

        public float ContextPushIntervalSeconds
        {
            get => Mathf.Max(1f, contextPushIntervalSeconds);
            set
            {
                var clamped = Mathf.Max(1f, value);
                if (Mathf.Approximately(contextPushIntervalSeconds, clamped))
                {
                    return;
                }

                contextPushIntervalSeconds = clamped;
                SaveSettings();
            }
        }

        public string ListenerPrefix => $"http://{NormalizeHostForUri(serverHost)}:{serverPort}/";

        public string BridgeWebSocketUrl => $"ws://{NormalizeHostForUri(serverHost)}:{serverPort}/bridge";

        public string McpServerUrl => $"http://{NormalizeHostForUri(serverHost)}:{serverPort}/mcp";

        public void UseDefaultServerInstallPath()
        {
            serverInstallPath = string.Empty;
            SaveSettings();
        }

        public void SaveSettings()
        {
            EditorUtility.SetDirty(this);
            Save(true);
            AssetDatabase.SaveAssets();
        }

        private string LoadTokenFromFile()
        {
            try
            {
                var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                var tokenPath = Path.Combine(projectRoot, ".mcp_bridge_token");
                if (File.Exists(tokenPath))
                {
                    var content = File.ReadAllText(tokenPath).Trim();
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        return content;
                    }
                }
            }
            catch (Exception)
            {
                // ignore and fallback
            }

            // Auto-create if missing
            try
            {
                var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                var tokenPath = Path.Combine(projectRoot, ".mcp_bridge_token");
                if (!File.Exists(tokenPath))
                {
                    var token = System.Guid.NewGuid().ToString("N");
                    File.WriteAllText(tokenPath, token);
                    return token;
                }
            }
            catch (Exception)
            {
                // ignore and fallback
            }

            return null;
        }

        private static string NormalizeHost(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "127.0.0.1" : value.Trim();
        }

        private static string NormalizeHostForUri(string value)
        {
            var normalized = NormalizeHost(value);
            if (normalized.Contains(":") &&
                !(normalized.StartsWith("[", StringComparison.Ordinal) &&
                  normalized.EndsWith("]", StringComparison.Ordinal)))
            {
                return $"[{normalized}]";
            }

            return normalized;
        }

        private static string NormalizeInstallPath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var trimmed = value.Trim();

            try
            {
                trimmed = Path.GetFullPath(trimmed);
            }
            catch (Exception)
            {
                // keep trimmed fallback
            }

            // Return path as-is without appending any folder name
            return trimmed.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
