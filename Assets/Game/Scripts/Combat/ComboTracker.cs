using System.Collections.Generic;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Pure combo timing: which step a press triggers, with buffering of early presses.
    public sealed class ComboTracker
    {
        private int current = -1;
        private float stepStart;
        private bool buffered;

        public int CurrentStep => current;

        // Returns the step to perform now, or -1 when the press is buffered or ignored.
        public int Press(float time, IReadOnlyList<ComboStep> steps)
        {
            if (steps == null || steps.Count == 0)
            {
                return -1;
            }

            if (current < 0 || current >= steps.Count || time > stepStart + steps[current].chainWindowEnd)
            {
                return Begin(0, time);
            }

            ComboStep step = steps[current];
            if (time < stepStart + step.chainWindowStart)
            {
                buffered = true;
                return -1;
            }

            if (current + 1 < steps.Count)
            {
                return Begin(current + 1, time);
            }

            return time >= stepStart + step.duration ? Begin(0, time) : -1;
        }

        // Call every frame: fires a buffered press once its window opens and resets stale combos.
        public int Tick(float time, IReadOnlyList<ComboStep> steps)
        {
            if (steps == null || current < 0 || current >= steps.Count)
            {
                return -1;
            }

            ComboStep step = steps[current];
            if (buffered && time >= stepStart + step.chainWindowStart)
            {
                if (current + 1 < steps.Count)
                {
                    return Begin(current + 1, time);
                }

                if (time >= stepStart + step.duration)
                {
                    return Begin(0, time);
                }
            }

            if (time > stepStart + step.chainWindowEnd)
            {
                current = -1;
                buffered = false;
            }

            return -1;
        }

        public void Reset()
        {
            current = -1;
            buffered = false;
        }

        private int Begin(int index, float time)
        {
            current = index;
            stepStart = time;
            buffered = false;
            return index;
        }
    }
}
