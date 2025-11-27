# MCP Server Installation Guide

**Skill for Unity** のMCPサーバーをAIツール（Cursor、Claude Desktop、Cline、Windsurf）に登録する方法を説明します。

---

## 📋 前提条件

1. **Python 3.8以上**がインストールされていること
2. **UV**がインストールされていること（推奨）
   - インストール: `pip install uv` または https://github.com/astral-sh/uv

---

## 🚀 クイックスタート

### Unity Editor から1クリックインストール

1. Unity Editorを開く
2. メニューから **Skill for Unity > MCP Server Manager** を選択
3. **Install Server** ボタンをクリック
4. 使用したいAIツールの **Register** ボタンをクリック
5. AIツールを再起動

これだけです！

---

## 📖 詳細手順

### 1. MCP Server Manager を開く

Unity Editorのメニューから：
```
Skill for Unity > MCP Server Manager
```

### 2. サーバーをインストール

**MCP Server Manager**ウィンドウで：

1. **Server Status**セクションで現在のステータスを確認
2. **Install Server**ボタンをクリック
3. インストールが完了するまで待機（通常10-30秒）

インストール先: `%USERPROFILE%\.claude\skills\SkillForUnity`

### 3. AIツールに登録

**AI Tool Registration**セクションで：

#### オプション1: 個別に登録

使用したいAIツールの **Register** ボタンをクリック：
- ✅ Cursor
- ✅ Claude Desktop
- ✅ Cline (VS Code)
- ✅ Windsurf

#### オプション2: 一括登録

**Register All** ボタンをクリックして、すべてのAIツールに一度に登録

### 4. AIツールを再起動

登録後、対象のAIツールを完全に再起動してください。

---

## 🔧 各AIツールの設定ファイル

### Cursor
**パス**: `%APPDATA%\Cursor\User\globalStorage\saoudrizwan.claude-dev\settings\cline_mcp_settings.json`

### Claude Desktop
**パス**: `%APPDATA%\Claude\claude_desktop_config.json`

### Cline (VS Code)
**パス**: `%APPDATA%\Code\User\globalStorage\saoudrizwan.claude-dev\settings\cline_mcp_settings.json`

### Windsurf
**パス**: `%APPDATA%\Windsurf\User\globalStorage\windsurf.windsurf\settings\mcp_settings.json`

---

## 🔄 更新手順

サーバーを最新版に更新するには：

1. **MCP Server Manager**を開く
2. **Reinstall Server**ボタンをクリック
3. AIツールを再起動

---

## 🗑️ アンインストール

### 完全アンインストール

1. **MCP Server Manager**で **Unregister All** をクリック
2. **Uninstall Server** をクリック
3. AIツールを再起動

### 特定のAIツールのみ解除

1. **MCP Server Manager**を開く
2. 解除したいツールの **Unregister** ボタンをクリック
3. 該当のAIツールを再起動

---

## 🐛 トラブルシューティング

### サーバーが起動しない

1. **Python**と**UV**がインストールされているか確認
   ```
   python --version
   uv --version
   ```

2. 手動でセットアップ
   ```powershell
   cd %USERPROFILE%\.claude\skills\SkillForUnity
   uv sync
   ```

### AIツールに表示されない

1. AIツールを完全に再起動（タスクマネージャーからプロセスを終了）
2. 設定ファイルが正しく作成されているか確認
3. **MCP Server Manager**で登録状態を確認

### エラーログの確認

1. **MCP Server Manager**の**Log**セクションを確認
2. Unity Consoleでエラーメッセージを確認
3. AIツールのログを確認（通常はツール内の設定から）

---

## 📚 追加情報

### 手動インストール（上級者向け）

MCP Server Managerを使わずに手動でインストールする場合：

1. サーバーファイルをコピー
   ```powershell
   xcopy /E /I /Y "Assets\SkillForUnity\MCPServer" "%USERPROFILE%\.claude\skills\SkillForUnity"
   ```

2. Python環境をセットアップ
   ```powershell
   cd %USERPROFILE%\.claude\skills\SkillForUnity
   uv sync
   ```

3. AIツールの設定ファイルを手動編集
   - 各ツールの設定ファイルに以下を追加：
   ```json
   {
     "mcpServers": {
       "skill-for-unity": {
         "command": "uv",
         "args": [
           "--directory",
           "C:\\Users\\YOUR_USERNAME\\.claude\\skills\\SkillForUnity",
           "run",
           "skill-for-unity"
         ]
       }
     }
   }
   ```

### バックアップと復元

設定ファイルは自動的にバックアップされます：
- バックアップ先: 元の設定ファイルと同じディレクトリ
- 命名規則: `{元のファイル名}.backup.{日時}`

復元が必要な場合は、**MCP Server Manager**の機能を使用するか、手動でバックアップファイルをリネームしてください。

---

## ❓ ヘルプとサポート

- **GitHub Issues**: https://github.com/kuroyasouiti/SkillForUnity/issues
- **ドキュメント**: https://github.com/kuroyasouiti/SkillForUnity
- **Unity Console**: エラーメッセージを確認

---

**最終更新**: 2025-11-27

