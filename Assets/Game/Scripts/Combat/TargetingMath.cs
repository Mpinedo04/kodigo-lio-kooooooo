using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Pure targeting helpers (lock-on choice, target switching, aim assist) so they can be unit tested.
    public static class TargetingMath
    {
        // Lower is better: mostly how close to the centre of the camera, a little how near.
        public static float LockOnScore(Vector3 cameraPosition, Vector3 cameraForward, Vector3 ownerPosition, Vector3 candidate)
        {
            float angle = Vector3.Angle(cameraForward, candidate - cameraPosition);
            return angle + Vector3.Distance(ownerPosition, candidate) * 0.5f;
        }

        public static int PickBest(Vector3 cameraPosition, Vector3 cameraForward, Vector3 ownerPosition, IList<Vector3> candidates)
        {
            int best = -1;
            float bestScore = float.MaxValue;
            for (int i = 0; i < candidates.Count; i++)
            {
                float score = LockOnScore(cameraPosition, cameraForward, ownerPosition, candidates[i]);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = i;
                }
            }

            return best;
        }

        // Next candidate to the right (direction > 0) or left (direction < 0) of the current one, seen from the camera.
        public static int PickNext(Vector3 cameraPosition, Vector3 current, IList<Vector3> candidates, int direction)
        {
            Vector3 toCurrent = Vector3.ProjectOnPlane(current - cameraPosition, Vector3.up);
            int best = -1;
            float bestAngle = float.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                Vector3 toCandidate = Vector3.ProjectOnPlane(candidates[i] - cameraPosition, Vector3.up);
                float signed = Vector3.SignedAngle(toCurrent, toCandidate, Vector3.up);
                if (signed * direction <= 0.5f)
                {
                    continue;
                }

                float magnitude = Mathf.Abs(signed);
                if (magnitude < bestAngle)
                {
                    bestAngle = magnitude;
                    best = i;
                }
            }

            return best;
        }

        // Candidate inside the aim cone that needs the smallest correction, or -1.
        public static int PickAimAssist(Vector3 origin, Vector3 aimDirection, IList<Vector3> candidates, float maxAngle, float maxDistance)
        {
            int best = -1;
            float bestAngle = maxAngle;
            for (int i = 0; i < candidates.Count; i++)
            {
                Vector3 toCandidate = candidates[i] - origin;
                if (toCandidate.magnitude > maxDistance)
                {
                    continue;
                }

                float angle = Vector3.Angle(Vector3.ProjectOnPlane(aimDirection, Vector3.up), Vector3.ProjectOnPlane(toCandidate, Vector3.up));
                if (angle <= bestAngle)
                {
                    bestAngle = angle;
                    best = i;
                }
            }

            return best;
        }
    }
}
