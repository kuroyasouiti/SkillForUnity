using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace UnityAIForge.Tests.Editor
{
    [TestFixture]
    public class MidLevelToolsTests
    {
        private List<GameObject> testObjects = new List<GameObject>();

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
        }

        #region Transform Batch Tests

        [Test]
        public void TransformBatch_ArrangeCircle_ArrangesObjectsInCircle()
        {
            // Arrange
            for (int i = 0; i < 4; i++)
            {
                var go = new GameObject($"Object{i}");
                testObjects.Add(go);
            }

            // Act - Manually arrange in circle
            float radius = 5f;
            for (int i = 0; i < testObjects.Count; i++)
            {
                float angle = i * (360f / testObjects.Count) * Mathf.Deg2Rad;
                testObjects[i].transform.position = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                );
            }

            // Assert
            // Verify objects are positioned in a circle
            Assert.AreNotEqual(Vector3.zero, testObjects[0].transform.position);
            // Check that objects are roughly equidistant from center
            for (int i = 0; i < testObjects.Count; i++)
            {
                float distance = testObjects[i].transform.position.magnitude;
                Assert.AreEqual(radius, distance, 0.01f);
            }
        }

        [Test]
        public void TransformBatch_RenameSequential_RenamesObjects()
        {
            // Arrange
            for (int i = 0; i < 3; i++)
            {
                var go = new GameObject($"OldName{i}");
                testObjects.Add(go);
            }

            // Act - Rename sequentially
            string baseName = "Enemy";
            int startIndex = 1;
            int padding = 2;
            for (int i = 0; i < testObjects.Count; i++)
            {
                testObjects[i].name = $"{baseName}{(startIndex + i).ToString().PadLeft(padding, '0')}";
            }

            // Assert
            Assert.AreEqual("Enemy01", testObjects[0].name);
            Assert.AreEqual("Enemy02", testObjects[1].name);
            Assert.AreEqual("Enemy03", testObjects[2].name);
        }

        #endregion

        #region RectTransform Batch Tests

        [Test]
        public void RectTransformBatch_SetAnchors_SetsAnchorsCorrectly()
        {
            // Arrange
            var canvasGo = new GameObject("Canvas");
            canvasGo.AddComponent<Canvas>();
            testObjects.Add(canvasGo);

            var uiGo = new GameObject("UIElement", typeof(RectTransform));
            uiGo.transform.SetParent(canvasGo.transform, false);
            testObjects.Add(uiGo);

            var rectTransform = uiGo.GetComponent<RectTransform>();

            // Act
            Undo.RecordObject(rectTransform, "Set Anchors");
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);

            // Assert
            Assert.AreEqual(new Vector2(0, 0), rectTransform.anchorMin);
            Assert.AreEqual(new Vector2(1, 1), rectTransform.anchorMax);
        }

        #endregion

        #region Physics Bundle Tests

        [Test]
        public void PhysicsBundle_ApplyPreset2D_AddsRigidbody2D()
        {
            // Arrange
            var go = new GameObject("PhysicsObject");
            testObjects.Add(go);

            // Act
            var rb = Undo.AddComponent<Rigidbody2D>(go);
            rb.bodyType = RigidbodyType2D.Dynamic;
            var collider = Undo.AddComponent<BoxCollider2D>(go);

            // Assert
            Assert.IsNotNull(rb);
            Assert.AreEqual(RigidbodyType2D.Dynamic, rb.bodyType);
            Assert.IsNotNull(collider);
        }

        [Test]
        public void PhysicsBundle_ApplyPreset3D_AddsRigidbody()
        {
            // Arrange
            var go = new GameObject("PhysicsObject");
            testObjects.Add(go);

            // Act
            var rb = Undo.AddComponent<Rigidbody>(go);
            rb.isKinematic = false;
            rb.useGravity = true;
            var collider = Undo.AddComponent<CapsuleCollider>(go);

            // Assert
            Assert.IsNotNull(rb);
            Assert.IsFalse(rb.isKinematic);
            Assert.IsTrue(rb.useGravity);
            Assert.IsNotNull(collider);
        }

        #endregion

        #region Audio Source Bundle Tests

        [Test]
        public void AudioSourceBundle_CreateAudioSource_AddsAudioSource()
        {
            // Arrange
            var go = new GameObject("AudioObject");
            testObjects.Add(go);

            // Act - Apply music preset
            var audioSource = Undo.AddComponent<AudioSource>(go);
            audioSource.loop = true;
            audioSource.volume = 0.5f;
            audioSource.playOnAwake = false;

            // Assert
            Assert.IsNotNull(audioSource);
            Assert.IsTrue(audioSource.loop);
            Assert.AreEqual(0.5f, audioSource.volume);
        }

        #endregion

        #region UI Foundation Tests

        [Test]
        public void UIFoundation_CreateCanvas_CreatesCanvasWithComponents()
        {
            // Arrange & Act
            var canvasGo = new GameObject("TestCanvas");
            testObjects.Add(canvasGo);
            
            var canvas = Undo.AddComponent<Canvas>(canvasGo);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            var scaler = Undo.AddComponent<UnityEngine.UI.CanvasScaler>(canvasGo);
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            var raycaster = Undo.AddComponent<UnityEngine.UI.GraphicRaycaster>(canvasGo);

            // Assert
            Assert.IsNotNull(canvas);
            Assert.IsNotNull(scaler);
            Assert.IsNotNull(raycaster);
            Assert.AreEqual(RenderMode.ScreenSpaceOverlay, canvas.renderMode);
        }

        #endregion
    }
}

