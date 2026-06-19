using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    public sealed class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private AttackStyle attackStyle = AttackStyle.Ranged;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private LayerMask enemyMask = ~0;
        [SerializeField] private float basicDamage = 20f;
        [SerializeField] private float specialDamage = 35f;
        [SerializeField] private float meleeRadius = 2.2f;
        [SerializeField] private float basicCooldown = 0.28f;
        [SerializeField] private float specialCooldown = 1.6f;

        private LockOnSystem lockOn;
        private float basicTimer;
        private float specialTimer;

        public void ApplyDefinition(CharacterDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            attackStyle = definition.attackStyle;
            basicDamage = definition.basicDamage;
            specialDamage = definition.specialDamage;
            basicCooldown = definition.basicCooldown;
            specialCooldown = definition.specialCooldown;
        }

        public void Configure(AttackStyle style, Projectile projectile, LockOnSystem targeter)
        {
            attackStyle = style;
            projectilePrefab = projectile;
            lockOn = targeter;
        }

        private void Awake()
        {
            if (attackOrigin == null)
            {
                attackOrigin = transform;
            }
        }

        private void Update()
        {
            basicTimer -= Time.deltaTime;
            specialTimer -= Time.deltaTime;
        }

        public void BasicAttack()
        {
            if (basicTimer > 0f)
            {
                return;
            }

            basicTimer = basicCooldown;

            if (attackStyle == AttackStyle.Melee)
            {
                DoMelee(basicDamage, meleeRadius, 0f);
                return;
            }

            FireProjectile(basicDamage, 0f);
        }

        public void SpecialAttack()
        {
            if (specialTimer > 0f)
            {
                return;
            }

            specialTimer = specialCooldown;

            switch (attackStyle)
            {
                case AttackStyle.Melee:
                    DoMelee(specialDamage, meleeRadius * 1.45f, 0.45f);
                    break;
                case AttackStyle.Telekinetic:
                    DoMelee(specialDamage, meleeRadius * 2.2f, 0.8f);
                    break;
                case AttackStyle.Support:
                    DoMelee(specialDamage * 0.65f, meleeRadius * 1.8f, 0.2f);
                    break;
                case AttackStyle.Operator:
                    FireProjectile(specialDamage * 0.5f, 0f);
                    break;
                default:
                    FireProjectile(specialDamage, -8f);
                    FireProjectile(specialDamage, 0f);
                    FireProjectile(specialDamage, 8f);
                    break;
            }
        }

        private void FireProjectile(float damage, float yawOffset)
        {
            if (projectilePrefab == null)
            {
                return;
            }

            Transform target = lockOn != null ? lockOn.CurrentTarget : null;
            Vector3 direction = target != null
                ? (target.position + Vector3.up - attackOrigin.position).normalized
                : Quaternion.Euler(0f, yawOffset, 0f) * transform.forward;

            Projectile projectile = Instantiate(projectilePrefab, attackOrigin.position + direction * 0.7f, Quaternion.LookRotation(direction));
            projectile.Launch(gameObject, direction, damage, false, target, target != null ? 4f : 0f);
        }

        private void DoMelee(float damage, float radius, float forwardBoost)
        {
            Vector3 center = transform.position + transform.forward * (1.4f + forwardBoost) + Vector3.up;
            Collider[] hits = Physics.OverlapSphere(center, radius, enemyMask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < hits.Length; i++)
            {
                if (!hits[i].CompareTag("Enemy") && hits[i].GetComponentInParent<EnemyAI>() == null)
                {
                    continue;
                }

                WeakPoint weakPoint = hits[i].GetComponent<WeakPoint>();
                if (weakPoint != null)
                {
                    weakPoint.ApplyDamage(damage, gameObject);
                    continue;
                }

                Health health = hits[i].GetComponentInParent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage, gameObject);
                }
            }
        }
    }
}
