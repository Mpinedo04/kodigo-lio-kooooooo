using UnityEngine;

namespace CodeLyokoFanGame
{
    // Keeps decorative pieces alive: tower rings, halos, floating data. Spins around a local axis and bobs.
    public sealed class Spin : MonoBehaviour
    {
        [SerializeField] private Vector3 axis = Vector3.up;
        [SerializeField] private float degreesPerSecond = 30f;
        [SerializeField] private float bobHeight;
        [SerializeField] private float bobSpeed = 1f;

        private Vector3 startPosition;
        private float offset;

        public void Configure(Vector3 newAxis, float speed, float bob, float bobRate)
        {
            axis = newAxis;
            degreesPerSecond = speed;
            bobHeight = bob;
            bobSpeed = bobRate;
        }

        private void Awake()
        {
            startPosition = transform.localPosition;
            offset = (transform.position.x * 0.37f + transform.position.z * 0.21f) % 6.28f;
        }

        private void Update()
        {
            transform.Rotate(axis, degreesPerSecond * Time.deltaTime, Space.Self);
            if (bobHeight > 0f)
            {
                transform.localPosition = startPosition + Vector3.up * Mathf.Sin(Time.time * bobSpeed + offset) * bobHeight;
            }
        }
    }
}
