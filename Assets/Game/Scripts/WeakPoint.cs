using UnityEngine;

namespace CodeLyokoFanGame
{
    // The eye of X.A.N.A.: extra damage (instant devirtualization on small monsters) while exposed.
    public sealed class WeakPoint : MonoBehaviour
    {
        [SerializeField] private Health target;
        [SerializeField] private float damageMultiplier = 2.5f;
        [SerializeField] private bool exposed = true;
        [SerializeField] private bool instantKill;
        [SerializeField] private float exposePulseSeconds = 0.35f;

        private Collider weakCollider;
        private Renderer weakRenderer;
        private Vector3 baseScale;
        private float pulseTimer;

        public Health Target => target;
        public float DamageMultiplier => damageMultiplier;
        public bool IsExposed => exposed;
        public bool InstantKill => instantKill;

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponentInParent<Health>();
            }

            weakCollider = GetComponent<Collider>();
            weakRenderer = GetComponent<Renderer>();
            baseScale = transform.localScale;
            SetExposed(exposed);
        }

        private void Update()
        {
            if (pulseTimer <= 0f)
            {
                return;
            }

            // Visual cue when the eye opens: a quick swell.
            pulseTimer -= Time.deltaTime;
            float pulse = Mathf.Sin(Mathf.Clamp01(1f - pulseTimer / exposePulseSeconds) * Mathf.PI);
            transform.localScale = baseScale * (1f + 0.6f * pulse);
        }

        public void SetTarget(Health health)
        {
            target = health;
        }

        public void Configure(float multiplier, bool startsExposed, bool killsInstantly = false)
        {
            damageMultiplier = multiplier;
            instantKill = killsInstantly;
            SetExposed(startsExposed);
        }

        public void SetExposed(bool value)
        {
            if (value && !exposed && Application.isPlaying)
            {
                pulseTimer = exposePulseSeconds;
            }

            exposed = value;

            if (weakCollider != null)
            {
                weakCollider.enabled = exposed;
            }

            if (weakRenderer != null)
            {
                weakRenderer.enabled = exposed;
            }
        }

        public float ApplyDamage(float amount, GameObject source)
        {
            return ReceiveHit(new DamageInfo { amount = amount, source = source, type = DamageType.Projectile });
        }

        public float ReceiveHit(DamageInfo info)
        {
            if (target == null || !exposed)
            {
                return 0f;
            }

            float amount = info.amount * damageMultiplier;
            if (instantKill)
            {
                amount = Mathf.Max(amount, target.Current);
            }

            info.amount = amount;
            info.isCritical = true;
            return target.TakeDamage(info);
        }
    }
}
