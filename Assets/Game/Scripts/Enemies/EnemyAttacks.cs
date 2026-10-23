using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // The actual hits of monster attacks. All damage goes through DamageInfo + HitResolver.
    public static class EnemyAttacks
    {
        private static readonly Collider[] Buffer = new Collider[32];

        public static void Perform(EnemyAI host, EnemyAttackDefinition attack, Health target)
        {
            switch (attack.type)
            {
                case EnemyAttackType.Melee:
                    Melee(host, attack);
                    break;
                case EnemyAttackType.IceShot:
                    StatusProjectile.Launch(host, attack, AimDirection(host, target), EnemyVisuals.IceBlue, attack.slowMultiplier, attack.statusDuration, 0f);
                    break;
                case EnemyAttackType.PrisonRay:
                    StatusProjectile.Launch(host, attack, AimDirection(host, target), EnemyVisuals.PrisonViolet, 1f, 0f, attack.statusDuration);
                    break;
                case EnemyAttackType.FireRing:
                    FireRingWave.Spawn(host, attack, Hit(host, attack, DamageType.Laser, host.transform.forward));
                    break;
                case EnemyAttackType.BeamHorizontal:
                case EnemyAttackType.BeamVertical:
                    BeamAttack.Spawn(host, attack, Hit(host, attack, DamageType.Laser, host.transform.forward));
                    break;
                default:
                    Laser(host, attack, target);
                    break;
            }
        }

        public static DamageInfo Hit(EnemyAI host, EnemyAttackDefinition attack, DamageType type, Vector3 direction)
        {
            return new DamageInfo
            {
                amount = attack.damage,
                type = type,
                source = host.gameObject,
                sourceTeam = Team.Enemy,
                direction = direction,
                knockback = attack.knockback,
                isHeavy = attack.heavy
            };
        }

        public static Vector3 FireOrigin(EnemyAI host)
        {
            return host.FirePoint != null ? host.FirePoint.position : host.transform.position + Vector3.up;
        }

        public static Vector3 AimDirection(EnemyAI host, Health target)
        {
            Vector3 origin = FireOrigin(host);
            Vector3 aim = target != null ? AggroTarget.AimPositionOf(target) : origin + host.transform.forward;
            Vector3 direction = aim - origin;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : host.transform.forward;
        }

        public static void Laser(EnemyAI host, EnemyAttackDefinition attack, Health target)
        {
            Projectile prefab = attack.projectilePrefab != null ? attack.projectilePrefab.GetComponent<Projectile>() : null;
            if (prefab == null)
            {
                prefab = host.ProjectilePrefab;
            }

            if (prefab == null)
            {
                return;
            }

            Vector3 direction = AimDirection(host, target);
            Projectile projectile = PrefabPool.Spawn(prefab, FireOrigin(host), Quaternion.LookRotation(direction));
            projectile.Launch(host.gameObject, Team.Enemy, direction, Hit(host, attack, DamageType.Laser, direction));
        }

        // Short-range strike in front of the monster (Krabe kick, contact attacks).
        public static int Melee(EnemyAI host, EnemyAttackDefinition attack)
        {
            float reach = Mathf.Max(0.5f, attack.areaSize);
            Vector3 center = host.transform.position + Vector3.up + host.transform.forward * reach * 0.6f;
            int count = Physics.OverlapSphereNonAlloc(center, reach * 0.6f, Buffer, Physics.AllLayers, QueryTriggerInteraction.Collide);
            DamageInfo info = Hit(host, attack, DamageType.Melee, host.transform.forward);
            info.point = center;
            return HitResolver.ApplyToColliders(Buffer, count, info, Team.Enemy);
        }

        // Ground-level hits for areas (fire rings): Player-team Health within the band and low enough.
        public static void HitPlayersWhere(System.Func<Health, bool> isInside, DamageInfo info, Vector3 origin, HashSet<Health> alreadyHit)
        {
            IReadOnlyList<Health> all = Health.All;
            for (int i = all.Count - 1; i >= 0; i--)
            {
                Health health = all[i];
                if (health == null || health.Team == Team.Enemy || health.IsDead || alreadyHit.Contains(health) || !isInside(health))
                {
                    continue;
                }

                alreadyHit.Add(health);
                DamageInfo hit = info;
                hit.point = health.transform.position;
                Vector3 push = Vector3.ProjectOnPlane(health.transform.position - origin, Vector3.up);
                hit.direction = push.sqrMagnitude > 0.001f ? push.normalized : info.direction;
                health.TakeDamage(hit);
            }
        }
    }
}
