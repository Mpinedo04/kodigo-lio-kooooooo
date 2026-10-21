using UnityEngine;

namespace CodeLyokoFanGame
{
    // Moves back and forth; characters standing on it are carried by its Delta.
    // Runs before players so they read this frame's movement.
    [DefaultExecutionOrder(-100)]
    public sealed class MovingPlatform : MonoBehaviour
    {
        [SerializeField] private Vector3 travel = new Vector3(0f, 0f, 6f);
        [SerializeField] private float period = 5f;

        private Vector3 start;

        public Vector3 Delta { get; private set; }

        public void Configure(Vector3 travelOffset, float cycleSeconds)
        {
            travel = travelOffset;
            period = cycleSeconds;
        }

        private void Awake()
        {
            start = transform.position;
        }

        private void Update()
        {
            float t = (Mathf.Sin(Time.time * 2f * Mathf.PI / Mathf.Max(0.1f, period)) + 1f) * 0.5f;
            Vector3 next = start + travel * t;
            Delta = next - transform.position;
            transform.position = next;
        }
    }
}
