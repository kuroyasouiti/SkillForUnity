# Performance Tips for Unity MCP Skill

This guide provides detailed performance optimization strategies for Unity MCP Skill.

## Performance Benchmarks

Understanding performance characteristics helps you make better decisions:

| Operation | No Optimization | Optimized | Speedup |
|-----------|----------------|-----------|---------|
| Inspect GameObject (all components) | 3s | 0.3s | **10x** |
| Inspect Component (all properties) | 2s | 0.2s | **10x** |
| Inspect Component (specific props) | 2s | 0.5s | **4x** |
| Find 10000 GameObjects | Timeout | 2s | **No timeout** |
| Add components to 5000 objects | Timeout | 5s | **No timeout** |
| Inspect 1000 components | Timeout | 3s | **No timeout** |
| Create 1 script | 5s | 5s | 1x |
| Create 10 scripts (individual) | 50s | 10s | **5x** |
| Create 20 scripts (individual) | 100s | 15s | **6.7x** |

## Optimization Techniques

### 1. GameObject/Component Inspection

#### Ultra-Fast: Existence Check

Use `includeProperties=false` to check if something exists without reading all data:

```python
# ⚡ 0.1 seconds - Just check if component exists
unity_component_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.CharacterController",
    "includeProperties": False
})

# Result: { "exists": true, "componentType": "UnityEngine.CharacterController" }
```

#### Fast: Specific Properties Only

Use `propertyFilter` to read only what you need:

```python
# ⚡ 0.3 seconds - Get only position and rotation
unity_component_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Transform",
    "propertyFilter": ["position", "rotation"]
})

# Result: { "position": {...}, "rotation": {...} }
```

#### Slow: Full Inspection

Avoid full inspection unless absolutely necessary:

```python
# 🐌 3 seconds - Reads ALL properties (50+ fields!)
unity_component_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Transform"
})
```

### 2. Multi-Object Operations

#### Safe: Limited Batch with Error Handling

Always use `maxResults` to prevent timeouts:

```python
# ⚡ 2 seconds for 1000 objects
result = unity_component_crud({
    "operation": "addMultiple",
    "pattern": "Enemy*",
    "componentType": "UnityEngine.Rigidbody",
    "maxResults": 1000,  # Prevents timeout
    "stopOnError": False  # Continues on errors
})

print(f"Processed: {result['successCount']}/{result['totalCount']}")
print(f"Errors: {result['errorCount']}")
```

#### Dangerous: Unlimited Batch

Never run batch operations without limits:

```python
# 🐌 Timeout risk! Could match 10000+ objects
unity_component_crud({
    "operation": "addMultiple",
    "pattern": "*",  # Matches EVERYTHING!
    "componentType": "UnityEngine.Rigidbody"
    # No maxResults - DANGEROUS!
})
```

### 3. Script Management

#### 10-20x Faster: Batch Script Operations

Always batch multiple scripts:

```python
# ⚡ 10 seconds for 10 scripts (single compilation)
unity_script_batch_manage({
    "scripts": [
        {"operation": "create", "scriptPath": "Assets/Scripts/Player.cs", "content": "..."},
        {"operation": "create", "scriptPath": "Assets/Scripts/Enemy.cs", "content": "..."},
        {"operation": "create", "scriptPath": "Assets/Scripts/Weapon.cs", "content": "..."},
        # ... 7 more scripts
    ],
    "timeoutSeconds": 30
})
```

#### Slow: Individual Script Operations

Avoid creating scripts one at a time:

```python
# 🐌 50 seconds for 10 scripts (10 separate compilations!)
for script_name in ["Player", "Enemy", "Weapon", ...]:
    unity_script_batch_manage({
        "scripts": [
            {"operation": "create", "scriptPath": f"Assets/Scripts/{script_name}.cs", "content": "..."}
        ]
    })
    # Each call triggers compilation!
```

## Optimization Workflows

### Workflow 1: Check GameObject Existence

```python
# Step 1: Fast existence check
exists = unity_gameobject_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "includeProperties": False
})

if exists["exists"]:
    # Step 2: Get only needed components
    transform = unity_component_crud({
        "operation": "inspect",
        "gameObjectPath": "Player",
        "componentType": "UnityEngine.Transform",
        "propertyFilter": ["position", "rotation"]
    })
```

### Workflow 2: Batch Operation with Error Handling

```python
# Step 1: Test with small limit
test = unity_component_crud({
    "operation": "addMultiple",
    "pattern": "Enemy*",
    "componentType": "UnityEngine.Rigidbody",
    "maxResults": 10,
    "stopOnError": False
})

print(f"Test: {test['successCount']}/10 succeeded, {test['errorCount']} errors")

# Step 2: Review errors
if test["errorCount"] > 0:
    for error in test["errors"]:
        print(f"Error on {error['gameObject']}: {error['error']}")

# Step 3: If successful, scale up
if test["errorCount"] == 0:
    final = unity_component_crud({
        "operation": "addMultiple",
        "pattern": "Enemy*",
        "componentType": "UnityEngine.Rigidbody",
        "maxResults": 5000,
        "stopOnError": False
    })
    print(f"Final: {final['successCount']} succeeded")
```

### Workflow 3: Progressive Inspection

```python
# Level 1: Quick overview (0.5s)
unity_context_inspect({
    "includeHierarchy": True,
    "includeComponents": False,
    "maxDepth": 2
})

# Level 2: Check specific object (0.1s)
unity_gameobject_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "includeProperties": False
})

# Level 3: Get specific properties (0.3s)
unity_component_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "componentType": "UnityEngine.Transform",
    "propertyFilter": ["position"]
})

# Level 4: Full details only when needed (3s)
unity_gameobject_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "includeProperties": True
})
```

