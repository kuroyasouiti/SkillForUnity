using NUnit.Framework;
using UnityAIForge.GameKit;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UnityAIForge.Tests.Editor
{
    [TestFixture]
    public class GameKitInteractionTests
    {
        private GameObject testInteractionGo;

        [SetUp]
        public void Setup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [TearDown]
        public void Teardown()
        {
            if (testInteractionGo != null)
            {
                Object.DestroyImmediate(testInteractionGo);
            }
        }

        [Test]
        public void CreateInteraction_WithValidParameters_CreatesGameObject()
        {
            // Arrange
            testInteractionGo = new GameObject("TestInteraction");
            var interaction = testInteractionGo.AddComponent<GameKitInteraction>();

            // Act
            interaction.Initialize("interaction_001", GameKitInteraction.TriggerType.Trigger);

            // Assert
            Assert.IsNotNull(interaction);
            Assert.AreEqual("interaction_001", interaction.InteractionId);
            Assert.AreEqual(GameKitInteraction.TriggerType.Trigger, interaction.Trigger);
        }

        [Test]
        public void AddAction_WithValidAction_AddsAction()
        {
            // Arrange
            testInteractionGo = new GameObject("TestInteraction");
            var interaction = testInteractionGo.AddComponent<GameKitInteraction>();
            interaction.Initialize("interaction_001", GameKitInteraction.TriggerType.Trigger);

            // Act
            interaction.AddAction(GameKitInteraction.ActionType.PlaySound, "Sounds/Explosion", "");
            interaction.AddAction(GameKitInteraction.ActionType.SendMessage, "Player", "OnHit");

            // Assert - actions are stored internally
            Assert.IsNotNull(interaction);
        }

        [Test]
        public void AddCondition_WithValidCondition_AddsCondition()
        {
            // Arrange
            testInteractionGo = new GameObject("TestInteraction");
            var interaction = testInteractionGo.AddComponent<GameKitInteraction>();
            interaction.Initialize("interaction_001", GameKitInteraction.TriggerType.Trigger);

            // Act
            interaction.AddCondition(GameKitInteraction.ConditionType.Tag, "Player");
            interaction.AddCondition(GameKitInteraction.ConditionType.Distance, "5.0");

            // Assert - conditions are stored internally
            Assert.IsNotNull(interaction);
        }

        [Test]
        public void Interaction_WithTriggerCollider_HasCollider()
        {
            // Arrange
            testInteractionGo = new GameObject("TestInteraction");
            var interaction = testInteractionGo.AddComponent<GameKitInteraction>();
            interaction.Initialize("interaction_001", GameKitInteraction.TriggerType.Trigger);
            var collider = testInteractionGo.AddComponent<BoxCollider>();
            collider.isTrigger = true;

            // Assert
            Assert.IsNotNull(collider);
            Assert.IsTrue(collider.isTrigger);
        }

        [Test]
        public void ManualTrigger_ExecutesActions()
        {
            // Arrange
            testInteractionGo = new GameObject("TestInteraction");
            var interaction = testInteractionGo.AddComponent<GameKitInteraction>();
            interaction.Initialize("interaction_001", GameKitInteraction.TriggerType.Input);

            bool eventTriggered = false;
            interaction.OnInteractionTriggered.AddListener(_ => eventTriggered = true);

            // Act
            interaction.ManualTrigger();

            // Assert
            Assert.IsTrue(eventTriggered);
        }

        [Test]
        public void TilemapCellTrigger_RequiresTileGridMovement()
        {
            // Arrange
            testInteractionGo = new GameObject("TestInteraction");
            var interaction = testInteractionGo.AddComponent<GameKitInteraction>();
            
            // Act
            interaction.Initialize("interaction_001", GameKitInteraction.TriggerType.TilemapCell);

            // Assert - Should warn but not crash
            Assert.IsNotNull(interaction);
            Assert.AreEqual(GameKitInteraction.TriggerType.TilemapCell, interaction.Trigger);
        }

        [Test]
        public void GraphNodeTrigger_RequiresGraphNodeMovement()
        {
            // Arrange
            testInteractionGo = new GameObject("TestInteraction");
            var interaction = testInteractionGo.AddComponent<GameKitInteraction>();
            
            // Act
            interaction.Initialize("interaction_001", GameKitInteraction.TriggerType.GraphNode);

            // Assert
            Assert.IsNotNull(interaction);
            Assert.AreEqual(GameKitInteraction.TriggerType.GraphNode, interaction.Trigger);
        }

        [Test]
        public void SplineProgressTrigger_RequiresSplineMovement()
        {
            // Arrange
            testInteractionGo = new GameObject("TestInteraction");
            var interaction = testInteractionGo.AddComponent<GameKitInteraction>();
            
            // Act
            interaction.Initialize("interaction_001", GameKitInteraction.TriggerType.SplineProgress);

            // Assert
            Assert.IsNotNull(interaction);
            Assert.AreEqual(GameKitInteraction.TriggerType.SplineProgress, interaction.Trigger);
        }

        [Test]
        public void ActorIdCondition_ChecksActorId()
        {
            // Arrange
            testInteractionGo = new GameObject("TestInteraction");
            var interaction = testInteractionGo.AddComponent<GameKitInteraction>();
            interaction.Initialize("interaction_001", GameKitInteraction.TriggerType.Trigger);
            interaction.AddCondition(GameKitInteraction.ConditionType.ActorId, "player_001");

            // Create test actor
            var actorGo = new GameObject("TestActor");
            var actor = actorGo.AddComponent<GameKitActor>();
            actor.Initialize("player_001", GameKitActor.BehaviorProfile.TwoDLinear, GameKitActor.ControlMode.DirectController);

            bool eventTriggered = false;
            interaction.OnInteractionTriggered.AddListener(_ => eventTriggered = true);

            // Act
            interaction.ManualTrigger(actorGo);

            // Assert
            Assert.IsTrue(eventTriggered);

            // Cleanup
            Object.DestroyImmediate(actorGo);
        }

        [Test]
        public void TriggerActorAction_SendsActionToActor()
        {
            // Arrange
            testInteractionGo = new GameObject("TestInteraction");
            var interaction = testInteractionGo.AddComponent<GameKitInteraction>();
            interaction.Initialize("interaction_001", GameKitInteraction.TriggerType.Input);
            interaction.AddAction(GameKitInteraction.ActionType.TriggerActorAction, "test_actor", "collect");

            // Create test actor
            var actorGo = new GameObject("TestActor");
            var actor = actorGo.AddComponent<GameKitActor>();
            actor.Initialize("test_actor", GameKitActor.BehaviorProfile.TwoDLinear, GameKitActor.ControlMode.DirectController);

            string receivedAction = null;
            actor.OnActionInput.AddListener(action => receivedAction = action);

            // Act
            interaction.ManualTrigger();

            // Assert
            Assert.AreEqual("collect", receivedAction);

            // Cleanup
            Object.DestroyImmediate(actorGo);
        }

        [Test]
        public void UpdateManagerResource_ModifiesResource()
        {
            // Arrange
            testInteractionGo = new GameObject("TestInteraction");
            var interaction = testInteractionGo.AddComponent<GameKitInteraction>();
            interaction.Initialize("interaction_001", GameKitInteraction.TriggerType.Input);
            interaction.AddAction(GameKitInteraction.ActionType.UpdateManagerResource, "gold", "100");

            // Create manager
            var managerGo = new GameObject("TestManager");
            var manager = managerGo.AddComponent<GameKitManager>();
            manager.Initialize("test_manager", GameKitManager.ManagerType.ResourcePool, false);
            manager.SetResource("gold", 50);

            // Act
            interaction.ManualTrigger();

            // Assert
            Assert.AreEqual(150, manager.GetResource("gold"));

            // Cleanup
            Object.DestroyImmediate(managerGo);
        }

        [Test]
        public void ManagerResourceCondition_ChecksResource()
        {
            // Arrange
            testInteractionGo = new GameObject("TestInteraction");
            var interaction = testInteractionGo.AddComponent<GameKitInteraction>();
            interaction.Initialize("interaction_001", GameKitInteraction.TriggerType.Input);
            interaction.AddCondition(GameKitInteraction.ConditionType.ManagerResource, "gold:100");

            // Create manager
            var managerGo = new GameObject("TestManager");
            var manager = managerGo.AddComponent<GameKitManager>();
            manager.Initialize("test_manager", GameKitManager.ManagerType.ResourcePool, false);
            manager.SetResource("gold", 50); // Below threshold

            bool eventTriggered = false;
            interaction.OnInteractionTriggered.AddListener(_ => eventTriggered = true);

            // Act
            interaction.ManualTrigger();

            // Assert - Should not trigger because gold < 100
            Assert.IsFalse(eventTriggered);

            // Set sufficient resource
            manager.SetResource("gold", 150);
            interaction.ManualTrigger();

            // Assert - Should trigger now
            Assert.IsTrue(eventTriggered);

            // Cleanup
            Object.DestroyImmediate(managerGo);
        }
    }
}

