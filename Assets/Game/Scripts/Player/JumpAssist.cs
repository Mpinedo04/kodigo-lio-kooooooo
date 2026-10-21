namespace CodeLyokoFanGame
{
    // Coyote time + jump buffering: a jump counts if pressed shortly before landing
    // or shortly after walking off a ledge. Pure logic so it can be unit tested.
    public sealed class JumpAssist
    {
        private const float Never = float.NegativeInfinity;

        private float coyoteTime;
        private float bufferTime;
        private float lastGroundedTime = Never;
        private float lastPressedTime = Never;

        public JumpAssist(float coyoteSeconds, float bufferSeconds)
        {
            Configure(coyoteSeconds, bufferSeconds);
        }

        public void Configure(float coyoteSeconds, float bufferSeconds)
        {
            coyoteTime = coyoteSeconds;
            bufferTime = bufferSeconds;
        }

        public void MarkGrounded(float time)
        {
            lastGroundedTime = time;
        }

        public void MarkJumpPressed(float time)
        {
            lastPressedTime = time;
        }

        public void ClearBuffer()
        {
            lastPressedTime = Never;
        }

        public bool CanUseCoyote(float time)
        {
            return time - lastGroundedTime <= coyoteTime;
        }

        public bool TryConsumeJump(float time)
        {
            bool buffered = time - lastPressedTime <= bufferTime;
            if (!buffered || !CanUseCoyote(time))
            {
                return false;
            }

            lastPressedTime = Never;
            lastGroundedTime = Never;
            return true;
        }
    }
}
