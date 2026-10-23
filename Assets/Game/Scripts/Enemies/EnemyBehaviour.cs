using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Per-monster rules plugged into the shared state machine: where to stand, which attack to use,
    // how to telegraph it and when the eye is exposed. Default = simple ground shooter.
    public abstract class EnemyBehaviour
    {
        private readonly Dictionary<EnemyAttackDefinition, float> readyAt = new Dictionary<EnemyAttackDefinition, float>();
        private readonly List<EnemyAttackDefinition> candidates = new List<EnemyAttackDefinition>();
        private readonly List<EnemyAttackDefinition> fallbackAttacks = new List<EnemyAttackDefinition>();

        protected EnemyAI Host { get; private set; }
        protected EnemyBrain Brain { get; private set; }
        protected EnemyMotor Motor { get; private set; }
        protected EnemyDefinition Definition => Host.Definition;

        public IReadOnlyList<EnemyAttackDefinition> Attacks =>
            Definition != null && Definition.attacks != null && Definition.attacks.Count > 0 ? Definition.attacks : fallbackAttacks;

        public virtual bool IsStationary => false;
        public virtual bool IsGuarded => false;
        public virtual bool IsEyeExposed => Definition.weakPointAlwaysExposed;

        public static EnemyBehaviour Create(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Kankrelat:
                    return new KankrelatBehaviour();
                case EnemyKind.Blok:
                    return new BlokBehaviour();
                case EnemyKind.Krab:
                    return new KrabBehaviour();
                case EnemyKind.Hornet:
                    return new HornetBehaviour();
                case EnemyKind.Megatank:
                    return new MegatankBehaviour();
                default:
                    return new GenericShooterBehaviour();
            }
        }

        public void Bind(EnemyAI host, EnemyBrain brain, EnemyMotor motor)
        {
            Host = host;
            Brain = brain;
            Motor = motor;
            fallbackAttacks.Clear();
            fallbackAttacks.Add(BuildFallbackAttack(host.Definition));
            OnBind();
        }

        protected virtual void OnBind()
        {
        }

        // Every frame while the brain runs (visuals: rolling, wings, guard pose).
        public virtual void Tick(float deltaTime)
        {
        }

        public virtual void OnSuspended(bool suspended)
        {
        }

        public virtual void OnDeath()
        {
        }

        // ---- Movement -----------------------------------------------------------------------------

        public virtual void TickIdle(float deltaTime)
        {
            Motor.Stop();
        }

        public virtual Vector3 PickPatrolPoint()
        {
            Vector2 circle = Random.insideUnitCircle * Definition.patrolRadius;
            return Motor.Home + new Vector3(circle.x, 0f, circle.y);
        }

        public virtual void TickPatrol(Vector3 point, float deltaTime)
        {
            Motor.MoveTowards(point, 0.5f, deltaTime);
            Motor.FaceMovement(Definition.turnSpeed * 0.5f, deltaTime);
        }

        public virtual void TickAlert(Health target, float deltaTime)
        {
            Motor.FaceTowards(target.transform.position, Definition.turnSpeed, deltaTime);
        }

        public virtual void TickChase(Health target, float deltaTime)
        {
            Motor.MoveTowards(DesiredPosition(target), 1f, deltaTime);
            Motor.FaceTowards(target.transform.position, Definition.turnSpeed, deltaTime);
        }

        // Default: keep the preferred distance on the line between the target and the monster.
        public virtual Vector3 DesiredPosition(Health target)
        {
            Vector3 away = Vector3.ProjectOnPlane(Host.transform.position - target.transform.position, Vector3.up);
            if (away.sqrMagnitude < 0.01f)
            {
                away = -target.transform.forward;
            }

            return target.transform.position + away.normalized * Definition.preferredDistance;
        }

        // ---- Attacks ------------------------------------------------------------------------------

        public virtual bool WantsToAttack(Health target)
        {
            return true;
        }

        public virtual EnemyAttackDefinition ChooseAttack(Health target, float distance)
        {
            candidates.Clear();
            float totalWeight = 0f;
            IReadOnlyList<EnemyAttackDefinition> attacks = Attacks;
            for (int i = 0; i < attacks.Count; i++)
            {
                EnemyAttackDefinition attack = attacks[i];
                if (attack == null || distance > attack.range || !IsReady(attack))
                {
                    continue;
                }

                candidates.Add(attack);
                totalWeight += Mathf.Max(0.01f, attack.weight);
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            float roll = Random.value * totalWeight;
            for (int i = 0; i < candidates.Count; i++)
            {
                roll -= Mathf.Max(0.01f, candidates[i].weight);
                if (roll <= 0f)
                {
                    return candidates[i];
                }
            }

            return candidates[candidates.Count - 1];
        }

        public bool IsReady(EnemyAttackDefinition attack)
        {
            return !readyAt.TryGetValue(attack, out float time) || Brain.Time >= time;
        }

        public void MarkUsed(EnemyAttackDefinition attack)
        {
            readyAt[attack] = Brain.Time + attack.windup + Mathf.Max(0f, attack.cooldown);
        }

        public virtual void BeginWindup(EnemyAttackDefinition attack, Health target)
        {
            Host.Telegraph.Begin(attack.windup, attack.heavy || attack.windup >= 0.8f);
        }

        public virtual void TickWindup(EnemyAttackDefinition attack, Health target, float progress, float deltaTime)
        {
            Motor.Stop();
            if (target != null && progress < 0.85f)
            {
                Motor.FaceTowards(target.transform.position, Definition.turnSpeed * 1.5f, deltaTime);
            }
        }

        public virtual void Fire(EnemyAttackDefinition attack, Health target, int shotIndex)
        {
            if (shotIndex == 0)
            {
                Host.Telegraph.End();
            }

            EnemyAttacks.Perform(Host, attack, target);
        }

        public virtual void TickActive(EnemyAttackDefinition attack, Health target, float deltaTime)
        {
            Motor.Stop();
            if (target != null && attack.type == EnemyAttackType.Burst)
            {
                Motor.FaceTowards(target.transform.position, Definition.turnSpeed, deltaTime);
            }
        }

        public virtual void BeginRecovery(EnemyAttackDefinition attack, Health target)
        {
        }

        public virtual void TickRecovery(EnemyAttackDefinition attack, Health target, float deltaTime)
        {
            Motor.Stop();
        }

        public virtual void EndAttack(EnemyAttackDefinition attack, bool aborted)
        {
            Host.Telegraph.End();
        }

        // ---- Reactions ----------------------------------------------------------------------------

        public virtual void OnStaggerStart()
        {
            Host.Telegraph.End();
        }

        public virtual void TickStagger(float deltaTime)
        {
        }

        public virtual void OnStaggerEnd()
        {
        }

        protected bool HasAttackOfType(EnemyAttackType type)
        {
            EnemyAttackDefinition attack = Brain != null ? Brain.CurrentAttack : null;
            return attack != null && attack.type == type;
        }

        private static EnemyAttackDefinition BuildFallbackAttack(EnemyDefinition definition)
        {
            bool melee = definition.contactDamage > 0f;
            EnemyAttackDefinition attack = EnemyDefaults.Attack(
                "Basico",
                melee ? EnemyAttackType.Melee : EnemyAttackType.Laser,
                melee ? definition.contactDamage : definition.projectileDamage,
                Mathf.Max(0.3f, definition.fireCooldown),
                melee ? 3f : definition.attackRange,
                definition.boss ? 0.9f : 0.4f,
                0.3f);
            attack.areaSize = 2.6f;
            attack.knockback = melee ? 5f : 0f;
            attack.heavy = definition.boss;
            return attack;
        }
    }
}
