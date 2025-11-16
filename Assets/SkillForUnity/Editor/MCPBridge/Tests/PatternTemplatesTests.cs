using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace MCP.Editor.Tests
{
    /// <summary>
    /// Manual tests for PatternTemplates code generation.
    /// Run via: Tools > SkillForUnity > Test Pattern Generation
    /// </summary>
    public static class PatternTemplatesTests
    {
        private const string TestOutputPath = "Assets/Scripts/DesignPatterns/";

        [MenuItem("Tools/SkillForUnity/Test Pattern Generation")]
        public static void RunAllTests()
        {
            Debug.Log("=== Starting Pattern Generation Tests ===");

            // Ensure output directory exists
            if (!Directory.Exists(TestOutputPath))
            {
                Directory.CreateDirectory(TestOutputPath);
                Debug.Log($"Created test directory: {TestOutputPath}");
            }

            int totalTests = 0;
            int passedTests = 0;

            // Test 1: Singleton Pattern (MonoBehaviour)
            totalTests++;
            if (TestSingletonPattern())
            {
                passedTests++;
                Debug.Log("✅ Singleton Pattern Test PASSED");
            }
            else
            {
                Debug.LogError("❌ Singleton Pattern Test FAILED");
            }

            // Test 2: ObjectPool Pattern
            totalTests++;
            if (TestObjectPoolPattern())
            {
                passedTests++;
                Debug.Log("✅ ObjectPool Pattern Test PASSED");
            }
            else
            {
                Debug.LogError("❌ ObjectPool Pattern Test FAILED");
            }

            // Test 3: StateMachine Pattern
            totalTests++;
            if (TestStateMachinePattern())
            {
                passedTests++;
                Debug.Log("✅ StateMachine Pattern Test PASSED");
            }
            else
            {
                Debug.LogError("❌ StateMachine Pattern Test FAILED");
            }

            // Test 4: Observer Pattern
            totalTests++;
            if (TestObserverPattern())
            {
                passedTests++;
                Debug.Log("✅ Observer Pattern Test PASSED");
            }
            else
            {
                Debug.LogError("❌ Observer Pattern Test FAILED");
            }

            // Test 5: Command Pattern
            totalTests++;
            if (TestCommandPattern())
            {
                passedTests++;
                Debug.Log("✅ Command Pattern Test PASSED");
            }
            else
            {
                Debug.LogError("❌ Command Pattern Test FAILED");
            }

            // Test 6: Factory Pattern
            totalTests++;
            if (TestFactoryPattern())
            {
                passedTests++;
                Debug.Log("✅ Factory Pattern Test PASSED");
            }
            else
            {
                Debug.LogError("❌ Factory Pattern Test FAILED");
            }

            // Test 7: ServiceLocator Pattern
            totalTests++;
            if (TestServiceLocatorPattern())
            {
                passedTests++;
                Debug.Log("✅ ServiceLocator Pattern Test PASSED");
            }
            else
            {
                Debug.LogError("❌ ServiceLocator Pattern Test FAILED");
            }

            // Refresh AssetDatabase
            AssetDatabase.Refresh();

            // Summary
            Debug.Log($"=== Test Summary ===");
            Debug.Log($"Total Tests: {totalTests}");
            Debug.Log($"Passed: {passedTests}");
            Debug.Log($"Failed: {totalTests - passedTests}");

            if (passedTests == totalTests)
            {
                Debug.Log("🎉 All tests PASSED!");
            }
            else
            {
                Debug.LogWarning($"⚠️ {totalTests - passedTests} test(s) failed.");
            }
        }

        private static bool TestSingletonPattern()
        {
            try
            {
                var options = new Dictionary<string, object>
                {
                    ["persistent"] = true,
                    ["threadSafe"] = true,
                    ["monoBehaviour"] = true
                };

                string code = PatternTemplates.GeneratePattern(
                    "singleton",
                    "TestGameManager",
                    "MCP.Tests",
                    options
                );

                string filePath = TestOutputPath + "TestGameManager.cs";
                File.WriteAllText(filePath, code);

                Debug.Log($"Singleton pattern generated: {filePath} ({code.Length} chars)");
                return !string.IsNullOrEmpty(code) && code.Contains("TestGameManager") && code.Contains("Instance");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Singleton test failed: {ex.Message}");
                return false;
            }
        }

        private static bool TestObjectPoolPattern()
        {
            try
            {
                var options = new Dictionary<string, object>
                {
                    ["pooledType"] = "GameObject",
                    ["defaultCapacity"] = "50",
                    ["maxSize"] = "200"
                };

                string code = PatternTemplates.GeneratePattern(
                    "objectpool",
                    "TestBulletPool",
                    "MCP.Tests",
                    options
                );

                string filePath = TestOutputPath + "TestBulletPool.cs";
                File.WriteAllText(filePath, code);

                Debug.Log($"ObjectPool pattern generated: {filePath} ({code.Length} chars)");
                return !string.IsNullOrEmpty(code) && code.Contains("TestBulletPool") && code.Contains("ObjectPool");
            }
            catch (Exception ex)
            {
                Debug.LogError($"ObjectPool test failed: {ex.Message}");
                return false;
            }
        }

        private static bool TestStateMachinePattern()
        {
            try
            {
                string code = PatternTemplates.GeneratePattern(
                    "statemachine",
                    "TestPlayerStateMachine",
                    "MCP.Tests",
                    null
                );

                string filePath = TestOutputPath + "TestPlayerStateMachine.cs";
                File.WriteAllText(filePath, code);

                Debug.Log($"StateMachine pattern generated: {filePath} ({code.Length} chars)");
                return !string.IsNullOrEmpty(code) && code.Contains("TestPlayerStateMachine") && code.Contains("IState");
            }
            catch (Exception ex)
            {
                Debug.LogError($"StateMachine test failed: {ex.Message}");
                return false;
            }
        }

        private static bool TestObserverPattern()
        {
            try
            {
                string code = PatternTemplates.GeneratePattern(
                    "observer",
                    "TestEventManager",
                    "MCP.Tests",
                    null
                );

                string filePath = TestOutputPath + "TestEventManager.cs";
                File.WriteAllText(filePath, code);

                Debug.Log($"Observer pattern generated: {filePath} ({code.Length} chars)");
                return !string.IsNullOrEmpty(code) && code.Contains("TestEventManager") && code.Contains("Subscribe");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Observer test failed: {ex.Message}");
                return false;
            }
        }

        private static bool TestCommandPattern()
        {
            try
            {
                string code = PatternTemplates.GeneratePattern(
                    "command",
                    "TestCommandManager",
                    "MCP.Tests",
                    null
                );

                string filePath = TestOutputPath + "TestCommandManager.cs";
                File.WriteAllText(filePath, code);

                Debug.Log($"Command pattern generated: {filePath} ({code.Length} chars)");
                return !string.IsNullOrEmpty(code) && code.Contains("TestCommandManager") && code.Contains("ICommand");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Command test failed: {ex.Message}");
                return false;
            }
        }

        private static bool TestFactoryPattern()
        {
            try
            {
                var options = new Dictionary<string, object>
                {
                    ["productType"] = "GameObject"
                };

                string code = PatternTemplates.GeneratePattern(
                    "factory",
                    "TestEnemyFactory",
                    "MCP.Tests",
                    options
                );

                string filePath = TestOutputPath + "TestEnemyFactory.cs";
                File.WriteAllText(filePath, code);

                Debug.Log($"Factory pattern generated: {filePath} ({code.Length} chars)");
                return !string.IsNullOrEmpty(code) && code.Contains("TestEnemyFactory") && code.Contains("CreateProduct");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Factory test failed: {ex.Message}");
                return false;
            }
        }

        private static bool TestServiceLocatorPattern()
        {
            try
            {
                string code = PatternTemplates.GeneratePattern(
                    "servicelocator",
                    "TestServiceLocator",
                    "MCP.Tests",
                    null
                );

                string filePath = TestOutputPath + "TestServiceLocator.cs";
                File.WriteAllText(filePath, code);

                Debug.Log($"ServiceLocator pattern generated: {filePath} ({code.Length} chars)");
                return !string.IsNullOrEmpty(code) && code.Contains("TestServiceLocator") && code.Contains("GetService");
            }
            catch (Exception ex)
            {
                Debug.LogError($"ServiceLocator test failed: {ex.Message}");
                return false;
            }
        }

        [MenuItem("Tools/SkillForUnity/Clean Pattern Test Files")]
        public static void CleanTestFiles()
        {
            if (!Directory.Exists(TestOutputPath))
            {
                Debug.Log("No test files to clean.");
                return;
            }

            string[] testFiles = Directory.GetFiles(TestOutputPath, "Test*.cs");
            int deletedCount = 0;

            foreach (string file in testFiles)
            {
                File.Delete(file);
                string metaFile = file + ".meta";
                if (File.Exists(metaFile))
                {
                    File.Delete(metaFile);
                }
                deletedCount++;
                Debug.Log($"Deleted: {file}");
            }

            AssetDatabase.Refresh();
            Debug.Log($"Cleaned {deletedCount} test file(s).");
        }
    }
}
