# UGUI Architecture Review: 最適な実装戦略

## 📋 Executive Summary

**重要な発見**: UGUIツールは実際には**4つのハンドラー**で実装可能です（6つではなく）！

deprecatedツール2つを除外し、`uguiManage`が統合ツールであることを活用すれば、実装時間を**50%削減**できます。

---

## 🔍 現状分析

### ツール構成（Python → C#）

| # | Python Tool Name | C# Handler Name | Status | 行数 | 複雑度 |
|---|-----------------|-----------------|--------|------|-------|
| 1 | `unity_ugui_manage` | `uguiManage` | ✅ Active | ~600 | ⭐⭐⭐ |
| 2 | `unity_ugui_createFromTemplate` | `uguiCreateFromTemplate` | ✅ Active | ~550 | ⭐⭐ |
| 3 | `unity_ugui_layoutManage` | `uguiLayoutManage` | ✅ Active | ~530 | ⭐⭐ |
| 4 | `unity_ugui_detectOverlaps` | `uguiDetectOverlaps` | ✅ Active | ~180 | ⭐ |
| 5 | `unity_ugui_rectAdjust` | `uguiRectAdjust` | ⚠️ **DEPRECATED** | ~100 | ⭐ |
| 6 | `unity_ugui_anchorManage` | `uguiAnchorManage` | ⚠️ **DEPRECATED** | ~120 | ⭐⭐ |

### `uguiManage`の統合性

`uguiManage`は**統合ツール**で、以下の操作をサポート：

```python
# uguiManage operations (7操作)
"rectAdjust"         # ← uguiRectAdjustの機能を含む
"setAnchor"          # ← uguiAnchorManageの機能を含む
"setAnchorPreset"    # ← uguiAnchorManageの機能を含む
"convertToAnchored"  # ← uguiAnchorManageの機能を含む
"convertToAbsolute"  # ← uguiAnchorManageの機能を含む
"inspect"
"updateRect"
```

**つまり**: `uguiRectAdjust`と`uguiAnchorManage`はレガシー互換性のためだけに存在し、実際の機能はすべて`uguiManage`に統合されています！

---

## 💡 推奨アーキテクチャ

### オプション A: 4ハンドラー戦略（推奨）

新しいハンドラーシステムでは、**アクティブな4ツールのみ実装**：

```
Assets/SkillForUnity/Editor/MCPBridge/Handlers/UI/
├── UguiManageCommandHandler.cs          (~500行)  🔴 優先度: 高
├── UguiCreateFromTemplateCommandHandler.cs (~400行)  🟠 優先度: 中
├── UguiLayoutManageCommandHandler.cs    (~350行)  🟠 優先度: 中
└── UguiDetectOverlapsCommandHandler.cs  (~150行)  🟡 優先度: 低

Assets/SkillForUnity/Editor/MCPBridge/Helpers/UI/
└── RectTransformHelper.cs               (~300行)  共通ヘルパー
```

**メリット**:
- ✅ 実装するハンドラー数: 6 → 4（33%削減）
- ✅ 実装時間: 6.5時間 → **3.5時間**（46%削減）
- ✅ deprecatedツールは既存のレガシーシステムで継続
- ✅ 明確な責任分離
- ✅ テスト容易性

**Deprecated ツールの処理**:

```csharp
// McpCommandProcessor.cs - ExecuteLegacy()
case "uguiRectAdjust" => HandleUguiRectAdjust(command.Payload),
case "uguiAnchorManage" => HandleUguiAnchorManage(command.Payload),
```

レガシーシステムで継続サポート（既存コードはそのまま）。

---

## 🏗️ 詳細設計

### 1. UguiManageCommandHandler

**責任**: RectTransform の統合管理

```csharp
public class UguiManageCommandHandler : BaseCommandHandler
{
    public override string Category => "uguiManage";
    
    public override IEnumerable<string> SupportedOperations => new[]
    {
        "rectAdjust",
        "setAnchor",
        "setAnchorPreset",
        "convertToAnchored",
        "convertToAbsolute",
        "inspect",
        "updateRect"
    };
    
    protected override object ExecuteOperation(string operation, Dictionary<string, object> payload)
    {
        var target = ResolveRectTransform(payload);
        var canvas = GetCanvas(target);
        
        return operation switch
        {
            "rectAdjust" => ExecuteRectAdjust(target, canvas, payload),
            "setAnchor" => ExecuteSetAnchor(target, payload),
            "setAnchorPreset" => ExecuteSetAnchorPreset(target, payload),
            "convertToAnchored" => ExecuteConvertToAnchored(target, payload),
            "convertToAbsolute" => ExecuteConvertToAbsolute(target, payload),
            "inspect" => ExecuteInspect(target, canvas),
            "updateRect" => ExecuteUpdateRect(target, payload),
            _ => throw new InvalidOperationException($"Unknown operation: {operation}")
        };
    }
    
    // Helper methods...
}
```

