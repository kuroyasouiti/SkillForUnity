#!/usr/bin/env python3
"""Main features test for SkillForUnity"""

import asyncio
import sys
from pathlib import Path

# Add src directory to path
src_path = Path(__file__).parent / "src"
sys.path.insert(0, str(src_path))

from bridge.bridge_connector import bridge_connector
from bridge.bridge_manager import bridge_manager


async def test_ping():
    """Test 1: Basic connectivity"""
    print("\n[Test 1] Unity Ping")
    print("=" * 60)
    try:
        result = await bridge_manager.send_command("pingUnityEditor", {})
        print(f"[OK] Success: Connected to Unity")
        print(f"   Unity Version: {result.get('unityVersion', 'N/A')}")
        print(f"   Project: {result.get('projectName', 'N/A')}")
        return True
    except Exception as e:
        print(f"[FAIL] Failed: {e}")
        return False


async def test_context_inspect():
    """Test 2: Context inspection"""
    print("\n[Test 2] Context Inspection")
    print("=" * 60)
    try:
        result = await bridge_manager.send_command("contextInspect", {
            "includeHierarchy": True,
            "includeComponents": False,
            "maxDepth": 2
        })
        print(f"[OK] Success: Retrieved scene context")
        print(f"   Scene: {result.get('sceneName', 'N/A')}")
        print(f"   Hierarchy items: {len(result.get('hierarchy', []))}")

        # Show first few hierarchy items
        hierarchy = result.get('hierarchy', [])
        if hierarchy:
            print(f"   Top-level objects:")
            for item in hierarchy[:5]:
                print(f"     - {item.get('name', 'Unknown')}")
        return True
    except Exception as e:
        print(f"[FAIL] Failed: {e}")
        return False


async def test_scene_quicksetup():
    """Test 3: Scene quick setup"""
    print("\n[Test 3] Scene Quick Setup (UI)")
    print("=" * 60)
    try:
        result = await bridge_manager.send_command("sceneQuickSetup", {
            "setupType": "UI"
        })
        print(f"[OK] Success: UI scene setup completed")
        objects = result.get('objectsCreated', [])
        if objects:
            print(f"   Created objects:")
            for obj in objects:
                print(f"     - {obj}")
        return True
    except Exception as e:
        print(f"[FAIL] Failed: {e}")
        return False


async def test_gameobject_template():
    """Test 4: GameObject creation from template"""
    print("\n[Test 4] GameObject Template Creation")
    print("=" * 60)
    try:
        result = await bridge_manager.send_command("gameObjectCreateFromTemplate", {
            "template": "Cube",
            "name": "TestCube_SkillTest",
            "position": {"x": 0, "y": 1, "z": 0},
            "scale": {"x": 0.5, "y": 0.5, "z": 0.5}
        })
        print(f"[OK] Success: Created GameObject from template")
        print(f"   Created: {result.get('gameObjectPath', 'TestCube_SkillTest')}")
        return True
    except Exception as e:
        print(f"[FAIL] Failed: {e}")
        return False


async def test_ugui_template():
    """Test 5: UI template creation"""
    print("\n[Test 5] UI Template Creation (Button)")
    print("=" * 60)
    try:
        result = await bridge_manager.send_command("uguiCreateFromTemplate", {
            "template": "Button",
            "text": "Test Button",
            "width": 200,
            "height": 50,
            "anchorPreset": "middle-center"
        })
        print(f"[OK] Success: Created UI element from template")
        print(f"   Created: {result.get('gameObjectPath', 'Button')}")
        return True
    except Exception as e:
        print(f"[FAIL] Failed: {e}")
        return False


async def test_hierarchy_builder():
    """Test 6: Hierarchy builder"""
    print("\n[Test 6] Hierarchy Builder")
    print("=" * 60)
    try:
        result = await bridge_manager.send_command("hierarchyBuilder", {
            "hierarchy": {
                "TestParent_SkillTest": {
                    "components": [],
                    "children": {
                        "Child1": {
                            "components": []
                        },
                        "Child2": {
                            "components": []
                        }
                    }
                }
            }
        })
        print(f"[OK] Success: Built hierarchy")
        objects = result.get('objectsCreated', [])
        if objects:
            print(f"   Created objects:")
            for obj in objects:
                print(f"     - {obj}")
        return True
    except Exception as e:
        print(f"[FAIL] Failed: {e}")
        return False


