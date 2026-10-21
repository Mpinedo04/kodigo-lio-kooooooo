using UnityEngine;

namespace CodeLyokoFanGame
{
    // Collider that can receive hits for a Health (body parts, armour plates...).
    // Colliders without a Hurtbox fall back to the Health in their parents.
    public sealed class Hurtbox : MonoBehaviour
    {
        [SerializeField] private Health owner;
        [SerializeField] private float damageMultiplier = 1f;

        public Health Owner
        {
            get
            {
                if (owner == null)
                {
                    owner = GetComponentInParent<Health>();
                }

                return owner;
            }
        }

        public float DamageMultiplier => damageMultiplier;

        public void Configure(Health health, float multiplier)
        {
            owner = health;
            damageMultiplier = multiplier;
        }
    }
}
