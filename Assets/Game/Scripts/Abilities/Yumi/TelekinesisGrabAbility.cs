using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Yumi's telekinesis grab (F5.18): lifts a rock or a small monster in front of her. Attack throws it where the
    // camera aims; Ability 1 again sets it down gently. Holding drains Concentration; running dry stuns her.
    public sealed class TelekinesisGrabAbility : Ability
    {
        private const float AimCone = 35f;
        private const float AiThrowDelay = 0.8f;

        private TelekinesisTarget heldObject;
        private EnemyAI heldEnemy;
        private GameObject aura;
        private float heldSince;
        private bool startedByAi;

        public TelekinesisGrabAbility(AbilityDefinition definition) : base(definition)
        {
        }

        public bool IsHolding => heldObject != null || heldEnemy != null;
        public override bool BlocksAttacks => IsHolding;
        public override string Status => IsHolding ? "Telequinesis: clic = lanzar, Q = soltar" : null;
        public override string UnavailableReason => "Telequinesis: nada que levantar delante";

        private GameObject Held => heldObject != null ? heldObject.gameObject : heldEnemy != null ? heldEnemy.gameObject : null;

        public override bool CanUse()
        {
            return base.CanUse() && FindTarget(out _, out _);
        }

        protected override void Activate()
        {
            FindTarget(out heldObject, out heldEnemy);
            startedByAi = !Owner.IsControlled;
            heldSince = Time.time;

            if (heldObject != null)
            {
                heldObject.BeginLift();
            }
            else if (heldEnemy != null)
            {
                heldEnemy.SetSuspended(true);
            }

            TelekinesisState.Mark(Held, true);
            ShowAura(Held);
        }

        // F09: a violet ring of energy turns around whatever Yumi holds.
        private void ShowAura(GameObject held)
        {
            if (held == null)
            {
                return;
            }

            aura = new GameObject("TelekinesisAura");
            aura.transform.SetParent(held.transform, false);
            aura.transform.localPosition = Vector3.up * 0.6f;
            aura.AddComponent<MeshFilter>().sharedMesh = LowPolyMesh.Ring(0.9f, 0.05f, 24, 4);
            MeshRenderer renderer = aura.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = LyokoMaterials.Glow(new Color(0.75f, 0.45f, 1f, 0.8f));
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            aura.AddComponent<Spin>().Configure(new Vector3(0.3f, 1f, 0.2f), 220f, 0f, 1f);
        }

        public override void Tick(float dt)
        {
            GameObject held = Held;
            if (held == null || !held.activeInHierarchy || (heldEnemy != null && IsDead(heldEnemy)))
            {
                heldObject = null;
                heldEnemy = null;
                End();
                return;
            }

            if (Controller.DrainConcentration(Definition.costPerSecond * dt))
            {
                Release(Vector3.zero);
                End();
                return;
            }

            Vector3 hold = HoldPoint();
            if (heldObject != null)
            {
                heldObject.MoveTowards(hold, dt);
            }
            else
            {
                held.transform.position = Vector3.Lerp(held.transform.position, hold, 1f - Mathf.Exp(-10f * dt));
            }

            float heldFor = Time.time - heldSince;
            bool throwNow = Owner.IsControlled
                ? Controller.Input != null && Controller.Input.AttackPressed
                : startedByAi && heldFor >= AiThrowDelay;

            if (throwNow)
            {
                Throw();
                End();
            }
            else if (heldFor >= Definition.duration)
            {
                Release(Vector3.zero);
                End();
            }
        }

        public override void OnPressedAgain()
        {
            Release(Vector3.zero);
            End();
        }

        protected override void OnEnd()
        {
            if (IsHolding)
            {
                Release(Vector3.zero);
            }

            if (aura != null)
            {
                Object.Destroy(aura);
                aura = null;
            }
        }

        private Vector3 HoldPoint()
        {
            Vector3 forward = Owner.IsControlled && Controller.CameraTransform != null
                ? Vector3.ProjectOnPlane(Controller.CameraTransform.forward, Vector3.up).normalized
                : Transform.forward;
            return Transform.position + Vector3.up * 2.6f + forward * 2.3f;
        }

        private void Throw()
        {
            GameObject held = Held;
            Vector3 from = held.transform.position;
            Vector3 direction = Controller.AimPoint(40f) - from;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Transform.forward;
            }

            Release(direction.normalized * Definition.speed);
        }

        private void Release(Vector3 velocity)
        {
            if (heldObject != null)
            {
                TelekinesisState.Mark(heldObject.gameObject, false);
                heldObject.Release(velocity, Owner.gameObject, Definition.damage);
            }
            else if (heldEnemy != null)
            {
                // Stays "lifted" until it lands: that is the window for Ulrich's finisher (synergy).
                ThrownBody.Launch(heldEnemy, velocity, Definition.damage, Owner.gameObject);
            }

            heldObject = null;
            heldEnemy = null;
            if (aura != null)
            {
                Object.Destroy(aura);
                aura = null;
            }
        }

        private bool FindTarget(out TelekinesisTarget obj, out EnemyAI enemy)
        {
            obj = null;
            enemy = null;
            Vector3 origin = Transform.position + Vector3.up * 1.2f;

            Transform aimed = Controller.CurrentTarget;
            if (aimed != null)
            {
                EnemyAI locked = aimed.GetComponentInParent<EnemyAI>();
                if (CanLift(locked) && (locked.transform.position - origin).magnitude <= Definition.range)
                {
                    enemy = locked;
                    return true;
                }
            }

            Vector3 aimDirection = (Controller.AimPoint(Definition.range) - origin).normalized;
            float bestScore = float.MaxValue;

            IReadOnlyList<TelekinesisTarget> objects = TelekinesisTarget.All;
            for (int i = 0; i < objects.Count; i++)
            {
                if (!objects[i].IsHeld && Score(origin, aimDirection, objects[i].transform.position, ref bestScore))
                {
                    obj = objects[i];
                    enemy = null;
                }
            }

            IReadOnlyList<Health> all = Health.All;
            for (int i = 0; i < all.Count; i++)
            {
                if (!AbilityMath.IsEnemy(all[i]))
                {
                    continue;
                }

                EnemyAI candidate = all[i].GetComponent<EnemyAI>();
                if (CanLift(candidate) && Score(origin, aimDirection, all[i].transform.position, ref bestScore))
                {
                    enemy = candidate;
                    obj = null;
                }
            }

            return obj != null || enemy != null;
        }

        private bool Score(Vector3 origin, Vector3 aimDirection, Vector3 position, ref float bestScore)
        {
            Vector3 toTarget = position - origin;
            float distance = toTarget.magnitude;
            if (distance > Definition.range)
            {
                return false;
            }

            float angle = Vector3.Angle(aimDirection, toTarget);
            if (angle > AimCone)
            {
                return false;
            }

            float score = angle + distance * 0.5f;
            if (score >= bestScore)
            {
                return false;
            }

            bestScore = score;
            return true;
        }

        private static bool CanLift(EnemyAI enemy)
        {
            return enemy != null && enemy.CanBeLifted && !enemy.IsSuspended && !IsDead(enemy);
        }

        private static bool IsDead(EnemyAI enemy)
        {
            Health health = enemy.GetComponent<Health>();
            return health == null || health.IsDead;
        }
    }
}
