# Version 1.6.0 Release Notes 🎉

**リリース日**: 2025-11-27  
**バージョン**: 1.6.0  
**タイプ**: メジャーアップデート  

---

## 📋 概要

SkillForUnity v1.6.0は、Unity Editorからの完全なMCPサーバー管理機能を追加するメジャーアップデートです。CLI依存を排除し、JSON設定ファイルの直接編集によるAIツール登録、ユーザー設定可能なインストールパス、Cursor設定ファイルの自動検出など、多数の改善が含まれています。

---

## 🎯 主要な変更点

### 1. 統合MCPサーバーマネージャー ✨

Unity Editorから直接MCPサーバーを管理できるようになりました。

**機能**:
- ✅ Install/Uninstall/Reinstall操作
- ✅ サーバーステータス監視（Python、UV可用性）
- ✅ インストール/ソースフォルダへの直接アクセス
- ✅ ユーザー設定可能なインストールパス（Browseボタン付き）

**アクセス**:
```
Tools > MCP Assistant > MCP Server Manager
```

### 2. AIツール登録システム 🔧

JSON設定ファイルの直接編集によるAIツール登録機能。

**対応ツール**:
- ✅ Cursor
- ✅ Claude Desktop
- ✅ Cline (VS Code)
- ✅ Windsurf

**機能**:
- ✅ 個別登録/解除
- ✅ 一括登録/解除
- ✅ 自動バックアップ（タイムスタンプ付き）
- ✅ 設定ファイルパス表示
- ✅ 📂ボタンで設定ファイル/ディレクトリを開く
- ✅ 📦ボタンで手動バックアップ作成

**利点**:
- CLI不要（Cursor CLI、claude-code CLIなどのインストール不要）
- 確実な登録（JSON直接編集）
- プラットフォーム非依存

### 3. Cursor設定ファイル自動検出 🔍

Cursorの設定ファイルを5つの候補パスから自動検出。

**検索順序**:
1. `Cursor\User\globalStorage\cursor\mcp.json`
2. `Cursor\User\globalStorage\cursor-mcp\settings.json`
3. `Cursor\User\globalStorage\rooveterinaryinc.roo-cline\settings\cline_mcp_settings.json` ⭐ 最も一般的
4. `Cursor\User\globalStorage\saoudrizwan.claude-dev\settings\cline_mcp_settings.json`
5. `Cursor\User\settings.json` (mcpServersセクションがある場合)

**機能**:
- ✅ 自動パス検出
- ✅ 環境適応
- ✅ 詳細デバッグログ
- ✅ フォールバック機能

### 4. ユーザー設定可能なインストールパス 🎯

MCPサーバーのインストール先を自由に設定可能。

**UI機能**:
- ✅ テキストフィールドで直接編集
- ✅ **Browse...**ボタン：フォルダ選択ダイアログ
- ✅ **Default**ボタン：デフォルトパスに復元
- ✅ デフォルトパス表示（参考用）
- ✅ リアルタイム更新

**保存**:
- プロジェクト設定として保存（`ProjectSettings/McpBridgeSettings.asset`）
- プロジェクトごとに設定
- Git経由で共有可能

### 5. デフォルトパスの変更 📁

**Before (v1.5.x)**:
```
C:\Users\username\.claude\skills\SkillForUnity
```

**After (v1.6.0)**:
```
C:\Users\username\SkillForUnity
```

**理由**:
- ✅ よりシンプルなパス構造
- ✅ ユーザーホームディレクトリ直下
- ✅ 見つけやすい
- ✅ 完全カスタマイズ可能

### 6. 統一MCP Assistantウィンドウ 🎨

すべてのMCP管理機能を1つのウィンドウに統合。

**セクション**:
1. **Bridge Listener**: WebSocket接続管理
2. **MCP Server Manager**: サーバーインストール・管理
3. **AI Tool Registration**: AIツール登録・設定
4. **Command Output**: リアルタイムログ表示

**アクセス**:
```
Tools > MCP Assistant
または
Skill for Unity > MCP Assistant
```

---

## 📊 統計

### コード統計
- **新規クラス**: 5クラス
  - `McpServerManager.cs` (305行)
  - `McpServerInstaller.cs` (266行)
  - `McpConfigManager.cs` (217行)
  - `McpToolRegistry.cs` (251行)
  - `McpCliRegistry.cs` (204行, deprecated)
- **更新クラス**: 2クラス
  - `McpBridgeWindow.cs` (大幅更新)
  - `McpBridgeSettings.cs` (パス管理統合)
- **総追加行数**: 約2,500行

### ドキュメント
- **新規ドキュメント**: 7ファイル
  - `JSON_CONFIG_REGISTRATION.md`
  - `CURSOR_CONFIG_FIX.md`
  - `USER_CONFIGURABLE_INSTALL_PATH.md`
  - `CLI_REGISTRATION_MIGRATION.md`
  - `MCP_SERVER_MANAGEMENT_PLAN.md`
  - `MCP_SERVER_MANAGEMENT_COMPLETED.md`
  - `MCP_BRIDGE_INTEGRATION_REPORT.md`
- **更新ドキュメント**: 4ファイル
  - `README.md`
  - `README_ja.md`
  - `CHANGELOG.md`
  - `skill.yml`

---

## 🚀 アップグレードガイド

### v1.5.x からのアップグレード

#### 1. Unity パッケージの更新

**Unity Package Manager経由**:
```
Window > Package Manager > SkillForUnity > Update
```

**手動**:
```
git pull
Unity Editor再起動
```

