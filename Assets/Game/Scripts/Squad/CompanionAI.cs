using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // AI for the warriors you are not controlling (F5.27-F5.28): follow the controlled warrior, fight the nearest
    // monster, use abilities now and then, and never walk off a ledge into the Digital Sea.
    // Enabled by PlayerController.SetControlActive(false).
    [RequireComponent(typeof(PlayerController))]
    public sealed class CompanionAI : MonoBehaviour
    {
        private const float Gravity = 26f;
        private const float FarTooLong = 3f;

        private static readonly Vector3[] FollowSlots =
        {
            new Vector3(-1.8f, 0f, -2.2f),
            new Vector3(1.8f, 0f, -2.2f),
            new Vector3(0f, 0f, -3.6f)
        };

        private PlayerController owner;
        private PlayerCombat combat;
        private AbilityController abilities;
        private CharacterController controller;
        private Health health;
        private SquadProfile squad;
        private Health target;
        private float verticalVelocity;
        private float attackTimer;
        private float abilityTimer;
        private float farTimer;
        private float lastHurtTime = float.NegativeInfinity;

        public Health Target => target;

        private void Awake()
        {
            owner = GetComponent<PlayerController>();
            combat = GetComponent<PlayerCombat>();
            abilities = GetComponent<AbilityController>();
            controller = GetComponent<CharacterController>();
            health = GetComponent<Health>();
            squad = abilities != null && abilities.Squad != null ? abilities.Squad : ScriptableObject.CreateInstance<SquadProfile>();
            if (health != null)
            {
                health.Damaged += OnDamaged;
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Damaged -= OnDamaged;
            }
        }

        private void OnEnable()
        {
            verticalVelocity = 0f;
            farTimer = 0f;
            abilityTimer = Random.Range(1f, 2f);
        }

        private void OnDisable()
        {
            target = null;
            if (combat != null)
            {
                combat.SetAimOverride(null);
            }

            if (abilities != null)
            {
                abilities.AiTarget = null;
            }
        }

        private void OnDamaged(Health damaged, DamageInfo info)
        {
            lastHurtTime = Time.time;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (owner == null || owner.IsControlled || dt <= 0f || (health != null && health.IsDead) || controller == null || !controller.enabled)
            {
                return;
            }

            PlayerController leader = Leader();
            target = PickTarget(leader);
            Transform aim = target != null ? AimOf(target) : null;
            combat.SetAimOverride(aim);
            if (abilities != null)
            {
                abilities.AiTarget = aim;
            }

            // Supersprint and other abilities drive the body for a moment.
            if (owner.IsForcedMoving)
            {
                return;
            }

            Vector3 move = Vector3.zero;
            Vector3 face = Vector3.zero;
            bool hurry = false;
            if (!owner.IsStunned)
            {
                if (target != null)
                {
                    Engage(dt, ref move, ref face);
                }
                else if (leader != null)
                {
                    move = Follow(leader, out hurry);
                    face = move;
                }

                ThinkAbilities(dt);
            }

            Apply(move, face, hurry, dt);
            if (leader != null)
            {
                KeepUp(leader, dt);
            }
        }

        private void Engage(float dt, ref Vector3 move, ref Vector3 face)
        {
            Vector3 toTarget = Vector3.ProjectOnPlane(target.transform.position - transform.position, Vector3.up);
            float distance = toTarget.magnitude;
            Vector3 direction = distance > 0.01f ? toTarget / distance : transform.forward;
            float range = PreferredRange();

            if (distance > range)
            {
                move = direction;
            }
            else if (combat.Style != AttackStyle.Melee && distance < range * 0.5f)
            {
                move = -direction * 0.6f;
            }

            face = direction;
            attackTimer -= dt;
            if (distance <= range + 1f && attackTimer <= 0f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
                combat.BasicAttack();
                attackTimer = combat.Style == AttackStyle.Melee ? squad.meleeAttackInterval : squad.rangedAttackInterval;
            }
        }

        private Vector3 Follow(PlayerController leader, out bool hurry)
        {
            Vector3 goal = leader.transform.position + leader.transform.rotation * FollowSlots[Slot(leader) % FollowSlots.Length];
            Vector3 toGoal = Vector3.ProjectOnPlane(goal - transform.position, Vector3.up);
            float distance = toGoal.magnitude;
            hurry = distance > squad.followDistance * 2f;
            if (distance < 0.6f)
            {
                return Vector3.zero;
            }

            return toGoal / distance * Mathf.Clamp01(distance / 2f);
        }

        private void Apply(Vector3 move, Vector3 face, bool hurry, float dt)
        {
            CharacterDefinition definition = owner.Definition;
            float speed = definition != null ? (hurry ? definition.sprintSpeed : definition.walkSpeed) : 7f;
            if (TryGetComponent(out StatusEffects status))
            {
                speed = status.IsRooted ? 0f : speed * status.SpeedMultiplier;
            }

            Vector3 horizontal = move * speed;

            // Never walk into the Digital Sea: no ground ahead, no step.
            if (horizontal.sqrMagnitude > 0.01f && !HasGroundAhead(horizontal.normalized))
            {
                horizontal = Vector3.zero;
            }

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity = Mathf.Max(verticalVelocity - Gravity * dt, -40f);
            }

            controller.Move((horizontal + Vector3.up * verticalVelocity) * dt);

            if (face.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(face), 720f * dt);
            }
        }

        // Too far or stuck for a while: reappear next to the controlled warrior.
        private void KeepUp(PlayerController leader, float dt)
        {
            float distance = Vector3.Distance(leader.transform.position, transform.position);
            farTimer = target == null && distance > squad.followDistance * 4f ? farTimer + dt : 0f;
            if (distance < squad.teleportDistance && farTimer < FarTooLong)
            {
                return;
            }

            Vector3 spot = leader.LastGroundedPosition + leader.transform.rotation * FollowSlots[Slot(leader) % FollowSlots.Length];
            if (!AbilityMath.TryFindGround(spot, 3f, out Vector3 ground))
            {
                ground = leader.LastGroundedPosition;
            }

            controller.enabled = false;
            transform.position = ground + Vector3.up * 0.1f;
            controller.enabled = true;
            verticalVelocity = 0f;
            farTimer = 0f;
        }

        private void ThinkAbilities(float dt)
        {
            if (abilities == null)
            {
                return;
            }

            abilityTimer -= dt;
            if (abilityTimer > 0f)
            {
                return;
            }

            abilityTimer = Random.Range(squad.abilityThinkMin, squad.abilityThinkMax);
            int near = AbilityMath.CountEnemies(transform.position, 10f);
            bool meterFull = combat.Meter != null && combat.Meter.IsFull;
            float targetDistance = target != null ? Vector3.Distance(target.transform.position, transform.position) : float.MaxValue;

            switch (owner.Character)
            {
                case LyokoCharacter.Odd:
                    if (Time.time - lastHurtTime < 2f)
                    {
                        abilities.TryUse(AbilitySlot.Ability1);
                    }

                    if (meterFull && near >= 3)
                    {
                        abilities.TryUse(AbilitySlot.Ultimate);
                    }

                    break;
                case LyokoCharacter.Ulrich:
                    if (near >= 2)
                    {
                        abilities.TryUse(AbilitySlot.Ability2);
                    }
                    else if (targetDistance >= 4f && targetDistance <= 12f)
                    {
                        abilities.TryUse(AbilitySlot.Ability1);
                    }

                    if (meterFull && AbilityMath.CountEnemies(transform.position, 6f) >= 3)
                    {
                        abilities.TryUse(AbilitySlot.Ultimate);
                    }

                    break;
                case LyokoCharacter.Yumi:
                    EnemyAI enemy = target != null ? target.GetComponent<EnemyAI>() : null;
                    if (enemy != null && enemy.CanBeLifted)
                    {
                        abilities.TryUse(AbilitySlot.Ability1);
                    }
                    else if (health != null && health.Current < health.Max * 0.5f && near >= 2)
                    {
                        abilities.TryUse(AbilitySlot.Ability2);
                    }

                    if (meterFull && near >= 3)
                    {
                        abilities.TryUse(AbilitySlot.Ultimate);
                    }

                    break;
            }
        }

        private float PreferredRange()
        {
            switch (combat.Style)
            {
                case AttackStyle.Melee:
                    return 1.7f;
                case AttackStyle.Telekinetic:
                    return 7f;
                default:
                    return 9f;
            }
        }

        private Health PickTarget(PlayerController leader)
        {
            // Keep the current one while it is still worth fighting.
            if (IsValidTarget(target, leader, squad.engageRange * 1.3f))
            {
                return target;
            }

            Health best = null;
            float bestDistance = float.MaxValue;
            IReadOnlyList<Health> all = Health.All;
            for (int i = 0; i < all.Count; i++)
            {
                if (!IsValidTarget(all[i], leader, squad.engageRange))
                {
                    continue;
                }

                float distance = (all[i].transform.position - transform.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = all[i];
                }
            }

            return best;
        }

        private bool IsValidTarget(Health candidate, PlayerController leader, float range)
        {
            if (!AbilityMath.IsEnemy(candidate) || !AreClose(candidate.transform.position, transform.position, range))
            {
                return false;
            }

            return leader == null || AreClose(candidate.transform.position, leader.transform.position, squad.leashRange);
        }

        private static bool AreClose(Vector3 a, Vector3 b, float distance)
        {
            return (a - b).sqrMagnitude <= distance * distance;
        }

        private static Transform AimOf(Health enemyHealth)
        {
            EnemyAI enemy = enemyHealth.GetComponent<EnemyAI>();
            return enemy != null && enemy.AimPoint != null ? enemy.AimPoint : enemyHealth.transform;
        }

        private PlayerController Leader()
        {
            IReadOnlyList<PlayerController> all = PlayerController.All;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].IsControlled)
                {
                    return all[i];
                }
            }

            return null;
        }

        private int Slot(PlayerController leader)
        {
            int slot = 0;
            IReadOnlyList<PlayerController> all = PlayerController.All;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i] == owner)
                {
                    return slot;
                }

                if (all[i] != leader)
                {
                    slot++;
                }
            }

            return slot;
        }

        private bool HasGroundAhead(Vector3 direction)
        {
            Vector3 probe = transform.position + direction * (controller.radius + 0.6f) + Vector3.up * 0.5f;
            return Physics.Raycast(probe, Vector3.down, 2f, AbilityMath.EnvironmentMask, QueryTriggerInteraction.Ignore);
        }
    }
}
