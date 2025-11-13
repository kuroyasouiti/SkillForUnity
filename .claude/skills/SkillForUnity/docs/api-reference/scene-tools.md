# Scene Management Tools API Reference

Complete API reference for Unity MCP scene management tools.

## unity_scene_crud

Create, load, save, delete, or duplicate Unity scenes.

### Operations

- **create** - Create a new empty scene
- **load** - Load an existing scene
- **save** - Save the current scene
- **delete** - Delete a scene file
- **duplicate** - Duplicate an existing scene

### Examples

```python
# Create new scene
unity_scene_crud({
    "operation": "create",
    "scenePath": "Assets/Scenes/Level1.unity"
})

# Load scene
unity_scene_crud({
    "operation": "load",
    "scenePath": "Assets/Scenes/MainMenu.unity"
})

# Save current scene
unity_scene_crud({"operation": "save"})
```

---

## unity_scene_quickSetup

Instantly set up scenes with common configurations.

### Setup Types

- **3D** - Camera + Directional Light
- **2D** - 2D Camera (orthographic)
- **UI** - Canvas + EventSystem
- **VR** - VR Camera setup
- **Empty** - Empty scene

### Example

```python
unity_scene_quickSetup({"setupType": "3D"})
```

---

## unity_context_inspect

Get comprehensive scene overview.

### Example

```python
unity_context_inspect({
    "includeHierarchy": True,
    "includeComponents": True,
    "maxDepth": 3
})
```

See CLAUDE.md for complete API details.
