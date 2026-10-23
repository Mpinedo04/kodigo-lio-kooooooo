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

        [Header("Vehicle")]
        [Tooltip("Extra field of view at the vehicle's top speed.")]
        [SerializeField] private float vehicleSpeedFovBoost = 14f;
        [Tooltip("The vehicle camera swings back behind the vehicle after this many seconds without camera input.")]
        [SerializeField] private float vehicleRecenterWait = 0.6f;
        [SerializeField] private float vehicleRecenterTime = 0.7f;
        [SerializeField] private float vehicleHeightRecenterWait = 1.5f;
        [SerializeField] private float vehicleHeightRecenterTime = 1.2f;
        [Tooltip("Speed lines appear above this share of the vehicle's top speed.")]
        [SerializeField] private float speedLinesThreshold = 0.5f;
        [SerializeField] private bool speedLinesEnabled = true;

        [Header("Shake")]
        [SerializeField] private float heavyHitDamage = 15f;
        [SerializeField] private float hitShakeForce = 0.6f;

        private static CameraRig active;

        private PlayerController target;
        private Health targetHealth;
        private Transform lockedTarget;
        private SpeedLines speedLines;

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

        private void Start()
        {
            ConfigureVehicleCamera();
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

            float vehicleSpeed01 = target.IsMounted && target.Vehicle != null ? target.Vehicle.Speed01 : 0f;
            if (vehicleCamera != null)
            {
                vehicleCamera.m_Lens.FieldOfView = Mathf.Lerp(vehicleCamera.m_Lens.FieldOfView, vehicleFov + vehicleSpeed01 * vehicleSpeedFovBoost, blend);
            }

            UpdateSpeedLines(vehicleSpeed01);
        }

        // The vehicle camera follows behind the vehicle again when the player leaves the mouse / stick alone.
        private void ConfigureVehicleCamera()
        {
            if (vehicleCamera == null)
            {
                return;
            }

            vehicleCamera.m_Heading.m_Definition = CinemachineOrbitalTransposer.Heading.HeadingDefinition.TargetForward;
            vehicleCamera.m_RecenterToTargetHeading.m_enabled = true;
            vehicleCamera.m_RecenterToTargetHeading.m_WaitTime = vehicleRecenterWait;
            vehicleCamera.m_RecenterToTargetHeading.m_RecenteringTime = vehicleRecenterTime;
            vehicleCamera.m_YAxisRecentering.m_enabled = true;
            vehicleCamera.m_YAxisRecentering.m_WaitTime = vehicleHeightRecenterWait;
            vehicleCamera.m_YAxisRecentering.m_RecenteringTime = vehicleHeightRecenterTime;
        }

        private void UpdateSpeedLines(float speed01)
        {
            if (!speedLinesEnabled)
            {
                return;
            }

            float intensity = speed01 > speedLinesThreshold ? (speed01 - speedLinesThreshold) / (1f - speedLinesThreshold) : 0f;
            if (speedLines == null)
            {
                if (intensity <= 0f || Camera.main == null)
                {
                    return;
                }

                speedLines = SpeedLines.Attach(Camera.main.transform);
            }

            speedLines.SetIntensity(intensity);
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
