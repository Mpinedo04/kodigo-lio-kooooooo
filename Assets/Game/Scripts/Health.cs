using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class Health : MonoBehaviour
    {
        private static readonly List<Health> Active = new List<Health>();

        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private Team team = Team.Neutral;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField] private float destroyDelay = 0.05f;
        [Tooltip("Invulnerability after taking a hit (warriors flicker during it).")]
        [SerializeField] private float hitInvulnerability;
        [SerializeField] private bool devirtualizeEffect = true;

        public event Action<Health, DamageInfo> Damaged;
        public event Action<Health, float> Healed;
        public event Action<Health> Died;

        // Raised for any Health taking damage (damage numbers, statistics).
        public static event Action<Health, DamageInfo, float> AnyDamaged;

        public static IReadOnlyList<Health> All => Active;

        public float Current { get; private set; }
        public float Max => maxHealth;
        public Team Team => team;
        public bool IsDead => Current <= 0f;
        public bool Invulnerable { get; set; }
        public bool IsRecovering => Time.time < hitRecoveryUntil;
        public bool IsInvulnerable => Invulnerable || Time.time < invulnerableUntil || IsRecovering;
        public DamageInfo LastDamage { get; private set; }

        private float invulnerableUntil;
        private float hitRecoveryUntil;

        // Short invulnerability windows (dodge i-frames) that do not fight the permanent flag.
        public void GrantInvulnerability(float seconds)
        {
            invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + seconds);
        }

        public void SetDestroyOnDeath(bool value)
        {
            destroyOnDeath = value;
        }

        public void SetTeam(Team value)
        {
            team = value;
        }

        public void SetHitInvulnerability(float seconds)
        {
            hitInvulnerability = Mathf.Max(0f, seconds);
        }

        private void Awake()
        {
            Current = maxHealth;
        }

        private void OnEnable()
        {
            if (!Active.Contains(this))
            {
                Active.Add(this);
            }
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }

        public void ResetHealth()
        {
            Current = maxHealth;
        }

        public void SetMaxHealth(float value, bool refill = true)
        {
            maxHealth = Mathf.Max(1f, value);
            if (refill)
            {
                Current = maxHealth;
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            float before = Current;
            Current = Mathf.Min(maxHealth, Current + amount);
            if (Current > before)
            {
                Healed?.Invoke(this, Current - before);
            }
        }

        public float TakeDamage(float amount, GameObject source = null)
        {
            return TakeDamage(DamageInfo.Simple(amount, source));
        }

        // Returns the damage actually applied (0 if ignored, parried or already dead).
        public float TakeDamage(DamageInfo info)
        {
            if (IsDead || info.amount <= 0f)
            {
                return 0f;
            }

            if (!info.ignoreInvulnerability)
            {
                if (IsInvulnerable)
                {
                    return 0f;
                }

                if (TryGetComponent(out PlayerGuard guard) && guard.TryParry(info))
                {
                    return 0f;
                }

                if (TryGetComponent(out IDamageInterceptor interceptor) && interceptor.TryIntercept(info))
                {
                    return 0f;
                }
            }

            float applied = Mathf.Min(Current, info.amount);
            Current -= applied;
            LastDamage = info;

            if (hitInvulnerability > 0f && Current > 0f)
            {
                hitRecoveryUntil = Time.time + hitInvulnerability;
            }

            Damaged?.Invoke(this, info);
            AnyDamaged?.Invoke(this, info, applied);
            CreditSpecialMeters(info, applied);

            if (info.knockback > 0f && Current > 0f && TryGetComponent(out IKnockbackReceiver receiver))
            {
                Vector3 push = info.direction.sqrMagnitude > 0.0001f ? info.direction.normalized : -transform.forward;
                receiver.ApplyKnockback(Vector3.ProjectOnPlane(push, Vector3.up).normalized * info.knockback);
            }

            if (Current <= 0f)
            {
                Die();
            }

            return applied;
        }

        // Instant devirtualization that ignores invulnerability (Digital Sea, scripted events).
        public void Kill(DamageType type = DamageType.Fall)
        {
            if (IsDead)
            {
                return;
            }

            TakeDamage(new DamageInfo { amount = Current, type = type, ignoreInvulnerability = true });
        }

        private void CreditSpecialMeters(DamageInfo info, float applied)
        {
            if (applied <= 0f)
            {
                return;
            }

            if (TryGetComponent(out SpecialMeter ownMeter))
            {
                ownMeter.OnDamageTaken(applied);
            }

            if (info.source != null && info.source != gameObject && info.source.TryGetComponent(out SpecialMeter attackerMeter))
            {
                attackerMeter.OnDamageDealt(applied);
            }
        }

        private void Die()
        {
            Died?.Invoke(this);
            SendMessage("OnDevirtualized", LastDamage.source, SendMessageOptions.DontRequireReceiver);

            if (devirtualizeEffect && Application.isPlaying)
            {
                Color color = team == Team.Enemy ? new Color(1f, 0.15f, 0.1f) : new Color(0.2f, 0.9f, 1f);
                DevirtualizeEffect.Spawn(transform.position + Vector3.up, color);
            }

            if (destroyOnDeath)
            {
                Destroy(gameObject, destroyDelay);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
