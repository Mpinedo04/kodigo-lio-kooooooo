using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerCombat))]
    public sealed class PlayerController : MonoBehaviour, IKnockbackReceiver
    {
        private static readonly List<PlayerController> Active = new List<PlayerController>();

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
        [SerializeField] private VehicleController vehiclePrefab;
        [SerializeField] private Transform vehicleSocket;

        private CharacterController controller;
        private PlayerCombat combat;
        private PlayerGuard guard;
        private Health health;
        private AbilityController abilities;
        private CompanionAI companion;
        private StatusEffects status;
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
        private Vector3 forcedVelocity;
        private float forcedTimer;
        private float stunTimer;
        private MovingPlatform groundPlatform;

        private bool controlActive;
        private bool checkVehicleHandoff;
        private VehicleController vehicleInstance;

        // Warriors in Lyoko right now (controlled or AI companions).
        public static IReadOnlyList<PlayerController> All => Active;

        // Aelita's order wheel borrows the move stick while it is open.
        public static bool MoveInputBlocked { get; set; }

        public LyokoCharacter Character => character;
        public CharacterDefinition Definition => definition;
        public bool IsControlled => controlActive;
        public bool IsMounted => vehicleInstance != null && vehicleInstance.HasRider;
        public VehicleController Vehicle => vehicleInstance;
        public bool IsStunned => stunTimer > 0f;
        // Blok's prison ray (F06): no walking or jumping while it lasts.
        public bool IsRooted => status != null && status.IsRooted;
        public bool IsForcedMoving => forcedTimer > 0f;
        // > 0 caps the fall speed (Aelita's wings).
        public float GlideFallSpeed { get; set; }
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

            if (TryGetComponent(out AbilityController ownAbilities))
            {
                ownAbilities.Initialize(reader, cameraTransformForMovement, targeter);
            }
        }

        // The warrior not being controlled stays in Lyoko as an AI companion (F5.28).
        public void SetControlActive(bool active)
        {
            if (!active && IsMounted)
            {
                vehicleInstance.Dismount(true);
            }

            controlActive = active;
            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            dodgeTimer = 0f;
            forcedTimer = 0f;
            IsSprinting = false;

            if (companion == null)
            {
                companion = GetComponent<CompanionAI>();
            }

            if (companion != null)
            {
                companion.enabled = !active;
            }

            if (!active)
            {
                if (combat != null)
                {
                    combat.CancelCharge();
                }

                return;
            }

            if (lockOn != null)
            {
                lockOn.SetOwner(transform);
            }

            checkVehicleHandoff = true;
            if (input != null)
            {
                input.SetVehicleMode(IsMounted);
            }
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            combat = GetComponent<PlayerCombat>();
            health = GetComponent<Health>();
            abilities = GetComponent<AbilityController>();
            companion = GetComponent<CompanionAI>();
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
                socket.transform.localPosition = Vector3.zero;
                vehicleSocket = socket.transform;
            }
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

        // Abilities take over the legs for a moment (Supersprint, Triangular). Works for AI companions too.
        public void ForceMove(Vector3 velocity, float duration)
        {
            forcedVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
            forcedTimer = duration;
            dodgeTimer = 0f;
            if (forcedVelocity.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(forcedVelocity);
            }
        }

        public void StopForcedMove()
        {
            forcedTimer = 0f;
            horizontalVelocity = Vector3.zero;
        }

        // Launches the warrior up (Feline Rain leap, wing flaps). Releasing jump does not cut it.
        public void SetVerticalVelocity(float value)
        {
            verticalVelocity = value;
            isJumping = value > 0f;
            jumpCutApplied = true;
        }

        public void Stun(float seconds)
        {
            stunTimer = Mathf.Max(stunTimer, seconds);
        }

        public void ForceDismount()
        {
            if (IsMounted)
            {
                vehicleInstance.Dismount(false);
            }
        }

        private void Update()
        {
            stunTimer -= Time.deltaTime;

            if (!controlActive || input == null)
            {
                if (forcedTimer > 0f && controller != null && controller.enabled)
                {
                    forcedTimer -= Time.deltaTime;
                    controller.Move((forcedVelocity + Vector3.down * 2f) * Time.deltaTime);
                }

                return;
            }

            if (input.LockOnPressed && lockOn != null)
            {
                lockOn.ToggleLock();
            }

            // A warrior switched in while the previous one was riding keeps going on its own vehicle.
            if (checkVehicleHandoff)
            {
                checkVehicleHandoff = false;
                if (VehicleController.HandoffPending && EnsureVehicle())
                {
                    vehicleInstance.TakeHandoff(input);
                }
            }

            if (IsMounted)
            {
                combat.CancelCharge();
                vehicleInstance.Drive(input, cameraTransform, Time.deltaTime);
                return;
            }

            if (input.VehiclePressed && !IsStunned)
            {
                ToggleVehicle();
            }

            // Telekinesis uses the attack button to throw; a stunned warrior can only fall.
            bool attacksFree = !IsStunned && (abilities == null || !abilities.BlocksAttacks);
            if (attacksFree)
            {
                combat.HandleAttackInput(input.AttackPressed, input.AttackHeld);
                if (input.GuardPressed)
                {
                    guard.Press();
                }
            }
            else
            {
                combat.CancelCharge();
            }

            if (attacksFree && input.SpecialPressed)
            {
                combat.SpecialAttack();
            }

            Move(Time.deltaTime);
        }

        private void Move(float dt)
        {
            bool grounded = controller.isGrounded;
            if (status == null)
            {
                TryGetComponent(out status);
            }

            Vector2 moveInput = IsStunned || IsRooted || MoveInputBlocked ? Vector2.zero : Vector2.ClampMagnitude(input.Move, 1f);
            Vector3 camForward = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized : Vector3.forward;
            Vector3 camRight = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized : Vector3.right;
            Vector3 desired = camForward * moveInput.y + camRight * moveInput.x;
            bool hasInput = desired.sqrMagnitude > 0.01f;

            bool wantsFast = input.SprintHeld;
            IsSprinting = wantsFast && hasInput;
            float targetSpeed = (wantsFast ? sprintSpeed : walkSpeed) * (status != null ? status.SpeedMultiplier : 1f);

            dodgeCooldownTimer -= dt;
            if (!IsStunned && !IsRooted && input.DodgePressed && dodgeCooldownTimer <= 0f)
            {
                StartDodge(desired);
            }

            if (forcedTimer > 0f)
            {
                forcedTimer -= dt;
                horizontalVelocity = forcedVelocity;
            }
            else if (dodgeTimer > 0f)
            {
                dodgeTimer -= dt;
                horizontalVelocity = dodgeDirection * dodgeSpeed;
            }
            else
            {
                float acceleration = grounded
                    ? (hasInput ? movement.acceleration : movement.deceleration)
                    : movement.airAcceleration;
                horizontalVelocity = MovementMath.Accelerate(horizontalVelocity, desired * targetSpeed, acceleration, dt);

                if (grounded && !wantsFast && movement.edgeProtection)
                {
                    horizontalVelocity = StopAtEdge(horizontalVelocity);
                }
            }

            Vector3 facing = forcedTimer > 0f ? forcedVelocity : dodgeTimer > 0f ? dodgeDirection : desired;
            if (facing.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(facing), movement.turnSpeed * dt);
            }

            UpdateVertical(grounded, desired, wantsFast, dt);

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

            if (input.JumpPressed && !IsStunned && !IsRooted)
            {
                jumpAssist.MarkJumpPressed(now);
            }

            if (jumpAssist.TryConsumeJump(now))
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
            else if (!IsStunned && input.JumpPressed && !grounded && canDoubleJump && airJumpsUsed < movement.maxAirJumps)
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
            if (GlideFallSpeed > 0f && verticalVelocity < -GlideFallSpeed)
            {
                verticalVelocity = -GlideFallSpeed;
            }
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
            if (IsMounted)
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

        // Summons the vehicle next to the warrior and starts riding (getting off is handled by VehicleController).
        private void ToggleVehicle()
        {
            if (!EnsureVehicle() || !vehicleInstance.TryMount(input, Velocity, false))
            {
                return;
            }

            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            dodgeTimer = 0f;
            IsSprinting = false;
        }

        private bool EnsureVehicle()
        {
            if (vehicleInstance == null && vehiclePrefab != null)
            {
                vehicleInstance = Instantiate(vehiclePrefab, vehicleSocket);
                vehicleInstance.Bind(this);
            }

            return vehicleInstance != null;
        }
    }
}
