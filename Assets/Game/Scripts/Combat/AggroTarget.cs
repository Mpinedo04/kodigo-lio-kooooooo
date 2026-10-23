using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Optional: how attractive a Player-team Health is to monsters (Aelita, clones, decoys).
    // Health without this component counts as priority 1. Priority 0 = ignored by monsters.
    public sealed class AggroTarget : MonoBehaviour
    {
        private static readonly List<AggroTarget> Active = new List<AggroTarget>();

        [SerializeField] private float priority = 1f;
        [Tooltip("Where monsters aim. Empty = this transform.")]
        [SerializeField] private Transform aimPoint;

        public static IReadOnlyList<AggroTarget> All => Active;

        public float Priority => priority;
        public Transform AimPoint => aimPoint != null ? aimPoint : transform;

        public void SetPriority(float value)
        {
            priority = Mathf.Max(0f, value);
        }

        public void SetAimPoint(Transform point)
        {
            aimPoint = point;
        }

        public static float PriorityOf(Component target)
        {
            if (target == null)
            {
                return 0f;
            }

            return target.TryGetComponent(out AggroTarget aggro) ? aggro.Priority : 1f;
        }

        // Chest height for warriors without an explicit aim point (their pivot is at the feet).
        public static Vector3 AimPositionOf(Component target)
        {
            if (target == null)
            {
                return Vector3.zero;
            }

            if (target.TryGetComponent(out AggroTarget aggro) && aggro.aimPoint != null)
            {
                return aggro.aimPoint.position;
            }

            return target.transform.position + Vector3.up;
        }

        private void OnEnable()
        {
            if (!Active.Contains(this))
            {
                Active.Add(this);
            }
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }
    }
}
