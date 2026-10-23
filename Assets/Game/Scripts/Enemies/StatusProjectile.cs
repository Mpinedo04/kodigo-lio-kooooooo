using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Blok's freezing laser and prison ray: a slow shot that, when it actually wounds a warrior,
    // applies a slow or a root through StatusEffects. Environment stops it.
    public sealed class StatusProjectile : MonoBehaviour
    {
        private const float Radius = 0.3f;
        private const float Lifetime = 3f;

        private readonly HashSet<Health> alreadyHit = new HashSet<Health>();
        private GameObject owner;
        private DamageInfo damage;
        private Vector3 velocity;
        private float slowMultiplier;
        private float slowDuration;
        private float rootDuration;
        private float age;
        private int mask;

        public static StatusProjectile Launch(EnemyAI host, EnemyAttackDefinition attack, Vector3 direction, Color color, float slowMultiplier, float slowDuration, float rootDuration)
        {
            GameObject shot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shot.name = attack.type + "Shot";
            LowPolyMesh.Facet(shot, PrimitiveType.Sphere);
            EnemyVisuals.RemoveCollider(shot);
            shot.transform.position = EnemyAttacks.FireOrigin(host);
            shot.transform.localScale = Vector3.one * (rootDuration > 0f ? 0.55f : 0.4f);
            EnemyVisuals.Tint(shot, color);
            shot.layer = GameLayers.Index(GameLayers.EnemyAttack);

            StatusProjectile projectile = shot.AddComponent<StatusProjectile>();
            projectile.owner = host.gameObject;
            projectile.damage = EnemyAttacks.Hit(host, attack, DamageType.Laser, direction);
            projectile.velocity = direction * Mathf.Max(1f, attack.projectileSpeed);
            projectile.slowMultiplier = slowMultiplier;
            projectile.slowDuration = slowDuration;
            projectile.rootDuration = rootDuration;
            projectile.mask = ~((1 << GameLayers.Index(GameLayers.Enemy)) | (1 << GameLayers.Index(GameLayers.EnemyAttack)) | (1 << GameLayers.Index(GameLayers.PlayerAttack)));
            return projectile;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            age += deltaTime;
            if (age > Lifetime)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 step = velocity * deltaTime;
            float distance = step.magnitude;
            if (distance > 0f && Physics.SphereCast(transform.position, Radius, step / distance, out RaycastHit hit, distance, mask, QueryTriggerInteraction.Ignore))
            {
                transform.position = hit.point;
                OnHit(hit.collider);
                return;
            }

            transform.position += step;
        }

        private void OnHit(Collider other)
        {
            if (owner != null && other.transform.IsChildOf(owner.transform))
            {
                return;
            }

            if (!GameLayers.Is(other.gameObject, GameLayers.Environment))
            {
                Health target = ResolveHealth(other);
                if (target != null && target.Team != Team.Enemy)
                {
                    float before = target.Current;
                    DamageInfo hit = damage;
                    hit.point = transform.position;
                    HitResolver.ApplyToCollider(other, hit, Team.Enemy, alreadyHit);
                    if (target.Current < before)
                    {
                        ApplyStatus(target.gameObject);
                    }
                }
            }

            Destroy(gameObject);
        }

        private void ApplyStatus(GameObject target)
        {
            StatusEffects effects = StatusEffects.For(target);
            if (slowDuration > 0f)
            {
                effects.ApplySlow(slowMultiplier, slowDuration);
            }

            if (rootDuration > 0f)
            {
                effects.ApplyRoot(rootDuration);
            }
        }

        private static Health ResolveHealth(Collider other)
        {
            if (other.TryGetComponent(out WeakPoint weakPoint))
            {
                return weakPoint.Target;
            }

            if (other.TryGetComponent(out Hurtbox hurtbox))
            {
                return hurtbox.Owner;
            }

            return other.GetComponentInParent<Health>();
        }
    }
}
