using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerCombat))]
    public sealed class PlayerController : MonoBehaviour, IKnockbackReceiver
    {
        [Header("Identity")]
        [SerializeField] private CharacterDefinition definition;
        [SerializeField] private LyokoCharacter character = LyokoCharacter.Odd;
        [SerializeField] private VehicleStyle vehicleStyle = VehicleStyle.Overboard;

        [Header("Movement")]
        [SerializeField] private MovementProfile movement;
        [SerializeField] private float walkSpeed = 7f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float dodgeSpeed = 18f;

        [Header("Traits")]
        [SerializeField] private bool canDoubleJump;
        [SerializeField] private bool canWallClimb;
        [SerializeField] private bool canLongJump;

        [Header("Vehicle")]
        [SerializeField] private VehicleRig vehiclePrefab;
        [SerializeField] private Transform vehicleSocket;

        private CharacterController controller;
        private PlayerCombat combat;
        private PlayerGuard guard;
        private Health health;
        private Transform cameraTransform;
        private Transform cameraTarget;
        private LockOnSystem lockOn;
        private InputReader input;
        private JumpAssist jumpAssist;

        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private bool isJumping;
        private bool jumpCutApplied;
        private int airJumpsUsed;
        private float wallClimbTimer;
        private Vector3 dodgeDirection;
        private float dodgeTimer;
        private float dodgeCooldownTimer;
        private MovingPlatform groundPlatform;

        private bool controlActive;
        private bool mounted;
        private VehicleRig vehicleInstance;

        public LyokoCharacter Character => character;
        public bool IsMounted => mounted;
        public bool IsSprinting { get; private set; }
        public bool IsDodging => dodgeTimer > 0f;
        public bool IsGrounded => controller != null && controller.isGrounded;
        public Vector3 Velocity => horizontalVelocity + Vector3.up * verticalVelocity;
        // Last place this warrior stood on solid ground (used to hand off when it falls into the Digital Sea).
        public Vector3 LastGroundedPosition { get; private set; }

        // Point the camera looks at (chest height); created by the level builders.
        public Transform CameraTarget
        {
            get
            {
                if (cameraTarget == null)
                {
                    cameraTarget = transform.Find("CameraTarget");
                    if (cameraTarget == null)
                    {
                        cameraTarget = transform;
                    }
                }

                return cameraTarget;
            }
        }

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
            canDoubleJump = definition.canDoubleJump;
            canWallClimb = definition.canWallClimb;
            canLongJump = definition.canLongJump;

            Health ownHealth = GetComponent<Health>();
            if (ownHealth != null)
            {
                ownHealth.SetMaxHealth(definition.maxHealth);
            }

            PlayerCombat playerCombat = GetComponent<PlayerCombat>();
            if (playerCombat != null)
            {
                playerCombat.ApplyDefinition(definition);
            }
        }

        public void Initialize(Transform cameraTransformForMovement, LockOnSystem targeter, InputReader reader)
        {
            cameraTransform = cameraTransformForMovement;
            lockOn = targeter;
            input = reader;
            combat = GetComponent<PlayerCombat>();
            combat.SetLockOn(targeter);
            ApplyDefinition(definition);
        }

        public void SetControlActive(bool active)
        {
            controlActive = active;
            gameObject.SetActive(active);

            if (!active)
            {
                return;
            }

            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            dodgeTimer = 0f;
            IsSprinting = false;

            if (lockOn != null)
            {
                lockOn.SetOwner(transform);
            }

            if (input != null)
            {
                input.SetVehicleMode(mounted);
            }
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            combat = GetComponent<PlayerCombat>();
            health = GetComponent<Health>();
            if (!TryGetComponent(out guard))
            {
                guard = gameObject.AddComponent<PlayerGuard>();
            }

            if (movement == null)
            {
                movement = ScriptableObject.CreateInstance<MovementProfile>();
            }

            jumpAssist = new JumpAssist(movement.coyoteTime, movement.jumpBufferTime);
            LastGroundedPosition = transform.position;
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
            if (!controlActive || input == null)
            {
                return;
            }

            if (input.LockOnPressed && lockOn != null)
            {
                lockOn.ToggleLock();
            }

            if (mounted ? input.DismountPressed : input.VehiclePressed)
            {
                ToggleVehicle();
            }

            if (mounted)
            {
                combat.CancelCharge();
                if (input.FirePressed)
                {
                    combat.BasicAttack();
                }
            }
            else
            {
                combat.HandleAttackInput(input.AttackPressed, input.AttackHeld);
                if (input.GuardPressed)
                {
                    guard.Press();
                }
            }

            if (input.SpecialPressed)
            {
                combat.SpecialAttack();
            }

            Move(Time.deltaTime);
        }

        private void Move(float dt)
        {
            bool grounded = controller.isGrounded;
            Vector2 moveInput = Vector2.ClampMagnitude(mounted ? input.Steer : input.Move, 1f);
            Vector3 camForward = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized : Vector3.forward;
            Vector3 camRight = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized : Vector3.right;
            Vector3 desired = camForward * moveInput.y + camRight * moveInput.x;
            bool hasInput = desired.sqrMagnitude > 0.01f;

            bool wantsFast = mounted ? input.BoostHeld : input.SprintHeld;
            IsSprinting = wantsFast && hasInput;
            float targetSpeed = wantsFast ? sprintSpeed : walkSpeed;
            if (mounted)
            {
                targetSpeed *= vehicleInstance != null ? vehicleInstance.SpeedMultiplier : 1.55f;
            }

            dodgeCooldownTimer -= dt;
            if (!mounted && input.DodgePressed && dodgeCooldownTimer <= 0f)
            {
                StartDodge(desired);
            }

            if (dodgeTimer > 0f)
            {
                dodgeTimer -= dt;
                horizontalVelocity = dodgeDirection * dodgeSpeed;
            }
            else
            {
                float acceleration = grounded || mounted
                    ? (hasInput ? movement.acceleration : movement.deceleration)
                    : movement.airAcceleration;
                horizontalVelocity = MovementMath.Accelerate(horizontalVelocity, desired * targetSpeed, acceleration, dt);

                if (!mounted && grounded && !wantsFast && movement.edgeProtection)
                {
                    horizontalVelocity = StopAtEdge(horizontalVelocity);
                }
            }

            Vector3 facing = dodgeTimer > 0f ? dodgeDirection : desired;
            if (facing.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(facing), movement.turnSpeed * dt);
            }

            if (mounted && vehicleInstance != null && vehicleInstance.CanFly)
            {
                float vertical = (input.AscendHeld ? 1f : 0f) - (input.DescendHeld ? 1f : 0f);
                verticalVelocity = vertical * movement.vehicleVerticalSpeed;
            }
            else
            {
                UpdateVertical(grounded, desired, wantsFast, dt);
            }

            Vector3 platformDelta = groundPlatform != null ? groundPlatform.Delta : Vector3.zero;
            groundPlatform = null; // refreshed by OnControllerColliderHit during Move
            controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * dt + platformDelta);
        }

        private void UpdateVertical(bool grounded, Vector3 desired, bool wantsFast, float dt)
        {
            float now = Time.time;
            jumpAssist.Configure(movement.coyoteTime, movement.jumpBufferTime);

            if (grounded)
            {
                LastGroundedPosition = transform.position;
                jumpAssist.MarkGrounded(now);
                airJumpsUsed = 0;
                wallClimbTimer = 0f;
                if (verticalVelocity < 0f)
                {
                    isJumping = false;
                    verticalVelocity = -2f;
                }
            }

            if (input.JumpPressed)
            {
                jumpAssist.MarkJumpPressed(now);
            }

            if (!mounted && jumpAssist.TryConsumeJump(now))
            {
                if (canLongJump && wantsFast && desired.sqrMagnitude > 0.25f)
                {
                    verticalVelocity = movement.longJumpUpForce;
                    horizontalVelocity = desired.normalized * movement.longJumpForwardSpeed;
                }
                else
                {
                    verticalVelocity = jumpForce;
                }

                isJumping = true;
                jumpCutApplied = false;
            }
            else if (!mounted && input.JumpPressed && !grounded && canDoubleJump && airJumpsUsed < movement.maxAirJumps)
            {
                verticalVelocity = movement.doubleJumpForce;
                airJumpsUsed++;
                isJumping = true;
                jumpCutApplied = false;
                jumpAssist.ClearBuffer();
            }

            // Releasing jump early gives a lower jump.
            if (isJumping && !jumpCutApplied && verticalVelocity > 0f && !input.JumpHeld)
            {
                verticalVelocity *= movement.jumpCutMultiplier;
                jumpCutApplied = true;
            }

            if (canWallClimb && !grounded && wallClimbTimer < movement.wallClimbMaxTime && IsPushingIntoWall(desired))
            {
                verticalVelocity = movement.wallClimbSpeed;
                wallClimbTimer += dt;
                return;
            }

            verticalVelocity = MovementMath.ApplyGravity(verticalVelocity, movement.gravity, movement.fallGravityMultiplier, movement.maxFallSpeed, dt);
        }

        private void StartDodge(Vector3 desired)
        {
            dodgeDirection = desired.sqrMagnitude > 0.01f ? desired.normalized : transform.forward;
            dodgeTimer = movement.dodgeDuration;
            dodgeCooldownTimer = movement.dodgeCooldown;

            if (health != null)
            {
                health.GrantInvulnerability(movement.dodgeInvulnerability);
            }
        }

        // Walking (not sprinting) stops at ledges instead of dropping into the Digital Sea.
        private Vector3 StopAtEdge(Vector3 velocity)
        {
            if (velocity.sqrMagnitude < 0.01f)
            {
                return velocity;
            }

            Vector3 probe = transform.position + velocity.normalized * (controller.radius + movement.edgeCheckDistance) + Vector3.up * 0.5f;
            bool groundAhead = Physics.Raycast(probe, Vector3.down, movement.edgeDropHeight + 0.5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            return groundAhead ? velocity : Vector3.zero;
        }

        private bool IsPushingIntoWall(Vector3 desired)
        {
            if (desired.sqrMagnitude < 0.25f)
            {
                return false;
            }

            Vector3 origin = transform.position + Vector3.up * controller.height * 0.5f;
            float distance = controller.radius + movement.wallCheckDistance;
            if (!Physics.Raycast(origin, transform.forward, out RaycastHit hit, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            bool steep = hit.normal.y < 0.3f;
            bool facingWall = Vector3.Dot(desired.normalized, -hit.normal) > 0.5f;
            return steep && facingWall && !hit.collider.CompareTag("Player");
        }

        public void ApplyKnockback(Vector3 velocity)
        {
            if (mounted)
            {
                return;
            }

            horizontalVelocity += velocity;
            verticalVelocity = Mathf.Max(verticalVelocity, 2f);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.normal.y > 0.5f)
            {
                groundPlatform = hit.collider.GetComponentInParent<MovingPlatform>();
            }
        }

        private void ToggleVehicle()
        {
            mounted = !mounted;
            verticalVelocity = 0f;

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

            if (input != null)
            {
                input.SetVehicleMode(mounted);
            }
        }
    }
}
