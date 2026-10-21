namespace CodeLyokoFanGame
{
    // Pure parry timing: pressing Guard opens a short window; a cooldown stops spamming.
    public sealed class ParryWindow
    {
        private float openUntil = float.NegativeInfinity;
        private float nextAllowed = float.NegativeInfinity;

        public bool TryOpen(float time, float duration, float cooldown)
        {
            if (time < nextAllowed)
            {
                return false;
            }

            openUntil = time + duration;
            nextAllowed = time + cooldown;
            return true;
        }

        public bool IsOpen(float time)
        {
            return time <= openUntil;
        }
    }
}
