using UnityEngine;

namespace CodeLyokoFanGame
{
    // A monster thrown (or dropped) by telekinesis: flies on a curve, hurts the monster it hits and itself,
    // and gives control back to its AI when it lands.
    public sealed class ThrownBody : MonoBehaviour
    {
        private const float Gravity = 20f;
        private const float HitRadius = 0.9f;
        private const float MaxFlightTime = 6f;

        private static readonly Collider[] Buffer = new Collider[16];

        private EnemyAI enemy;
        private Health ownHealth;
        private GameObject thrower;
        private Vector3 velocity;
        private float damage;
        private float age;

        public static void Launch(EnemyAI enemy, Vector3 velocity, float damage, GameObject thrower)
        {
            if (!enemy.TryGetComponent(out ThrownBody body))
            {
                body = enemy.gameObject.AddComponent<ThrownBody>();
            }

            body.enemy = enemy;
            body.ownHealth = enemy.GetComponent<Health>();
            body.velocity = velocity;
            body.damage = damage;
            body.thrower = thrower;
            body.age = 0f;
            body.enabled = true;
            enemy.SetSuspended(true);
            TelekinesisState.Mark(enemy.gameObject, true);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            age += dt;
            velocity += Vector3.down * Gravity * dt;
            Vector3 step = velocity * dt;
            float speed = velocity.magnitude;

            if (speed > 4f && HitAnotherMonster())
            {
                Land();
                return;
            }

            if (velocity.y <= 0f && Physics.Raycast(transform.position + Vector3.up * 0.3f, Vector3.down, out RaycastHit ground, Mathf.Max(0.35f, -step.y + 0.35f), AbilityMath.EnvironmentMask, QueryTriggerInteraction.Ignore))
            {
                transform.position = ground.point;
                if (speed > 8f)
                {
                    HurtSelf(damage * 0.5f);
                }

                Land();
                return;
            }

            Vector3 horizontal = new Vector3(step.x, 0f, step.z);
            if (horizontal.sqrMagnitude > 0.0001f && Physics.Raycast(transform.position + Vector3.up * 0.6f, horizontal.normalized, horizontal.magnitude + 0.5f, AbilityMath.EnvironmentMask, QueryTriggerInteraction.Ignore))
            {
                HurtSelf(damage * 0.5f);
                velocity = new Vector3(0f, Mathf.Min(velocity.y, 0f), 0f);
                step = velocity * dt;
            }

            transform.position += step;
            if (age >= MaxFlightTime)
            {
                Land();
            }
        }

        private bool HitAnotherMonster()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position + Vector3.up * 0.6f, HitRadius, Buffer, Physics.AllLayers, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                Health other = Buffer[i].GetComponentInParent<Health>();
                if (other == null || other == ownHealth || !AbilityMath.IsEnemy(other))
                {
                    continue;
                }

                DamageInfo impact = Impact(damage);
                HitResolver.ApplyToCollider(Buffer[i], impact, Team.Player);
                HurtSelf(damage);
                CameraRig.Shake(0.3f);
                return true;
            }

            return false;
        }

        private DamageInfo Impact(float amount)
        {
            return new DamageInfo
            {
                amount = amount,
                type = DamageType.Ability,
                source = thrower,
                sourceTeam = Team.Player,
                point = transform.position,
                direction = velocity,
                knockback = 6f,
                isHeavy = true
            };
        }

        private void HurtSelf(float amount)
        {
            if (ownHealth != null && amount > 0f)
            {
                ownHealth.TakeDamage(Impact(amount));
            }
        }

        private void Land()
        {
            TelekinesisState.Mark(gameObject, false);
            if (enemy != null)
            {
                enemy.SetSuspended(false);
            }

            enabled = false;
        }

        private void OnDisable()
        {
            TelekinesisState.Mark(gameObject, false);
        }
    }
}
