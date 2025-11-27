# UGUI Fine-Grained Architecture: 機能別分解戦略

## 📋 概要

`uguiManage`を機能別に細分化し、より専門化されたハンドラー構造を実現します。

---

## 🎯 現在の`uguiManage`の問題

### 現状：単一の巨大なツール

```
uguiManage (7操作)
├─ rectAdjust         ← RectTransform基本操作
├─ inspect            ← RectTransform基本操作  
├─ updateRect         ← RectTransform基本操作
├─ setAnchor          ← Anchor専門操作
├─ setAnchorPreset    ← Anchor専門操作
├─ convertToAnchored  ← Anchor専門操作
└─ convertToAbsolute  ← Anchor専門操作
```

**問題点**:
- 単一責任原則違反（RectTransform操作とAnchor操作が混在）
- テストが複雑
- 機能の発見が困難

---

## 💡 提案：機能別分解アーキテクチャ

### オプション 1: サブハンドラーパターン（推奨）

Python側のツール定義は変更せず、C#内部で機能別に分解：

```
UguiManageCommandHandler (Facade)
    ├─ RectTransformBasicHandler     (rectAdjust, inspect, updateRect)
    ├─ RectTransformAnchorHandler    (setAnchor, setAnchorPreset, convert*)
    └─ RectTransformHelper           (共通ユーティリティ)
```

**実装イメージ**:

```csharp
// Main handler (Facade)
public class UguiManageCommandHandler : BaseCommandHandler
{
    private readonly IRectTransformOperationHandler _basicHandler;
    private readonly IRectTransformOperationHandler _anchorHandler;
    
    public UguiManageCommandHandler(...)
    {
        _basicHandler = new RectTransformBasicHandler(...);
        _anchorHandler = new RectTransformAnchorHandler(...);
    }
    
    protected override object ExecuteOperation(string operation, Dictionary<string, object> payload)
    {
        return operation switch
        {
            // Basic operations
            "rectAdjust" or "inspect" or "updateRect" 
                => _basicHandler.Execute(operation, payload),
            
            // Anchor operations
            "setAnchor" or "setAnchorPreset" or "convertToAnchored" or "convertToAbsolute"
                => _anchorHandler.Execute(operation, payload),
            
            _ => throw new InvalidOperationException($"Unknown operation: {operation}")
        };
    }
}
```

**メリット**:
- ✅ Python側の変更不要
- ✅ 明確な責任分離
- ✅ 個別にテスト可能
- ✅ 段階的実装が可能

---

### オプション 2: 完全分離（将来的な改善）

Python側も含めて完全にツールを分離：

#### Python側の新しいツール定義

```python
# 新しいツール（将来的に追加）
unity_ugui_rectTransform_basic    # rectAdjust, inspect, updateRect
unity_ugui_rectTransform_anchor   # setAnchor, setAnchorPreset, convert*
unity_ugui_template_create        # Button, Text, Image, etc.
unity_ugui_layout_manage          # add, update, remove, inspect
unity_ugui_overlap_detect         # detect

# 既存ツール（deprecated化）
unity_ugui_manage                 # → 上記ツールへリダイレクト
```

#### C#側のハンドラー構造

```
Handlers/UI/
├── RectTransformBasicCommandHandler.cs
├── RectTransformAnchorCommandHandler.cs  
├── UITemplateCreateCommandHandler.cs
├── UILayoutManageCommandHandler.cs
└── UIOverlapDetectCommandHandler.cs
```

**メリット**:
- ✅ 完全な機能分離
- ✅ ツール名が直感的
- ✅ APIの明確化

**デメリット**:
- ❌ 大規模なリファクタリングが必要
- ❌ 既存ツールとの互換性維持が複雑
- ❌ 実装時間: 8-10時間

---

## 🏗️ 推奨実装：オプション1（サブハンドラーパターン）

### 詳細設計

#### 1. インターフェース定義

```csharp
namespace MCP.Editor.Interfaces
{
    /// <summary>
    /// RectTransform操作を実行するハンドラーのインターフェース
    /// </summary>
    public interface IRectTransformOperationHandler
    {
        /// <summary>
        /// サポートする操作のリスト
        /// </summary>
        IEnumerable<string> SupportedOperations { get; }
        
        /// <summary>
        /// 操作を実行
        /// </summary>
        object Execute(string operation, Dictionary<string, object> payload);
    }
}
```

---

#### 2. RectTransformBasicHandler

