# デザインパターン生成例

このガイドでは、`unity_designPattern_generate`ツールを使用して、一般的なデザインパターンのプロダクション対応実装を作成する方法を示します。

## 概要

デザインパターンジェネレーターは、7つの一般的なUnityデザインパターンの完全でコメント付きのC#コードを作成します：
- **Singleton** - 永続性を持つシングルインスタンス管理
- **ObjectPool** - パフォーマンスのための効率的なオブジェクト再利用
- **StateMachine** - 遷移を伴う状態管理
- **Observer** - イベント駆動通信
- **Command** - 元に戻す/やり直し機能を持つアクション抽象化
- **Factory** - オブジェクト作成パターン
- **ServiceLocator** - グローバルサービスアクセス

## 例1: Singletonパターン（ゲームマネージャー）

```python
unity_designPattern_generate({
    "patternType": "singleton",
    "className": "GameManager",
    "scriptPath": "Assets/Scripts/Managers/GameManager.cs",
    "namespace": "MyGame.Managers",
    "options": {
        "persistent": True,      # シーン変更を超えて存続
        "threadSafe": True,      # スレッドセーフな初期化
        "monoBehaviour": True    # Unity MonoBehaviour
    }
})
```

**生成される機能:**
- 永続性のためのDontDestroyOnLoad
- スレッドセーフな遅延初期化
- 重複に対するAwake()保護
- カスタムメソッドを追加可能

## 例2: ObjectPoolパターン（弾丸プール）

```python
unity_designPattern_generate({
    "patternType": "objectpool",
    "className": "BulletPool",
    "scriptPath": "Assets/Scripts/Combat/BulletPool.cs",
    "namespace": "MyGame.Combat",
    "options": {
        "pooledType": "Bullet",       # プールする型
        "defaultCapacity": "100",     # 初期サイズ
        "maxSize": "500"              # 最大サイズ
    }
})
```

**生成される機能:**
- Unity ObjectPool<T>統合
- 設定可能なプールサイズ
- Get/Release/Clearメソッド
- 自動Prefabインスタンス化

**使用方法:**
```csharp
// プールから弾丸を取得
Bullet bullet = bulletPool.Get();
bullet.Fire(direction);

// 完了したらプールに戻す
bulletPool.Release(bullet);
```

## 例3: StateMachineパターン（プレイヤー状態）

```python
unity_designPattern_generate({
    "patternType": "statemachine",
    "className": "PlayerStateMachine",
    "scriptPath": "Assets/Scripts/Player/PlayerStateMachine.cs",
    "namespace": "MyGame.Player"
})
```

**生成される機能:**
- Enter/Execute/Exit付きのIStateインターフェース
- 型安全な状態登録
- 状態変更管理
- IdleとMove状態の例

**使用方法:**
```csharp
// 状態を登録
stateMachine.RegisterState(new IdleState());
stateMachine.RegisterState(new MoveState());
stateMachine.RegisterState(new JumpState());

// 状態を変更
stateMachine.ChangeState<MoveState>();
```

## 例4: Observerパターン（イベントシステム）

```python
unity_designPattern_generate({
    "patternType": "observer",
    "className": "EventManager",
    "scriptPath": "Assets/Scripts/Core/EventManager.cs",
    "namespace": "MyGame.Core"
})
```

**生成される機能:**
- Singletonイベントマネージャー
- 型安全なイベントサブスクリプション
- ジェネリックイベント公開
- 文字列ベースのイベント名

**使用方法:**
```csharp
// イベントをサブスクライブ
EventManager.Instance.Subscribe<int>("ScoreChanged", OnScoreChanged);
EventManager.Instance.Subscribe("GameOver", OnGameOver);

// イベントを公開
EventManager.Instance.Publish("ScoreChanged", newScore);
EventManager.Instance.Publish("GameOver");

// サブスクライブ解除
EventManager.Instance.Unsubscribe<int>("ScoreChanged", OnScoreChanged);
```

## 例5: Commandパターン（元に戻す/やり直しシステム）

