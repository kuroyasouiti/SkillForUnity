# 例題3: 完全なゲームレベルの構築

この例では、Unity-AI-Forgeを使用して、地形、プレイヤー、敵、収集アイテム、環境オブジェクトを含む完全な3Dゲームレベルを作成する方法を示します。

## 作成するもの

- 物理演算対応の地面プレーン
- 移動スクリプト付きプレイヤーキャラクター
- 複数の敵NPC
- 収集可能なアイテム
- 環境障害物
- 基本的なライティング設定

## 前提条件

- MCP Bridgeが接続されたUnity Editorが実行中
- UnityのGameObjectとコンポーネントの基本的な理解

## ステップ1: シーンのセットアップ

まず、カメラとライティングを含む3Dシーンをセットアップします：

```python
# カメラとディレクショナルライトを含む新しい3Dシーンを作成
unity_scene_quickSetup({"setupType": "3D"})

# より良いビューのためにカメラ位置を調整
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Main Camera",
    "componentType": "UnityEngine.Transform",
    "propertyChanges": {
        "position": {"x": 0, "y": 8, "z": -10},
        "rotation": {"x": 30, "y": 0, "z": 0}
    }
})
```

## ステップ2: 地面の作成

物理演算対応の大きな地面プレーンを作成します：

```python
# 地面を作成
unity_gameobject_createFromTemplate({
    "template": "Plane",
    "name": "Ground",
    "position": {"x": 0, "y": 0, "z": 0},
    "scale": {"x": 10, "y": 1, "z": 10}
})

# 物理インタラクション用のコライダーを追加
unity_component_crud({
    "operation": "add",
    "gameObjectPath": "Ground",
    "componentType": "UnityEngine.MeshCollider"
})
```

## ステップ3: プレイヤーの作成

カプセル形状のプレイヤーキャラクターを作成します：

```python
# テンプレートを使用してプレイヤーを作成
unity_gameobject_createFromTemplate({
    "template": "Player",
    "name": "Player",
    "position": {"x": 0, "y": 1, "z": 0}
})

# 物理コンポーネントを追加
unity_component_crud({
    "operation": "add",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Rigidbody",
    "propertyChanges": {
        "mass": 1.0,
        "useGravity": True,
        "constraints": 112  # X軸とZ軸の回転を固定
    }
})

unity_component_crud({
    "operation": "add",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.CapsuleCollider",
    "propertyChanges": {
        "height": 2.0,
        "radius": 0.5,
        "center": {"x": 0, "y": 1, "z": 0}
    }
})
```

## ステップ4: プレイヤー移動スクリプトの作成

シンプルなプレイヤー移動スクリプトを作成します：

```python
player_script = """using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 入力を取得
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // プレイヤーを移動
        Vector3 movement = new Vector3(horizontal, 0f, vertical) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // ジャンプ
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
"""

# asset_crudを使用してスクリプトを作成
unity_asset_crud({
    "operation": "create",
    "assetPath": "Assets/Scripts/PlayerMovement.cs",
    "content": player_script
})

# コンポーネントとして追加する前にUnityがスクリプトをコンパイルするのを待つ
unity_component_crud({
    "operation": "add",
    "gameObjectPath": "Player",
    "componentType": "PlayerMovement",
    "propertyChanges": {
        "moveSpeed": 5.0,
        "jumpForce": 5.0
    }
})

# Groundタグを設定
unity_tagLayer_manage({
    "operation": "addTag",
    "tag": "Ground"
})

unity_tagLayer_manage({
    "operation": "setTag",
    "gameObjectPath": "Ground",
    "tag": "Ground"
})
```

## ステップ5: 敵の作成

テンプレートカスタマイズを使用して複数の敵NPCを作成します：

```python
# 敵コンテナを作成
unity_gameobject_crud({
    "operation": "create",
    "name": "Enemies"
})

# コンポーネント付きの複数の敵を作成
enemy_positions = [
    {"name": "Enemy1", "x": 5, "y": 1, "z": 5},
    {"name": "Enemy2", "x": -5, "y": 1, "z": 5},
    {"name": "Enemy3", "x": 5, "y": 1, "z": -5},
    {"name": "Enemy4", "x": -5, "y": 1, "z": -5}
]

for enemy_data in enemy_positions:
    # テンプレートを使用して敵を作成
    unity_gameobject_createFromTemplate({
        "template": "Capsule",
        "name": enemy_data["name"],
        "parentPath": "Enemies"
    })

    # コンポーネントと位置でカスタマイズ
    unity_template_manage({
        "operation": "customize",
        "gameObjectPath": f"Enemies/{enemy_data['name']}",
        "components": [
            {
                "type": "UnityEngine.CapsuleCollider"
            },
            {
                "type": "UnityEngine.Rigidbody",
                "properties": {
                    "mass": 2.0,
                    "useGravity": True
                }
            }
        ]
    })

    # 位置を設定
    unity_component_crud({
        "operation": "update",
        "gameObjectPath": f"Enemies/{enemy_data['name']}",
        "componentType": "UnityEngine.Transform",
        "propertyChanges": {
            "position": {"x": enemy_data["x"], "y": enemy_data["y"], "z": enemy_data["z"]}
        }
    })
```

## ステップ6: 収集アイテムの作成

レベル周辺に収集可能な球体を追加します：

