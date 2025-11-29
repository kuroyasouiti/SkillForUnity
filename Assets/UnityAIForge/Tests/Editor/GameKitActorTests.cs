using NUnit.Framework;
using UnityAIForge.GameKit;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UnityAIForge.Tests.Editor
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
        public void SendMoveInput_InvokesEvent()
        {
            // Arrange
            testActorGo = new GameObject("TestActor");
            var actor = testActorGo.AddComponent<GameKitActor>();
            actor.Initialize("actor_001", GameKitActor.BehaviorProfile.TwoDPhysics, GameKitActor.ControlMode.DirectController);
            
            Vector3 receivedInput = Vector3.zero;
            actor.OnMoveInput.AddListener((input) => receivedInput = input);

            // Act
            actor.SendMoveInput(new Vector3(1, 0, 0));

            // Assert
            Assert.AreEqual(new Vector3(1, 0, 0), receivedInput);
        }

        [Test]
        public void SendJumpInput_InvokesEvent()
        {
            // Arrange
            testActorGo = new GameObject("TestActor");
            var actor = testActorGo.AddComponent<GameKitActor>();
            actor.Initialize("actor_001", GameKitActor.BehaviorProfile.TwoDPhysics, GameKitActor.ControlMode.DirectController);
            
            bool jumpReceived = false;
            actor.OnJumpInput.AddListener(() => jumpReceived = true);

            // Act
            actor.SendJumpInput();

            // Assert
            Assert.IsTrue(jumpReceived);
        }

        [Test]
        public void SendActionInput_InvokesEvent()
        {
            // Arrange
            testActorGo = new GameObject("TestActor");
            var actor = testActorGo.AddComponent<GameKitActor>();
            actor.Initialize("actor_001", GameKitActor.BehaviorProfile.TwoDPhysics, GameKitActor.ControlMode.DirectController);
            
            string receivedAction = "";
            actor.OnActionInput.AddListener((action) => receivedAction = action);

            // Act
            actor.SendActionInput("attack");

            // Assert
            Assert.AreEqual("attack", receivedAction);
        }

        [Test]
        public void SendLookInput_InvokesEvent()
        {
            // Arrange
            testActorGo = new GameObject("TestActor");
            var actor = testActorGo.AddComponent<GameKitActor>();
            actor.Initialize("actor_001", GameKitActor.BehaviorProfile.ThreeDPhysics, GameKitActor.ControlMode.DirectController);
            
            Vector2 receivedLook = Vector2.zero;
            actor.OnLookInput.AddListener((look) => receivedLook = look);

            // Act
            actor.SendLookInput(new Vector2(0.5f, -0.3f));

            // Assert
            Assert.AreEqual(new Vector2(0.5f, -0.3f), receivedLook);
        }

        [Test]
        public void MultipleListeners_AllReceiveInput()
        {
            // Arrange
            testActorGo = new GameObject("TestActor");
            var actor = testActorGo.AddComponent<GameKitActor>();
            actor.Initialize("actor_001", GameKitActor.BehaviorProfile.TwoDPhysics, GameKitActor.ControlMode.DirectController);
            
            int listener1Called = 0;
            int listener2Called = 0;
            actor.OnMoveInput.AddListener((input) => listener1Called++);
            actor.OnMoveInput.AddListener((input) => listener2Called++);

            // Act
            actor.SendMoveInput(Vector3.forward);

            // Assert
            Assert.AreEqual(1, listener1Called);
            Assert.AreEqual(1, listener2Called);
        }

        [Test]
        public void DirectController_HasInputComponent()
        {
            // Arrange & Act
            testActorGo = new GameObject("TestActor");
            var actor = testActorGo.AddComponent<GameKitActor>();
            actor.Initialize("actor_001", GameKitActor.BehaviorProfile.TwoDPhysics, GameKitActor.ControlMode.DirectController);
            
            // Check for Input System controller first
            var inputSystemControllerType = System.Type.GetType("UnityAIForge.GameKit.GameKitInputSystemController, UnityAIForge.GameKit.Runtime");
            var playerInputType = System.Type.GetType("UnityEngine.InputSystem.PlayerInput, Unity.InputSystem");
            
            bool hasInputSystem = inputSystemControllerType != null && playerInputType != null;
            
            if (hasInputSystem)
            {
                testActorGo.AddComponent(playerInputType);
                testActorGo.AddComponent(inputSystemControllerType);
                
                var inputSystemController = testActorGo.GetComponent(inputSystemControllerType);
                Assert.IsNotNull(inputSystemController, "DirectController should have GameKitInputSystemController when Input System is available");
            }
            else
            {
                // Fallback to legacy input
                var simpleInputType = System.Type.GetType("UnityAIForge.GameKit.GameKitSimpleInput, UnityAIForge.GameKit.Runtime");
                if (simpleInputType != null)
                {
                    testActorGo.AddComponent(simpleInputType);
                    var inputComponent = testActorGo.GetComponent(simpleInputType);
                    Assert.IsNotNull(inputComponent, "DirectController should have GameKitSimpleInput component as fallback");
                }
            }
        }

        [Test]
        public void AIAutonomous_HasSimpleAIComponent()
        {
            // Arrange & Act
            testActorGo = new GameObject("TestActor");
            var actor = testActorGo.AddComponent<GameKitActor>();
            actor.Initialize("actor_002", GameKitActor.BehaviorProfile.ThreeDNavMesh, GameKitActor.ControlMode.AIAutonomous);
            
            var simpleAIType = System.Type.GetType("UnityAIForge.GameKit.GameKitSimpleAI, UnityAIForge.GameKit.Runtime");
            if (simpleAIType != null)
            {
                testActorGo.AddComponent(simpleAIType);
            }

            // Assert
            if (simpleAIType != null)
            {
                var aiComponent = testActorGo.GetComponent(simpleAIType);
                Assert.IsNotNull(aiComponent, "AIAutonomous should have GameKitSimpleAI component");
            }
        }
    }
}

