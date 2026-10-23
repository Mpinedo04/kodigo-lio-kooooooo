using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // What a monster notices: vision range and cone, line of sight (Environment blocks it),
    // very close warriors, and warnings from nearby allies.
    public sealed class EnemyPerception
    {
        private readonly EnemyAI host;

        public EnemyPerception(EnemyAI host)
        {
            this.host = host;
        }

        public static int EnvironmentMask => 1 << GameLayers.Index(GameLayers.Environment);

        private Vector3 EyePosition => host.AimPoint != null ? host.AimPoint.position : host.transform.position + Vector3.up;

        // First sight: range, cone (or very close) and line of sight.
        public bool CanNotice(Health target, float range, float closeSenseRadius)
        {
            if (target == null)
            {
                return false;
            }

            Vector3 point = AggroTarget.AimPositionOf(target);
            float distance = Vector3.Distance(host.transform.position, target.transform.position);
            if (distance > range)
            {
                return false;
            }

            bool close = distance <= closeSenseRadius;
            if (!close && !InVisionCone(host.transform.position, host.transform.forward, target.transform.position, range, host.Definition.visionAngle))
            {
                return false;
            }

            return HasLineOfSight(EyePosition, point);
        }

        // Once a target is known only range and line of sight matter (no cone).
        public bool CanTrack(Health target, float range)
        {
            if (target == null)
            {
                return false;
            }

            return Vector3.Distance(host.transform.position, target.transform.position) <= range
                && HasLineOfSight(EyePosition, AggroTarget.AimPositionOf(target));
        }

        public bool CanShoot(Health target)
        {
            Vector3 origin = host.FirePoint != null ? host.FirePoint.position : EyePosition;
            return target != null && HasLineOfSight(origin, AggroTarget.AimPositionOf(target));
        }

        // Flat (yaw only) cone so flyers still see warriors below them.
        public static bool InVisionCone(Vector3 origin, Vector3 forward, Vector3 point, float range, float fullAngleDegrees)
        {
            Vector3 offset = point - origin;
            if (offset.sqrMagnitude > range * range)
            {
                return false;
            }

            if (fullAngleDegrees >= 360f)
            {
                return true;
            }

            Vector3 flatOffset = Vector3.ProjectOnPlane(offset, Vector3.up);
            Vector3 flatForward = Vector3.ProjectOnPlane(forward, Vector3.up);
            if (flatOffset.sqrMagnitude < 0.0001f || flatForward.sqrMagnitude < 0.0001f)
            {
                return true;
            }

            return Vector3.Angle(flatForward, flatOffset) <= fullAngleDegrees * 0.5f;
        }

        public static bool HasLineOfSight(Vector3 from, Vector3 to)
        {
            return !Physics.Linecast(from, to, EnvironmentMask, QueryTriggerInteraction.Ignore);
        }

        // Warns idle allies around the source about a target.
        public static int AlertAllies(EnemyAI source, Health target, float radius)
        {
            if (source == null || target == null || radius <= 0f)
            {
                return 0;
            }

            int alerted = 0;
            IReadOnlyList<EnemyAI> all = EnemyAI.All;
            for (int i = 0; i < all.Count; i++)
            {
                EnemyAI ally = all[i];
                if (ally == null || ally == source || ally.Target != null)
                {
                    continue;
                }

                if (Vector3.Distance(ally.transform.position, source.transform.position) <= radius && ally.Alert(target))
                {
                    alerted++;
                }
            }

            return alerted;
        }
    }
}
