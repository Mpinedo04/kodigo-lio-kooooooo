using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Which monsters are in the air because of Yumi's telekinesis (held or thrown). Synergies read it.
    public static class TelekinesisState
    {
        private static readonly HashSet<GameObject> Lifted = new HashSet<GameObject>();

        public static bool IsLifted(GameObject target)
        {
            return target != null && Lifted.Contains(target);
        }

        public static void Mark(GameObject target, bool lifted)
        {
            if (target == null)
            {
                return;
            }

            if (lifted)
            {
                Lifted.Add(target);
            }
            else
            {
                Lifted.Remove(target);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            Lifted.Clear();
        }
    }
}
