# 例題4: Prefabワークフロー - アセットの作成、変更、再利用

この例では、Unity-AI-ForgeでUnity Prefabを操作する方法を示します。シーンのGameObjectからPrefabを作成し、シーンにインスタンス化し、変更を加え、Prefabオーバーライドを管理する方法を学びます。

## 学ぶこと

- シーンのGameObjectからPrefabを作成
- Prefabをシーンにインスタンス化
- Prefabインスタンスの変更
- インスタンスオーバーライドの適用と復帰
- Prefabインスタンスのアンパック
- Prefabアセットの検査

## 前提条件

- MCP Bridgeが接続されたUnity Editorが実行中
- 例題01-03を完了（または基本的なUnity知識）

## ステップ1: 再利用可能な敵Prefabの作成

まず、Prefabに変換する敵GameObjectを作成します：

```python
# 3Dシーンをセットアップ
unity_scene_quickSetup({"setupType": "3D"})

# テンプレートを使用して敵GameObjectを作成
unity_gameobject_createFromTemplate({
    "template": "Capsule",
    "name": "Enemy_Template",
    "position": {"x": 0, "y": 1, "z": 0}
})

# コンポーネントと子オブジェクトでカスタマイズ
unity_template_manage({
    "operation": "customize",
    "gameObjectPath": "Enemy_Template",
    "components": [
        {
            "type": "UnityEngine.CapsuleCollider",
            "properties": {
                "height": 2.0,
                "radius": 0.5
            }
        },
        {
            "type": "UnityEngine.Rigidbody",
            "properties": {
                "mass": 2.0,
                "useGravity": True,
                "constraints": 112  # X、Z軸の回転を固定
            }
        }
    ],
    "children": [
        {
            "name": "HealthBar",
            "components": [
                {
                    "type": "UnityEngine.Canvas"
                }
            ]
        },
        {
            "name": "DetectionZone",
            "components": [
                {
                    "type": "UnityEngine.SphereCollider",
                    "properties": {
                        "isTrigger": True,
                        "radius": 3.0
                    }
                }
            ]
        }
    ]
})
```

## ステップ2: Prefabアセットの作成

次にGameObjectをPrefabに変換します：

```python
# Enemy_Template GameObjectからPrefabを作成
unity_prefab_crud({
    "operation": "create",
    "gameObjectPath": "Enemy_Template",
    "prefabPath": "Assets/Prefabs/Enemy.prefab"
})

# Prefabを検査して正しく作成されたことを確認
unity_prefab_crud({
    "operation": "inspect",
    "prefabPath": "Assets/Prefabs/Enemy.prefab"
})
```

## ステップ3: 複数のPrefabインスタンスのインスタンス化

敵Prefabのいくつかのインスタンスを作成します：

```python
# すべての敵用のコンテナを作成
unity_gameobject_crud({
    "operation": "create",
    "name": "Enemies"
})

# バッチ操作を使用して異なる位置に敵をインスタンス化
positions = [
    {"x": 5, "y": 1, "z": 5},
    {"x": -5, "y": 1, "z": 5},
    {"x": 5, "y": 1, "z": -5},
    {"x": -5, "y": 1, "z": -5},
    {"x": 0, "y": 1, "z": 7}
]

# 複数のインスタンスを作成
for i, pos in enumerate(positions):
    unity_prefab_crud({
        "operation": "instantiate",
        "prefabPath": "Assets/Prefabs/Enemy.prefab",
        "parentPath": "Enemies"
    })

    # 各インスタンスの名前変更と位置設定
    unity_gameobject_crud({
        "operation": "rename",
        "gameObjectPath": f"Enemies/Enemy(Clone)",
        "name": f"Enemy_{i+1}"
    })

    unity_component_crud({
        "operation": "update",
        "gameObjectPath": f"Enemies/Enemy_{i+1}",
        "componentType": "UnityEngine.Transform",
        "propertyChanges": {
            "position": pos
        }
    })
```

## ステップ4: Prefabインスタンスの変更

特定のインスタンスに固有の変更を加えます：

