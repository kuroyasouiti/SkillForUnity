# Tool Selection Guide

**How to choose the right SkillForUnity tool for your task**

This guide helps you quickly identify which of the 28 available tools to use for common Unity development tasks.

---

## Quick Decision Tree

```
Need to work with Unity?
├─ Need to test connection? → unity_ping
├─ Need to understand current scene? → unity_context_inspect
│
├─ Working with Scenes?
│  ├─ Quick setup (3D/2D/UI/VR)? → unity_scene_quickSetup
│  └─ Manual scene management? → unity_scene_crud
│
├─ Creating GameObjects?
│  ├─ Common objects (Cube, Player, etc.)? → unity_gameobject_createFromTemplate
│  ├─ Complex hierarchy? → unity_hierarchy_builder
│  └─ Custom creation/management? → unity_gameobject_crud
│
├─ Working with Components?
│  └─ All operations → unity_component_crud
│
├─ Working with UI?
│  ├─ Creating UI elements? → unity_ugui_createFromTemplate
│  ├─ Managing layouts? → unity_ugui_layoutManage
│  ├─ RectTransform operations? → unity_ugui_manage
│  └─ Detecting overlaps? → unity_ugui_detectOverlaps
│
├─ Working with Assets?
│  ├─ C# scripts? → unity_script_batch_manage (ALWAYS!)
│  └─ Other assets? → unity_asset_crud
│
├─ Working with Prefabs?
│  └─ All operations → unity_prefab_crud
│
├─ Advanced Features?
│  ├─ Project settings? → unity_projectSettings_crud
│  ├─ Render pipeline? → unity_renderPipeline_manage
│  ├─ Input system? → unity_inputSystem_manage
│  ├─ Tilemap? → unity_tilemap_manage
│  ├─ NavMesh? → unity_navmesh_manage
│  ├─ Constants/enums? → unity_constant_convert
│  └─ Tags/Layers? → unity_tagLayer_manage
│
├─ Batch Operations?
│  ├─ Multiple different tools? → unity_batch_execute
│  └─ Same tool, multiple objects? → Use tool's *Multiple operations
│
└─ Debugging?
   ├─ Console logs? → unity_console_log
   └─ Wait for compilation? → unity_await_compilation
```

---

## Task-Based Tool Selection

### I want to...

#### ...create a new scene
**Best:** `unity_scene_quickSetup` - One command for standard setups
```python
unity_scene_quickSetup({"setupType": "3D"})  # or "2D", "UI", "VR"
```

**Alternative:** `unity_scene_crud` - For manual control
```python
unity_scene_crud({"operation": "create", "scenePath": "Assets/Scenes/Level1.unity"})
```

---

#### ...create UI elements
**Best:** `unity_ugui_createFromTemplate` - Complete UI elements
```python
unity_ugui_createFromTemplate({"template": "Button", "text": "Start"})
```

**Not Recommended:** Manual creation with `unity_gameobject_crud` + `unity_component_crud`
- ❌ Slower (multiple commands)
- ❌ Error-prone (missing components)
- ❌ More complex (need to know all required components)

---

#### ...create GameObjects
**Best (Common Objects):** `unity_gameobject_createFromTemplate`
```python
unity_gameobject_createFromTemplate({"template": "Sphere", "position": {"x": 0, "y": 5, "z": 0}})
```

**Best (Complex Hierarchy):** `unity_hierarchy_builder`
```python
unity_hierarchy_builder({
    "hierarchy": {
        "Player": {
            "components": ["Rigidbody"],
            "children": {
                "Camera": {"components": ["Camera"]},
                "Weapon": {}
            }
        }
    }
})
```

**Alternative (Custom):** `unity_gameobject_crud`
```python
unity_gameobject_crud({"operation": "create", "name": "CustomObject"})
```

---

#### ...manage C# scripts
**ALWAYS USE:** `unity_script_batch_manage` - Even for single scripts!
```python
unity_script_batch_manage({
    "scripts": [
        {"operation": "create", "scriptPath": "Assets/Scripts/Player.cs", "content": "..."}
    ]
})
```

