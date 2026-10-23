using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Odd's Future Vision (F5.6): a few seconds of slow motion with the path of every enemy shot drawn.
    public sealed class FutureVisionAbility : Ability
    {
        private float endsAtUnscaled;
        private TrajectoryLines lines;

        public FutureVisionAbility(AbilityDefinition definition) : base(definition)
        {
        }

        public override string Status => IsActive ? "Vision del futuro" : null;

        protected override void Activate()
        {
            endsAtUnscaled = Time.unscaledTime + Definition.duration;
            SlowMotion.Begin(Definition.timeScale);
            lines = TrajectoryLines.Create();
        }

        public override void Tick(float dt)
        {
            if (Time.unscaledTime >= endsAtUnscaled)
            {
                End();
            }
        }

        protected override void OnEnd()
        {
            SlowMotion.End();
            if (lines != null)
            {
                Object.Destroy(lines.gameObject);
                lines = null;
            }
        }
    }
}