```python
# Enemy_1を大きく重くする（ボス敵）
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Enemies/Enemy_1",
    "componentType": "UnityEngine.Transform",
    "propertyChanges": {
        "scale": {"x": 1.5, "y": 1.5, "z": 1.5}
    }
})

unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Enemies/Enemy_1",
    "componentType": "UnityEngine.Rigidbody",
    "propertyChanges": {
        "mass": 5.0
    }
})

# Enemy_2を速くする（偵察敵）
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Enemies/Enemy_2",
    "componentType": "UnityEngine.Rigidbody",
    "propertyChanges": {
        "mass": 1.0
    }
})
```

## ステップ5: インスタンスオーバーライドをPrefabに適用

Enemy_1からの変更をPrefabに保存したい場合：

```python
# Enemy_1からのオーバーライドをEnemyPrefabに適用
unity_prefab_crud({
    "operation": "applyOverrides",
    "gameObjectPath": "Enemies/Enemy_1"
})

# これ以降のすべてのインスタンスが大きなスケールと重い質量を持つようになります
```

## ステップ6: インスタンスオーバーライドの復帰

インスタンスをPrefabの元の状態にリセットしたい場合：

```python
# Enemy_2をPrefabのデフォルトに戻す
unity_prefab_crud({
    "operation": "revertOverrides",
    "gameObjectPath": "Enemies/Enemy_2"
})

# Enemy_2は再びPrefabと同一になります
```

## ステップ7: Prefabアセットを直接更新

Prefabアセット自体を変更します：

```python
# まず、元のテンプレートGameObjectを変更しましょう
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Enemy_Template",
    "componentType": "UnityEngine.CapsuleCollider",
    "propertyChanges": {
        "radius": 0.6  # コライダーを少し大きくする
    }
})

# テンプレートからの変更でPrefabアセットを更新
unity_prefab_crud({
    "operation": "update",
    "gameObjectPath": "Enemy_Template",
    "prefabPath": "Assets/Prefabs/Enemy.prefab"
})

# オーバーライドのないすべてのインスタンスが自動的に更新されます！
```

## ステップ8: Prefabインスタンスのアンパック

Prefabインスタンスを通常のGameObjectに戻します：

```python
# Enemy_3をアンパック（Prefab接続を削除）
unity_prefab_crud({
    "operation": "unpack",
    "gameObjectPath": "Enemies/Enemy_3",
    "unpackMode": "Completely"
})

# Enemy_3は通常のGameObjectになり、Prefabに接続されていません
# Prefabへの変更はもう影響しません
```

## ステップ9: ネストされたPrefabの作成

武器Prefabを作成して敵に追加します：

```python
# 武器GameObjectを作成
unity_gameobject_createFromTemplate({
    "template": "Cube",
    "name": "Sword",
    "scale": {"x": 0.2, "y": 1.0, "z": 0.1}
})

# 武器Prefabを作成
unity_prefab_crud({
    "operation": "create",
    "gameObjectPath": "Sword",
    "prefabPath": "Assets/Prefabs/Sword.prefab"
})

# 敵テンプレートの子として剣をインスタンス化
unity_prefab_crud({
    "operation": "instantiate",
    "prefabPath": "Assets/Prefabs/Sword.prefab",
    "parentPath": "Enemy_Template"
})

# 剣の位置を設定
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Enemy_Template/Sword(Clone)",
    "componentType": "UnityEngine.Transform",
    "propertyChanges": {
        "position": {"x": 0.5, "y": 1, "z": 0},
        "rotation": {"x": 0, "y": 0, "z": 45}
    }
})

# 敵Prefabを更新して剣を含める
unity_prefab_crud({
    "operation": "update",
    "gameObjectPath": "Enemy_Template",
    "prefabPath": "Assets/Prefabs/Enemy.prefab"
})

# 新しい敵インスタンスはすべて剣を持つようになります！
```

## ステップ10: 検査と確認

Prefab構造とインスタンスを確認します：

