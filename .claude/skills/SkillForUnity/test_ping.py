"""Test Unity Editor connection using SkillForUnity ping tool."""
import asyncio
import sys
sys.path.insert(0, 'src')

async def main():
    try:
        from bridge.bridge_connector import bridge_connector
        from bridge.bridge_manager import bridge_manager
        from utils.json_utils import as_pretty_json

        print("Starting Unity Bridge connector...")
        bridge_connector.start()

        # Wait for connection (max 10 seconds)
        for i in range(20):
            if bridge_manager.is_connected():
                break
            await asyncio.sleep(0.5)
        else:
            print("\nFailed to connect to Unity Editor within 10 seconds")
            print("\nPlease ensure:")
            print("1. Unity Editor is open (CONFIRMED RUNNING)")
            print("2. Go to Tools > MCP Assistant")
            print("3. Click 'Start Bridge'")
            print("4. Status shows 'Connected'")
            print(f"\nChecking port 7070: Unity Editor is LISTENING on this port")
            await bridge_connector.stop()
            return

        # Ping Unity
        print("\nTesting Unity Editor connection...")
        heartbeat = bridge_manager.get_last_heartbeat()
        bridge_response = await bridge_manager.send_command("pingUnityEditor", {})

        # Display results
        print("\n" + "="*50)
        print("Unity Editor Connection Test - SUCCESS")
        print("="*50)
        print(f"Connection: Active")
        print(f"Session ID: {bridge_manager.get_session_id()}")
        print(f"Last Heartbeat: {heartbeat}")
        print("\nBridge Response:")
        print(as_pretty_json(bridge_response))
        print("="*50)

        # Clean up
        await bridge_connector.stop()

    except Exception as e:
        print(f"\nConnection test failed: {e}")
        import traceback
        traceback.print_exc()
        print("\nMake sure:")
        print("- Unity Editor is open")
        print("- MCP Bridge is started (Tools > MCP Assistant > Start Bridge)")

if __name__ == "__main__":
    asyncio.run(main())
