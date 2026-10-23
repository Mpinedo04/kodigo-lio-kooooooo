using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CodeLyokoFanGame
{
    // Aelita as an escorted AI companion (F07): follows orders (follow, wait, hide, go to the tower), walks along the
    // NavMesh when there is one, keeps away from monsters, shoots energy fields and never steps into the Digital Sea.
    [RequireComponent(typeof(CharacterController))]
    public sealed class AelitaCompanion : MonoBehaviour
    {
        private const float Gravity = 26f;
        private const float RepathInterval = 0.3f;

        [SerializeField] private CharacterSwitcher switcher;
        [SerializeField] private float followDistance = 3.4f;
        [SerializeField] private float moveSpeed = 5.2f;
        [SerializeField] private float runSpeed = 7.5f;
        [SerializeField] private float shieldRadius = 5f;
        [SerializeField] private float shieldCooldown = 8f;
        [Tooltip("Aelita never walks off a ledge: she waits until there is ground ahead (the Digital Sea is fatal).")]
        [SerializeField] private float edgeCheckDistance = 0.6f;
        [SerializeField] private float edgeDropHeight = 1.5f;

        [Header("Escort (F07)")]
        [Tooltip("Monsters closer than this push her away from them.")]
        [SerializeField] private float enemyAvoidRadius = 7f;
        [Tooltip("Hide: how far behind the covering warrior she stands.")]
        [SerializeField] private float hideDistance = 2.2f;
        [Tooltip("Go to the tower: she stops this close to it.")]
        [SerializeField] private float towerStopDistance = 3f;
        [Tooltip("Monsters pick her this much more than a companion (the controlled warrior counts x1.5).")]
        [SerializeField] private float aggroPriority = 2f;
        [Tooltip("Warning on the HUD below this share of health.")]
        [Range(0f, 1f)] [SerializeField] private float dangerHealth01 = 0.4f;

        [Header("Energy fields (F5.22)")]
        [SerializeField] private Projectile energyFieldPrefab;
        [SerializeField] private float energyFieldDamage = 10f;
        [SerializeField] private float energyFieldInterval = 1.8f;
        [SerializeField] private float energyFieldRange = 14f;

        private readonly List<Vector3> pathCorners = new List<Vector3>();
        private NavMeshPath path;
        private CharacterController controller;
        private Health health;
        private TowerObjective tower;
        private Transform seat;
        private Vector3 waitPoint;
        private float shieldTimer;
        private float fieldTimer;
        private float repathTimer;
        private float verticalVelocity;
        private float lastHurtTime = float.NegativeInfinity;

        public AelitaOrder Order { get; private set; } = AelitaOrder.Follow;
        public float Health01 => health != null ? health.Current / health.Max : 1f;

        // On the HUD: low health, just hit, or a monster right next to her.
        public bool InDanger => health != null && !health.IsDead
            && (Health01 <= dangerHealth01 || Time.time - lastHurtTime < 1.5f || AbilityMath.CountEnemies(transform.position, 5f) > 0);

        // Riding as a vehicle passenger (F08): she follows the seat and does not walk.
        public bool IsSeated => seat != null;

        public void SetOrder(AelitaOrder order)
        {
            Order = order;
            repathTimer = 0f;
            if (order == AelitaOrder.Wait)
            {
                waitPoint = transform.position;
            }

            Debug.Log("[Aelita] Orden: " + order);
        }

        public void BoardSeat(Transform vehicleSeat)
        {
            seat = vehicleSeat;
            if (controller != null)
            {
                controller.enabled = false;
            }

            FollowSeat();
        }

        public void LeaveSeat(Vector3 groundPosition)
        {
            seat = null;
            transform.position = groundPosition;
            if (controller != null)
            {
                controller.enabled = true;
            }
        }

        public bool IsNearTower(TowerObjective activatedTower)
        {
            if (activatedTower == null)
            {
                return false;
            }

            // Horizontal distance only: the tower pivot sits above the ground.
            Vector3 flat = Vector3.ProjectOnPlane(activatedTower.transform.position - transform.position, Vector3.up);
            return flat.magnitude <= activatedTower.RequiredAelitaDistance;
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            path = new NavMeshPath();
            if (TryGetComponent(out health))
            {
                health.SetTeam(Team.Player);
                health.Damaged += OnDamaged;
            }

            // X.A.N.A. goes after her often (F7.3): the CombatDirector weighs this priority.
            if (!TryGetComponent(out AggroTarget aggro))
            {
                aggro = gameObject.AddComponent<AggroTarget>();
            }

            aggro.SetPriority(aggroPriority);
        }

        private void Start()
        {
            tower = FindObjectOfType<TowerObjective>();
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Damaged -= OnDamaged;
            }
        }

        private void OnDamaged(Health damaged, DamageInfo info)
        {
            lastHurtTime = Time.time;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            shieldTimer -= dt;
            fieldTimer -= dt;

            if (seat != null || dt <= 0f || !controller.enabled)
            {
                return;
            }

            PlayerController leader = switcher != null ? switcher.ActiveCharacter : null;
            Vector3 move = Vector3.zero;
            float distanceToGoal = 0f;
            if (TryGetGoal(leader, out Vector3 goal, out float stopDistance))
            {
                distanceToGoal = Vector3.ProjectOnPlane(goal - transform.position, Vector3.up).magnitude;
                if (distanceToGoal > stopDistance)
                {
                    move = Vector3.ProjectOnPlane(NextWaypoint(goal) - transform.position, Vector3.up).normalized;
                }
            }

            move = AvoidMonsters(move);
            Walk(move, InDanger || distanceToGoal > followDistance * 3f ? runSpeed : moveSpeed, dt);

            ShootEnergyField();
            ShieldPulse();
        }

        private void LateUpdate()
        {
            if (seat == null)
            {
                return;
            }

            if (!seat.gameObject.activeInHierarchy)
            {
                LeaveSeat(transform.position);
                return;
            }

            FollowSeat();
        }

        private void FollowSeat()
        {
            transform.SetPositionAndRotation(seat.position, Quaternion.Euler(0f, seat.eulerAngles.y, 0f));
        }

        private bool TryGetGoal(PlayerController leader, out Vector3 goal, out float stopDistance)
        {
            switch (Order)
            {
                case AelitaOrder.Wait:
                    goal = waitPoint;
                    stopDistance = 0.6f;
                    return true;
                case AelitaOrder.Hide:
                    if (TryGetHidingSpot(out goal))
                    {
                        stopDistance = 0.6f;
                        return true;
                    }

                    break;
                case AelitaOrder.GoToTower:
                    if (tower != null && !tower.IsCleared)
                    {
                        goal = tower.transform.position;
                        stopDistance = Mathf.Min(towerStopDistance, tower.RequiredAelitaDistance * 0.8f);
                        return true;
                    }

                    break;
            }

            goal = leader != null ? leader.transform.position : transform.position;
            stopDistance = followDistance;
            return leader != null;
        }

        // Behind the warrior closest to her, on the side away from the nearest monster.
        private bool TryGetHidingSpot(out Vector3 spot)
        {
            spot = transform.position;
            Health monster = AbilityMath.NearestEnemy(transform.position, 25f);
            PlayerController cover = null;
            float best = float.MaxValue;
            IReadOnlyList<PlayerController> warriors = PlayerController.All;
            for (int i = 0; i < warriors.Count; i++)
            {
                float distance = (warriors[i].transform.position - transform.position).sqrMagnitude;
                if (distance < best)
                {
                    best = distance;
                    cover = warriors[i];
                }
            }

            if (monster == null || cover == null)
            {
                return false;
            }

            Vector3 away = Vector3.ProjectOnPlane(cover.transform.position - monster.transform.position, Vector3.up);
            spot = cover.transform.position + (away.sqrMagnitude > 0.01f ? away.normalized : -cover.transform.forward) * hideDistance;
            return true;
        }

        // Next corner of the NavMesh path when the level has one; straight at the goal otherwise.
        private Vector3 NextWaypoint(Vector3 goal)
        {
            repathTimer -= Time.deltaTime;
            if (repathTimer <= 0f)
            {
                repathTimer = RepathInterval;
                pathCorners.Clear();
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit from, 2f, NavMesh.AllAreas)
                    && NavMesh.SamplePosition(goal, out NavMeshHit to, 3f, NavMesh.AllAreas)
                    && NavMesh.CalculatePath(from.position, to.position, NavMesh.AllAreas, path)
                    && path.status != NavMeshPathStatus.PathInvalid)
                {
                    pathCorners.AddRange(path.corners);
                }
            }

            for (int i = 0; i < pathCorners.Count; i++)
            {
                if (Vector3.ProjectOnPlane(pathCorners[i] - transform.position, Vector3.up).sqrMagnitude > 0.36f)
                {
                    return pathCorners[i];
                }
            }

            return goal;
        }

        private Vector3 AvoidMonsters(Vector3 move)
        {
            Vector3 push = Vector3.zero;
            IReadOnlyList<Health> all = Health.All;
            for (int i = 0; i < all.Count; i++)
            {
                if (!AbilityMath.IsEnemy(all[i]))
                {
                    continue;
                }

                Vector3 away = Vector3.ProjectOnPlane(transform.position - all[i].transform.position, Vector3.up);
                float distance = away.magnitude;
                if (distance < enemyAvoidRadius && distance > 0.01f)
                {
                    push += away / distance * (1f - distance / enemyAvoidRadius);
                }
            }

            if (push.sqrMagnitude < 0.0001f)
            {
                return move;
            }

            Vector3 blended = move + push * 1.2f;
            return blended.sqrMagnitude > 1f ? blended.normalized : blended;
        }

        private void Walk(Vector3 move, float speed, float dt)
        {
            Vector3 horizontal = move * speed;
            if (horizontal.sqrMagnitude > 0.01f && !HasGroundAhead(horizontal.normalized))
            {
                horizontal = Vector3.zero;
            }

            verticalVelocity = controller.isGrounded && verticalVelocity < 0f ? -2f : Mathf.Max(verticalVelocity - Gravity * dt, -40f);
            controller.Move((horizontal + Vector3.up * verticalVelocity) * dt);

            if (horizontal.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(horizontal), dt * 8f);
            }
        }

        // Slow homing energy fields at the nearest monster; doubled behind Odd's shield (synergy).
        private void ShootEnergyField()
        {
            if (energyFieldPrefab == null || fieldTimer > 0f)
            {
                return;
            }

            Health target = AbilityMath.NearestEnemy(transform.position, energyFieldRange);
            if (target == null)
            {
                return;
            }

            fieldTimer = energyFieldInterval;
            EnemyAI enemy = target.GetComponent<EnemyAI>();
            Transform aim = enemy != null && enemy.AimPoint != null ? enemy.AimPoint : target.transform;
            Vector3 origin = transform.position + Vector3.up * 1.2f;
            Vector3 direction = (aim.position - origin).normalized;
            float boost = TeamSynergy.EnergyFieldMultiplier(transform.position);

            DamageInfo info = new DamageInfo
            {
                amount = energyFieldDamage * boost,
                type = DamageType.Projectile,
                source = gameObject,
                sourceTeam = Team.Player,
                knockback = 1f
            };

            Projectile field = PrefabPool.Spawn(energyFieldPrefab, origin + direction * 0.5f, Quaternion.LookRotation(direction));
            field.Launch(gameObject, Team.Player, direction, info, aim, 3f, 0, Mathf.Sqrt(boost), 0f, 0.55f);
        }

        private void ShieldPulse()
        {
            if (shieldTimer > 0f)
            {
                return;
            }

            Collider[] nearby = Physics.OverlapSphere(transform.position, shieldRadius, Physics.AllLayers, QueryTriggerInteraction.Collide);
            DamageInfo pulse = new DamageInfo { amount = 10f, type = DamageType.Ability, source = gameObject, sourceTeam = Team.Player };
            HitResolver.ApplyToColliders(nearby, nearby.Length, pulse, Team.Player, null, false);
            shieldTimer = shieldCooldown;
        }

        private bool HasGroundAhead(Vector3 direction)
        {
            Vector3 probe = transform.position + direction * (controller.radius + edgeCheckDistance) + Vector3.up * 0.5f;
            return Physics.Raycast(probe, Vector3.down, edgeDropHeight + 0.5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }
    }
}