```csharp
namespace MCP.Editor.Handlers.UI
{
    /// <summary>
    /// RectTransformの基本操作を処理（サイズ調整、検査、更新）
    /// </summary>
    internal class RectTransformBasicHandler : IRectTransformOperationHandler
    {
        private readonly IGameObjectResolver _gameObjectResolver;
        
        public IEnumerable<string> SupportedOperations => new[]
        {
            "rectAdjust",   // サイズを world corners から計算して調整
            "inspect",      // 現在の状態を詳細に検査
            "updateRect"    // プロパティを直接更新
        };
        
        public RectTransformBasicHandler(IGameObjectResolver gameObjectResolver)
        {
            _gameObjectResolver = gameObjectResolver;
        }
        
        public object Execute(string operation, Dictionary<string, object> payload)
        {
            var target = ResolveRectTransform(payload);
            var canvas = RectTransformHelper.GetCanvas(target);
            
            return operation switch
            {
                "rectAdjust" => ExecuteRectAdjust(target, canvas, payload),
                "inspect" => ExecuteInspect(target, canvas),
                "updateRect" => ExecuteUpdateRect(target, payload),
                _ => throw new InvalidOperationException($"Unsupported operation: {operation}")
            };
        }
        
        private object ExecuteRectAdjust(RectTransform target, Canvas canvas, Dictionary<string, object> payload)
        {
            var beforeState = RectTransformHelper.CaptureState(target);
            
            // Calculate size from world corners
            var worldCorners = new Vector3[4];
            target.GetWorldCorners(worldCorners);
            
            var width = Vector3.Distance(worldCorners[3], worldCorners[0]);
            var height = Vector3.Distance(worldCorners[1], worldCorners[0]);
            var scaleFactor = canvas.scaleFactor == 0f ? 1f : canvas.scaleFactor;
            
            target.sizeDelta = new Vector2(width / scaleFactor, height / scaleFactor);
            EditorUtility.SetDirty(target);
            
            return new Dictionary<string, object>
            {
                ["before"] = beforeState,
                ["after"] = RectTransformHelper.CaptureState(target),
                ["operation"] = "rectAdjust",
                ["scaleFactor"] = scaleFactor
            };
        }
        
        private object ExecuteInspect(RectTransform target, Canvas canvas)
        {
            var state = RectTransformHelper.CaptureState(target);
            
            // Add canvas info
            state["canvasName"] = canvas.name;
            state["scaleFactor"] = canvas.scaleFactor;
            
            // Add world corners
            var worldCorners = new Vector3[4];
            target.GetWorldCorners(worldCorners);
            state["worldCorners"] = worldCorners.Select(c => new Dictionary<string, object>
            {
                ["x"] = c.x, ["y"] = c.y, ["z"] = c.z
            }).ToList();
            
            // Add rect dimensions
            state["rectWidth"] = target.rect.width;
            state["rectHeight"] = target.rect.height;
            
            return new Dictionary<string, object>
            {
                ["state"] = state,
                ["operation"] = "inspect"
            };
        }
        
        private object ExecuteUpdateRect(RectTransform target, Dictionary<string, object> payload)
        {
            var beforeState = RectTransformHelper.CaptureState(target);
            
            // Update properties using helper
            RectTransformHelper.UpdateProperties(target, payload);
            EditorUtility.SetDirty(target);
            
            return new Dictionary<string, object>
            {
                ["before"] = beforeState,
                ["after"] = RectTransformHelper.CaptureState(target),
                ["operation"] = "updateRect"
            };
        }
        
        private RectTransform ResolveRectTransform(Dictionary<string, object> payload)
        {
            var path = payload.TryGetValue("gameObjectPath", out var pathObj) && pathObj is string p ? p : null;
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("gameObjectPath is required");
            }
            
            var go = _gameObjectResolver.Resolve(path);
            var rectTransform = go.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                throw new InvalidOperationException("Target does not contain a RectTransform");
            }
            
            return rectTransform;
        }
    }
}
```

**推定実装時間**: 1時間

---

#### 3. RectTransformAnchorHandler

