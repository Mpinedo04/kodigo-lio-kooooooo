using UnityEngine;

namespace CodeLyokoFanGame
{
    public static class MovementMath
    {
        public static Vector3 Accelerate(Vector3 current, Vector3 target, float acceleration, float deltaTime)
        {
            return Vector3.MoveTowards(current, target, acceleration * deltaTime);
        }

        // Stronger gravity while falling, clamped to a terminal speed.
        public static float ApplyGravity(float verticalVelocity, float gravity, float fallMultiplier, float maxFallSpeed, float deltaTime)
        {
            float g = verticalVelocity > 0f ? gravity : gravity * fallMultiplier;
            return Mathf.Max(verticalVelocity + g * deltaTime, -maxFallSpeed);
        }
    }
}
