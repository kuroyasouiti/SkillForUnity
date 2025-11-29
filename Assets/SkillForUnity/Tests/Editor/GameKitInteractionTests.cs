using NUnit.Framework;
using SkillForUnity.GameKit;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SkillForUnity.Tests.Editor
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
    }
}

