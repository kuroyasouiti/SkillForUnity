# Changelog

All notable changes to SkillForUnity will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.8.0] - 2025-11-29

### Added

#### New Tools
- **Prefab Management** (`unity_prefab_crud`)
  - Create prefabs from GameObjects
  - Update, inspect, instantiate prefabs
  - Unpack prefabs (completely or outermost)
  - Apply/revert prefab overrides
  
- **Vector Sprite Conversion** (`unity_vector_sprite_convert`)
  - Generate sprites from primitives (square, circle, triangle, polygon)
  - Import SVG to sprite
  - Convert textures to sprites
  - Create solid color sprites

#### GameKit Framework (High-Level Tools)
- **GameKit Actor** (`unity_gamekit_actor`)
  - Behavior profiles: 2D/3D physics, linear, tilemap movement
  - Control modes: direct controller, AI, UI command
  - Stats, abilities, weapon loadouts
  
- **GameKit Manager** (`unity_gamekit_manager`)
  - Manager types: turn-based, realtime, resource pool, event hub, state manager
  - Turn phase management
  - Resource pool with Machinations framework support
  - Persistence (DontDestroyOnLoad)
  
- **GameKit Interaction** (`unity_gamekit_interaction`)
  - Trigger types: collision, trigger, raycast, proximity, input
  - Declarative actions: spawn prefab, destroy object, play sound, send message, change scene
  - Conditions: tag, layer, distance, custom
  
- **GameKit UI Command** (`unity_gamekit_ui_command`)
  - Command panels with button layouts (horizontal, vertical, grid)
  - Actor command dispatch
  - Icon and label support
  
- **GameKit SceneFlow** (`unity_gamekit_sceneflow`)
  - Scene state machine with transitions
  - Additive scene loading
  - Persistent manager scene
  - Shared scene groups (UI, Audio)
  - Scene-crossing reference resolution

#### Mid-Level Tools
- **Transform Batch** (`unity_transform_batch`)
  - Arrange objects in circles/lines
  - Sequential/list-based renaming
  - Auto-generate menu hierarchies
  
- **RectTransform Batch** (`unity_rectTransform_batch`)
  - Set anchors, pivot, size, position
  - Align to parent presets
  - Distribute horizontally/vertically
  - Match size from source
  
- **Physics Bundle** (`unity_physics_bundle`)
  - 2D/3D Rigidbody + Collider presets
  - Presets: dynamic, kinematic, static, character, platformer, topDown, vehicle, projectile
  - Update individual physics properties
  
- **Camera Rig** (`unity_camera_rig`)
  - Camera rig presets: follow, orbit, split-screen, fixed, dolly
  - Target tracking and smooth movement
  - Viewport configuration
  
- **UI Foundation** (`unity_ui_foundation`)
  - Create Canvas, Panel, Button, Text, Image, InputField
  - Anchor presets
  - TextMeshPro support
  - Automatic layout
  
- **Audio Source Bundle** (`unity_audio_source_bundle`)
  - Audio presets: music, sfx, ambient, voice, ui
  - 2D/3D spatial audio
  - Mixer group integration
  
- **Input Profile** (`unity_input_profile`)
  - New Input System integration
  - Action map configuration
  - Notification behaviors: sendMessages, broadcastMessages, invokeUnityEvents, invokeCSharpEvents
  - Create InputActions assets

#### Features
- **Compilation Wait System**
  - Operations execute first, then wait for compilation if triggered
  - Bridge reconnection detection for early wait release
  - 60-second timeout with configurable intervals
  - Transparent wait information in responses
  - Automatic handling in BaseCommandHandler

- **Comprehensive Test Suite**
  - 100+ unit tests covering all tool categories
  - Unity Test Framework integration
  - 97.7% pass rate (42/43 tests)
  - Editor menu integration: `Tools > SkillForUnity > Run All Tests`
  - Command-line test runners (PowerShell, Bash)
  - CI/CD with GitHub Actions

#### Documentation
- Test suite documentation (`Assets/SkillForUnity/Tests/Editor/README.md`)
- Test results summary (`docs/TestResults_Summary.md`)
- Tooling roadmap - Japanese (`docs/tooling-roadmap.ja.md`)
- Compilation wait feature guide (`docs/Compilation_Wait_Feature.md`)
- Legacy cleanup summary (`docs/Unused_Handlers_Cleanup_Summary.md`)

### Changed

- **Tool Count**: Increased from 7 to 21 tools
- **BaseCommandHandler**: Compilation wait moved from before to after operation execution
- **AssetCommandHandler**: Added `AssetDatabase.Refresh()` after create/update operations
- **skill.yml**: Updated tool count and added new categories (prefab_management, sprite_conversion, batch_operations, gamekit_systems)

### Removed

- Legacy test files in `Assets/SkillForUnity/Editor/Tests/`
  - BaseCommandHandlerTests.cs
  - PayloadValidatorTests.cs
  - ResourceResolverTests.cs
  - CommandHandlerIntegrationTests.cs

- Unused handlers (not registered in MCP)
  - TemplateCommandHandler
  - UguiCreateFromTemplateHandler
  - UguiDetectOverlapsHandler
  - UguiLayoutManageHandler
  - UguiManageCommandHandler
  - ConstantConvertHandler
  - RenderPipelineManageHandler
  - TagLayerManageHandler (integrated into ProjectSettingsManageHandler)
  - RectTransformAnchorHandler (functionality in RectTransformBatchHandler)
  - RectTransformBasicHandler (functionality in RectTransformBatchHandler)

### Fixed

- Compilation wait now occurs after operation execution (more reliable)
- Bridge reconnection properly releases compilation wait
- Test suite compilation errors resolved

---

## [1.7.1] - 2025-11-XX

### Fixed

- **Template Tools**: Fixed scene quick setup, GameObject templates, UI templates, design patterns, script templates
- **Constant Conversion**: Fixed enum type resolution for Unity 2024.2+ module system
- **SerializedField Support**: Added support for `[SerializeField]` private fields in Component and ScriptableObject operations
- **Type Resolution**: 99%+ performance improvement through caching

### Added

- `listCommonEnums` operation: Lists commonly used Unity enum types by category
- Enhanced error messages with debugging information

### Changed

- Streamlined toolset: Focus on low-level CRUD operations

---

## [1.7.0] - 2025-XX-XX

### Added

- Initial MCP server implementation
- WebSocket bridge for Unity Editor
- Core CRUD operations: Scene, GameObject, Component, Asset, ScriptableObject
- Project settings management

---

[1.8.0]: https://github.com/kuroyasouiti/SkillForUnity/releases/tag/v1.8.0
[1.7.1]: https://github.com/kuroyasouiti/SkillForUnity/releases/tag/v1.7.1
[1.7.0]: https://github.com/kuroyasouiti/SkillForUnity/releases/tag/v1.7.0
