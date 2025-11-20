# SkillForUnity Tools Reference

**Complete reference for all 26 MCP tools available in SkillForUnity.**

Last Updated: 2025-01-14

---

## Table of Contents

1. [Core Tools (3)](#core-tools)
2. [Scene Management (2)](#scene-management)
3. [GameObject Operations (3)](#gameobject-operations)
4. [Component Management (1)](#component-management)
5. [Asset Management (2)](#asset-management)
6. [UI (UGUI) Tools (6)](#ui-ugui-tools)
7. [Prefab Management (1)](#prefab-management)
8. [Advanced Features (7)](#advanced-features)
9. [Utility Tools (1)](#utility-tools)

**Total: 26 Tools**

---

## Core Tools

### 1. `unity_ping`
**Purpose:** Verify bridge connectivity and return heartbeat information

**Parameters:** None

**Example:**
```python
unity_ping()
```

**Returns:** Connection status, Unity version, last heartbeat timestamp

**Use Cases:**
- Test connection to Unity Editor
- Verify bridge is running
- Get Unity version information

---

### 2. `unity_context_inspect`
**Purpose:** Get comprehensive overview of current scene structure

**Parameters:**
- `includeHierarchy` (boolean): Include full scene hierarchy (default: true)
- `includeComponents` (boolean): Include component types (default: false)
- `maxDepth` (integer): Maximum hierarchy depth (default: unlimited)
- `filter` (string): Filter GameObjects by name pattern (supports * and ?)

**Example:**
```python
# Quick overview
unity_context_inspect({
    "includeHierarchy": True,
    "includeComponents": False,
    "maxDepth": 2
})

# Detailed inspection with filter
unity_context_inspect({
    "includeHierarchy": True,
    "includeComponents": True,
    "filter": "Player*"
})
```

**Returns:** Scene hierarchy, active GameObjects, components, selection, Git info

**Performance:** Fast when `includeComponents=false`, slower with full component details

**Use Cases:**
- Understand scene structure before making changes
- Find specific GameObjects by pattern
- Inspect current selection
- Check scene state for debugging

---

### 3. `unity_batch_execute`
**Purpose:** Execute multiple Unity tool operations in a single batch

**Parameters:**
- `operations` (array): List of operations to execute
  - Each has `tool` (tool name) and `payload` (arguments)
- `stopOnError` (boolean): Stop on first error (default: false)
- `awaitCompilation` (boolean): Wait for compilation (default: true)
- `timeoutSeconds` (integer): Compilation timeout (default: 60)

**Example:**
```python
unity_batch_execute({
    "operations": [
        {
            "tool": "gameObjectCreateFromTemplate",
            "payload": {"template": "Cube", "name": "Wall1", "position": {"x": 5, "y": 0, "z": 0}}
        },
        {
            "tool": "gameObjectCreateFromTemplate",
            "payload": {"template": "Cube", "name": "Wall2", "position": {"x": -5, "y": 0, "z": 0}}
        },
        {
            "tool": "componentManage",
            "payload": {"operation": "add", "gameObjectPath": "Wall1", "componentType": "UnityEngine.Rigidbody"}
        }
    ],
    "stopOnError": False
})
```

**Returns:** Results for each operation, compilation status if triggered

**Use Cases:**
- Execute multiple related operations atomically
- Create complex scenes with multiple steps
- Combine GameObject creation with component setup
- Batch asset operations

---

## Scene Management

### 6. `unity_scene_crud`
**Purpose:** Create, load, save, delete scenes and manage build settings

**Operations:**
- **Scene Operations:** create, load, save, delete, duplicate
- **Build Settings:** listBuildSettings, addToBuildSettings, removeFromBuildSettings, reorderBuildSettings, setBuildSettingsEnabled

**Parameters:**
- `operation` (string): Operation to perform
- `scenePath` (string): Path under Assets/ (e.g., Assets/Scenes/Main.unity)
- `newSceneName` (string): For duplicate operation
- `additive` (boolean): Open scene additively (default: false)
- `includeOpenScenes` (boolean): Save all open scenes (default: false)
- `enabled` (boolean): Scene enabled in build settings (default: true)
- `index` (integer): Position in build settings
- `fromIndex` (integer): Source index for reorder
- `toIndex` (integer): Target index for reorder

**Examples:**
```python
# Create new scene
unity_scene_crud({
    "operation": "create",
    "scenePath": "Assets/Scenes/Level1.unity"
})

# Load scene additively
unity_scene_crud({
    "operation": "load",
    "scenePath": "Assets/Scenes/Level1.unity",
    "additive": True
})

# Add scene to build settings
unity_scene_crud({
    "operation": "addToBuildSettings",
    "scenePath": "Assets/Scenes/MainMenu.unity",
    "index": 0,
    "enabled": True
})

# Reorder scenes in build
unity_scene_crud({
    "operation": "reorderBuildSettings",
    "fromIndex": 2,
    "toIndex": 0
})
```

**Use Cases:**
- Manage scene lifecycle
- Set up build configurations
- Work with multiple scenes
- Organize scene order in builds

---

### 7. `unity_scene_quickSetup`
**Purpose:** Instantly set up new scenes with common configurations

**Setup Types:**
- `3D` - Main Camera + Directional Light
- `2D` - 2D Camera (orthographic)
- `UI` - Canvas + EventSystem
- `VR` - VR Camera setup
- `Empty` - Empty scene

**Parameters:**
- `setupType` (string): Type of scene setup
- `includeEventSystem` (boolean): Include EventSystem (default: true for UI)
- `cameraPosition` (object): Camera position {x, y, z}
- `cameraRotation` (object): Camera rotation {x, y, z}
- `lightIntensity` (number): Light intensity for 3D (default: 1.0)

**Example:**
```python
# Set up 3D scene
unity_scene_quickSetup({"setupType": "3D"})

# Set up UI scene with custom camera
unity_scene_quickSetup({
    "setupType": "UI",
    "cameraPosition": {"x": 0, "y": 0, "z": -10}
})
```

**Returns:** Created GameObjects and their paths

**Use Cases:**
- Quick scene initialization
- Standard scene templates
- Rapid prototyping
- Starting new projects

---

## GameObject Operations

### 8. `unity_gameobject_crud`
**Purpose:** Create, modify, delete, and inspect GameObjects

**Operations:**
- **Single:** create, delete, move, rename, duplicate, inspect
- **Multiple:** findMultiple, deleteMultiple, inspectMultiple

**Parameters:**
- `operation` (string): Operation to perform
- `gameObjectPath` (string): Hierarchy path (e.g., Root/Child/Button)
- `parentPath` (string): Target parent path
- `name` (string): GameObject name
- `template` (string): Prefab path or template identifier
- `pattern` (string): Wildcard/regex pattern for multiple operations
- `useRegex` (boolean): Treat pattern as regex (default: false)
- `includeComponents` (boolean): Include component types (inspectMultiple, default: false)
- `includeProperties` (boolean): Include properties (inspect, default: true)
- `componentFilter` (array): Filter specific components (inspect only)
- `maxResults` (integer): Maximum results for multiple operations (default: 1000)

**Examples:**
```python
# Create GameObject
unity_gameobject_crud({
    "operation": "create",
    "name": "Player",
    "parentPath": "Game"
})

# Fast inspection (existence check)
unity_gameobject_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "includeProperties": False  # 10x faster!
})

# Inspect specific components only
unity_gameobject_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentFilter": ["UnityEngine.Transform", "UnityEngine.Rigidbody"]
})

# Find all enemies
unity_gameobject_crud({
    "operation": "findMultiple",
    "pattern": "Enemy*",
    "maxResults": 100
})

# Delete all temporary objects
unity_gameobject_crud({
    "operation": "deleteMultiple",
    "pattern": "Temp_*"
})
```

**Performance Tips:**
- Use `includeProperties=false` for fast existence checks
- Use `componentFilter` to inspect only needed components
- Set `maxResults` to prevent timeouts on large operations

**Use Cases:**
- Manage scene hierarchy
- Batch operations on similar objects
- Quick existence checks
- Find objects by pattern

---

### 9. `unity_gameobject_createFromTemplate`
**Purpose:** Create common GameObjects from templates with one command

**Templates:**
- **Primitives:** Cube, Sphere, Plane, Cylinder, Capsule, Quad
- **Lights:** Light-Directional, Light-Point, Light-Spot
- **Special:** Camera, Empty, Player, Enemy, Particle System, Audio Source

**Parameters:**
- `template` (string): Template name
- `name` (string): GameObject name (optional, uses template name)
- `parentPath` (string): Parent path (optional, creates at root)
- `position` (object): Position {x, y, z} (default: 0, 0, 0)
- `rotation` (object): Rotation {x, y, z} (default: 0, 0, 0)
- `scale` (object): Scale {x, y, z} (default: 1, 1, 1)

**Example:**
```python
# Create player
unity_gameobject_createFromTemplate({
    "template": "Player",
    "name": "MainPlayer",
    "position": {"x": 0, "y": 1, "z": 0}
})

# Create light
unity_gameobject_createFromTemplate({
    "template": "Light-Directional",
    "rotation": {"x": 50, "y": -30, "z": 0}
})
```

**Returns:** Created GameObject path and components

**Use Cases:**
- Rapid GameObject creation
- Standard game objects with sensible defaults
- Prototyping
- Level design

---

### 10. `unity_tagLayer_manage`
**Purpose:** Manage tags and layers in Unity

**Operations:**
- **GameObject:** setTag, getTag, setLayer, getLayer, setLayerRecursive
- **Project:** listTags, addTag, removeTag, listLayers, addLayer, removeLayer

**Parameters:**
- `operation` (string): Operation to perform
- `gameObjectPath` (string): GameObject path (for GameObject operations)
- `tag` (string): Tag name
- `layer` (string/integer): Layer name or index

**Examples:**
```python
# Set tag on GameObject
unity_tagLayer_manage({
    "operation": "setTag",
    "gameObjectPath": "Player",
    "tag": "Player"
})

# Set layer recursively
unity_tagLayer_manage({
    "operation": "setLayerRecursive",
    "gameObjectPath": "UI",
    "layer": "UI"
})

# List all tags
unity_tagLayer_manage({"operation": "listTags"})

# Add new layer
unity_tagLayer_manage({
    "operation": "addLayer",
    "layer": "Ground"
})
```

**Use Cases:**
- Organize GameObjects with tags
- Set up layer-based rendering
- Configure physics collision layers
- Manage project tags and layers

---

## Component Management

### 11. `unity_component_crud`
**Purpose:** Add, remove, update, and inspect components on GameObjects

**Operations:**
- **Single:** add, remove, update, inspect
- **Multiple:** addMultiple, removeMultiple, updateMultiple, inspectMultiple

**Parameters:**
- `operation` (string): Operation to perform
- `gameObjectPath` (string): GameObject path (not for multiple operations)
- `gameObjectGlobalObjectId` (string): GlobalObjectId for precise identification
- `componentType` (string): Fully qualified type (e.g., UnityEngine.UI.Text)
- `propertyChanges` (object): Property/value pairs to apply
- `pattern` (string): Wildcard/regex pattern for multiple operations
- `useRegex` (boolean): Treat pattern as regex (default: false)
- `includeProperties` (boolean): Include properties (default: true)
- `propertyFilter` (array): Filter specific properties (inspect only)
- `maxResults` (integer): Maximum results (default: 1000)
- `stopOnError` (boolean): Stop on first error (default: false)

**Examples:**
```python
# Add component
unity_component_crud({
    "operation": "add",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Rigidbody"
})

# Update component properties
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Transform",
    "propertyChanges": {
        "position": {"x": 0, "y": 1, "z": 0},
        "rotation": {"x": 0, "y": 45, "z": 0}
    }
})

# Fast existence check
unity_component_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.CharacterController",
    "includeProperties": False  # 10x faster!
})

# Get specific properties only
unity_component_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Transform",
    "propertyFilter": ["position", "rotation"]
})

# Batch add components
unity_component_crud({
    "operation": "addMultiple",
    "pattern": "Enemy*",
    "componentType": "UnityEngine.Rigidbody",
    "propertyChanges": {"mass": 2.0, "useGravity": True},
    "maxResults": 1000,
    "stopOnError": False
})
```

**Asset Reference Formats:**
```python
# GUID (recommended)
{"_ref": "asset", "guid": "abc123def456789"}

# Path
{"_ref": "asset", "path": "Assets/Models/Sphere.fbx"}

# Direct path string
"Assets/Models/Sphere.fbx"

# Built-in resources
"Library/unity default resources::Sphere"
```

**UnityEvent Listeners:**
```python
# Simple format (single listener)
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Canvas/Button",
    "componentType": "UnityEngine.UI.Button",
    "propertyChanges": {
        "onClick": "GameManager.OnButtonClick"
    }
})

# Complex format (multiple listeners with arguments)
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Canvas/Button",
    "componentType": "UnityEngine.UI.Button",
    "propertyChanges": {
        "onClick": {
            "clearListeners": True,
            "listeners": [
                {
                    "targetPath": "GameManager",
                    "methodName": "OnButtonClick",
                    "mode": "Void"
                },
                {
                    "targetPath": "AudioManager",
                    "methodName": "PlaySound",
                    "mode": "String",
                    "argument": "button_click"
                }
            ]
        }
    }
})
```

**Listener Modes:** Void, Int, Float, String, Bool, Object

**Performance Tips:**
- Use `includeProperties=false` for existence checks
- Use `propertyFilter` for specific properties only
- Set `maxResults` to prevent timeouts
- Use `stopOnError=false` for resilient batch operations

---

## Asset Management

### 12. `unity_asset_crud`
**Purpose:** Manage Unity asset importer settings and asset operations

**Operations:**
- **Single:** updateImporter, delete, rename, duplicate, inspect
- **Multiple:** findMultiple, deleteMultiple, inspectMultiple

**IMPORTANT:** This tool does NOT create or modify file contents! Use Claude Code's Write/Edit tools for that. For C# scripts, use `unity_script_batch_manage` instead.

**Parameters:**
- `operation` (string): Operation to perform
- `assetPath` (string): Path under Assets/
- `assetGuid` (string): GUID for precise identification
- `destinationPath` (string): Target path for rename/duplicate
- `propertyChanges` (object): Importer property changes
- `pattern` (string): Wildcard/regex pattern for multiple operations
- `useRegex` (boolean): Treat pattern as regex (default: false)
- `includeProperties` (boolean): Include importer properties (default: false)

**Examples:**
```python
# Update texture importer settings
unity_asset_crud({
    "operation": "updateImporter",
    "assetPath": "Assets/Textures/Icon.png",
    "propertyChanges": {
        "textureType": "Sprite",
        "isReadable": True,
        "filterMode": "Bilinear"
    }
})

# Rename asset
unity_asset_crud({
    "operation": "rename",
    "assetPath": "Assets/Materials/OldMaterial.mat",
    "destinationPath": "Assets/Materials/NewMaterial.mat"
})

# Find all PNG textures
unity_asset_crud({
    "operation": "findMultiple",
    "pattern": "Assets/Textures/*.png"
})

# Delete temporary assets
unity_asset_crud({
    "operation": "deleteMultiple",
    "pattern": "Assets/Temp/*"
})
```

**Use Cases:**
- Configure texture import settings
- Manage model import options
- Organize assets
- Batch asset operations
- NOT for C# scripts (use script_batch_manage)

---

### 13. `unity_script_batch_manage`
**Purpose:** CRITICAL tool for ALL C# script operations with automatic compilation

**ALWAYS USE THIS TOOL FOR SCRIPTS!** Do not use `unity_asset_crud` for .cs files!

**Operations:** create, update, delete, inspect

**Parameters:**
- `scripts` (array): Array of script operations
  - `operation` (string): create/update/delete/inspect
  - `scriptPath` (string): Path to .cs file
  - `content` (string): Script content (for create/update)
  - `namespace` (string): Optional namespace
- `stopOnError` (boolean): Stop on first error (default: false)
- `timeoutSeconds` (integer): Compilation timeout (default: 30)

**Examples:**
```python
# Single script (still use array!)
unity_script_batch_manage({
    "scripts": [
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/Player.cs",
            "content": "using UnityEngine;\n\npublic class Player : MonoBehaviour\n{\n    void Start()\n    {\n    }\n}"
        }
    ],
    "timeoutSeconds": 30
})

# Multiple scripts (10-20x faster than individual!)
unity_script_batch_manage({
    "scripts": [
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/Player.cs",
            "content": "..."
        },
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/Enemy.cs",
            "content": "..."
        },
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/Weapon.cs",
            "content": "..."
        }
    ],
    "stopOnError": False,
    "timeoutSeconds": 60
})
```

**Benefits:**
- 10-20x faster for multiple scripts (single compilation)
- Atomic operations (all succeed or fail together)
- Automatic compilation detection and waiting
- Proper error reporting with per-script results

**Returns:** Batch results + compilation results with success status and errors

---

## UI (UGUI) Tools

### 14. `unity_ugui_createFromTemplate`
**Purpose:** Create UI elements from templates with one command

**Templates:**
- Button, Text, Image, RawImage, Panel
- ScrollView, InputField, Slider, Toggle, Dropdown

**Parameters:**
- `template` (string): UI element template
- `name` (string): GameObject name (optional)
- `parentPath` (string): Parent path (must be under Canvas)
- `anchorPreset` (string): Anchor preset (default: center)
- `width` (number): Element width
- `height` (number): Element height
- `positionX` (number): Anchored position X (default: 0)
- `positionY` (number): Anchored position Y (default: 0)
- `text` (string): Text content (for text elements)
- `fontSize` (integer): Font size
- `interactable` (boolean): Interactable state (default: true)
- `useTextMeshPro` (boolean): Use TMP (default: false)

**Example:**
```python
# Create button
unity_ugui_createFromTemplate({
    "template": "Button",
    "text": "Start Game",
    "width": 200,
    "height": 50,
    "anchorPreset": "middle-center"
})

# Create input field
unity_ugui_createFromTemplate({
    "template": "InputField",
    "name": "UsernameInput",
    "parentPath": "Canvas/LoginPanel",
    "text": "Enter username...",
    "width": 300,
    "height": 40
})
```

**Returns:** Created GameObject path and components

**Use Cases:**
- Rapid UI prototyping
- Standard UI elements
- Menu creation
- Form building

---

### 15. `unity_ugui_layoutManage`
**Purpose:** Manage layout components on UI GameObjects

**Layout Types:**
- HorizontalLayoutGroup, VerticalLayoutGroup, GridLayoutGroup
- ContentSizeFitter, LayoutElement, AspectRatioFitter

**Operations:** add, update, remove, inspect

**Parameters:**
- `gameObjectPath` (string): Target GameObject
- `operation` (string): Operation type
- `layoutType` (string): Layout component type
- **Common:** padding, spacing, childAlignment, childControlWidth/Height, childForceExpandWidth/Height
- **Grid:** cellSizeX/Y, constraint, constraintCount, startCorner, startAxis
- **ContentSizeFitter:** horizontalFit, verticalFit
- **LayoutElement:** minWidth/Height, preferredWidth/Height, flexibleWidth/Height, ignoreLayout
- **AspectRatioFitter:** aspectMode, aspectRatio

**Examples:**
```python
# Add vertical layout
unity_ugui_layoutManage({
    "operation": "add",
    "gameObjectPath": "Canvas/Panel",
    "layoutType": "VerticalLayoutGroup",
    "spacing": 10,
    "padding": {"left": 20, "right": 20, "top": 20, "bottom": 20},
    "childControlWidth": True,
    "childControlHeight": False
})

# Add grid layout
unity_ugui_layoutManage({
    "operation": "add",
    "gameObjectPath": "Canvas/InventoryPanel",
    "layoutType": "GridLayoutGroup",
    "cellSizeX": 80,
    "cellSizeY": 80,
    "spacing": 10,
    "constraint": "FixedColumnCount",
    "constraintCount": 5
})
```

**Use Cases:**
- Organize UI layouts
- Create grids and lists
- Dynamic sizing
- Responsive UI

---

### 16. `unity_ugui_manage`
**Purpose:** Unified UGUI management tool for RectTransform operations

**Operations:**
- rectAdjust, setAnchor, setAnchorPreset
- convertToAnchored, convertToAbsolute
- inspect, updateRect

**Parameters:**
- `gameObjectPath` (string): Target GameObject
- `operation` (string): Operation type
- **setAnchor:** anchorMinX/Y, anchorMaxX/Y
- **setAnchorPreset:** preset, preservePosition
- **convertToAnchored:** absoluteX/Y
- **updateRect:** anchoredPositionX/Y, sizeDeltaX/Y, pivotX/Y, offsetMinX/Y, offsetMaxX/Y

**Anchor Presets:**
- **Corners:** top-left, top-center, top-right, middle-left, middle-center/center, middle-right, bottom-left, bottom-center, bottom-right
- **Stretch:** stretch-horizontal, stretch-vertical, stretch-all/stretch, stretch-top, stretch-middle, stretch-bottom, stretch-left, stretch-center-vertical, stretch-right

**Examples:**
```python
# Set anchor preset
unity_ugui_manage({
    "operation": "setAnchorPreset",
    "gameObjectPath": "Canvas/Title",
    "preset": "top-center",
    "preservePosition": True
})

# Update RectTransform
unity_ugui_manage({
    "operation": "updateRect",
    "gameObjectPath": "Canvas/Panel",
    "sizeDeltaX": 400,
    "sizeDeltaY": 300,
    "anchoredPositionX": 0,
    "anchoredPositionY": 0
})
```

**Use Cases:**
- Position UI elements
- Set up responsive layouts
- Adjust RectTransform properties
- Convert between coordinate systems

---

### 17. `unity_ugui_rectAdjust`
**Purpose:** Adjust RectTransform using uGUI layout utilities (Legacy)

**Note:** Consider using `unity_ugui_manage` instead for modern workflows

**Parameters:**
- `gameObjectPath` (string): Target GameObject
- `referenceResolution` (array): [width, height]
- `matchMode` (string): widthOrHeight/expand/shrink

---

### 18. `unity_ugui_anchorManage`
**Purpose:** Manage RectTransform anchors (Legacy)

**Note:** Consider using `unity_ugui_manage` instead for modern workflows

**Operations:** setAnchor, setAnchorPreset, convertToAnchored, convertToAbsolute

---

### 19. `unity_ugui_detectOverlaps`
**Purpose:** Detect overlapping UI elements in the scene

**Parameters:**
- `gameObjectPath` (string): Target GameObject (optional)
- `checkAll` (boolean): Check all UI elements (default: false)
- `includeChildren` (boolean): Include children (default: false)
- `threshold` (number): Minimum overlap area (default: 0)

**Example:**
```python
# Check specific element for overlaps
unity_ugui_detectOverlaps({
    "gameObjectPath": "Canvas/Button",
    "includeChildren": False
})

# Check all UI elements
unity_ugui_detectOverlaps({
    "checkAll": True,
    "threshold": 100
})
```

**Returns:** List of overlapping pairs with bounds and overlap areas

**Use Cases:**
- Debug UI layout issues
- Verify UI element positioning
- Find accidental overlaps
- UI quality assurance

---

## Prefab Management

### 20. `unity_prefab_crud`
**Purpose:** Manage Unity prefabs

**Operations:**
- create, update, inspect, instantiate
- unpack, applyOverrides, revertOverrides

**Parameters:**
- `operation` (string): Operation to perform
- `prefabPath` (string): Prefab asset path (e.g., Assets/Prefabs/MyPrefab.prefab)
- `gameObjectPath` (string): GameObject path
- `parentPath` (string): Parent path for instantiation
- `replacePrefabOptions` (string): Default/ConnectToPrefab/ReplaceNameBased
- `unpackMode` (string): Completely/OutermostRoot
- `includeChildren` (boolean): Include children when creating

**Examples:**
```python
# Create prefab from GameObject
unity_prefab_crud({
    "operation": "create",
    "gameObjectPath": "Player",
    "prefabPath": "Assets/Prefabs/Player.prefab"
})

# Instantiate prefab
unity_prefab_crud({
    "operation": "instantiate",
    "prefabPath": "Assets/Prefabs/Enemy.prefab",
    "parentPath": "Enemies"
})

# Apply instance overrides to prefab
unity_prefab_crud({
    "operation": "applyOverrides",
    "gameObjectPath": "Player"
})

# Revert instance to prefab state
unity_prefab_crud({
    "operation": "revertOverrides",
    "gameObjectPath": "Player"
})

# Unpack prefab instance
unity_prefab_crud({
    "operation": "unpack",
    "gameObjectPath": "EnemyInstance",
    "unpackMode": "Completely"
})
```

**Use Cases:**
- Create reusable GameObjects
- Manage prefab instances
- Apply/revert modifications
- Prefab workflow automation

---

## Advanced Features

### 21. `unity_projectSettings_crud`
**Purpose:** Read, write, or list Unity Project Settings

**Operations:** read, write, list

**Categories:**
- **player:** PlayerSettings (company, product, version, screen settings)
- **quality:** QualitySettings (quality levels, shadows, anti-aliasing)
- **time:** Time settings (fixedDeltaTime, timeScale)
- **physics:** Physics settings (gravity, collision)
- **audio:** AudioSettings (DSP buffer, sample rate)
- **editor:** EditorSettings (serialization, line endings)

**Parameters:**
- `operation` (string): read/write/list
- `category` (string): Settings category
- `property` (string): Property name (optional for read)
- `value`: Value to write (required for write)

**Examples:**
```python
# List available categories
unity_projectSettings_crud({"operation": "list"})

# Read company name
unity_projectSettings_crud({
    "operation": "read",
    "category": "player",
    "property": "companyName"
})

# Write product name
unity_projectSettings_crud({
    "operation": "write",
    "category": "player",
    "property": "productName",
    "value": "My Game"
})

# Read all player settings
unity_projectSettings_crud({
    "operation": "read",
    "category": "player"
})
```

---

### 22. `unity_renderPipeline_manage`
**Purpose:** Manage Unity Render Pipeline

**Operations:** inspect, setAsset, getSettings, updateSettings

**Parameters:**
- `operation` (string): Operation type
- `assetPath` (string): RenderPipelineAsset path (empty = Built-in)
- `settings` (object): Settings to update

**Examples:**
```python
# Inspect current pipeline
unity_renderPipeline_manage({"operation": "inspect"})

# Set URP asset
unity_renderPipeline_manage({
    "operation": "setAsset",
    "assetPath": "Assets/Settings/UniversalRP-HighQuality.asset"
})

# Update pipeline settings
unity_renderPipeline_manage({
    "operation": "updateSettings",
    "settings": {
        "shadowDistance": 150,
        "cascadeCount": 4
    }
})
```

---

### 23. `unity_inputSystem_manage`
**Purpose:** Manage Unity New Input System

**Operations:**
- listActions, createAsset, addActionMap, addAction, addBinding
- inspectAsset, deleteAsset, deleteActionMap, deleteAction, deleteBinding

**Parameters:**
- `operation` (string): Operation type
- `assetPath` (string): .inputactions file path
- `mapName` (string): Action map name
- `actionName` (string): Action name
- `actionType` (string): Button/Value/PassThrough
- `path` (string): Binding path (e.g., `<Keyboard>/space`)
- `bindingIndex` (integer): Binding index (-1 = all)

**Examples:**
```python
# Create Input Action asset
unity_inputSystem_manage({
    "operation": "createAsset",
    "assetPath": "Assets/Input/PlayerControls.inputactions"
})

# Add action map
unity_inputSystem_manage({
    "operation": "addActionMap",
    "assetPath": "Assets/Input/PlayerControls.inputactions",
    "mapName": "Player"
})

# Add action
unity_inputSystem_manage({
    "operation": "addAction",
    "assetPath": "Assets/Input/PlayerControls.inputactions",
    "mapName": "Player",
    "actionName": "Jump",
    "actionType": "Button"
})

# Add binding
unity_inputSystem_manage({
    "operation": "addBinding",
    "assetPath": "Assets/Input/PlayerControls.inputactions",
    "mapName": "Player",
    "actionName": "Jump",
    "path": "<Keyboard>/space"
})
```

**Common Binding Paths:**
- Keyboard: `<Keyboard>/space`, `<Keyboard>/w`
- Mouse: `<Mouse>/leftButton`, `<Mouse>/position`
- Gamepad: `<Gamepad>/buttonSouth`, `<Gamepad>/leftStick`

---

### 24. `unity_tilemap_manage`
**Purpose:** Manage Unity Tilemap system

**Operations:**
- createTilemap, setTile, getTile, clearTile
- fillArea, inspectTilemap, clearAll

**Parameters:**
- `operation` (string): Operation type
- `gameObjectPath` (string): Tilemap GameObject path
- `tilemapName` (string): Name for new Tilemap
- `parentPath` (string): Parent for new Tilemap
- `tileAssetPath` (string): Tile asset path
- `positionX/Y/Z` (integer): Grid coordinates
- `startX/Y, endX/Y` (integer): Area bounds

**Examples:**
```python
# Create Tilemap
unity_tilemap_manage({
    "operation": "createTilemap",
    "tilemapName": "Ground",
    "parentPath": "Level"
})

# Set tile
unity_tilemap_manage({
    "operation": "setTile",
    "gameObjectPath": "Grid/Ground",
    "tileAssetPath": "Assets/Tiles/GrassTile.asset",
    "positionX": 0,
    "positionY": 0
})

# Fill area
unity_tilemap_manage({
    "operation": "fillArea",
    "gameObjectPath": "Grid/Ground",
    "tileAssetPath": "Assets/Tiles/GrassTile.asset",
    "startX": 0,
    "startY": 0,
    "endX": 10,
    "endY": 10
})
```

---

### 25. `unity_navmesh_manage`
**Purpose:** Manage Unity NavMesh navigation system

**Operations:**
- bakeNavMesh, clearNavMesh, addNavMeshAgent
- setDestination, inspectNavMesh, updateSettings, createNavMeshSurface

**Parameters:**
- `operation` (string): Operation type
- `gameObjectPath` (string): GameObject path
- `destinationX/Y/Z` (number): Destination coordinates
- `settings` (object): NavMesh bake settings
- `agentSpeed/Acceleration/StoppingDistance` (number): Agent properties

**Examples:**
```python
# Bake NavMesh
unity_navmesh_manage({"operation": "bakeNavMesh"})

# Add NavMeshAgent
unity_navmesh_manage({
    "operation": "addNavMeshAgent",
    "gameObjectPath": "Enemy",
    "agentSpeed": 3.5,
    "agentAcceleration": 8,
    "agentStoppingDistance": 0.5
})

# Set destination
unity_navmesh_manage({
    "operation": "setDestination",
    "gameObjectPath": "Enemy",
    "destinationX": 10,
    "destinationY": 0,
    "destinationZ": 5
})

# Update bake settings
unity_navmesh_manage({
    "operation": "updateSettings",
    "settings": {
        "agentRadius": 0.5,
        "agentHeight": 2.0,
        "agentSlope": 45
    }
})
```

---

### 26. `unity_constant_convert`
**Purpose:** Convert between Unity constants and numeric values

**Operations:**
- enumToValue, valueToEnum, listEnums
- colorToRGBA, rgbaToColor, listColors
- layerToIndex, indexToLayer, listLayers

**Parameters:**
- `operation` (string): Operation type
- `enumType` (string): Fully qualified enum type
- `enumValue` (string): Enum value name
- `numericValue` (integer): Numeric value
- `colorName` (string): Color name
- `r/g/b/a` (number): RGBA components
- `layerName` (string): Layer name
- `layerIndex` (integer): Layer index

**Examples:**
```python
# Enum to value
unity_constant_convert({
    "operation": "enumToValue",
    "enumType": "UnityEngine.KeyCode",
    "enumValue": "Space"
})

# Color to RGBA
unity_constant_convert({
    "operation": "colorToRGBA",
    "colorName": "red"
})

# Layer to index
unity_constant_convert({
    "operation": "layerToIndex",
    "layerName": "Default"
})
```

---

### 27. `unity_await_compilation`
**Purpose:** Wait for Unity compilation to complete

**Note:** Does NOT start compilation, only waits for ongoing compilation

**Parameters:**
- `timeoutSeconds` (integer): Maximum wait time (default: 60)

**Example:**
```python
unity_await_compilation({"timeoutSeconds": 60})
```

**Returns:** Compilation result with success status, error count, and messages

**Use Cases:**
- Wait for automatic compilation after file changes
- Ensure compilation finishes before testing
- Monitor compilation status

---

## Utility Tools

### 28. `unity_constant_convert`
_(Already documented in Advanced Features section)_

---

## Tool Categories Summary

| Category | Tools | Primary Use |
|----------|-------|-------------|
| **Core** | 3 | Connection, context, batch operations |
| **Scene** | 2 | Scene management, quick setup |
| **GameObject** | 3 | Create, modify, manage GameObjects |
| **Component** | 1 | Component CRUD operations |
| **Asset** | 2 | Asset management, script batch operations |
| **UI (UGUI)** | 6 | UI creation, layouts, RectTransform |
| **Prefab** | 1 | Prefab workflow |
| **Advanced** | 7 | Settings, pipeline, input, tilemap, navmesh, constants |
| **Utility** | 1 | Compilation waiting |

---

## Best Practices

### Performance Optimization

1. **Use `includeProperties=false` for fast checks:**
   ```python
   unity_component_crud({
       "operation": "inspect",
       "gameObjectPath": "Player",
       "componentType": "UnityEngine.Rigidbody",
       "includeProperties": False  # 10x faster!
   })
   ```

2. **Use `propertyFilter` for specific data:**
   ```python
   unity_component_crud({
       "operation": "inspect",
       "gameObjectPath": "Player",
       "componentType": "UnityEngine.Transform",
       "propertyFilter": ["position", "rotation"]
   })
   ```

3. **Set `maxResults` for batch operations:**
   ```python
   unity_component_crud({
       "operation": "addMultiple",
       "pattern": "Enemy*",
       "componentType": "UnityEngine.Rigidbody",
       "maxResults": 1000  # Prevent timeouts
   })
   ```

### Tool Selection

1. **Always use templates when available:**
   - Use `unity_ugui_createFromTemplate` instead of manual UI creation
   - Use `unity_gameobject_createFromTemplate` for common objects
   - Use `unity_scene_quickSetup` for standard scenes

2. **Always use script batch manager for C# scripts:**
   - NEVER use `unity_asset_crud` for .cs files
   - Always use `unity_script_batch_manage` with scripts array
   - Even for single scripts, use the batch format

3. **Use template manager for customization:**
   - Add components and children in one command
   - Better than multiple individual operations

4. **Use batch execute for related operations:**
   - Combine multiple tool calls
   - Automatic compilation handling

### Error Handling

1. **Use `stopOnError=false` for resilient batch operations:**
   ```python
   unity_component_crud({
       "operation": "addMultiple",
       "pattern": "Enemy*",
       "componentType": "UnityEngine.Rigidbody",
       "stopOnError": False  # Continue on errors
   })
   ```

2. **Check results for errors:**
   ```python
   result = unity_component_crud(...)
   if result["errorCount"] > 0:
       for error in result["errors"]:
           print(f"Failed: {error['gameObject']} - {error['error']}")
   ```

---

## Quick Reference

### Most Used Tools

1. `unity_context_inspect` - Understand scene state
2. `unity_scene_quickSetup` - Quick scene initialization
3. `unity_gameobject_createFromTemplate` - Create common objects
4. `unity_ugui_createFromTemplate` - Create UI elements
5. `unity_component_crud` - Manage components
6. `unity_menu_hierarchyCreate` - Build menu systems
7. `unity_template_manage` - Customize objects
8. `unity_script_batch_manage` - Manage C# scripts
9. `unity_batch_execute` - Execute multiple operations

### Tool Aliases (Internal Names)

| MCP Tool Name | Internal Tool Name (for batch_execute) |
|---------------|----------------------------------------|
| unity_scene_crud | sceneManage |
| unity_gameobject_crud | gameObjectManage |
| unity_component_crud | componentManage |
| unity_asset_crud | assetManage |
| unity_prefab_crud | prefabManage |
| unity_script_batch_manage | scriptBatchManage |

---

## Version History

- **v1.0** - Initial 28 tools
  - Core tools (5)
  - Scene management (2)
  - GameObject operations (3)
  - Component management (1)
  - Asset management (2)
  - UI/UGUI tools (6)
  - Prefab management (1)
  - Advanced features (7)
  - Utility tools (1)

---

**Last Updated:** 2025-01-14
**Total Tools:** 26
**Skill Version:** 1.0.0