**NEVER USE:** `unity_asset_crud` for .cs files
- ❌ No automatic compilation
- ❌ Will cause compilation issues
- ❌ No error handling

---

#### ...work with components
**Use:** `unity_component_crud` for everything
```python
# Add
unity_component_crud({
    "operation": "add",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Rigidbody"
})

# Update
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Transform",
    "propertyChanges": {"position": {"x": 0, "y": 1, "z": 0}}
})

# Inspect (fast)
unity_component_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Rigidbody",
    "includeProperties": False  # 10x faster!
})
```

---

#### ...organize UI layout
**Best:** `unity_ugui_layoutManage`
```python
unity_ugui_layoutManage({
    "operation": "add",
    "gameObjectPath": "Canvas/Panel",
    "layoutType": "VerticalLayoutGroup",
    "spacing": 10,
    "padding": {"left": 20, "right": 20, "top": 20, "bottom": 20}
})
```

---

#### ...position UI elements
**Best:** `unity_ugui_manage` - Modern unified tool
```python
unity_ugui_manage({
    "operation": "setAnchorPreset",
    "gameObjectPath": "Canvas/Button",
    "preset": "middle-center"
})
```

**Legacy:** `unity_ugui_anchorManage` - Still works but prefer modern tool

---

#### ...work with prefabs
**Use:** `unity_prefab_crud` for all prefab operations
```python
# Create
unity_prefab_crud({
    "operation": "create",
    "gameObjectPath": "Player",
    "prefabPath": "Assets/Prefabs/Player.prefab"
})

# Instantiate
unity_prefab_crud({
    "operation": "instantiate",
    "prefabPath": "Assets/Prefabs/Enemy.prefab"
})

# Apply overrides
unity_prefab_crud({
    "operation": "applyOverrides",
    "gameObjectPath": "PlayerInstance"
})
```

---

#### ...execute multiple operations
**Best:** `unity_batch_execute` - Different tools
```python
unity_batch_execute({
    "operations": [
        {"tool": "gameObjectCreateFromTemplate", "payload": {...}},
        {"tool": "componentManage", "payload": {...}},
        {"tool": "assetManage", "payload": {...}}
    ]
})
```

**Alternative:** Use tool's `*Multiple` operations - Same tool, multiple objects
```python
unity_component_crud({
    "operation": "addMultiple",
    "pattern": "Enemy*",
    "componentType": "UnityEngine.Rigidbody"
})
```

---

#### ...understand the scene
**Best:** `unity_context_inspect` - Comprehensive overview
```python
# Fast overview
unity_context_inspect({
    "includeHierarchy": True,
    "includeComponents": False,  # Fast mode
    "maxDepth": 2
})

# Detailed inspection
unity_context_inspect({
    "includeHierarchy": True,
    "includeComponents": True,
    "filter": "Player*"
})
```

**Alternative:** `unity_gameobject_crud` - Specific GameObject
```python
unity_gameobject_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "includeProperties": False  # Fast mode
})
```

---

## Performance Optimization Guide

### Fast Operations (< 0.5s)

1. **Existence checks** - Use `includeProperties=false`
   ```python
   unity_component_crud({
       "operation": "inspect",
       "gameObjectPath": "Player",
       "componentType": "UnityEngine.Rigidbody",
       "includeProperties": False  # ⚡ 10x faster!
   })
   ```

2. **Specific properties** - Use `propertyFilter`
   ```python
   unity_component_crud({
       "operation": "inspect",
       "gameObjectPath": "Player",
       "componentType": "UnityEngine.Transform",
       "propertyFilter": ["position"]  # ⚡ Only what you need
   })
   ```

3. **Limited batch operations** - Use `maxResults`
   ```python
   unity_gameobject_crud({
       "operation": "findMultiple",
       "pattern": "Enemy*",
       "maxResults": 100  # ⚡ Prevent timeout
   })
   ```

