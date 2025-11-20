# Troubleshooting Guide

Common issues and solutions when using Unity MCP Skill.

## Connection Issues

### Error: "Unity bridge is not connected"

**Symptoms:**
- All Unity MCP commands fail
- Error message: "Unity bridge is not connected"

**Solutions:**

1. **Check Unity Editor is open**
   ```
   Ensure Unity Editor is running with your project
   ```

2. **Start MCP Bridge in Unity**
   - Go to **Tools > MCP Assistant** in Unity Editor
   - Click **Start Bridge**
   - Verify status shows "Connected"

3. **Check firewall settings**
   - Ensure localhost connections are allowed
   - Default port: 7077

4. **Restart both sides**
   - Close Unity Editor
   - Restart MCP client (Claude Desktop/Cursor)
   - Reopen Unity and start bridge

### Bridge Starts But Immediately Disconnects

**Symptoms:**
- Bridge shows "Connected" then "Disconnected"
- Connection drops after a few seconds

**Solutions:**

1. **Check for port conflicts**
   ```bash
   # Windows: Check if port 7077 is in use
   netstat -ano | findstr :7077

   # Linux/Mac
   lsof -i :7077
   ```

2. **Change bridge port** (if needed)
   - Edit Unity bridge settings
   - Update MCP configuration to match

3. **Check Unity Console for errors**
   - Look for WebSocket errors
   - Look for MCPBridge exceptions

## Script Compilation Issues

### Error: "Compilation failed" or "Script errors"

**Symptoms:**
- Scripts create but don't compile
- Compilation errors in Unity Console
- Components can't be added to GameObjects

**Solutions:**

1. **Always use `unity_script_batch_manage`**
   ```python
   # ✅ CORRECT
   unity_script_batch_manage({
       "scripts": [
           {"operation": "create", "scriptPath": "Assets/Scripts/Player.cs", "content": "..."}
       ],
       "timeoutSeconds": 30
   })
   ```

   ```python
   # ❌ WRONG - Causes compilation issues!
   unity_asset_crud({
       "operation": "create",
       "assetPath": "Assets/Scripts/Player.cs",
       "content": "..."
   })
   ```

2. **Check script syntax**
   - Ensure valid C# code
   - Check for missing semicolons, braces
   - Verify namespace declarations

3. **Increase timeout for large projects**
   ```python
   unity_script_batch_manage({
       "scripts": [...],
       "timeoutSeconds": 60  # Increase from default 30
   })
   ```

4. **Check compilation results**
   ```python
   # Wait for compilation and get results including errors
   result = unity_await_compilation({"timeoutSeconds": 60})
   if not result["success"]:
       print(f"Compilation errors: {result['errorMessages']}")
   ```

### Error: "Compilation did not finish within timeout"

**Symptoms:**
- Scripts created but compilation timeout
- Message: "Compilation did not finish within X seconds"

**Solutions:**

1. **Increase timeout**
   ```python
   unity_script_batch_manage({
       "scripts": [...],
       "timeoutSeconds": 90  # Increase for large projects
   })
   ```

2. **Check for compilation errors**
   ```python
   # Wait manually for compilation
   unity_await_compilation({
       "timeoutSeconds": 120
   })
   ```

3. **Check compilation results**
   ```python
   # Wait for compilation and review detailed results
   result = unity_await_compilation({"timeoutSeconds": 120})
   print(f"Success: {result['success']}")
   print(f"Errors: {result['errorCount']}")
   if result['errorMessages']:
       for msg in result['errorMessages']:
           print(msg)
   ```

## GameObject/Component Issues

### Error: "GameObject not found"

**Symptoms:**
- Error: "GameObject at path 'X' not found"
- Operations fail on specific objects

**Solutions:**

1. **Check hierarchy path**
   ```python
   # Verify current hierarchy
   unity_context_inspect({
       "includeHierarchy": True,
       "maxDepth": 3
   })
   ```

2. **Use correct path format**
   ```python
   # ✅ CORRECT: Parent/Child/Target
   "gameObjectPath": "Canvas/Panel/Button"

   # ❌ WRONG: Missing parent hierarchy
   "gameObjectPath": "Button"
   ```

3. **Check object exists in active scene**
   ```python
   # Find object by pattern
   unity_gameobject_crud({
       "operation": "findMultiple",
       "pattern": "*Button*",
       "maxResults": 50
   })
   ```

### Error: "Component type not found"

**Symptoms:**
- Error: "Component type 'X' not found"
- Can't add or update components

**Solutions:**

