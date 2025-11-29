using NUnit.Framework;
using SkillForUnity.GameKit;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SkillForUnity.Tests.Editor
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
                Object.DestroyImmediate(testManagerGo);
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
    }
}

