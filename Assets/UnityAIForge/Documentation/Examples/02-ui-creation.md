# 例題2: UIメニュー作成

**目標**: ボタン、タイトル、パネルを含む完全なメインメニューUIを作成する。

**難易度**: 初級
**所要時間**: 10分

## 前提条件

- Unity Editor 2021.3以降
- MCP Bridgeが実行中
- Unity UI（Canvas、RectTransform）の基本的な理解

## 作成するもの

- 適切な設定のCanvas
- 半透明の背景パネル
- タイトルテキスト
- 3つのメニューボタン（Play、Settings、Quit）
- スペーシングを含む適切なレイアウト

## ステップバイステップガイド

### 1. UIシーンのセットアップ

UIコンポーネントを含む新しいシーンを作成します：

```python
unity_scene_quickSetup({
    "setupType": "UI"
})
```

これにより以下が作成されます：
- Canvas（レスポンシブデザイン用のCanvas Scaler付き）
- EventSystem（UI入力の処理用）

### 2. 背景パネルの作成

背景として半透明のパネルを追加します：

```python
unity_ugui_createFromTemplate({
    "template": "Panel",
    "name": "MenuPanel",
    "parentPath": "Canvas",
    "width": 400,
    "height": 600
})
```

### 3. パネルの色を更新

パネルを半透明の暗い色にします：

```python
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Canvas/MenuPanel",
    "componentType": "UnityEngine.UI.Image",
    "propertyChanges": {
        "color": {
            "r": 0.0,
            "g": 0.0,
            "b": 0.0,
            "a": 0.8
        }
    }
})
```

### 4. タイトルテキストの追加

パネル上部にタイトルを作成します：

```python
unity_ugui_createFromTemplate({
    "template": "Text",
    "name": "TitleText",
    "parentPath": "Canvas/MenuPanel",
    "text": "Main Menu",
    "fontSize": 48,
    "width": 350,
    "height": 80,
    "anchorPreset": "top-center"
})
```

### 5. タイトルの位置調整

タイトルの位置を調整します：

```python
unity_ugui_manage({
    "operation": "updateRect",
    "gameObjectPath": "Canvas/MenuPanel/TitleText",
    "anchoredPositionX": 0,
    "anchoredPositionY": -50
})
```

### 6. ボタンコンテナの作成

ボタンを整理するための垂直レイアウトグループを追加します：

```python
unity_gameobject_crud({
    "operation": "create",
    "name": "ButtonContainer",
    "parentPath": "Canvas/MenuPanel"
})

unity_ugui_layoutManage({
    "operation": "add",
    "gameObjectPath": "Canvas/MenuPanel/ButtonContainer",
    "layoutType": "VerticalLayoutGroup",
    "spacing": 20,
    "padding": {
        "left": 50,
        "right": 50,
        "top": 150,
        "bottom": 50
    },
    "childControlWidth": True,
    "childControlHeight": False,
    "childForceExpandWidth": True
})
```

### 7. メニューボタンの作成

適切なスタイリングで3つのボタンを追加します：

```python
# Playボタン
unity_ugui_createFromTemplate({
    "template": "Button",
    "name": "PlayButton",
    "parentPath": "Canvas/MenuPanel/ButtonContainer",
    "text": "Play",
    "width": 300,
    "height": 60
})

# Settingsボタン
unity_ugui_createFromTemplate({
    "template": "Button",
    "name": "SettingsButton",
    "parentPath": "Canvas/MenuPanel/ButtonContainer",
    "text": "Settings",
    "width": 300,
    "height": 60
})

# Quitボタン
unity_ugui_createFromTemplate({
    "template": "Button",
    "name": "QuitButton",
    "parentPath": "Canvas/MenuPanel/ButtonContainer",
    "text": "Quit",
    "width": 300,
    "height": 60
})
```

### 8. ボタンテキストサイズの更新

ボタンテキストを読みやすくします：

```python
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Canvas/MenuPanel/ButtonContainer/PlayButton/Text",
    "componentType": "UnityEngine.UI.Text",
    "propertyChanges": {
        "fontSize": 24,
        "alignment": "MiddleCenter"
    }
})

unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Canvas/MenuPanel/ButtonContainer/SettingsButton/Text",
    "componentType": "UnityEngine.UI.Text",
    "propertyChanges": {
        "fontSize": 24,
        "alignment": "MiddleCenter"
    }
})

unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Canvas/MenuPanel/ButtonContainer/QuitButton/Text",
    "componentType": "UnityEngine.UI.Text",
    "propertyChanges": {
        "fontSize": 24,
        "alignment": "MiddleCenter"
    }
})
```

### 9. パネルを中央に配置

パネルが画面中央にあることを確認します：

```python
unity_ugui_manage({
    "operation": "setAnchorPreset",
    "gameObjectPath": "Canvas/MenuPanel",
    "preset": "center"
})
```

## 期待される結果

UI階層は以下のようになります：

```
Canvas
└── MenuPanel
    ├── TitleText
    └── ButtonContainer
        ├── PlayButton
        │   └── Text
        ├── SettingsButton
        │   └── Text
        └── QuitButton
            └── Text
```

ゲームビューには以下が表示されます：
- 中央に配置された暗いパネル（400x600）
- 上部の「Main Menu」タイトル
- 下に等間隔で配置された3つのボタン
- 幅いっぱいに広がるボタン

## 次のステップ - 機能の追加

### オプション1: クリックイベントの追加（手動）

Unityで手動でクリックイベントを追加できます：
1. ボタンを選択
2. InspectorでButtonコンポーネントを見つける
3. OnClickセクションで+をクリック
4. スクリプトのGameObjectをドラッグ
5. 呼び出すメソッドを選択

### オプション2: メニューマネージャースクリプトの作成

ボタンクリックを処理するスクリプトを作成します：

```python
# asset_crudを使用してMenuManagerスクリプトを作成
unity_asset_crud({
    "operation": "create",
    "assetPath": "Assets/Scripts/MenuManager.cs",
    "content": '''using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void OnPlayClicked()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnSettingsClicked()
    {
        Debug.Log("Settings clicked");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
'''
})
```

## よくある問題

**問題**: ボタンがクリックに反応しない
- **解決策**: 階層にEventSystemが存在することを確認

**問題**: UIが異なる画面で小さすぎる/大きすぎる
- **解決策**: Canvas Scalerはレスポンシブデザイン用に既に設定されています

**問題**: テキストがぼやける
- **解決策**: Canvas Scalerの参照解像度を上げるか、TextMeshProを使用

## 強化

以下の改善を試してみましょう：

1. **ホバーエフェクトの追加**: Inspectorでボタン遷移を使用
2. **アイコンの追加**: スプライトをインポートしてボタンに追加
3. **トランジションアニメーション**: Unityのアニメーションシステムを使用
4. **サウンドエフェクトの追加**: ボタンにAudioSourceをアタッチ

## 関連する例題

- [01-basic-scene-setup.md](01-basic-scene-setup.md) - ゲームシーンにこのUIを追加
- [04-prefab-workflow.md](04-prefab-workflow.md) - このメニューを再利用可能なPrefabに変換
