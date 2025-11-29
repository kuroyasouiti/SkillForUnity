using NUnit.Framework;
using UnityAIForge.GameKit;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UnityAIForge.Tests.Editor
{
    [TestFixture]
    public class GameKitUICommandTests
    {
        private GameObject testUICommandGo;
        private GameObject testCanvasGo;
        private GameObject testActorGo;
        private GameKitActor testActor;

        [SetUp]
        public void Setup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Create a canvas for UI tests
            testCanvasGo = new GameObject("TestCanvas");
            testCanvasGo.AddComponent<Canvas>();
            testCanvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            testCanvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create test actor
            testActorGo = new GameObject("TestActor");
            testActor = testActorGo.AddComponent<GameKitActor>();
            testActor.Initialize("actor_001", GameKitActor.BehaviorProfile.TwoDLinear, GameKitActor.ControlMode.UICommand);
        }

        [TearDown]
        public void Teardown()
        {
            if (testUICommandGo != null)
            {
                Object.DestroyImmediate(testUICommandGo);
            }
            if (testCanvasGo != null)
            {
                Object.DestroyImmediate(testCanvasGo);
            }
            if (testActorGo != null)
            {
                Object.DestroyImmediate(testActorGo);
            }
        }

        [Test]
        public void CreateUICommand_WithValidParameters_CreatesPanel()
        {
            // Arrange
            testUICommandGo = new GameObject("CommandPanel", typeof(RectTransform));
            testUICommandGo.transform.SetParent(testCanvasGo.transform, false);
            var uiCommand = testUICommandGo.AddComponent<GameKitUICommand>();

            // Act
            uiCommand.Initialize("panel_001", "actor_001");

            // Assert
            Assert.IsNotNull(uiCommand);
            Assert.AreEqual("panel_001", uiCommand.PanelId);
            Assert.AreEqual("actor_001", uiCommand.TargetActorId);
        }

        [Test]
        public void RegisterButton_WithValidButton_RegistersButton()
        {
            // Arrange
            testUICommandGo = new GameObject("CommandPanel", typeof(RectTransform));
            testUICommandGo.transform.SetParent(testCanvasGo.transform, false);
            var uiCommand = testUICommandGo.AddComponent<GameKitUICommand>();
            uiCommand.Initialize("panel_001", "actor_001");

            var buttonGo = new GameObject("MoveButton", typeof(RectTransform));
            buttonGo.transform.SetParent(testUICommandGo.transform, false);
            var button = buttonGo.AddComponent<Button>();

            // Act
            uiCommand.RegisterButton("move", button);

            // Assert
            Assert.IsNotNull(button);
            Assert.IsTrue(uiCommand.HasCommand("move"));
        }

        [Test]
        public void SetTargetActor_UpdatesTargetActor()
        {
            // Arrange
            testUICommandGo = new GameObject("CommandPanel", typeof(RectTransform));
            testUICommandGo.transform.SetParent(testCanvasGo.transform, false);
            var uiCommand = testUICommandGo.AddComponent<GameKitUICommand>();
            uiCommand.Initialize("panel_001", "actor_001");

            // Act
            uiCommand.SetTargetActor("actor_002");

            // Assert
            Assert.AreEqual("actor_002", uiCommand.TargetActorId);
        }

        [Test]
        public void SetTargetActor_WithReference_UpdatesActorReference()
        {
            // Arrange
            testUICommandGo = new GameObject("CommandPanel", typeof(RectTransform));
            testUICommandGo.transform.SetParent(testCanvasGo.transform, false);
            var uiCommand = testUICommandGo.AddComponent<GameKitUICommand>();

            // Act
            uiCommand.SetTargetActor(testActor);

            // Assert
            Assert.AreEqual(testActor, uiCommand.TargetActor);
            Assert.AreEqual("actor_001", uiCommand.TargetActorId);
        }

        [Test]
        public void ExecuteMoveCommand_SendsInputToActor()
        {
            // Arrange
            testUICommandGo = new GameObject("CommandPanel", typeof(RectTransform));
            testUICommandGo.transform.SetParent(testCanvasGo.transform, false);
            var uiCommand = testUICommandGo.AddComponent<GameKitUICommand>();
            uiCommand.SetTargetActor(testActor);

            Vector3 receivedInput = Vector3.zero;
            testActor.OnMoveInput.AddListener(input => receivedInput = input);

            // Act
            Vector3 expectedDirection = new Vector3(1, 0, 0);
            uiCommand.ExecuteMoveCommand(expectedDirection);

            // Assert
            Assert.AreEqual(expectedDirection, receivedInput);
        }

        [Test]
        public void ExecuteJumpCommand_SendsJumpToActor()
        {
            // Arrange
            testUICommandGo = new GameObject("CommandPanel", typeof(RectTransform));
            testUICommandGo.transform.SetParent(testCanvasGo.transform, false);
            var uiCommand = testUICommandGo.AddComponent<GameKitUICommand>();
            uiCommand.SetTargetActor(testActor);

            bool jumpReceived = false;
            testActor.OnJumpInput.AddListener(() => jumpReceived = true);

            // Act
            uiCommand.ExecuteJumpCommand();

            // Assert
            Assert.IsTrue(jumpReceived);
        }

        [Test]
        public void ExecuteActionCommand_SendsActionToActor()
        {
            // Arrange
            testUICommandGo = new GameObject("CommandPanel", typeof(RectTransform));
            testUICommandGo.transform.SetParent(testCanvasGo.transform, false);
            var uiCommand = testUICommandGo.AddComponent<GameKitUICommand>();
            uiCommand.SetTargetActor(testActor);

            string receivedAction = null;
            testActor.OnActionInput.AddListener(action => receivedAction = action);

            // Act
            uiCommand.ExecuteActionCommand("attack");

            // Assert
            Assert.AreEqual("attack", receivedAction);
        }

        [Test]
        public void ExecuteLookCommand_SendsLookToActor()
        {
            // Arrange
            testUICommandGo = new GameObject("CommandPanel", typeof(RectTransform));
            testUICommandGo.transform.SetParent(testCanvasGo.transform, false);
            var uiCommand = testUICommandGo.AddComponent<GameKitUICommand>();
            uiCommand.SetTargetActor(testActor);

            Vector2 receivedDirection = Vector2.zero;
            testActor.OnLookInput.AddListener(direction => receivedDirection = direction);

            // Act
            Vector2 expectedDirection = new Vector2(0.5f, 0.5f);
            uiCommand.ExecuteLookCommand(expectedDirection);

            // Assert
            Assert.AreEqual(expectedDirection, receivedDirection);
        }

        [Test]
        public void RegisterDirectionalButton_CreatesProperBinding()
        {
            // Arrange
            testUICommandGo = new GameObject("CommandPanel", typeof(RectTransform));
            testUICommandGo.transform.SetParent(testCanvasGo.transform, false);
            var uiCommand = testUICommandGo.AddComponent<GameKitUICommand>();
            uiCommand.SetTargetActor(testActor);

            var buttonGo = new GameObject("UpButton", typeof(RectTransform));
            buttonGo.transform.SetParent(testUICommandGo.transform, false);
            var button = buttonGo.AddComponent<Button>();

            Vector3 receivedInput = Vector3.zero;
            testActor.OnMoveInput.AddListener(input => receivedInput = input);

            // Act
            Vector3 upDirection = new Vector3(0, 1, 0);
            uiCommand.RegisterDirectionalButton("moveUp", button, upDirection);
            uiCommand.ExecuteCommand("moveUp");

            // Assert
            Assert.AreEqual(upDirection, receivedInput);
        }

        [Test]
        public void ExecuteCommand_WithBinding_SendsCorrectCommand()
        {
            // Arrange
            testUICommandGo = new GameObject("CommandPanel", typeof(RectTransform));
            testUICommandGo.transform.SetParent(testCanvasGo.transform, false);
            var uiCommand = testUICommandGo.AddComponent<GameKitUICommand>();
            uiCommand.SetTargetActor(testActor);

            var buttonGo = new GameObject("ActionButton", typeof(RectTransform));
            buttonGo.transform.SetParent(testUICommandGo.transform, false);
            var button = buttonGo.AddComponent<Button>();

            string receivedAction = null;
            testActor.OnActionInput.AddListener(action => receivedAction = action);

            // Act
            uiCommand.RegisterButton("useItem", button, GameKitUICommand.CommandType.Action, "potion");
            uiCommand.ExecuteCommand("useItem");

            // Assert
            Assert.AreEqual("potion", receivedAction);
        }

        [Test]
        public void GetCommandNames_ReturnsAllRegisteredCommands()
        {
            // Arrange
            testUICommandGo = new GameObject("CommandPanel", typeof(RectTransform));
            testUICommandGo.transform.SetParent(testCanvasGo.transform, false);
            var uiCommand = testUICommandGo.AddComponent<GameKitUICommand>();

            var button1 = CreateButton("Button1");
            var button2 = CreateButton("Button2");
            var button3 = CreateButton("Button3");

            uiCommand.RegisterButton("command1", button1);
            uiCommand.RegisterButton("command2", button2);
            uiCommand.RegisterButton("command3", button3);

            // Act
            var commandNames = uiCommand.GetCommandNames();

            // Assert
            Assert.AreEqual(3, commandNames.Count);
            Assert.Contains("command1", commandNames);
            Assert.Contains("command2", commandNames);
            Assert.Contains("command3", commandNames);
        }

        [Test]
        public void ClearBindings_RemovesAllCommands()
        {
            // Arrange
            testUICommandGo = new GameObject("CommandPanel", typeof(RectTransform));
            testUICommandGo.transform.SetParent(testCanvasGo.transform, false);
            var uiCommand = testUICommandGo.AddComponent<GameKitUICommand>();

            var button = CreateButton("Button");
            uiCommand.RegisterButton("command1", button);

            // Act
            uiCommand.ClearBindings();

            // Assert
            Assert.AreEqual(0, uiCommand.GetCommandNames().Count);
            Assert.IsFalse(uiCommand.HasCommand("command1"));
        }

        [Test]
        public void HasCommand_ReturnsTrueForRegisteredCommand()
        {
            // Arrange
            testUICommandGo = new GameObject("CommandPanel", typeof(RectTransform));
            testUICommandGo.transform.SetParent(testCanvasGo.transform, false);
            var uiCommand = testUICommandGo.AddComponent<GameKitUICommand>();

            var button = CreateButton("Button");
            uiCommand.RegisterButton("testCommand", button);

            // Act & Assert
            Assert.IsTrue(uiCommand.HasCommand("testCommand"));
            Assert.IsFalse(uiCommand.HasCommand("nonexistent"));
        }

        private Button CreateButton(string name)
        {
            var buttonGo = new GameObject(name, typeof(RectTransform));
            buttonGo.transform.SetParent(testUICommandGo.transform, false);
            return buttonGo.AddComponent<Button>();
        }
    }
}

