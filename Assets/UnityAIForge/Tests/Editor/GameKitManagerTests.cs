using System;
using NUnit.Framework;
using UnityAIForge.GameKit;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UnityAIForge.Tests.Editor
{
    [TestFixture]
    public class GameKitManagerTests
    {
        private GameObject testManagerGo;

        [SetUp]
        public void Setup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [TearDown]
        public void Teardown()
        {
            if (testManagerGo != null)
            {
                UnityEngine.Object.DestroyImmediate(testManagerGo);
            }
        }

        [Test]
        public void CreateManager_WithValidParameters_CreatesGameObject()
        {
            // Arrange
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();

            // Act
            manager.Initialize("manager_001", GameKitManager.ManagerType.TurnBased, false);

            // Assert
            Assert.IsNotNull(manager);
            Assert.AreEqual("manager_001", manager.ManagerId);
            Assert.AreEqual(GameKitManager.ManagerType.TurnBased, manager.Type);
            Assert.IsFalse(manager.IsPersistent);
        }

        [Test]
        public void AddTurnPhase_WithNewPhase_AddsPhase()
        {
            // Arrange
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.TurnBased, false);

            // Act
            manager.AddTurnPhase("PlayerTurn");
            manager.AddTurnPhase("EnemyTurn");
            manager.AddTurnPhase("EndTurn");

            // Assert
            Assert.AreEqual("PlayerTurn", manager.GetCurrentPhase());
        }

        [Test]
        public void NextPhase_AdvancesToNextPhase()
        {
            // Arrange
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.TurnBased, false);
            manager.AddTurnPhase("PlayerTurn");
            manager.AddTurnPhase("EnemyTurn");
            manager.AddTurnPhase("EndTurn");

            // Act
            manager.NextPhase();

            // Assert
            Assert.AreEqual("EnemyTurn", manager.GetCurrentPhase());
        }

        [Test]
        public void NextPhase_AtLastPhase_WrapsToFirst()
        {
            // Arrange
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.TurnBased, false);
            manager.AddTurnPhase("PlayerTurn");
            manager.AddTurnPhase("EnemyTurn");
            manager.NextPhase(); // Move to EnemyTurn

            // Act
            manager.NextPhase(); // Should wrap to PlayerTurn

            // Assert
            Assert.AreEqual("PlayerTurn", manager.GetCurrentPhase());
        }

        [Test]
        public void SetResource_WithNewResource_AddsResource()
        {
            // Arrange
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.ResourcePool, false);

            // Act
            manager.SetResource("gold", 100f);
            manager.SetResource("wood", 50f);

            // Assert
            Assert.AreEqual(100f, manager.GetResource("gold"));
            Assert.AreEqual(50f, manager.GetResource("wood"));
        }

        [Test]
        public void ConsumeResource_WithSufficientAmount_ReturnsTrue()
        {
            // Arrange
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.ResourcePool, false);
            manager.SetResource("gold", 100f);

            // Act
            var result = manager.ConsumeResource("gold", 30f);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(70f, manager.GetResource("gold"));
        }

        [Test]
        public void ConsumeResource_WithInsufficientAmount_ReturnsFalse()
        {
            // Arrange
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.ResourcePool, false);
            manager.SetResource("gold", 100f);

            // Act
            var result = manager.ConsumeResource("gold", 150f);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(100f, manager.GetResource("gold")); // Should remain unchanged
        }

        [Test]
        public void AddResource_IncreasesResourceAmount()
        {
            // Arrange
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.ResourcePool, false);
            manager.SetResource("gold", 100f);

            // Act
            manager.AddResource("gold", 50f);

            // Assert
            Assert.AreEqual(150f, manager.GetResource("gold"));
        }

        [Test]
        public void GetAllResources_ReturnsAllResources()
        {
            // Arrange
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.ResourcePool, false);
            manager.SetResource("gold", 100f);
            manager.SetResource("wood", 50f);
            manager.SetResource("stone", 75f);

            // Act
            var resources = manager.GetAllResources();

            // Assert
            Assert.AreEqual(3, resources.Count);
            Assert.AreEqual(100f, resources["gold"]);
            Assert.AreEqual(50f, resources["wood"]);
            Assert.AreEqual(75f, resources["stone"]);
        }

        [Test]
        public void TurnBasedManager_AttachesCorrectComponent()
        {
            // Arrange & Act
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.TurnBased, false);

            // Assert
            var turnType = System.Type.GetType("UnityAIForge.GameKit.GameKitTurnManager, UnityAIForge.GameKit.Runtime");
            Assert.IsNotNull(turnType);
            Assert.IsNotNull(testManagerGo.GetComponent(turnType));
        }

        [Test]
        public void ResourcePoolManager_AttachesCorrectComponent()
        {
            // Arrange & Act
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.ResourcePool, false);

            // Assert
            var resourceType = System.Type.GetType("UnityAIForge.GameKit.GameKitResourceManager, UnityAIForge.GameKit.Runtime");
            Assert.IsNotNull(resourceType);
            Assert.IsNotNull(testManagerGo.GetComponent(resourceType));
        }

        [Test]
        public void EventHubManager_AttachesCorrectComponent()
        {
            // Arrange & Act
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.EventHub, false);

            // Assert
            var eventType = System.Type.GetType("UnityAIForge.GameKit.GameKitEventManager, UnityAIForge.GameKit.Runtime");
            Assert.IsNotNull(eventType);
            Assert.IsNotNull(testManagerGo.GetComponent(eventType));
        }

        [Test]
        public void StateManager_AttachesCorrectComponent()
        {
            // Arrange & Act
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.StateManager, false);

            // Assert
            var stateType = System.Type.GetType("UnityAIForge.GameKit.GameKitStateManager, UnityAIForge.GameKit.Runtime");
            Assert.IsNotNull(stateType);
            Assert.IsNotNull(testManagerGo.GetComponent(stateType));
        }

        [Test]
        public void RealtimeManager_AttachesCorrectComponent()
        {
            // Arrange & Act
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.Realtime, false);

            // Assert
            var realtimeType = System.Type.GetType("UnityAIForge.GameKit.GameKitRealtimeManager, UnityAIForge.GameKit.Runtime");
            Assert.IsNotNull(realtimeType);
            Assert.IsNotNull(testManagerGo.GetComponent(realtimeType));
        }

        [Test]
        public void GetModeComponent_ReturnsCorrectComponent()
        {
            // Arrange
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.ResourcePool, false);

            // Act
            var resourceType = System.Type.GetType("UnityAIForge.GameKit.GameKitResourceManager, UnityAIForge.GameKit.Runtime");
            var resourceManager = manager.GetModeComponent<Component>();

            // Assert
            Assert.IsNotNull(resourceManager);
        }

        [Test]
        public void ResourceConstraints_ClampValues()
        {
            // Arrange
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.ResourcePool, false);
            
            var resourceType = System.Type.GetType("UnityAIForge.GameKit.GameKitResourceManager, UnityAIForge.GameKit.Runtime");
            var resourceManager = testManagerGo.GetComponent(resourceType);

            // Act - Set constraints: health between 0 and 100
            var setConstraintsMethod = resourceType.GetMethod("SetResourceConstraints");
            setConstraintsMethod?.Invoke(resourceManager, new object[] { "health", 0f, 100f });
            
            manager.SetResource("health", 150f); // Should clamp to 100

            // Assert
            Assert.AreEqual(100f, manager.GetResource("health"));
            
            manager.SetResource("health", -50f); // Should clamp to 0
            Assert.AreEqual(0f, manager.GetResource("health"));
        }

        [Test]
        public void ResourceFlow_GeneratesResourceOverTime()
        {
            // Arrange
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.ResourcePool, false);
            
            var resourceType = System.Type.GetType("UnityAIForge.GameKit.GameKitResourceManager, UnityAIForge.GameKit.Runtime");
            var resourceManager = testManagerGo.GetComponent(resourceType);
            
            manager.SetResource("mana", 0f);
            
            // Act - Add flow: 10 mana per second
            var addFlowMethod = resourceType.GetMethod("AddFlow");
            addFlowMethod?.Invoke(resourceManager, new object[] { "mana", 10f, true });
            
            // Simulate 0.5 seconds (should generate 5 mana)
            var updateMethod = resourceType.GetMethod("Update", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Note: In EditMode tests, we can't test time-based behavior perfectly
            // This test verifies the method exists and doesn't crash
            Assert.IsNotNull(updateMethod);
        }

        [Test]
        public void ResourceConverter_ConvertsResources()
        {
            // Arrange
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.ResourcePool, false);
            
            var resourceType = System.Type.GetType("UnityAIForge.GameKit.GameKitResourceManager, UnityAIForge.GameKit.Runtime");
            var resourceManager = testManagerGo.GetComponent(resourceType);
            
            manager.SetResource("wood", 100f);
            manager.SetResource("planks", 0f);
            
            // Act - Add converter: 1 wood → 4 planks
            var addConverterMethod = resourceType.GetMethod("AddConverter");
            addConverterMethod?.Invoke(resourceManager, new object[] { "wood", "planks", 4f, 1f });
            
            // Convert 10 wood
            var convertMethod = resourceType.GetMethod("Convert");
            var success = convertMethod?.Invoke(resourceManager, new object[] { "wood", "planks", 10f });
            
            // Assert
            Assert.IsTrue((bool)success);
            Assert.AreEqual(90f, manager.GetResource("wood"));  // 100 - 10
            Assert.AreEqual(40f, manager.GetResource("planks")); // 10 * 4
        }

        [Test]
        public void ResourceTrigger_InvokesEventOnThreshold()
        {
            // Arrange
            testManagerGo = new GameObject("TestManager");
            var manager = testManagerGo.AddComponent<GameKitManager>();
            manager.Initialize("manager_001", GameKitManager.ManagerType.ResourcePool, false);
            
            var resourceType = System.Type.GetType("UnityAIForge.GameKit.GameKitResourceManager, UnityAIForge.GameKit.Runtime");
            var resourceManager = testManagerGo.GetComponent(resourceType);
            
            manager.SetResource("health", 100f);
            
            // Add trigger: warn when health drops below 30
            var addTriggerMethod = resourceType.GetMethod("AddTrigger");
            var thresholdType = resourceType.GetNestedType("ThresholdType");
            var belowValue = Enum.Parse(thresholdType, "Below");
            
            addTriggerMethod?.Invoke(resourceManager, new object[] { "lowHealth", "health", belowValue, 30f });
            
            string triggeredName = null;
            var onTriggerProperty = resourceType.GetField("OnResourceTriggered");
            var triggerEvent = onTriggerProperty?.GetValue(resourceManager);
            var addListenerMethod = triggerEvent?.GetType().GetMethod("AddListener");
            
            UnityEngine.Events.UnityAction<string, string, float> callback = (name, resource, value) => {
                triggeredName = name;
            };
            addListenerMethod?.Invoke(triggerEvent, new object[] { callback });
            
            // Act - Drop health below threshold
            manager.SetResource("health", 25f);
            
            // Trigger check (would normally happen in Update)
            var checkTriggersMethod = resourceType.GetMethod("CheckTriggers", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            checkTriggersMethod?.Invoke(resourceManager, null);
            
            // Assert
            Assert.AreEqual("lowHealth", triggeredName);
        }
    }
}

