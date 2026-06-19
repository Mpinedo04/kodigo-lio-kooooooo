using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerCombat))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private CharacterDefinition definition;
        [SerializeField] private LyokoCharacter character = LyokoCharacter.Odd;
        [SerializeField] private VehicleStyle vehicleStyle = VehicleStyle.Overboard;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 7f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = -24f;
        [SerializeField] private float dodgeSpeed = 18f;
        [SerializeField] private float dodgeDuration = 0.16f;

        [Header("Vehicle")]
        [SerializeField] private VehicleRig vehiclePrefab;
        [SerializeField] private Transform vehicleSocket;

        private CharacterController controller;
        private PlayerCombat combat;
        private Transform cameraTransform;
        private LockOnSystem lockOn;
        private Vector3 velocity;
        private Vector3 dodgeDirection;
        private float dodgeTimer;
        private bool controlActive;
        private bool mounted;
        private VehicleRig vehicleInstance;

        public LyokoCharacter Character => character;
        public bool IsMounted => mounted;

        public void ApplyDefinition(CharacterDefinition newDefinition)
        {
            if (newDefinition == null)
            {
                return;
            }

            definition = newDefinition;
            character = definition.character;
            vehicleStyle = definition.vehicleStyle;
            walkSpeed = definition.walkSpeed;
            sprintSpeed = definition.sprintSpeed;
            jumpForce = definition.jumpForce;
            dodgeSpeed = definition.dodgeSpeed;

            Health health = GetComponent<Health>();
            if (health != null)
            {
                health.SetMaxHealth(definition.maxHealth);
            }

            PlayerCombat playerCombat = GetComponent<PlayerCombat>();
            if (playerCombat != null)
            {
                playerCombat.ApplyDefinition(definition);
            }
        }

        public void Initialize(Transform cameraTarget, LockOnSystem targeter)
        {
            cameraTransform = cameraTarget;
            lockOn = targeter;
            combat = GetComponent<PlayerCombat>();
            ApplyDefinition(definition);
        }

        public void SetControlActive(bool active)
        {
            controlActive = active;
            gameObject.SetActive(active);

            if (active && lockOn != null)
            {
                lockOn.SetOwner(transform);
            }
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            combat = GetComponent<PlayerCombat>();
            ApplyDefinition(definition);

            if (vehicleSocket == null)
            {
                GameObject socket = new GameObject("VehicleSocket");
                socket.transform.SetParent(transform);
                socket.transform.localPosition = new Vector3(0f, -0.9f, 0f);
                vehicleSocket = socket.transform;
            }
        }

        private void Update()
        {
            if (!controlActive)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Tab) && lockOn != null)
            {
                lockOn.ToggleLock();
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                ToggleVehicle();
            }

            if (Input.GetMouseButtonDown(0))
            {
                combat.BasicAttack();
            }

            if (Input.GetMouseButtonDown(1))
            {
                combat.SpecialAttack();
            }

            Move();
        }

        private void Move()
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 camForward = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized : Vector3.forward;
            Vector3 camRight = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized : Vector3.right;
            Vector3 move = camForward * input.y + camRight * input.x;

            float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

            if (mounted)
            {
                speed *= vehicleInstance != null ? vehicleInstance.SpeedMultiplier : 1.55f;
                if (vehicleInstance != null && vehicleInstance.CanFly)
                {
                    move += Vector3.up * Input.GetAxisRaw("Jump") * 0.55f;
                }
            }

            if (move.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(move, Vector3.up)), Time.deltaTime * 14f);
            }

            if (controller.isGrounded && velocity.y < 0f)
            {
                velocity.y = -2f;
            }

            if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded && !mounted)
            {
                velocity.y = jumpForce;
            }

            if (Input.GetKeyDown(KeyCode.LeftControl) && move.sqrMagnitude > 0.01f)
            {
                dodgeTimer = dodgeDuration;
                dodgeDirection = move.normalized;
            }

            Vector3 frameMove = move * speed;

            if (dodgeTimer > 0f)
            {
                dodgeTimer -= Time.deltaTime;
                frameMove += dodgeDirection * dodgeSpeed;
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move((frameMove + velocity) * Time.deltaTime);
        }

        private void ToggleVehicle()
        {
            mounted = !mounted;

            if (vehicleInstance == null && vehiclePrefab != null)
            {
                vehicleInstance = Instantiate(vehiclePrefab, vehicleSocket);
                vehicleInstance.transform.localPosition = Vector3.zero;
                vehicleInstance.transform.localRotation = Quaternion.identity;
                vehicleInstance.Configure(vehicleStyle);
            }

            if (vehicleInstance != null)
            {
                vehicleInstance.gameObject.SetActive(mounted);
            }
        }
    }
}