## When to Use Each Optimization

| Use Case | Optimization | Example |
|----------|-------------|---------|
| "Does Player have Rigidbody?" | `includeProperties=false` | Existence check |
| "Get player position and rotation" | `propertyFilter=[...]` | Specific properties |
| "Get Transform component only" | `componentFilter=[...]` | Specific components |
| "Find first 100 enemies" | `maxResults=100` | Exploratory queries |
| "Add colliders to all enemies" | `maxResults=1000` | Bulk modifications |
| "Update all, report failures" | `stopOnError=false` | Resilient batch ops |
| "Create 5+ scripts" | Script batch manager | Script operations |

## Real-World Examples

### Example 1: Find and Modify Enemies (Optimized)

```python
# Fast: Find enemies (1s for 1000 objects)
enemies = unity_gameobject_crud({
    "operation": "findMultiple",
    "pattern": "Enemy*",
    "maxResults": 1000
})

print(f"Found {len(enemies['results'])} enemies")

# Fast: Check if they have Rigidbody (0.1s per check)
for enemy in enemies['results'][:5]:  # Sample first 5
    has_rb = unity_component_crud({
        "operation": "inspect",
        "gameObjectPath": enemy['path'],
        "componentType": "UnityEngine.Rigidbody",
        "includeProperties": False
    })
    print(f"{enemy['path']}: has Rigidbody = {has_rb['exists']}")

# Fast: Add to all at once (2s for 1000)
unity_component_crud({
    "operation": "addMultiple",
    "pattern": "Enemy*",
    "componentType": "UnityEngine.Rigidbody",
    "propertyChanges": {"mass": 2.0},
    "maxResults": 1000,
    "stopOnError": False
})
```

### Example 2: Create Game Scripts (Optimized)

```python
# Fast: Create all scripts at once (15s for 10 scripts)
scripts = []

for script_name in ["Player", "Enemy", "Weapon", "GameManager", "ScoreManager",
                     "AudioManager", "InputHandler", "CameraController",
                     "UIManager", "LevelManager"]:
    scripts.append({
        "operation": "create",
        "scriptPath": f"Assets/Scripts/{script_name}.cs",
        "content": f"""using UnityEngine;

public class {script_name} : MonoBehaviour
{{
    // TODO: Implement {script_name}
}}
"""
    })

# Single batch operation - much faster!
result = unity_script_batch_manage({
    "scripts": scripts,
    "stopOnError": False,
    "timeoutSeconds": 60
})

print(f"Created {result['batch']['successCount']}/{len(scripts)} scripts")
print(f"Compilation: {result['compilation']['success']}")
```

### Example 3: Inspect Large Scene (Optimized)

```python
# Step 1: Quick overview (1s)
overview = unity_context_inspect({
    "includeHierarchy": True,
    "includeComponents": False,
    "maxDepth": 2
})

print(f"Scene has {len(overview['hierarchy'])} root objects")

# Step 2: Count specific types (0.5s)
enemies = unity_gameobject_crud({
    "operation": "findMultiple",
    "pattern": "Enemy*",
    "maxResults": 100
})

collectibles = unity_gameobject_crud({
    "operation": "findMultiple",
    "pattern": "Coin*",
    "maxResults": 100
})

print(f"Enemies: {len(enemies['results'])}, Collectibles: {len(collectibles['results'])}")

# Step 3: Sample inspection (2s for 10 objects)
for enemy_path in enemies['results'][:10]:
    components = unity_gameobject_crud({
        "operation": "inspect",
        "gameObjectPath": enemy_path['path'],
        "includeProperties": False,
        "componentFilter": ["UnityEngine.Rigidbody", "UnityEngine.BoxCollider"]
    })
    print(f"{enemy_path['path']}: {len(components['components'])} components")
```

## Performance Checklist

Before running operations on large scenes:

- [ ] Am I using `includeProperties=false` for existence checks?
- [ ] Am I using `propertyFilter` for specific properties?
- [ ] Am I using `componentFilter` for specific components?
- [ ] Am I using `maxResults` to limit batch operations?
- [ ] Am I using `stopOnError=false` for resilience?
- [ ] Am I batching multiple scripts together?
- [ ] Am I testing with small limits before scaling up?
- [ ] Am I inspecting progressively (quick → detailed)?

## Performance Anti-Patterns

### ❌ Anti-Pattern 1: Full Inspection in Loop

```python
# 🐌 30 seconds for 10 objects!
for i in range(10):
    result = unity_gameobject_crud({
        "operation": "inspect",
        "gameObjectPath": f"Enemy_{i}",
        "includeProperties": True  # Full inspection!
    })
```

### ✅ Solution: Use inspectMultiple with Filters

```python
# ⚡ 2 seconds for 10 objects
result = unity_gameobject_crud({
    "operation": "inspectMultiple",
    "pattern": "Enemy_*",
    "includeComponents": False,
    "maxResults": 10
})
```

### ❌ Anti-Pattern 2: Individual Script Creation

```python
# 🐌 50 seconds for 10 scripts!
for i in range(10):
    unity_script_batch_manage({
        "scripts": [{"operation": "create", ...}]
    })
```

### ✅ Solution: Batch All Scripts

```python
# ⚡ 10 seconds for 10 scripts
unity_script_batch_manage({
    "scripts": [
        {"operation": "create", ...},
        {"operation": "create", ...},
        # ... all 10 scripts
    ]
})
```

---

**See Also:**
- [Best Practices](best-practices.md)
- [Getting Started](getting-started.md)
- [Troubleshooting](../troubleshooting.md)