#### 2. インストールパスの確認

デフォルトパスが変更されました：

**旧**:
```
C:\Users\username\.claude\skills\SkillForUnity
```

**新**:
```
C:\Users\username\SkillForUnity
```

**オプション**:
1. 新しいデフォルトパスで再インストール（推奨）
2. 旧パスを保持（カスタムパスとして設定）

#### 3. MCP Assistant を開く

```
Tools > MCP Assistant
```

#### 4. サーバーをインストール

**新規インストール**:
1. MCP Server Manager セクション
2. Install Path Settings で好みのパスを選択
3. **Install Server** をクリック

**既存インストールの移行**:
1. 旧パスをカスタムパスとして設定
2. または **Reinstall** で新パスに移行

#### 5. AIツールを登録

1. AI Tool Registration セクション
2. 各ツールの **Register** ボタンをクリック
3. AIツールを再起動

---

## 💡 使用例

### 基本的なワークフロー

```
1. Unity Editorを開く
   ↓
2. Tools > MCP Assistant を開く
   ↓
3. MCP Server Manager
   - Install Path Settings でパスを設定（オプション）
   - Install Server をクリック
   ↓
4. AI Tool Registration
   - Cursor の Register をクリック
   - （他のツールも必要に応じて登録）
   ↓
5. Cursorを再起動
   ↓
6. 完了！Cursorから Unity を操作可能
```

### カスタムパスの設定

```
1. MCP Server Manager > Install Path Settings
   ↓
2. オプションA: 直接入力
   - Install To フィールドに入力
   ↓
   オプションB: Browse
   - Browse... ボタンをクリック
   - フォルダを選択
   ↓
3. Install Server または Reinstall
   ↓
4. AIツールを再登録
```

---

## ⚠️ 重要な変更と互換性

### 破壊的変更

1. **デフォルトインストールパスの変更**
   - 旧: `~/.claude/skills/SkillForUnity`
   - 新: `~/SkillForUnity`
   - 影響: 既存インストールは旧パスに残る（手動移行が必要）

2. **CLI登録の廃止**
   - `McpCliRegistry.cs` は deprecated
   - JSON直接編集方式を使用
   - 影響: CLI依存の古いスクリプトは動作しない

### 互換性の維持

- ✅ 旧パスも引き続き使用可能（カスタムパスとして）
- ✅ 既存の設定ファイルは自動バックアップ
- ✅ 既存の登録は影響を受けない

---

## 🐛 既知の問題

### 1. Cursor設定ファイルが見つからない

**症状**: Cursorへの登録時に設定ファイルが見つからない

**解決策**:
1. Cursorを起動
2. Cline拡張機能をインストール
3. Clineを一度使用
4. MCP Assistant で **Refresh Status**
5. 📂ボタンで実際のパスを確認

### 2. 旧インストールが残っている

**症状**: 旧パスのインストールが削除されない

**解決策**:
1. 旧パスを手動で削除
2. または旧パスをカスタムパスとして設定し続ける

### 3. AIツールが認識しない

**症状**: 登録後もAIツールが認識しない

**解決策**:
1. AIツールを完全に再起動
2. 📂ボタンで設定ファイルを確認
3. パスが正しいか確認

---

## 🔮 今後の予定

### v1.6.1 (パッチ)
- ☐ バグ修正
- ☐ ドキュメント改善
- ☐ パフォーマンス最適化

### v1.7.0 (次期メジャー)
- ☐ 複数インスタンス対応
- ☐ プロファイル管理
- ☐ 高度な設定エディタ
- ☐ 自動アップデート機能

### v2.0.0 (将来)
- ☐ プラグインシステム
- ☐ カスタムツール開発API
- ☐ GUIベースの設定エディタ

---

## 📚 参考ドキュメント

### 新機能ドキュメント
- `docs/JSON_CONFIG_REGISTRATION.md` - JSON設定ファイル登録ガイド
- `docs/CURSOR_CONFIG_FIX.md` - Cursor設定検出の改善
- `docs/USER_CONFIGURABLE_INSTALL_PATH.md` - インストールパスのカスタマイズ
- `docs/MCP_SERVER_MANAGEMENT_COMPLETED.md` - サーバー管理機能完了レポート
- `docs/MCP_BRIDGE_INTEGRATION_REPORT.md` - Bridge統合サマリー

### 既存ドキュメント
- `README.md` - プロジェクト概要
- `README_ja.md` - プロジェクト概要（日本語）
- `CHANGELOG.md` - 変更履歴
- `CLAUDE.md` - API リファレンス

---

## 🎉 謝辞

このリリースは、ユーザーフィードバックと継続的な改善の結果です。以下の点で大きな進歩を遂げました：

- 🎯 **使いやすさ**: Unity EditorからすべてのMCP管理が可能
- 🚀 **信頼性**: CLI依存を排除し、直接的な設定管理
- 🔧 **柔軟性**: 完全にカスタマイズ可能なインストールパス
- 📈 **保守性**: 明確なアーキテクチャと詳細なドキュメント

皆様のフィードバックをお待ちしております！

---

## 🔗 リンク

- **GitHub**: https://github.com/kuroyasouiti/SkillForUnity
- **Issues**: https://github.com/kuroyasouiti/SkillForUnity/issues
- **Releases**: https://github.com/kuroyasouiti/SkillForUnity/releases/tag/v1.6.0

---

**リリース日**: 2025-11-27  
**バージョン**: 1.6.0  
**ステータス**: ✅ 安定版

