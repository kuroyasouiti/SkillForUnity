"""Test SkillForUnity tools functionality."""
import asyncio
import sys
sys.path.insert(0, 'src')

async def test_context_inspect():
    """Test unity_scene_crud (inspect for context)."""
    from bridge.bridge_connector import bridge_connector
    from bridge.bridge_manager import bridge_manager
    from utils.json_utils import as_pretty_json

    print("\n" + "="*60)
    print("TEST 1: unity_scene_crud (context inspect)")
    print("="*60)

    bridge_connector.start()

    # Wait for connection
    for i in range(20):
        if bridge_manager.is_connected():
            break
        await asyncio.sleep(0.5)

    if not bridge_manager.is_connected():
        print("FAILED: Could not connect to Unity")
        return False

    try:
        # Get Unity context via scene inspect
        result = await bridge_manager.send_command("sceneManage", {
            "operation": "inspect",
            "includeHierarchy": True,
            "includeComponents": False
        })

        print("[PASS] SUCCESS")
        print("\nContext Information:")
        print(f"  Active Scene: {result.get('sceneName', 'Unknown')}")
        print(f"  Scene Path: {result.get('scenePath', 'Unknown')}")
        print(f"  Total GameObjects: {result.get('totalGameObjects', 0)}")
        print(f"  Cameras: {result.get('cameraCount', 0)}")
        print(f"  Lights: {result.get('lightCount', 0)}")

        hierarchy = result.get('hierarchy', [])
        print(f"\n  Root GameObjects: {len(hierarchy)}")
        for obj in hierarchy[:5]:  # Show first 5
            print(f"    - {obj.get('name', 'Unknown')}")

        return True
    except Exception as e:
        print(f"[FAIL] FAILED: {e}")
        return False
    finally:
        await bridge_connector.stop()


async def test_scene_inspect():
    """Test unity_scene_crud (inspect operation)."""
    from bridge.bridge_connector import bridge_connector
    from bridge.bridge_manager import bridge_manager

    print("\n" + "="*60)
    print("TEST 2: unity_scene_crud (inspect)")
    print("="*60)

    bridge_connector.start()

    for i in range(20):
        if bridge_manager.is_connected():
            break
        await asyncio.sleep(0.5)

    if not bridge_manager.is_connected():
        print("FAILED: Could not connect to Unity")
        return False

    try:
        result = await bridge_manager.send_command("sceneManage", {
            "operation": "inspect",
            "includeHierarchy": True,
            "includeComponents": False
        })

        print("[PASS] SUCCESS")
        print(f"\n  Scene Name: {result.get('sceneName', 'Unknown')}")
        print(f"  Scene Path: {result.get('scenePath', 'Unknown')}")
        print(f"  Total GameObjects: {result.get('totalGameObjects', 0)}")
        print(f"  Is Dirty: {result.get('isDirty', False)}")

        return True
    except Exception as e:
        print(f"[FAIL] FAILED: {e}")
        return False
    finally:
        await bridge_connector.stop()


async def test_gameobject_create():
    """Test unity_gameobject_crud (create operation)."""
    from bridge.bridge_connector import bridge_connector
    from bridge.bridge_manager import bridge_manager

    print("\n" + "="*60)
    print("TEST 3: unity_gameobject_crud (create)")
    print("="*60)

    bridge_connector.start()

    for i in range(20):
        if bridge_manager.is_connected():
            break
        await asyncio.sleep(0.5)

    if not bridge_manager.is_connected():
        print("FAILED: Could not connect to Unity")
        return False

    try:
        # Create a test GameObject
        result = await bridge_manager.send_command("gameObjectManage", {
            "operation": "create",
            "name": "TestObject_SkillForUnity"
        })

        print("[PASS] SUCCESS - GameObject created")
        print(f"  Name: {result.get('name', 'Unknown')}")
        print(f"  Path: {result.get('path', 'Unknown')}")

        # Inspect it
        inspect_result = await bridge_manager.send_command("gameObjectManage", {
            "operation": "inspect",
            "gameObjectPath": "TestObject_SkillForUnity"
        })

        print(f"  Components: {len(inspect_result.get('components', []))}")
        for comp in inspect_result.get('components', []):
            print(f"    - {comp}")

        # Delete it
        await bridge_manager.send_command("gameObjectManage", {
            "operation": "delete",
            "gameObjectPath": "TestObject_SkillForUnity"
        })
        print("\n  [OK] Test GameObject cleaned up")

        return True
    except Exception as e:
        print(f"[FAIL] FAILED: {e}")
        import traceback
        traceback.print_exc()
        return False
    finally:
        await bridge_connector.stop()


