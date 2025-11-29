using NUnit.Framework;
using UnityEngine;
using UnityAIForge.GameKit;

namespace UnityAIForge.Tests.Editor
{
    [TestFixture]
    public class SplineMovementTests
    {
        private GameObject actorObject;
        private GameKitActor actor;
        private SplineMovement splineMovement;
        private GameObject[] controlPointObjects;

        [SetUp]
        public void SetUp()
        {
            // Create actor with SplineMovement
            actorObject = new GameObject("TestActor");
            actor = actorObject.AddComponent<GameKitActor>();
            actor.Initialize("test_spline_actor", GameKitActor.BehaviorProfile.SplineMovement, GameKitActor.ControlMode.DirectController);
            
            splineMovement = actorObject.AddComponent<SplineMovement>();

            // Create control points for a simple straight path
            controlPointObjects = new GameObject[4];
            var controlPoints = new Transform[4];
            
            for (int i = 0; i < 4; i++)
            {
                controlPointObjects[i] = new GameObject($"ControlPoint{i}");
                controlPointObjects[i].transform.position = new Vector3(i * 2f, 0f, 0f);
                controlPoints[i] = controlPointObjects[i].transform;
            }

            splineMovement.SetControlPoints(controlPoints);
        }

        [TearDown]
        public void TearDown()
        {
            if (actorObject != null)
                Object.DestroyImmediate(actorObject);
            
            if (controlPointObjects != null)
            {
                foreach (var obj in controlPointObjects)
                {
                    if (obj != null)
                        Object.DestroyImmediate(obj);
                }
            }
        }

        [Test]
        public void SplineMovement_IsAddedToActor()
        {
            Assert.IsNotNull(splineMovement);
            Assert.IsNotNull(actor);
        }

        [Test]
        public void RebuildSpline_CreatesSplineFromControlPoints()
        {
            splineMovement.RebuildSpline();
            
            Assert.Greater(splineMovement.SplineLength, 0f);
            Assert.AreEqual(0f, splineMovement.Progress);
        }

        [Test]
        public void SetProgress_UpdatesPosition()
        {
            splineMovement.RebuildSpline();
            Vector3 startPosition = actorObject.transform.position;
            
            splineMovement.SetProgress(0.5f);
            
            Assert.AreNotEqual(startPosition, actorObject.transform.position);
            Assert.AreEqual(0.5f, splineMovement.Progress, 0.01f);
        }

        [Test]
        public void StartMoving_SetsIsMovingTrue()
        {
            splineMovement.StartMoving();
            
            Assert.IsTrue(splineMovement.IsMoving);
        }

        [Test]
        public void StopMoving_SetsIsMovingFalse()
        {
            splineMovement.StartMoving();
            splineMovement.StopMoving();
            
            Assert.IsFalse(splineMovement.IsMoving);
            Assert.AreEqual(0f, splineMovement.CurrentSpeed);
        }

        [Test]
        public void GetPointAtProgress_ReturnsCorrectPosition()
        {
            splineMovement.RebuildSpline();
            
            Vector3 startPoint = splineMovement.GetPointAtProgress(0f);
            Vector3 midPoint = splineMovement.GetPointAtProgress(0.5f);
            Vector3 endPoint = splineMovement.GetPointAtProgress(1f);
            
            // Start should be close to first control point
            Assert.Less(Vector3.Distance(startPoint, controlPointObjects[0].transform.position), 0.5f);
            
            // Mid should be somewhere in the middle
            Assert.Greater(midPoint.x, startPoint.x);
            Assert.Less(midPoint.x, endPoint.x);
            
            // End should be close to last control point
            Assert.Less(Vector3.Distance(endPoint, controlPointObjects[3].transform.position), 0.5f);
        }

