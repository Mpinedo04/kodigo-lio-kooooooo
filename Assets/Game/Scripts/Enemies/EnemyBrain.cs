using System;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Monster state machine (F06). EnemyAI hosts it; per-monster rules live in EnemyBehaviour.
    // Uses its own clock so tests can drive it frame by frame.
    public sealed class EnemyBrain
    {
        private readonly EnemyState[] states;
        private EnemyState current;
        private float stateStartTime;
        private float nextPerceptionTime;
        private float nextRetargetTime;
        private float lastSeenTime;
        private float nextAttackTime;

        public EnemyBrain(EnemyAI host, EnemyBehaviour behaviour, EnemyMotor motor, EnemyPerception perception)
        {
            Host = host;
            Behaviour = behaviour;
            Motor = motor;
            Perception = perception;

            states = new EnemyState[]
            {
                new EnemyIdleState(this),
                new EnemyPatrolState(this),
                new EnemyAlertState(this),
                new EnemyChaseState(this),
                new EnemyAttackState(this),
                new EnemyStaggerState(this),
                new EnemyDeadState(this)
            };

            nextPerceptionTime = UnityEngine.Random.Range(0f, 0.2f);
            current = states[(int)EnemyStateId.Idle];
            current.Enter();
        }

        public EnemyAI Host { get; }
        public EnemyBehaviour Behaviour { get; }
        public EnemyMotor Motor { get; }
        public EnemyPerception Perception { get; }

        public float Time { get; private set; }
        public float StateTime => Time - stateStartTime;
        public EnemyStateId State => current.Id;
        public Health Target { get; private set; }
        public Health LastAttacker { get; private set; }
        public EnemyAttackDefinition CurrentAttack { get; private set; }
        public EnemyAttackPhase Phase { get; set; }
        public float StaggerRemaining { get; set; }
        public bool StaggerCracked { get; private set; }

        public bool IsAttacking => State == EnemyStateId.Attack;
        public bool IsStaggered => State == EnemyStateId.Stagger;
        public bool IsDead => State == EnemyStateId.Dead;
        public bool CanStartAttack => Time >= nextAttackTime;
        public bool Paused => Host.IsSuspended || Host.IsMaterializing;

        public CombatDirector Director => CombatDirector.Instance;
        public CombatDirectorProfile Settings => Director != null ? Director.Profile : CombatDirector.DefaultProfile;

        public void Tick(float deltaTime)
        {
            if (IsDead || Paused)
            {
                return;
            }

            Time += deltaTime;
            if (Time >= nextPerceptionTime)
            {
                nextPerceptionTime = Time + Mathf.Max(0.02f, Settings.perceptionInterval);
                UpdatePerception();
            }

            current.Tick(deltaTime);
        }

        public void ChangeState(EnemyStateId id)
        {
            if (current.Id == id || IsDead)
            {
                return;
            }

            current.Exit();
            current = states[(int)id];
            stateStartTime = Time;
            current.Enter();
        }

        // ---- Events from the host -----------------------------------------------------------------

        public void Stagger(float seconds, bool cracked)
        {
            if (IsDead || seconds <= 0f)
            {
                return;
            }

            StaggerCracked |= cracked;
            if (IsStaggered)
            {
                StaggerRemaining = Mathf.Max(StaggerRemaining, seconds);
                return;
            }

            StaggerRemaining = seconds;
            ChangeState(EnemyStateId.Stagger);
        }

        public void ClearStagger()
        {
            StaggerRemaining = 0f;
            StaggerCracked = false;
        }

        public void Die()
        {
            if (IsDead)
            {
                return;
            }

            current.Exit();
            current = states[(int)EnemyStateId.Dead];
            stateStartTime = Time;
            current.Enter();
        }

        // A warning from an ally or a script. Ignored when the monster already has a target.
        public bool Alert(Health target)
        {
            if (IsDead || Target != null || !CombatDirector.IsValidTarget(target))
            {
                return false;
            }

            SetTarget(target);
            if (State == EnemyStateId.Idle || State == EnemyStateId.Patrol)
            {
                ChangeState(EnemyStateId.Alert);
            }

            return true;
        }

        public void NotifyDamaged(DamageInfo info)
        {
            if (IsDead || info.source == null)
            {
                return;
            }

            Health attacker = info.source.GetComponentInParent<Health>();
            if (!CombatDirector.IsValidTarget(attacker))
            {
                return;
            }

            LastAttacker = attacker;
            if (Target == null)
            {
                SetTarget(attacker);
                if (State == EnemyStateId.Idle || State == EnemyStateId.Patrol)
                {
                    ChangeState(EnemyStateId.Alert);
                }
            }
            else if (attacker == Target)
            {
                lastSeenTime = Time;
            }
        }

        // Telekinesis grabbed the monster mid-attack: drop the attack cleanly.
        public void Interrupt()
        {
            if (IsAttacking)
            {
                ChangeState(Target != null ? EnemyStateId.Chase : EnemyStateId.Idle);
            }
        }

        public void BeginAttack(EnemyAttackDefinition attack)
        {
            CurrentAttack = attack;
            Behaviour.MarkUsed(attack);
            ChangeState(EnemyStateId.Attack);
        }

        public void FinishAttack()
        {
            CurrentAttack = null;
            Phase = EnemyAttackPhase.None;
            nextAttackTime = Time + Host.Definition.attackGap;
        }

        public void Dispose()
        {
            if (current != null && !IsDead)
            {
                current.Exit();
            }

            CombatDirector director = CombatDirector.Current;
            if (director != null)
            {
                director.Forget(Host);
            }
        }

        // ---- Perception ---------------------------------------------------------------------------

        private void UpdatePerception()
        {
            EnemyDefinition definition = Host.Definition;
            CombatDirectorProfile settings = Settings;

            if (Target != null && !CombatDirector.IsValidTarget(Target))
            {
                SetTarget(null);
            }

            if (Target != null)
            {
                if (Perception.CanTrack(Target, definition.detectionRange * settings.keepTargetRangeMultiplier))
                {
                    lastSeenTime = Time;
                }
                else if (Time - lastSeenTime > definition.forgetTargetAfter)
                {
                    SetTarget(null);
                }
            }

            if (Target != null && Time < nextRetargetTime)
            {
                return;
            }

            nextRetargetTime = Time + settings.retargetInterval;
            // Losing a target is handled above; retargeting only switches to a better one.
            Health chosen = ChooseTarget(definition.detectionRange, settings);
            if (chosen == null || chosen == Target)
            {
                return;
            }

            bool fresh = Target == null;
            SetTarget(chosen);
            if (fresh && chosen != null && (State == EnemyStateId.Idle || State == EnemyStateId.Patrol))
            {
                ChangeState(EnemyStateId.Alert);
            }
        }

        private Health ChooseTarget(float range, CombatDirectorProfile settings)
        {
            Predicate<Health> canNotice = candidate => Perception.CanNotice(candidate, range, settings.closeSenseRadius);
            CombatDirector director = Director;
            if (director != null)
            {
                return director.ChooseTarget(Host, Target, range, canNotice);
            }

            return CombatDirector.SelectTarget(Health.All, Host.transform.position, Target, range, canNotice, settings, null, null);
        }

        private void SetTarget(Health target)
        {
            Target = target;
            lastSeenTime = Time;
            CombatDirector director = Director;
            if (director != null)
            {
                director.Engage(Host, target);
            }
        }
    }
}
