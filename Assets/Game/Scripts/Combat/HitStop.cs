using UnityEngine;

namespace CodeLyokoFanGame
{
    // Freezes the game for a few hundredths of a second on strong hits so they feel heavy.
    public static class HitStop
    {
        private static HitStopRunner runner;

        public static bool Active { get; private set; }
        public static float EndsAt { get; private set; }

        public static void Trigger(float seconds)
        {
            if (seconds <= 0f || !Application.isPlaying)
            {
                return;
            }

            if (runner == null)
            {
                GameObject host = new GameObject("HitStop");
                Object.DontDestroyOnLoad(host);
                runner = host.AddComponent<HitStopRunner>();
            }

            if (!Active)
            {
                runner.RememberTimeScale(Time.timeScale);
                Active = true;
            }

            EndsAt = Mathf.Max(EndsAt, Time.unscaledTime + seconds);
            Time.timeScale = 0f;
        }

        internal static void End(float restoredTimeScale)
        {
            Active = false;
            // Slow motion may have started or ended during the freeze; it decides the speed to go back to.
            Time.timeScale = restoredTimeScale <= 0f ? 0f : SlowMotion.CurrentScale;
        }
    }
}
