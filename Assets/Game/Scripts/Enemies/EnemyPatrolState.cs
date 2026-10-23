using UnityEngine;

namespace CodeLyokoFanGame
{
    // Wanders to a random point around home, then idles again.
    public sealed class EnemyPatrolState : EnemyState
    {
        private const float GiveUpAfter = 8f;

        private Vector3 point;

        public EnemyPatrolState(EnemyBrain brain) : base(brain)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Patrol;

        public override void Enter()
        {
            point = Behaviour.PickPatrolPoint();
        }

        public override void Tick(float deltaTime)
        {
            if (Brain.Target != null)
            {
                Brain.ChangeState(EnemyStateId.Alert);
                return;
            }

            Behaviour.TickPatrol(point, deltaTime);
            Vector3 flat = Vector3.ProjectOnPlane(point - Host.transform.position, Vector3.up);
            if (flat.magnitude < 0.8f || Brain.StateTime > GiveUpAfter)
            {
                Brain.ChangeState(EnemyStateId.Idle);
            }
        }
    }
}