### Slow Operations (> 5s)

1. **Full property inspection** - Reads all properties
   ```python
   unity_gameobject_crud({
       "operation": "inspect",
       "gameObjectPath": "Player"
       # 🐌 Reads ALL components with ALL properties
   })
   ```

2. **Unlimited batch operations** - No limits
   ```python
   unity_component_crud({
       "operation": "addMultiple",
       "pattern": "*"  # 🐌 Matches everything!
   })
   ```

3. **Script compilation** - Unavoidable but manageable
   ```python
   unity_script_batch_manage({
       "scripts": [...],
       "timeoutSeconds": 60  # ⏱️ Increase for large projects
   })
   ```

---

## Common Workflows

### Workflow 1: Create a Main Menu

```python
# Step 1: Setup UI scene
unity_scene_quickSetup({"setupType": "UI"})

# Step 2: Create menu structure
unity_hierarchy_builder({
    "hierarchy": {
        "MenuPanel": {
            "components": ["UnityEngine.UI.Image"],
            "children": {
                "Title": {"components": ["UnityEngine.UI.Text"]},
                "ButtonList": {"components": ["UnityEngine.UI.VerticalLayoutGroup"]}
            }
        }
    },
    "parentPath": "Canvas"
})

# Step 3: Add layout to ButtonList
unity_ugui_layoutManage({
    "operation": "add",
    "gameObjectPath": "Canvas/MenuPanel/ButtonList",
    "layoutType": "VerticalLayoutGroup",
    "spacing": 15
})

# Step 4: Create buttons (using batch)
unity_batch_execute({
    "operations": [
        {"tool": "uguiCreateFromTemplate", "payload": {"template": "Button", "name": "PlayButton", "parentPath": "Canvas/MenuPanel/ButtonList", "text": "Play"}},
        {"tool": "uguiCreateFromTemplate", "payload": {"template": "Button", "name": "SettingsButton", "parentPath": "Canvas/MenuPanel/ButtonList", "text": "Settings"}},
        {"tool": "uguiCreateFromTemplate", "payload": {"template": "Button", "name": "QuitButton", "parentPath": "Canvas/MenuPanel/ButtonList", "text": "Quit"}}
    ]
})
```

**Tools Used:** 4 tools, 5 operations
**Estimated Time:** ~2-3 seconds

---

### Workflow 2: Create a Game Level

```python
# Step 1: Setup 3D scene
unity_scene_quickSetup({"setupType": "3D"})

# Step 2: Create player with hierarchy
unity_hierarchy_builder({
    "hierarchy": {
        "Player": {
            "components": ["Rigidbody", "CapsuleCollider"],
            "properties": {"position": {"x": 0, "y": 1, "z": 0}},
            "children": {
                "Camera": {
                    "components": ["Camera"],
                    "properties": {"position": {"x": 0, "y": 0.5, "z": -3}}
                }
            }
        }
    }
})

# Step 3: Create ground
unity_gameobject_createFromTemplate({
    "template": "Plane",
    "name": "Ground",
    "scale": {"x": 10, "y": 1, "z": 10}
})

# Step 4: Create multiple obstacles (using batch)
unity_batch_execute({
    "operations": [
        {"tool": "gameObjectCreateFromTemplate", "payload": {"template": "Cube", "name": "Obstacle1", "position": {"x": 3, "y": 0.5, "z": 0}}},
        {"tool": "gameObjectCreateFromTemplate", "payload": {"template": "Cube", "name": "Obstacle2", "position": {"x": -3, "y": 0.5, "z": 0}}},
        {"tool": "gameObjectCreateFromTemplate", "payload": {"template": "Cube", "name": "Obstacle3", "position": {"x": 0, "y": 0.5, "z": 3}}}
    ]
})

# Step 5: Add physics to obstacles
unity_component_crud({
    "operation": "addMultiple",
    "pattern": "Obstacle*",
    "componentType": "UnityEngine.BoxCollider"
})
```

