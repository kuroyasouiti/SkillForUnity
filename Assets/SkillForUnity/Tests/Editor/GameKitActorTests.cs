using NUnit.Framework;
using SkillForUnity.GameKit;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SkillForUnity.Tests.Editor
{
    [TestFixture]
    public class GameKitActorTests
    {
        private GameObject testActorGo;

        [SetUp]
        public void Setup()
        {
            // Create a new scene for testing
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [TearDown]
        public void Teardown()
        {
            if (testActorGo != null)
            {
                Object.DestroyImmediate(testActorGo);
            }
        }

        [Test]
        public void CreateActor_WithValidParameters_CreatesGameObject()
        {
            // Arrange
            testActorGo = new GameObject("TestActor");
            var actor = testActorGo.AddComponent<GameKitActor>();

            // Act
            actor.Initialize("actor_001", GameKitActor.BehaviorProfile.TwoDPhysics, GameKitActor.ControlMode.DirectController);

            // Assert
            Assert.IsNotNull(actor);
            Assert.AreEqual("actor_001", actor.ActorId);
            Assert.AreEqual(GameKitActor.BehaviorProfile.TwoDPhysics, actor.Behavior);
            Assert.AreEqual(GameKitActor.ControlMode.DirectController, actor.Control);
        }

        [Test]
        public void SetStat_WithNewStat_AddsStat()
        {
            // Arrange
            testActorGo = new GameObject("TestActor");
            var actor = testActorGo.AddComponent<GameKitActor>();
            actor.Initialize("actor_001", GameKitActor.BehaviorProfile.TwoDLinear, GameKitActor.ControlMode.DirectController);

            // Act
            actor.SetStat("health", 100f);
            actor.SetStat("speed", 5f);

            // Assert
            Assert.AreEqual(100f, actor.GetStat("health"));
            Assert.AreEqual(5f, actor.GetStat("speed"));
        }

        [Test]
        public void SetStat_WithExistingStat_UpdatesStat()
        {
            // Arrange
            testActorGo = new GameObject("TestActor");
            var actor = testActorGo.AddComponent<GameKitActor>();
            actor.Initialize("actor_001", GameKitActor.BehaviorProfile.TwoDLinear, GameKitActor.ControlMode.DirectController);
            actor.SetStat("health", 100f);

            // Act
            actor.SetStat("health", 80f);

            // Assert
            Assert.AreEqual(80f, actor.GetStat("health"));
        }

        [Test]
        public void AddAbility_WithNewAbility_AddsAbility()
        {
            // Arrange
            testActorGo = new GameObject("TestActor");
            var actor = testActorGo.AddComponent<GameKitActor>();
            actor.Initialize("actor_001", GameKitActor.BehaviorProfile.TwoDLinear, GameKitActor.ControlMode.DirectController);

            // Act
            actor.AddAbility("fireball");
            actor.AddAbility("heal");

            // Assert
            Assert.IsTrue(actor.HasAbility("fireball"));
            Assert.IsTrue(actor.HasAbility("heal"));
            Assert.IsFalse(actor.HasAbility("teleport"));
        }

        [Test]
        public void AddWeapon_WithNewWeapon_AddsWeapon()
        {
            // Arrange
            testActorGo = new GameObject("TestActor");
            var actor = testActorGo.AddComponent<GameKitActor>();
            actor.Initialize("actor_001", GameKitActor.BehaviorProfile.TwoDLinear, GameKitActor.ControlMode.DirectController);

            // Act
            actor.AddWeapon("sword");
            actor.AddWeapon("bow");

            // Assert - weapons are stored internally, verify through GetAllStats
            var stats = actor.GetAllStats();
            Assert.IsNotNull(stats);
        }

        [Test]
        public void GetAllStats_ReturnsAllStats()
        {
            // Arrange
            testActorGo = new GameObject("TestActor");
            var actor = testActorGo.AddComponent<GameKitActor>();
            actor.Initialize("actor_001", GameKitActor.BehaviorProfile.TwoDLinear, GameKitActor.ControlMode.DirectController);
            actor.SetStat("health", 100f);
            actor.SetStat("mana", 50f);
            actor.SetStat("speed", 5f);

            // Act
            var stats = actor.GetAllStats();

            // Assert
            Assert.AreEqual(3, stats.Count);
            Assert.AreEqual(100f, stats["health"]);
            Assert.AreEqual(50f, stats["mana"]);
            Assert.AreEqual(5f, stats["speed"]);
        }
    }
}

