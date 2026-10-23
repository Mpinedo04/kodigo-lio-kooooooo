using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Shared helpers for abilities: finding enemies, area hits, formations and ground checks.
    public static class AbilityMath
    {
        private static readonly Collider[] Buffer = new Collider[64];
        private static readonly Collider[] Single = new Collider[1];
        private static readonly HashSet<Health> Hit = new HashSet<Health>();

        public static int EnvironmentMask => 1 << GameLayers.Index(GameLayers.Environment);

        public static bool IsEnemy(Health health)
        {
            return health != null && health.Team == Team.Enemy && !health.IsDead && health.gameObject.activeInHierarchy;
        }

        public static Health NearestEnemy(Vector3 position, float range)
        {
            Health best = null;
            float bestDistance = range * range;
            IReadOnlyList<Health> all = Health.All;
            for (int i = 0; i < all.Count; i++)
            {
                if (!IsEnemy(all[i]))
                {
                    continue;
                }

                float distance = (all[i].transform.position - position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = all[i];
                }
            }

            return best;
        }

        public static int CountEnemies(Vector3 position, float range)
        {
            int count = 0;
            IReadOnlyList<Health> all = Health.All;
            for (int i = 0; i < all.Count; i++)
            {
                if (IsEnemy(all[i]) && (all[i].transform.position - position).sqrMagnitude <= range * range)
                {
                    count++;
                }
            }

            return count;
        }

        // Hits every enemy around a point once. pull = true drags them in (Fan Dance), false pushes them out (Impact).
        public static int AreaHit(Vector3 center, float radius, DamageInfo template, bool pull)
        {
            Hit.Clear();
            int count = Physics.OverlapSphereNonAlloc(center, radius, Buffer, Physics.AllLayers, QueryTriggerInteraction.Collide);
            int hits = 0;
            for (int i = 0; i < count; i++)
            {
                Collider collider = Buffer[i];
                Vector3 away = Vector3.ProjectOnPlane(collider.transform.position - center, Vector3.up);
                DamageInfo info = template;
                info.point = collider.ClosestPoint(center);
                info.direction = away.sqrMagnitude > 0.01f ? (pull ? -away.normalized : away.normalized) : Vector3.up;
                // Area blasts do not aim, so the eye of X.A.N.A. counts as body.
                Single[0] = collider;
                hits += HitResolver.ApplyToColliders(Single, 1, info, template.sourceTeam, Hit, false);
            }

            return hits;
        }

        // Three points around a target, 120 degrees apart, the first one on the side of "from" (Triangular).
        public static Vector3[] TriangleAround(Vector3 target, Vector3 from, float radius)
        {
            Vector3 toFrom = Vector3.ProjectOnPlane(from - target, Vector3.up);
            if (toFrom.sqrMagnitude < 0.001f)
            {
                toFrom = Vector3.forward;
            }

            toFrom = toFrom.normalized * radius;
            return new[]
            {
                target + toFrom,
                target + Quaternion.Euler(0f, 120f, 0f) * toFrom,
                target + Quaternion.Euler(0f, 240f, 0f) * toFrom
            };
        }

        public static bool TryFindGround(Vector3 point, float searchHeight, out Vector3 ground)
        {
            if (Physics.Raycast(point + Vector3.up * searchHeight, Vector3.down, out RaycastHit hit, searchHeight * 2f, EnvironmentMask, QueryTriggerInteraction.Ignore))
            {
                ground = hit.point;
                return true;
            }

            ground = point;
            return false;
        }
    }
}