```python
unity_designPattern_generate({
    "patternType": "command",
    "className": "CommandManager",
    "scriptPath": "Assets/Scripts/Editor/CommandManager.cs",
    "namespace": "MyGame.Editor"
})
```

**生成される機能:**
- ICommandインターフェース
- コマンド履歴スタック
- 元に戻す/やり直し機能
- MoveCommandの例

**使用方法:**
```csharp
// コマンドを実行
var moveCmd = new MoveCommand(player.transform, newPosition);
commandManager.ExecuteCommand(moveCmd);

// 最後のアクションを元に戻す
commandManager.Undo();

// やり直し
commandManager.Redo();

// 履歴をクリア
commandManager.ClearHistory();
```

## 例6: Factoryパターン（敵スポーナー）

```python
unity_designPattern_generate({
    "patternType": "factory",
    "className": "EnemyFactory",
    "scriptPath": "Assets/Scripts/Enemies/EnemyFactory.cs",
    "namespace": "MyGame.Enemies",
    "options": {
        "productType": "GameObject"
    }
})
```

**生成される機能:**
- プロダクトIDからPrefabへのマッピング
- Inspector対応の設定
- 型安全な作成メソッド
- 位置/回転のオーバーロード

**使用方法:**
```csharp
// IDで敵を作成
GameObject zombie = enemyFactory.CreateProduct("zombie");

// 位置付きで作成
GameObject boss = enemyFactory.CreateProduct("boss", spawnPos, spawnRot);

// コンポーネントアクセス付きで作成
Enemy skeleton = enemyFactory.CreateProduct<Enemy>("skeleton");
```

## 例7: ServiceLocatorパターン（グローバルサービス）

```python
unity_designPattern_generate({
    "patternType": "servicelocator",
    "className": "ServiceLocator",
    "scriptPath": "Assets/Scripts/Core/ServiceLocator.cs",
    "namespace": "MyGame.Core"
})
```

**生成される機能:**
- Singletonサービスレジストリ
- 型安全な登録
- サービス存在チェック
- IAudioServiceインターフェースの例

**使用方法:**
```csharp
// サービスを登録
ServiceLocator.Instance.RegisterService<IAudioService>(new AudioService());
ServiceLocator.Instance.RegisterService<IInputService>(new InputService());

// サービスを取得
IAudioService audio = ServiceLocator.Instance.GetService<IAudioService>();
audio.PlaySound("explosion");

// サービスが存在するか確認
if (ServiceLocator.Instance.HasService<IAnalytics>()) {
    var analytics = ServiceLocator.Instance.GetService<IAnalytics>();
    analytics.LogEvent("level_complete");
}
```

## 完全なゲームアーキテクチャの例

堅牢なゲームアーキテクチャのために複数のパターンを組み合わせます：

```python
# 1. コアインフラストラクチャ
unity_designPattern_generate({
    "patternType": "singleton",
    "className": "GameManager",
    "scriptPath": "Assets/Scripts/Core/GameManager.cs",
    "namespace": "MyGame.Core",
    "options": {"persistent": True, "monoBehaviour": True}
})

# 2. 疎結合な通信のためのイベントシステム
unity_designPattern_generate({
    "patternType": "observer",
    "className": "EventManager",
    "scriptPath": "Assets/Scripts/Core/EventManager.cs",
    "namespace": "MyGame.Core"
})

# 3. グローバルサービスのためのサービスロケーター
unity_designPattern_generate({
    "patternType": "servicelocator",
    "className": "ServiceLocator",
    "scriptPath": "Assets/Scripts/Core/ServiceLocator.cs",
    "namespace": "MyGame.Core"
})

# 4. パフォーマンスのためのオブジェクトプーリング
unity_designPattern_generate({
    "patternType": "objectpool",
    "className": "BulletPool",
    "scriptPath": "Assets/Scripts/Combat/BulletPool.cs",
    "namespace": "MyGame.Combat",
    "options": {"pooledType": "Bullet", "defaultCapacity": "100", "maxSize": "500"}
})

# 5. プレイヤー状態管理
unity_designPattern_generate({
    "patternType": "statemachine",
    "className": "PlayerStateMachine",
    "scriptPath": "Assets/Scripts/Player/PlayerStateMachine.cs",
    "namespace": "MyGame.Player"
})

# 6. 敵スポーン
unity_designPattern_generate({
    "patternType": "factory",
    "className": "EnemyFactory",
    "scriptPath": "Assets/Scripts/Enemies/EnemyFactory.cs",
    "namespace": "MyGame.Enemies"
})
```

