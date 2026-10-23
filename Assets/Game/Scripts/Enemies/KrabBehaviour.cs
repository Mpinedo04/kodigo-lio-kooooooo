using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // F6.10 Krabe: walks and kicks or fires its laser from the eye on top (exposed while firing).
    // Its legs take damage separately; break two and it collapses with the eye exposed.
    public sealed class KrabBehaviour : EnemyBehaviour
    {
        private const float DownedDrop = 1.1f;

        private readonly List<EnemyPart> legs = new List<EnemyPart>();
        private Vector3 bodyRestPosition;
        private bool hasRestPose;

        public bool IsKnockedDown { get; private set; }
        public int BrokenLegs
        {
            get
            {
                int broken = 0;
                for (int i = 0; i < legs.Count; i++)
                {
                    if (legs[i] == null || legs[i].IsBroken)
                    {
                        broken++;
                    }
                }

                return broken;
            }
        }

        public override bool IsEyeExposed => IsKnockedDown || (Brain.IsAttacking && HasAttackOfType(EnemyAttackType.Laser));

        protected override void OnBind()
        {
            legs.Clear();
            Host.GetComponentsInChildren(true, legs);
            for (int i = 0; i < legs.Count; i++)
            {
                legs[i].Configure("Leg_" + i, Definition.partHealth);
                legs[i].Health.Died -= OnLegBroken;
                legs[i].Health.Died += OnLegBroken;
                legs[i].Health.Damaged -= OnLegDamaged;
                legs[i].Health.Damaged += OnLegDamaged;
            }

            if (Host.BodyVisual != null)
            {
                bodyRestPosition = Host.BodyVisual.localPosition;
                hasRestPose = true;
            }
        }

        public override void OnDeath()
        {
            for (int i = 0; i < legs.Count; i++)
            {
                if (legs[i] != null)
                {
                    legs[i].Health.Died -= OnLegBroken;
                    legs[i].Health.Damaged -= OnLegDamaged;
                }
            }
        }

        public override Vector3 DesiredPosition(Health target)
        {
            float distance = Vector3.Distance(Host.transform.position, target.transform.position);
            if (distance <= 4f)
            {
                return Host.transform.position;
            }

            return base.DesiredPosition(target);
        }

        // Close: kick. Far: laser.
        public override EnemyAttackDefinition ChooseAttack(Health target, float distance)
        {
            for (int i = 0; i < Attacks.Count; i++)
            {
                EnemyAttackDefinition attack = Attacks[i];
                if (attack.type == EnemyAttackType.Melee && distance <= attack.range && IsReady(attack))
                {
                    return attack;
                }
            }

            return base.ChooseAttack(target, distance);
        }

        public override void TickWindup(EnemyAttackDefinition attack, Health target, float progress, float deltaTime)
        {
            base.TickWindup(attack, target, progress, deltaTime);
            if (attack.type == EnemyAttackType.Melee && Host.BodyVisual != null && hasRestPose)
            {
                Host.BodyVisual.localRotation = Quaternion.Euler(-12f * progress, 0f, 0f);
            }
        }

        public override void EndAttack(EnemyAttackDefinition attack, bool aborted)
        {
            base.EndAttack(attack, aborted);
            if (Host.BodyVisual != null && !IsKnockedDown)
            {
                Host.BodyVisual.localRotation = Quaternion.identity;
            }
        }

        public override void OnStaggerEnd()
        {
            if (!IsKnockedDown)
            {
                return;
            }

            IsKnockedDown = false;
            for (int i = 0; i < legs.Count; i++)
            {
                if (legs[i] != null)
                {
                    legs[i].Restore();
                }
            }
        }

        public override void Tick(float deltaTime)
        {
            if (Host.BodyVisual == null || !hasRestPose)
            {
                return;
            }

            Vector3 goal = IsKnockedDown ? bodyRestPosition + Vector3.down * DownedDrop : bodyRestPosition;
            Host.BodyVisual.localPosition = Vector3.MoveTowards(Host.BodyVisual.localPosition, goal, 4f * deltaTime);
            if (IsKnockedDown)
            {
                Host.BodyVisual.localRotation = Quaternion.RotateTowards(Host.BodyVisual.localRotation, Quaternion.Euler(8f, 0f, 10f), 90f * deltaTime);
            }
        }

        private void OnLegDamaged(Health leg, DamageInfo info)
        {
            Host.NotifyPartDamaged(info);
        }

        private void OnLegBroken(Health leg)
        {
            if (IsKnockedDown || Brain.IsDead || BrokenLegs < Mathf.Max(1, Definition.partsToKnockDown))
            {
                return;
            }

            IsKnockedDown = true;
            Brain.Stagger(Definition.knockedDownDuration, false);
        }
    }
}
