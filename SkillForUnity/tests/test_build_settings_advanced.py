#!/usr/bin/env python3
"""Advanced test script for Build Settings - Add/Remove operations"""

import asyncio
import sys
from pathlib import Path

# Add src directory to path
src_path = Path(__file__).parent / "src"
sys.path.insert(0, str(src_path))

from bridge.bridge_connector import bridge_connector
from bridge.bridge_manager import bridge_manager


async def run_advanced_tests():
    """Run advanced build settings tests"""
    print("Starting Advanced Build Settings Tests...")
    print(f"Connecting to Unity Bridge at ws://127.0.0.1:7070/bridge")

    # Start bridge connector
    bridge_connector.start()

    # Wait for connection
    await asyncio.sleep(2)

    if not bridge_manager.is_connected():
        print("[FAIL] Failed to connect to Unity Bridge")
        return

    print("[OK] Connected to Unity Bridge\n")

    # Find available scenes
    print("=== Finding available scenes ===")
    try:
        asset_result = await bridge_manager.send_command("assetManage", {
            "operation": "findMultiple",
            "pattern": "Assets/**/*.unity"
        })
        all_scenes = asset_result.get('assets', [])
        print(f"[OK] Found {len(all_scenes)} scene files in project")
        for scene in all_scenes[:5]:  # Show first 5
            print(f"  - {scene.get('path')}")
    except Exception as e:
        print(f"[FAIL] Could not find scenes: {e}")
        await bridge_connector.stop()
        return

    # Get current build settings
    print("\n=== Current build settings ===")
    try:
        list_result = await bridge_manager.send_command("sceneManage", {
            "operation": "listBuildSettings"
        })
        current_scenes = list_result.get('scenes', [])
        print(f"[OK] Currently {len(current_scenes)} scenes in build")
        for scene in current_scenes:
            status = "ENABLED" if scene.get('enabled') else "DISABLED"
            print(f"  [{status}] {scene.get('index')}: {scene.get('path')}")
    except Exception as e:
        print(f"[FAIL] Could not list build settings: {e}")
        await bridge_connector.stop()
        return

    # Find a scene NOT in build settings
    current_paths = [s.get('path') for s in current_scenes]
    test_scene = None
    for scene in all_scenes:
        if scene.get('path') not in current_paths:
            test_scene = scene.get('path')
            break

    if not test_scene:
        print("\n[INFO] All scenes are already in build settings")
        if len(all_scenes) > 0:
            # Create a test scene
            print("\n=== Creating test scene ===")
            try:
                create_result = await bridge_manager.send_command("sceneManage", {
                    "operation": "create",
                    "scenePath": "Assets/TestBuildScene.unity"
                })
                test_scene = create_result.get('path')
                print(f"[OK] Created test scene: {test_scene}")
            except Exception as e:
                print(f"[FAIL] Could not create test scene: {e}")
                await bridge_connector.stop()
                return
        else:
            print("[FAIL] No scenes available for testing")
            await bridge_connector.stop()
            return

    print(f"\n[INFO] Using test scene: {test_scene}")

    # Test 1: Add scene to build settings
    print("\n=== Test 1: Add scene to build settings ===")
    try:
        add_result = await bridge_manager.send_command("sceneManage", {
            "operation": "addToBuildSettings",
            "scenePath": test_scene,
            "enabled": True
        })
        print(f"[OK] Added to build settings")
        print(f"  Path: {add_result.get('path')}")
        print(f"  Index: {add_result.get('index')}")
        print(f"  Enabled: {add_result.get('enabled')}")
        print(f"  Total scenes: {add_result.get('totalScenes')}")
    except Exception as e:
        print(f"[FAIL] Add to build settings failed: {e}")

    await asyncio.sleep(0.5)

    # Test 2: Try to add the same scene again (should fail)
    print("\n=== Test 2: Try to add duplicate (should fail) ===")
    try:
        await bridge_manager.send_command("sceneManage", {
            "operation": "addToBuildSettings",
            "scenePath": test_scene,
            "enabled": True
        })
        print("[FAIL] Should have failed but didn't!")
    except Exception as e:
        print(f"[OK] Correctly rejected duplicate: {e}")

    await asyncio.sleep(0.5)

    # Test 3: Verify it was added
    print("\n=== Test 3: Verify scene in build settings ===")
    try:
        list_result = await bridge_manager.send_command("sceneManage", {
            "operation": "listBuildSettings"
        })
        scenes = list_result.get('scenes', [])
        found = False
        for scene in scenes:
            if scene.get('path') == test_scene:
                found = True
                print(f"[OK] Scene found in build at index {scene.get('index')}")
                print(f"  Enabled: {scene.get('enabled')}")
                print(f"  GUID: {scene.get('guid')}")
                break

        if not found:
            print(f"[FAIL] Scene not found in build settings!")
    except Exception as e:
        print(f"[FAIL] Could not verify: {e}")

    await asyncio.sleep(0.5)

    # Test 4: Add another scene at specific index
    if len(all_scenes) > 1:
        second_scene = None
        for scene in all_scenes:
            scene_path = scene.get('path')
            if scene_path != test_scene and scene_path not in current_paths:
                second_scene = scene_path
                break

        if second_scene:
            print(f"\n=== Test 4: Add scene at index 0 ===")
            try:
                add_result = await bridge_manager.send_command("sceneManage", {
                    "operation": "addToBuildSettings",
                    "scenePath": second_scene,
                    "enabled": True,
                    "index": 0
                })
                print(f"[OK] Added at index 0")
                print(f"  Path: {add_result.get('path')}")
                print(f"  Index: {add_result.get('index')}")

                # Clean up - remove the second scene
                await asyncio.sleep(0.5)
                await bridge_manager.send_command("sceneManage", {
                    "operation": "removeFromBuildSettings",
                    "scenePath": second_scene
                })
                print(f"[OK] Cleaned up second test scene")
            except Exception as e:
                print(f"[FAIL] Add at index failed: {e}")

    await asyncio.sleep(0.5)

    # Test 5: Remove scene by path
    print("\n=== Test 5: Remove scene from build settings ===")
    try:
        remove_result = await bridge_manager.send_command("sceneManage", {
            "operation": "removeFromBuildSettings",
            "scenePath": test_scene
        })
        print(f"[OK] Removed from build settings")
        print(f"  Removed: {remove_result.get('removed')}")
        print(f"  Total scenes remaining: {remove_result.get('totalScenes')}")
    except Exception as e:
        print(f"[FAIL] Remove from build settings failed: {e}")

    await asyncio.sleep(0.5)

    # Test 6: Verify removal
    print("\n=== Test 6: Verify scene removed ===")
    try:
        list_result = await bridge_manager.send_command("sceneManage", {
            "operation": "listBuildSettings"
        })
        scenes = list_result.get('scenes', [])
        found = False
        for scene in scenes:
            if scene.get('path') == test_scene:
                found = True
                break

        if found:
            print(f"[FAIL] Scene still in build settings!")
        else:
            print(f"[OK] Scene successfully removed from build")
    except Exception as e:
        print(f"[FAIL] Could not verify: {e}")

    # Clean up test scene if we created it
    if test_scene == "Assets/TestBuildScene.unity":
        print("\n=== Cleaning up test scene ===")
        try:
            await bridge_manager.send_command("assetManage", {
                "operation": "delete",
                "assetPath": test_scene
            })
            print(f"[OK] Deleted test scene file")
        except Exception as e:
            print(f"[FAIL] Could not delete test scene: {e}")

    print("\n" + "=" * 60)
    print("ADVANCED BUILD SETTINGS TESTS COMPLETED")
    print("=" * 60)

    # Stop bridge connector
    await bridge_connector.stop()


if __name__ == "__main__":
    asyncio.run(run_advanced_tests())
