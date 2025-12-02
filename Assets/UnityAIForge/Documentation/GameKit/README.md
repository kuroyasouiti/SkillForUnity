# GameKitランタイムコンポーネント

<div align="center">

**🎮 高レベルゲーム開発フレームワーク**

迅速なゲーム開発のための柔軟でモジュール式のコンポーネント

[📚 インデックスに戻る](../INDEX.md) | [🚀 はじめに](../GETTING_STARTED.md) | [🎓 完全ガイド](../MCPServer/SKILL_GAMEKIT.md)

</div>

---

GameKitは、柔軟性とモジュール性を重視した高レベルのゲーム開発コンポーネントを提供します。

## 📖 クイックナビゲーション

| コンポーネント | 説明 | ドキュメント |
|:---|:---|:---|
| **🎭 Actor** | プレイヤー/NPCシステム | [詳細](#actorシステム) |
| **🎯 Manager** | リソース/状態/ターン管理 | [詳細](#managerシステム) |
| **💰 Resources** | 経済とリソースプール | [README](GameKitResourceManager.README.md) |
| **📊 Machinations** | 経済システム設計 | [README](GameKitMachinations.README.md) |
| **🔄 SceneFlow** | シーン遷移ステートマシン | [README](GameKitSceneFlow.README.md) |
| **🎨 UICommand** | UI → ロジックブリッジ | [README](GameKitUICommand.README.md) |
| **🎯 Interaction** | トリガーベースインタラクション | [README](GameKitInteraction.README.md) |
| **🛤️ Spline** | レールベース移動 | [README](SplineMovement.README.md) |
| **🗺️ Graph** | A*パスファインディング | [README](GraphNodeMovement.README.md) |

---

## Actorシステム

### GameKitActor

コントローラーから動作コンポーネントへの入力をUnityEventsを介して中継するコアハブコンポーネント。

**イベント:**
- `OnMoveInput(Vector3)` - 移動方向
- `OnJumpInput()` - ジャンプアクション
- `OnActionInput(string)` - 汎用アクション（例: "interact"、"attack"）
- `OnLookInput(Vector2)` - 視点/回転入力

**動作プロファイル:**
- `TwoDLinear` - 2Dトランスフォームベース移動
- `TwoDPhysics` - 2D物理ベース移動（Rigidbody2D）
- `TwoDTileGrid` - 2Dタイルベースグリッド移動
- `GraphNode` - A*パスファインディング付きノードベースグラフ移動（2D/3D非依存）
- `SplineMovement` - 2.5Dゲーム、レールシューター、サイドスクローラー用のレール/スプライン移動
- `ThreeDCharacterController` - 3Dキャラクターコントローラー
- `ThreeDPhysics` - 3D物理ベース移動（Rigidbody）
- `ThreeDNavMesh` - 3D NavMeshエージェント

**制御モード:**
- `DirectController` - プレイヤー入力制御
- `AIAutonomous` - AI駆動制御
- `UICommand` - UIボタン制御
- `ScriptTriggerOnly` - スクリプトのみの制御

### 入力コントローラー

#### GameKitInputSystemController（推奨）

Unityの新しいInput Systemを使用（Input Systemパッケージが必要）。

**機能:**
- 自動PlayerInput統合
- 事前設定されたアクションマップ（Move、Look、Jump、Action、Fire）
- WASD/方向キー + ゲームパッドサポート
- マウス + 右スティックによる視点入力
- 動作プロファイルに基づく自動2D/3D入力変換

**要件:**
- Input Systemパッケージ（`com.unity.inputsystem`）
- PlayerInputコンポーネント（自動追加）
- DefaultGameKitInputActionsアセット（自動生成）

**使用方法:**
```csharp
// DirectControllerモードでアクターを作成すると自動的に追加されます
// デフォルトバインディング:
// - WASD/左スティック: Move
// - マウス/右スティック: Look
// - Space/Aボタン: Jump
// - E/Xボタン: Action
// - 左クリック/RT: Fire
```

#### GameKitSimpleInput（レガシーフォールバック）

最大限の互換性のためにUnityのレガシーInputシステムを使用。

**機能:**
- Input.GetAxis()ベースの移動
- キーボード + ゲームパッドサポート
- 自動2D/3D変換
- 追加パッケージ不要

**フォールバック動作:**
- Input Systemがインストールされていない場合に自動使用
- GameKitInputSystemControllerを削除することで手動切り替え可能

### AIコントローラー

#### GameKitSimpleAI

NPCと敵のための自律AIコントローラー。

**動作:**
- `Idle` - 何もしない
- `Patrol` - ウェイポイントを巡回
- `Follow` - ターゲットを追跡
- `Wander` - ランダム移動

**例:**
```csharp
var ai = actor.GetComponent<GameKitSimpleAI>();
ai.SetBehavior(GameKitSimpleAI.AIBehaviorType.Patrol);
ai.SetPatrolPoints(waypointArray);
```

### UIコマンドハブ

#### GameKitUICommand

UIコントロールをGameKitActorのUnityEventsにブリッジし、UI-toアクター通信の中心ハブとして機能。

**機能:**
- コマンドタイプマッピング（Move、Jump、Action、Look、Custom）
- 移動用の方向ボタンサポート
- パラメータベースアクション
- パフォーマンスのためのActor参照キャッシング
- コマンドバインディング管理

**コマンドタイプ:**
- `Move` - `OnMoveInput(Vector3)`にマップ
- `Jump` - `OnJumpInput()`にマップ
- `Action` - `OnActionInput(string)`にマップ
- `Look` - `OnLookInput(Vector2)`にマップ
- `Custom` - 後方互換性のためのSendMessage

**例:**
```csharp
// UIコマンドハブをセットアップ
var uiCommand = commandPanel.GetComponent<GameKitUICommand>();
uiCommand.SetTargetActor(playerActor);

// 方向ボタンを登録
uiCommand.RegisterDirectionalButton("moveUp", upButton, Vector3.up);
uiCommand.RegisterDirectionalButton("moveDown", downButton, Vector3.down);

// アクションボタンを登録
uiCommand.RegisterButton("jump", jumpButton, GameKitUICommand.CommandType.Jump);
uiCommand.RegisterButton("attack", attackButton, GameKitUICommand.CommandType.Action, "sword");

// またはコマンドを直接実行
uiCommand.ExecuteMoveCommand(new Vector3(1, 0, 0));
uiCommand.ExecuteActionCommand("usePotion");
```

**ユースケース:**
- モバイルゲームのタッチコントロール
- 仮想ジョイスティックとD-pad
- アクションボタンパネル
- クイックアクションラジアルメニュー
- コマンドパレットシステム

## 移動コンポーネント

### TileGridMovement

タイルベースゲーム用のグリッドベース移動。

**機能:**
- スムーズな補間による離散タイル移動
- 設定可能なグリッドサイズ
- 斜め移動サポート
- 衝突検出
- 移動キューイング

**自動リスニング:**
- `GameKitActor.OnMoveInput` - グリッド方向入力用

### GraphNodeMovement

離散移動空間のためのA*パスファインディング付きノードベースグラフ移動。

**機能:**
- 2Dと3Dの両方で動作（次元非依存）
- ノード間のA*パスファインディング
- 重み付け、通過可能な接続
- 到達可能ノードクエリ
- スムーズな補間または即座の移動
- デバッグ可視化

**ユースケース:**
- ボードゲーム（チェス、チェッカー）
- タクティカルRPG（ファイアーエムブレムスタイル）
- パズルゲーム（スライディングパズル）
- アドベンチャーゲーム（部屋から部屋へのナビゲーション）
- タワーディフェンス（敵のパス追従）

**主要コンポーネント:**

#### GraphNode
移動グラフ内の位置/場所を表します。

**メソッド:**
- `AddConnection(node, cost, bidirectional)` - 別のノードに接続
- `RemoveConnection(node, bidirectional)` - ノードから切断
- `IsConnectedTo(node)` - 直接接続されているか確認
- `SetConnectionTraversable(node, traversable)` - 接続を有効/無効化
- `AutoConnectToNearbyNodes(radius)` - 半径内で自動接続
- `ClearConnections(bidirectional)` - すべての接続を削除

#### GraphNodeMovement
グラフに沿ったアクター移動を処理。

**メソッド:**
- `MoveToNode(node)` - 隣接ノードに移動（パスファインディングなし）
- `MoveToNodeWithPathfinding(node)` - パスを見つけて任意のノードに移動
- `SnapToNearestNode()` - 最も近いノードを見つけてスナップ
- `TeleportToNode(node)` - ノードへの即座の移動
- `GetReachableNodes(maxDistance)` - 距離内のすべてのノードを取得

**プロパティ:**
- `CurrentNode` - アクターが現在いるノード
- `IsMoving` - アクターが現在移動中かどうか
- `CurrentPath` - 追従中のアクティブなパス

**自動リスニング:**
- `GameKitActor.OnMoveInput` - 方向に基づいて最適な隣接ノードを選択

**セットアップ例:**
```csharp
// グラフノードを作成
var node1 = new GameObject("Node1").AddComponent<GraphNode>();
var node2 = new GameObject("Node2").AddComponent<GraphNode>();
var node3 = new GameObject("Node3").AddComponent<GraphNode>();

node1.transform.position = new Vector3(0, 0, 0);
node2.transform.position = new Vector3(5, 0, 0);
node3.transform.position = new Vector3(10, 0, 0);

// ノードを接続
node1.AddConnection(node2, 1f, true);
node2.AddConnection(node3, 1f, true);

// アクターを作成
var actor = CreateActor("Player", graphNode);
var movement = actor.GetComponent<GraphNodeMovement>();
movement.SnapToNearestNode();

// ノードに移動（直接）
movement.MoveToNode(node2);

// またはパスファインディングを使用
movement.MoveToNodeWithPathfinding(node3);

// 到達可能ノードをクエリ
var reachable = movement.GetReachableNodes(2);
```

## Managerシステム

### GameKitManager（ハブ）

`ManagerType`に基づいてモード固有コンポーネントを自動的に追加するゲーム管理の中心ハブ。

**アーキテクチャ:**
- GameKitManagerは軽量ハブとして機能
- 初期化時にモード固有コンポーネントを自動アタッチ
- 便利メソッドはモード固有コンポーネントにデリゲート
- `GetModeComponent<T>()`による直接アクセス

**マネージャータイプとコンポーネント:**

#### TurnBased → GameKitTurnManager
ターンベースゲームフロー管理。

**機能:**
- ターンフェーズ管理
- ターンカウンター
- フェーズ遷移
- イベント: `OnPhaseChanged`、`OnTurnAdvanced`

**例:**
```csharp
var manager = managerGo.AddComponent<GameKitManager>();
manager.Initialize("gameManager", ManagerType.TurnBased, false);

manager.AddTurnPhase("PlayerTurn");
manager.AddTurnPhase("EnemyTurn");
manager.AddTurnPhase("EndTurn");

manager.NextPhase(); // PlayerTurn → EnemyTurn

// TurnManagerへの直接アクセス
var turnManager = manager.GetModeComponent<GameKitTurnManager>();
turnManager.OnPhaseChanged.AddListener(phase => {
    Debug.Log($"フェーズ変更: {phase}");
});
```

#### ResourcePool → GameKitResourceManager
ゲーム経済のためのMachinations風リソースフローシステム。

**機能:**
- 最小/最大制約付きリソースプール
- 自動フロー（ソースは生成、ドレインは消費）
- リソースコンバーター（クラフティング、変換チェーン）
- リソーストリガー（しきい値ベースイベント）
- イベント: `OnResourceChanged`、`OnResourceTriggered`

**例:**
```csharp
var manager = managerGo.AddComponent<GameKitManager>();
manager.Initialize("resourceManager", ManagerType.ResourcePool, false);

// ResourceManagerへの直接アクセス
var resourceManager = manager.GetModeComponent<GameKitResourceManager>();

// 基本リソース
manager.SetResource("gold", 100);
resourceManager.SetResourceConstraints("health", 0f, 100f);

// 自動フロー
resourceManager.AddFlow("gold", 5f, isSource: true);  // 5ゴールド/秒の収入
resourceManager.AddFlow("mana", 2f, isSource: false); // 2マナ/秒のドレイン

// リソース変換（クラフティング）
resourceManager.AddConverter("wood", "planks", conversionRate: 4f, inputCost: 1f);
bool crafted = resourceManager.Convert("wood", "planks", 10f); // 10木材 → 40板材

// しきい値トリガー
resourceManager.AddTrigger("lowHealth", "health", ThresholdType.Below, 30f);
resourceManager.OnResourceTriggered.AddListener((trigger, resource, value) => {
    if (trigger == "lowHealth") ShowWarning();
});

// または便利メソッドを使用
bool consumed = manager.ConsumeResource("gold", 75);
```

詳細なドキュメントは[GameKitResourceManager.README.md](./GameKitResourceManager.README.md)を参照。

#### EventHub → GameKitEventManager
カスタムイベント用のゲームワイドイベントハブ。

**機能:**
- イベント登録/登録解除
- イベントトリガー
- 名前付きイベントシステム

**例:**
```csharp
var manager = managerGo.AddComponent<GameKitManager>();
manager.Initialize("eventHub", ManagerType.EventHub, false);

manager.RegisterEventListener("OnLevelComplete", () => {
    Debug.Log("レベルクリア!");
});

manager.TriggerEvent("OnLevelComplete");
```

#### StateManager → GameKitStateManager
ゲーム状態管理（メニュー、プレイ中、一時停止など）

**機能:**
- 状態遷移
- 状態履歴
- 前の状態の追跡
- イベント: `OnStateChanged`

**例:**
```csharp
var manager = managerGo.AddComponent<GameKitManager>();
manager.Initialize("stateManager", ManagerType.StateManager, false);

manager.ChangeState("MainMenu");
manager.ChangeState("Playing");
manager.ChangeState("Paused");

manager.ReturnToPreviousState(); // Paused → Playing

var currentState = manager.GetCurrentState(); // "Playing"

// StateManagerへの直接アクセス
var stateManager = manager.GetModeComponent<GameKitStateManager>();
stateManager.OnStateChanged.AddListener((newState, oldState) => {
    Debug.Log($"状態: {oldState} → {newState}");
});
```

#### Realtime → GameKitRealtimeManager
リアルタイムゲームフロー管理（タイムスケール、一時停止、タイマー）

**機能:**
- タイムスケール制御
- 一時停止/再開
- タイマー管理
- 経過時間追跡
- イベント: `OnTimeScaleChanged`、`OnPauseChanged`

**例:**
```csharp
var manager = managerGo.AddComponent<GameKitManager>();
manager.Initialize("timeManager", ManagerType.Realtime, false);

manager.SetTimeScale(0.5f); // スローモーション
manager.Pause();
manager.Resume();

// タイマー用のRealtimeManagerへの直接アクセス
var realtimeManager = manager.GetModeComponent<GameKitRealtimeManager>();
realtimeManager.AddTimer("powerup", 5f, () => {
    Debug.Log("パワーアップ期限切れ!");
});
```

**後方互換性:**
GameKitManagerを使用する既存のコードはすべて引き続き動作します。便利メソッドは適切なモード固有コンポーネントに自動的にデリゲートします。

## 統合例

### プレイヤーキャラクターの作成

```csharp
// MCP経由
unity_gamekit_actor({
    "operation": "create",
    "actorId": "Player",
    "behaviorProfile": "2dPhysics",
    "controlMode": "directController",
    "spritePath": "Assets/Sprites/player.png",
    "position": {"x": 0, "y": 0, "z": 0}
})
```

結果:
- GameKitActor付きGameObject
- Rigidbody2D + BoxCollider2D（2dPhysicsプロファイルから）
- PlayerInput + GameKitInputSystemController（directControllerモードから）
- 割り当てられたスプライト付きSpriteRenderer

### AI敵の作成

```csharp
unity_gamekit_actor({
    "operation": "create",
    "actorId": "Enemy",
    "behaviorProfile": "2dPhysics",
    "controlMode": "aiAutonomous",
    "spritePath": "Assets/Sprites/enemy.png"
})
```

結果:
- GameKitActor付きGameObject
- Rigidbody2D + BoxCollider2D
- GameKitSimpleAI（aiAutonomousモードから）

### グリッドベースキャラクターの作成

```csharp
unity_gamekit_actor({
    "operation": "create",
    "actorId": "GridHero",
    "behaviorProfile": "2dTileGrid",
    "controlMode": "directController"
})
```

結果:
- GameKitActor付きGameObject
- TileGridMovementコンポーネント
- 入力コントローラー（Input Systemまたはレガシー）

### グラフベースキャラクターの作成（ボードゲーム）

```csharp
// 1. グラフノードを作成
unity_gameobject_crud({
    "operation": "create",
    "objectName": "BoardSpace1",
    "position": {"x": 0, "y": 0, "z": 0}
})

unity_component_crud({
    "operation": "add",
    "gameObjectPath": "BoardSpace1",
    "componentType": "Unity-AI-Forge.GameKit.GraphNode"
})

// さらにノードを繰り返し...

// 2. ノードを接続（スクリプトまたはエディタで手動）
// node1.AddConnection(node2, cost: 1.0f, bidirectional: true)

// 3. グラフ移動付きアクターを作成
unity_gamekit_actor({
    "operation": "create",
    "actorId": "GamePiece",
    "behaviorProfile": "graphNode",
    "controlMode": "uiCommand",
    "position": {"x": 0, "y": 0, "z": 0}
})
```

結果:
- GameKitActor付きGameObject
- GraphNodeMovementコンポーネント
- 開始時に最も近いノードにスナップ
- 入力方向またはパスファインディングAPIで移動

**一般的なグラフパターン:**
- **ボードゲーム**: 斜め接続付き正方形グリッド
- **タクティカルRPG**: 六角形グリッドまたは不規則な地形ノード
- **パズルゲーム**: 接続されたパズルピース/タイル
- **アドベンチャーゲーム**: 部屋から部屋へのナビゲーショングラフ
- **タワーディフェンス**: 敵パスウェイポイント

### SplineMovementコンポーネント

Catmull-Romスプラインを使用した2.5Dゲーム用のスムーズなレール/スプラインベース移動を提供。

**機能:**
- 制御点で定義された滑らかな曲線パス
- 自然な回転のための自動接線計算
- 円形トラック用のクローズドループサポート
- レーンベースゲームプレイのための横方向オフセット
- 手動または自動速度制御
- 前進および後進移動サポート
- Scene viewでのビジュアルスプラインデバッグ

**主要プロパティ:**
- `controlPoints` - スプラインパスを定義するTransform配列
- `moveSpeed` - スプラインに沿った移動速度
- `closedLoop` - 最後の点を最初の点に接続
- `autoRotate` - 移動方向を向く
- `allowManualControl` - 入力を速度制御に使用
- `lateralOffset` - パスからのオフセット（レーン用）

**一般的なユースケース:**
- **レールシューター**: カメラが横移動で固定パスを追従
- **サイドスクローラー**: キャラクターが2.5D環境の曲がりくねったパスを追従
- **レーシングゲーム**: 車両がレーン変更付きトラックを追従
- **オンレールシーケンス**: パスに沿ったカットシーンまたはスクリプト移動
- **ジェットコースター**: トラックに沿った物理無効化ライド

## アーキテクチャ

```
入力ソース（キーボード/AI/UI）
    ↓
コントローラーコンポーネント（GameKitInputSystemController/GameKitSimpleAI）
    ↓
GameKitActor（UnityEvents付きハブ）
    ↓
動作コンポーネント（TileGridMovement/カスタムスクリプト）
    ↓
ゲームロジック
```

この疎結合アーキテクチャにより：
- 動作を変更せずに入力ソースを交換
- コントローラーを変更せずに動作を交換
- イベントごとに複数のリスナー
- 簡単なテストとデバッグ

## バージョン定義

- `UNITY_INPUT_SYSTEM_INSTALLED` - Input Systemパッケージがインストールされている場合に定義

---

## 📚 関連ドキュメント

### 詳細ガイド

- [**GameKitResourceManager**](GameKitResourceManager.README.md) - リソースプール、フロー、経済
- [**GameKitMachinations**](GameKitMachinations.README.md) - アセットとしての経済システム設計
- [**GameKitSceneFlow**](GameKitSceneFlow.README.md) - シーン遷移ステートマシン
- [**GameKitUICommand**](GameKitUICommand.README.md) - UIボタン → ロジックコマンド
- [**GameKitInteraction**](GameKitInteraction.README.md) - トリガーベースインタラクション
- [**SplineMovement**](SplineMovement.README.md) - レールベース移動システム
- [**GraphNodeMovement**](GraphNodeMovement.README.md) - グラフノードでのA*パスファインディング

### チュートリアル

- [**はじめに**](../GETTING_STARTED.md) - GameKitの最初のステップ
- [**完全ガイド**](../MCPServer/SKILL_GAMEKIT.md) - 例付き包括的GameKitガイド
- [**例**](../Examples/README.md) - 実践的なチュートリアル

---

<div align="center">

**🎮 ハッピーゲーム開発！ ✨**

[📚 インデックスに戻る](../INDEX.md) | [🚀 はじめに](../GETTING_STARTED.md) | [💡 例](../Examples/README.md)

</div>
