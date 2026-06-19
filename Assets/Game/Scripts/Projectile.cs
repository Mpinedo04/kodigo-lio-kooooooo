using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 24f;
        [SerializeField] private float damage = 15f;
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private bool hostileToPlayer;
        [SerializeField] private GameObject impactPrefab;

        private GameObject owner;
        private Transform homingTarget;
        private float homingStrength;

        public void Launch(GameObject projectileOwner, Vector3 direction, float newDamage, bool newHostileToPlayer, Transform target = null, float newHomingStrength = 0f)
        {
            owner = projectileOwner;
            damage = newDamage;
            hostileToPlayer = newHostileToPlayer;
            homingTarget = target;
            homingStrength = newHomingStrength;
            transform.forward = direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
        }

        private void Update()
        {
            if (homingTarget != null)
            {
                Vector3 desired = (homingTarget.position - transform.position).normalized;
                transform.forward = Vector3.Slerp(transform.forward, desired, Time.deltaTime * homingStrength);
            }

            transform.position += transform.forward * speed * Time.deltaTime;
            lifeTime -= Time.deltaTime;

            if (lifeTime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (owner != null && other.transform.IsChildOf(owner.transform))
            {
                return;
            }

            bool hitPlayer = other.CompareTag("Player");
            bool hitEnemy = other.CompareTag("Enemy") || other.GetComponentInParent<EnemyAI>() != null;

            if (hostileToPlayer && !hitPlayer)
            {
                return;
            }

            if (!hostileToPlayer && !hitEnemy)
            {
                return;
            }

            WeakPoint weakPoint = other.GetComponent<WeakPoint>();
            if (weakPoint != null)
            {
                weakPoint.ApplyDamage(damage, owner);
            }
            else
            {
                Health health = other.GetComponentInParent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage, owner);
                }
            }

            if (impactPrefab != null)
            {
                Instantiate(impactPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}
