using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // F6.9 Blok: a turret. Between attacks it turns so its eye faces away from the target (flank it!);
    // to attack it turns the eye towards the target: laser, freezing laser, rings of fire or prison ray.
    // A heavy hit cracks the guard: dazed, it stares at its attacker with the eye exposed.
    public sealed class BlokBehaviour : EnemyBehaviour
    {
        public override bool IsStationary => true;
        public override bool IsGuarded => !Brain.IsAttacking && !Brain.IsStaggered;

        public override void TickIdle(float deltaTime)
        {
            Host.transform.Rotate(0f, Definition.turnSpeed * 0.25f * deltaTime, 0f);
        }

        public override void TickAlert(Health target, float deltaTime)
        {
            HideEye(target, deltaTime);
        }

        public override void TickChase(Health target, float deltaTime)
        {
            Motor.Stop();
            HideEye(target, deltaTime);
        }

        // Rings when the warrior gets close; otherwise any ready shot.
        public override EnemyAttackDefinition ChooseAttack(Health target, float distance)
        {
            for (int i = 0; i < Attacks.Count; i++)
            {
                EnemyAttackDefinition attack = Attacks[i];
                if (attack.type == EnemyAttackType.FireRing && distance <= attack.range && IsReady(attack))
                {
                    return attack;
                }
            }

            return base.ChooseAttack(target, distance);
        }

        public override void TickWindup(EnemyAttackDefinition attack, Health target, float progress, float deltaTime)
        {
            if (target != null && attack.type != EnemyAttackType.FireRing)
            {
                Motor.FaceTowards(target.transform.position, Definition.turnSpeed * 3f, deltaTime);
            }

            // Rings: the cube shudders before it erupts.
            Transform body = Host.BodyVisual;
            if (body != null && attack.type == EnemyAttackType.FireRing)
            {
                body.localScale = Vector3.one * 1.5f * (1f + 0.08f * Mathf.Sin(progress * 60f));
            }
        }

        public override void EndAttack(EnemyAttackDefinition attack, bool aborted)
        {
            base.EndAttack(attack, aborted);
            if (Host.BodyVisual != null)
            {
                Host.BodyVisual.localScale = Vector3.one * 1.5f;
            }
        }

        public override void TickStagger(float deltaTime)
        {
            Health attacker = Brain.LastAttacker != null ? Brain.LastAttacker : Brain.Target;
            if (Brain.StaggerCracked && attacker != null)
            {
                Motor.FaceTowards(attacker.transform.position, Definition.turnSpeed * 2f, deltaTime);
            }
        }

        private void HideEye(Health target, float deltaTime)
        {
            Vector3 away = Host.transform.position - target.transform.position;
            Motor.FaceDirection(away, Definition.turnSpeed, deltaTime);
        }
    }
}
