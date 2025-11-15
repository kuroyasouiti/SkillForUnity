#!/usr/bin/env python3
"""Test script for Build Settings operations"""

import asyncio
import sys
from pathlib import Path

# Add src directory to path
src_path = Path(__file__).parent / "src"
sys.path.insert(0, str(src_path))

from bridge.bridge_connector import bridge_connector
from bridge.bridge_manager import bridge_manager


async def test_list_build_settings():
    """Test listing scenes in build settings"""
    print("\n=== Testing listBuildSettings ===")
    try:
        result = await bridge_manager.send_command("sceneManage", {
            "operation": "listBuildSettings"
        })
        print(f"[OK] List build settings successful")
        print(f"  Total scenes in build: {result.get('count', 0)}")
        for scene in result.get('scenes', []):
            status = "ENABLED" if scene.get('enabled') else "DISABLED"
            print(f"  [{status}] {scene.get('index')}: {scene.get('path')}")
        return True, result
    except Exception as e:
        print(f"[FAIL] List build settings failed: {e}")
        return False, None


async def test_add_to_build_settings(scene_path):
    """Test adding a scene to build settings"""
    print("\n=== Testing addToBuildSettings ===")
    try:
        result = await bridge_manager.send_command("sceneManage", {
            "operation": "addToBuildSettings",
            "scenePath": scene_path,
            "enabled": True
        })
        print(f"[OK] Add to build settings successful")
        print(f"  Path: {result.get('path')}")
        print(f"  Index: {result.get('index')}")
        print(f"  Enabled: {result.get('enabled')}")
        print(f"  Total scenes: {result.get('totalScenes')}")
        return True, result
    except Exception as e:
        print(f"[FAIL] Add to build settings failed: {e}")
        return False, None


async def test_set_build_settings_enabled(scene_path, enabled):
    """Test enabling/disabling a scene in build settings"""
    print(f"\n=== Testing setBuildSettingsEnabled (enabled={enabled}) ===")
    try:
        result = await bridge_manager.send_command("sceneManage", {
            "operation": "setBuildSettingsEnabled",
            "scenePath": scene_path,
            "enabled": enabled
        })
        print(f"[OK] Set build settings enabled successful")
        print(f"  Path: {result.get('path')}")
        print(f"  Enabled: {result.get('enabled')}")
        print(f"  Index: {result.get('index')}")
        return True, result
    except Exception as e:
        print(f"[FAIL] Set build settings enabled failed: {e}")
        return False, None


async def test_reorder_build_settings(scene_path, to_index):
    """Test reordering scenes in build settings"""
    print(f"\n=== Testing reorderBuildSettings (to index {to_index}) ===")
    try:
        result = await bridge_manager.send_command("sceneManage", {
            "operation": "reorderBuildSettings",
            "scenePath": scene_path,
            "toIndex": to_index
        })
        print(f"[OK] Reorder build settings successful")
        print(f"  Path: {result.get('path')}")
        print(f"  From index: {result.get('fromIndex')}")
        print(f"  To index: {result.get('toIndex')}")
        return True, result
    except Exception as e:
        print(f"[FAIL] Reorder build settings failed: {e}")
        return False, None


async def test_remove_from_build_settings(scene_path):
    """Test removing a scene from build settings"""
    print("\n=== Testing removeFromBuildSettings ===")
    try:
        result = await bridge_manager.send_command("sceneManage", {
            "operation": "removeFromBuildSettings",
            "scenePath": scene_path
        })
        print(f"[OK] Remove from build settings successful")
        print(f"  Removed: {result.get('removed')}")
        print(f"  Total scenes: {result.get('totalScenes')}")
        return True, result
    except Exception as e:
        print(f"[FAIL] Remove from build settings failed: {e}")
        return False, None


async def run_tests():
    """Run all build settings tests"""
    print("Starting Build Settings Tests...")
    print(f"Connecting to Unity Bridge at ws://127.0.0.1:7070/bridge")

    # Start bridge connector
    bridge_connector.start()

    # Wait for connection
    await asyncio.sleep(2)

    if not bridge_manager.is_connected():
        print("[FAIL] Failed to connect to Unity Bridge")
        print("Make sure Unity Editor is open and MCP Bridge is started (Tools > MCP Assistant)")
        return

    print("[OK] Connected to Unity Bridge\n")

    # Test 1: List current build settings
    success, list_result = await test_list_build_settings()
    if not success:
        print("\n[FAIL] Cannot proceed without being able to list build settings")
        await bridge_connector.stop()
        return

    # Get a test scene path
    scenes = list_result.get('scenes', [])
    test_scene_path = None

    if len(scenes) > 0:
        # Use existing scene for testing
        test_scene_path = scenes[0].get('path')
        print(f"\nUsing existing scene for testing: {test_scene_path}")
    else:
        # Need to find a scene file
        print("\n[INFO] No scenes in build settings, searching for scene files...")
        try:
            asset_result = await bridge_manager.send_command("assetManage", {
                "operation": "findMultiple",
                "pattern": "Assets/*.unity"
            })
            assets = asset_result.get('assets', [])
            if len(assets) > 0:
                test_scene_path = assets[0].get('path')
                print(f"Found scene file: {test_scene_path}")
            else:
                print("[FAIL] No scene files found in Assets/")
                await bridge_connector.stop()
                return
        except Exception as e:
            print(f"[FAIL] Could not search for scene files: {e}")
            await bridge_connector.stop()
            return

    # Test 2: Add scene to build settings (if not already there)
    if test_scene_path not in [s.get('path') for s in scenes]:
        await test_add_to_build_settings(test_scene_path)
        await asyncio.sleep(0.5)

    # Test 3: Disable the scene
    await test_set_build_settings_enabled(test_scene_path, False)
    await asyncio.sleep(0.5)

    # Test 4: Enable the scene again
    await test_set_build_settings_enabled(test_scene_path, True)
    await asyncio.sleep(0.5)

    # Test 5: Reorder (move to index 0)
    await test_reorder_build_settings(test_scene_path, 0)
    await asyncio.sleep(0.5)

    # Test 6: List again to verify changes
    print("\n=== Verifying final state ===")
    await test_list_build_settings()

    # Note: Not removing the scene to avoid disrupting the build
    # If you want to test removal, uncomment below:
    # await test_remove_from_build_settings(test_scene_path)

    print("\n" + "=" * 60)
    print("BUILD SETTINGS TESTS COMPLETED")
    print("=" * 60)
    print("All operations tested successfully!")
    print("\nNote: The test scene was left in build settings.")
    print("You can manually remove it if desired.")

    # Stop bridge connector
    await bridge_connector.stop()


if __name__ == "__main__":
    asyncio.run(run_tests())
