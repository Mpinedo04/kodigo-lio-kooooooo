using System.Collections.Generic;

namespace CodeLyokoFanGame
{
    // Pure bookkeeping of who is allowed to attack right now (used by CombatDirector, tested alone).
    public sealed class AttackSlots
    {
        private readonly Dictionary<object, object> holders = new Dictionary<object, object>();
        private readonly List<object> scratch = new List<object>();

        public int Count => holders.Count;

        public bool Holds(object attacker)
        {
            return attacker != null && holders.ContainsKey(attacker);
        }

        public int CountFor(object target)
        {
            int count = 0;
            foreach (KeyValuePair<object, object> pair in holders)
            {
                if (Equals(pair.Value, target))
                {
                    count++;
                }
            }

            return count;
        }

        // maxTotal / maxPerTarget <= 0 mean "no limit".
        public bool TryAcquire(object attacker, object target, int maxTotal, int maxPerTarget)
        {
            if (attacker == null)
            {
                return false;
            }

            if (holders.ContainsKey(attacker))
            {
                holders[attacker] = target;
                return true;
            }

            if (maxTotal > 0 && holders.Count >= maxTotal)
            {
                return false;
            }

            if (target != null && maxPerTarget > 0 && CountFor(target) >= maxPerTarget)
            {
                return false;
            }

            holders.Add(attacker, target);
            return true;
        }

        public void Release(object attacker)
        {
            if (attacker != null)
            {
                holders.Remove(attacker);
            }
        }

        public void Clear()
        {
            holders.Clear();
        }

        // Drops holders that no longer exist (destroyed monsters).
        public void Prune(System.Func<object, bool> isGone)
        {
            scratch.Clear();
            foreach (KeyValuePair<object, object> pair in holders)
            {
                if (isGone(pair.Key))
                {
                    scratch.Add(pair.Key);
                }
            }

            for (int i = 0; i < scratch.Count; i++)
            {
                holders.Remove(scratch[i]);
            }
        }
    }
}
