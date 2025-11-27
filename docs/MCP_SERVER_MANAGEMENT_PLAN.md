# MCP Server Management Implementation Plan

**日付**: 2025-11-27  
**目的**: MCPサーバーをUnityプロジェクト内に統合し、ユーザーフォルダへの自動インストール・登録機能を実装  

---

## 📋 概要

現在、MCPサーバーは`.claude/skills/SkillForUnity`に配置されていますが、これをUnityプロジェクト内（`Assets/SkillForUnity/MCPServer/`）に移動し、Unity Editorから各種AIツールへの登録・解除を管理できるようにします。

---

## 🎯 目標

1. **MCPサーバーのUnity統合**
   - サーバーコードを`Assets/SkillForUnity/MCPServer/`に移動
   - Unityプロジェクトと一緒に配布可能に

2. **自動インストール機能**
   - Unity Editorメニューから1クリックでインストール
   - ユーザーホーム（`%USERPROFILE%/.claude/skills/SkillForUnity`など）にコピー

3. **AIツール登録管理**
   - Cursor
   - Claude Desktop
   - Cline (VS Code拡張)
   - Windsurf
   - 各ツールの設定ファイルを自動編集

4. **アンインストール機能**
   - サーバーファイルの削除
   - 設定ファイルからの登録解除

---

## 🏗️ 新しいディレクトリ構造

```
Assets/SkillForUnity/
├── MCPServer/                          ← 新規追加
│   ├── src/                           (Pythonソースコード)
│   │   ├── __init__.py
│   │   ├── main.py
│   │   ├── bridge/
│   │   ├── config/
│   │   ├── resources/
│   │   ├── server/
│   │   ├── services/
│   │   ├── tools/
│   │   ├── utils/
│   │   └── version.py
│   ├── config/                        (設定ファイルテンプレート)
│   │   ├── cursor.json.example
│   │   ├── claude-desktop.json.example
│   │   ├── cline.json.example
│   │   └── windsurf.json.example
│   ├── pyproject.toml
│   ├── uv.lock
│   ├── README.md
│   └── QUICKSTART.md
├── Editor/
│   ├── MCPBridge/                     (既存)
│   └── MCPServerManager/              ← 新規追加
│       ├── McpServerManager.cs        (サーバー管理)
│       ├── McpServerInstaller.cs      (インストール処理)
│       ├── McpConfigManager.cs        (設定ファイル管理)
│       ├── McpToolRegistry.cs         (AIツール登録管理)
│       └── McpServerManagerWindow.cs  (Editorウィンドウ)
└── Runtime/                           (既存)
```

---

## 🔧 実装詳細

### 1. McpServerManager.cs

**役割**: サーバー管理の中心クラス

```csharp
public static class McpServerManager
{
    // インストール先パス
    public static string UserInstallPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".claude", "skills", "SkillForUnity"
    );
    
    // ソースパス
    public static string SourcePath => Path.Combine(
        Application.dataPath, "SkillForUnity", "MCPServer"
    );
    
    // メソッド
    public static bool IsInstalled();
    public static void Install();
    public static void Uninstall();
    public static void Reinstall();
    public static ServerStatus GetStatus();
}
```

### 2. McpServerInstaller.cs

**役割**: ファイルコピーとPython環境セットアップ

```csharp
public static class McpServerInstaller
{
    public static void CopyServerFiles(string sourcePath, string destPath);
    public static void SetupPythonEnvironment(string installPath);
    public static void ValidateInstallation(string installPath);
}
```

### 3. McpConfigManager.cs

**役割**: 各AIツールの設定ファイル管理

```csharp
public static class McpConfigManager
{
    // 設定ファイルパス取得
    public static string GetCursorConfigPath();
    public static string GetClaudeDesktopConfigPath();
    public static string GetClineConfigPath();
    public static string GetWindsurfConfigPath();
    
    // 設定ファイル操作
    public static bool ConfigExists(AITool tool);
    public static JObject LoadConfig(AITool tool);
    public static void SaveConfig(AITool tool, JObject config);
    public static void BackupConfig(AITool tool);
}

public enum AITool
{
    Cursor,
    ClaudeDesktop,
    Cline,
    Windsurf
}
```

