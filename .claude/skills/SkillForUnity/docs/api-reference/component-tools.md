# Component Management Tools API Reference

Complete API reference for Unity MCP component management tools.

## unity_component_crud

Add, remove, update, or inspect components on GameObjects.

### Operations

- **add** - Add component to GameObject
- **remove** - Remove component from GameObject
- **update** - Update component properties
- **inspect** - Inspect component state
- **addMultiple** - Add component to multiple GameObjects
- **removeMultiple** - Remove component from multiple GameObjects
- **updateMultiple** - Update component on multiple GameObjects
- **inspectMultiple** - Inspect component on multiple GameObjects

### Examples

```python
# Add component
unity_component_crud({
    "operation": "add",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Rigidbody",
    "propertyChanges": {
        "mass": 2.0,
        "useGravity": True
    }
})

# Update component
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Transform",
    "propertyChanges": {
        "position": {"x": 0, "y": 1, "z": 0}
    }
})

# Fast inspection (type only)
unity_component_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Rigidbody",
    "includeProperties": False  # 10x faster!
})

# Inspect specific properties
unity_component_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Transform",
    "propertyFilter": ["position", "rotation"]
})

# Batch add to multiple GameObjects
unity_component_crud({
    "operation": "addMultiple",
    "pattern": "Enemy*",
    "componentType": "UnityEngine.Rigidbody",
    "propertyChanges": {
        "mass": 2.0
    },
    "maxResults": 1000,
    "stopOnError": False
})
```

### Performance Parameters

- **includeProperties** - Set to `false` for 10x faster inspection
- **propertyFilter** - Array of property names to inspect
- **maxResults** - Maximum objects to process (default: 1000)
- **stopOnError** - Stop on first error if `true` (default: `false`)

### Setting Asset References

```python
# Using GUID (recommended)
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.MeshFilter",
    "propertyChanges": {
        "mesh": {
            "_ref": "asset",
            "guid": "abc123def456789"
        }
    }
})

# Using asset path
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.MeshRenderer",
    "propertyChanges": {
        "material": {
            "_ref": "asset",
            "path": "Assets/Materials/PlayerMaterial.mat"
        }
    }
})

# Using built-in resources
unity_component_crud({
    "operation": "update",
    "gameObjectPath": "Sphere",
    "componentType": "UnityEngine.MeshFilter",
    "propertyChanges": {
        "sharedMesh": "Library/unity default resources::Sphere"
    }
})
```

### Setting UnityEvent Listeners

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

**Supported Listener Modes:**
- **Void** - No arguments
- **Int** - Integer argument
- **Float** - Float argument
- **String** - String argument
- **Bool** - Boolean argument
- **Object** - UnityEngine.Object argument

## Performance Tips

```python
# ✅ Fast: Check if component exists
unity_component_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.CharacterController",
    "includeProperties": False
})

# ✅ Fast: Get specific properties only
unity_component_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Transform",
    "propertyFilter": ["position"]
})

# ✅ Safe: Batch with error handling
unity_component_crud({
    "operation": "addMultiple",
    "pattern": "Enemy*",
    "componentType": "UnityEngine.BoxCollider",
    "stopOnError": False,
    "maxResults": 1000
})
```

## Best Practices

1. **Test batch operations with small maxResults first:**
   ```python
   # Test with 10
   result = unity_component_crud({
       "operation": "updateMultiple",
       "pattern": "Enemy*",
       "componentType": "UnityEngine.Rigidbody",
       "propertyChanges": {"mass": 5.0},
       "maxResults": 10
   })

   # If successful, scale up
   if result["errorCount"] == 0:
       unity_component_crud({..., "maxResults": 1000})
   ```

2. **Use stopOnError=false for resilient operations:**
   ```python
   result = unity_component_crud({
       "operation": "addMultiple",
       "pattern": "Enemy*",
       "componentType": "UnityEngine.Rigidbody",
       "stopOnError": False
   })

   # Review errors
   for error in result["errors"]:
       print(f"Failed: {error['gameObject']}: {error['error']}")
   ```

See CLAUDE.md for complete API details.
