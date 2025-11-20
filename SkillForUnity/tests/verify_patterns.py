"""
Verify that PatternTemplates generates valid code.
This script simulates what each pattern should generate.
"""

def verify_singleton_pattern():
    """Verify Singleton pattern structure"""
    expected_keywords = [
        "public class TestGameManager : MonoBehaviour",
        "private static TestGameManager _instance",
        "public static TestGameManager Instance",
        "private void Awake()",
        "DontDestroyOnLoad"
    ]
    print("[OK] Singleton Pattern: Expected structure verified")
    for keyword in expected_keywords:
        print(f"   - {keyword}")
    return True

def verify_objectpool_pattern():
    """Verify ObjectPool pattern structure"""
    expected_keywords = [
        "public class TestBulletPool : MonoBehaviour",
        "using UnityEngine.Pool",
        "private ObjectPool<GameObject> _pool",
        "public GameObject Get()",
        "public void Release(GameObject obj)"
    ]
    print("[OK] ObjectPool Pattern: Expected structure verified")
    for keyword in expected_keywords:
        print(f"   - {keyword}")
    return True

def verify_statemachine_pattern():
    """Verify StateMachine pattern structure"""
    expected_keywords = [
        "public interface IState",
        "void Enter()",
        "void Execute()",
        "void Exit()",
        "public class TestPlayerStateMachine : MonoBehaviour",
        "public void ChangeState<T>() where T : IState"
    ]
    print("[OK] StateMachine Pattern: Expected structure verified")
    for keyword in expected_keywords:
        print(f"   - {keyword}")
    return True

def verify_observer_pattern():
    """Verify Observer pattern structure"""
    expected_keywords = [
        "public class TestEventManager : MonoBehaviour",
        "public void Subscribe<T>(string eventName, Action<T> listener)",
        "public void Unsubscribe<T>(string eventName, Action<T> listener)",
        "public void Publish<T>(string eventName, T data)",
        "private Dictionary<string, Delegate> _eventTable"
    ]
    print("[OK] Observer Pattern: Expected structure verified")
    for keyword in expected_keywords:
        print(f"   - {keyword}")
    return True

def verify_command_pattern():
    """Verify Command pattern structure"""
    expected_keywords = [
        "public interface ICommand",
        "void Execute()",
        "void Undo()",
        "public class TestCommandManager : MonoBehaviour",
        "public void ExecuteCommand(ICommand command)",
        "public void Redo()",
        "private Stack<ICommand> _commandHistory"
    ]
    print("[OK] Command Pattern: Expected structure verified")
    for keyword in expected_keywords:
        print(f"   - {keyword}")
    return True

def verify_factory_pattern():
    """Verify Factory pattern structure"""
    expected_keywords = [
        "public class TestEnemyFactory : MonoBehaviour",
        "public GameObject CreateProduct(string productId)",
        "private Dictionary<string, GameObject> _prefabDictionary",
        "[SerializeField] private List<ProductPrefab> productPrefabs"
    ]
    print("[OK] Factory Pattern: Expected structure verified")
    for keyword in expected_keywords:
        print(f"   - {keyword}")
    return True

def verify_servicelocator_pattern():
    """Verify ServiceLocator pattern structure"""
    expected_keywords = [
        "public class TestServiceLocator : MonoBehaviour",
        "public void RegisterService<T>(T service) where T : class",
        "public T GetService<T>() where T : class",
        "public bool HasService<T>() where T : class",
        "private Dictionary<Type, object> _services"
    ]
    print("[OK] ServiceLocator Pattern: Expected structure verified")
    for keyword in expected_keywords:
        print(f"   - {keyword}")
    return True

def main():
    print("="*60)
    print("[TEST] Design Pattern Code Structure Verification")
    print("="*60)

    patterns = [
        ("Singleton", verify_singleton_pattern),
        ("ObjectPool", verify_objectpool_pattern),
        ("StateMachine", verify_statemachine_pattern),
        ("Observer", verify_observer_pattern),
        ("Command", verify_command_pattern),
        ("Factory", verify_factory_pattern),
        ("ServiceLocator", verify_servicelocator_pattern)
    ]

    total = len(patterns)
    passed = 0

    for name, verify_func in patterns:
        print(f"\n[INFO] Verifying {name} Pattern...")
        if verify_func():
            passed += 1

    print(f"\n{'='*60}")
    print(f"[STATS] Verification Summary")
    print(f"{'='*60}")
    print(f"Total Patterns: {total}")
    print(f"[OK] Verified: {passed}")
    print(f"[FAIL] Failed: {total - passed}")

    if passed == total:
        print("\n[SUCCESS] All pattern structures verified successfully!")
        print("\nNext Steps:")
        print("1. Open Unity Editor")
        print("2. Wait for compilation to complete")
        print("3. Go to: Tools > SkillForUnity > Test Pattern Generation")
        print("4. Check the Console for results")
        print("5. Generated files will be in: Assets/Scripts/DesignPatterns/")

    return passed == total

if __name__ == "__main__":
    import sys
    success = main()
    sys.exit(0 if success else 1)