**Tools Used:** 5 tools, 6 operations
**Estimated Time:** ~3-4 seconds

---

### Workflow 3: Create and Configure Scripts

```python
# Step 1: Create multiple scripts in one batch
unity_script_batch_manage({
    "scripts": [
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/Player.cs",
            "content": "using UnityEngine;\n\npublic class Player : MonoBehaviour\n{\n    public float speed = 5f;\n    \n    void Update()\n    {\n        float h = Input.GetAxis(\"Horizontal\");\n        float v = Input.GetAxis(\"Vertical\");\n        transform.Translate(new Vector3(h, 0, v) * speed * Time.deltaTime);\n    }\n}"
        },
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/CameraFollow.cs",
            "content": "using UnityEngine;\n\npublic class CameraFollow : MonoBehaviour\n{\n    public Transform target;\n    public Vector3 offset = new Vector3(0, 5, -10);\n    \n    void LateUpdate()\n    {\n        if (target != null)\n            transform.position = target.position + offset;\n    }\n}"
        }
    ],
    "timeoutSeconds": 30
})

# Step 2: Wait for compilation (if not automatic)
unity_await_compilation({"timeoutSeconds": 30})

# Step 3: Add scripts to GameObjects
unity_batch_execute({
    "operations": [
        {"tool": "componentManage", "payload": {"operation": "add", "gameObjectPath": "Player", "componentType": "Player"}},
        {"tool": "componentManage", "payload": {"operation": "add", "gameObjectPath": "Main Camera", "componentType": "CameraFollow"}}
    ]
})

# Step 4: Configure camera to follow player
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Main Camera",
    "componentType": "CameraFollow",
    "propertyChanges": {
        "target": {"_ref": "GameObject", "path": "Player"}
    }
})
```

**Tools Used:** 3 tools, 4 operations
**Estimated Time:** ~5-30 seconds (depends on compilation)

---

## Tool Comparison

### GameObject Creation

| Method | Speed | Flexibility | Best For |
|--------|-------|-------------|----------|
| `unity_gameobject_createFromTemplate` | ⚡⚡⚡ Fast | 🔧 Low | Common objects (Cube, Player, Light) |
| `unity_hierarchy_builder` | ⚡⚡ Medium | 🔧🔧🔧 High | Complex hierarchies, multi-level structures |
| `unity_gameobject_crud` | ⚡⚡⚡ Fast | 🔧🔧 Medium | Custom objects, manual control |

### UI Creation

| Method | Speed | Flexibility | Best For |
|--------|-------|-------------|----------|
| `unity_ugui_createFromTemplate` | ⚡⚡⚡ Fast | 🔧 Low | Standard UI elements (Button, Text) |
| `unity_hierarchy_builder` | ⚡⚡ Medium | 🔧🔧🔧 High | Complete UI layouts, nested panels |
| Manual (gameobject + component) | ⚡ Slow | 🔧🔧🔧 High | Custom UI, special cases |

### Batch Operations

| Method | Speed | Best For |
|--------|-------|----------|
| Tool's `*Multiple` operations | ⚡⚡⚡ Fast | Same tool, many objects (e.g., add Rigidbody to all enemies) |
| `unity_batch_execute` | ⚡⚡ Medium | Different tools, related operations (create + configure) |
| Individual calls | ⚡ Slow | Single operations, testing |

---

## Common Mistakes to Avoid

### ❌ Mistake 1: Using asset_crud for C# scripts
**Wrong:**
```python
unity_asset_crud({"operation": "create", "assetPath": "Assets/Scripts/Player.cs", "content": "..."})
```

**Right:**
```python
unity_script_batch_manage({"scripts": [{"operation": "create", "scriptPath": "Assets/Scripts/Player.cs", "content": "..."}]})
```

**Why:** Script batch manager handles compilation automatically!

---