### 4. McpToolRegistry.cs

**役割**: AIツールへの登録・解除

```csharp
public static class McpToolRegistry
{
    // 登録
    public static void Register(AITool tool);
    public static void RegisterAll();
    
    // 解除
    public static void Unregister(AITool tool);
    public static void UnregisterAll();
    
    // 状態確認
    public static bool IsRegistered(AITool tool);
    public static Dictionary<AITool, bool> GetRegistrationStatus();
}
```

### 5. McpServerManagerWindow.cs

**役割**: Unity Editor GUIウィンドウ

```csharp
public class McpServerManagerWindow : EditorWindow
{
    [MenuItem("Skill for Unity/MCP Server Manager")]
    public static void ShowWindow();
    
    private void OnGUI()
    {
        // サーバーステータス表示
        // インストール/アンインストールボタン
        // AIツール登録状態と登録/解除ボタン
        // ログ表示
    }
}
```

---

## 📝 設定ファイル形式

### Cursor (`%APPDATA%/Cursor/User/globalStorage/saoudrizwan.claude-dev/settings/cline_mcp_settings.json`)

```json
{
  "mcpServers": {
    "skill-for-unity": {
      "command": "uv",
      "args": [
        "--directory",
        "C:\\Users\\{USERNAME}\\.claude\\skills\\SkillForUnity",
        "run",
        "skill-for-unity"
      ]
    }
  }
}
```

### Claude Desktop (`%APPDATA%/Claude/claude_desktop_config.json`)

```json
{
  "mcpServers": {
    "skill-for-unity": {
      "command": "uv",
      "args": [
        "--directory",
        "C:\\Users\\{USERNAME}\\.claude\\skills\\SkillForUnity",
        "run",
        "skill-for-unity"
      ]
    }
  }
}
```

### Cline (VS Code) (`%APPDATA%/Code/User/globalStorage/saoudrizwan.claude-dev/settings/cline_mcp_settings.json`)

```json
{
  "mcpServers": {
    "skill-for-unity": {
      "command": "uv",
      "args": [
        "--directory",
        "C:\\Users\\{USERNAME}\\.claude\\skills\\SkillForUnity",
        "run",
        "skill-for-unity"
      ]
    }
  }
}
```

### Windsurf (`%APPDATA%/Windsurf/User/globalStorage/windsurf.windsurf/settings/mcp_settings.json`)

```json
{
  "mcpServers": {
    "skill-for-unity": {
      "command": "uv",
      "args": [
        "--directory",
        "C:\\Users\\{USERNAME}\\.claude\\skills\\SkillForUnity",
        "run",
        "skill-for-unity"
      ]
    }
  }
}
```

---

## 🔄 処理フロー

### インストールフロー

1. **検証**
   - ソースディレクトリの存在確認
   - Pythonとuvの存在確認

2. **ファイルコピー**
   - `Assets/SkillForUnity/MCPServer/` → `%USERPROFILE%/.claude/skills/SkillForUnity`
   - ディレクトリ構造を維持してコピー

3. **Python環境セットアップ**
   - `uv sync`を実行して依存関係をインストール

4. **検証**
   - インストール完了を確認
   - サーバーが起動可能か確認

### 登録フロー

1. **設定ファイル確認**
   - 対象AIツールの設定ファイルパスを取得
   - ファイルが存在するか確認

2. **バックアップ**
   - 既存の設定ファイルをバックアップ

3. **設定追加**
   - JSON設定を読み込み
   - `mcpServers`セクションに`skill-for-unity`を追加
   - 既存の設定は保持

4. **保存**
   - 更新された設定をファイルに書き込み

### 解除フロー

1. **設定ファイル読み込み**
2. **エントリ削除**
   - `mcpServers.skill-for-unity`を削除
3. **保存**

---

## 🎨 EditorウィンドウUI設計

