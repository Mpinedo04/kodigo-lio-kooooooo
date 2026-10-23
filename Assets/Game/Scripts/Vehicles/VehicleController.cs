using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Common vehicle physics and controls (F08); every value comes from VehicleDefinition.
    // The rider's CharacterController is the body: while mounted the vehicle drives it, stretches it down
    // to the hull, and hits on the rider land on the vehicle's own Health. PlayerController only calls Drive.
    [RequireComponent(typeof(Health))]
    public sealed class VehicleController : MonoBehaviour, IKnockbackReceiver
    {
        private static readonly Collider[] Buffer = new Collider[48];
        private static readonly Vector3 MountStartOffset = new Vector3(1.6f, 0.3f, 0f);

        private enum State
        {
            Stored,
            Mounting,
            Riding,
            Dismounting
        }

        [SerializeField] private VehicleDefinition definition;
        [Tooltip("Visual root that leans, bobs and spins during tricks.")]
        [SerializeField] private Transform visual;
        [SerializeField] private PassengerSeat passengerSeat;
        [SerializeField] private Color explosionColor = new Color(1f, 0.55f, 0.15f);

        // A warrior switched out while mounted hands the ride to the next one (see TakeHandoff).
        private static int handoffFrame = -100;
        private static Vector3 handoffVelocity;
        private static AelitaCompanion handoffPassenger;
        private static int groundMask;

        private readonly TrickTracker tricks = new TrickTracker();
        private readonly HashSet<Health> ramHits = new HashSet<Health>();

        private Health health;
        private PlayerController rider;
        private CharacterController riderBody;
        private PlayerCombat riderCombat;
        private Health riderHealth;
        private RiderHitRedirect hitRedirect;
        private InputReader input;
        private Transform homeParent;
        private Vector3 visualBasePosition;

        private State state = State.Stored;
        private float stateTimer;
        private float baseHeight;
        private Vector3 baseCenter;
        private bool bodyStretched;
        private float risen;

        private float heading;
        private float forwardSpeed;
        private Vector3 planarVelocity;
        private float verticalVelocity;
        private float measuredVerticalSpeed;
        private float groundClimbSpeed;
        private float yawRate;
        private float bank;
        private bool grounded;
        private float lastGroundedTime = -10f;
        private int driftDirection;
        private float driftTime;
        private float driftBoostTimer;
        private float driftBoostStrength;
        private float ramClearTimer;
        private float availableAt;
        private float storedSince;
        private Vector3 lastSafeGround;
        private Vector3 dismountVelocity;
        private string notice;
        private float noticeUntil;

        public static bool HandoffPending => Time.frameCount - handoffFrame <= 2;

        public VehicleDefinition Definition => definition;
        public PlayerController Rider => rider;
        public bool HasRider => state == State.Mounting || state == State.Riding;
        public bool IsRiding => state == State.Riding;
        public bool IsBoosting { get; private set; }
        public bool IsDrifting => driftDirection != 0;
        public bool IsTricking => tricks.IsActive;
        public int TrickCombo => tricks.Combo;
        public bool HasPassenger => passengerSeat != null && passengerSeat.IsOccupied;
        public PassengerSeat Seat => passengerSeat;
        public Health VehicleHealth => health;
        public float Speed => planarVelocity.magnitude;
        public float Speed01 => definition != null && definition.BoostSpeed > 0f ? Mathf.Clamp01(Speed / definition.BoostSpeed) : 0f;
        public bool IsAvailable => Time.time >= availableAt;
        public float CooldownRemaining => Mathf.Max(0f, availableAt - Time.time);
        public string DisplayName => definition != null && !string.IsNullOrEmpty(definition.displayName) ? definition.displayName : name;

        private Vector3 Forward => Quaternion.Euler(0f, heading, 0f) * Vector3.forward;

        private static int GroundMask
        {
            get
            {
                if (groundMask == 0)
                {
                    groundMask = Physics.DefaultRaycastLayers & ~LayerMask.GetMask(GameLayers.Player, GameLayers.PlayerAttack, GameLayers.EnemyAttack);
                }

                return groundMask;
            }
        }

        // One line for the HUD (empty when there is nothing to say).
        public string HudStatus
        {
            get
            {
                bool showNotice = Time.time < noticeUntil && !string.IsNullOrEmpty(notice);
                if (HasRider && health != null)
                {
                    string line = DisplayName + ": " + Mathf.CeilToInt(health.Current) + "/" + Mathf.CeilToInt(health.Max) + " PV | " + Mathf.RoundToInt(Speed * 3.6f) + " km/h";
                    if (IsBoosting)
                    {
                        line += " | TURBO";
                    }

                    if (IsDrifting)
                    {
                        line += " | DERRAPE";
                    }

                    if (definition.canTrick && tricks.Combo > 0)
                    {
                        line += " | Trucos x" + tricks.Combo;
                    }

                    if (HasPassenger)
                    {
                        line += " | Aelita a bordo";
                    }

                    return showNotice ? line + "  -  " + notice : line;
                }

                if (!IsAvailable)
                {
                    return DisplayName + " destruido: se rematerializa en " + Mathf.CeilToInt(CooldownRemaining) + " s";
                }

                return showNotice ? notice : string.Empty;
            }
        }

        public void Configure(VehicleDefinition newDefinition)
        {
            definition = newDefinition;
            if (health != null)
            {
                ApplyDefinition();
            }
        }

        private void Awake()
        {
            health = GetComponent<Health>();
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<VehicleDefinition>();
            }

            if (visual == null)
            {
                Transform found = transform.Find("Visual");
                visual = found != null ? found : transform;
            }

            if (passengerSeat == null)
            {
                passengerSeat = GetComponentInChildren<PassengerSeat>(true);
            }

            visualBasePosition = visual != transform ? visual.localPosition : Vector3.zero;
            ApplyDefinition();
            health.Died += OnVehicleDestroyed;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= OnVehicleDestroyed;
            }

            if (riderHealth != null)
            {
                riderHealth.Died -= OnRiderDied;
            }
        }

        private void ApplyDefinition()
        {
            health.SetTeam(Team.Player);
            health.SetDestroyOnDeath(false);
            health.SetMaxHealth(definition.maxHealth);
            health.SetHitInvulnerability(definition.hitInvulnerability);
            if (passengerSeat != null)
            {
                passengerSeat.Configure(definition);
            }
        }

        // Called once by the owner right after instantiating the vehicle under its socket.
        public void Bind(PlayerController owner)
        {
            rider = owner;
            riderBody = owner.GetComponent<CharacterController>();
            riderCombat = owner.GetComponent<PlayerCombat>();
            riderHealth = owner.GetComponent<Health>();
            hitRedirect = new RiderHitRedirect(owner.gameObject);
            homeParent = transform.parent != null ? transform.parent : owner.transform;
            baseHeight = riderBody.height;
            baseCenter = riderBody.center;
            if (riderHealth != null)
            {
                riderHealth.Died += OnRiderDied;
            }

            GameLayers.SetRecursively(gameObject, GameLayers.Player);
            Store();
        }

        // Starts riding: the vehicle flies in from the rider's side (instant = no animation, used for hand-offs).
        public bool TryMount(InputReader reader, Vector3 inheritedVelocity, bool instant)
        {
            if (rider == null || HasRider)
            {
                return false;
            }

            if (!IsAvailable)
            {
                ShowNotice(DisplayName + " se esta rematerializando", 2f);
                return false;
            }

            if (state == State.Dismounting)
            {
                Store();
            }

            input = reader;
            if (health.IsDead)
            {
                health.ResetHealth();
            }
            else
            {
                health.Heal(definition.regenPerSecondStored * (Time.time - storedSince));
            }

            transform.SetParent(homeParent, false);
            transform.localPosition = MountStartOffset;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one * 0.15f;
            gameObject.SetActive(true);
            health.Invulnerable = false;
            hitRedirect.Redirect(health);

            heading = rider.transform.eulerAngles.y;
            planarVelocity = Vector3.ProjectOnPlane(inheritedVelocity, Vector3.up);
            forwardSpeed = Mathf.Max(0f, Vector3.Dot(planarVelocity, Forward));
            verticalVelocity = 0f;
            driftDirection = 0;
            driftBoostTimer = 0f;
            tricks.Cancel();
            risen = 0f;
            stateTimer = 0f;
            lastSafeGround = rider.transform.position;
            state = State.Mounting;

            if (input != null)
            {
                input.SetVehicleMode(true);
            }

            if (instant)
            {
                CompleteMount();
            }
            else
            {
                ShowNotice(ControlsHint(), 4f);
            }

            return true;
        }

        // Called by the incoming warrior after a switch while mounted: it keeps flying on its own vehicle.
        public void TakeHandoff(InputReader reader)
        {
            if (!HandoffPending)
            {
                return;
            }

            Vector3 velocity = handoffVelocity;
            AelitaCompanion passenger = handoffPassenger;
            handoffFrame = -100;
            handoffPassenger = null;

            if (TryMount(reader, velocity, true) && passenger != null && passengerSeat != null && !passenger.IsSeated)
            {
                passengerSeat.Board(passenger);
            }
        }

        // handoff = the rider is being switched out: no animation, and the next warrior takes over.
        public void Dismount(bool handoff)
        {
            if (!HasRider)
            {
                return;
            }

            Vector3 velocity = planarVelocity + Vector3.up * verticalVelocity;
            AelitaCompanion passenger = ReleaseRider(handoff ? Vector3.zero : planarVelocity * 0.4f);

            if (handoff)
            {
                handoffFrame = Time.frameCount;
                handoffVelocity = velocity;
                handoffPassenger = passenger;
                Store();
                return;
            }

            // The empty vehicle fades out where it was left.
            state = State.Dismounting;
            stateTimer = 0f;
            dismountVelocity = velocity * 0.5f;
            health.Invulnerable = true;
            transform.SetParent(null, true);
        }

        // Every frame while mounted (PlayerController returns right after).
        public void Drive(InputReader reader, Transform cameraTransform, float deltaTime)
        {
            input = reader;
            if (deltaTime <= 0f || input == null)
            {
                return;
            }

            if (state == State.Mounting)
            {
                TickMount(deltaTime);
                return;
            }

            if (state != State.Riding)
            {
                return;
            }

            if (input.DismountPressed)
            {
                Dismount(false);
                return;
            }

            Vector3 desired = SteeringDirection(cameraTransform, out float throttle);
            bool wasGrounded = grounded;
            Steer(desired, throttle, deltaTime);

            if (definition.canFly)
            {
                Fly(cameraTransform, deltaTime);
                MoveBody(deltaTime);
                grounded = riderBody.isGrounded;
            }
            else
            {
                PrepareGroundMove(deltaTime);
                MoveBody(deltaTime);
                FinishGroundMove(wasGrounded);
            }

            TrackSafeGround();
            TickTricks();
            TickRam(deltaTime);
            TickCombat();
            TickPassenger(deltaTime);
            UpdateVisual(deltaTime);
        }

        public void ApplyKnockback(Vector3 velocity)
        {
            if (state == State.Riding)
            {
                planarVelocity += Vector3.ProjectOnPlane(velocity, Vector3.up) * 0.5f;
            }
        }

        private void Update()
        {
            if (state != State.Dismounting)
            {
                return;
            }

            float dt = Time.deltaTime;
            stateTimer += dt;
            float t = Mathf.Clamp01(stateTimer / Mathf.Max(0.01f, definition.dismountDuration));
            transform.position += dismountVelocity * dt;
            dismountVelocity = Vector3.MoveTowards(dismountVelocity, Vector3.zero, 20f * dt);
            transform.localScale = Vector3.one * (1f - t);
            if (visual != transform)
            {
                visual.localRotation *= Quaternion.Euler(0f, 720f * dt, 0f);
            }

            if (t >= 1f)
            {
                Store();
            }
        }

        // ---- Mounting ---------------------------------------------------------------

        private void TickMount(float dt)
        {
            stateTimer += dt;
            float t = Mathf.Clamp01(stateTimer / Mathf.Max(0.01f, definition.mountDuration));
            float eased = 1f - (1f - t) * (1f - t);

            transform.localPosition = Vector3.Lerp(MountStartOffset, Vector3.zero, eased);
            transform.localScale = Vector3.one * Mathf.Lerp(0.15f, 1f, eased);
            if (visual != transform)
            {
                visual.localRotation = Quaternion.Euler(0f, (1f - eased) * 360f, 0f);
            }

            // The rider hops onto the seat while keeping some of its momentum.
            float targetRise = definition.seatHeight * eased;
            planarVelocity = Vector3.MoveTowards(planarVelocity, Vector3.zero, 12f * dt);
            riderBody.Move(planarVelocity * dt + Vector3.up * (targetRise - risen));
            risen = targetRise;

            if (t >= 1f)
            {
                CompleteMount();
            }
        }

        private void CompleteMount()
        {
            if (state != State.Mounting)
            {
                return;
            }

            if (risen < definition.seatHeight)
            {
                riderBody.Move(Vector3.up * (definition.seatHeight - risen));
                risen = definition.seatHeight;
            }

            StretchRiderBody(true);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            if (visual != transform)
            {
                visual.localRotation = Quaternion.identity;
            }

            grounded = false;
            forwardSpeed = Mathf.Max(forwardSpeed, Vector3.Dot(planarVelocity, Forward));
            state = State.Riding;
        }

        // The capsule grows down to the hull so the vehicle (not the rider's feet) touches the ground.
        private void StretchRiderBody(bool stretched)
        {
            if (riderBody == null || bodyStretched == stretched)
            {
                return;
            }

            float extra = Mathf.Max(0f, definition.seatHeight);
            riderBody.height = stretched ? baseHeight + extra : baseHeight;
            riderBody.center = stretched ? baseCenter - Vector3.up * (extra * 0.5f) : baseCenter;
            bodyStretched = stretched;
        }

        // Gives the rider back its normal body. Returns the passenger who was on board, if any.
        private AelitaCompanion ReleaseRider(Vector3 carry)
        {
            state = State.Stored;
            IsBoosting = false;
            driftDirection = 0;
            tricks.Cancel();
            StretchRiderBody(false);
            if (hitRedirect != null)
            {
                hitRedirect.Restore();
            }

            if (input != null)
            {
                input.SetVehicleMode(false);
            }

            AelitaCompanion passenger = null;
            if (passengerSeat != null && passengerSeat.IsOccupied)
            {
                passenger = passengerSeat.Release(SafeDropPoint(passengerSeat.transform.position));
            }

            if (rider != null && rider.isActiveAndEnabled)
            {
                rider.ApplyKnockback(carry);
            }

            return passenger;
        }

        private void Store()
        {
            state = State.Stored;
            storedSince = Time.time;
            if (homeParent != null && transform.parent != homeParent)
            {
                transform.SetParent(homeParent, false);
            }

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            if (visual != transform)
            {
                visual.localRotation = Quaternion.identity;
                visual.localPosition = visualBasePosition;
            }

            if (health != null)
            {
                health.Invulnerable = true;
            }

            gameObject.SetActive(false);
        }

        // ---- Riding -------------------------------------------------------------------

        // Camera-relative like on foot: the nose turns toward where the stick points.
        private Vector3 SteeringDirection(Transform cameraTransform, out float throttle)
        {
            Vector2 stick = Vector2.ClampMagnitude(input.Steer, 1f);
            Vector3 camForward = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized : Forward;
            Vector3 camRight = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized : Vector3.Cross(Vector3.up, Forward);
            Vector3 desired = camForward * stick.y + camRight * stick.x;
            throttle = Mathf.Clamp01(desired.magnitude);
            return throttle > 0.0001f ? desired / desired.magnitude : Vector3.zero;
        }

        private void Steer(Vector3 desired, float throttle, float dt)
        {
            bool steering = throttle > 0.1f;
            IsBoosting = input.BoostHeld && steering;
            UpdateDrift(desired, steering, dt);

            float turnRate = VehicleMath.TurnRate(definition.turnRate, definition.highSpeedTurnFactor, Speed01);
            if (driftDirection != 0)
            {
                turnRate *= definition.driftTurnMultiplier;
            }

            float previous = heading;
            if (steering)
            {
                heading = VehicleMath.SteerYaw(heading, desired, turnRate * dt);
            }

            yawRate = Mathf.DeltaAngle(previous, heading) / dt;

            float alignment = steering ? Vector3.Dot(Forward, desired) : 1f;
            float target = VehicleMath.TargetSpeed(definition.cruiseSpeed, definition.boostMultiplier, IsBoosting, throttle, alignment, definition.cornerSpeedFactor);
            if (driftBoostTimer > 0f)
            {
                driftBoostTimer -= dt;
                target += definition.driftBoostSpeed * driftBoostStrength;
            }

            float acceleration = definition.acceleration * (IsBoosting ? 1.5f : 1f);
            forwardSpeed = VehicleMath.ApproachSpeed(forwardSpeed, target, acceleration, definition.drag, dt);

            float grip = driftDirection != 0 ? definition.driftGrip : definition.grip;
            if (!definition.canFly && !grounded)
            {
                grip *= 0.15f;
            }

            planarVelocity = Vector3.Lerp(planarVelocity, Forward * forwardSpeed, 1f - Mathf.Exp(-grip * dt));
        }

        // Overbike: hold Descend while turning on the ground to slide; letting go gives a mini-boost.
        private void UpdateDrift(Vector3 desired, bool steering, float dt)
        {
            bool wantsDrift = definition.canDrift && grounded && input.DescendHeld && steering && forwardSpeed >= definition.driftMinSpeed;
            if (wantsDrift)
            {
                if (driftDirection == 0)
                {
                    float side = Vector3.Dot(Vector3.Cross(Forward, desired), Vector3.up);
                    if (Mathf.Abs(side) > 0.15f)
                    {
                        driftDirection = side > 0f ? 1 : -1;
                        driftTime = 0f;
                    }
                }

                if (driftDirection != 0)
                {
                    driftTime += dt;
                }

                return;
            }

            if (driftDirection != 0)
            {
                driftBoostStrength = VehicleMath.DriftBoostStrength(driftTime, definition.driftFullChargeTime);
                if (driftBoostStrength > 0f)
                {
                    driftBoostTimer = definition.driftBoostDuration;
                    ShowNotice("Mini turbo!", 1f);
                }

                driftDirection = 0;
            }
        }

        private void Fly(Transform cameraTransform, float dt)
        {
            float climb = (input.AscendHeld ? 1f : 0f) - (input.DescendHeld ? 1f : 0f);
            float target = climb * definition.verticalSpeed;

            if (definition.pitchSteering > 0f && cameraTransform != null && forwardSpeed > 1f)
            {
                float pitch = Mathf.Asin(Mathf.Clamp(cameraTransform.forward.y, -1f, 1f)) * Mathf.Rad2Deg;
                float pitchClimb = VehicleMath.ClimbFromPitch(pitch, definition.neutralCameraPitch, definition.pitchDeadZone, definition.pitchRange);
                target += pitchClimb * definition.pitchSteering * forwardSpeed * 0.6f;
            }

            if (rider.transform.position.y >= definition.maxAltitude && target > 0f)
            {
                target = 0f;
            }

            verticalVelocity = Mathf.MoveTowards(verticalVelocity, target, definition.verticalAcceleration * dt);
        }

        private void PrepareGroundMove(float dt)
        {
            grounded = riderBody.isGrounded;
            if (grounded)
            {
                lastGroundedTime = Time.time;
                if (verticalVelocity < 0f)
                {
                    verticalVelocity = -2f;
                }
            }

            if (input.AscendPressed && Time.time - lastGroundedTime <= 0.15f && verticalVelocity <= 0.1f)
            {
                verticalVelocity = definition.jumpForce * (IsBoosting ? definition.boostJumpMultiplier : 1f);
                lastGroundedTime = -10f;
                grounded = false;
                driftDirection = 0;
            }

            if (!grounded)
            {
                verticalVelocity = Mathf.Max(verticalVelocity - definition.gravity * dt, -50f);
            }
        }

        private void MoveBody(float dt)
        {
            Vector3 before = rider.transform.position;
            riderBody.Move((planarVelocity + Vector3.up * verticalVelocity) * dt);
            measuredVerticalSpeed = (rider.transform.position.y - before.y) / dt;

            // Walls eat speed; ramps (most of the speed still moving) do not.
            Vector3 actualPlanar = Vector3.ProjectOnPlane(riderBody.velocity, Vector3.up);
            if (planarVelocity.sqrMagnitude > 4f && actualPlanar.magnitude < planarVelocity.magnitude * 0.6f)
            {
                planarVelocity = actualPlanar;
                forwardSpeed = Mathf.Min(forwardSpeed, actualPlanar.magnitude);
            }

            CollisionFlags flags = riderBody.collisionFlags;
            if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            {
                verticalVelocity = 0f;
            }

            if (definition.canFly && (flags & CollisionFlags.Below) != 0 && verticalVelocity < 0f)
            {
                verticalVelocity = 0f;
            }

            rider.transform.rotation = Quaternion.Euler(0f, heading, 0f);
        }

        // Stays glued over bumps and crests; leaves the ground with its real speed at the end of a ramp.
        private void FinishGroundMove(bool wasGrounded)
        {
            grounded = riderBody.isGrounded;
            if (grounded)
            {
                lastGroundedTime = Time.time;
                groundClimbSpeed = measuredVerticalSpeed;
                return;
            }

            if (!wasGrounded || verticalVelocity > 0f)
            {
                return;
            }

            float bottom = BottomHeight();
            Vector3 origin = new Vector3(rider.transform.position.x, bottom + 0.3f, rider.transform.position.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, definition.groundSnapDistance + 0.3f, GroundMask, QueryTriggerInteraction.Ignore) && hit.normal.y > 0.5f)
            {
                riderBody.Move(Vector3.down * Mathf.Max(0f, hit.distance - 0.3f + riderBody.skinWidth));
                grounded = riderBody.isGrounded;
                lastGroundedTime = Time.time;
                return;
            }

            verticalVelocity = Mathf.Max(verticalVelocity, groundClimbSpeed);
        }

        private float BottomHeight()
        {
            return rider.transform.position.y + riderBody.center.y - riderBody.height * 0.5f;
        }

        private void TrackSafeGround()
        {
            Vector3 origin = rider.transform.position + Vector3.up * 0.2f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, definition.seatHeight + 3f, GroundMask, QueryTriggerInteraction.Ignore) && hit.normal.y > 0.5f)
            {
                lastSafeGround = hit.point;
            }
        }

        private Vector3 SafeDropPoint(Vector3 from)
        {
            if (Physics.Raycast(from + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 80f, GroundMask, QueryTriggerInteraction.Ignore) && hit.normal.y > 0.5f)
            {
                return hit.point + Vector3.up * 0.05f;
            }

            return lastSafeGround + Vector3.up * 0.05f;
        }

        // ---- Tricks, ram, combat, passenger ---------------------------------------------

        private void TickTricks()
        {
            if (!definition.canTrick)
            {
                return;
            }

            float now = Time.time;
            bool airborne = definition.canFly || !grounded;

            if (definition.tricksOnlyAirborne && grounded && tricks.IsActive)
            {
                tricks.Cancel();
                forwardSpeed *= 0.6f;
                ShowNotice("Truco fallido!", 1.2f);
                return;
            }

            if (input.TrickPressed && (airborne || !definition.tricksOnlyAirborne))
            {
                tricks.TryStart(now, definition.trickDuration, definition.trickCooldown, PickTrick());
            }

            float multiplier = tricks.Tick(now, definition.trickComboWindow, definition.trickComboBonus, definition.maxTrickMultiplier);
            if (multiplier > 0f)
            {
                float gain = definition.trickMeterGain * multiplier;
                if (riderCombat != null && riderCombat.Meter != null)
                {
                    riderCombat.Meter.Add(gain);
                }

                ShowNotice(TrickName(tricks.Kind) + " x" + tricks.Combo + "  +" + Mathf.RoundToInt(gain) + " energia", 1.4f);
            }
        }

        private int PickTrick()
        {
            Vector2 stick = input.Steer;
            if (Mathf.Abs(stick.x) > 0.5f)
            {
                return stick.x > 0f ? 0 : 1;
            }

            if (stick.y < -0.5f)
            {
                return 2;
            }

            return tricks.Kind == 3 ? 4 : 3;
        }

        private static string TrickName(int kind)
        {
            switch (kind)
            {
                case 0:
                case 1:
                    return "Tonel";
                case 2:
                    return "Mortal atras";
                case 3:
                    return "Mortal";
                default:
                    return "Giro 360";
            }
        }

        private Quaternion TrickRotation()
        {
            if (!tricks.IsActive)
            {
                return Quaternion.identity;
            }

            float p = tricks.Progress(Time.time);
            float angle = 360f * p * p * (3f - 2f * p);
            switch (tricks.Kind)
            {
                case 0:
                    return Quaternion.Euler(0f, 0f, -angle);
                case 1:
                    return Quaternion.Euler(0f, 0f, angle);
                case 2:
                    return Quaternion.Euler(-angle, 0f, 0f);
                case 3:
                    return Quaternion.Euler(angle, 0f, 0f);
                default:
                    return Quaternion.Euler(0f, angle, 0f);
            }
        }

        // Overbike: hitting enemies at speed hurts them (through HitResolver, so teams are respected).
        private void TickRam(float dt)
        {
            if (definition.ramDamage <= 0f)
            {
                return;
            }

            ramClearTimer -= dt;
            if (ramClearTimer <= 0f)
            {
                ramHits.Clear();
                ramClearTimer = definition.ramRehitDelay;
            }

            float speed = Speed;
            float damage = VehicleMath.RamDamage(definition.ramDamage, speed, definition.ramMinSpeed, definition.BoostSpeed);
            if (damage <= 0f)
            {
                return;
            }

            Vector3 direction = planarVelocity / speed;
            Vector3 center = rider.transform.position + direction * (riderBody.radius + definition.ramRadius * 0.5f) + Vector3.up * 0.5f;
            int count = Physics.OverlapSphereNonAlloc(center, definition.ramRadius, Buffer, Physics.AllLayers, QueryTriggerInteraction.Collide);
            DamageInfo info = new DamageInfo
            {
                amount = damage,
                type = DamageType.Melee,
                source = rider.gameObject,
                sourceTeam = Team.Player,
                point = center,
                direction = direction,
                knockback = definition.ramKnockback,
                isHeavy = true
            };

            int hits = HitResolver.ApplyToColliders(Buffer, count, info, Team.Player, ramHits, false);
            if (hits > 0)
            {
                forwardSpeed *= definition.ramSpeedKept;
                planarVelocity *= definition.ramSpeedKept;
                CameraRig.Shake(0.5f);
                HitStop.Trigger(0.05f);
            }
        }

        private void TickCombat()
        {
            if (riderCombat == null)
            {
                return;
            }

            if (definition.riderCanShoot && input.FirePressed)
            {
                riderCombat.BasicAttack();
            }

            if (definition.riderCanUseSpecial && input.SpecialPressed)
            {
                riderCombat.SpecialAttack();
            }
        }

        private void TickPassenger(float dt)
        {
            if (passengerSeat == null)
            {
                return;
            }

            if (input.PassengerPressed)
            {
                if (passengerSeat.IsOccupied)
                {
                    passengerSeat.Release(SafeDropPoint(passengerSeat.transform.position));
                    ShowNotice("Aelita baja del " + DisplayName, 2f);
                }
                else if (passengerSeat.TryBoard())
                {
                    ShowNotice("Aelita sube al " + DisplayName, 2f);
                }
                else
                {
                    ShowNotice("Acercate a Aelita (menos de " + Mathf.RoundToInt(passengerSeat.BoardRadius) + " m) para que suba", 2.5f);
                }
            }

            passengerSeat.Tick(dt);
        }

        private void UpdateVisual(float dt)
        {
            if (visual == transform)
            {
                return;
            }

            float bankTarget = Mathf.Clamp(-yawRate / Mathf.Max(1f, definition.turnRate), -1f, 1f) * definition.bankAngle;
            bank = Mathf.Lerp(bank, bankTarget, 1f - Mathf.Exp(-8f * dt));
            float pitch = definition.canFly ? Mathf.Clamp(-verticalVelocity * 2f, -20f, 20f) : 0f;
            Quaternion lean = Quaternion.Euler(pitch, driftDirection * 25f, bank);
            visual.localRotation = lean * TrickRotation();

            float bob = definition.canFly ? Mathf.Sin(Time.time * definition.hoverRate) * definition.hoverBob : 0f;
            visual.localPosition = visualBasePosition + Vector3.up * bob;
        }

        // ---- Damage ---------------------------------------------------------------------

        private void OnVehicleDestroyed(Health dead)
        {
            Vector3 position = visual != null ? visual.position : transform.position;
            VehicleExplosion.Spawn(position, explosionColor);
            CameraRig.Shake(0.9f);
            availableAt = Time.time + definition.respawnCooldown;
            Debug.Log("[Vehicle] " + DisplayName + " destroyed.");

            if (rider != null && definition.explosionDamage > 0f)
            {
                int count = Physics.OverlapSphereNonAlloc(position, definition.explosionRadius, Buffer, Physics.AllLayers, QueryTriggerInteraction.Collide);
                DamageInfo blast = new DamageInfo
                {
                    amount = definition.explosionDamage,
                    type = DamageType.Ability,
                    source = rider.gameObject,
                    sourceTeam = Team.Player,
                    point = position,
                    knockback = 6f,
                    isHeavy = true
                };
                HitResolver.ApplyToColliders(Buffer, count, blast, Team.Player, null, false);
            }

            if (HasRider)
            {
                ReleaseRider(planarVelocity * 0.3f);
            }

            ShowNotice(DisplayName + " destruido!", 2f);
            Store();
        }

        private void OnRiderDied(Health dead)
        {
            if (HasRider)
            {
                ReleaseRider(Vector3.zero);
                Store();
            }
        }

        // ---- Helpers --------------------------------------------------------------------

        private void ShowNotice(string text, float seconds)
        {
            notice = text;
            noticeUntil = Time.time + seconds;
        }

        private string ControlsHint()
        {
            string hint = "Shift: turbo | V: bajar";
            if (definition.canFly)
            {
                hint = "Espacio/Ctrl: subir/bajar | " + hint;
            }
            else
            {
                hint = "Espacio: salto" + (definition.canDrift ? " | Ctrl: derrape" : string.Empty) + " | " + hint;
            }

            if (definition.canTrick)
            {
                hint += " | C: truco";
            }

            if (passengerSeat != null)
            {
                hint += " | E: Aelita";
            }

            return hint;
        }
    }
}