### ❌ Mistake 2: Creating UI manually
**Wrong:**
```python
# Multiple commands, error-prone
unity_gameobject_crud({"operation": "create", "name": "Button"})
unity_component_crud({"operation": "add", "gameObjectPath": "Button", "componentType": "UnityEngine.UI.Image"})
unity_component_crud({"operation": "add", "gameObjectPath": "Button", "componentType": "UnityEngine.UI.Button"})
# ... many more steps
```

**Right:**
```python
# One command, complete element
unity_ugui_createFromTemplate({"template": "Button", "text": "Click Me!"})
```

**Why:** Templates include all necessary components and default settings!

---

### ❌ Mistake 3: Not using fast inspection
**Wrong:**
```python
# Reads all properties (slow!)
unity_component_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Transform"
})
```

**Right:**
```python
# Just check existence (10x faster!)
unity_component_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Transform",
    "includeProperties": False
})

# Or get specific properties only
unity_component_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Transform",
    "propertyFilter": ["position"]
})
```

**Why:** Only read what you need!

---

### ❌ Mistake 4: Not using hierarchy builder
**Wrong:**
```python
# Many individual commands
unity_gameobject_crud({"operation": "create", "name": "Player"})
unity_component_crud({"operation": "add", "gameObjectPath": "Player", "componentType": "Rigidbody"})
unity_gameobject_crud({"operation": "create", "name": "Camera", "parentPath": "Player"})
unity_component_crud({"operation": "add", "gameObjectPath": "Player/Camera", "componentType": "Camera"})
# ... many more
```

**Right:**
```python
# One command, entire hierarchy
unity_hierarchy_builder({
    "hierarchy": {
        "Player": {
            "components": ["Rigidbody"],
            "children": {
                "Camera": {"components": ["Camera"]}
            }
        }
    }
})
```

**Why:** Faster and more maintainable!

---

### ❌ Mistake 5: Unlimited batch operations
**Wrong:**
```python
# Could match 10,000+ objects and timeout!
unity_component_crud({
    "operation": "addMultiple",
    "pattern": "*",
    "componentType": "UnityEngine.Rigidbody"
})
```

**Right:**
```python
# Limited to 1000 objects
unity_component_crud({
    "operation": "addMultiple",
    "pattern": "Enemy*",  # More specific pattern
    "componentType": "UnityEngine.Rigidbody",
    "maxResults": 1000
})
```

**Why:** Prevents timeouts and allows error handling!

---

## Tool Selection Checklist

Before choosing a tool, ask yourself:

- [ ] **Is there a template?** → Use template tools (faster, more reliable)
- [ ] **Is it a C# script?** → Use `unity_script_batch_manage` (ALWAYS!)
- [ ] **Do I need multiple operations?** → Use batch tools or `*Multiple` operations
- [ ] **Do I need full properties?** → Use `includeProperties=false` or `propertyFilter`
- [ ] **Is it a common workflow?** → Check workflow examples above
- [ ] **Could this timeout?** → Set `maxResults` limit

---

## Summary

### Top 5 Most Used Tools

1. **`unity_context_inspect`** - Start here to understand the scene
2. **`unity_scene_quickSetup`** - Quick scene initialization
3. **`unity_gameobject_createFromTemplate`** - Create common objects
4. **`unity_ugui_createFromTemplate`** - Create UI elements
5. **`unity_component_crud`** - Manage all components

### Always Use These Tools

- **Scripts:** `unity_script_batch_manage` (NEVER asset_crud!)
- **Templates:** When available (faster and more reliable)
- **Batch operations:** For multiple similar operations
- **Fast inspection:** `includeProperties=false` when checking existence

### Never Do This

- ❌ Use `unity_asset_crud` for .cs files
- ❌ Create UI manually when templates exist
- ❌ Skip `includeProperties=false` for existence checks
- ❌ Use unlimited batch operations without `maxResults`
- ❌ Forget to check context before major changes

---

**Last Updated:** 2025-01-14
**Total Tools:** 28
