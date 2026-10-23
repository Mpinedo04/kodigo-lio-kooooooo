using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class Projectile : MonoBehaviour
    {
        private static readonly List<Projectile> Flying = new List<Projectile>();
        private static readonly Collider[] ExplosionBuffer = new Collider[32];

        [SerializeField] private float speed = 24f;
        [SerializeField] private float lifeTime = 3f;
        [Tooltip("Downward acceleration (0 = straight laser).")]
        [SerializeField] private float gravity;
        [SerializeField] private GameObject impactPrefab;

        private readonly HashSet<Health> alreadyHit = new HashSet<Health>();
        private GameObject owner;
        private Team team;
        private DamageInfo damage;
        private Transform homingTarget;
        private float homingStrength;
        private int piercesLeft;
        private float explosionRadius;
        private float remainingLife;
        private Vector3 velocity;
        private Vector3 baseScale;
        private bool scaleCaptured;

        // Shots in flight (Odd's Future Vision draws where they go).
        public static IReadOnlyList<Projectile> All => Flying;

        public Vector3 Velocity => velocity;
        public Team Team => team;

        // explosionRadius > 0: the shot bursts on impact and hurts everything around (Aelita's charged field).
        public void Launch(GameObject projectileOwner, Team ownerTeam, Vector3 direction, DamageInfo hit, Transform target = null, float newHomingStrength = 0f, int pierce = 0, float scale = 1f, float newExplosionRadius = 0f, float speedMultiplier = 1f)
        {
            if (!scaleCaptured)
            {
                baseScale = transform.localScale;
                scaleCaptured = true;
            }

            owner = projectileOwner;
            team = ownerTeam;
            damage = hit;
            homingTarget = target;
            homingStrength = newHomingStrength;
            piercesLeft = Mathf.Max(0, pierce);
            explosionRadius = Mathf.Max(0f, newExplosionRadius);
            remainingLife = lifeTime;
            alreadyHit.Clear();
            transform.localScale = baseScale * scale;

            Vector3 forward = direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
            velocity = forward * speed * Mathf.Max(0.1f, speedMultiplier);
            transform.forward = forward;

            // Pooled shots must not drag their old trail across the level.
            if (TryGetComponent(out TrailRenderer trail))
            {
                trail.Clear();
            }
        }

        private void OnEnable()
        {
            Flying.Add(this);
        }

        private void OnDisable()
        {
            Flying.Remove(this);
        }

        private void Update()
        {
            if (homingTarget != null && homingTarget.gameObject.activeInHierarchy)
            {
                Vector3 desired = (homingTarget.position - transform.position).normalized * velocity.magnitude;
                velocity = Vector3.Slerp(velocity, desired, Time.deltaTime * homingStrength);
            }

            velocity += Vector3.down * gravity * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            if (velocity.sqrMagnitude > 0.001f)
            {
                transform.forward = velocity;
            }

            remainingLife -= Time.deltaTime;
            if (remainingLife <= 0f)
            {
                PrefabPool.Despawn(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            if (owner != null && other.transform.IsChildOf(owner.transform))
            {
                return;
            }

            // Walls and floors stop shots.
            if (GameLayers.Is(other.gameObject, GameLayers.Environment) && !other.isTrigger)
            {
                Impact();
                return;
            }

            DamageInfo hit = damage;
            hit.point = transform.position;
            hit.direction = velocity;
            if (HitResolver.ApplyToCollider(other, hit, team, alreadyHit) == null)
            {
                return;
            }

            if (piercesLeft > 0 && explosionRadius <= 0f)
            {
                piercesLeft--;
                return;
            }

            Impact();
        }

        private void Impact()
        {
            if (explosionRadius > 0f)
            {
                Explode();
            }

            if (impactPrefab != null)
            {
                Instantiate(impactPrefab, transform.position, Quaternion.identity);
            }

            PrefabPool.Despawn(gameObject);
        }

        private void Explode()
        {
            DamageInfo blast = damage;
            blast.point = transform.position;
            blast.direction = velocity;
            int count = Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, ExplosionBuffer, Physics.AllLayers, QueryTriggerInteraction.Collide);
            HitResolver.ApplyToColliders(ExplosionBuffer, count, blast, team, alreadyHit, false);
            DevirtualizeEffect.Spawn(transform.position, new Color(1f, 0.45f, 0.8f));
        }
    }
}
