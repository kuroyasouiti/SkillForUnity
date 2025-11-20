# Script Management Tools API Reference

Complete API reference for Unity MCP C# script management tools.

## unity_script_batch_manage

**CRITICAL: ALWAYS use this tool for ALL C# script operations!**

Batch manage C# scripts with automatic compilation handling. This is the ONLY correct way to manage scripts - using `unity_asset_crud` for scripts will cause compilation issues!

### Benefits

1. **10-20x faster** for multiple scripts (single compilation)
2. **Atomic operations** - all succeed or fail together
3. **Automatic compilation** detection and waiting
4. **Proper error reporting** with per-script results

### Operations

- **create** - Create new C# script file
- **update** - Update existing C# script file
- **delete** - Delete C# script file
- **inspect** - Inspect C# script content

### Examples

**Single Script Creation:**
```python
# ALWAYS use scripts array, even for single script!
unity_script_batch_manage({
    "scripts": [
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/Player.cs",
            "content": """using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical);
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
}
"""
        }
    ],
    "timeoutSeconds": 30
})
```

**Multiple Scripts (Recommended):**
```python
# Create multiple scripts at once - much faster!
unity_script_batch_manage({
    "scripts": [
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/Player.cs",
            "content": "using UnityEngine;\n\npublic class Player : MonoBehaviour\n{\n    // Player logic\n}\n"
        },
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/Enemy.cs",
            "content": "using UnityEngine;\n\npublic class Enemy : MonoBehaviour\n{\n    // Enemy logic\n}\n"
        },
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/GameManager.cs",
            "content": "using UnityEngine;\n\npublic class GameManager : MonoBehaviour\n{\n    // Game management logic\n}\n"
        }
    ],
    "stopOnError": False,
    "timeoutSeconds": 30
})
# All scripts created, then single compilation (10-20x faster!)
```

**Update Script:**
```python
unity_script_batch_manage({
    "scripts": [
        {
            "operation": "update",
            "scriptPath": "Assets/Scripts/Player.cs",
            "content": """using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 10f;  // Increased speed!
    public float jumpForce = 5f;   // New feature!

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical);
        transform.Translate(movement * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
"""
        }
    ],
    "timeoutSeconds": 30
})
```

**Delete Script:**
```python
unity_script_batch_manage({
    "scripts": [
        {
            "operation": "delete",
            "scriptPath": "Assets/Scripts/OldScript.cs"
        }
    ],
    "timeoutSeconds": 30
})
```

**Inspect Script:**
```python
unity_script_batch_manage({
    "scripts": [
        {
            "operation": "inspect",
            "scriptPath": "Assets/Scripts/Player.cs"
        }
    ]
})
```

**With Namespace:**
```python
unity_script_batch_manage({
    "scripts": [
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/Combat/Weapon.cs",
            "namespace": "MyGame.Combat",
            "content": """using UnityEngine;

namespace MyGame.Combat
{
    public class Weapon : MonoBehaviour
    {
        public int damage = 10;

        public void Attack()
        {
            // Attack logic
        }
    }
}
"""
        }
    ],
    "timeoutSeconds": 30
})
```

### Parameters

- **scripts** (required) - Array of script operations
  - **operation** (required) - "create", "update", "delete", or "inspect"
  - **scriptPath** (required) - Path to .cs file (e.g., "Assets/Scripts/Player.cs")
  - **content** - Script content (required for create/update)
  - **namespace** - Optional namespace for the script

- **stopOnError** (optional) - Stop on first error (default: false)
- **timeoutSeconds** (optional) - Compilation timeout (default: 30)

### Return Value

```json
{
    "batch": {
        "success": true,
        "totalCount": 3,
        "successCount": 3,
        "errorCount": 0,
        "results": [
            {"scriptPath": "Assets/Scripts/Player.cs", "success": true},
            {"scriptPath": "Assets/Scripts/Enemy.cs", "success": true},
            {"scriptPath": "Assets/Scripts/GameManager.cs", "success": true}
        ],
        "compilationTriggered": true
    },
    "compilation": {
        "success": true,
        "completed": true,
        "errorCount": 0,
        "warningCount": 0,
        "message": "Compilation completed successfully"
    }
}
```

