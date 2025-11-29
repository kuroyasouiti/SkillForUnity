using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace SkillForUnity.Tests.Editor
{
    [TestFixture]
    public class LowLevelToolsTests
    {
        private List<GameObject> testObjects = new List<GameObject>();
        private string testScenePath = "Assets/TestScene.unity";

        [SetUp]
        public void Setup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            testObjects.Clear();
        }

        [TearDown]
        public void Teardown()
        {
            foreach (var obj in testObjects)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }
            testObjects.Clear();

            // Clean up test scene if it exists
            if (System.IO.File.Exists(testScenePath))
            {
                AssetDatabase.DeleteAsset(testScenePath);
            }
        }

        #region Scene CRUD Tests

        [Test]
        public void SceneManage_Create_CreatesNewScene()
        {
            // Arrange & Act
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, testScenePath);

            // Assert
            Assert.IsNotNull(scene);
            Assert.IsTrue(System.IO.File.Exists(testScenePath));
        }

        #endregion

        #region GameObject CRUD Tests

        [Test]
        public void GameObjectManage_Create_CreatesGameObject()
        {
            // Arrange & Act
            var go = new GameObject("TestObject");
            testObjects.Add(go);

            // Assert
            Assert.IsNotNull(go);
            Assert.AreEqual("TestObject", go.name);
            Assert.IsNotNull(GameObject.Find("TestObject"));
        }

        [Test]
        public void GameObjectManage_Rename_RenamesGameObject()
        {
            // Arrange
            var go = new GameObject("OldName");
            testObjects.Add(go);

            // Act
            Undo.RecordObject(go, "Rename GameObject");
            go.name = "NewName";

            // Assert
            Assert.AreEqual("NewName", go.name);
            Assert.IsNotNull(GameObject.Find("NewName"));
        }

        [Test]
        public void GameObjectManage_Delete_DeletesGameObject()
        {
            // Arrange
            var go = new GameObject("ToDelete");
            testObjects.Add(go);

            // Act
            Undo.DestroyObjectImmediate(go);
            testObjects.Remove(go);

            // Assert
            Assert.IsNull(GameObject.Find("ToDelete"));
        }

        #endregion

        #region Component CRUD Tests

        [Test]
        public void ComponentManage_Add_AddsComponent()
        {
            // Arrange
            var go = new GameObject("TestObject");
            testObjects.Add(go);

            // Act
            var rb = Undo.AddComponent<Rigidbody>(go);

            // Assert
            Assert.IsNotNull(rb);
            Assert.IsNotNull(go.GetComponent<Rigidbody>());
        }

        [Test]
        public void ComponentManage_Remove_RemovesComponent()
        {
            // Arrange
            var go = new GameObject("TestObject");
            var rb = go.AddComponent<Rigidbody>();
            testObjects.Add(go);

            // Act
            Undo.DestroyObjectImmediate(rb);

            // Assert
            Assert.IsNull(go.GetComponent<Rigidbody>());
        }

        [Test]
        public void ComponentManage_Update_UpdatesComponentProperties()
        {
            // Arrange
            var go = new GameObject("TestObject");
            var rb = go.AddComponent<Rigidbody>();
            testObjects.Add(go);

            // Act
            Undo.RecordObject(rb, "Update Rigidbody");
            rb.mass = 2f;
            rb.useGravity = false;

            // Assert
            Assert.AreEqual(2f, rb.mass);
            Assert.IsFalse(rb.useGravity);
        }

        #endregion

        #region ScriptableObject CRUD Tests

        [Test]
        public void ScriptableObjectManage_Create_CreatesAsset()
        {
            // Arrange
            var assetPath = "Assets/TestScriptableObject.asset";

            try
            {
                // Act
                var so = ScriptableObject.CreateInstance<ScriptableObject>();
                AssetDatabase.CreateAsset(so, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // Assert
                Assert.IsNotNull(so);
                Assert.IsTrue(System.IO.File.Exists(assetPath));
                var loadedAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                Assert.IsNotNull(loadedAsset);
            }
            finally
            {
                // Cleanup
                if (System.IO.File.Exists(assetPath))
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }
        }

        #endregion
    }
}