```python
# 収集アイテムコンテナを作成
unity_gameobject_crud({
    "operation": "create",
    "name": "Collectibles"
})

# 複数の収集アイテムを作成
unity_gameobject_createFromTemplate({
    "template": "Sphere",
    "name": "Coin1",
    "parentPath": "Collectibles",
    "position": {"x": 2, "y": 0.5, "z": 0},
    "scale": {"x": 0.3, "y": 0.3, "z": 0.3}
})

unity_gameobject_createFromTemplate({
    "template": "Sphere",
    "name": "Coin2",
    "parentPath": "Collectibles",
    "position": {"x": -2, "y": 0.5, "z": 2},
    "scale": {"x": 0.3, "y": 0.3, "z": 0.3}
})

unity_gameobject_createFromTemplate({
    "template": "Sphere",
    "name": "Coin3",
    "parentPath": "Collectibles",
    "position": {"x": 0, "y": 0.5, "z": -3},
    "scale": {"x": 0.3, "y": 0.3, "z": 0.3}
})

unity_gameobject_createFromTemplate({
    "template": "Sphere",
    "name": "Coin4",
    "parentPath": "Collectibles",
    "position": {"x": 3, "y": 0.5, "z": 3},
    "scale": {"x": 0.3, "y": 0.3, "z": 0.3}
})

# すべてのコインにトリガーコライダーを追加
unity_component_crud({
    "operation": "addMultiple",
    "pattern": "Collectibles/Coin*",
    "componentType": "UnityEngine.SphereCollider",
    "propertyChanges": {
        "isTrigger": True,
        "radius": 0.5
    },
    "maxResults": 10
})
```

## ステップ7: 環境障害物の追加

壁と障害物を作成します：

```python
# 障害物コンテナを作成
unity_gameobject_crud({
    "operation": "create",
    "name": "Obstacles"
})

# 位置とコライダー付きの壁を作成
walls = [
    {"name": "WallNorth", "position": {"x": 0, "y": 1, "z": 10}, "scale": {"x": 20, "y": 2, "z": 0.5}},
    {"name": "WallSouth", "position": {"x": 0, "y": 1, "z": -10}, "scale": {"x": 20, "y": 2, "z": 0.5}},
    {"name": "WallEast", "position": {"x": 10, "y": 1, "z": 0}, "scale": {"x": 0.5, "y": 2, "z": 20}},
    {"name": "WallWest", "position": {"x": -10, "y": 1, "z": 0}, "scale": {"x": 0.5, "y": 2, "z": 20}}
]

for wall in walls:
    # 壁キューブを作成
    unity_gameobject_createFromTemplate({
        "template": "Cube",
        "name": wall["name"],
        "parentPath": "Obstacles"
    })

    # トランスフォームを設定してコライダーを追加
    unity_component_crud({
        "operation": "update",
        "gameObjectPath": f"Obstacles/{wall['name']}",
        "componentType": "UnityEngine.Transform",
        "propertyChanges": {
            "position": wall["position"],
            "scale": wall["scale"]
        }
    })

    unity_component_crud({
        "operation": "add",
        "gameObjectPath": f"Obstacles/{wall['name']}",
        "componentType": "UnityEngine.BoxCollider"
    })
```

## ステップ8: 最終シーンの検査

完全なシーン階層を確認します：

```python
# シーンの完全な概要を取得
unity_scene_crud({
    "operation": "inspect",
    "includeHierarchy": True,
    "includeComponents": True
})
```

## 期待される結果

以下を含む完全なゲームレベルが完成しているはずです：
- ✓ 移動コントロール付きのプレイヤーキャラクター（WASD + Space）
- ✓ レベル周辺に配置された4体の敵NPC
- ✓ 4つの収集可能なコイン
- ✓ プレイヤーが落ちないようにする境界壁
- ✓ 物理演算対応の地面
- ✓ 適切なライティングとカメラセットアップ

## 次のステップ

このレベルを強化するには、次のことができます：

1. **敵AIの追加**: 敵が巡回したりプレイヤーを追いかけるスクリプトを作成
2. **収集システム**: プレイヤーがコインを収集したときを検出するスクリプトを追加
3. **UI要素**: スコア表示とヘルスバーを作成
4. **Prefab**: 再利用のために敵と収集アイテムをPrefabに変換
5. **マテリアル**: オブジェクトを区別するために色とテクスチャを追加

## パフォーマンスのヒント

大きなレベルを作成する場合：

- 検査時に`includeProperties=false`を使用してクエリを高速化
- バッチ操作を制限するために`maxResults`パラメータを使用
- 効率のためにコンポーネントバッチ操作（`addMultiple`、`updateMultiple`）を使用
- 1回の操作で複数コンポーネントを持つGameObjectを作成するためにテンプレートカスタマイズを使用

## よくある問題

**問題**: プレイヤーが地面を突き抜ける
**解決策**: 地面にコライダーがあり、プレイヤーにRigidbodyがあることを確認

**問題**: スクリプトがコンパイルされない
**解決策**: C#スクリプトの作成/更新に`unity_asset_crud`を使用。Unityは自動的に変更を検出してコンパイルします。

**問題**: 敵オブジェクトが見えない
**解決策**: 適切なメッシュ参照を持つMeshRendererとMeshFilterコンポーネントを追加

---

**関連項目:**
- [01-basic-scene-setup.md](01-basic-scene-setup.md) - 基本的な3Dシーン作成
- [02-ui-creation.md](02-ui-creation.md) - UIシステム作成
- [04-prefab-workflow.md](04-prefab-workflow.md) - Prefabの操作
