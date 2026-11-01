using _Project.Features.Items.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace _Project.Tests.Items
{
    public class HeldItemEquipMotionTests
    {
        private static readonly Vector3 StowedPosition = new Vector3(0.42f, -0.9f, 0.35f);
        private static readonly Vector3 HeldPosition = new Vector3(0.42f, -0.34f, 0.72f);
        private static readonly Vector3 HeldRotation = new Vector3(12f, -18f, 6f);

        [Test]
        public void AdvanceProgress_ZeroDuration_SnapsToComplete()
        {
            Assert.AreEqual(1f, HeldItemEquipMotion.AdvanceProgress(0.2f, 0.016f, 0f));
            Assert.AreEqual(1f, HeldItemEquipMotion.AdvanceProgress(0.2f, 0.016f, -0.5f));
        }

        [Test]
        public void AdvanceProgress_LargeDeltaTime_ClampsToComplete()
        {
            Assert.AreEqual(1f, HeldItemEquipMotion.AdvanceProgress(0.9f, 10f, 0.28f));
        }

        [Test]
        public void AdvanceProgress_NormalAccumulation_StaysWithinUnitRange()
        {
            float progress = HeldItemEquipMotion.AdvanceProgress(0.5f, 0.1f, 0.28f);
            Assert.That(progress, Is.EqualTo(0.5f + 0.1f / 0.28f).Within(0.001f),
                "Progress must accumulate proportionally to the configured duration.");
        }

        [Test]
        public void Ease_StartAndEnd_MatchExactEndpoints()
        {
            Assert.AreEqual(0f, HeldItemEquipMotion.Ease(0f));
            Assert.AreEqual(1f, HeldItemEquipMotion.Ease(1f));
        }

        [Test]
        public void EvaluatePosition_Start_ReturnsStowedPosition()
        {
            Vector3 position = HeldItemEquipMotion.EvaluatePosition(StowedPosition, HeldPosition, 0f);
            Assert.AreEqual(StowedPosition, position);
        }

        [Test]
        public void EvaluatePosition_End_ReturnsHeldPosition()
        {
            Vector3 position = HeldItemEquipMotion.EvaluatePosition(StowedPosition, HeldPosition, 1f);
            Assert.AreEqual(HeldPosition, position);
        }

        [Test]
        public void EvaluateRotation_End_ReturnsHeldRotation()
        {
            Quaternion rotation = HeldItemEquipMotion.EvaluateRotation(HeldRotation, 1f);
            Quaternion expected = Quaternion.Euler(HeldRotation);
            Assert.IsTrue(Quaternion.Angle(rotation, expected) < 0.01f,
                $"Expected {expected}, got {rotation}.");
        }

        [Test]
        public void EvaluateRotation_Start_TiltsPitchByStowAngle()
        {
            Quaternion rotation = HeldItemEquipMotion.EvaluateRotation(HeldRotation, 0f);
            Quaternion expected = Quaternion.Euler(
                HeldRotation.x + HeldItemEquipMotion.StowPitchAngle, HeldRotation.y, HeldRotation.z);
            Assert.IsTrue(Quaternion.Angle(rotation, expected) < 0.01f,
                $"Expected {expected}, got {rotation}.");
        }
    }
}
