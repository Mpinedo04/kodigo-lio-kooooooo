using UnityEngine;

namespace CodeLyokoFanGame
{
    // A named point in a level that mission steps can ask the player to reach.
    public sealed class MissionMarker : MonoBehaviour
    {
        [SerializeField] private string id;
        [SerializeField] private float radius = 6f;

        public string Id => id;
        public float Radius => radius;

        public void Configure(string markerId, float reachRadius)
        {
            id = markerId;
            radius = reachRadius;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
