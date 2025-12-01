# GameKit Machinations Execution

## Overview

GameKitMachinationsのダイアグラムに従ってリソースを動的に制御するシステムです。Flows、Converters、Triggersを実行してゲーム経済を駆動します。

## 🎯 アーキテクチャ

### 1. GameKitResourceManager（コアストレージ）
- リソースの保存と基本操作
- ダイアグラム実行メソッドを提供（手動呼び出し用）

### 2. GameKitMachinationsController（オプショナル）
- ダイアグラムの自動実行コントローラー
- Update()で自動的にFlows/Converters/Triggersを処理

### 3. GameKitMachinationsAsset（設定データ）
- Pools、Flows、Converters、Triggersの定義
- ScriptableObjectとして保存

## 実行メソッド

### ProcessFlows
時間経過に応じてリソースを自動生成/消費します。

```csharp
// すべてのFlowsを処理
resourceManager.ProcessFlows(Time.deltaTime);

// 特定のFlowのみ処理
resourceManager.ProcessFlow("manaRegen", Time.deltaTime);
```

### ExecuteConverter
リソースを変換します。

```csharp
// 10ゴールド → 50ヘルス
bool success = resourceManager.ExecuteConverter("buyHealthPotion", amount: 1f);
if (success)
{
    Debug.Log("Health potion purchased!");
}
```

### CheckTriggers
リソースの閾値をチェックしてトリガーを評価します。

```csharp
// すべてのTriggersをチェック
resourceManager.CheckTriggers();

// 特定のTriggerをチェック
bool isLowHealth = resourceManager.CheckTrigger("lowHealth");
if (isLowHealth)
{
    ShowWarning("Low health!");
}
```

## 使用方法

### 方法1: 手動実行

```csharp
public class CustomEconomyController : MonoBehaviour
{
    [SerializeField] private GameKitManager manager;
    private GameKitResourceManager resourceManager;

    void Start()
    {
        resourceManager = manager.GetComponent<GameKitResourceManager>();
    }

    void Update()
    {
        // Flowsを処理（マナ自動回復など）
        resourceManager.ProcessFlows(Time.deltaTime);
        
        // Triggersをチェック
        if (resourceManager.CheckTrigger("lowHealth"))
        {
            PlayLowHealthWarning();
        }
    }

    public void CraftItem()
    {
        // Converterを実行（木材 → 剣）
        if (resourceManager.ExecuteConverter("craftSword"))
        {
            Debug.Log("Sword crafted!");
        }
    }
}
```

### 方法2: 自動実行（GameKitMachinationsController）

```csharp
// Inspectorで設定するか、コードで追加
var controller = manager.gameObject.AddComponent<GameKitMachinationsController>();

// 設定
controller.SetAutoProcessFlows(true);        // Flows自動実行
controller.SetAutoCheckTriggers(true);       // Triggers自動チェック
controller.SetAutoProcessConverters(false);  // Converters手動のまま
controller.SetTimeScale(1.5f);               // 時間スケール調整
```

### 方法3: MCPから制御

```python
# MachinationsControllerを追加
await call_tool("gamekitMachinationsExecution", "addController", {
    "managerId": "PlayerEconomy",
    "autoProcessFlows": True,
    "autoCheckTriggers": True,
    "timeScale": 1.0
})

# Flowsを手動実行
await call_tool("gamekitMachinationsExecution", "processFlows", {
    "managerId": "PlayerEconomy",
    "deltaTime": 0.1
})

# Converterを実行
result = await call_tool("gamekitMachinationsExecution", "executeConverter", {
    "managerId": "PlayerEconomy",
    "converterId": "buyHealthPotion",
    "amount": 1
})

# Triggerをチェック
result = await call_tool("gamekitMachinationsExecution", "checkTriggers", {
    "managerId": "PlayerEconomy",
    "triggerName": "lowHealth"
})
```

## 実用例

### RPG マナ回復システム

```csharp
// Machinations Asset設定
// Flow: manaRegen (+1.5 MP/s)
// Trigger: fullMana (MP >= 50)

public class ManaSystem : MonoBehaviour
{
    [SerializeField] private GameKitManager economy;
    private GameKitResourceManager resourceManager;

    void Start()
    {
        resourceManager = economy.GetComponent<GameKitResourceManager>();
        
        // 自動マナ回復を有効化
        var controller = economy.gameObject.AddComponent<GameKitMachinationsController>();
        controller.SetAutoProcessFlows(true);
    }

    void Update()
    {
        // マナが満タンになったか確認
        if (resourceManager.CheckTrigger("fullMana"))
        {
            Debug.Log("Mana fully restored!");
        }
    }
}
```

### タワーディフェンス 建設システム

```csharp
// Machinations Asset設定
// Converter: buildBasicTower (50 Gold → 1 Tower)
// Converter: buildAdvancedTower (150 Gold → 1 AdvancedTower)

public class TowerBuilder : MonoBehaviour
{
    [SerializeField] private GameKitManager economy;
    private GameKitResourceManager resourceManager;

    void Start()
    {
        resourceManager = economy.GetComponent<GameKitResourceManager>();
    }

    public void BuildBasicTower()
    {
        if (resourceManager.ExecuteConverter("buildBasicTower"))
        {
            SpawnTower("BasicTower");
            Debug.Log("Basic tower built!");
        }
        else
        {
            ShowMessage("Not enough gold!");
        }
    }

    public void BuildAdvancedTower()
    {
        if (resourceManager.ExecuteConverter("buildAdvancedTower"))
        {
            SpawnTower("AdvancedTower");
            Debug.Log("Advanced tower built!");
        }
        else
        {
            ShowMessage("Not enough gold!");
        }
    }
}
```

