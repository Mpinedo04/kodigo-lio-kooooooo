namespace CodeLyokoFanGame
{
    // Stunned: no movement, no attacks. A "cracked" stagger is a broken guard (eye exposed).
    public sealed class EnemyStaggerState : EnemyState
    {
        public EnemyStaggerState(EnemyBrain brain) : base(brain)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Stagger;

        public override void Enter()
        {
            Motor.Stop();
            Behaviour.OnStaggerStart();
        }

        public override void Tick(float deltaTime)
        {
            Brain.StaggerRemaining -= deltaTime;
            Behaviour.TickStagger(deltaTime);
            if (Brain.StaggerRemaining <= 0f)
            {
                Brain.ChangeState(Brain.Target != null ? EnemyStateId.Chase : EnemyStateId.Idle);
            }
        }

        public override void Exit()
        {
            Behaviour.OnStaggerEnd();
            Brain.ClearStagger();
        }
    }
}
