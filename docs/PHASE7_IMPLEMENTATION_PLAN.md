# Phase 7 Implementation Plan: Settings & Utilities Handlers

**日付**: 2025-11-27  
**ステータス**: 🚧 進行中  
**推定時間**: 3-4時間  

---

## 📋 概要

Phase 7では、Settings & Utilities関連の操作を既存の`McpCommandProcessor.Settings.cs`から独立したハンドラーに移行します。

---

## 🎯 実装対象

### 1. TagLayerManageHandler
**ツール**: `unity_tagLayer_manage`  
**操作**: 11操作
- GameObject操作: `setTag`, `getTag`, `setLayer`, `getLayer`, `setLayerRecursive`
- Project操作: `listTags`, `addTag`, `removeTag`, `listLayers`, `addLayer`, `removeLayer`

**推定時間**: 45分

### 2. ProjectSettingsManageHandler
**ツール**: `unity_projectSettings_manage`  
**操作**: 3操作
- `read` - 設定の読み取り
- `write` - 設定の書き込み
- `list` - 利用可能な設定のリスト

**サポートカテゴリ**: 6種類
- `player` - PlayerSettings
- `quality` - QualitySettings
- `time` - Time settings
- `physics` - Physics settings
- `audio` - AudioSettings
- `editor` - EditorSettings

**推定時間**: 1時間

### 3. RenderPipelineManageHandler
**ツール**: `unity_renderPipeline_manage`  
**操作**: 4操作
- `inspect` - 現在のパイプライン検査
- `setAsset` - パイプラインアセット設定
- `getSettings` - パイプライン設定取得
- `updateSettings` - パイプライン設定更新

**推定時間**: 30分

### 4. ConstantConvertHandler
**ツール**: `unity_constant_convert`  
**操作**: 9操作
- Enum変換: `enumToValue`, `valueToEnum`, `listEnums`
- Color変換: `colorToRGBA`, `rgbaToColor`, `listColors`
- Layer変換: `layerToIndex`, `indexToLayer`, `listLayers`

**推定時間**: 30分

### 5. CompilationAwaitHandler
**ツール**: `unity_await_compilation`  
**操作**: 1操作
- `await` - コンパイル完了待機

**推定時間**: 15分

---

## 🏗️ アーキテクチャ

### ファイル構造

```
Assets/SkillForUnity/Editor/MCPBridge/
├── Handlers/
│   ├── Settings/
│   │   ├── TagLayerManageHandler.cs           (~400行)
│   │   ├── ProjectSettingsManageHandler.cs    (~600行)
│   │   ├── RenderPipelineManageHandler.cs     (~250行)
│   │   └── ConstantConvertHandler.cs          (~500行)
│   └── CompilationAwaitHandler.cs             (~100行)
└── Base/
    └── CommandHandlerInitializer.cs (更新)
```

### 既存コードからの移行

**元ファイル**: `Assets/SkillForUnity/Editor/MCPBridge/Settings/McpCommandProcessor.Settings.cs` (約1,700行)

**移行後の合計**: 約1,850行（各ハンドラー独立化のため若干増加）

---

## ✅ 実装順序

### Phase 7-1: TagLayerManageHandler (45分)
1. ✅ 既存実装確認
2. ⏳ `TagLayerManageHandler.cs`作成
3. ⏳ GameObject操作実装（5操作）
4. ⏳ Project操作実装（6操作）
5. ⏳ ハンドラー登録

### Phase 7-2: ProjectSettingsManageHandler (1時間)
1. ⏳ `ProjectSettingsManageHandler.cs`作成
2. ⏳ Read操作実装（6カテゴリ）
3. ⏳ Write操作実装（6カテゴリ）
4. ⏳ List操作実装
5. ⏳ ハンドラー登録

### Phase 7-3: RenderPipelineManageHandler (30分)
1. ⏳ `RenderPipelineManageHandler.cs`作成
2. ⏳ Inspect/SetAsset実装
3. ⏳ GetSettings/UpdateSettings実装
4. ⏳ ハンドラー登録

### Phase 7-4: ConstantConvertHandler (30分)
1. ⏳ `ConstantConvertHandler.cs`作成
2. ⏳ Enum変換実装（3操作）
3. ⏳ Color変換実装（3操作）
4. ⏳ Layer変換実装（3操作）
5. ⏳ ハンドラー登録

### Phase 7-5: CompilationAwaitHandler (15分)
1. ⏳ `CompilationAwaitHandler.cs`作成
2. ⏳ Await実装
3. ⏳ ハンドラー登録

### Phase 7-6: 最終確認とレポート (30分)
1. ⏳ すべてのハンドラー登録確認
2. ⏳ コンパイルエラーチェック
3. ⏳ Phase7実装レポート作成
4. ⏳ Git commit & push

---

## 📊 進捗状況

| ハンドラー | ステータス | 進捗 |
|----------|----------|------|
| TagLayerManageHandler | ⏳ 準備中 | 0% |
| ProjectSettingsManageHandler | ⏳ 準備中 | 0% |
| RenderPipelineManageHandler | ⏳ 準備中 | 0% |
| ConstantConvertHandler | ⏳ 準備中 | 0% |
| CompilationAwaitHandler | ⏳ 準備中 | 0% |
| **合計** | **⏳ 進行中** | **0%** |

---

## 🎨 設計パターン

### BaseCommandHandlerの活用
すべてのハンドラーは`BaseCommandHandler`を継承し、以下を活用：
- ペイロード検証（`Validator`）
- リソース解決（`GameObjectResolver`, `AssetResolver`, `TypeResolver`）
- コンパイル待機（`RequiresCompilationWait`）
- 共通ヘルパー（`GetString`, `GetInt`, `GetBool`, `GetFloat`）

### 責任分離
- 各ハンドラーは単一の機能領域を担当
- Settings.csのような巨大ファイルを回避
- テスト容易性の向上

---

## 📝 次のステップ

1. Phase 7-1から順番に実装
2. 各ステップでコンパイルエラーチェック
3. すべて完了後、Phase 7実装レポート作成

---

**作成日**: 2025-11-27  
**最終更新**: 2025-11-27  
**ステータス**: 🚧 進行中