async def test_component_management():
    """Test 7: Component management"""
    print("\n[Test 7] Component Management")
    print("=" * 60)
    try:
        # Add Rigidbody to TestCube
        result = await bridge_manager.send_command("componentManage", {
            "operation": "add",
            "gameObjectPath": "TestCube_SkillTest",
            "componentType": "UnityEngine.Rigidbody",
            "propertyChanges": {
                "mass": 2.0,
                "useGravity": True
            }
        })
        print(f"[OK] Success: Added component")
        print(f"   Component: {result.get('componentType', 'Rigidbody')}")
        print(f"   GameObject: {result.get('gameObjectPath', 'TestCube_SkillTest')}")
        return True
    except Exception as e:
        print(f"[FAIL] Failed: {e}")
        return False


async def test_gameobject_inspect():
    """Test 8: GameObject inspection"""
    print("\n[Test 8] GameObject Inspection")
    print("=" * 60)
    try:
        result = await bridge_manager.send_command("gameObjectManage", {
            "operation": "inspect",
            "gameObjectPath": "TestCube_SkillTest",
            "includeProperties": False
        })
        print(f"[OK] Success: Inspected GameObject")
        print(f"   GameObject: {result.get('gameObjectPath', 'TestCube_SkillTest')}")
        components = result.get('components', [])
        if components:
            print(f"   Components ({len(components)}):")
            for comp in components:
                comp_type = comp.get('type', 'Unknown') if isinstance(comp, dict) else comp
                print(f"     - {comp_type}")
        return True
    except Exception as e:
        print(f"[FAIL] Failed: {e}")
        return False


async def cleanup():
    """Cleanup test objects"""
    print("\n[Cleanup] Removing test objects")
    print("=" * 60)
    try:
        # Delete test cube
        await bridge_manager.send_command("gameObjectManage", {
            "operation": "delete",
            "gameObjectPath": "TestCube_SkillTest"
        })
        print("[OK] Deleted: TestCube_SkillTest")
    except Exception as e:
        print(f"[WARN] Could not delete TestCube_SkillTest - {e}")

    try:
        # Delete test hierarchy
        await bridge_manager.send_command("gameObjectManage", {
            "operation": "delete",
            "gameObjectPath": "TestParent_SkillTest"
        })
        print("[OK] Deleted: TestParent_SkillTest")
    except Exception as e:
        print(f"[WARN] Could not delete TestParent_SkillTest - {e}")


async def run_tests():
    """Run all tests"""
    print("=" * 60)
    print("      SkillForUnity Main Features Test Suite")
    print("=" * 60)
    print(f"\nConnecting to Unity Bridge at ws://127.0.0.1:7070/bridge")

    # Start bridge connector
    bridge_connector.start()

    # Wait for connection
    await asyncio.sleep(2)

    if not bridge_manager.is_connected():
        print("[FAIL] Could not connect to Unity Bridge")
        print("   Make sure:")
        print("   1. Unity Editor is open")
        print("   2. MCP Bridge is started (Tools > MCP Assistant)")
        return

    print("[OK] Connected to Unity Bridge\n")

    # Define tests
    tests = [
        ("Ping", test_ping),
        ("Context Inspection", test_context_inspect),
        ("Scene Quick Setup", test_scene_quicksetup),
        ("GameObject Template", test_gameobject_template),
        ("UI Template", test_ugui_template),
        ("Hierarchy Builder", test_hierarchy_builder),
        ("Component Management", test_component_management),
        ("GameObject Inspection", test_gameobject_inspect),
    ]

    results = {}
    for name, test_func in tests:
        try:
            results[name] = await test_func()
            await asyncio.sleep(0.5)  # Small delay between tests
        except Exception as e:
            print(f"[CRASH] Test '{name}' crashed: {e}")
            results[name] = False

    # Cleanup
    await cleanup()

    # Summary
    print("\n" + "=" * 60)
    print("TEST SUMMARY")
    print("=" * 60)

    passed = sum(1 for r in results.values() if r)
    total = len(results)
    success_rate = (passed / total * 100) if total > 0 else 0

    print(f"\nPassed: {passed}/{total} ({success_rate:.1f}%)")
    print(f"Failed: {total - passed}/{total}\n")

    print("Individual Test Results:")
    for name, result in results.items():
        status = "[PASS]" if result else "[FAIL]"
        print(f"  {status}  {name}")

    # Final verdict
    print("\n" + "=" * 60)
    if passed == total:
        print("ALL TESTS PASSED! SkillForUnity is working perfectly.")
    elif passed > total / 2:
        print("MOSTLY WORKING: Some tests failed, but core functionality works.")
    else:
        print("MAJOR ISSUES: Many tests failed. Please check Unity connection.")
    print("=" * 60)

    # Stop bridge connector
    await bridge_connector.stop()


if __name__ == "__main__":
    asyncio.run(run_tests())
