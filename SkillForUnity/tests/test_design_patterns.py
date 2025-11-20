"""Test script for design pattern generation"""
import asyncio
import sys
import os

# Add the skill directory to path
skill_dir = r"D:\Projects\SkillForUnity\.claude\skills\SkillForUnity\src"
sys.path.insert(0, skill_dir)

from bridge.bridge_manager import bridge_manager

async def test_design_patterns():
    """Test all design pattern generations"""

    print("🔌 Connecting to Unity bridge...")

    # Wait for connection
    max_wait = 10
    for i in range(max_wait):
        if bridge_manager.is_connected():
            print("✅ Connected to Unity bridge!")
            break
        await asyncio.sleep(1)
        print(f"⏳ Waiting for connection... ({i+1}/{max_wait})")

    if not bridge_manager.is_connected():
        print("❌ Failed to connect to Unity bridge")
        print("Please ensure:")
        print("1. Unity Editor is open")
        print("2. Tools > MCP Assistant > Start Bridge is clicked")
        return False

    # Test patterns
    patterns = [
        {
            "name": "Singleton",
            "patternType": "singleton",
            "className": "TestGameManager",
            "scriptPath": "Assets/Scripts/DesignPatterns/TestGameManager.cs",
            "options": {"persistent": True, "threadSafe": True, "monoBehaviour": True}
        },
        {
            "name": "ObjectPool",
            "patternType": "objectpool",
            "className": "TestBulletPool",
            "scriptPath": "Assets/Scripts/DesignPatterns/TestBulletPool.cs",
            "options": {"pooledType": "GameObject", "defaultCapacity": "50", "maxSize": "200"}
        },
        {
            "name": "StateMachine",
            "patternType": "statemachine",
            "className": "TestPlayerStateMachine",
            "scriptPath": "Assets/Scripts/DesignPatterns/TestPlayerStateMachine.cs",
            "options": {}
        },
        {
            "name": "Observer",
            "patternType": "observer",
            "className": "TestEventManager",
            "scriptPath": "Assets/Scripts/DesignPatterns/TestEventManager.cs",
            "options": {}
        },
        {
            "name": "Command",
            "patternType": "command",
            "className": "TestCommandManager",
            "scriptPath": "Assets/Scripts/DesignPatterns/TestCommandManager.cs",
            "options": {}
        },
        {
            "name": "Factory",
            "patternType": "factory",
            "className": "TestEnemyFactory",
            "scriptPath": "Assets/Scripts/DesignPatterns/TestEnemyFactory.cs",
            "options": {"productType": "GameObject"}
        },
        {
            "name": "ServiceLocator",
            "patternType": "servicelocator",
            "className": "TestServiceLocator",
            "scriptPath": "Assets/Scripts/DesignPatterns/TestServiceLocator.cs",
            "options": {}
        }
    ]

    success_count = 0
    failed_patterns = []

    for pattern in patterns:
        print(f"\n{'='*60}")
        print(f"🧪 Testing {pattern['name']} Pattern...")
        print(f"{'='*60}")

        try:
            # Send command to Unity
            result = await bridge_manager.send_command(
                "designPatternGenerate",
                {
                    "patternType": pattern["patternType"],
                    "className": pattern["className"],
                    "scriptPath": pattern["scriptPath"],
                    "options": pattern["options"]
                },
                timeout_ms=30000
            )

            print(f"✅ {pattern['name']} pattern generated successfully!")
            print(f"   Script path: {pattern['scriptPath']}")
            print(f"   Class name: {pattern['className']}")

            if isinstance(result, dict):
                if result.get("success"):
                    print(f"   Message: {result.get('message', 'N/A')}")
                    success_count += 1
                else:
                    print(f"   ⚠️ Warning: {result}")
                    failed_patterns.append(pattern['name'])
            else:
                print(f"   Result: {result}")
                success_count += 1

        except Exception as e:
            print(f"❌ Failed to generate {pattern['name']} pattern")
            print(f"   Error: {str(e)}")
            failed_patterns.append(pattern['name'])

    # Summary
    print(f"\n{'='*60}")
    print(f"📊 Test Summary")
    print(f"{'='*60}")
    print(f"Total patterns tested: {len(patterns)}")
    print(f"✅ Successful: {success_count}")
    print(f"❌ Failed: {len(failed_patterns)}")

    if failed_patterns:
        print(f"\nFailed patterns: {', '.join(failed_patterns)}")

    # Check if files were created
    print(f"\n{'='*60}")
    print(f"📁 Verifying Generated Files")
    print(f"{'='*60}")

    for pattern in patterns:
        file_path = pattern["scriptPath"].replace("Assets/", r"D:\Projects\SkillForUnity\Assets\\")
        if os.path.exists(file_path):
            file_size = os.path.getsize(file_path)
            print(f"✅ {pattern['name']}: {file_path} ({file_size} bytes)")
        else:
            print(f"❌ {pattern['name']}: File not found - {file_path}")

    return success_count == len(patterns)

if __name__ == "__main__":
    # Run the test
    success = asyncio.run(test_design_patterns())
    sys.exit(0 if success else 1)
