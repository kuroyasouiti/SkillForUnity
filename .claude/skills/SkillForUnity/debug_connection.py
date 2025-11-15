#!/usr/bin/env python
"""Debug Unity bridge WebSocket connection with detailed logging."""
import asyncio
import logging
import sys
from pathlib import Path

# Setup detailed logging
logging.basicConfig(
    level=logging.DEBUG,
    format="%(asctime)s.%(msecs)03d [%(levelname)s] %(name)s: %(message)s",
    datefmt="%H:%M:%S"
)

# Add src to path
src_path = Path(__file__).parent / "src"
sys.path.insert(0, str(src_path))

import websockets
from websockets.asyncio.client import connect


async def test_raw_websocket():
    """Test raw WebSocket connection to Unity bridge."""
    url = "ws://127.0.0.1:7070/bridge"

    print("=" * 70)
    print("Unity Bridge WebSocket Connection Test")
    print("=" * 70)
    print(f"\nAttempting connection to: {url}\n")

    try:
        print("Step 1: Opening WebSocket connection...")
        async with connect(
            url,
            open_timeout=10,
            close_timeout=10,
            max_size=10 * 1024 * 1024,
            compression=None,
            ping_interval=None,
            ping_timeout=None,
            logger=logging.getLogger("websockets.client"),
        ) as websocket:
            print("[OK] WebSocket connected!")
            print(f"   State: {websocket.state}")
            print(f"   Subprotocol: {websocket.subprotocol}")

            print("\nStep 2: Waiting for 'hello' message from Unity...")
            try:
                message = await asyncio.wait_for(websocket.recv(), timeout=5.0)
                print(f"[OK] Received message: {message[:200]}...")

                print("\nStep 3: Sending ping message...")
                import json
                import time
                ping_msg = json.dumps({
                    "type": "ping",
                    "timestamp": int(time.time() * 1000)
                })
                await websocket.send(ping_msg)
                print(f"[OK] Sent ping: {ping_msg}")

                print("\nStep 4: Waiting for response...")
                response = await asyncio.wait_for(websocket.recv(), timeout=5.0)
                print(f"[OK] Received response: {response[:200]}...")

                print("\n[SUCCESS] Connection test successful!")

            except asyncio.TimeoutError:
                print("[ERROR] Timeout waiting for message from Unity")
                print("   Unity bridge may not be sending 'hello' message")

            # Keep connection open for a bit
            print("\nKeeping connection open for 5 seconds...")
            await asyncio.sleep(5)

    except asyncio.TimeoutError:
        print("[ERROR] Connection timeout (10 seconds)")
        print("   Possible causes:")
        print("   - Unity Editor is not running")
        print("   - MCP Assistant bridge is not started")
        print("   - Firewall blocking localhost connections")

    except ConnectionRefusedError:
        print("[ERROR] Connection refused")
        print("   Unity bridge is not listening on port 7070")
        print("   Please start the bridge in Unity Editor (Tools > MCP Assistant)")

    except Exception as e:
        print(f"[ERROR] Connection failed: {type(e).__name__}: {e}")
        import traceback
        print("\nFull traceback:")
        traceback.print_exc()

    print("\n" + "=" * 70)
    print("Test completed")
    print("=" * 70)


if __name__ == "__main__":
    asyncio.run(test_raw_websocket())
