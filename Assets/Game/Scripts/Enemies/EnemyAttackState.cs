using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Wind-up (telegraph) -> active (shots, burst, beam) -> recovery. Interrupted by stagger,
    // telekinesis or losing the target; the attack slot is always given back on exit.
    public sealed class EnemyAttackState : EnemyState
    {
        private EnemyAttackDefinition attack;
        private float timer;
        private int shots;
        private bool completed;

        public EnemyAttackState(EnemyBrain brain) : base(brain)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Attack;

        public override void Enter()
        {
            attack = Brain.CurrentAttack;
            timer = 0f;
            shots = 0;
            completed = false;
            Brain.Phase = EnemyAttackPhase.Windup;
            Motor.Stop();

            if (attack != null)
            {
                Behaviour.BeginWindup(attack, Brain.Target);
            }
        }

        public override void Tick(float deltaTime)
        {
            Health target = Brain.Target;
            if (attack == null || (target == null && Brain.Phase == EnemyAttackPhase.Windup))
            {
                Leave(target);
                return;
            }

            timer += deltaTime;
            switch (Brain.Phase)
            {
                case EnemyAttackPhase.Windup:
                    Behaviour.TickWindup(attack, target, Mathf.Clamp01(timer / Mathf.Max(0.01f, attack.windup)), deltaTime);
                    if (timer >= attack.windup)
                    {
                        Brain.Phase = EnemyAttackPhase.Active;
                        timer = 0f;
                        Fire(target);
                    }

                    break;
                case EnemyAttackPhase.Active:
                    Behaviour.TickActive(attack, target, deltaTime);
                    int burst = Mathf.Max(1, attack.burstCount);
                    if (shots < burst && timer >= shots * attack.burstInterval)
                    {
                        Fire(target);
                    }

                    if (shots >= burst && timer >= ActiveLength(attack))
                    {
                        Brain.Phase = EnemyAttackPhase.Recovery;
                        timer = 0f;
                        Behaviour.BeginRecovery(attack, target);
                    }

                    break;
                default:
                    Behaviour.TickRecovery(attack, target, deltaTime);
                    if (timer >= attack.recovery)
                    {
                        completed = true;
                        Leave(target);
                    }

                    break;
            }
        }

        public override void Exit()
        {
            if (attack != null)
            {
                Behaviour.EndAttack(attack, !completed);
            }

            CombatDirector director = Brain.Director;
            if (director != null)
            {
                director.EndAttack(Host);
            }

            Brain.FinishAttack();
            attack = null;
        }

        public static float ActiveLength(EnemyAttackDefinition attack)
        {
            float burst = (Mathf.Max(1, attack.burstCount) - 1) * attack.burstInterval;
            bool beam = attack.type == EnemyAttackType.BeamHorizontal || attack.type == EnemyAttackType.BeamVertical;
            return Mathf.Max(burst, beam ? attack.activeTime : 0f);
        }

        private void Fire(Health target)
        {
            if (target != null)
            {
                Behaviour.Fire(attack, target, shots);
            }

            shots++;
        }

        private void Leave(Health target)
        {
            Brain.ChangeState(target != null ? EnemyStateId.Chase : EnemyStateId.Idle);
        }
    }
}
