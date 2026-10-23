using UnityEngine;

namespace CodeLyokoFanGame
{
    // One of Ulrich's clones: runs at the nearest monster and slashes softly. One point of life,
    // so the first hit makes it vanish. Monsters target it like any warrior.
    public sealed class UlrichClone : MonoBehaviour
    {
        private const float AttackRange = 1.7f;
        private const float AttackInterval = 0.9f;
        private const float Gravity = 20f;

        private static readonly Collider[] Buffer = new Collider[16];

        private PlayerController original;
        private Health health;
        private Transform focus;
        private float damage;
        private float moveSpeed;
        private float searchRange;
        private float side;
        private float attackTimer;
        private float fallSpeed;

        public bool IsGone => health == null || health.IsDead;

        public static UlrichClone Spawn(PlayerController original, Vector3 position, Quaternion rotation, float hitPoints, float damage, float speed, float searchRange, float side, float aggroPriority)
        {
            GameObject root = new GameObject(original.name + "_Clone");
            root.transform.SetPositionAndRotation(position, rotation);

            CapsuleCollider capsule = root.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            capsule.radius = 0.38f;
            capsule.center = Vector3.up;
            Rigidbody body = root.AddComponent<Rigidbody>();
            body.isKinematic = true;

            Health health = root.AddComponent<Health>();
            health.SetTeam(Team.Player);
            health.SetMaxHealth(Mathf.Max(1f, hitPoints));
            health.SetDestroyOnDeath(true);
            root.AddComponent<AggroTarget>().SetPriority(aggroPriority);

            WarriorModel.Build(LyokoCharacter.Ulrich, root.transform);
            GameLayers.SetRecursively(root, GameLayers.Player);

            root.AddComponent<ProceduralAnimator>();
            UlrichClone clone = root.AddComponent<UlrichClone>();
            clone.original = original;
            clone.health = health;
            clone.damage = damage;
            clone.moveSpeed = speed > 0f ? speed : 8f;
            clone.searchRange = searchRange > 0f ? searchRange : 16f;
            clone.side = side;
            DevirtualizeEffect.Spawn(position + Vector3.up, AbilityVisuals.UlrichGreen);
            return clone;
        }

        public void Teleport(Vector3 position, Transform newFocus)
        {
            if (AbilityMath.TryFindGround(position, 3f, out Vector3 ground))
            {
                position = ground;
            }

            transform.position = position;
            focus = newFocus;
            Face(newFocus != null ? newFocus.position - position : transform.forward);
        }

        public void Vanish()
        {
            if (this == null || IsGone)
            {
                return;
            }

            DevirtualizeEffect.Spawn(transform.position + Vector3.up, AbilityVisuals.UlrichGreen);
            Destroy(gameObject);
        }

        private void Update()
        {
            if (IsGone)
            {
                return;
            }

            float dt = Time.deltaTime;
            Health enemy = focus != null && focus.gameObject.activeInHierarchy ? focus.GetComponentInParent<Health>() : null;
            if (!AbilityMath.IsEnemy(enemy))
            {
                enemy = AbilityMath.NearestEnemy(transform.position, searchRange);
            }

            Vector3 goal = enemy != null
                ? enemy.transform.position
                : original != null ? original.transform.position + original.transform.right * side : transform.position;
            Vector3 flat = Vector3.ProjectOnPlane(goal - transform.position, Vector3.up);
            float distance = flat.magnitude;
            float stopAt = enemy != null ? AttackRange * 0.85f : 1.2f;

            if (distance > stopAt && HasGroundAhead(flat / Mathf.Max(0.001f, distance)))
            {
                transform.position += flat / distance * Mathf.Min(moveSpeed * dt, distance - stopAt);
            }

            Face(flat);
            StickToGround(dt);

            attackTimer -= dt;
            if (enemy != null && distance <= AttackRange && attackTimer <= 0f)
            {
                Slash();
                attackTimer = AttackInterval;
            }
        }

        private void Slash()
        {
            DamageInfo info = new DamageInfo
            {
                amount = damage,
                type = DamageType.Melee,
                source = original != null ? original.gameObject : gameObject,
                sourceTeam = Team.Player,
                direction = transform.forward,
                knockback = 1f
            };

            int count = Physics.OverlapSphereNonAlloc(transform.position + Vector3.up + transform.forward, 1.2f, Buffer, Physics.AllLayers, QueryTriggerInteraction.Collide);
            HitResolver.ApplyToColliders(Buffer, count, info, Team.Player, null, false);
        }

        private void Face(Vector3 direction)
        {
            direction = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (direction.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private bool HasGroundAhead(Vector3 direction)
        {
            Vector3 probe = transform.position + direction * 0.9f + Vector3.up * 0.5f;
            return Physics.Raycast(probe, Vector3.down, 2f, AbilityMath.EnvironmentMask, QueryTriggerInteraction.Ignore);
        }

        private void StickToGround(float dt)
        {
            if (Physics.Raycast(transform.position + Vector3.up * 1.2f, Vector3.down, out RaycastHit hit, 1.5f + fallSpeed * dt, AbilityMath.EnvironmentMask, QueryTriggerInteraction.Ignore))
            {
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
                fallSpeed = 0f;
                return;
            }

            fallSpeed += Gravity * dt;
            transform.position += Vector3.down * fallSpeed * dt;
        }
    }
}
