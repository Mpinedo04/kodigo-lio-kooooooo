using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Turns "these colliders were hit" into damage: one hit per Health, exposed weak points first,
    // and never against the attacker's own team.
    public static class HitResolver
    {
        private struct Candidate
        {
            public Health health;
            public WeakPoint weakPoint;
            public float multiplier;
        }

        public static int ApplyToColliders(Collider[] colliders, int count, DamageInfo info, Team attackerTeam, ICollection<Health> exclude = null, bool allowWeakPoint = true)
        {
            Dictionary<Health, Candidate> best = new Dictionary<Health, Candidate>();
            for (int i = 0; i < count; i++)
            {
                if (!TryResolve(colliders[i], attackerTeam, allowWeakPoint, out Candidate candidate))
                {
                    continue;
                }

                if (exclude != null && exclude.Contains(candidate.health))
                {
                    continue;
                }

                if (!best.TryGetValue(candidate.health, out Candidate existing) || IsBetter(candidate, existing))
                {
                    best[candidate.health] = candidate;
                }
            }

            foreach (Candidate candidate in best.Values)
            {
                Apply(candidate, info);
                exclude?.Add(candidate.health);
            }

            return best.Count;
        }

        // Single collider (projectiles). Returns the Health that was hit, or null.
        public static Health ApplyToCollider(Collider collider, DamageInfo info, Team attackerTeam, ICollection<Health> exclude = null)
        {
            if (!TryResolve(collider, attackerTeam, true, out Candidate candidate))
            {
                return null;
            }

            if (exclude != null && exclude.Contains(candidate.health))
            {
                return null;
            }

            Apply(candidate, info);
            exclude?.Add(candidate.health);
            return candidate.health;
        }

        public static bool CanHit(Collider collider, Team attackerTeam)
        {
            return TryResolve(collider, attackerTeam, true, out _);
        }

        private static bool TryResolve(Collider collider, Team attackerTeam, bool allowWeakPoint, out Candidate candidate)
        {
            candidate = default;
            if (collider == null)
            {
                return false;
            }

            if (collider.TryGetComponent(out WeakPoint weakPoint))
            {
                if (!weakPoint.IsExposed || weakPoint.Target == null)
                {
                    return false;
                }

                candidate.health = weakPoint.Target;
                candidate.weakPoint = allowWeakPoint ? weakPoint : null;
                candidate.multiplier = allowWeakPoint ? weakPoint.DamageMultiplier : 1f;
            }
            else if (collider.TryGetComponent(out Hurtbox hurtbox))
            {
                candidate.health = hurtbox.Owner;
                candidate.multiplier = hurtbox.DamageMultiplier;
            }
            else
            {
                candidate.health = collider.GetComponentInParent<Health>();
                candidate.multiplier = 1f;
            }

            if (candidate.health == null || candidate.health.IsDead)
            {
                return false;
            }

            return attackerTeam == Team.Neutral || candidate.health.Team != attackerTeam;
        }

        private static bool IsBetter(Candidate candidate, Candidate existing)
        {
            if ((candidate.weakPoint != null) != (existing.weakPoint != null))
            {
                return candidate.weakPoint != null;
            }

            return candidate.multiplier > existing.multiplier;
        }

        private static void Apply(Candidate candidate, DamageInfo info)
        {
            if (candidate.weakPoint != null)
            {
                candidate.weakPoint.ReceiveHit(info);
                return;
            }

            candidate.health.TakeDamage(info.WithAmount(info.amount * candidate.multiplier));
        }
    }
}
