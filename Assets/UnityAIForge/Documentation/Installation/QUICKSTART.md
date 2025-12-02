# Unity-AI-Forge - クイックスタートガイド

**5分でUnity-AI-Forgeを始めましょう！**

## 前提条件

- ✅ Unity Editor 2022.3以降（2021.3以降サポート）
- ✅ Python 3.10以降
- ✅ uvパッケージマネージャー（推奨）

## ステップ1: Unityパッケージのインストール（1分）

### オプションA: Unity Package Manager経由（推奨）

1. Unity Editorを開く
2. **Window > Package Manager**を開く
3. **+（プラス）**ボタンをクリック → **Add package from git URL...**
4. 入力: `https://github.com/kuroyasouiti/Unity-AI-Forge.git?path=/Assets/UnityAIForge`
5. **Add**をクリック

### オプションB: 手動インストール

1. リポジトリをダウンロード
2. `Assets/UnityAIForge`をUnityプロジェクトの`Assets/`フォルダにコピー

## ステップ2: MCPサーバーのインストール（2分）

### オプションA: 自動インストール（推奨）

1. Unity Editorで**Tools > Unity-AI-Forge > MCP Server Manager**に移動
2. **Install Server**をクリック（`~/Unity-AI-Forge`にインストール）
3. AIツール（Cursor、Claude Desktop、Cline、Windsurf）用に**Register**をクリック
4. AIツールを再起動

### オプションB: 手動セットアップ

```bash
# Windows (PowerShell)
xcopy /E /I /Y "Assets\UnityAIForge\MCPServer" "%USERPROFILE%\Unity-AI-Forge"
cd %USERPROFILE%\Unity-AI-Forge
uv sync

# macOS/Linux
cp -r Assets/UnityAIForge/MCPServer ~/Unity-AI-Forge
cd ~/Unity-AI-Forge
uv sync
```

## ステップ3: Unity Bridgeの起動（30秒）

1. Unity Editorで**Tools > Unity-AI-Forge > MCP Assistant**に移動
2. **Start Bridge**をクリック
3. ステータスが"Connected"と表示されることを確認

💡 ブリッジはデフォルトで`ws://localhost:7077/bridge`をリッスンします。

## ステップ4: MCPクライアントの設定（手動セットアップの場合）

**注意:** ステップ2で自動インストールを使用した場合は、この設定は既に完了しています！

### Claude Desktopの場合

設定ファイルの場所:
- Windows: `%APPDATA%\Claude\claude_desktop_config.json`
- macOS: `~/Library/Application Support/Claude/claude_desktop_config.json`
- Linux: `~/.config/Claude/claude_desktop_config.json`

以下の設定を追加:
```json
{
  "mcpServers": {
    "unity-ai-forge": {
      "command": "uv",
      "args": [
        "--directory",
        "C:/Users/YOUR_USERNAME/Unity-AI-Forge",
        "run",
        "unity-ai-forge"
      ]
    }
  }
}
```

`C:/Users/YOUR_USERNAME`を実際のホームディレクトリパスに置き換えてください。

macOS/Linuxの場合:
```json
{
  "mcpServers": {
    "unity-ai-forge": {
      "command": "uv",
      "args": [
        "--directory",
        "/Users/YOUR_USERNAME/Unity-AI-Forge",
        "run",
        "unity-ai-forge"
      ]
    }
  }
}
```

### Cursorの場合

設定ファイルは通常以下にあります: `%APPDATA%\Cursor\User\globalStorage\saoudrizwan.claude-dev\settings\cline_mcp_settings.json`

Claude Desktopと同様の設定を使用してください。

### その他のツール

ClineとWindsurfの設定については[INSTALL_GUIDE.md](INSTALL_GUIDE.md)を参照してください。

## ステップ5: 接続のテスト

MCPクライアント（Claude Desktop、Cursorなど）を開いて試してください:

```
Unity MCP接続をテストしてもらえますか？
```

AIは`unity_ping()`を呼び出し、Unityのバージョン情報を表示します。

## 最初のコマンド

### 3Dシーンの作成

```
プレイヤーと地面のある3Dゲームシーンを作成してください。
```

これにより:
- カメラとライトを備えた3Dシーンをセットアップ
- 地面プレーンを作成
- プレイヤーカプセルを追加

### UIメニューの作成

```
Play、Settings、Quitボタンのあるメインメニューを作成してください。
```

これにより:
- CanvasとEventSystemをセットアップ
- メニューパネルを作成
- 3つのスタイル付きボタンを追加

### シーンの検査

```
現在のシーンにはどんなGameObjectがありますか？
```

これによりシーン階層とすべてのGameObjectが表示されます。

## よく使うコマンドリファレンス

| タスク | コマンド例 |
|------|-----------------|
| シーン作成 | "3Dシーンをセットアップしてください" |
| GameObject作成 | "位置(0, 1, 0)にCubeを作成してください" |
| コンポーネント追加 | "PlayerにRigidbodyを追加してください" |
| UI作成 | "「Start Game」というテキストのボタンを作成してください" |
| ScriptableObject作成 | "maxPlayers=4のGameConfig ScriptableObjectを作成してください" |
| GameObjectリスト | "シーン内のすべてのGameObjectを表示してください" |
| バッチ操作 | "一列に10個のCubeを作成してください" |