```
┌─────────────────────────────────────────────┐
│ MCP Server Manager                          │
├─────────────────────────────────────────────┤
│                                             │
│ 📦 Server Status                            │
│ ┌─────────────────────────────────────────┐ │
│ │ Status: ✅ Installed                    │ │
│ │ Path: C:\Users\..\.claude\skills\...   │ │
│ │ Version: 0.1.0                          │ │
│ └─────────────────────────────────────────┘ │
│                                             │
│ [Install Server]  [Uninstall Server]       │
│ [Reinstall Server]                          │
│                                             │
│ ─────────────────────────────────────────── │
│                                             │
│ 🔧 AI Tool Registration                     │
│ ┌─────────────────────────────────────────┐ │
│ │ ✅ Cursor          [Unregister]         │ │
│ │ ✅ Claude Desktop  [Unregister]         │ │
│ │ ❌ Cline           [Register]           │ │
│ │ ❌ Windsurf        [Register]           │ │
│ └─────────────────────────────────────────┘ │
│                                             │
│ [Register All]  [Unregister All]           │
│                                             │
│ ─────────────────────────────────────────── │
│                                             │
│ 📋 Log                                      │
│ ┌─────────────────────────────────────────┐ │
│ │ [2025-11-27] Server installed           │ │
│ │ [2025-11-27] Registered to Cursor       │ │
│ │ ...                                     │ │
│ └─────────────────────────────────────────┘ │
│                                             │
│ [Open Server Folder]  [View Documentation] │
└─────────────────────────────────────────────┘
```

---

## 🚀 実装順序

### Phase 1: MCPサーバーの移動
1. ✅ `.claude/skills/SkillForUnity` → `Assets/SkillForUnity/MCPServer/`
2. ✅ `.gitignore`更新

### Phase 2: 基本機能実装
1. `McpServerManager.cs` - サーバー管理
2. `McpServerInstaller.cs` - インストール処理
3. `McpConfigManager.cs` - 設定ファイル管理

### Phase 3: AIツール登録機能
1. `McpToolRegistry.cs` - 登録/解除ロジック
2. 各AIツールの設定ファイルパス実装

### Phase 4: EditorウィンドウUI
1. `McpServerManagerWindow.cs` - GUIウィンドウ
2. メニュー項目追加
3. ステータス表示

### Phase 5: テストとドキュメント
1. 各機能のテスト
2. ドキュメント更新
3. QUICKSTART.md更新

---

## 🔒 注意事項

### セキュリティ
- ユーザー設定ファイルを編集する前に必ずバックアップ
- バックアップは`.backup`拡張子で保存
- JSONのバリデーションを必ず実施

### エラーハンドリング
- ファイル操作の例外を適切にキャッチ
- ユーザーに分かりやすいエラーメッセージを表示
- ロールバック機能を実装

### クロスプラットフォーム
- Windowsを優先実装
- macOS/Linuxは将来的にサポート

---

## 📚 参考情報

### AIツール設定パス (Windows)

- **Cursor**: `%APPDATA%/Cursor/User/globalStorage/saoudrizwan.claude-dev/settings/cline_mcp_settings.json`
- **Claude Desktop**: `%APPDATA%/Claude/claude_desktop_config.json`
- **Cline (VS Code)**: `%APPDATA%/Code/User/globalStorage/saoudrizwan.claude-dev/settings/cline_mcp_settings.json`
- **Windsurf**: `%APPDATA%/Windsurf/User/globalStorage/windsurf.windsurf/settings/mcp_settings.json`

### AIツール設定パス (macOS)

- **Cursor**: `~/Library/Application Support/Cursor/User/globalStorage/saoudrizwan.claude-dev/settings/cline_mcp_settings.json`
- **Claude Desktop**: `~/Library/Application Support/Claude/claude_desktop_config.json`
- **Cline (VS Code)**: `~/Library/Application Support/Code/User/globalStorage/saoudrizwan.claude-dev/settings/cline_mcp_settings.json`
- **Windsurf**: `~/Library/Application Support/Windsurf/User/globalStorage/windsurf.windsurf/settings/mcp_settings.json`

---

**作成日**: 2025-11-27  
**最終更新**: 2025-11-27  
**ステータス**: 🚧 計画中

