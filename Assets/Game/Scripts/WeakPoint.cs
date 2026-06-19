using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class WeakPoint : MonoBehaviour
    {
        [SerializeField] private Health target;
        [SerializeField] private float damageMultiplier = 2.5f;
        [SerializeField] private bool exposed = true;

        private Collider weakCollider;
        private Renderer weakRenderer;

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponentInParent<Health>();
            }

            weakCollider = GetComponent<Collider>();
            weakRenderer = GetComponent<Renderer>();
            SetExposed(exposed);
        }

        public void Configure(float multiplier, bool startsExposed)
        {
            damageMultiplier = multiplier;
            SetExposed(startsExposed);
        }

        public void SetExposed(bool value)
        {
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

        public void ApplyDamage(float amount, GameObject source)
        {
            if (target != null && exposed)
            {
                target.TakeDamage(amount * damageMultiplier, source);
            }
        }
    }
}
