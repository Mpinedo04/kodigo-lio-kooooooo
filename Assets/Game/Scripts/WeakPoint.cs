using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class WeakPoint : MonoBehaviour
    {
        [SerializeField] private Health target;
        [SerializeField] private float damageMultiplier = 2.5f;

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponentInParent<Health>();
            }
        }

        public void ApplyDamage(float amount, GameObject source)
        {
            if (target != null)
            {
                target.TakeDamage(amount * damageMultiplier, source);
            }
        }
    }
}
