using UnityEngine;

namespace CodeLyokoFanGame
{
    // Practice target for the vehicle track: a cube on the Enemy team that comes back after a while.
    // With a projectile it also shoots slow lasers at nearby warriors (to test vehicle health).
    public sealed class TargetDummy : MonoBehaviour
    {
        [SerializeField] private Health target;
        [SerializeField] private float respawnDelay = 3f;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private float fireInterval = 2.2f;
        [SerializeField] private float fireRange = 28f;
        [SerializeField] private float damage = 12f;

        private float respawnAt = -1f;
        private float fireTimer;

        public void Configure(Health body, Projectile projectile)
        {
            target = body;
            projectilePrefab = projectile;
        }

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponentInChildren<Health>(true);
            }

            if (target != null)
            {
                target.SetTeam(Team.Enemy);
                target.SetDestroyOnDeath(false);
                target.Died += OnDied;
            }

            fireTimer = fireInterval;
        }

        private void OnDestroy()
        {
            if (target != null)
            {
                target.Died -= OnDied;
            }
        }

        private void OnDied(Health health)
        {
            respawnAt = Time.time + respawnDelay;
        }

        private void Update()
        {
            if (target == null)
            {
                return;
            }

            if (respawnAt >= 0f)
            {
                if (Time.time >= respawnAt)
                {
                    respawnAt = -1f;
                    target.gameObject.SetActive(true);
                    target.ResetHealth();
                }

                return;
            }

            TickFire(Time.deltaTime);
        }

        private void TickFire(float deltaTime)
        {
            if (projectilePrefab == null)
            {
                return;
            }

            fireTimer -= deltaTime;
            if (fireTimer > 0f)
            {
                return;
            }

            fireTimer = fireInterval;
            Transform victim = ClosestWarrior();
            if (victim == null)
            {
                return;
            }

            Vector3 origin = target.transform.position + Vector3.up * 0.8f;
            Vector3 direction = (victim.position + Vector3.up - origin).normalized;
            DamageInfo info = new DamageInfo
            {
                amount = damage,
                type = DamageType.Laser,
                source = target.gameObject,
                sourceTeam = Team.Enemy,
                direction = direction,
                knockback = 2f
            };

            Projectile projectile = PrefabPool.Spawn(projectilePrefab, origin + direction * 0.8f, Quaternion.LookRotation(direction));
            projectile.Launch(target.gameObject, Team.Enemy, direction, info);
        }

        private Transform ClosestWarrior()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            Transform best = null;
            float bestDistance = fireRange * fireRange;
            for (int i = 0; i < players.Length; i++)
            {
                float distance = (players[i].transform.position - target.transform.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = players[i].transform;
                }
            }

            return best;
        }
    }
}
