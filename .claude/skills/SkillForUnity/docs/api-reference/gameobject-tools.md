# GameObject Management Tools API Reference

Complete API reference for Unity MCP GameObject management tools.

## unity_gameobject_crud

Modify scene hierarchy - create, delete, move, rename, duplicate, and inspect GameObjects.

### Operations

- **create** - Create a new GameObject
- **delete** - Delete a GameObject
- **move** - Move GameObject to new parent
- **rename** - Rename a GameObject
- **duplicate** - Duplicate a GameObject
- **inspect** - Inspect GameObject and components
- **findMultiple** - Find GameObjects by pattern
- **deleteMultiple** - Delete multiple GameObjects
- **inspectMultiple** - Inspect multiple GameObjects

### Examples

```python
# Create GameObject
unity_gameobject_crud({
    "operation": "create",
    "name": "Player",
    "parentPath": "Characters"
})

# Inspect with all components
unity_gameobject_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "includeProperties": True
})

# Fast inspection (component types only)
unity_gameobject_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "includeProperties": False  # 10x faster!
})

# Find multiple by pattern
unity_gameobject_crud({
    "operation": "findMultiple",
    "pattern": "Enemy*",
    "maxResults": 100
})
```

### Performance Parameters

- **includeProperties** - Set to `false` for 10x faster inspection
- **componentFilter** - Array of component types to inspect
- **maxResults** - Limit for multiple operations (default: 1000)

---

## unity_gameobject_createFromTemplate

Create GameObjects from templates with one command.

### Available Templates

**Primitives:**
- Cube, Sphere, Plane, Cylinder, Capsule, Quad

**Lights:**
- Light-Directional, Light-Point, Light-Spot

**Special:**
- Camera, Empty, Player, Enemy, Particle System, Audio Source

### Example

```python
unity_gameobject_createFromTemplate({
    "template": "Player",
    "name": "MainPlayer",
    "position": {"x": 0, "y": 1, "z": 0},
    "scale": {"x": 1, "y": 1, "z": 1}
})
```

---

## unity_hierarchy_builder

Build complex nested GameObject structures declaratively.

### Example

```python
unity_hierarchy_builder({
    "hierarchy": {
        "Player": {
            "components": ["Rigidbody", "CapsuleCollider"],
            "properties": {
                "position": {"x": 0, "y": 1, "z": 0}
            },
            "children": {
                "Camera": {
                    "components": ["Camera"],
                    "properties": {
                        "position": {"x": 0, "y": 0.5, "z": -3}
                    }
                },
                "Weapon": {
                    "components": ["BoxCollider"]
                }
            }
        }
    },
    "parentPath": "Characters"
})
```

## Performance Tips

```python
# ✅ Fast: Check existence only
unity_gameobject_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "includeProperties": False
})

# ✅ Fast: Specific components only
unity_gameobject_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentFilter": ["UnityEngine.Transform"]
})

# ✅ Safe: Limit batch operations
unity_gameobject_crud({
    "operation": "findMultiple",
    "pattern": "Enemy*",
    "maxResults": 100
})
```

See CLAUDE.md for complete API details.
