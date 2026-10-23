using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Ulrich's Supersprint (F5.10): an ultra-fast run that goes through shots and hits whatever it rams.
    // With his clones out it becomes Triangular (F5.12): the three surround the target and the real one lands a critical hit.
    public sealed class SupersprintAbility : Ability
    {
        private const string TriangulateId = "ulrich_triangulate";
        private const float TriangleRadius = 2.2f;
        private const float StrikeDistance = 1.8f;

        private static readonly Collider[] Buffer = new Collider[24];

        private readonly HashSet<Health> rammed = new HashSet<Health>();
        private Health triangleTarget;
        private float criticalMultiplier;

        public SupersprintAbility(AbilityDefinition definition) : base(definition)
        {
        }

        // Pressing again does not cut it short.
        public override void OnPressedAgain()
        {
        }

        protected override void Activate()
        {
            rammed.Clear();
            triangleTarget = null;
            Vector3 direction = Controller.FlatAimDirection();
            float duration = Definition.duration;

            if (TryStartTriangle(out Health target))
            {
                Vector3 toTarget = Vector3.ProjectOnPlane(target.transform.position - Transform.position, Vector3.up);
                direction = toTarget.normalized;
                duration = Mathf.Max(duration, toTarget.magnitude / Mathf.Max(1f, Definition.speed) + 0.2f);
            }

            Owner.ForceMove(direction * Definition.speed, duration);
            if (Controller.Health != null)
            {
                Controller.Health.GrantInvulnerability(duration + 0.1f);
            }
        }

        private bool TryStartTriangle(out Health target)
        {
            target = null;
            TriplicateAbility clones = Controller.Find<TriplicateAbility>();
            if (clones == null || clones.CloneCount == 0 || !Controller.HasUnlocked(TriangulateId))
            {
                return false;
            }

            Transform aimed = Controller.CurrentTarget;
            target = aimed != null ? aimed.GetComponentInParent<Health>() : AbilityMath.NearestEnemy(Transform.position, Mathf.Max(Definition.range, 6f));
            if (!AbilityMath.IsEnemy(target))
            {
                target = null;
                return false;
            }

            AbilityDefinition triangulate = Controller.FindDefinition(TriangulateId);
            criticalMultiplier = triangulate != null && triangulate.power > 0f ? triangulate.power : 3f;
            Vector3[] corners = AbilityMath.TriangleAround(target.transform.position, Transform.position, TriangleRadius);
            clones.Surround(corners[1], corners[2], target.transform);
            triangleTarget = target;
            rammed.Add(target);
            Controller.Notify("Triangular!");
            return true;
        }

        public override void Tick(float dt)
        {
            if (triangleTarget != null)
            {
                if (!AbilityMath.IsEnemy(triangleTarget))
                {
                    triangleTarget = null;
                }
                else if (Vector3.ProjectOnPlane(triangleTarget.transform.position - Transform.position, Vector3.up).magnitude <= StrikeDistance)
                {
                    CriticalStrike();
                    Owner.StopForcedMove();
                    End();
                    return;
                }
            }

            DamageInfo info = new DamageInfo
            {
                amount = Definition.damage,
                type = DamageType.Melee,
                source = Owner.gameObject,
                sourceTeam = Team.Player,
                direction = Transform.forward,
                knockback = 6f
            };

            int count = Physics.OverlapSphereNonAlloc(Transform.position + Vector3.up, Mathf.Max(0.5f, Definition.radius), Buffer, Physics.AllLayers, QueryTriggerInteraction.Collide);
            if (HitResolver.ApplyToColliders(Buffer, count, info, Team.Player, rammed, false) > 0)
            {
                HitStop.Trigger(0.03f);
            }

            if (!Owner.IsForcedMoving)
            {
                End();
            }
        }

        private void CriticalStrike()
        {
            float baseDamage = Controller.Combat != null ? Controller.Combat.BasicDamage : Definition.damage;
            DamageInfo critical = new DamageInfo
            {
                amount = baseDamage * criticalMultiplier,
                type = DamageType.Melee,
                source = Owner.gameObject,
                sourceTeam = Team.Player,
                direction = Transform.forward,
                point = triangleTarget.transform.position + Vector3.up,
                isCritical = true,
                isHeavy = true,
                knockback = 8f
            };

            triangleTarget.TakeDamage(critical);
            HitStop.Trigger(0.1f);
            CameraRig.Shake(0.5f);
        }

        protected override void OnEnd()
        {
            rammed.Clear();
            triangleTarget = null;
        }
    }
}
