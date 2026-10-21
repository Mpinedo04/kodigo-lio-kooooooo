using UnityEngine;

namespace CodeLyokoFanGame
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class AelitaCompanion : MonoBehaviour
    {
        [SerializeField] private CharacterSwitcher switcher;
        [SerializeField] private float followDistance = 3.4f;
        [SerializeField] private float moveSpeed = 5.2f;
        [SerializeField] private float shieldRadius = 5f;
        [SerializeField] private float shieldCooldown = 8f;
        [Tooltip("Aelita never walks off a ledge: she waits until there is ground ahead (the Digital Sea is fatal).")]
        [SerializeField] private float edgeCheckDistance = 0.6f;
        [SerializeField] private float edgeDropHeight = 1.5f;

        private CharacterController controller;
        private float shieldTimer;

        public bool IsNearTower(TowerObjective tower)
        {
            if (tower == null)
            {
                return false;
            }

            // Horizontal distance only: the tower pivot sits above the ground.
            Vector3 flat = Vector3.ProjectOnPlane(tower.transform.position - transform.position, Vector3.up);
            return flat.magnitude <= tower.RequiredAelitaDistance;
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (TryGetComponent(out Health health))
            {
                health.SetTeam(Team.Player);
            }
        }

        private void Update()
        {
            shieldTimer -= Time.deltaTime;

            if (switcher == null || switcher.ActiveCharacter == null)
            {
                return;
            }

            Transform target = switcher.ActiveCharacter.transform;
            Vector3 toTarget = target.position - transform.position;
            Vector3 flat = Vector3.ProjectOnPlane(toTarget, Vector3.up);

            if (flat.magnitude > followDistance && HasGroundAhead(flat.normalized))
            {
                controller.Move(flat.normalized * moveSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat.normalized), Time.deltaTime * 8f);
            }

            controller.Move(Vector3.down * 12f * Time.deltaTime);

            if (shieldTimer <= 0f)
            {
                Collider[] nearby = Physics.OverlapSphere(transform.position, shieldRadius, Physics.AllLayers, QueryTriggerInteraction.Collide);
                DamageInfo pulse = new DamageInfo { amount = 10f, type = DamageType.Ability, source = gameObject, sourceTeam = Team.Player };
                HitResolver.ApplyToColliders(nearby, nearby.Length, pulse, Team.Player, null, false);

                shieldTimer = shieldCooldown;
            }
        }

        private bool HasGroundAhead(Vector3 direction)
        {
            Vector3 probe = transform.position + direction * (controller.radius + edgeCheckDistance) + Vector3.up * 0.5f;
            return Physics.Raycast(probe, Vector3.down, edgeDropHeight + 0.5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }
    }
}
