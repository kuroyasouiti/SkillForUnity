# Best Practices for Unity MCP Skill

This guide covers best practices for using Unity MCP Skill effectively and efficiently.

## General Principles

### 1. Always Check Context First

Before making changes, inspect the current state:

```python
# ✅ DO: Check before modifying
unity_context_inspect({
    "includeHierarchy": True,
    "maxDepth": 2
})

# Then make changes based on what exists
```

### 2. Use Templates When Available

```python
# ❌ DON'T: Manual creation
unity_gameobject_crud({"operation": "create", "name": "Button"})
unity_component_crud({"operation": "add", "gameObjectPath": "Button", "componentType": "UnityEngine.UI.Image"})
unity_component_crud({"operation": "add", "gameObjectPath": "Button", "componentType": "UnityEngine.UI.Button"})
# ... many more steps

# ✅ DO: Use templates
unity_ugui_createFromTemplate({"template": "Button", "text": "Click Me!"})
```

### 3. Batch Related Operations

```python
# ❌ DON'T: Individual operations
unity_gameobject_createFromTemplate({"template": "Cube", "name": "Wall1", ...})
unity_gameobject_createFromTemplate({"template": "Cube", "name": "Wall2", ...})
unity_gameobject_createFromTemplate({"template": "Cube", "name": "Wall3", ...})

# ✅ DO: Batch execute
unity_batch_execute({
    "operations": [
        {"tool": "gameObjectCreateFromTemplate", "payload": {"template": "Cube", "name": "Wall1", ...}},
        {"tool": "gameObjectCreateFromTemplate", "payload": {"template": "Cube", "name": "Wall2", ...}},
        {"tool": "gameObjectCreateFromTemplate", "payload": {"template": "Cube", "name": "Wall3", ...}}
    ]
})
```

## Script Management

### CRITICAL: Always Use Script Batch Manager

```python
# ❌ NEVER DO THIS: Using asset CRUD for scripts
unity_asset_crud({
    "operation": "create",
    "assetPath": "Assets/Scripts/Player.cs",
    "content": "..."
})

# ✅ ALWAYS DO THIS: Use script batch manager
unity_script_batch_manage({
    "scripts": [
        {"operation": "create", "scriptPath": "Assets/Scripts/Player.cs", "content": "..."}
    ],
    "timeoutSeconds": 30
})
```

### Batch Multiple Scripts

```python
# ✅ BEST: Create all scripts at once (10-20x faster!)
unity_script_batch_manage({
    "scripts": [
        {"operation": "create", "scriptPath": "Assets/Scripts/Player.cs", "content": "..."},
        {"operation": "create", "scriptPath": "Assets/Scripts/Enemy.cs", "content": "..."},
        {"operation": "create", "scriptPath": "Assets/Scripts/Weapon.cs", "content": "..."}
    ],
    "stopOnError": False,
    "timeoutSeconds": 30
})
```

## Performance Optimization

### Use includeProperties=false for Existence Checks

```python
# ❌ SLOW: Full inspection (3 seconds)
unity_gameobject_crud({
    "operation": "inspect",
    "gameObjectPath": "Player"
})

# ✅ FAST: Check existence only (0.3 seconds)
unity_gameobject_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "includeProperties": False
})
```

### Use propertyFilter for Specific Properties

```python
# ❌ SLOW: Get all properties (2 seconds)
unity_component_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Transform"
})

# ✅ FAST: Get only position (0.5 seconds)
unity_component_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Transform",
    "propertyFilter": ["position"]
})
```

### Limit Batch Operations with maxResults

```python
# ❌ DANGEROUS: Could process 10000+ objects (timeout!)
unity_component_crud({
    "operation": "addMultiple",
    "pattern": "*",
    "componentType": "UnityEngine.Rigidbody"
})

# ✅ SAFE: Limit to 1000 objects
unity_component_crud({
    "operation": "addMultiple",
    "pattern": "Enemy*",
    "componentType": "UnityEngine.Rigidbody",
    "maxResults": 1000
})
```

### Test with Small Limits First

```python
# ✅ BEST PRACTICE: Test with small limit
test_result = unity_component_crud({
    "operation": "updateMultiple",
    "pattern": "Enemy*",
    "componentType": "UnityEngine.Rigidbody",
    "propertyChanges": {"mass": 5.0},
    "maxResults": 10  # Test with 10 first
})

# Check for errors
if test_result["errorCount"] == 0:
    # Scale up
    final_result = unity_component_crud({
        "operation": "updateMultiple",
        "pattern": "Enemy*",
        "componentType": "UnityEngine.Rigidbody",
        "propertyChanges": {"mass": 5.0},
        "maxResults": 5000
    })
```

## Error Handling

### Use stopOnError=false for Resilient Operations

```python
# ✅ RECOMMENDED: Continue on errors
result = unity_component_crud({
    "operation": "addMultiple",
    "pattern": "Enemy*",
    "componentType": "UnityEngine.BoxCollider",
    "stopOnError": False,
    "maxResults": 1000
})

# Review results
print(f"Success: {result['successCount']}/{result['totalCount']}")
print(f"Errors: {result['errorCount']}")

# Check specific errors
if result["errorCount"] > 0:
    for error in result["errors"]:
        print(f"Failed on {error['gameObject']}: {error['error']}")
```

## Hierarchy Organization

### Use Hierarchy Builder for Complex Structures