**実装ポイント**:
- 既存の`HandleUguiManage()`から移植
- `RectTransformHelper`を活用
- 7操作を独立メソッドとして実装

**推定時間**: 2時間

---

### 2. UguiCreateFromTemplateCommandHandler

**責任**: UIテンプレートからGameObject作成

```csharp
public class UguiCreateFromTemplateCommandHandler : BaseCommandHandler
{
    public override string Category => "uguiCreateFromTemplate";
    
    public override IEnumerable<string> SupportedOperations => new[]
    {
        "Button", "Text", "Image", "RawImage", "Panel",
        "ScrollView", "InputField", "Slider", "Toggle", "Dropdown"
    };
    
    protected override object ExecuteOperation(string operation, Dictionary<string, object> payload)
    {
        // 'operation'はここでは'template'として解釈
        var template = GetString(payload, "template");
        if (string.IsNullOrEmpty(template))
        {
            throw new InvalidOperationException("template is required");
        }
        
        return CreateFromTemplate(template, payload);
    }
    
    private object CreateFromTemplate(string template, Dictionary<string, object> payload)
    {
        var parent = ResolveParentOrFindCanvas(payload);
        var name = GetString(payload, "name") ?? template;
        
        GameObject go = template switch
        {
            "Button" => CreateButton(name, parent, payload),
            "Text" => CreateText(name, parent, payload),
            // ... 他のテンプレート
            _ => throw new InvalidOperationException($"Unknown template: {template}")
        };
        
        return CreateSuccessResponse(
            ("template", template),
            ("gameObjectPath", GetHierarchyPath(go)),
            ("name", go.name)
        );
    }
}
```

**実装ポイント**:
- テンプレート作成ロジックは既存コードから移植
- `RectTransformHelper`でサイズ/位置設定を共通化

**推定時間**: 1時間

---

### 3. UguiLayoutManageCommandHandler

**責任**: レイアウトコンポーネント管理

```csharp
public class UguiLayoutManageCommandHandler : BaseCommandHandler
{
    public override string Category => "uguiLayoutManage";
    
    public override IEnumerable<string> SupportedOperations => new[]
    {
        "add", "update", "remove", "inspect"
    };
    
    protected override object ExecuteOperation(string operation, Dictionary<string, object> payload)
    {
        var go = ResolveGameObject(GetString(payload, "gameObjectPath"));
        
        return operation switch
        {
            "add" => AddLayoutComponent(go, payload),
            "update" => UpdateLayoutComponent(go, payload),
            "remove" => RemoveLayoutComponent(go, payload),
            "inspect" => InspectLayoutComponent(go, payload),
            _ => throw new InvalidOperationException($"Unknown operation: {operation}")
        };
    }
}
```

**実装ポイント**:
- 6種類のレイアウトコンポーネントをサポート
- Apply/Serializeヘルパーメソッドで共通化

**推定時間**: 30分

---

### 4. UguiDetectOverlapsCommandHandler

**責任**: UI要素のオーバーラップ検出

```csharp
public class UguiDetectOverlapsCommandHandler : BaseCommandHandler
{
    public override string Category => "uguiDetectOverlaps";
    
    public override IEnumerable<string> SupportedOperations => new[] { "detect" };
    
    protected override object ExecuteOperation(string operation, Dictionary<string, object> payload)
    {
        var checkAll = GetBool(payload, "checkAll", false);
        var threshold = GetFloat(payload, "threshold", 0f);
        
        List<Dictionary<string, object>> overlaps;
        
        if (checkAll)
        {
            overlaps = DetectAllOverlaps(threshold);
        }
        else
        {
            var target = ResolveGameObject(GetString(payload, "gameObjectPath"));
            var includeChildren = GetBool(payload, "includeChildren", false);
            overlaps = DetectTargetOverlaps(target, includeChildren, threshold);
        }
        
        return CreateSuccessResponse(
            ("overlaps", overlaps),
            ("count", overlaps.Count)
        );
    }
}
```

