using NUnit.Framework;
using System.Collections.Generic;
using MCP.Editor.Handlers;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MCP.Editor.Tests.Integration
{
    /// <summary>
    /// Integration tests for command handlers.
    /// Tests the actual execution of handlers in a Unity environment.
    /// </summary>
    [TestFixture]
    public class CommandHandlerIntegrationTests
    {
        private SceneCommandHandler _sceneHandler;
        private GameObjectCommandHandler _gameObjectHandler;
        private ComponentCommandHandler _componentHandler;
        private AssetCommandHandler _assetHandler;
        
        private string _testScenePath = "Assets/Tests/TestScene.unity";
        private string _testAssetPath = "Assets/Tests/TestAsset.txt";
        
        [SetUp]
        public void SetUp()
        {
            // Initialize handlers
            _sceneHandler = new SceneCommandHandler();
            _gameObjectHandler = new GameObjectCommandHandler();
            _componentHandler = new ComponentCommandHandler();
            _assetHandler = new AssetCommandHandler();
            
            // Ensure test directory exists
            if (!System.IO.Directory.Exists("Assets/Tests"))
            {
                System.IO.Directory.CreateDirectory("Assets/Tests");
                AssetDatabase.Refresh();
            }
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up test assets
            if (System.IO.File.Exists(_testScenePath))
            {
                AssetDatabase.DeleteAsset(_testScenePath);
            }
            if (System.IO.File.Exists(_testAssetPath))
            {
                AssetDatabase.DeleteAsset(_testAssetPath);
            }
            
            AssetDatabase.Refresh();
        }
        
        #region Scene Handler Tests
        
        [Test]
        public void SceneHandler_CreateAndLoadScene_Success()
        {
            // Arrange
            var createPayload = new Dictionary<string, object>
            {
                ["operation"] = "create",
                ["scenePath"] = _testScenePath
            };
            
            // Act
            var createResult = _sceneHandler.Execute(createPayload) as Dictionary<string, object>;
            
            // Assert
            Assert.IsNotNull(createResult);
            Assert.IsTrue((bool)createResult["success"]);
            Assert.IsTrue(System.IO.File.Exists(_testScenePath));
        }
        
        [Test]
        public void SceneHandler_InspectScene_ReturnsValidInfo()
        {
            // Arrange
            var inspectPayload = new Dictionary<string, object>
            {
                ["operation"] = "inspect",
                ["includeHierarchy"] = true
            };
            
            // Act
            var result = _sceneHandler.Execute(inspectPayload) as Dictionary<string, object>;
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue((bool)result["success"]);
            Assert.IsTrue(result.ContainsKey("sceneName"));
            Assert.IsTrue(result.ContainsKey("hierarchy"));
        }
        
        #endregion
        
        #region GameObject Handler Tests
        
        [Test]
        public void GameObjectHandler_CreateGameObject_Success()
        {
            // Arrange
            var payload = new Dictionary<string, object>
            {
                ["operation"] = "create",
                ["name"] = "TestObject"
            };
            
            // Act
            var result = _gameObjectHandler.Execute(payload) as Dictionary<string, object>;
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue((bool)result["success"]);
            Assert.IsTrue(result.ContainsKey("gameObjectPath"));
            
            // Verify GameObject exists
            var createdObject = GameObject.Find("TestObject");
            Assert.IsNotNull(createdObject);
            
            // Cleanup
            GameObject.DestroyImmediate(createdObject);
        }
        
        [Test]
        public void GameObjectHandler_InspectGameObject_ReturnsValidInfo()
        {
            // Arrange
            var testObject = new GameObject("TestInspect");
            testObject.tag = "Player";
            
            var payload = new Dictionary<string, object>
            {
                ["operation"] = "inspect",
                ["gameObjectPath"] = "TestInspect"
            };
            
            // Act
            var result = _gameObjectHandler.Execute(payload) as Dictionary<string, object>;
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue((bool)result["success"]);
            Assert.AreEqual("TestInspect", result["name"]);
            Assert.AreEqual("Player", result["tag"]);
            Assert.IsTrue(result.ContainsKey("transform"));
            Assert.IsTrue(result.ContainsKey("components"));
            
            // Cleanup
            GameObject.DestroyImmediate(testObject);
        }
        
        #endregion
        
        #region Component Handler Tests
        
        [Test]
        public void ComponentHandler_AddComponent_Success()
        {
            // Arrange
            var testObject = new GameObject("TestComponent");
            
            var payload = new Dictionary<string, object>
            {
                ["operation"] = "add",
                ["gameObjectPath"] = "TestComponent",
                ["componentType"] = "UnityEngine.Rigidbody"
            };
            
            // Act
            var result = _componentHandler.Execute(payload) as Dictionary<string, object>;
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue((bool)result["success"]);
            Assert.IsNotNull(testObject.GetComponent<Rigidbody>());
            
            // Cleanup
            GameObject.DestroyImmediate(testObject);
        }
        
        [Test]
        public void ComponentHandler_UpdateComponent_Success()
        {
            // Arrange
            var testObject = new GameObject("TestComponentUpdate");
            var rigidbody = testObject.AddComponent<Rigidbody>();
            
            var payload = new Dictionary<string, object>
            {
                ["operation"] = "update",
                ["gameObjectPath"] = "TestComponentUpdate",
                ["componentType"] = "UnityEngine.Rigidbody",
                ["propertyChanges"] = new Dictionary<string, object>
                {
                    ["mass"] = 10.0f,
                    ["useGravity"] = false
                }
            };
            
            // Act
            var result = _componentHandler.Execute(payload) as Dictionary<string, object>;
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue((bool)result["success"]);
            Assert.AreEqual(10.0f, rigidbody.mass, 0.01f);
            Assert.IsFalse(rigidbody.useGravity);
            
            // Cleanup
            GameObject.DestroyImmediate(testObject);
        }
        
        #endregion
        
        #region Asset Handler Tests
        
        [Test]
        public void AssetHandler_CreateAsset_Success()
        {
            // Arrange
            var payload = new Dictionary<string, object>
            {
                ["operation"] = "create",
                ["assetPath"] = _testAssetPath,
                ["content"] = "Test content"
            };
            
            // Act
            var result = _assetHandler.Execute(payload) as Dictionary<string, object>;
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue((bool)result["success"]);
            Assert.IsTrue(System.IO.File.Exists(_testAssetPath));
            
            var content = System.IO.File.ReadAllText(_testAssetPath);
            Assert.AreEqual("Test content", content);
        }
        
        [Test]
        public void AssetHandler_PreventCsFileCreation_ThrowsException()
        {
            // Arrange
            var payload = new Dictionary<string, object>
            {
                ["operation"] = "create",
                ["assetPath"] = "Assets/Tests/Test.cs",
                ["content"] = "// test"
            };
            
            // Act & Assert
            var result = _assetHandler.Execute(payload) as Dictionary<string, object>;
            Assert.IsNotNull(result);
            Assert.IsFalse((bool)result["success"]);
            Assert.IsTrue(result.ContainsKey("error"));
            Assert.IsTrue(result["error"].ToString().Contains("restricted"));
        }
        
        #endregion
        
        #region Cross-Handler Integration Tests
        
        [Test]
        public void CrossHandler_CreateGameObjectAddComponentAndInspect_Success()
        {
            // Step 1: Create GameObject
            var createPayload = new Dictionary<string, object>
            {
                ["operation"] = "create",
                ["name"] = "CrossTestObject"
            };
            
            var createResult = _gameObjectHandler.Execute(createPayload) as Dictionary<string, object>;
            Assert.IsTrue((bool)createResult["success"]);
            
            // Step 2: Add Component
            var addComponentPayload = new Dictionary<string, object>
            {
                ["operation"] = "add",
                ["gameObjectPath"] = "CrossTestObject",
                ["componentType"] = "UnityEngine.BoxCollider"
            };
            
            var addResult = _componentHandler.Execute(addComponentPayload) as Dictionary<string, object>;
            Assert.IsTrue((bool)addResult["success"]);
            
            // Step 3: Inspect GameObject
            var inspectPayload = new Dictionary<string, object>
            {
                ["operation"] = "inspect",
                ["gameObjectPath"] = "CrossTestObject"
            };
            
            var inspectResult = _gameObjectHandler.Execute(inspectPayload) as Dictionary<string, object>;
            Assert.IsTrue((bool)inspectResult["success"]);
            
            var components = inspectResult["components"] as List<object>;
            Assert.IsNotNull(components);
            Assert.IsTrue(components.Count > 0);
            
            // Verify BoxCollider is in the list
            bool hasBoxCollider = false;
            foreach (var comp in components)
            {
                var compDict = comp as Dictionary<string, object>;
                if (compDict != null && compDict["name"].ToString().Contains("BoxCollider"))
                {
                    hasBoxCollider = true;
                    break;
                }
            }
            Assert.IsTrue(hasBoxCollider);
            
            // Cleanup
            var testObject = GameObject.Find("CrossTestObject");
            if (testObject != null)
            {
                GameObject.DestroyImmediate(testObject);
            }
        }
        
        #endregion
        
        #region Performance Tests
        
        [Test]
        [Category("Performance")]
        public void PerformanceTest_BatchGameObjectCreation_UnderTwoSeconds()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var createdObjects = new List<GameObject>();
            
            // Act
            for (int i = 0; i < 100; i++)
            {
                var payload = new Dictionary<string, object>
                {
                    ["operation"] = "create",
                    ["name"] = $"PerfTest_{i}"
                };
                
                var result = _gameObjectHandler.Execute(payload) as Dictionary<string, object>;
                Assert.IsTrue((bool)result["success"]);
                
                var go = GameObject.Find($"PerfTest_{i}");
                if (go != null)
                {
                    createdObjects.Add(go);
                }
            }
            
            stopwatch.Stop();
            
            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 2000, 
                $"Batch creation of 100 GameObjects took {stopwatch.ElapsedMilliseconds}ms, expected < 2000ms");
            
            // Cleanup
            foreach (var go in createdObjects)
            {
                GameObject.DestroyImmediate(go);
            }
        }
        
        #endregion
    }
}

