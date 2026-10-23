using UnityEngine;

namespace CodeLyokoFanGame
{
    // Waits in place; goes on patrol after a while (unless it is a turret).
    public sealed class EnemyIdleState : EnemyState
    {
        private float duration;

        public EnemyIdleState(EnemyBrain brain) : base(brain)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Idle;

        public override void Enter()
        {
            Motor.Stop();
            duration = Host.Definition.idleTime * Random.Range(0.7f, 1.3f);
        }

        public override void Tick(float deltaTime)
        {
            if (Brain.Target != null)
            {
                Brain.ChangeState(EnemyStateId.Alert);
                return;
            }

            Behaviour.TickIdle(deltaTime);
            if (Brain.StateTime >= duration && !Behaviour.IsStationary && Host.Definition.patrolRadius > 0.1f)
            {
                Brain.ChangeState(EnemyStateId.Patrol);
            }
        }
    }
}