```python
# 敵Prefabを検査
unity_prefab_crud({
    "operation": "inspect",
    "prefabPath": "Assets/Prefabs/Enemy.prefab"
})

# シーン内の敵Prefabのすべてのインスタンスを検索
unity_gameobject_crud({
    "operation": "findMultiple",
    "pattern": "Enemies/Enemy*",
    "maxResults": 20
})

# 詳細付きで特定のインスタンスを検査
unity_gameobject_crud({
    "operation": "inspect",
    "gameObjectPath": "Enemies/Enemy_1",
    "includeProperties": True
})
```

## 完全なPrefabワークフローの例

すべての概念を組み合わせた完全なスクリプトです：

```python
# 1. テンプレートを作成
unity_gameobject_createFromTemplate({
    "template": "Cube",
    "name": "Powerup_Template",
    "scale": {"x": 0.5, "y": 0.5, "z": 0.5}
})

# 2. コンポーネントを追加
unity_component_crud({
    "operation": "add",
    "gameObjectPath": "Powerup_Template",
    "componentType": "UnityEngine.SphereCollider",
    "propertyChanges": {"isTrigger": True}
})

# 3. Prefabを作成
unity_prefab_crud({
    "operation": "create",
    "gameObjectPath": "Powerup_Template",
    "prefabPath": "Assets/Prefabs/Powerup.prefab"
})

# 4. 複数回インスタンス化
for i in range(5):
    unity_prefab_crud({
        "operation": "instantiate",
        "prefabPath": "Assets/Prefabs/Powerup.prefab"
    })

# 5. 1つのインスタンスを変更
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Powerup(Clone)",
    "componentType": "UnityEngine.Transform",
    "propertyChanges": {
        "scale": {"x": 1.0, "y": 1.0, "z": 1.0}
    }
})

# 6. 変更をPrefabに適用
unity_prefab_crud({
    "operation": "applyOverrides",
    "gameObjectPath": "Powerup(Clone)"
})
```

## ベストプラクティス

1. **繰り返しオブジェクトには常にPrefabを使用**
   - 敵、収集アイテム、UI要素、環境小道具
   - メンテナンスと更新が容易

2. **Prefabをフォルダに整理**
   - `Assets/Prefabs/Characters/`
   - `Assets/Prefabs/Environment/`
   - `Assets/Prefabs/UI/`

3. **ネストされたPrefabを使用**
   - 複雑なオブジェクトを小さなPrefabコンポーネントに分割
   - パーツの再利用が容易（例：異なる車両のホイール）

4. **適用と復帰を慎重に使用**
   - `applyOverrides`: 変更をPrefabに保存（すべてのインスタンスに影響）
   - `revertOverrides`: 変更を破棄（Prefabにリセット）

5. **必要な場合のみアンパック**
   - アンパックするとPrefab接続が切れます
   - ユニークな一回限りのバリエーションが必要な場合のみアンパック

## パフォーマンスのヒント

多くのPrefabを扱う場合：

```python
# 検査を高速化するためにincludeProperties=falseを使用
unity_gameobject_crud({
    "operation": "inspectMultiple",
    "pattern": "Enemies/*",
    "includeComponents": False,
    "maxResults": 100
})

# 複数のPrefabをインスタンス化
for i in range(5):
    unity_prefab_crud({
        "operation": "instantiate",
        "prefabPath": "Assets/Prefabs/Enemy.prefab",
        "parentPath": "Enemies"
    })
```

## よくある問題

**問題**: Prefabへの変更がインスタンスに影響しない
**解決策**: インスタンスにオーバーライドがないことを確認。まず`revertOverrides`を使用。

**問題**: Prefabインスタンスを変更できない
**解決策**: インスタンスがまだPrefabに接続されているか確認（アンパックされていない）。

**問題**: ネストされたPrefabの変更が失われる
**解決策**: 最も内側から最も外側のPrefabへとオーバーライドを適用。

---

**関連項目:**
- [01-basic-scene-setup.md](01-basic-scene-setup.md) - 基本的なシーン作成
- [02-ui-creation.md](02-ui-creation.md) - UI Prefabワークフロー
- [03-game-level.md](03-game-level.md) - レベルでのPrefab使用
- CLAUDE.md - API詳細についてのPrefab管理セクション
