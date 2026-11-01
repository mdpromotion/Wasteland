using UnityEngine;

namespace _Project.Features.Items.Presentation
{
    /// <summary>
    /// Pure pose math of the held-item equip motion. Extracted from the view so
    /// edge cases (zero duration, over-drawn progress) are unit-testable and
    /// future motion layers (sway, bob, custom curves) can interpolate between
    /// the same stowed/held poses without duplicating constants.
    /// </summary>
    public static class HeldItemEquipMotion
    {
        public const float StowPitchAngle = 70f;

        /// <summary>Scaled progress clamped into [0,1]; snaps to 1 for a zero duration.</summary>
        public static float AdvanceProgress(float progress, float deltaTime, float duration)
        {
            if (duration <= 0f)
                return 1f;

            return Mathf.Min(1f, progress + deltaTime / duration);
        }

        /// <summary>Cubic smooth-step used by the equip motion.</summary>
        public static float Ease(float progress)
        {
            return progress * progress * (3f - 2f * progress);
        }

        /// <summary>Interpolated local position at the eased progress.</summary>
        public static Vector3 EvaluatePosition(Vector3 stowedPosition, Vector3 heldPosition, float easedProgress)
        {
            return Vector3.Lerp(stowedPosition, heldPosition, easedProgress);
        }

        /// <summary>Interpolated local rotation at the eased progress.</summary>
        public static Quaternion EvaluateRotation(Vector3 heldRotation, float easedProgress)
        {
            Quaternion held = Quaternion.Euler(heldRotation);
            Quaternion stowed = Quaternion.Euler(heldRotation + new Vector3(StowPitchAngle, 0f, 0f));
            return Quaternion.Slerp(stowed, held, easedProgress);
        }
    }
}
