using UnityEngine;

namespace CodeLyokoFanGame
{
    // Which hits a frontal shield stops: shots (lasers, projectiles) coming from the front.
    public static class ShieldMath
    {
        public static bool Blocks(Vector3 forward, DamageInfo info, float minFacing = 0.2f)
        {
            if (info.type != DamageType.Laser && info.type != DamageType.Projectile)
            {
                return false;
            }

            if (info.direction.sqrMagnitude < 0.0001f)
            {
                return true;
            }

            return Vector3.Dot(forward.normalized, -info.direction.normalized) > minFacing;
        }
    }
}
