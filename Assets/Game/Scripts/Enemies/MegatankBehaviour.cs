using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // F6.12 Megatank: rolls towards the target, stops, opens for one second (warning + aim preview)
    // and fires a horizontal (jump) or vertical (step aside) beam. Only vulnerable while open:
    // closed, its armour (Hurtbox x0.001) shrugs off every hit. A heavy hit or a synergy can force it open.
    public sealed class MegatankBehaviour : EnemyBehaviour
    {
        private const float AimLockAt = 0.75f;
        // Closed armour: practically no damage, but the hit still registers so heavy blows can crack it.
        private const float ClosedArmourMultiplier = 0.001f;

        private Hurtbox armour;
        private Transform iris;
        private Renderer irisRenderer;
        private BeamAttack preview;
        private bool nextHorizontal = true;
        private float openAmount;

        public bool IsOpen => (Brain.IsAttacking && Brain.Phase != EnemyAttackPhase.None)
            || (Brain.IsStaggered && Brain.StaggerCracked) || Host.IsWeakPointForced;

        public override bool IsGuarded => !IsOpen;
        public override bool IsEyeExposed => IsOpen;

        protected override void OnBind()
        {
            armour = Host.GetComponentInChildren<Hurtbox>(true);
            iris = Host.FindPart("Iris");
            irisRenderer = iris != null ? iris.GetComponent<Renderer>() : null;
            ApplyArmour();
        }

        public override void Tick(float deltaTime)
        {
            ApplyArmour();
            openAmount = Mathf.MoveTowards(openAmount, IsOpen ? 1f : 0f, 3f * deltaTime);

            Transform body = Host.BodyVisual;
            if (body == null)
            {
                return;
            }

            // Rolling while closed and moving; squashed open with the iris showing.
            float speed = Motor.FlatSpeed;
            if (openAmount <= 0.01f && speed > 0.1f)
            {
                float radius = Mathf.Max(0.5f, body.localScale.y * 0.5f);
                body.Rotate(Vector3.right, speed / radius * Mathf.Rad2Deg * deltaTime, Space.Self);
            }
            else if (openAmount > 0.01f)
            {
                body.localRotation = Quaternion.RotateTowards(body.localRotation, Quaternion.identity, 360f * deltaTime);
            }

            if (irisRenderer != null)
            {
                irisRenderer.enabled = openAmount > 0.05f;
                iris.localScale = new Vector3(1.7f * openAmount, 0.05f, 1.7f * openAmount);
            }
        }

        public override EnemyAttackDefinition ChooseAttack(Health target, float distance)
        {
            EnemyAttackType wanted = nextHorizontal ? EnemyAttackType.BeamHorizontal : EnemyAttackType.BeamVertical;
            for (int i = 0; i < Attacks.Count; i++)
            {
                EnemyAttackDefinition attack = Attacks[i];
                if (attack.type == wanted && distance <= attack.range && IsReady(attack))
                {
                    nextHorizontal = !nextHorizontal;
                    return attack;
                }
            }

            return base.ChooseAttack(target, distance);
        }

        public override void BeginWindup(EnemyAttackDefinition attack, Health target)
        {
            base.BeginWindup(attack, target);
            if (Application.isPlaying && IsBeam(attack))
            {
                preview = BeamAttack.Preview(Host, attack);
            }
        }

        public override void TickWindup(EnemyAttackDefinition attack, Health target, float progress, float deltaTime)
        {
            Motor.Stop();
            if (target != null && progress < AimLockAt)
            {
                Motor.FaceTowards(target.transform.position, Definition.turnSpeed * 1.5f, deltaTime);
            }

            if (preview != null)
            {
                preview.Aim(Host, attack);
            }
        }

        public override void Fire(EnemyAttackDefinition attack, Health target, int shotIndex)
        {
            ClearPreview();
            base.Fire(attack, target, shotIndex);
            CameraRig.Shake(0.5f);
        }

        public override void EndAttack(EnemyAttackDefinition attack, bool aborted)
        {
            base.EndAttack(attack, aborted);
            ClearPreview();
        }

        public override void OnDeath()
        {
            ClearPreview();
        }

        private void ApplyArmour()
        {
            if (armour != null)
            {
                armour.Configure(Host.Health, IsOpen ? 1f : ClosedArmourMultiplier);
            }
        }

        private void ClearPreview()
        {
            if (preview != null)
            {
                Object.Destroy(preview.gameObject);
                preview = null;
            }
        }

        private static bool IsBeam(EnemyAttackDefinition attack)
        {
            return attack.type == EnemyAttackType.BeamHorizontal || attack.type == EnemyAttackType.BeamVertical;
        }
    }
}