```python
# ❌ DON'T: Create manually
unity_gameobject_crud({"operation": "create", "name": "Player"})
unity_gameobject_crud({"operation": "create", "name": "Camera", "parentPath": "Player"})
unity_gameobject_crud({"operation": "create", "name": "Weapon", "parentPath": "Player"})
unity_component_crud({"operation": "add", "gameObjectPath": "Player", "componentType": "Rigidbody"})
# ... many more steps

# ✅ DO: Use hierarchy builder
unity_hierarchy_builder({
    "hierarchy": {
        "Player": {
            "components": ["Rigidbody", "CapsuleCollider"],
            "properties": {"position": {"x": 0, "y": 1, "z": 0}},
            "children": {
                "Camera": {
                    "components": ["Camera"],
                    "properties": {"position": {"x": 0, "y": 0.5, "z": -3}}
                },
                "Weapon": {
                    "components": ["BoxCollider"]
                }
            }
        }
    }
})
```

## UI Development

### Always Set Up UI Scene First

```python
# ✅ DO: Set up UI scene first
unity_scene_quickSetup({"setupType": "UI"})

# Now create UI elements
unity_ugui_createFromTemplate({"template": "Button"})
```

### Use Layouts for Organization

```python
# ✅ DO: Use layouts for dynamic UI
unity_ugui_layoutManage({
    "operation": "add",
    "gameObjectPath": "Canvas/Panel",
    "layoutType": "VerticalLayoutGroup",
    "spacing": 10,
    "padding": {"left": 20, "right": 20, "top": 20, "bottom": 20},
    "childControlWidth": True
})
```

## Prefab Workflow

### Use Prefabs for Repeated Objects

```python
# ✅ DO: Create prefab for enemies
unity_prefab_crud({
    "operation": "create",
    "gameObjectPath": "Enemy_Template",
    "prefabPath": "Assets/Prefabs/Enemy.prefab"
})

# Instantiate multiple times
for i in range(5):
    unity_prefab_crud({
        "operation": "instantiate",
        "prefabPath": "Assets/Prefabs/Enemy.prefab",
        "parentPath": "Enemies"
    })
```

### Apply Overrides Carefully

```python
# ✅ DO: Know the difference

# applyOverrides: Saves changes to prefab (affects ALL instances!)
unity_prefab_crud({
    "operation": "applyOverrides",
    "gameObjectPath": "Enemy_Boss"  # Save boss modifications to prefab
})

# revertOverrides: Discards changes (resets to prefab)
unity_prefab_crud({
    "operation": "revertOverrides",
    "gameObjectPath": "Enemy_1"  # Reset to original prefab
})
```

## Asset Management

### Never Edit .meta Files

```python
# ❌ NEVER DO THIS: Editing .meta files manually
# This can break asset references!
unity_asset_crud({
    "operation": "update",
    "assetPath": "Assets/Scripts/Player.cs.meta",
    "content": "..."
})

# ✅ DO: Let Unity manage .meta files automatically
```

### Use updateImporter for Asset Settings

```python
# ✅ DO: Modify importer settings, not file content
unity_asset_crud({
    "operation": "updateImporter",
    "assetPath": "Assets/Textures/Sprite.png",
    "propertyChanges": {
        "textureType": "Sprite",
        "filterMode": "Point",
        "wrapMode": "Clamp"
    }
})
```

## Scene Management

### Save Regularly

```python
# ✅ DO: Save after major changes
unity_scene_crud({"operation": "save"})
```

### Use Additive Loading for Multi-Scene Setups

```python
# ✅ DO: Load scenes additively
unity_scene_crud({
    "operation": "load",
    "scenePath": "Assets/Scenes/Gameplay.unity"
})

unity_scene_crud({
    "operation": "load",
    "scenePath": "Assets/Scenes/UI.unity",
    "additive": True
})
```

## Inspection Strategies

### Progressive Inspection

```python
# 1. Quick overview (fast)
unity_context_inspect({
    "includeHierarchy": True,
    "includeComponents": False,
    "maxDepth": 2
})

# 2. Check specific object exists (very fast)
unity_gameobject_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "includeProperties": False
})

# 3. Get detailed info only when needed (slower)
unity_gameobject_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "includeProperties": True
})
```

## Component Management

### Use GlobalObjectId for Stable References

```python
# ✅ BETTER: Use GlobalObjectId (survives renames/moves)
unity_component_crud({
    "operation": "update",
    "gameObjectGlobalObjectId": "GlobalObjectId_V1-1-abc123-456-0",
    "componentType": "UnityEngine.Transform",
    "propertyChanges": {"position": {"x": 5, "y": 0, "z": 0}}
})

# ✅ ALSO GOOD: Use path (human-readable)
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Transform",
    "propertyChanges": {"position": {"x": 5, "y": 0, "z": 0}}
})
```

### Set Asset References Properly

```python
# ✅ BEST: Use GUID (most reliable)
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.MeshFilter",
    "propertyChanges": {
        "mesh": {"_ref": "asset", "guid": "abc123def456"}
    }
})

# ✅ GOOD: Use asset path
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.MeshFilter",
    "propertyChanges": {
        "mesh": {"_ref": "asset", "path": "Assets/Models/Sphere.fbx"}
    }
})

# ✅ OK: Direct path string (for simple cases)
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.MeshFilter",
    "propertyChanges": {
        "mesh": "Assets/Models/Sphere.fbx"
    }
})
```

## Summary Checklist

Before starting any Unity MCP session:

- [ ] Unity MCP Bridge is running
- [ ] I understand which tools to use
- [ ] I'll check context before making changes
- [ ] I'll use templates when available
- [ ] I'll batch related operations
- [ ] I'll use script batch manager for ALL scripts
- [ ] I'll optimize with includeProperties=false and propertyFilter
- [ ] I'll limit batch operations with maxResults
- [ ] I'll use stopOnError=false for resilience
- [ ] I'll save my work regularly
- [ ] I'll never edit .meta files

---

**Next:** [Performance Tips](performance-tips.md)
