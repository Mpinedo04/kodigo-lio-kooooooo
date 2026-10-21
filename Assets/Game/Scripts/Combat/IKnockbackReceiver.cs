using UnityEngine;

namespace CodeLyokoFanGame
{
    // Implemented by anything that can be pushed by a hit (enemies, warriors).
    public interface IKnockbackReceiver
    {
        void ApplyKnockback(Vector3 velocity);
    }
}
