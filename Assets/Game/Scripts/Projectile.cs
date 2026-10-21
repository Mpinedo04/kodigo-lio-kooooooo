using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class Projectile : MonoBehaviour
    {
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
        private float remainingLife;
        private Vector3 velocity;
        private Vector3 baseScale;
        private bool scaleCaptured;

        public void Launch(GameObject projectileOwner, Team ownerTeam, Vector3 direction, DamageInfo hit, Transform target = null, float newHomingStrength = 0f, int pierce = 0, float scale = 1f)
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
            remainingLife = lifeTime;
            alreadyHit.Clear();
            transform.localScale = baseScale * scale;

            Vector3 forward = direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
            velocity = forward * speed;
            transform.forward = forward;
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

            if (piercesLeft > 0)
            {
                piercesLeft--;
                return;
            }

            Impact();
        }

        private void Impact()
        {
            if (impactPrefab != null)
            {
                Instantiate(impactPrefab, transform.position, Quaternion.identity);
            }

            PrefabPool.Despawn(gameObject);
        }
    }
}
