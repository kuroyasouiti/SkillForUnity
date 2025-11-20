import asyncio
import sys
import os
sys.path.insert(0, os.path.join(os.path.dirname(__file__), 'SkillForUnity', 'src'))

async def test_unity_connection():
    try:
        # Import bridge manager
        from bridge.bridge_manager import bridge_manager

        # Check if connected
        is_connected = bridge_manager.is_connected
        print(f"Bridge connected: {is_connected}")

        if not is_connected:
            print("\nPlease ensure:")
            print("1. Unity Editor is open (CONFIRMED)")
            print("2. Go to Tools > MCP Assistant")
            print("3. Click 'Start Bridge'")
            print("4. Wait for 'Connected' status")
            return False

        # Try to ping Unity
        from tools.unity_ping import ping
        result = await ping({})

        print("\n=== Unity Editor Connection Test ===")
        print(f"Status: SUCCESS")
        print(f"Unity Version: {result.get('unityVersion', 'Unknown')}")
        print(f"Unity Product: {result.get('unityProduct', 'Unknown')}")
        print(f"Bridge Version: {result.get('bridgeVersion', 'Unknown')}")
        print("=" * 35)

        return True

    except Exception as e:
        print(f"\n=== Connection Test Failed ===")
        print(f"Error: {e}")
        print("\nTroubleshooting:")
        print("1. Unity Editor is running: YES")
        print("2. MCP Bridge started? Check Tools > MCP Assistant")
        print("3. Bridge status shows 'Connected'?")
        return False

# Run the test
if __name__ == "__main__":
    asyncio.run(test_unity_connection())
