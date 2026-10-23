using UnityEngine;

namespace CodeLyokoFanGame
{
    // A bar that is spent and refills on its own after a short pause: Yumi's Concentration, Aelita's flight energy.
    public sealed class ResourcePool
    {
        private readonly float regenPerSecond;
        private readonly float regenDelay;
        private float sinceUse;

        public ResourcePool(float max, float regenPerSecond, float regenDelay)
        {
            Max = Mathf.Max(1f, max);
            Current = Max;
            this.regenPerSecond = regenPerSecond;
            this.regenDelay = regenDelay;
            sinceUse = regenDelay;
        }

        public float Max { get; }
        public float Current { get; private set; }
        public float Normalized => Current / Max;

        public bool CanAfford(float amount)
        {
            return Current >= amount;
        }

        public bool TrySpend(float amount)
        {
            if (Current < amount)
            {
                return false;
            }

            Current -= amount;
            sinceUse = 0f;
            return true;
        }

        // Continuous use (telekinesis held). Returns true when the bar runs dry.
        public bool Drain(float amount)
        {
            Current = Mathf.Max(0f, Current - amount);
            sinceUse = 0f;
            return Current <= 0f;
        }

        public void Fill()
        {
            Current = Max;
        }

        public void Tick(float dt)
        {
            sinceUse += dt;
            if (sinceUse >= regenDelay)
            {
                Current = Mathf.Min(Max, Current + regenPerSecond * dt);
            }
        }
    }
}
