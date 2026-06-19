using UnityEngine;

namespace CodeLyokoFanGame
{
    [RequireComponent(typeof(Health))]
    public sealed class EnemyAI : MonoBehaviour
    {
        [SerializeField] private EnemyKind kind = EnemyKind.Kankrelat;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform aimPoint;
        [SerializeField] private float detectionRange = 24f;
        [SerializeField] private float attackRange = 18f;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float projectileDamage = 10f;
        [SerializeField] private float fireCooldown = 1.2f;
        [SerializeField] private float hoverHeight;

        private Transform player;
        private Vector3 spawnPosition;
        private float fireTimer;

        public EnemyKind Kind => kind;
        public Transform AimPoint => aimPoint;

        public void Configure(EnemyKind newKind, Projectile projectile)
        {
            kind = newKind;
            projectilePrefab = projectile;
            ApplyKindStats();
        }

        private void Awake()
        {
            spawnPosition = transform.position;

            if (firePoint == null)
            {
                firePoint = transform;
            }

            if (aimPoint == null)
            {
                aimPoint = transform;
            }

            ApplyKindStats();
        }

        private void Update()
        {
            fireTimer -= Time.deltaTime;
            AcquirePlayer();

            if (player == null)
            {
                Patrol();
                return;
            }

            Vector3 toPlayer = player.position - transform.position;
            float distance = toPlayer.magnitude;
            Vector3 flatDirection = Vector3.ProjectOnPlane(toPlayer, Vector3.up).normalized;

            if (flatDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flatDirection), Time.deltaTime * 8f);
            }

            if (distance > attackRange * 0.65f)
            {
                transform.position += flatDirection * moveSpeed * Time.deltaTime;
            }

            if (hoverHeight > 0f)
            {
                transform.position = new Vector3(transform.position.x, hoverHeight + Mathf.Sin(Time.time * 2.4f) * 0.35f, transform.position.z);
            }

            if (distance <= attackRange && fireTimer <= 0f)
            {
                Fire(toPlayer.normalized);
            }
        }

        private void AcquirePlayer()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            float best = detectionRange * detectionRange;
            Transform bestTarget = null;

            for (int i = 0; i < players.Length; i++)
            {
                if (!players[i].activeInHierarchy)
                {
                    continue;
                }

                float distance = Vector3.SqrMagnitude(players[i].transform.position - transform.position);
                if (distance < best)
                {
                    best = distance;
                    bestTarget = players[i].transform;
                }
            }

            player = bestTarget;
        }

        private void Patrol()
        {
            Vector3 orbit = spawnPosition + new Vector3(Mathf.Sin(Time.time * 0.45f), 0f, Mathf.Cos(Time.time * 0.45f)) * 2.2f;
            transform.position = Vector3.MoveTowards(transform.position, orbit, moveSpeed * 0.45f * Time.deltaTime);
        }

        private void Fire(Vector3 direction)
        {
            if (projectilePrefab == null)
            {
                return;
            }

            fireTimer = fireCooldown;
            Projectile projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
            projectile.Launch(gameObject, direction, projectileDamage, true, player, kind == EnemyKind.Hornet ? 2.5f : 0f);
        }

        private void ApplyKindStats()
        {
            Health health = GetComponent<Health>();
            float maxHealth = 60f;

            switch (kind)
            {
                case EnemyKind.Blok:
                    maxHealth = 85f;
                    detectionRange = 26f;
                    attackRange = 20f;
                    moveSpeed = 1.8f;
                    projectileDamage = 14f;
                    fireCooldown = 1f;
                    break;
                case EnemyKind.Krab:
                    maxHealth = 120f;
                    detectionRange = 28f;
                    attackRange = 18f;
                    moveSpeed = 3.2f;
                    projectileDamage = 18f;
                    fireCooldown = 1.15f;
                    break;
                case EnemyKind.Hornet:
                    maxHealth = 55f;
                    detectionRange = 30f;
                    attackRange = 22f;
                    moveSpeed = 4.8f;
                    projectileDamage = 12f;
                    fireCooldown = 0.72f;
                    hoverHeight = 4.5f;
                    break;
                case EnemyKind.Megatank:
                    maxHealth = 260f;
                    detectionRange = 34f;
                    attackRange = 24f;
                    moveSpeed = 1.4f;
                    projectileDamage = 34f;
                    fireCooldown = 2.4f;
                    break;
                default:
                    detectionRange = 22f;
                    attackRange = 15f;
                    moveSpeed = 3.8f;
                    projectileDamage = 9f;
                    fireCooldown = 0.95f;
                    break;
            }

            if (health != null)
            {
                health.SetMaxHealth(maxHealth);
            }
        }
    }
}
