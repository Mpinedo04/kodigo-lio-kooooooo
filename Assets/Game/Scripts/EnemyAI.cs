using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    [RequireComponent(typeof(Health))]
    public sealed class EnemyAI : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private EnemyKind kind = EnemyKind.Kankrelat;
        [SerializeField] private EnemyBehaviorStyle behaviorStyle = EnemyBehaviorStyle.SwarmShooter;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform aimPoint;
        [SerializeField] private WeakPoint weakPoint;
        [SerializeField] private float detectionRange = 24f;
        [SerializeField] private float attackRange = 18f;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float projectileDamage = 10f;
        [SerializeField] private float fireCooldown = 1.2f;
        [SerializeField] private float specialCooldown = 4f;
        [SerializeField] private float contactDamage;
        [SerializeField] private float hoverHeight;
        [SerializeField] private bool weakPointAlwaysExposed = true;

        private Transform player;
        private Vector3 spawnPosition;
        private float fireTimer;
        private float specialTimer;
        private float weakPointExposeTimer;

        public EnemyKind Kind => kind;
        public Transform AimPoint => aimPoint;

        public void Configure(EnemyDefinition newDefinition, Projectile projectile)
        {
            definition = newDefinition;
            projectilePrefab = projectile;
            ApplyDefinition(definition);
        }

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
            ApplyDefinition(definition);
        }

        private void Update()
        {
            fireTimer -= Time.deltaTime;
            specialTimer -= Time.deltaTime;
            weakPointExposeTimer -= Time.deltaTime;
            AcquirePlayer();
            UpdateWeakPointExposure();

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

            TickBehavior(distance, flatDirection, toPlayer.normalized);
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

        private void TickBehavior(float distance, Vector3 flatDirection, Vector3 aimDirection)
        {
            switch (behaviorStyle)
            {
                case EnemyBehaviorStyle.Turret:
                    if (distance <= attackRange && fireTimer <= 0f)
                    {
                        Fire(aimDirection);
                    }
                    break;
                case EnemyBehaviorStyle.ArmoredBurst:
                    MoveIfFar(distance, flatDirection, attackRange * 0.78f);
                    if (distance <= attackRange && fireTimer <= 0f)
                    {
                        weakPointExposeTimer = 1.15f;
                        Fire(aimDirection);
                    }
                    break;
                case EnemyBehaviorStyle.EliteShooter:
                    Strafe(flatDirection);
                    if (distance <= attackRange && fireTimer <= 0f)
                    {
                        Fire(aimDirection);
                    }
                    break;
                case EnemyBehaviorStyle.Ambusher:
                    MoveIfFar(distance, flatDirection, 2.2f);
                    if (distance <= 3f)
                    {
                        DamagePlayer(contactDamage > 0f ? contactDamage : projectileDamage);
                    }
                    break;
                case EnemyBehaviorStyle.MineLayer:
                    Hover();
                    Strafe(flatDirection);
                    if (distance <= attackRange && fireTimer <= 0f)
                    {
                        Fire(aimDirection);
                    }
                    if (specialTimer <= 0f)
                    {
                        specialTimer = specialCooldown;
                        Fire(-transform.forward + Vector3.down * 0.15f);
                    }
                    break;
                case EnemyBehaviorStyle.DigitalSeaChaser:
                    Hover();
                    MoveIfFar(distance, flatDirection, 4f);
                    if (distance <= attackRange && fireTimer <= 0f)
                    {
                        Fire(aimDirection);
                    }
                    break;
                case EnemyBehaviorStyle.Grappler:
                    Hover();
                    MoveIfFar(distance, flatDirection, 1.5f);
                    if (distance <= 2.8f)
                    {
                        DamagePlayer(contactDamage > 0f ? contactDamage : projectileDamage);
                    }
                    break;
                case EnemyBehaviorStyle.CaptureField:
                    MoveIfFar(distance, flatDirection, 3.5f);
                    if (distance <= 4f)
                    {
                        DamagePlayer(projectileDamage * 0.4f);
                    }
                    break;
                case EnemyBehaviorStyle.MemoryDrainer:
                    Hover();
                    MoveIfFar(distance, flatDirection, 2.8f);
                    if (distance <= 3.4f)
                    {
                        DamagePlayer(projectileDamage);
                    }
                    break;
                case EnemyBehaviorStyle.RaidBoss:
                    MoveIfFar(distance, flatDirection, attackRange * 0.85f);
                    if (distance <= attackRange && fireTimer <= 0f)
                    {
                        weakPointExposeTimer = 1.8f;
                        Fire(aimDirection);
                    }
                    break;
                case EnemyBehaviorStyle.Duelist:
                    MoveIfFar(distance, flatDirection, 2.6f);
                    if (distance <= 3.5f)
                    {
                        DamagePlayer(contactDamage > 0f ? contactDamage : projectileDamage);
                    }
                    else if (distance <= attackRange && fireTimer <= 0f)
                    {
                        Fire(aimDirection);
                    }
                    break;
                case EnemyBehaviorStyle.Exploder:
                    MoveIfFar(distance, flatDirection, 1.2f);
                    if (distance <= 2f)
                    {
                        DamagePlayerInstant(projectileDamage * 2f);
                        Health health = GetComponent<Health>();
                        if (health != null)
                        {
                            health.TakeDamage(health.Current, gameObject);
                        }
                    }
                    break;
                default:
                    MoveIfFar(distance, flatDirection, attackRange * 0.65f);
                    Hover();
                    if (distance <= attackRange && fireTimer <= 0f)
                    {
                        Fire(aimDirection);
                    }
                    break;
            }
        }

        private void DamagePlayerInstant(float amount)
        {
            if (player == null || amount <= 0f)
            {
                return;
            }

            Health health = player.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(amount, gameObject);
            }
        }

        private void MoveIfFar(float distance, Vector3 flatDirection, float desiredDistance)
        {
            if (distance > desiredDistance)
            {
                transform.position += flatDirection * moveSpeed * Time.deltaTime;
            }
        }

        private void Strafe(Vector3 flatDirection)
        {
            Vector3 strafe = Vector3.Cross(Vector3.up, flatDirection).normalized;
            transform.position += (flatDirection * 0.35f + strafe * Mathf.Sin(Time.time * 1.7f)) * moveSpeed * Time.deltaTime;
            Hover();
        }

        private void Hover()
        {
            if (hoverHeight > 0f)
            {
                transform.position = new Vector3(transform.position.x, hoverHeight + Mathf.Sin(Time.time * 2.4f) * 0.35f, transform.position.z);
            }
        }

        private void DamagePlayer(float amount)
        {
            if (player == null || amount <= 0f)
            {
                return;
            }

            Health health = player.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(amount * Time.deltaTime, gameObject);
            }
        }

        private void UpdateWeakPointExposure()
        {
            if (weakPoint == null)
            {
                weakPoint = GetComponentInChildren<WeakPoint>();
            }

            if (weakPoint != null)
            {
                weakPoint.SetExposed(weakPointAlwaysExposed || weakPointExposeTimer > 0f);
            }
        }

        private void ApplyDefinition(EnemyDefinition enemyDefinition)
        {
            if (enemyDefinition == null)
            {
                return;
            }

            kind = enemyDefinition.kind;
            behaviorStyle = enemyDefinition.behaviorStyle;
            detectionRange = enemyDefinition.detectionRange;
            attackRange = enemyDefinition.attackRange;
            moveSpeed = enemyDefinition.moveSpeed;
            projectileDamage = enemyDefinition.projectileDamage;
            fireCooldown = enemyDefinition.fireCooldown;
            specialCooldown = enemyDefinition.specialCooldown;
            contactDamage = enemyDefinition.contactDamage;
            hoverHeight = enemyDefinition.hoverHeight;
            weakPointAlwaysExposed = enemyDefinition.weakPointAlwaysExposed;

            Health health = GetComponent<Health>();
            if (health != null)
            {
                health.SetMaxHealth(enemyDefinition.maxHealth);
            }

            if (weakPoint == null)
            {
                weakPoint = GetComponentInChildren<WeakPoint>();
            }

            if (weakPoint != null)
            {
                weakPoint.Configure(enemyDefinition.weakPointDamageMultiplier, weakPointAlwaysExposed);
            }
        }

        private void ApplyKindStats()
        {
            Health health = GetComponent<Health>();
            float maxHealth = 60f;
            behaviorStyle = EnemyBehaviorStyle.SwarmShooter;
            weakPointAlwaysExposed = true;

            switch (kind)
            {
                case EnemyKind.Blok:
                    maxHealth = 85f;
                    behaviorStyle = EnemyBehaviorStyle.Turret;
                    detectionRange = 26f;
                    attackRange = 20f;
                    moveSpeed = 1.8f;
                    projectileDamage = 14f;
                    fireCooldown = 1f;
                    break;
                case EnemyKind.Krab:
                    maxHealth = 120f;
                    behaviorStyle = EnemyBehaviorStyle.HeavyWalker;
                    detectionRange = 28f;
                    attackRange = 18f;
                    moveSpeed = 3.2f;
                    projectileDamage = 18f;
                    fireCooldown = 1.15f;
                    break;
                case EnemyKind.Hornet:
                    maxHealth = 55f;
                    behaviorStyle = EnemyBehaviorStyle.FlyingShooter;
                    detectionRange = 30f;
                    attackRange = 22f;
                    moveSpeed = 4.8f;
                    projectileDamage = 12f;
                    fireCooldown = 0.72f;
                    hoverHeight = 4.5f;
                    break;
                case EnemyKind.Megatank:
                    maxHealth = 260f;
                    behaviorStyle = EnemyBehaviorStyle.ArmoredBurst;
                    weakPointAlwaysExposed = false;
                    detectionRange = 34f;
                    attackRange = 24f;
                    moveSpeed = 1.4f;
                    projectileDamage = 34f;
                    fireCooldown = 2.4f;
                    break;
                case EnemyKind.Tarantula:
                    maxHealth = 150f;
                    behaviorStyle = EnemyBehaviorStyle.EliteShooter;
                    detectionRange = 32f;
                    attackRange = 25f;
                    moveSpeed = 2.8f;
                    projectileDamage = 22f;
                    fireCooldown = 0.8f;
                    break;
                case EnemyKind.Creeper:
                    maxHealth = 75f;
                    behaviorStyle = EnemyBehaviorStyle.Ambusher;
                    detectionRange = 22f;
                    attackRange = 12f;
                    moveSpeed = 5.2f;
                    projectileDamage = 18f;
                    contactDamage = 32f;
                    break;
                case EnemyKind.Manta:
                case EnemyKind.BlackManta:
                    maxHealth = kind == EnemyKind.BlackManta ? 180f : 120f;
                    behaviorStyle = EnemyBehaviorStyle.MineLayer;
                    detectionRange = 34f;
                    attackRange = 25f;
                    moveSpeed = 4.2f;
                    projectileDamage = 18f;
                    fireCooldown = 1.1f;
                    hoverHeight = 5.2f;
                    break;
                case EnemyKind.Kongre:
                case EnemyKind.Shark:
                    maxHealth = kind == EnemyKind.Shark ? 120f : 90f;
                    behaviorStyle = EnemyBehaviorStyle.DigitalSeaChaser;
                    detectionRange = 34f;
                    attackRange = 18f;
                    moveSpeed = 5f;
                    projectileDamage = kind == EnemyKind.Shark ? 24f : 16f;
                    hoverHeight = 2.6f;
                    break;
                case EnemyKind.Kalamar:
                    maxHealth = 190f;
                    behaviorStyle = EnemyBehaviorStyle.Grappler;
                    detectionRange = 30f;
                    attackRange = 10f;
                    moveSpeed = 3.4f;
                    projectileDamage = 26f;
                    contactDamage = 42f;
                    hoverHeight = 3.2f;
                    break;
                case EnemyKind.Guardian:
                    maxHealth = 220f;
                    behaviorStyle = EnemyBehaviorStyle.CaptureField;
                    detectionRange = 20f;
                    attackRange = 6f;
                    moveSpeed = 2.2f;
                    projectileDamage = 12f;
                    weakPointAlwaysExposed = false;
                    break;
                case EnemyKind.Scyphozoa:
                    maxHealth = 260f;
                    behaviorStyle = EnemyBehaviorStyle.MemoryDrainer;
                    detectionRange = 30f;
                    attackRange = 5f;
                    moveSpeed = 2.6f;
                    projectileDamage = 20f;
                    hoverHeight = 4.2f;
                    weakPointAlwaysExposed = false;
                    break;
                case EnemyKind.Kolossus:
                    maxHealth = 900f;
                    behaviorStyle = EnemyBehaviorStyle.RaidBoss;
                    detectionRange = 42f;
                    attackRange = 30f;
                    moveSpeed = 1.1f;
                    projectileDamage = 60f;
                    fireCooldown = 3.4f;
                    weakPointAlwaysExposed = false;
                    break;
                case EnemyKind.Specter:
                case EnemyKind.PolymorphicClone:
                case EnemyKind.Zombies:
                    maxHealth = 80f;
                    behaviorStyle = EnemyBehaviorStyle.RealWorldPossessor;
                    detectionRange = 24f;
                    attackRange = 8f;
                    moveSpeed = 3.6f;
                    projectileDamage = 10f;
                    contactDamage = 18f;
                    break;
                case EnemyKind.XanaWilliam:
                    maxHealth = 280f;
                    behaviorStyle = EnemyBehaviorStyle.Duelist;
                    detectionRange = 34f;
                    attackRange = 18f;
                    moveSpeed = 4.2f;
                    projectileDamage = 28f;
                    contactDamage = 45f;
                    break;
                case EnemyKind.ExplodingRoachster:
                    maxHealth = 35f;
                    behaviorStyle = EnemyBehaviorStyle.Exploder;
                    detectionRange = 18f;
                    attackRange = 4f;
                    moveSpeed = 5.5f;
                    projectileDamage = 34f;
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
