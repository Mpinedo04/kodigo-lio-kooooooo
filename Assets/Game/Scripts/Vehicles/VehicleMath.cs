using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Pure vehicle maths (steering, speed, ram, drift, flight pitch) so it can be tested without playing.
    public static class VehicleMath
    {
        // Yaw (degrees) turned toward a flat direction by at most maxDegrees.
        public static float SteerYaw(float currentYaw, Vector3 desired, float maxDegrees)
        {
            if (desired.x * desired.x + desired.z * desired.z < 0.0001f)
            {
                return currentYaw;
            }

            float targetYaw = Mathf.Atan2(desired.x, desired.z) * Mathf.Rad2Deg;
            return Mathf.MoveTowardsAngle(currentYaw, targetYaw, Mathf.Max(0f, maxDegrees));
        }

        // Fast vehicles turn wider: the turn rate blends toward highSpeedFactor at top speed.
        public static float TurnRate(float baseRate, float highSpeedFactor, float speed01)
        {
            return baseRate * Mathf.Lerp(1f, highSpeedFactor, Mathf.Clamp01(speed01));
        }

        // Throttle (0..1) times cruise or boost speed, slower while the nose points away from the steering.
        // alignment = dot(nose, steering direction).
        public static float TargetSpeed(float cruiseSpeed, float boostMultiplier, bool boosting, float throttle, float alignment, float cornerSpeedFactor)
        {
            float top = boosting ? cruiseSpeed * Mathf.Max(1f, boostMultiplier) : cruiseSpeed;
            float corner = Mathf.Lerp(Mathf.Clamp01(cornerSpeedFactor), 1f, Mathf.Clamp01(alignment));
            return top * Mathf.Clamp01(throttle) * corner;
        }

        public static float ApproachSpeed(float current, float target, float acceleration, float drag, float deltaTime)
        {
            float rate = target > current ? acceleration : drag;
            return Mathf.MoveTowards(current, target, Mathf.Max(0f, rate) * deltaTime);
        }

        // 0 below the minimum speed, then 60%..100% of the base damage up to top speed.
        public static float RamDamage(float baseDamage, float speed, float minSpeed, float topSpeed)
        {
            if (baseDamage <= 0f || speed < minSpeed)
            {
                return 0f;
            }

            float t = topSpeed > minSpeed ? Mathf.InverseLerp(minSpeed, topSpeed, speed) : 1f;
            return baseDamage * Mathf.Lerp(0.6f, 1f, t);
        }

        // Strength (0..1) of the mini-boost earned by a drift that lasted driftSeconds.
        public static float DriftBoostStrength(float driftSeconds, float fullChargeTime, float minSeconds = 0.3f)
        {
            if (driftSeconds < minSeconds)
            {
                return 0f;
            }

            return fullChargeTime > 0f ? Mathf.Clamp01(driftSeconds / fullChargeTime) : 1f;
        }

        // -1..1 climb request from the camera pitch (degrees, + = looking up), with a dead zone around level flight.
        public static float ClimbFromPitch(float pitch, float neutralPitch, float deadZone, float range)
        {
            float offset = pitch - neutralPitch;
            float beyond = Mathf.Abs(offset) - Mathf.Max(0f, deadZone);
            if (beyond <= 0f || range <= 0f)
            {
                return 0f;
            }

            return Mathf.Sign(offset) * Mathf.Clamp01(beyond / range);
        }

        // Index of the closest position within maxRange, or -1.
        public static int PickNearest(Vector3 origin, IReadOnlyList<Vector3> positions, float maxRange)
        {
            int best = -1;
            float bestDistance = maxRange * maxRange;
            for (int i = 0; i < positions.Count; i++)
            {
                float distance = (positions[i] - origin).sqrMagnitude;
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }

            return best;
        }
    }
}
