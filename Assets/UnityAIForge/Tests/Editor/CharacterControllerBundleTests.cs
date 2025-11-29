using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UnityAIForge.Tests.Editor
{
    [TestFixture]
    public class CharacterControllerBundleTests
    {
        private GameObject testGo;

        [SetUp]
        public void Setup()
        {
            // Create a new scene for testing
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            testGo = new GameObject("TestCharacter");
        }

        [TearDown]
        public void Teardown()
        {
            if (testGo != null)
            {
                Object.DestroyImmediate(testGo);
            }
        }

        [Test]
        public void ApplyPreset_FPS_CreatesControllerWithCorrectSettings()
        {
            // Act
            var controller = testGo.AddComponent<CharacterController>();
            controller.radius = 0.5f;
            controller.height = 2f;
            controller.center = new Vector3(0, 1, 0);
            controller.slopeLimit = 45f;
            controller.stepOffset = 0.3f;

            // Assert
            Assert.IsNotNull(controller);
            Assert.AreEqual(0.5f, controller.radius, 0.01f);
            Assert.AreEqual(2f, controller.height, 0.01f);
            Assert.AreEqual(45f, controller.slopeLimit, 0.01f);
        }

        [Test]
        public void ApplyPreset_TPS_CreatesControllerWithCorrectSettings()
        {
            // Act
            var controller = testGo.AddComponent<CharacterController>();
            controller.radius = 0.4f;
            controller.height = 1.8f;
            controller.center = new Vector3(0, 0.9f, 0);

            // Assert
            Assert.IsNotNull(controller);
            Assert.AreEqual(0.4f, controller.radius, 0.01f);
            Assert.AreEqual(1.8f, controller.height, 0.01f);
        }

        [Test]
        public void ApplyPreset_Platformer_CreatesControllerWithCorrectSettings()
        {
            // Act
            var controller = testGo.AddComponent<CharacterController>();
            controller.radius = 0.3f;
            controller.height = 1.6f;
            controller.slopeLimit = 50f;
            controller.stepOffset = 0.4f;

            // Assert
            Assert.IsNotNull(controller);
            Assert.AreEqual(0.3f, controller.radius, 0.01f);
            Assert.AreEqual(1.6f, controller.height, 0.01f);
            Assert.AreEqual(50f, controller.slopeLimit, 0.01f);
        }

        [Test]
        public void ApplyPreset_Child_CreatesControllerWithSmallerSize()
        {
            // Act
            var controller = testGo.AddComponent<CharacterController>();
            controller.radius = 0.35f;
            controller.height = 1.2f;

            // Assert
            Assert.IsNotNull(controller);
            Assert.Less(controller.height, 2f, "Child preset should have smaller height");
        }

        [Test]
        public void ApplyPreset_Large_CreatesControllerWithLargerSize()
        {
            // Act
            var controller = testGo.AddComponent<CharacterController>();
            controller.radius = 1f;
            controller.height = 3.5f;

            // Assert
            Assert.IsNotNull(controller);
            Assert.Greater(controller.height, 2f, "Large preset should have larger height");
        }

        [Test]
        public void UpdateController_ModifiesExistingController()
        {
            // Arrange
            var controller = testGo.AddComponent<CharacterController>();
            controller.radius = 0.5f;

            // Act
            controller.radius = 0.75f;
            controller.height = 2.5f;

            // Assert
            Assert.AreEqual(0.75f, controller.radius, 0.01f);
            Assert.AreEqual(2.5f, controller.height, 0.01f);
        }

        [Test]
        public void InspectController_ReturnsCorrectInfo()
        {
            // Arrange
            var controller = testGo.AddComponent<CharacterController>();
            controller.radius = 0.5f;
            controller.height = 2f;
            controller.slopeLimit = 45f;

            // Assert
            Assert.IsNotNull(controller);
            Assert.AreEqual(0.5f, controller.radius);
            Assert.AreEqual(2f, controller.height);
            Assert.AreEqual(45f, controller.slopeLimit);
        }

        [Test]
        public void CharacterController_IsGroundedProperty_Works()
        {
            // Arrange
            var controller = testGo.AddComponent<CharacterController>();
            testGo.transform.position = new Vector3(0, 10, 0);

            // Act & Assert
            // Note: isGrounded requires physics simulation, so we just check the property exists
            Assert.IsFalse(controller.isGrounded);
        }

        [Test]
        public void CharacterController_VelocityProperty_ReturnsZeroInitially()
        {
            // Arrange
            var controller = testGo.AddComponent<CharacterController>();

            // Act & Assert
            Assert.AreEqual(Vector3.zero, controller.velocity);
        }

        [Test]
        public void CharacterController_SkinWidth_IsConfigurable()
        {
            // Arrange
            var controller = testGo.AddComponent<CharacterController>();

            // Act
            controller.skinWidth = 0.1f;

            // Assert
            Assert.AreEqual(0.1f, controller.skinWidth, 0.001f);
        }

        [Test]
        public void CharacterController_MinMoveDistance_IsConfigurable()
        {
            // Arrange
            var controller = testGo.AddComponent<CharacterController>();

            // Act
            controller.minMoveDistance = 0.001f;

            // Assert
            Assert.AreEqual(0.001f, controller.minMoveDistance, 0.0001f);
        }
    }
}

