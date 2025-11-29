using NUnit.Framework;
using UnityAIForge.GameKit;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using UnityEngine.TestTools;

namespace UnityAIForge.Tests.Editor
{
    [TestFixture]
    public class TileGridMovementTests
    {
        private GameObject testActorGo;
        private GameKitActor actor;
        private TileGridMovement tileMovement;

        [SetUp]
        public void Setup()
        {
            // Create a new scene for testing
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Create test actor with TileGridMovement
            testActorGo = new GameObject("TestActor");
            actor = testActorGo.AddComponent<GameKitActor>();
            actor.Initialize("test_actor", GameKitActor.BehaviorProfile.TwoDTileGrid, GameKitActor.ControlMode.DirectController);
            tileMovement = testActorGo.AddComponent<TileGridMovement>();
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
        public void GridPosition_ReturnsCorrectGridCoordinates()
        {
            // Arrange
            testActorGo.transform.position = new Vector3(2.5f, 3.5f, 0);
            
            // Act
            Vector2Int gridPos = tileMovement.GridPosition;
            
            // Assert
            Assert.AreEqual(new Vector2Int(3, 4), gridPos); // Rounds to nearest grid
        }

        [Test]
        public void SnapToGrid_AlignsToNearestGridCell()
        {
            // Arrange
            testActorGo.transform.position = new Vector3(1.3f, 2.7f, 0);
            
            // Act
            tileMovement.SnapToGrid();
            
            // Assert
            Assert.AreEqual(new Vector3(1f, 3f, 0), testActorGo.transform.position);
        }

        [Test]
        public void TryMove_ValidDirection_ReturnsTrue()
        {
            // Arrange
            testActorGo.transform.position = Vector3.zero;
            tileMovement.SnapToGrid();
            
            // Act
            bool result = tileMovement.TryMove(Vector2Int.right);
            
            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void TryMove_WhileMoving_ReturnsFalse()
        {
            // Arrange
            testActorGo.transform.position = Vector3.zero;
            tileMovement.SnapToGrid();
            tileMovement.TryMove(Vector2Int.right);
            
            // Act - try to move again while already moving
            bool result = tileMovement.TryMove(Vector2Int.up);
            
            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TeleportToGrid_MovesInstantly()
        {
            // Arrange
            testActorGo.transform.position = Vector3.zero;
            Vector2Int targetGrid = new Vector2Int(5, 3);
            
            // Act
            tileMovement.TeleportToGrid(targetGrid);
            
            // Assert
            Assert.AreEqual(new Vector3(5f, 3f, 0), testActorGo.transform.position);
            Assert.IsFalse(tileMovement.IsMoving);
        }

        [Test]
        public void GetWorldPosition_ReturnsCorrectWorldCoordinates()
        {
            // Arrange
            Vector2Int gridPos = new Vector2Int(3, 4);
            
            // Act
            Vector3 worldPos = tileMovement.GetWorldPosition(gridPos);
            
            // Assert
            Assert.AreEqual(new Vector3(3f, 4f, 0), worldPos);
        }

        [Test]
        public void TileMovement_WithDisabledSmoothing_MovesInstantly()
        {
            // Arrange
            testActorGo.transform.position = Vector3.zero;
            tileMovement.SnapToGrid();
            
            // Disable smooth movement for immediate completion
            var serializedMovement = new UnityEditor.SerializedObject(tileMovement);
            serializedMovement.FindProperty("smoothMovement").boolValue = false;
            serializedMovement.ApplyModifiedProperties();
            
            Vector3 startPos = testActorGo.transform.position;
            
            // Act - Test the actual movement functionality (not the event system)
            bool moveResult = tileMovement.TryMove(Vector2Int.right);
            
            // Assert
            Assert.IsTrue(moveResult, "TryMove should succeed");
            Assert.AreNotEqual(startPos, testActorGo.transform.position, "Position should have changed after movement");
            Assert.AreEqual(new Vector3(1f, 0f, 0f), testActorGo.transform.position, "Should be one grid unit to the right");
        }

        [Test]
        public void GridSize_DefaultValue_IsOne()
        {
            // Assert
            Assert.AreEqual(1f, tileMovement.GridSize);
        }

        [Test]
        public void ImmediateMovement_CompletesInstantly()
        {
            // Arrange
            testActorGo.transform.position = Vector3.zero;
            tileMovement.SnapToGrid();
            
            // Disable smooth movement for immediate completion
            var serializedMovement = new UnityEditor.SerializedObject(tileMovement);
            serializedMovement.FindProperty("smoothMovement").boolValue = false;
            serializedMovement.ApplyModifiedProperties();
            
            // Act
            bool result = tileMovement.TryMove(Vector2Int.right);
            
            // Assert
            Assert.IsTrue(result, "Move should succeed");
            Assert.IsFalse(tileMovement.IsMoving, "Movement should complete instantly when smooth movement is disabled");
            Assert.AreEqual(new Vector3(1f, 0f, 0f), testActorGo.transform.position, "Should be at target position");
        }

        [Test]
        public void IsGridPositionBlocked_WithNoObstacles_ReturnsFalse()
        {
            // Arrange
            Vector2Int testPos = new Vector2Int(5, 5);
            
            // Act
            bool isBlocked = tileMovement.IsGridPositionBlocked(testPos);
            
            // Assert
            Assert.IsFalse(isBlocked);
        }
    }
}