**実装ポイント**:
- シンプルなロジック
- 既存コードをほぼそのまま移植

**推定時間**: 20分

---

### 5. RectTransformHelper（共通ヘルパー）

```csharp
namespace MCP.Editor.Helpers
{
    /// <summary>
    /// RectTransformの共通操作を提供するヘルパークラス
    /// </summary>
    public static class RectTransformHelper
    {
        // State capture
        public static Dictionary<string, object> CaptureState(RectTransform rectTransform) { }
        
        // Anchor operations
        public static void SetAnchor(RectTransform rectTransform, float minX, float minY, float maxX, float maxY, bool preservePosition) { }
        public static void SetAnchorPreset(RectTransform rectTransform, string preset, bool preservePosition) { }
        public static void RestoreWorldCorners(RectTransform rectTransform, Vector3[] corners) { }
        
        // Position conversion
        public static void ConvertToAnchored(RectTransform rectTransform, float? absoluteX, float? absoluteY) { }
        
        // Property updates
        public static void UpdateProperties(RectTransform rectTransform, Dictionary<string, object> payload) { }
        
        // Common settings
        public static void ApplyCommonSettings(RectTransform rectTransform, Dictionary<string, object> payload, float defaultWidth, float defaultHeight) { }
        
        // Validation
        public static Canvas GetCanvas(RectTransform rectTransform) { }
        public static void ValidateRectTransform(GameObject go) { }
    }
}
```

**実装ポイント**:
- UI.csの共通メソッドを抽出
- すべてのUGUIハンドラーで共有
- 静的メソッドで実装

**推定時間**: 40分

---

## 📊 実装時間の比較

### 当初の見積もり（Phase 6b全体）

| コンポーネント | 時間 |
|--------------|------|
| Core Utilities | 1.0h |
| UguiManageHandler | 2.0h |
| UguiCreateFromTemplateHandler | 1.5h |
| UguiLayoutManageHandler | 1.0h |
| その他3ハンドラー | 1.0h |
| **合計** | **6.5h** |

### 新しい見積もり（4ハンドラー戦略）

| コンポーネント | 時間 |
|--------------|------|
| RectTransformHelper（共通） | 0.7h |
| UguiManageHandler | 2.0h |
| UguiCreateFromTemplateHandler | 1.0h |
| UguiLayoutManageHandler | 0.5h |
| UguiDetectOverlapsHandler | 0.3h |
| **合計** | **3.5h** |

**削減**: 6.5h → 3.5h = **46%削減** 🎉

---

## 🔄 ハンドラー登録

```csharp
// CommandHandlerInitializer.cs

// Phase 6b: UGUI ハンドラー
RegisterHandler("uguiManage", new UguiManageCommandHandler(validator, goResolver, assetResolver, typeResolver));
RegisterHandler("uguiCreateFromTemplate", new UguiCreateFromTemplateCommandHandler(validator, goResolver, assetResolver, typeResolver));
RegisterHandler("uguiLayoutManage", new UguiLayoutManageCommandHandler(validator, goResolver, assetResolver, typeResolver));
RegisterHandler("uguiDetectOverlaps", new UguiDetectOverlapsCommandHandler(validator, goResolver, assetResolver, typeResolver));

// Deprecated tools remain in legacy system
// - uguiRectAdjust
// - uguiAnchorManage
```

---

## 🎯 実装順序（推奨）

### Phase 6b-1: 共通ヘルパー（40分）
1. `RectTransformHelper.cs`作成
2. 既存UI.csから共通メソッド抽出
3. 単体テスト作成

### Phase 6b-2: UguiManageHandler（2時間）
1. ハンドラー本体実装
2. 7操作の実装
3. 統合テスト

### Phase 6b-3: 残りのハンドラー（1時間）
1. UguiCreateFromTemplateHandler
2. UguiLayoutManageHandler
3. UguiDetectOverlapsHandler

### Phase 6b-4: 統合とドキュメント（30分）
1. ハンドラー登録
2. ドキュメント更新
3. 動作確認

**合計**: 約4時間（余裕を見て）

---

## ✅ 実装チェックリスト

### 準備
- [ ] `Assets/.../Handlers/UI/`ディレクトリ作成
- [ ] `Assets/.../Helpers/UI/`ディレクトリ作成