## ベストプラクティス

### 1. 適切なパターンを使用

- **Singleton**: マネージャー（GameManager、AudioManager、InputManager）
- **ObjectPool**: 頻繁にスポーンされるオブジェクト（弾丸、パーティクル、敵）
- **StateMachine**: 複雑な振る舞い（プレイヤー状態、AI状態、UI状態）
- **Observer**: 疎結合なイベント（スコア変更、実績、ゲームイベント）
- **Command**: 元に戻せるアクション（レベルエディタ、ゲームプレイ巻き戻し）
- **Factory**: ランタイムオブジェクト作成（敵スポーナー、アイテム生成）
- **ServiceLocator**: 横断的関心事（オーディオ、分析、ローカライゼーション）

### 2. パターンを効果的に組み合わせ

```python
# コアシステム
GameManager (Singleton) + EventManager (Observer) + ServiceLocator

# 戦闘システム
BulletPool (ObjectPool) + EnemyFactory (Factory)

# プレイヤーシステム
PlayerStateMachine (StateMachine) + CommandManager (アビリティ用Command)
```

### 3. 名前空間の整理

```python
unity_designPattern_generate({
    "namespace": "MyGame.Core",      # コアインフラストラクチャ
    # または
    "namespace": "MyGame.Combat",    # 戦闘システム
    # または
    "namespace": "MyGame.UI",        # UIシステム
    ...
})
```

### 4. 生成されたコードをカスタマイズ

生成後、コードを編集して：
- カスタムメソッドとプロパティを追加
- ゲーム固有のロジックを実装
- Inspectorフィールドを設定
- ドキュメントコメントを追加

## 一般的なワークフロー

### ワークフロー1: 新しいプロジェクトのセットアップ

```python
# 1. コアインフラストラクチャ
unity_designPattern_generate({"patternType": "singleton", "className": "GameManager", ...})
unity_designPattern_generate({"patternType": "observer", "className": "EventManager", ...})
unity_designPattern_generate({"patternType": "servicelocator", "className": "ServiceLocator", ...})

# 2. GameManager.Awake()で初期化
# 3. GameManager.Start()でサービスを登録
```

### ワークフロー2: 戦闘システム

```python
# 1. パフォーマンスのためのオブジェクトプール
unity_designPattern_generate({"patternType": "objectpool", "className": "BulletPool", ...})
unity_designPattern_generate({"patternType": "objectpool", "className": "ParticlePool", ...})

# 2. 敵スポーン
unity_designPattern_generate({"patternType": "factory", "className": "EnemyFactory", ...})

# 3. イベントシステムを配線
# EventManager.Instance.Publish("EnemyKilled", enemyType)
```

### ワークフロー3: プレイヤーコントローラー

```python
# 1. プレイヤー用のステートマシン
unity_designPattern_generate({"patternType": "statemachine", "className": "PlayerStateMachine", ...})

# 2. アビリティ用のCommandパターン（元に戻す機能付き）
unity_designPattern_generate({"patternType": "command", "className": "AbilityManager", ...})

# 3. 状態を実装: Idle、Move、Jump、Attack、Die
```

## ヒント

1. **常に名前空間を使用** - コードを適切に整理
2. **生成されたコードを編集** - 特定のニーズに合わせてカスタマイズ
3. **パターンをテスト** - Unityメニュー: Tools > SkillForUnity > Test Pattern Generation
4. **生成されたコメントを読む** - 使用例が含まれています
5. **賢く組み合わせる** - 過度な設計をせず、必要なものを使用

## 次のステップ

パターン生成後：
1. 生成されたコードをレビュー
2. ゲームのニーズに合わせてカスタマイズ
3. ユニットテストを作成
4. 既存のシステムと統合
5. アーキテクチャをドキュメント化

**ハッピーコーディング！** 🎮
