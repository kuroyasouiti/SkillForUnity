using NUnit.Framework;
using UnityAIForge.GameKit;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UnityAIForge.Tests.Editor
{
    [TestFixture]
    public class GraphNodeMovementTests
    {
        private GameObject testActorGo;
        private GameKitActor actor;
        private GraphNodeMovement graphMovement;
        private GraphNode node1, node2, node3, node4;

        [SetUp]
        public void Setup()
        {
            // Create a new scene for testing
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Create actor with graph node movement
            testActorGo = new GameObject("TestActor");
            actor = testActorGo.AddComponent<GameKitActor>();
            actor.Initialize("actor_001", GameKitActor.BehaviorProfile.GraphNode, GameKitActor.ControlMode.DirectController);
            graphMovement = testActorGo.AddComponent<GraphNodeMovement>();

            // Create test graph nodes
            CreateTestGraph();
        }

        [TearDown]
        public void Teardown()
        {
            if (node1 != null) Object.DestroyImmediate(node1.gameObject);
            if (node2 != null) Object.DestroyImmediate(node2.gameObject);
            if (node3 != null) Object.DestroyImmediate(node3.gameObject);
            if (node4 != null) Object.DestroyImmediate(node4.gameObject);
            if (testActorGo != null) Object.DestroyImmediate(testActorGo);
        }

        private void CreateTestGraph()
        {
            // Create a simple graph:
            // node1 -- node2 -- node3
            //    |              |
            //   node4 ----------+

            var go1 = new GameObject("Node1");
            go1.transform.position = new Vector3(0, 0, 0);
            node1 = go1.AddComponent<GraphNode>();

            var go2 = new GameObject("Node2");
            go2.transform.position = new Vector3(1, 0, 0);
            node2 = go2.AddComponent<GraphNode>();

            var go3 = new GameObject("Node3");
            go3.transform.position = new Vector3(2, 0, 0);
            node3 = go3.AddComponent<GraphNode>();

            var go4 = new GameObject("Node4");
            go4.transform.position = new Vector3(0, -1, 0);
            node4 = go4.AddComponent<GraphNode>();

            // Connect nodes
            node1.AddConnection(node2, 1f, true);
            node2.AddConnection(node3, 1f, true);
            node1.AddConnection(node4, 1f, true);
            node4.AddConnection(node3, 2.236f, true); // diagonal
        }

        [Test]
        public void GraphNodeMovement_CreatesComponent()
        {
            // Assert
            Assert.IsNotNull(graphMovement);
        }

        [Test]
        public void SnapToNearestNode_FindsClosestNode()
        {
            // Arrange
            testActorGo.transform.position = new Vector3(0.1f, 0.1f, 0);

            // Act
            graphMovement.SnapToNearestNode();

            // Assert
            Assert.AreEqual(node1, graphMovement.CurrentNode);
            Assert.AreEqual(node1.Position, testActorGo.transform.position);
        }

        [Test]
        public void MoveToNode_AdjacentNode_ReturnsTrue()
        {
            // Arrange
            graphMovement.TeleportToNode(node1);
            
            // Disable smooth movement for immediate completion
            var serializedMovement = new UnityEditor.SerializedObject(graphMovement);
            serializedMovement.FindProperty("smoothMovement").boolValue = false;
            serializedMovement.ApplyModifiedProperties();

            // Act
            bool result = graphMovement.MoveToNode(node2);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(node2, graphMovement.CurrentNode);
            Assert.AreEqual(node2.Position, testActorGo.transform.position);
        }

        [Test]
        public void MoveToNode_NonAdjacentNode_ReturnsFalse()
        {
            // Arrange
            graphMovement.TeleportToNode(node1);

            // Act
            bool result = graphMovement.MoveToNode(node3);

            // Assert
            Assert.IsFalse(result, "Cannot move to non-adjacent node directly");
        }

        [Test]
        public void MoveToNodeWithPathfinding_FindsPath()
        {
            // Arrange
            graphMovement.TeleportToNode(node1);
            
            // Disable smooth movement for immediate completion
            var serializedMovement = new UnityEditor.SerializedObject(graphMovement);
            serializedMovement.FindProperty("smoothMovement").boolValue = false;
            serializedMovement.ApplyModifiedProperties();

            // Act
            bool result = graphMovement.MoveToNodeWithPathfinding(node3);

            // Assert
            Assert.IsTrue(result, "Should find path from node1 to node3");
            Assert.AreEqual(node3, graphMovement.CurrentNode);
        }

        [Test]
        public void GetReachableNodes_ReturnsCorrectNodes()
        {
            // Arrange
            graphMovement.TeleportToNode(node1);

            // Act
            var reachable = graphMovement.GetReachableNodes(1);

            // Assert
            Assert.AreEqual(2, reachable.Count, "Should reach node2 and node4 from node1");
            Assert.Contains(node2, reachable);
            Assert.Contains(node4, reachable);
        }

        [Test]
        public void GetReachableNodes_WithMaxDistance2_ReturnsAll()
        {
            // Arrange
            graphMovement.TeleportToNode(node1);

            // Act
            var reachable = graphMovement.GetReachableNodes(2);

            // Assert
            Assert.GreaterOrEqual(reachable.Count, 2, "Should reach at least 2 nodes");
        }

        [Test]
        public void TeleportToNode_MovesInstantly()
        {
            // Act
            graphMovement.TeleportToNode(node3);

            // Assert
            Assert.AreEqual(node3, graphMovement.CurrentNode);
            Assert.AreEqual(node3.Position, testActorGo.transform.position);
            Assert.IsFalse(graphMovement.IsMoving);
        }

        [Test]
        public void GraphNode_AddConnection_CreatesConnection()
        {
            // Arrange
            var newNode = new GameObject("NewNode").AddComponent<GraphNode>();
            newNode.transform.position = new Vector3(3, 0, 0);
            int initialConnectionCount = node3.Connections.Count;

            // Act
            node3.AddConnection(newNode, 1f, false);

            // Assert
            Assert.IsTrue(node3.IsConnectedTo(newNode));
            Assert.AreEqual(initialConnectionCount + 1, node3.Connections.Count);

            // Cleanup
            UnityEngine.Object.DestroyImmediate(newNode.gameObject);
        }

        [Test]
        public void GraphNode_RemoveConnection_RemovesConnection()
        {
            // Arrange
            int initialConnections = node1.Connections.Count;

            // Act
            node1.RemoveConnection(node2, false);

            // Assert
            Assert.AreEqual(initialConnections - 1, node1.Connections.Count);
            Assert.IsFalse(node1.IsConnectedTo(node2));
        }

        [Test]
        public void GraphNode_SetConnectionTraversable_UpdatesTraversability()
        {
            // Arrange
            Assert.IsTrue(node1.IsConnectedTo(node2));

            // Act
            node1.SetConnectionTraversable(node2, false);

            // Assert
            Assert.IsFalse(node1.IsConnectedTo(node2), "Connection should be non-traversable");
        }

        [Test]
        public void CurrentPath_IsNullInitially()
        {
            // Assert
            Assert.IsNull(graphMovement.CurrentPath);
        }

        [Test]
        public void IsMoving_IsFalseInitially()
        {
            // Assert
            Assert.IsFalse(graphMovement.IsMoving);
        }
    }
}