## ツールリファレンス

### 最もよく使われるツール

**シーン管理:**
```python
unity_scene_quickSetup({"setupType": "3D"})  # または "2D"、"UI"
unity_scene_crud({"operation": "create", "scenePath": "Assets/Scenes/Level1.unity"})
```

**GameObject作成:**
```python
unity_gameobject_createFromTemplate({
    "template": "Cube",  # またはSphere、Player、Enemyなど
    "name": "MyObject",
    "position": {"x": 0, "y": 1, "z": 0}
})
```

**UI作成:**
```python
unity_ugui_createFromTemplate({
    "template": "Button",  # またはText、Panel、Imageなど
    "text": "Click Me!",
    "width": 200,
    "height": 50
})
```

**コンポーネント管理:**
```python
unity_component_crud({
    "operation": "add",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Rigidbody"
})
```

**シーン検査:**
```python
unity_scene_crud({
    "operation": "inspect",
    "includeHierarchy": True,
    "includeComponents": True,
    "filter": "Player*"
})
```

**ScriptableObject管理:**
```python
# ScriptableObjectを作成
unity_scriptableObject_crud({
    "operation": "create",
    "typeName": "MyGame.GameConfig",
    "assetPath": "Assets/Data/Config.asset",
    "properties": {
        "maxPlayers": 4,
        "gameSpeed": 1.0
    }
})

# 既存のScriptableObjectを検査
unity_scriptableObject_crud({
    "operation": "inspect",
    "assetPath": "Assets/Data/Config.asset",
    "includeProperties": True
})

# プロパティを更新
unity_scriptableObject_crud({
    "operation": "update",
    "assetPath": "Assets/Data/Config.asset",
    "properties": {
        "maxPlayers": 8
    }
})

# 特定の型のすべてのScriptableObjectを検索
unity_scriptableObject_crud({
    "operation": "findByType",
    "typeName": "MyGame.GameConfig",
    "includeProperties": True
})
```

## 次のステップ

### さらに学ぶ

- 📖 **[README.md](README.md)** - 完全なMCPサーバードキュメント
- 📋 **[INSTALL_GUIDE.md](INSTALL_GUIDE.md)** - 詳細なインストール手順
- 🎮 **[examples/](examples/)** - 実践的なチュートリアル
- 📚 **[プロジェクトREADME](../../README.md)** - 完全なプロジェクトドキュメント

### これらの例を試してみましょう

1. **[基本シーンセットアップ](examples/01-basic-scene-setup.md)** - 最初のゲームシーンを作成
2. **[UI作成](examples/02-ui-creation.md)** - 完全なメニューシステムを構築
3. **[ゲームレベル](examples/03-game-level.md)** - ゲームレベルをデザイン
4. **[Prefabワークフロー](examples/04-prefab-workflow.md)** - Prefabを使って作業
5. **[デザインパターン](examples/05-design-patterns.md)** - デザインパターンコードを生成

### ベストプラクティス

✅ **推奨:**
- 利用可能な場合はテンプレートを使用（`createFromTemplate`）
- 変更を加える前にシーンコンテキストを確認（`unity_scene_crud`で`operation="inspect"`）
- 類似の複数タスクにはバッチ操作を使用
- 完全なコンポーネント型名を指定（例: `UnityEngine.Rigidbody`）

❌ **非推奨:**
- テンプレートが存在する場合に手動でGameObjectを作成
- バッチ操作の代わりに多数の個別呼び出し
- ツール使用前にUnity Bridgeの起動を忘れる

## トラブルシューティング

### Unity Bridgeが接続されない

**問題:** ツールが"Unity bridge is not connected"で失敗する

**解決策:**
1. Unity Editorを開く
2. Tools > MCP Assistantに移動
3. "Start Bridge"をクリック
4. "Connected"ステータスを待つ

### コマンドがタイムアウトする

**問題:** コマンドに時間がかかりすぎてタイムアウトする

**解決策:**
- MCP設定でタイムアウトを増やす
- Unityがスクリプトをコンパイルしていないか確認
- より軽い検査操作を使用（`includeProperties: false`）

### GameObjectが見つからない

**問題:** "GameObject not found"エラー

**解決策:**
1. `unity_scene_crud({"operation": "inspect"})`を使用して存在するものを確認
2. GameObjectパスが正しいか確認（大文字小文字区別）
3. GameObjectがアクティブなシーンにあることを確認

## ヘルプの入手

- 🐛 **問題報告**: [GitHub Issues](https://github.com/kuroyasouiti/Unity-AI-Forge/issues)
- 📖 **ドキュメント**: [README.md](README.md)と[INSTALL_GUIDE.md](INSTALL_GUIDE.md)
- 💬 **Examples**: 実践的なガイドについては[examples/](examples/)を確認

---

**AI支援で素晴らしいUnityプロジェクトを構築する準備ができました！** 🚀
