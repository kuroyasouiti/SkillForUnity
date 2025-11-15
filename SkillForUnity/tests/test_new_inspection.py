#!/usr/bin/env python3
"""Test new GameObject Inspection specification"""

import asyncio
import json
import sys
from pathlib import Path

# Add src directory to path
src_path = Path(__file__).parent / "src"
sys.path.insert(0, str(src_path))

from bridge.bridge_connector import bridge_connector
from bridge.bridge_manager import bridge_manager


async def test_new_inspection():
    """Test the new GameObject inspection with children"""
    print("=" * 70)
    print("GameObject Inspection - New Specification Test")
    print("=" * 70)

    # Start bridge
    bridge_connector.start()
    await asyncio.sleep(2)

    if not bridge_manager.is_connected():
        print("[FAIL] Could not connect to Unity Bridge")
        print("Make sure Unity Editor is open and MCP Bridge is started")
        return

    print("[OK] Connected to Unity Bridge\n")

    # Create test hierarchy
    print("Setting up test hierarchy...")
    try:
        await bridge_manager.send_command("hierarchyBuilder", {
            "hierarchy": {
                "TestParent": {
                    "components": [],
                    "children": {
                        "Child1": {
                            "components": [],
                            "children": {
                                "Grandchild1": {
                                    "components": []
                                },
                                "Grandchild2": {
                                    "components": []
                                }
                            }
                        },
                        "Child2": {
                            "components": [],
                            "children": {
                                "Grandchild3": {
                                    "components": []
                                }
                            }
                        },
                        "Child3": {
                            "components": []
                        }
                    }
                }
            }
        })
        print("[OK] Created test hierarchy\n")
    except Exception as e:
        print(f"[FAIL] Could not create test hierarchy: {e}")
        await bridge_connector.stop()
        return

    # Test 1: Default inspection (direct children only, maxDepth=1)
    print("\n" + "=" * 70)
    print("Test 1: Default Inspection (direct children, depth=1)")
    print("=" * 70)
    try:
        result = await bridge_manager.send_command("gameObjectManage", {
            "operation": "inspect",
            "gameObjectPath": "TestParent"
        })
        print(json.dumps(result, indent=2))

        # Verify structure
        assert "gameObjectPath" in result, "Missing gameObjectPath"
        assert "componentTypes" in result, "Missing componentTypes (should be component type names only)"
        assert "children" in result, "Missing children array"
        assert "childCount" in result, "Missing childCount"
        print("\n[OK] Default inspection returns expected structure")

        # Check children
        if result.get("children"):
            print(f"[OK] Found {len(result['children'])} direct children")
            for child in result["children"]:
                print(f"     - {child.get('name', 'Unknown')}")
                # Grandchildren should NOT be present at depth 1
                if "children" in child:
                    print(f"       [WARN] Unexpected: child has nested children at depth 1")

    except Exception as e:
        print(f"[FAIL] {e}")

    # Test 2: Deep inspection (maxDepth=2, includes grandchildren)
    print("\n" + "=" * 70)
    print("Test 2: Deep Inspection (depth=2, includes grandchildren)")
    print("=" * 70)
    try:
        result = await bridge_manager.send_command("gameObjectManage", {
            "operation": "inspect",
            "gameObjectPath": "TestParent",
            "maxDepth": 2
        })
        print(json.dumps(result, indent=2))

        # Check grandchildren
        if result.get("children"):
            for child in result["children"]:
                if child.get("children"):
                    print(f"\n[OK] Child '{child.get('name')}' has grandchildren:")
                    for grandchild in child["children"]:
                        print(f"     - {grandchild.get('name', 'Unknown')}")

    except Exception as e:
        print(f"[FAIL] {e}")

    # Test 3: No children inspection (includeChildren=False)
    print("\n" + "=" * 70)
    print("Test 3: No Children Inspection (includeChildren=False)")
    print("=" * 70)
    try:
        result = await bridge_manager.send_command("gameObjectManage", {
            "operation": "inspect",
            "gameObjectPath": "TestParent",
            "includeChildren": False
        })
        print(json.dumps(result, indent=2))

        # Verify no children
        if "children" not in result or not result.get("children"):
            print("\n[OK] No children included (as expected)")
        else:
            print("\n[WARN] Children were included despite includeChildren=False")

    except Exception as e:
        print(f"[FAIL] {e}")

    # Test 4: Component types are included (not full component details)
    print("\n" + "=" * 70)
    print("Test 4: Component Types (should be type names only)")
    print("=" * 70)
    try:
        # Add some components to TestParent
        await bridge_manager.send_command("componentManage", {
            "operation": "add",
            "gameObjectPath": "TestParent",
            "componentType": "UnityEngine.BoxCollider"
        })

        result = await bridge_manager.send_command("gameObjectManage", {
            "operation": "inspect",
            "gameObjectPath": "TestParent",
            "includeChildren": False
        })

        component_types = result.get("componentTypes", [])
        print(f"Component Types: {component_types}")

        # Verify they are strings (type names), not dictionaries (full component data)
        if component_types:
            first_type = component_types[0]
            if isinstance(first_type, str):
                print(f"\n[OK] Component types are strings (type names only)")
                print(f"     Example: {first_type}")
            else:
                print(f"\n[WARN] Component types are not strings: {type(first_type)}")

    except Exception as e:
        print(f"[FAIL] {e}")

    # Test 5: Verify component details are NOT included
    print("\n" + "=" * 70)
    print("Test 5: Verify No Component Properties")
    print("=" * 70)
    try:
        result = await bridge_manager.send_command("gameObjectManage", {
            "operation": "inspect",
            "gameObjectPath": "TestParent",
            "includeChildren": False
        })

        # Check that components don't have properties
        if "componentTypes" in result:
            types = result["componentTypes"]
            if isinstance(types, list) and all(isinstance(t, str) for t in types):
                print("[OK] Components are type names only (no properties)")
                print("     Use unity_component_crud for component details")
            else:
                print("[WARN] Components have unexpected structure")

    except Exception as e:
        print(f"[FAIL] {e}")

    # Cleanup
    print("\n" + "=" * 70)
    print("Cleanup")
    print("=" * 70)
    try:
        await bridge_manager.send_command("gameObjectManage", {
            "operation": "delete",
            "gameObjectPath": "TestParent"
        })
        print("[OK] Deleted test hierarchy")
    except Exception as e:
        print(f"[WARN] Could not delete test hierarchy: {e}")

    await bridge_connector.stop()

    print("\n" + "=" * 70)
    print("Summary of New Specification:")
    print("=" * 70)
    print("✓ GameObject inspection returns children (not components)")
    print("✓ Component type names are included (not full details)")
    print("✓ Use includeChildren=False to exclude children")
    print("✓ Use maxDepth to control hierarchy depth (default: 1)")
    print("✓ Use unity_component_crud for component property details")
    print("=" * 70)


if __name__ == "__main__":
    asyncio.run(test_new_inspection())
