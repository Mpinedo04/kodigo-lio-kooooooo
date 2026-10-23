using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Back seat of a vehicle (Overwing). Aelita rides here and shoots at the nearest enemy on her own.
    public sealed class PassengerSeat : MonoBehaviour
    {
        private static readonly List<Vector3> CandidatePositions = new List<Vector3>();

        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private float boardRadius = 7f;
        [SerializeField] private float fireInterval = 1.1f;
        [SerializeField] private float fireRange = 24f;
        [SerializeField] private float damage = 10f;

        private AelitaCompanion passenger;
        private float fireTimer;

        public bool IsOccupied => passenger != null;
        public AelitaCompanion Passenger => passenger;
        public float BoardRadius => boardRadius;

        public void Configure(VehicleDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            boardRadius = definition.passengerBoardRadius;
            fireInterval = definition.passengerFireInterval;
            fireRange = definition.passengerFireRange;
            damage = definition.passengerDamage;
        }

        public void SetProjectile(Projectile prefab)
        {
            projectilePrefab = prefab;
        }

        // Boards the companion if she is close enough (horizontal distance).
        public bool TryBoard()
        {
            if (IsOccupied)
            {
                return false;
            }

            AelitaCompanion companion = FindObjectOfType<AelitaCompanion>();
            if (companion == null || companion.IsSeated || !companion.isActiveAndEnabled)
            {
                return false;
            }

            if (companion.TryGetComponent(out Health companionHealth) && companionHealth.IsDead)
            {
                return false;
            }

            Vector3 flat = Vector3.ProjectOnPlane(companion.transform.position - transform.position, Vector3.up);
            if (flat.magnitude > boardRadius)
            {
                return false;
            }

            Board(companion);
            return true;
        }

        public void Board(AelitaCompanion companion)
        {
            if (companion == null || IsOccupied)
            {
                return;
            }

            passenger = companion;
            passenger.BoardSeat(transform);
            fireTimer = fireInterval * 0.5f;
        }

        public AelitaCompanion Release(Vector3 dropPosition)
        {
            AelitaCompanion leaving = passenger;
            passenger = null;
            if (leaving != null)
            {
                leaving.LeaveSeat(dropPosition);
            }

            return leaving;
        }

        public void Tick(float deltaTime)
        {
            if (passenger == null || projectilePrefab == null)
            {
                return;
            }

            fireTimer -= deltaTime;
            if (fireTimer > 0f)
            {
                return;
            }

            Vector3 origin = transform.position + Vector3.up * 1.2f;
            if (!FindTarget(origin, out Vector3 aim))
            {
                fireTimer = 0.25f;
                return;
            }

            fireTimer = fireInterval;
            Vector3 direction = (aim - origin).normalized;
            DamageInfo info = new DamageInfo
            {
                amount = damage,
                type = DamageType.Projectile,
                source = passenger.gameObject,
                sourceTeam = Team.Player,
                direction = direction,
                knockback = 1f
            };

            Projectile projectile = PrefabPool.Spawn(projectilePrefab, origin + direction * 0.4f, Quaternion.LookRotation(direction));
            projectile.Launch(passenger.gameObject, Team.Player, direction, info);
        }

        private bool FindTarget(Vector3 origin, out Vector3 aim)
        {
            CandidatePositions.Clear();
            int blockers = LayerMask.GetMask(GameLayers.Environment);
            IReadOnlyList<Health> all = Health.All;
            for (int i = 0; i < all.Count; i++)
            {
                Health health = all[i];
                if (health == null || health.IsDead || health.Team != Team.Enemy)
                {
                    continue;
                }

                Vector3 point = AimPoint(health);
                if (Physics.Linecast(origin, point, blockers, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                CandidatePositions.Add(point);
            }

            int best = VehicleMath.PickNearest(origin, CandidatePositions, fireRange);
            aim = best >= 0 ? CandidatePositions[best] : Vector3.zero;
            return best >= 0;
        }

        // Middle of the target's body (its first collider), or just above its pivot.
        private static Vector3 AimPoint(Health health)
        {
            Collider body = health.GetComponentInChildren<Collider>();
            return body != null ? body.bounds.center : health.transform.position + Vector3.up;
        }
    }
}
