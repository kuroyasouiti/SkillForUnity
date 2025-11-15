#!/usr/bin/env python3
"""Simple test for GameObject Inspection"""

import asyncio
import json
import sys
from pathlib import Path

# Add src directory to path
src_path = Path(__file__).parent / "src"
sys.path.insert(0, str(src_path))

from bridge.bridge_connector import bridge_connector
from bridge.bridge_manager import bridge_manager


async def test_simple():
    """Simple test"""
    print("Simple GameObject Inspection Test")
    print("=" * 60)

    # Start bridge
    bridge_connector.start()
    await asyncio.sleep(2)

    if not bridge_manager.is_connected():
        print("[FAIL] Not connected")
        return

    print("[OK] Connected\n")

    # Test on existing GameObject (Camera should exist)
    print("Testing on Main Camera...")
    try:
        result = await bridge_manager.send_command("gameObjectManage", {
            "operation": "inspect",
            "gameObjectPath": "Main Camera",
            "includeChildren": False
        })
        print("Result:")
        print(json.dumps(result, indent=2))
        print("\n[OK] Test completed")
    except Exception as e:
        print(f"[FAIL] Error: {e}")

    await bridge_connector.stop()


if __name__ == "__main__":
    asyncio.run(test_simple())
