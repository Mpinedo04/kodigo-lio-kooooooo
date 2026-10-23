using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Monsters of later seasons until their own phase: walk (or hover) into range and shoot,
    // or close in for contact attacks. Hidden eyes open while attacking. Exploders blow up.
    public sealed class GenericShooterBehaviour : EnemyBehaviour
    {
        public override bool IsEyeExposed => Definition.weakPointAlwaysExposed || Brain.IsAttacking;

        private bool IsMelee => Definition.contactDamage > 0f;

        public override Vector3 DesiredPosition(Health target)
        {
            float distance = IsMelee ? 1.6f : Definition.attackRange * 0.65f;
            Vector3 away = Vector3.ProjectOnPlane(Host.transform.position - target.transform.position, Vector3.up);
            if (away.sqrMagnitude < 0.01f)
            {
                away = -target.transform.forward;
            }

            return target.transform.position + away.normalized * distance;
        }

        public override void Fire(EnemyAttackDefinition attack, Health target, int shotIndex)
        {
            base.Fire(attack, target, shotIndex);
            if (Definition.behaviorStyle == EnemyBehaviorStyle.Exploder && Host.Health != null)
            {
                Host.Health.TakeDamage(new DamageInfo
                {
                    amount = Host.Health.Current,
                    type = DamageType.Ability,
                    source = Host.gameObject,
                    sourceTeam = Team.Neutral,
                    direction = Vector3.up,
                    ignoreInvulnerability = true
                });
            }
        }
    }
}