1. **Use fully qualified names**
   ```python
   # ✅ CORRECT
   "componentType": "UnityEngine.UI.Button"

   # ❌ WRONG
   "componentType": "Button"
   ```

2. **Common component namespaces:**
   - UI components: `UnityEngine.UI.*`
   - Standard: `UnityEngine.*`
   - Custom scripts: `YourNamespace.ClassName`

3. **For custom scripts, ensure compilation succeeded**
   ```python
   # Check compilation status
   unity_await_compilation({"timeoutSeconds": 30})
   ```

## UI Issues

### Error: "Must be under a Canvas"

**Symptoms:**
- Can't create UI elements
- Error mentions Canvas requirement

**Solutions:**

1. **Set up UI scene first**
   ```python
   # ✅ Always set up UI scene first
   unity_scene_quickSetup({"setupType": "UI"})

   # Then create UI elements
   unity_ugui_createFromTemplate({"template": "Button"})
   ```

2. **Specify parent path under Canvas**
   ```python
   unity_ugui_createFromTemplate({
       "template": "Button",
       "parentPath": "Canvas"  # Explicit parent
   })
   ```

### UI Elements Not Visible

**Symptoms:**
- UI elements created but not visible in Game view
- Elements exist in Hierarchy but don't render

**Solutions:**

1. **Check Canvas render mode**
   ```python
   unity_component_crud({
       "operation": "inspect",
       "gameObjectPath": "Canvas",
       "componentType": "UnityEngine.Canvas",
       "propertyFilter": ["renderMode"]
   })
   ```

2. **Check EventSystem exists**
   ```python
   unity_gameobject_crud({
       "operation": "findMultiple",
       "pattern": "EventSystem"
   })
   ```

3. **Check RectTransform settings**
   ```python
   unity_ugui_manage({
       "operation": "inspect",
       "gameObjectPath": "Canvas/Button"
   })
   ```

## Performance Issues

### Operations Timing Out

**Symptoms:**
- Commands take too long
- Timeout errors on batch operations

**Solutions:**

1. **Use maxResults to limit operations**
   ```python
   unity_component_crud({
       "operation": "addMultiple",
       "pattern": "Enemy*",
       "componentType": "UnityEngine.Rigidbody",
       "maxResults": 1000  # Prevent timeout
   })
   ```

2. **Use includeProperties=false for faster inspection**
   ```python
   unity_gameobject_crud({
       "operation": "inspect",
       "gameObjectPath": "Player",
       "includeProperties": False  # 10x faster
   })
   ```

3. **Use propertyFilter for specific properties**
   ```python
   unity_component_crud({
       "operation": "inspect",
       "gameObjectPath": "Player",
       "componentType": "UnityEngine.Transform",
       "propertyFilter": ["position"]  # Only what you need
   })
   ```

See [Performance Tips](guides/performance-tips.md) for detailed optimization strategies.

### Slow Script Creation

**Symptoms:**
- Creating multiple scripts takes very long
- Each script triggers separate compilation

**Solutions:**

1. **Batch all scripts together**
   ```python
   # ✅ FAST: Single compilation for all scripts
   unity_script_batch_manage({
       "scripts": [
           {"operation": "create", ...},
           {"operation": "create", ...},
           {"operation": "create", ...}
       ]
   })
   ```

2. **Don't create scripts individually**
   ```python
   # ❌ SLOW: Multiple compilations
   for script in scripts:
       unity_script_batch_manage({
           "scripts": [script]
       })
   ```

## Asset Issues

### Error: "Asset not found"

**Symptoms:**
- Asset operations fail
- Error: "Asset at path 'X' not found"

**Solutions:**

1. **Use correct asset path format**
   ```python
   # ✅ CORRECT: Must start with Assets/
   "assetPath": "Assets/Materials/Material.mat"

   # ❌ WRONG: Missing Assets/ prefix
   "assetPath": "Materials/Material.mat"
   ```

2. **Check asset exists**
   ```python
   unity_asset_crud({
       "operation": "findMultiple",
       "pattern": "Assets/Materials/*.mat"
   })
   ```

3. **Use forward slashes, not backslashes**
   ```python
   # ✅ CORRECT
   "Assets/Scripts/Player.cs"

   # ❌ WRONG
   "Assets\\Scripts\\Player.cs"
   ```

### .meta File Corruption

**Symptoms:**
- "Missing script" errors
- Asset references broken
- GUIDs changed unexpectedly

**Solutions:**

1. **NEVER manually edit .meta files**
   ```python
   # ❌ NEVER DO THIS
   unity_asset_crud({
       "operation": "update",
       "assetPath": "Assets/Scripts/Player.cs.meta",
       "content": "..."
   })
   ```

