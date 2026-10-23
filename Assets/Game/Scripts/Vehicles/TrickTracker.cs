using UnityEngine;

namespace CodeLyokoFanGame
{
    // Timing of vehicle tricks: one trick at a time, landed tricks chain into a combo with a growing multiplier.
    public sealed class TrickTracker
    {
        private float startTime;
        private float endTime = -1f;
        private float readyTime;
        private float lastLandTime = -1000f;

        public int Combo { get; private set; }
        public int Kind { get; private set; }

        public bool IsActive => endTime >= 0f;

        public bool TryStart(float now, float duration, float cooldown, int kind)
        {
            if (IsActive || now < readyTime)
            {
                return false;
            }

            startTime = now;
            endTime = now + Mathf.Max(0.05f, duration);
            readyTime = endTime + Mathf.Max(0f, cooldown);
            Kind = kind;
            return true;
        }

        public float Progress(float now)
        {
            return IsActive ? Mathf.Clamp01((now - startTime) / (endTime - startTime)) : 0f;
        }

        // Returns the energy multiplier when a trick lands this call, otherwise 0.
        public float Tick(float now, float comboWindow, float comboBonus, float maxMultiplier)
        {
            if (!IsActive)
            {
                if (Combo > 0 && now - lastLandTime > comboWindow)
                {
                    Combo = 0;
                }

                return 0f;
            }

            if (now < endTime)
            {
                return 0f;
            }

            endTime = -1f;
            Combo = now - lastLandTime <= comboWindow ? Combo + 1 : 1;
            lastLandTime = now;
            return Mathf.Min(Mathf.Max(1f, maxMultiplier), 1f + (Combo - 1) * comboBonus);
        }

        // A failed or interrupted trick also breaks the chain.
        public void Cancel()
        {
            endTime = -1f;
            Combo = 0;
        }
    }
}
