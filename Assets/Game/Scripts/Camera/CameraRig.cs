using Cinemachine;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Drives the Cinemachine cameras: free orbit on foot, a wider orbit on vehicles,
    // a framing camera while locked on, sprint FOV and hit shake.
    public sealed class CameraRig : MonoBehaviour
    {
        private const int InactivePriority = 5;
        private const int ExplorePriority = 10;
        private const int VehiclePriority = 20;
        private const int LockOnPriority = 30;

        [SerializeField] private InputReader input;
        [SerializeField] private LockOnSystem lockOn;
        [SerializeField] private CinemachineFreeLook exploreCamera;
        [SerializeField] private CinemachineFreeLook vehicleCamera;
        [SerializeField] private CinemachineVirtualCamera lockOnCamera;
        [SerializeField] private CinemachineTargetGroup lockOnGroup;
        [SerializeField] private Transform lockOnPivot;
        [SerializeField] private CinemachineImpulseSource impulseSource;

        [Header("Field of view")]
        [SerializeField] private float exploreFov = 50f;
        [SerializeField] private float vehicleFov = 56f;
        [SerializeField] private float sprintFovBoost = 8f;
        [SerializeField] private float fovSharpness = 8f;

        [Header("Shake")]
        [SerializeField] private float heavyHitDamage = 15f;
        [SerializeField] private float hitShakeForce = 0.6f;

        private static CameraRig active;

        private PlayerController target;
        private Health targetHealth;
        private Transform lockedTarget;

        public static void Shake(float force)
        {
            if (active != null)
            {
                active.GenerateShake(force);
            }
        }

        public void SetTarget(PlayerController player)
        {
            if (targetHealth != null)
            {
                targetHealth.Damaged -= OnTargetDamaged;
            }

            target = player;
            targetHealth = player != null ? player.GetComponent<Health>() : null;
            if (targetHealth != null)
            {
                targetHealth.Damaged += OnTargetDamaged;
            }

            Transform follow = player != null ? player.transform : null;
            Transform look = player != null ? player.CameraTarget : null;
            SetOrbitTargets(exploreCamera, follow, look);
            SetOrbitTargets(vehicleCamera, follow, look);
            lockedTarget = null;
        }

        private void OnEnable()
        {
            active = this;
        }

        private void OnDisable()
        {
            if (active == this)
            {
                active = null;
            }
        }

        private void Update()
        {
            if (target == null)
            {
                return;
            }

            FeedLookInput();
            UpdateLockOn();
            UpdatePriorities();
            UpdateFieldOfView(Time.deltaTime);
        }

        private void FeedLookInput()
        {
            Vector2 look = input != null ? input.LookDelta : Vector2.zero;
            float sensitivity = GameSettings.LookSensitivity;
            float x = look.x * sensitivity;
            // Moving the mouse up lowers the orbit so the camera looks up.
            float y = -look.y * sensitivity * (GameSettings.InvertY ? -1f : 1f);

            SetAxisInput(exploreCamera, x, y);
            SetAxisInput(vehicleCamera, x, y);
        }

        private void UpdateLockOn()
        {
            Transform locked = lockOn != null ? lockOn.CurrentTarget : null;
            if (locked != lockedTarget)
            {
                lockedTarget = locked;
                if (lockOnGroup != null)
                {
                    lockOnGroup.m_Targets = locked == null
                        ? new CinemachineTargetGroup.Target[0]
                        : new[]
                        {
                            new CinemachineTargetGroup.Target { target = target.CameraTarget, weight = 1f, radius = 1f },
                            new CinemachineTargetGroup.Target { target = locked, weight = 1f, radius = 1.5f }
                        };
                }
            }

            if (locked == null || lockOnPivot == null)
            {
                return;
            }

            lockOnPivot.position = target.transform.position;
            Vector3 toTarget = Vector3.ProjectOnPlane(locked.position - target.transform.position, Vector3.up);
            if (toTarget.sqrMagnitude > 0.01f)
            {
                lockOnPivot.rotation = Quaternion.LookRotation(toTarget);
            }
        }

        private void UpdatePriorities()
        {
            if (exploreCamera != null)
            {
                exploreCamera.Priority = ExplorePriority;
            }

            if (vehicleCamera != null)
            {
                vehicleCamera.Priority = target.IsMounted ? VehiclePriority : InactivePriority;
            }

            if (lockOnCamera != null)
            {
                lockOnCamera.Priority = lockedTarget != null ? LockOnPriority : InactivePriority;
            }
        }

        private void UpdateFieldOfView(float deltaTime)
        {
            float boost = target.IsSprinting ? sprintFovBoost : 0f;
            float blend = 1f - Mathf.Exp(-fovSharpness * deltaTime);

            if (exploreCamera != null)
            {
                exploreCamera.m_Lens.FieldOfView = Mathf.Lerp(exploreCamera.m_Lens.FieldOfView, exploreFov + boost, blend);
            }

            if (vehicleCamera != null)
            {
                vehicleCamera.m_Lens.FieldOfView = Mathf.Lerp(vehicleCamera.m_Lens.FieldOfView, vehicleFov + boost, blend);
            }
        }

        private void OnTargetDamaged(Health health, DamageInfo info)
        {
            if (info.amount >= heavyHitDamage || info.isHeavy)
            {
                GenerateShake(hitShakeForce);
            }
        }

        private void GenerateShake(float force)
        {
            if (impulseSource != null)
            {
                impulseSource.GenerateImpulseWithForce(force);
            }
        }

        private static void SetOrbitTargets(CinemachineFreeLook freeLook, Transform follow, Transform look)
        {
            if (freeLook == null)
            {
                return;
            }

            freeLook.Follow = follow;
            freeLook.LookAt = look;
        }

        private static void SetAxisInput(CinemachineFreeLook freeLook, float x, float y)
        {
            if (freeLook == null)
            {
                return;
            }

            freeLook.m_XAxis.m_InputAxisValue = x;
            freeLook.m_YAxis.m_InputAxisValue = y;
        }
    }
}
