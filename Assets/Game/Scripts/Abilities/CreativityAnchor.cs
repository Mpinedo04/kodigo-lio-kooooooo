using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // A spot where Aelita's Creativity works (F5.23), so level design stays under control.
    // Bridge: spans from here to endPoint. Wall: stands here, facing this transform's forward.
    public sealed class CreativityAnchor : MonoBehaviour
    {
        private static readonly List<CreativityAnchor> Active = new List<CreativityAnchor>();

        [SerializeField] private CreativityKind kind = CreativityKind.Bridge;
        [Tooltip("Bridge only: where it ends.")]
        [SerializeField] private Transform endPoint;
        [SerializeField] private float bridgeWidth = 2.5f;
        [SerializeField] private float bridgeThickness = 0.4f;
        [SerializeField] private Vector3 wallSize = new Vector3(6f, 3.5f, 0.8f);

        private GameObject built;

        public static IReadOnlyList<CreativityAnchor> All => Active;

        public CreativityKind Kind => kind;
        public Transform EndPoint => endPoint;
        public bool InUse => built != null;

        public void Configure(CreativityKind newKind, Transform newEndPoint)
        {
            kind = newKind;
            endPoint = newEndPoint;
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

        // sizeMultiplier > 1 = the huge version (Yumi + Aelita synergy).
        public GameObject Build(float lifetime, float sizeMultiplier)
        {
            if (kind == CreativityKind.Bridge && endPoint != null)
            {
                Vector3 span = endPoint.position - transform.position;
                Vector3 middle = transform.position + span * 0.5f - Vector3.up * bridgeThickness * 0.5f;
                Vector3 size = new Vector3(bridgeWidth * sizeMultiplier, bridgeThickness, span.magnitude + 0.6f);
                built = TemporaryStructure.Create("CreativityBridge", middle, Quaternion.LookRotation(span), size, AbilityVisuals.AelitaPink, lifetime, TemporaryStructure.Appear.Grow, false);
            }
            else
            {
                Vector3 size = new Vector3(wallSize.x * sizeMultiplier, wallSize.y * Mathf.Sqrt(sizeMultiplier), wallSize.z);
                built = TemporaryStructure.Create("CreativityWall", transform.position + Vector3.up * size.y * 0.5f, transform.rotation, size, AbilityVisuals.AelitaPink, lifetime, TemporaryStructure.Appear.Rise, true);
            }

            return built;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = AbilityVisuals.AelitaPink;
            if (kind == CreativityKind.Bridge && endPoint != null)
            {
                Gizmos.DrawLine(transform.position, endPoint.position);
                Gizmos.DrawWireSphere(endPoint.position, 0.3f);
            }
            else
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position + Vector3.up * wallSize.y * 0.5f, transform.rotation, wallSize);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }
    }
}
