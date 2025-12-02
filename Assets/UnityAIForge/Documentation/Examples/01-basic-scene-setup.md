# 例題1: 基本的な3Dシーンセットアップ

**目標**: プレイヤー、地面、カメラ、ライティングを含むシンプルな3Dゲームシーンを作成する。

**難易度**: 初級
**所要時間**: 5分

## 前提条件

- Unity Editor 2021.3以降
- MCP Bridgeが実行中（Tools > MCP Assistant > Start Bridge）
- MCPクライアントが接続済み

## 作成するもの

- 適切なライティングを持つ3Dシーン
- スポーン地点のプレイヤーカプセル
- 地面プレーン
- ディレクショナルライト
- 正しく配置されたメインカメラ

## ステップバイステップガイド

### 1. シーンのセットアップ

まず、デフォルト設定で新しい3Dシーンを作成します：

```python
unity_scene_quickSetup({
    "setupType": "3D"
})
```

これにより自動的に以下が作成されます：
- 位置(0, 1, -10)のメインカメラ
- デフォルト強度のディレクショナルライト

### 2. 地面の作成

地面用の大きなプレーンを追加します：

```python
unity_gameobject_createFromTemplate({
    "template": "Plane",
    "name": "Ground",
    "position": {"x": 0, "y": 0, "z": 0},
    "scale": {"x": 10, "y": 1, "z": 10}
})
```

### 3. プレイヤーの作成

プレイヤーを表すカプセルを追加します：

```python
unity_gameobject_createFromTemplate({
    "template": "Player",
    "name": "Player",
    "position": {"x": 0, "y": 1, "z": 0}
})
```

Playerテンプレートには以下が含まれます：
- カプセルメッシュ
- カプセルコライダー
- Rigidbody（物理演算用）

### 4. 障害物の追加

いくつかのキューブ障害物を作成します：

```python
unity_gameobject_createFromTemplate({
    "template": "Cube",
    "name": "Obstacle1",
    "position": {"x": 3, "y": 0.5, "z": 0}
})

unity_gameobject_createFromTemplate({
    "template": "Cube",
    "name": "Obstacle2",
    "position": {"x": -3, "y": 0.5, "z": 0}
})

unity_gameobject_createFromTemplate({
    "template": "Cube",
    "name": "Obstacle3",
    "position": {"x": 0, "y": 0.5, "z": 3}
})
```

### 5. シーンの確認

階層を確認して、すべてが作成されたことを確認します：

```python
unity_context_inspect({
    "includeHierarchy": True,
    "maxDepth": 2
})
```

## 期待される結果

シーンの階層は以下のようになります：

```
Main Camera
Directional Light
Ground
Player
Obstacle1
Obstacle2
Obstacle3
```

シーンビューには以下が表示されます：
- 大きな灰色のプレーン（地面）
- 中央のカプセル（プレイヤー）
- プレイヤーの周りに配置された3つのキューブ
- ディレクショナルライトからの良好なライティング

## 次のステップ

以下の拡張を試してみましょう：

1. **色の追加**: マテリアルを作成してオブジェクトに適用
2. **カメラの調整**: プレイヤーを追従するようにカメラを配置
3. **物理の追加**: 障害物を落下させたり、衝突に反応させる
4. **スクリプトの追加**: プレイヤーに移動スクリプトをアタッチ

## よくある問題

**問題**: オブジェクトが暗すぎる
- **解決策**: ディレクショナルライトの強度を調整：
  ```python
  unity_component_crud({
      "operation": "update",
      "gameObjectPath": "Directional Light",
      "componentType": "UnityEngine.Light",
      "propertyChanges": {
          "intensity": 1.5
      }
  })
  ```

**問題**: プレイヤーが地面を突き抜ける
- **解決策**: 地面にコライダーがあることを確認：
  ```python
  unity_component_crud({
      "operation": "add",
      "gameObjectPath": "Ground",
      "componentType": "UnityEngine.MeshCollider"
  })
  ```

## 関連する例題

- [02-ui-creation.md](02-ui-creation.md) - このシーンにUIを追加
- [03-game-level.md](03-game-level.md) - 完全なゲームレベルに拡張
