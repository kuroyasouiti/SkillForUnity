import asyncio
import sys
from pathlib import Path

# Add src to path
sys.path.insert(0, str(Path(__file__).parent / "src"))

from bridge.bridge_connector import bridge_connector
from bridge.bridge_manager import bridge_manager
from logger import logger

async def test():
    print("Starting bridge connector...")
    bridge_connector.start()

    print("Waiting for connection...")
    await asyncio.sleep(5)

    is_connected = bridge_manager.is_connected()
    session_id = bridge_manager.get_session_id()

    print(f"Connected: {is_connected}")
    print(f"Session ID: {session_id}")

    if is_connected:
        print("Testing ping...")
        try:
            await bridge_manager.send_ping()
            print("Ping successful!")
        except Exception as e:
            print(f"Ping failed: {e}")

    print("Stopping connector...")
    await bridge_connector.stop()
    print("Done!")

if __name__ == "__main__":
    asyncio.run(test())