async def test_component_add():
    """Test unity_component_crud (add operation)."""
    from bridge.bridge_connector import bridge_connector
    from bridge.bridge_manager import bridge_manager

    print("\n" + "="*60)
    print("TEST 4: unity_component_crud (add component)")
    print("="*60)

    bridge_connector.start()

    for i in range(20):
        if bridge_manager.is_connected():
            break
        await asyncio.sleep(0.5)

    if not bridge_manager.is_connected():
        print("FAILED: Could not connect to Unity")
        return False

    try:
        # Create test GameObject
        await bridge_manager.send_command("gameObjectManage", {
            "operation": "create",
            "name": "TestComponent_GameObject"
        })

        # Add Rigidbody component
        result = await bridge_manager.send_command("componentManage", {
            "operation": "add",
            "gameObjectPath": "TestComponent_GameObject",
            "componentType": "UnityEngine.Rigidbody"
        })

        print("[PASS] SUCCESS - Component added")
        print(f"  Component: {result.get('componentType', 'Unknown')}")

        # Inspect component
        inspect_result = await bridge_manager.send_command("componentManage", {
            "operation": "inspect",
            "gameObjectPath": "TestComponent_GameObject",
            "componentType": "UnityEngine.Rigidbody",
            "includeProperties": False  # Fast check
        })

        print(f"  Exists: {inspect_result.get('exists', False)}")

        # Clean up
        await bridge_manager.send_command("gameObjectManage", {
            "operation": "delete",
            "gameObjectPath": "TestComponent_GameObject"
        })
        print("\n  [OK] Test GameObject cleaned up")

        return True
    except Exception as e:
        print(f"[FAIL] FAILED: {e}")
        import traceback
        traceback.print_exc()
        return False
    finally:
        await bridge_connector.stop()


async def test_template_create():
    """Test unity_gameobject_createFromTemplate."""
    from bridge.bridge_connector import bridge_connector
    from bridge.bridge_manager import bridge_manager

    print("\n" + "="*60)
    print("TEST 5: unity_gameobject_createFromTemplate")
    print("="*60)

    bridge_connector.start()

    for i in range(20):
        if bridge_manager.is_connected():
            break
        await asyncio.sleep(0.5)

    if not bridge_manager.is_connected():
        print("FAILED: Could not connect to Unity")
        return False

    try:
        # Create GameObject from template
        result = await bridge_manager.send_command("gameObjectCreateFromTemplate", {
            "template": "Cube",
            "name": "TestCube",
            "position": {"x": 0, "y": 1, "z": 0},
            "scale": {"x": 0.5, "y": 0.5, "z": 0.5}
        })

        print("[PASS] SUCCESS - GameObject created from template")
        print(f"  Name: {result.get('name', 'Unknown')}")
        print(f"  Template: Cube")
        print(f"  Position: (0, 1, 0)")
        print(f"  Scale: (0.5, 0.5, 0.5)")

        # Clean up
        await bridge_manager.send_command("gameObjectManage", {
            "operation": "delete",
            "gameObjectPath": "TestCube"
        })
        print("\n  [OK] Test GameObject cleaned up")

        return True
    except Exception as e:
        print(f"[FAIL] FAILED: {e}")
        import traceback
        traceback.print_exc()
        return False
    finally:
        await bridge_connector.stop()


async def main():
    """Run all tests."""
    print("\n" + "="*60)
    print("SkillForUnity Tools Test Suite")
    print("="*60)

    results = []

    # Test 1: Context Inspect
    results.append(("unity_context_inspect", await test_context_inspect()))
    await asyncio.sleep(1)

    # Test 2: Scene Inspect
    results.append(("unity_scene_crud", await test_scene_inspect()))
    await asyncio.sleep(1)

    # Test 3: GameObject Create
    results.append(("unity_gameobject_crud", await test_gameobject_create()))
    await asyncio.sleep(1)

    # Test 4: Component Add
    results.append(("unity_component_crud", await test_component_add()))
    await asyncio.sleep(1)

    # Test 5: Template Create
    results.append(("unity_gameobject_createFromTemplate", await test_template_create()))

    # Summary
    print("\n" + "="*60)
    print("TEST SUMMARY")
    print("="*60)

    passed = 0
    failed = 0

    for tool_name, success in results:
        status = "[PASS] PASS" if success else "[FAIL] FAIL"
        print(f"{status}  {tool_name}")
        if success:
            passed += 1
        else:
            failed += 1

    print("\n" + "-"*60)
    print(f"Total: {len(results)} tests | Passed: {passed} | Failed: {failed}")
    print("="*60)

    return failed == 0


if __name__ == "__main__":
    success = asyncio.run(main())
    sys.exit(0 if success else 1)

