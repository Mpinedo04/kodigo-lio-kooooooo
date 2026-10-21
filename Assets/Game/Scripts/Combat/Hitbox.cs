using UnityEngine;

namespace CodeLyokoFanGame
{
    // Where a melee attack lands: a sphere in front of the attacker, tunable in the Inspector.
    public sealed class Hitbox : MonoBehaviour
    {
        private static readonly Collider[] Buffer = new Collider[48];

        [SerializeField] private float radius = 2.2f;
        [SerializeField] private LayerMask mask = Physics.AllLayers;

        public float Radius => radius;

        public void Configure(float sphereRadius)
        {
            radius = sphereRadius;
        }

        // Returns how many targets were hit.
        // allowWeakPoint = false: a plain swing counts as a body hit even if the sphere touches the eye.
        public int Strike(DamageInfo info, Team attackerTeam, float radiusMultiplier = 1f, float forwardOffset = 0f, bool allowWeakPoint = true)
        {
            Vector3 center = transform.position + transform.forward * forwardOffset;
            int count = Physics.OverlapSphereNonAlloc(center, radius * radiusMultiplier, Buffer, mask, QueryTriggerInteraction.Collide);
            info.point = center;
            if (info.direction.sqrMagnitude < 0.0001f)
            {
                info.direction = transform.forward;
            }

            return HitResolver.ApplyToColliders(Buffer, count, info, attackerTeam, null, allowWeakPoint);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
