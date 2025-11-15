#!/usr/bin/env python3
"""GameObject Inspection detailed test"""

import asyncio
import json
import sys
from pathlib import Path

# Add src directory to path
src_path = Path(__file__).parent / "src"
sys.path.insert(0, str(src_path))

from bridge.bridge_connector import bridge_connector
from bridge.bridge_manager import bridge_manager


async def test_inspection_modes():
    """Test different inspection modes"""
    print("=" * 60)
    print("GameObject Inspection Test - Various Modes")
    print("=" * 60)

    # Start bridge
    bridge_connector.start()
    await asyncio.sleep(2)

    if not bridge_manager.is_connected():
        print("[FAIL] Could not connect to Unity Bridge")
        return

    print("[OK] Connected to Unity Bridge\n")

    # First, create a test object with multiple components
    print("Creating test GameObject with multiple components...")
    try:
        await bridge_manager.send_command("gameObjectCreateFromTemplate", {
            "template": "Cube",
            "name": "InspectionTestCube",
            "position": {"x": 0, "y": 1, "z": 0}
        })
        print("[OK] Created InspectionTestCube\n")
    except Exception as e:
        print(f"[FAIL] Could not create test object: {e}")
        await bridge_connector.stop()
        return

    # Add Rigidbody component
    try:
        await bridge_manager.send_command("componentManage", {
            "operation": "add",
            "gameObjectPath": "InspectionTestCube",
            "componentType": "UnityEngine.Rigidbody",
            "propertyChanges": {
                "mass": 2.5,
                "useGravity": True
            }
        })
        print("[OK] Added Rigidbody component\n")
    except Exception as e:
        print(f"[WARN] Could not add Rigidbody: {e}\n")

    # Test 1: Full inspection
    print("\n" + "=" * 60)
    print("Test 1: Full Inspection (all components + all properties)")
    print("=" * 60)
    try:
        result = await bridge_manager.send_command("gameObjectManage", {
            "operation": "inspect",
            "gameObjectPath": "InspectionTestCube"
        })
        print(json.dumps(result, indent=2))
    except Exception as e:
        print(f"[FAIL] {e}")

    # Test 2: Light inspection (no properties)
    print("\n" + "=" * 60)
    print("Test 2: Light Inspection (component types only)")
    print("=" * 60)
    try:
        result = await bridge_manager.send_command("gameObjectManage", {
            "operation": "inspect",
            "gameObjectPath": "InspectionTestCube",
            "includeProperties": False
        })
        print(json.dumps(result, indent=2))
    except Exception as e:
        print(f"[FAIL] {e}")

    # Test 3: Filtered inspection (specific components)
    print("\n" + "=" * 60)
    print("Test 3: Filtered Inspection (Transform and Rigidbody only)")
    print("=" * 60)
    try:
        result = await bridge_manager.send_command("gameObjectManage", {
            "operation": "inspect",
            "gameObjectPath": "InspectionTestCube",
            "componentFilter": ["UnityEngine.Transform", "UnityEngine.Rigidbody"]
        })
        print(json.dumps(result, indent=2))
    except Exception as e:
        print(f"[FAIL] {e}")

    # Test 4: Inspect a UI object (if Canvas exists)
    print("\n" + "=" * 60)
    print("Test 4: Inspect UI Object (if exists)")
    print("=" * 60)
    try:
        # Try to find Canvas
        result = await bridge_manager.send_command("gameObjectManage", {
            "operation": "inspect",
            "gameObjectPath": "Canvas",
            "includeProperties": False
        })
        print("Found Canvas with components:")
        print(json.dumps(result, indent=2))
    except Exception as e:
        print(f"[INFO] No Canvas found or error: {e}")

    # Cleanup
    print("\n" + "=" * 60)
    print("Cleanup")
    print("=" * 60)
    try:
        await bridge_manager.send_command("gameObjectManage", {
            "operation": "delete",
            "gameObjectPath": "InspectionTestCube"
        })
        print("[OK] Deleted InspectionTestCube")
    except Exception as e:
        print(f"[WARN] Could not delete test object: {e}")

    await bridge_connector.stop()
    print("\n[DONE] Inspection test completed")


if __name__ == "__main__":
    asyncio.run(test_inspection_modes())
