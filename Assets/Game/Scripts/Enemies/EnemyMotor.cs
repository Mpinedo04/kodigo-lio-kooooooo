using UnityEngine;
using UnityEngine.AI;

namespace CodeLyokoFanGame
{
    // Moves a monster. Ground units walk on the NavMesh when there is one (they can never walk off
    // a ledge) and otherwise use simple movement that refuses to step where there is no ground.
    // Flyers move freely at their cruise altitude. Telekinesis can suspend all of it.
    public sealed class EnemyMotor
    {
        private const float Gravity = 22f;
        private const float GroundProbeHeight = 0.6f;
        private const float NavMeshSnapDistance = 2.5f;

        private readonly EnemyAI host;
        private readonly Transform transform;
        private NavMeshAgent agent;
        private StatusEffects status;
        private Vector3 knockback;
        private Vector3 lastDestination = Vector3.positiveInfinity;
        private Vector3 lastPosition;
        private float verticalSpeed;
        private bool airborne;
        private bool initialized;

        public EnemyMotor(EnemyAI host, bool flying)
        {
            this.host = host;
            transform = host.transform;
            Flying = flying;
            Home = transform.position;
            HomeGroundY = transform.position.y;
            lastPosition = transform.position;
        }

        public bool Flying { get; }
        public bool Suspended { get; private set; }
        public bool UsesNavMesh => agent != null && agent.enabled && agent.isOnNavMesh;
        public Vector3 Home { get; private set; }
        public float HomeGroundY { get; private set; }
        public float CruiseAltitude => HomeGroundY + host.Definition.hoverHeight;
        public float DesiredAltitude { get; set; }
        public Vector3 Velocity { get; private set; }
        public float FlatSpeed => Vector3.ProjectOnPlane(Velocity, Vector3.up).magnitude;
        public bool IsRooted => Status != null && Status.IsRooted;

        private StatusEffects Status
        {
            get
            {
                if (status == null)
                {
                    host.TryGetComponent(out status);
                }

                return status;
            }
        }

        private float SpeedMultiplier => Status != null ? Status.SpeedMultiplier : 1f;

        // Called from Start, when the scene's NavMesh is already loaded.
        public void Initialize()
        {
            Home = transform.position;
            HomeGroundY = TryGround(transform.position, 60f, out float groundY) ? groundY : transform.position.y;
            DesiredAltitude = CruiseAltitude;
            lastPosition = transform.position;
            initialized = true;

            if (!Flying)
            {
                TryAttachAgent();
            }
        }

        public void MoveTowards(Vector3 destination, float speedScale, float deltaTime)
        {
            if (!initialized || Suspended)
            {
                return;
            }

            float speed = host.Definition.moveSpeed * speedScale * SpeedMultiplier;
            if (IsRooted || speed <= 0.01f)
            {
                Stop();
                return;
            }

            if (UsesNavMesh)
            {
                agent.speed = speed;
                agent.isStopped = false;
                if ((destination - lastDestination).sqrMagnitude > 0.25f)
                {
                    lastDestination = destination;
                    Vector3 goal = NavMesh.SamplePosition(destination, out NavMeshHit hit, 4f, NavMesh.AllAreas) ? hit.position : destination;
                    agent.SetDestination(goal);
                }

                return;
            }

            if (Flying)
            {
                Vector3 goal = new Vector3(destination.x, DesiredAltitude, destination.z);
                transform.position = Vector3.MoveTowards(transform.position, goal, speed * deltaTime);
                return;
            }

            Vector3 flat = Vector3.ProjectOnPlane(destination - transform.position, Vector3.up);
            float distance = flat.magnitude;
            if (distance < 0.2f)
            {
                return;
            }

            Vector3 next = transform.position + flat / distance * Mathf.Min(distance, speed * deltaTime);
            if (HasGroundAt(next))
            {
                transform.position = next;
            }
        }

        public void Stop()
        {
            lastDestination = Vector3.positiveInfinity;
            if (UsesNavMesh)
            {
                agent.isStopped = true;
                if (agent.hasPath)
                {
                    agent.ResetPath();
                }
            }
        }

        // Turns on the vertical axis only.
        public void FaceTowards(Vector3 point, float degreesPerSecond, float deltaTime)
        {
            FaceDirection(point - transform.position, degreesPerSecond, deltaTime);
        }

