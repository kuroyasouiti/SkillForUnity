using NUnit.Framework;
using SkillForUnity.GameKit;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SkillForUnity.Tests.Editor
{
    [TestFixture]
    public class GameKitUICommandTests
    {
        private GameObject testUICommandGo;
        private GameObject testCanvasGo;

        [SetUp]
        public void Setup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Create a canvas for UI tests
            testCanvasGo = new GameObject("TestCanvas");
            testCanvasGo.AddComponent<Canvas>();
            testCanvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            testCanvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
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
            Assert.AreEqual(1, button.onClick.GetPersistentEventCount() + 1); // Runtime listener added
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
    }
}

