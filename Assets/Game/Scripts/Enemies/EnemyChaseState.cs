using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Gets into position (each monster has its own idea of where) and starts an attack when the
    // cooldowns allow it and the CombatDirector hands out an attack slot.
    public sealed class EnemyChaseState : EnemyState
    {
        public EnemyChaseState(EnemyBrain brain) : base(brain)
        {
        }

        public override EnemyStateId Id => EnemyStateId.Chase;

        public override void Tick(float deltaTime)
        {
            Health target = Brain.Target;
            if (target == null)
            {
                Brain.ChangeState(EnemyStateId.Idle);
                return;
            }

            Behaviour.TickChase(target, deltaTime);
            TryStartAttack(target);
        }

        private void TryStartAttack(Health target)
        {
            if (!Host.IsArmed || !Brain.CanStartAttack || !Behaviour.WantsToAttack(target))
            {
                return;
            }

            float distance = Vector3.Distance(Host.transform.position, target.transform.position);
            EnemyAttackDefinition attack = Behaviour.ChooseAttack(target, distance);
            if (attack == null)
            {
                return;
            }

            if (attack.type != EnemyAttackType.FireRing && !Brain.Perception.CanShoot(target))
            {
                return;
            }

            CombatDirector director = Brain.Director;
            if (director != null && !director.TryBeginAttack(Host, target))
            {
                return;
            }

            Brain.BeginAttack(attack);
        }
    }
}
