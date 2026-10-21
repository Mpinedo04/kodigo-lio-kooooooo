using System;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Energy for ultimates: fills when dealing and taking damage. Spent in F05.
    public sealed class SpecialMeter : MonoBehaviour
    {
        [SerializeField] private float max = 100f;
        [SerializeField] private float gainPerDamageDealt = 0.4f;
        [SerializeField] private float gainPerDamageTaken = 0.6f;

        public event Action<SpecialMeter> Changed;

        public float Current { get; private set; }
        public float Max => max;
        public float Normalized => max > 0f ? Current / max : 0f;
        public bool IsFull => Current >= max;

        public void Configure(float dealtGain, float takenGain)
        {
            gainPerDamageDealt = dealtGain;
            gainPerDamageTaken = takenGain;
        }

        public void OnDamageDealt(float amount)
        {
            Add(amount * gainPerDamageDealt);
        }

        public void OnDamageTaken(float amount)
        {
            Add(amount * gainPerDamageTaken);
        }

        public void Add(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            float before = Current;
            Current = Mathf.Min(max, Current + amount);
            if (!Mathf.Approximately(before, Current))
            {
                Changed?.Invoke(this);
            }
        }

        public bool TrySpend(float amount)
        {
            if (Current < amount)
            {
                return false;
            }

            Current -= amount;
            Changed?.Invoke(this);
            return true;
        }
    }
}
