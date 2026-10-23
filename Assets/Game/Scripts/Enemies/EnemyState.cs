namespace CodeLyokoFanGame
{
    // One state of the monster state machine (Idle, Patrol, Alert, Chase, Attack, Stagger, Dead).
    public abstract class EnemyState
    {
        protected EnemyState(EnemyBrain brain)
        {
            Brain = brain;
        }

        public abstract EnemyStateId Id { get; }

        protected EnemyBrain Brain { get; }
        protected EnemyAI Host => Brain.Host;
        protected EnemyBehaviour Behaviour => Brain.Behaviour;
        protected EnemyMotor Motor => Brain.Motor;

        public virtual void Enter()
        {
        }

        public virtual void Tick(float deltaTime)
        {
        }

        public virtual void Exit()
        {
        }
    }
}
