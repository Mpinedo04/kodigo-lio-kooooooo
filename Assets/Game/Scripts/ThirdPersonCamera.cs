using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private LockOnSystem lockOn;
        [SerializeField] private Vector3 offset = new Vector3(0f, 2.3f, -6.5f);
        [SerializeField] private float mouseSensitivity = 3.2f;
        [SerializeField] private float followSharpness = 12f;
        [SerializeField] private float minPitch = -25f;
        [SerializeField] private float maxPitch = 62f;

        private float yaw;
        private float pitch = 18f;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Transform locked = lockOn != null ? lockOn.CurrentTarget : null;

            if (locked != null)
            {
                Vector3 toLocked = locked.position - target.position;
                yaw = Mathf.Atan2(toLocked.x, toLocked.z) * Mathf.Rad2Deg;
            }
            else
            {
                yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPosition = target.position + rotation * offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-followSharpness * Time.deltaTime));
            transform.LookAt(target.position + Vector3.up * 1.4f);
        }
    }
}
