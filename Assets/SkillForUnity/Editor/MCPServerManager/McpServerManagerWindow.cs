using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MCP.Editor.ServerManager
{
    /// <summary>
    /// MCP Server Manager用のEditorウィンドウ。
    /// サーバーのインストール、AIツールへの登録などのGUIを提供します。
    /// </summary>
    public class McpServerManagerWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private ServerStatus serverStatus;
        private Dictionary<AITool, bool> registrationStatus;
        private List<string> logMessages = new List<string>();
        private const int MaxLogMessages = 100;
        
        // GUI Styles
        private GUIStyle headerStyle;
        private GUIStyle sectionStyle;
        private GUIStyle statusBoxStyle;
        private GUIStyle logBoxStyle;
        private bool stylesInitialized = false;
        
        [MenuItem("Skill for Unity/MCP Server Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<McpServerManagerWindow>("MCP Server Manager");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }
        
        private void OnEnable()
        {
            RefreshStatus();
            
            // ログハンドラーを登録
            Application.logMessageReceived += OnLogMessageReceived;
        }
        
        private void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }
        
        private void OnGUI()
        {
            InitializeStyles();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // ヘッダー
            DrawHeader();
            
            EditorGUILayout.Space(10);
            
            // サーバーステータス
            DrawServerStatus();
            
            EditorGUILayout.Space(10);
            
            // サーバー操作
            DrawServerOperations();
            
            EditorGUILayout.Space(20);
            
            // AIツール登録
            DrawToolRegistration();
            
            EditorGUILayout.Space(20);
            
            // ログ
            DrawLog();
            
            EditorGUILayout.Space(10);
            
            // フッター
            DrawFooter();
            
            EditorGUILayout.EndScrollView();
        }
        
        #region GUI Drawing
        
        private void DrawHeader()
        {
            EditorGUILayout.LabelField("MCP Server Manager", headerStyle);
            EditorGUILayout.LabelField($"Version {McpServerManager.Version}", EditorStyles.miniLabel);
        }
        
        private void DrawServerStatus()
        {
            EditorGUILayout.LabelField("📦 Server Status", sectionStyle);
            
            EditorGUILayout.BeginVertical(statusBoxStyle);
            
            var status = serverStatus ?? McpServerManager.GetStatus();
            
            // ステータス表示
            var statusIcon = status.IsInstalled ? "✅" : "❌";
            EditorGUILayout.LabelField($"{statusIcon} Status: {(status.IsInstalled ? "Installed" : "Not Installed")}");
            
            if (status.IsInstalled)
            {
                EditorGUILayout.LabelField($"Install Path: {status.InstallPath}", EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField($"Version: {status.Version}");
                EditorGUILayout.LabelField($"Python: {(status.PythonAvailable ? "✅ Available" : "❌ Not Found")}");
                EditorGUILayout.LabelField($"UV: {(status.UvAvailable ? "✅ Available" : "❌ Not Found")}");
            }
            else
            {
                EditorGUILayout.LabelField($"Source Path: {status.SourcePath}", EditorStyles.wordWrappedLabel);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawServerOperations()
        {
            var isInstalled = serverStatus != null && serverStatus.IsInstalled;
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = !isInstalled;
            if (GUILayout.Button("Install Server", GUILayout.Height(30)))
            {
                ExecuteWithErrorHandling(() =>
                {
                    McpServerManager.Install();
                    RefreshStatus();
                    AddLog("Server installed successfully!");
                });
            }
            GUI.enabled = true;
            
            GUI.enabled = isInstalled;
            if (GUILayout.Button("Uninstall Server", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Confirm Uninstall",
                    "Are you sure you want to uninstall the MCP server?",
                    "Yes", "No"))
                {
                    ExecuteWithErrorHandling(() =>
                    {
                        McpServerManager.Uninstall();
                        RefreshStatus();
                        AddLog("Server uninstalled successfully!");
                    });
                }
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = isInstalled;
            if (GUILayout.Button("Reinstall Server", GUILayout.Height(30)))
            {
                ExecuteWithErrorHandling(() =>
                {
                    McpServerManager.Reinstall();
                    RefreshStatus();
                    AddLog("Server reinstalled successfully!");
                });
            }
            GUI.enabled = true;
            
            if (GUILayout.Button("Refresh Status", GUILayout.Height(30)))
            {
                RefreshStatus();
                AddLog("Status refreshed");
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawToolRegistration()
        {
            EditorGUILayout.LabelField("🔧 AI Tool Registration", sectionStyle);
            
            var isInstalled = serverStatus != null && serverStatus.IsInstalled;
            
            if (!isInstalled)
            {
                EditorGUILayout.HelpBox("Please install the MCP server first.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.BeginVertical(statusBoxStyle);
            
            // 各ツールの登録状態
            foreach (AITool tool in Enum.GetValues(typeof(AITool)))
            {
                DrawToolRegistrationRow(tool);
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // 一括操作ボタン
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Register All", GUILayout.Height(25)))
            {
                ExecuteWithErrorHandling(() =>
                {
                    McpToolRegistry.RegisterAll();
                    RefreshStatus();
                    AddLog("Registered to all available tools");
                });
            }
            
            if (GUILayout.Button("Unregister All", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Confirm Unregister All",
                    "Are you sure you want to unregister from all AI tools?",
                    "Yes", "No"))
                {
                    ExecuteWithErrorHandling(() =>
                    {
                        McpToolRegistry.UnregisterAll();
                        RefreshStatus();
                        AddLog("Unregistered from all tools");
                    });
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawToolRegistrationRow(AITool tool)
        {
            EditorGUILayout.BeginHorizontal();
            
            var isRegistered = registrationStatus != null && registrationStatus.ContainsKey(tool) && registrationStatus[tool];
            var statusIcon = isRegistered ? "✅" : "❌";
            
            EditorGUILayout.LabelField($"{statusIcon} {McpConfigManager.GetToolDisplayName(tool)}", GUILayout.Width(200));
            
            GUILayout.FlexibleSpace();
            
            if (isRegistered)
            {
                if (GUILayout.Button("Unregister", GUILayout.Width(100)))
                {
                    ExecuteWithErrorHandling(() =>
                    {
                        McpToolRegistry.Unregister(tool);
                        RefreshStatus();
                        AddLog($"Unregistered from {tool}");
                    });
                }
            }
            else
            {
                if (GUILayout.Button("Register", GUILayout.Width(100)))
                {
                    ExecuteWithErrorHandling(() =>
                    {
                        McpToolRegistry.Register(tool);
                        RefreshStatus();
                        AddLog($"Registered to {tool}");
                    });
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawLog()
        {
            EditorGUILayout.LabelField("📋 Log", sectionStyle);
            
            EditorGUILayout.BeginVertical(logBoxStyle);
            
            if (logMessages.Count == 0)
            {
                EditorGUILayout.LabelField("No log messages", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                for (int i = logMessages.Count - 1; i >= 0; i--)
                {
                    EditorGUILayout.LabelField(logMessages[i], EditorStyles.wordWrappedLabel);
                }
            }
            
            EditorGUILayout.EndVertical();
            
            if (GUILayout.Button("Clear Log"))
            {
                logMessages.Clear();
            }
        }
        
        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Open Server Folder"))
            {
                McpServerManager.OpenInstallFolder();
            }
            
            if (GUILayout.Button("Open Source Folder"))
            {
                McpServerManager.OpenSourceFolder();
            }
            
            if (GUILayout.Button("View Documentation"))
            {
                Application.OpenURL("https://github.com/kuroyasouiti/SkillForUnity");
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        #endregion
        
        #region Helper Methods
        
        private void InitializeStyles()
        {
            if (stylesInitialized) return;
            
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            
            sectionStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12
            };
            
            statusBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
            
            logBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
            
            stylesInitialized = true;
        }
        
        private void RefreshStatus()
        {
            serverStatus = McpServerManager.GetStatus();
            registrationStatus = McpToolRegistry.GetRegistrationStatus();
            Repaint();
        }
        
        private void AddLog(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            logMessages.Add($"[{timestamp}] {message}");
            
            // 最大メッセージ数を超えたら古いものを削除
            while (logMessages.Count > MaxLogMessages)
            {
                logMessages.RemoveAt(0);
            }
            
            Repaint();
        }
        
        private void ExecuteWithErrorHandling(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
                EditorUtility.DisplayDialog("Error", ex.Message, "OK");
            }
        }
        
        private void OnLogMessageReceived(string message, string stackTrace, LogType type)
        {
            // MCPサーバーマネージャー関連のログのみ表示
            if (message.Contains("[McpServerManager]") || 
                message.Contains("[McpServerInstaller]") ||
                message.Contains("[McpConfigManager]") ||
                message.Contains("[McpToolRegistry]"))
            {
                // [タグ]を削除してメッセージのみ表示
                var cleanMessage = message;
                if (message.Contains("]"))
                {
                    var index = message.IndexOf("]");
                    cleanMessage = message.Substring(index + 1).Trim();
                }
                
                AddLog(cleanMessage);
            }
        }
        
        #endregion
    }
}

