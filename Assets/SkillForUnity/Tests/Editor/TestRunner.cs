using UnityEditor;
using UnityEngine;
using UnityEditor.TestTools.TestRunner.Api;

namespace SkillForUnity.Tests.Editor
{
    /// <summary>
    /// Test runner utility for executing SkillForUnity tests from the Unity Editor menu.
    /// </summary>
    public static class TestRunner
    {
        [MenuItem("Tools/SkillForUnity/Run All Tests")]
        public static void RunAllTests()
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            
            var filter = new Filter
            {
                testMode = TestMode.EditMode
            };

            testRunnerApi.Execute(new ExecutionSettings(filter));
            
            Debug.Log("[TestRunner] Executing all EditMode tests...");
        }

        [MenuItem("Tools/SkillForUnity/Run Low-Level Tests")]
        public static void RunLowLevelTests()
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            
            var filter = new Filter
            {
                testMode = TestMode.EditMode,
                testNames = new[] { "SkillForUnity.Tests.Editor.LowLevelToolsTests" }
            };

            testRunnerApi.Execute(new ExecutionSettings(filter));
            
            Debug.Log("[TestRunner] Executing Low-Level Tools tests...");
        }

        [MenuItem("Tools/SkillForUnity/Run Mid-Level Tests")]
        public static void RunMidLevelTests()
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            
            var filter = new Filter
            {
                testMode = TestMode.EditMode,
                testNames = new[] { "SkillForUnity.Tests.Editor.MidLevelToolsTests" }
            };

            testRunnerApi.Execute(new ExecutionSettings(filter));
            
            Debug.Log("[TestRunner] Executing Mid-Level Tools tests...");
        }

        [MenuItem("Tools/SkillForUnity/Run GameKit Tests")]
        public static void RunGameKitTests()
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            
            var filter = new Filter
            {
                testMode = TestMode.EditMode,
                testNames = new[]
                {
                    "SkillForUnity.Tests.Editor.GameKitActorTests",
                    "SkillForUnity.Tests.Editor.GameKitManagerTests",
                    "SkillForUnity.Tests.Editor.GameKitInteractionTests",
                    "SkillForUnity.Tests.Editor.GameKitUICommandTests",
                    "SkillForUnity.Tests.Editor.GameKitSceneFlowTests"
                }
            };

            testRunnerApi.Execute(new ExecutionSettings(filter));
            
            Debug.Log("[TestRunner] Executing GameKit tests...");
        }

        [MenuItem("Tools/SkillForUnity/Open Test Runner Window")]
        public static void OpenTestRunnerWindow()
        {
            EditorWindow.GetWindow<UnityEditor.TestTools.TestRunner.TestRunnerWindow>();
        }
    }
}

