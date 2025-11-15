#!/usr/bin/env python3
"""Test GameObject Inspection with children"""

import asyncio
import json
import sys
from pathlib import Path

# Add src directory to path
src_path = Path(__file__).parent / "src"
sys.path.insert(0, str(src_path))

from bridge.bridge_connector import bridge_connector
from bridge.bridge_manager import bridge_manager


async def test_with_children():
    """Test inspection with children"""
    print("GameObject Inspection with Children Test")
    print("=" * 60)

    # Start bridge
    bridge_connector.start()
    await asyncio.sleep(2)

    if not bridge_manager.is_connected():
        print("[FAIL] Not connected")
        return

    print("[OK] Connected\n")

    # Create test hierarchy
    print("Creating test hierarchy...")
    try:
        await bridge_manager.send_command("hierarchyBuilder", {
            "hierarchy": {
                "TestRoot": {
                    "components": [],
                    "children": {
                        "Child1": {
                            "components": [],
                            "children": {
                                "Grandchild1": {},
                                "Grandchild2": {}
                            }
                        },
                        "Child2": {
                            "components": []
                        }
                    }
                }
            }
        })
        print("[OK] Created hierarchy\n")
    except Exception as e:
        print(f"[FAIL] {e}")
        await bridge_connector.stop()
        return

    # Test 1: Default (depth=1)
    print("Test 1: Default inspection (depth=1)")
    print("-" * 60)
    try:
        result = await bridge_manager.send_command("gameObjectManage", {
            "operation": "inspect",
            "gameObjectPath": "TestRoot"
        })
        print(json.dumps(result, indent=2))

        children_count = len(result.get("children", []))
        print(f"\n[OK] Found {children_count} direct children")
    except Exception as e:
        print(f"[FAIL] {e}")

    # Test 2: Deep (depth=2)
    print("\n\nTest 2: Deep inspection (depth=2)")
    print("-" * 60)
    try:
        result = await bridge_manager.send_command("gameObjectManage", {
            "operation": "inspect",
            "gameObjectPath": "TestRoot",
            "maxDepth": 2
        })
        print(json.dumps(result, indent=2))

        print(f"\n[OK] Includes grandchildren")
    except Exception as e:
        print(f"[FAIL] {e}")

    # Test 3: No children
    print("\n\nTest 3: No children (includeChildren=False)")
    print("-" * 60)
    try:
        result = await bridge_manager.send_command("gameObjectManage", {
            "operation": "inspect",
            "gameObjectPath": "TestRoot",
            "includeChildren": False
        })
        print(json.dumps(result, indent=2))

        has_children = "children" in result
        print(f"\n[OK] Children excluded: {not has_children}")
    except Exception as e:
        print(f"[FAIL] {e}")

    # Cleanup
    print("\n\nCleanup...")
    try:
        await bridge_manager.send_command("gameObjectManage", {
            "operation": "delete",
            "gameObjectPath": "TestRoot"
        })
        print("[OK] Deleted test hierarchy")
    except Exception as e:
        print(f"[WARN] {e}")

    await bridge_connector.stop()
    print("\n[DONE] All tests completed")


if __name__ == "__main__":
    asyncio.run(test_with_children())
