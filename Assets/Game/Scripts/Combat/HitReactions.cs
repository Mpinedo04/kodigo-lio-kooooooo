using UnityEngine;

namespace CodeLyokoFanGame
{
    // Hit effects that need the monster's AI: stuns from DamageInfo.stagger (Odd's charged arrow, Impact).
    public static class HitReactions
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            Health.AnyDamaged -= OnAnyDamaged;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            Health.AnyDamaged -= OnAnyDamaged;
            Health.AnyDamaged += OnAnyDamaged;
        }

        private static void OnAnyDamaged(Health target, DamageInfo info, float applied)
        {
            if (info.stagger <= 0f || applied <= 0f || target == null || target.IsDead)
            {
                return;
            }

            if (target.TryGetComponent(out EnemyAI enemy))
            {
                enemy.Stagger(info.stagger);
            }
        }
    }
}