        public void FaceDirection(Vector3 direction, float degreesPerSecond, float deltaTime)
        {
            if (Suspended)
            {
                return;
            }

            Vector3 flat = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (flat.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion goal = Quaternion.LookRotation(flat.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, degreesPerSecond * deltaTime);
        }

        public void FaceMovement(float degreesPerSecond, float deltaTime)
        {
            if (FlatSpeed > 0.2f)
            {
                FaceDirection(Velocity, degreesPerSecond, deltaTime);
            }
        }

        public void AddKnockback(Vector3 velocity)
        {
            if (!Suspended)
            {
                knockback += velocity;
            }
        }

        // Telekinesis: on suspend the agent lets go of the transform; on release ground units fall
        // until they land and then snap back onto the NavMesh (or use simple movement if there is none).
        public void SetSuspended(bool value)
        {
            if (Suspended == value)
            {
                return;
            }

            Suspended = value;
            knockback = Vector3.zero;
            verticalSpeed = 0f;

            if (value)
            {
                if (agent != null)
                {
                    agent.enabled = false;
                }

                return;
            }

            lastPosition = transform.position;
            if (Flying)
            {
                DesiredAltitude = CruiseAltitude;
                return;
            }

            airborne = true;
        }

        public void Disable()
        {
            knockback = Vector3.zero;
            if (agent != null)
            {
                agent.enabled = false;
            }
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            if (Suspended)
            {
                Velocity = Vector3.zero;
                lastPosition = transform.position;
                return;
            }

            ApplyKnockback(deltaTime);

            if (!Flying && !UsesNavMesh)
            {
                ApplyGravity(deltaTime);
            }

            Velocity = (transform.position - lastPosition) / deltaTime;
            lastPosition = transform.position;
        }

        private void ApplyKnockback(float deltaTime)
        {
            if (knockback.sqrMagnitude < 0.01f)
            {
                knockback = Vector3.zero;
                return;
            }

            Vector3 delta = knockback * deltaTime;
            if (UsesNavMesh)
            {
                agent.Move(delta);
            }
            else if (Flying || HasGroundAt(transform.position + delta))
            {
                transform.position += delta;
            }

            knockback = Vector3.MoveTowards(knockback, Vector3.zero, 30f * deltaTime);
        }

        // Simple movement: stick to the ground below; thrown monsters keep falling (even into the Digital Sea).
        private void ApplyGravity(float deltaTime)
        {
            Vector3 position = transform.position;
            if (TryGround(position, airborne ? 3f : 50f, out float groundY))
            {
                if (position.y > groundY + 0.02f)
                {
                    verticalSpeed += Gravity * deltaTime;
                    position.y = Mathf.Max(groundY, position.y - verticalSpeed * deltaTime);
                    transform.position = position;
                }

                if (transform.position.y <= groundY + 0.02f)
                {
                    verticalSpeed = 0f;
                    if (airborne)
                    {
                        airborne = false;
                        TryAttachAgent();
                    }
                }

                return;
            }

            if (airborne)
            {
                verticalSpeed += Gravity * deltaTime;
                transform.position = position + Vector3.down * verticalSpeed * deltaTime;
            }
        }

        private bool TryAttachAgent()
        {
            if (Flying || !Application.isPlaying)
            {
                return false;
            }

            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, NavMeshSnapDistance, NavMesh.AllAreas))
            {
                return false;
            }

            transform.position = hit.position;
            if (agent == null && !host.TryGetComponent(out agent))
            {
                agent = host.gameObject.AddComponent<NavMeshAgent>();
            }

            agent.enabled = true;
            agent.baseOffset = 0f;
            agent.radius = Mathf.Max(0.2f, host.Definition.bodyRadius);
            agent.height = 2f;
            agent.speed = host.Definition.moveSpeed;
            agent.acceleration = 30f;
            agent.angularSpeed = 0f;
            agent.updateRotation = false;
            agent.stoppingDistance = 0.4f;
            agent.autoBraking = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            agent.avoidancePriority = Random.Range(30, 70);
            agent.Warp(hit.position);
            lastDestination = Vector3.positiveInfinity;
            return true;
        }

        private static bool TryGround(Vector3 position, float maxDistance, out float groundY)
        {
            Vector3 origin = position + Vector3.up * GroundProbeHeight;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxDistance + GroundProbeHeight, EnemyPerception.EnvironmentMask, QueryTriggerInteraction.Ignore))
            {
                groundY = hit.point.y;
                return true;
            }

            groundY = position.y;
            return false;
        }

        private static bool HasGroundAt(Vector3 position)
        {
            return TryGround(position, 2.5f, out _);
        }
    }
}