2. **Let Unity regenerate .meta files**
   - Delete corrupted .meta file
   - Unity will regenerate automatically
   - References may be lost (restore from backup)

3. **Use version control for .meta files**
   - Always commit .meta files to git
   - Prevents GUID conflicts

## Prefab Issues

### Error: "GameObject is not a prefab instance"

**Symptoms:**
- Can't apply/revert overrides
- Error mentions prefab instance requirement

**Solutions:**

1. **Check if GameObject is prefab instance**
   ```python
   unity_prefab_crud({
       "operation": "inspect",
       "prefabPath": "Assets/Prefabs/Enemy.prefab"
   })
   ```

2. **Unpack only applies to instances**
   ```python
   # Only works on prefab instances, not prefab assets
   unity_prefab_crud({
       "operation": "unpack",
       "gameObjectPath": "Enemy(Clone)"  # Instance, not asset
   })
   ```

### Changes to Prefab Don't Affect Instances

**Symptoms:**
- Modified prefab but instances unchanged
- Instance overrides prevent updates

**Solutions:**

1. **Revert instance overrides first**
   ```python
   unity_prefab_crud({
       "operation": "revertOverrides",
       "gameObjectPath": "Enemy_1"
   })
   ```

2. **Check for local overrides**
   ```python
   unity_gameobject_crud({
       "operation": "inspect",
       "gameObjectPath": "Enemy_1",
       "includeProperties": True
   })
   ```

## Batch Operation Errors

### Some Operations Succeed, Others Fail

**Symptoms:**
- Batch operation partially succeeds
- Error count > 0 in results

**Solutions:**

1. **Use stopOnError=false**
   ```python
   result = unity_component_crud({
       "operation": "addMultiple",
       "pattern": "Enemy*",
       "componentType": "UnityEngine.Rigidbody",
       "stopOnError": False  # Continue on errors
   })

   # Review errors
   for error in result["errors"]:
       print(f"Failed: {error['gameObject']}: {error['error']}")
   ```

2. **Check individual errors**
   - Some objects may already have component
   - Some paths may be invalid
   - Some objects may be read-only

## Debugging Tips

### Get Detailed Error Messages

```python
# 1. Check compilation results (includes console errors)
result = unity_await_compilation({"timeoutSeconds": 60})
if not result["success"]:
    print(f"Compilation errors:")
    for msg in result["errorMessages"]:
        print(f"  {msg}")

# 2. Get full context
unity_context_inspect({
    "includeHierarchy": True,
    "includeComponents": True,
    "maxDepth": 3
})

# 3. Inspect specific object
unity_gameobject_crud({
    "operation": "inspect",
    "gameObjectPath": "Player",
    "includeProperties": True
})
```

### Test in Isolation

```python
# 1. Create new scene
unity_scene_quickSetup({"setupType": "3D"})

# 2. Test minimal operation
unity_gameobject_createFromTemplate({
    "template": "Cube",
    "name": "TestCube"
})

# 3. If works, check what's different in your scene
```

## Getting Help

If issues persist:

1. **Check Unity Console**
   - Look for errors/warnings
   - Note exact error messages

2. **Check MCP Bridge Status**
   - Tools > MCP Assistant
   - Verify "Connected" status

3. **Check Unity Version**
   - Ensure Unity 2021.3+
   - Some features require newer versions

4. **Review CLAUDE.md**
   - Contains detailed API documentation
   - Includes troubleshooting sections

5. **Create Minimal Reproduction**
   - New scene
   - Minimal steps to reproduce
   - Note exact error messages

## Common Error Messages Reference

| Error | Cause | Solution |
|-------|-------|----------|
| "Unity bridge is not connected" | Bridge not running | Start bridge in Tools > MCP Assistant |
| "GameObject not found" | Invalid path | Use `context_inspect` to find correct path |
| "Component type not found" | Missing namespace | Use full name: `UnityEngine.UI.Button` |
| "Must be under a Canvas" | UI without Canvas | Use `scene_quickSetup({"setupType": "UI"})` |
| "Compilation failed" | Script syntax error | Check `await_compilation` results for error details |
| "Compilation timeout" | Large project | Increase `timeoutSeconds` |
| "Asset not found" | Wrong path | Use "Assets/" prefix, forward slashes |
| "Operation timed out" | Too many objects | Use `maxResults` parameter |

---

**See Also:**
- [Getting Started](guides/getting-started.md)
- [Best Practices](guides/best-practices.md)
- [Performance Tips](guides/performance-tips.md)