### ストラテジー リソース生産チェーン

```csharp
// Machinations Asset設定
// Flow: mineGold (+2 Gold/s)
// Flow: chopWood (+1 Wood/s)
// Converter: woodToGold (10 Wood → 5 Gold)

public class ResourceProduction : MonoBehaviour
{
    [SerializeField] private GameKitManager economy;
    private GameKitResourceManager resourceManager;

    void Start()
    {
        resourceManager = economy.GetComponent<GameKitResourceManager>();
        
        // 自動資源生産を有効化
        var controller = economy.gameObject.AddComponent<GameKitMachinationsController>();
        controller.SetAutoProcessFlows(true);
    }

    public void SellWoodForGold()
    {
        // 木材をゴールドに変換
        if (resourceManager.ExecuteConverter("woodToGold", amount: 1f))
        {
            Debug.Log("Sold wood for gold!");
        }
    }
}
```

## GameKitMachinationsController 設定

### Inspector設定

| プロパティ | 説明 | デフォルト |
|-----------|------|-----------|
| Auto Process Flows | Flowsを毎フレーム実行 | `true` |
| Auto Process Converters | Convertersを自動実行 | `false` |
| Auto Check Triggers | Triggersを毎フレームチェック | `true` |
| Time Scale | Flow処理の時間スケール | `1.0` |
| Converter Interval | Converter実行間隔（秒） | `0.0` |
| Log Execution | 実行ログを出力 | `false` |

### ランタイム制御

```csharp
var controller = manager.GetComponent<GameKitMachinationsController>();

// フローのオン/オフ
controller.SetAutoProcessFlows(true);

// 時間スケール調整（スローモーション/早送り）
controller.SetTimeScale(2.0f); // 2倍速

// 手動実行
controller.ProcessFlowsOnce(Time.deltaTime);
controller.ProcessConvertersOnce();
controller.CheckTriggersOnce();
```

## パフォーマンス最適化

### 選択的実行

```csharp
// すべてのFlowsではなく、特定のFlowのみ処理
void Update()
{
    resourceManager.ProcessFlow("manaRegen", Time.deltaTime);
    resourceManager.ProcessFlow("healthRegen", Time.deltaTime);
    // 他のFlowsはスキップ
}
```

### 間隔実行

```csharp
float triggerCheckInterval = 0.5f; // 0.5秒ごと
float timer = 0f;

void Update()
{
    // Flowsは毎フレーム
    resourceManager.ProcessFlows(Time.deltaTime);
    
    // Triggersは間隔を空けてチェック
    timer += Time.deltaTime;
    if (timer >= triggerCheckInterval)
    {
        resourceManager.CheckTriggers();
        timer = 0f;
    }
}
```

### 条件付き実行

```csharp
void Update()
{
    // ゲーム中のみ実行
    if (GameState.IsPlaying)
    {
        resourceManager.ProcessFlows(Time.deltaTime);
    }
    
    // メニュー画面では停止
}
```

## トラブルシューティング

### Q: Flowsが実行されない
**A**: 以下を確認してください：
1. MachinationsAssetが割り当てられているか
2. Flow定義の`enabledByDefault`が`true`か
3. `ProcessFlows()`が呼ばれているか

### Q: Converterが失敗する
**A**: 以下を確認してください：
1. 入力リソースが十分にあるか（`GetResource()`で確認）
2. Converter IDが正しいか
3. `enabledByDefault`が`true`か

### Q: Triggerが反応しない
**A**: 以下を確認してください：
1. 閾値の設定が正しいか
2. `CheckTriggers()`が呼ばれているか
3. リソース値が実際に閾値を超えているか

### Q: Controllerを追加したが動作しない
**A**: 以下を確認してください：
1. MachinationsAssetが設定されているか
2. Auto実行フラグが有効か
3. ゲームが実行中か（EditモードではUpdate()は動作しない）

## ベストプラクティス

### 1. 役割分担
- **GameKitResourceManager**: 基本的なリソース操作のみ
- **GameKitMachinationsController**: 自動実行が必要な場合のみ使用
- **カスタムスクリプト**: ゲーム固有のロジック

### 2. 手動 vs 自動
- **自動実行**: 常時処理が必要（マナ回復、時間経過）
- **手動実行**: イベント駆動（建設、購入、スキル使用）

### 3. パフォーマンス
- 不要なConverterは`enabledByDefault = false`に
- Trigger頻度を調整（毎フレームでなく0.5秒ごとなど）
- 特定のFlowのみ処理する

### 4. デバッグ
- `Log Execution`を有効化してダイアグラム実行を監視
- `OnResourceChanged`イベントでリソース変更をトラッキング

## 関連ドキュメント

- [GameKitResourceManager.README.md](./GameKitResourceManager.README.md) - リソース管理の基本
- [GameKitMachinations.README.md](./GameKitMachinations.README.md) - Machinations Asset
- [GameKitManager README](./README.md) - GameKitManager概要

---

**ダイアグラムに従った動的なリソース制御で、複雑なゲーム経済を実現！**