## Best Practices

### ✅ DO: Always Use Script Batch Manager

```python
# ✅ CORRECT: Use script batch manager
unity_script_batch_manage({
    "scripts": [
        {"operation": "create", "scriptPath": "Assets/Scripts/Player.cs", "content": "..."}
    ],
    "timeoutSeconds": 30
})
```

### ❌ DON'T: Use Asset CRUD for Scripts

```python
# ❌ WRONG: Will cause compilation issues!
unity_asset_crud({
    "operation": "create",
    "assetPath": "Assets/Scripts/Player.cs",
    "content": "..."
})
```

### ✅ DO: Batch Multiple Scripts

```python
# ✅ CORRECT: Batch for better performance
unity_script_batch_manage({
    "scripts": [
        {"operation": "create", "scriptPath": "Assets/Scripts/Player.cs", ...},
        {"operation": "create", "scriptPath": "Assets/Scripts/Enemy.cs", ...},
        {"operation": "create", "scriptPath": "Assets/Scripts/Weapon.cs", ...}
    ]
})
# 10-20x faster than individual operations!
```

### ✅ DO: Use stopOnError=False for Resilience

```python
# ✅ CORRECT: Continue on errors, review later
result = unity_script_batch_manage({
    "scripts": [...],
    "stopOnError": False
})

# Check for errors
if result["batch"]["errorCount"] > 0:
    for error in result["batch"]["errors"]:
        print(f"Failed: {error['scriptPath']}: {error['error']}")
```

### ✅ DO: Increase Timeout for Large Projects

```python
# ✅ CORRECT: Large projects need more time
unity_script_batch_manage({
    "scripts": [...],
    "timeoutSeconds": 60  # Increase for large projects
})
```

## Error Handling

```python
# Check compilation result
result = unity_script_batch_manage({
    "scripts": [...]
})

if not result["compilation"]["success"]:
    print("Compilation failed!")
    print(f"Errors: {result['compilation']['errorCount']}")

    # Error messages are included in the compilation result
    if result["compilation"]["errorMessages"]:
        print("Error details:")
        for msg in result["compilation"]["errorMessages"]:
            print(f"  {msg}")
```

## Common Workflows

### Create Player Controller

```python
player_script = """using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Movement
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(h, 0f, v) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
"""

unity_script_batch_manage({
    "scripts": [
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/PlayerController.cs",
            "content": player_script
        }
    ],
    "timeoutSeconds": 30
})
```

### Create Complete Game System

```python
# Create all scripts at once for maximum efficiency
unity_script_batch_manage({
    "scripts": [
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/GameManager.cs",
            "content": "// GameManager script..."
        },
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/Player.cs",
            "content": "// Player script..."
        },
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/Enemy.cs",
            "content": "// Enemy script..."
        },
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/Weapon.cs",
            "content": "// Weapon script..."
        },
        {
            "operation": "create",
            "scriptPath": "Assets/Scripts/UI/ScoreDisplay.cs",
            "content": "// ScoreDisplay script..."
        }
    ],
    "stopOnError": False,
    "timeoutSeconds": 60
})
```

## Performance Benchmarks

| Operation | Individual Calls | Batch Manager | Speedup |
|-----------|-----------------|---------------|---------|
| 1 script | 5s | 5s | 1x |
| 5 scripts | 25s | 7s | **3.5x** |
| 10 scripts | 50s | 10s | **5x** |
| 20 scripts | 100s | 15s | **6.7x** |

**Why so much faster?**
- Individual: Each script triggers separate compilation (5s × N)
- Batch: All scripts written, then single compilation (5s + N×0.5s)

See CLAUDE.md for complete API details and more examples.
