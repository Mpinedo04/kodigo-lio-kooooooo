using System;
using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField] private float destroyDelay = 0.05f;

        public event Action<Health> Died;
        public event Action<Health, float> Damaged;

        public float Current { get; private set; }
        public float Max => maxHealth;
        public bool IsDead => Current <= 0f;

        private void Awake()
        {
            Current = maxHealth;
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

        public void TakeDamage(float amount, GameObject source = null)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            Current = Mathf.Max(0f, Current - amount);
            Damaged?.Invoke(this, amount);

            if (Current <= 0f)
            {
                Died?.Invoke(this);
                SendMessage("OnDevirtualized", source, SendMessageOptions.DontRequireReceiver);

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
}
