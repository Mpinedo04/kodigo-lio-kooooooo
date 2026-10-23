using UnityEngine;

namespace CodeLyokoFanGame
{
    // Long slow motion (Odd's Future Vision). Works together with HitStop: a hit freeze ends back in slow motion.
    public static class SlowMotion
    {
        public static bool IsActive { get; private set; }
        public static float Scale { get; private set; } = 1f;

        // Time scale the game should run at when no hit freeze is happening.
        public static float CurrentScale => IsActive ? Scale : 1f;

        public static void Begin(float scale)
        {
            IsActive = true;
            Scale = Mathf.Clamp(scale, 0.05f, 1f);
            if (Application.isPlaying && !HitStop.Active)
            {
                Time.timeScale = Scale;
            }
        }

        public static void End()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            Scale = 1f;
            if (Application.isPlaying && !HitStop.Active)
            {
                Time.timeScale = 1f;
            }
        }
    }
}