### Phase 6b-1: 共通ヘルパー
- [ ] `RectTransformHelper.cs`作成
- [ ] `CaptureState`実装
- [ ] `SetAnchor`/`SetAnchorPreset`実装
- [ ] `ConvertToAnchored`実装
- [ ] `UpdateProperties`実装
- [ ] `ApplyCommonSettings`実装
- [ ] `GetCanvas`/`ValidateRectTransform`実装

### Phase 6b-2: UguiManageHandler
- [ ] ハンドラークラス作成
- [ ] `ExecuteRectAdjust`実装
- [ ] `ExecuteSetAnchor`/`ExecuteSetAnchorPreset`実装
- [ ] `ExecuteConvertToAnchored`/`ExecuteConvertToAbsolute`実装
- [ ] `ExecuteInspect`実装
- [ ] `ExecuteUpdateRect`実装

### Phase 6b-3: その他ハンドラー
- [ ] `UguiCreateFromTemplateCommandHandler`実装
  - [ ] 10テンプレート作成メソッド
- [ ] `UguiLayoutManageCommandHandler`実装
  - [ ] Add/Update/Remove/Inspect操作
- [ ] `UguiDetectOverlapsCommandHandler`実装
  - [ ] オーバーラップ検出ロジック

### Phase 6b-4: 統合
- [ ] `CommandHandlerInitializer`に登録
- [ ] 各ハンドラーの動作確認
- [ ] ドキュメント更新
  - [ ] `PHASE6B_IMPLEMENTATION_REPORT.md`
  - [ ] `CHANGELOG.md`
  - [ ] `INTERFACE_EXTRACTION.md`

---

## 📈 期待される効果

### Before（現状）

```
UI.cs (2081行) - レガシーシステム
├─ HandleUguiRectAdjust (100行)
├─ HandleUguiAnchorManage (120行)
├─ HandleUguiManage (600行)
├─ HandleUguiCreateFromTemplate (550行)
├─ HandleUguiLayoutManage (530行)
├─ HandleUguiDetectOverlaps (180行)
└─ 37 ヘルパーメソッド
```

### After（新アーキテクチャ）

```
Handlers/UI/
├─ UguiManageCommandHandler.cs (500行) ✨
├─ UguiCreateFromTemplateCommandHandler.cs (400行) ✨
├─ UguiLayoutManageCommandHandler.cs (350行) ✨
└─ UguiDetectOverlapsCommandHandler.cs (150行) ✨

Helpers/UI/
└─ RectTransformHelper.cs (300行) ✨

McpCommandProcessor.UI.cs (220行) - Deprecated only
├─ HandleUguiRectAdjust (100行)
└─ HandleUguiAnchorManage (120行)
```

**メリット**:
- ✅ モジュール性: 4つの独立したハンドラー
- ✅ テスト容易性: 各ハンドラー個別にテスト可能
- ✅ 保守性: 明確な責任分離
- ✅ 拡張性: 新機能追加が容易
- ✅ コード削減: 2081行 → 1700行（18%削減）

---

## 🚀 推奨：即座実装

### なぜ今実装すべきか

1. **実装時間が許容範囲**: 3.5時間は1セッションで完了可能
2. **明確な設計**: アーキテクチャが確立済み
3. **高い価値**: UGUIは使用頻度が高い
4. **リスクが低い**: deprecatedツールはレガシーで継続

### 代替案

もし時間が限られている場合：

**ミニマル実装**（2時間）:
1. `RectTransformHelper`（40分）
2. `UguiManageHandler`のみ（80分）← 最重要

残りは次回セッションで実装。

---

## 結論

**推奨**: オプションA（4ハンドラー戦略）を採用し、**即座に実装**

**理由**:
1. ✅ deprecatedツール除外で実装量46%削減
2. ✅ 3.5時間で完了可能（1セッション内）
3. ✅ 明確なアーキテクチャと責任分離
4. ✅ UGUIは高頻度使用される重要機能
5. ✅ レガシーシステムで下位互換性維持

**次のステップ**:
1. ユーザー承認
2. Phase 6b-1開始（RectTransformHelper実装）
3. 順次ハンドラー実装

---

**作成日**: 2025-11-27  
**ステータス**: アーキテクチャレビュー完了、実装承認待ち  
**推定実装時間**: 3.5時間

