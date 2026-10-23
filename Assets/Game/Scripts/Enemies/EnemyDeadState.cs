namespace CodeLyokoFanGame
{
    // Devirtualized. Final state: the host spawns the fragments and the Health removes the object.
    public sealed class EnemyDeadState : EnemyState
    {
        public EnemyDeadState(EnemyBrain brain) : base(brain)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Dead;

        public override void Enter()
        {
            Motor.Stop();
            Motor.Disable();
            Host.Telegraph.End();
            Behaviour.OnDeath();

            CombatDirector director = Brain.Director;
            if (director != null)
            {
                director.Forget(Host);
            }
        }
    }
}