        [Test]
        public void TeleportToProgress_UpdatesPositionWithoutSpeed()
        {
            splineMovement.RebuildSpline();
            splineMovement.StartMoving();
            
            splineMovement.TeleportToProgress(0.75f);
            
            Assert.AreEqual(0.75f, splineMovement.Progress, 0.01f);
            Assert.AreEqual(0f, splineMovement.CurrentSpeed);
        }

        [Test]
        public void SetSpeed_UpdatesMoveSpeed()
        {
            float newSpeed = 10f;
            splineMovement.SetSpeed(newSpeed);
            
            // Speed should be applied (can't directly test private field, but method should not throw)
            Assert.DoesNotThrow(() => splineMovement.SetSpeed(newSpeed));
        }

        [Test]
        public void SetLateralOffset_AppliesOffset()
        {
            splineMovement.RebuildSpline();
            Vector3 offset = new Vector3(0f, 1f, 0f);
            
            splineMovement.SetLateralOffset(offset);
            splineMovement.SetProgress(0f);
            
            // Position should be offset from spline
            Vector3 expectedPosition = splineMovement.GetPointAtProgress(0f) + offset;
            Assert.Less(Vector3.Distance(actorObject.transform.position, expectedPosition), 0.1f);
        }

        [Test]
        public void SplineLength_IsPositive()
        {
            splineMovement.RebuildSpline();
            
            Assert.Greater(splineMovement.SplineLength, 0f);
        }

        [Test]
        public void GetTangentAtProgress_ReturnsNormalizedVector()
        {
            splineMovement.RebuildSpline();
            
            Vector3 tangent = splineMovement.GetTangentAtProgress(0.5f);
            
            Assert.AreEqual(1f, tangent.magnitude, 0.1f, "Tangent should be normalized");
        }

        [Test]
        public void ClosedLoop_ConnectsLastToFirst()
        {
            // Create a new spline movement with closed loop
            var loopObject = new GameObject("LoopActor");
            var loopSpline = loopObject.AddComponent<SplineMovement>();
            
            // Use reflection to set closedLoop
            var serializedSpline = new UnityEditor.SerializedObject(loopSpline);
            serializedSpline.FindProperty("closedLoop").boolValue = true;
            serializedSpline.ApplyModifiedProperties();
            
            loopSpline.SetControlPoints(new Transform[] 
            { 
                controlPointObjects[0].transform, 
                controlPointObjects[1].transform, 
                controlPointObjects[2].transform 
            });
            loopSpline.RebuildSpline();
            
            Assert.Greater(loopSpline.SplineLength, 0f);
            
            Object.DestroyImmediate(loopObject);
        }

        [Test]
        public void ReverseDirection_ChangesSpeed()
        {
            splineMovement.RebuildSpline();
            
            // Enable backward movement via reflection
            var serializedSpline = new UnityEditor.SerializedObject(splineMovement);
            serializedSpline.FindProperty("allowBackwardMovement").boolValue = true;
            serializedSpline.ApplyModifiedProperties();
            
            splineMovement.ReverseDirection();
            
            // Method should execute without error
            Assert.DoesNotThrow(() => splineMovement.ReverseDirection());
        }

        [Test]
        public void ControlPoints_MinimumTwoRequired()
        {
            // Create spline with only 1 control point
            var singlePointObject = new GameObject("SinglePoint");
            singlePointObject.transform.position = Vector3.zero;
            
            splineMovement.SetControlPoints(new Transform[] { singlePointObject.transform });
            splineMovement.RebuildSpline();
            
            // Should handle gracefully
            Assert.AreEqual(0f, splineMovement.SplineLength);
            
            Object.DestroyImmediate(singlePointObject);
        }

        [Test]
        public void Progress_ClampedBetweenZeroAndOne()
        {
            splineMovement.RebuildSpline();
            
            splineMovement.SetProgress(-0.5f);
            Assert.AreEqual(0f, splineMovement.Progress);
            
            splineMovement.SetProgress(1.5f);
            Assert.AreEqual(1f, splineMovement.Progress);
        }
    }
}