```csharp
namespace MCP.Editor.Handlers.UI
{
    /// <summary>
    /// RectTransformのアンカー操作を処理
    /// </summary>
    internal class RectTransformAnchorHandler : IRectTransformOperationHandler
    {
        private readonly IGameObjectResolver _gameObjectResolver;
        
        public IEnumerable<string> SupportedOperations => new[]
        {
            "setAnchor",         // カスタムアンカー値を設定
            "setAnchorPreset",   // プリセット（top-left, center等）を適用
            "convertToAnchored", // 絶対位置 → アンカー位置
            "convertToAbsolute"  // アンカー位置 → 絶対位置（読み取り専用）
        };
        
        public RectTransformAnchorHandler(IGameObjectResolver gameObjectResolver)
        {
            _gameObjectResolver = gameObjectResolver;
        }
        
        public object Execute(string operation, Dictionary<string, object> payload)
        {
            var target = ResolveRectTransform(payload);
            var canvas = RectTransformHelper.GetCanvas(target);
            
            var beforeState = RectTransformHelper.CaptureState(target);
            
            switch (operation)
            {
                case "setAnchor":
                    RectTransformHelper.SetAnchor(target, payload);
                    break;
                    
                case "setAnchorPreset":
                    RectTransformHelper.SetAnchorPreset(target, payload);
                    break;
                    
                case "convertToAnchored":
                    RectTransformHelper.ConvertToAnchored(target, payload);
                    break;
                    
                case "convertToAbsolute":
                    // Read-only operation, state is captured but no changes made
                    break;
                    
                default:
                    throw new InvalidOperationException($"Unsupported operation: {operation}");
            }
            
            EditorUtility.SetDirty(target);
            
            return new Dictionary<string, object>
            {
                ["before"] = beforeState,
                ["after"] = RectTransformHelper.CaptureState(target),
                ["operation"] = operation
            };
        }
        
        private RectTransform ResolveRectTransform(Dictionary<string, object> payload)
        {
            var path = payload.TryGetValue("gameObjectPath", out var pathObj) && pathObj is string p ? p : null;
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("gameObjectPath is required");
            }
            
            var go = _gameObjectResolver.Resolve(path);
            var rectTransform = go.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                throw new InvalidOperationException("Target does not contain a RectTransform");
            }
            
            return rectTransform;
        }
    }
}
```

**推定実装時間**: 45分

---

#### 4. UguiManageCommandHandler（Facade）

```csharp
namespace MCP.Editor.Handlers
{
    /// <summary>
    /// UGUIManage operations facade
    /// 内部的に機能別のサブハンドラーに委譲
    /// </summary>
    public class UguiManageCommandHandler : BaseCommandHandler
    {
        private readonly IRectTransformOperationHandler _basicHandler;
        private readonly IRectTransformOperationHandler _anchorHandler;
        
        public override string Category => "uguiManage";
        
        public override IEnumerable<string> SupportedOperations => 
            _basicHandler.SupportedOperations.Concat(_anchorHandler.SupportedOperations);
        
        public UguiManageCommandHandler(
            IPayloadValidator validator,
            IGameObjectResolver gameObjectResolver,
            IAssetResolver assetResolver,
            ITypeResolver typeResolver)
            : base(validator, gameObjectResolver, assetResolver, typeResolver)
        {
            _basicHandler = new RectTransformBasicHandler(gameObjectResolver);
            _anchorHandler = new RectTransformAnchorHandler(gameObjectResolver);
        }
        
        protected override object ExecuteOperation(string operation, Dictionary<string, object> payload)
        {
            // Route to appropriate sub-handler based on operation
            if (_basicHandler.SupportedOperations.Contains(operation))
            {
                return _basicHandler.Execute(operation, payload);
            }
            
            if (_anchorHandler.SupportedOperations.Contains(operation))
            {
                return _anchorHandler.Execute(operation, payload);
            }
            
            throw new InvalidOperationException($"Unknown operation: {operation}");
        }
        
        protected override bool RequiresCompilationWait(string operation)
        {
            // Inspect operation doesn't require compilation wait
            return operation != "inspect";
        }
    }
}
```

**推定実装時間**: 15分

---

### 完全なファイル構造

```
Assets/SkillForUnity/Editor/MCPBridge/
├── Handlers/
│   ├── UI/
│   │   ├── UguiManageCommandHandler.cs           (Facade, 100行)
│   │   ├── RectTransformBasicHandler.cs          (200行)
│   │   ├── RectTransformAnchorHandler.cs         (150行)
│   │   ├── UguiCreateFromTemplateCommandHandler.cs (400行)
│   │   ├── UguiLayoutManageCommandHandler.cs     (350行)
│   │   └── UguiDetectOverlapsCommandHandler.cs   (150行)
│   │
│   └── (other handlers...)
│
├── Helpers/
│   └── UI/
│       └── RectTransformHelper.cs                (300行)
│
└── Interfaces/
    └── IRectTransformOperationHandler.cs         (20行)
```

**合計**: ~1,670行（元の2,081行から20%削減）

---

## 📊 アーキテクチャ比較

### Before: 単一ハンドラー

