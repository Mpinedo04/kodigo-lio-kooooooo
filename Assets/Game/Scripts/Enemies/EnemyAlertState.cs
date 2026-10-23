namespace CodeLyokoFanGame
{
    // Just spotted a target: the eye flashes, nearby allies are warned, then the chase starts.
    public sealed class EnemyAlertState : EnemyState
    {
        public EnemyAlertState(EnemyBrain brain) : base(brain)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Alert;

        public override void Enter()
        {
            Motor.Stop();
            Host.Telegraph.Begin(Host.Definition.reactionTime, false);
            EnemyPerception.AlertAllies(Host, Brain.Target, Host.Definition.alertAllyRadius);
        }

        public override void Tick(float deltaTime)
        {
            if (Brain.Target == null)
            {
                Brain.ChangeState(EnemyStateId.Idle);
                return;
            }

            Behaviour.TickAlert(Brain.Target, deltaTime);
            if (Brain.StateTime >= Host.Definition.reactionTime)
            {
                Brain.ChangeState(EnemyStateId.Chase);
            }
        }

        public override void Exit()
        {
            Host.Telegraph.End();
        }
    }
}