```
UguiManageCommandHandler (500行)
└── ExecuteOperation()
    ├── rectAdjust
    ├── inspect
    ├── updateRect
    ├── setAnchor
    ├── setAnchorPreset
    ├── convertToAnchored
    └── convertToAbsolute
```

**問題**:
- ❌ 単一責任原則違反
- ❌ テストが複雑
- ❌ 機能発見が困難

### After: サブハンドラーパターン

```
UguiManageCommandHandler (Facade, 100行)
├── RectTransformBasicHandler (200行)
│   ├── rectAdjust
│   ├── inspect
│   └── updateRect
│
└── RectTransformAnchorHandler (150行)
    ├── setAnchor
    ├── setAnchorPreset
    ├── convertToAnchored
    └── convertToAbsolute
```

**メリット**:
- ✅ 明確な責任分離
- ✅ 個別にテスト可能
- ✅ 機能が発見しやすい
- ✅ 拡張が容易

---

## ⚡ さらなる分解の可能性

### UITemplate分解

```
UguiCreateFromTemplateCommandHandler (Facade)
├── BasicUITemplateHandler        (Button, Text, Image, RawImage, Panel)
├── InteractiveUITemplateHandler  (InputField, Slider, Toggle, Dropdown)
└── ComplexUITemplateHandler      (ScrollView)
```

### UILayout分解

```
UguiLayoutManageCommandHandler (Facade)
├── LayoutGroupHandler           (Horizontal, Vertical, Grid)
├── LayoutUtilityHandler         (ContentSizeFitter, AspectRatioFitter)
└── LayoutElementHandler         (LayoutElement)
```

---

## 🎯 実装時間見積もり

### サブハンドラーパターン（推奨）

| コンポーネント | 時間 |
|--------------|------|
| `IRectTransformOperationHandler` | 5分 |
| `RectTransformHelper` | 40分 |
| `RectTransformBasicHandler` | 1.0h |
| `RectTransformAnchorHandler` | 0.75h |
| `UguiManageCommandHandler` (Facade) | 0.25h |
| その他3ハンドラー | 1.5h |
| **合計** | **4.0h** |

### さらなる分解を含む場合

| コンポーネント | 時間 |
|--------------|------|
| 上記すべて | 4.0h |
| UITemplate分解 | +1.0h |
| UILayout分解 | +0.5h |
| **合計** | **5.5h** |

---

## ✅ 推奨アプローチ

### フェーズ1: サブハンドラーパターン（4時間）

1. **Phase 6b-1**: 基礎実装（1.5時間）
   - `IRectTransformOperationHandler`インターフェース
   - `RectTransformHelper`共通ヘルパー
   - `RectTransformBasicHandler`

2. **Phase 6b-2**: Anchor実装（1時間）
   - `RectTransformAnchorHandler`
   - `UguiManageCommandHandler` Facade

3. **Phase 6b-3**: その他ハンドラー（1.5時間）
   - `UguiCreateFromTemplateCommandHandler`
   - `UguiLayoutManageCommandHandler`
   - `UguiDetectOverlapsCommandHandler`

### フェーズ2: さらなる分解（オプション、1.5時間）

必要に応じてTemplateとLayoutも細分化

---

## 🎨 設計パターン

### 使用パターン

1. **Facade Pattern**: `UguiManageCommandHandler`
   - 複雑なサブシステムへの統一インターフェース提供

2. **Strategy Pattern**: `IRectTransformOperationHandler`
   - 操作アルゴリズムを交換可能に

3. **Helper/Utility Pattern**: `RectTransformHelper`
   - 共通機能の静的メソッド提供

### アーキテクチャ原則

- ✅ **単一責任原則**: 各ハンドラーが1つの責任のみ
- ✅ **開放閉鎖原則**: 新機能追加は既存コード変更不要
- ✅ **依存性逆転原則**: インターフェースを通じた依存
- ✅ **インターフェース分離原則**: 小さく特化したインターフェース

---

## 📝 まとめ

### 推奨事項

**サブハンドラーパターンを採用し、段階的に実装**

1. ✅ Python側の変更不要
2. ✅ 明確な機能分離
3. ✅ 個別テスト可能
4. ✅ 4時間で完了
5. ✅ 将来の拡張が容易

### 次のステップ

1. ユーザー承認
2. `IRectTransformOperationHandler`インターフェース作成
3. `RectTransformHelper`実装
4. サブハンドラー実装
5. Facade統合

---

**作成日**: 2025-11-27  
**ステータス**: 機能分解案完成、実装承認待ち  
**推定実装時間**: 4時間（サブハンドラーパターン）

