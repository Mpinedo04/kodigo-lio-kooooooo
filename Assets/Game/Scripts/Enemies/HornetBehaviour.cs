using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // F6.11 Hornet: circles high around its target, dives to fire a burst, climbs again.
    // Left alone (no other monster nearby) it keeps its distance and only snipes now and then.
    public sealed class HornetBehaviour : EnemyBehaviour
    {
        private const float DiveHeight = 2.2f;
        private const float AloneRadius = 22f;
        private const float AloneShotInterval = 6f;

        private float orbitAngle;
        private float orbitDirection = 1f;
        private float lastAloneShot = -100f;
        private float flapSeed;
        private Transform leftWing;
        private Transform rightWing;

        public bool IsAlone { get; private set; }

        protected override void OnBind()
        {
            orbitDirection = Random.value < 0.5f ? -1f : 1f;
            orbitAngle = Random.Range(0f, Mathf.PI * 2f);
            flapSeed = Random.Range(0f, 10f);
            leftWing = Host.FindPart("Wing_L");
            rightWing = Host.FindPart("Wing_R");
        }

        public override void Tick(float deltaTime)
        {
            float flap = Mathf.Sin((Brain.Time + flapSeed) * 40f) * 25f;
            if (leftWing != null)
            {
                leftWing.localRotation = Quaternion.Euler(0f, 0f, flap);
            }

            if (rightWing != null)
            {
                rightWing.localRotation = Quaternion.Euler(0f, 0f, -flap);
            }
        }

        public override void TickIdle(float deltaTime)
        {
            // Lazy circles around home.
            orbitAngle += orbitDirection * 0.6f * deltaTime;
            Motor.DesiredAltitude = Motor.CruiseAltitude;
            Vector3 point = Motor.Home + new Vector3(Mathf.Sin(orbitAngle), 0f, Mathf.Cos(orbitAngle)) * 3f;
            Motor.MoveTowards(point, 0.4f, deltaTime);
            Motor.FaceMovement(Definition.turnSpeed, deltaTime);
        }

        public override void TickChase(Health target, float deltaTime)
        {
            IsAlone = !HasAlliesNearby();
            Vector3 center = target.transform.position;
            float groundY = Mathf.Max(center.y, Motor.HomeGroundY);

            if (IsAlone)
            {
                // Flee: back off to the edge of its range, higher up.
                Vector3 away = Vector3.ProjectOnPlane(Host.transform.position - center, Vector3.up);
                if (away.sqrMagnitude < 0.01f)
                {
                    away = Vector3.forward;
                }

                Motor.DesiredAltitude = groundY + Definition.hoverHeight + 2f;
                Motor.MoveTowards(center + away.normalized * (Definition.attackRange + 4f), 1.2f, deltaTime);
            }
            else
            {
                float radius = Mathf.Max(2f, Definition.preferredDistance);
                orbitAngle += orbitDirection * Definition.moveSpeed / radius * deltaTime;
                Motor.DesiredAltitude = groundY + Definition.hoverHeight;
                Motor.MoveTowards(center + new Vector3(Mathf.Sin(orbitAngle), 0f, Mathf.Cos(orbitAngle)) * radius, 1f, deltaTime);
            }

            Motor.FaceTowards(center, Definition.turnSpeed, deltaTime);
        }

        public override bool WantsToAttack(Health target)
        {
            if (!IsAlone)
            {
                return true;
            }

            if (Brain.Time - lastAloneShot < AloneShotInterval)
            {
                return false;
            }

            lastAloneShot = Brain.Time;
            return true;
        }

        // Dive towards the target before the burst (skipped when fleeing).
        public override void TickWindup(EnemyAttackDefinition attack, Health target, float progress, float deltaTime)
        {
            if (target == null)
            {
                return;
            }

            Vector3 center = target.transform.position;
            Motor.FaceTowards(center, Definition.turnSpeed * 2f, deltaTime);
            if (IsAlone)
            {
                return;
            }

            Motor.DesiredAltitude = center.y + DiveHeight;
            Vector3 toward = Vector3.ProjectOnPlane(Host.transform.position - center, Vector3.up);
            Vector3 divePoint = center + (toward.sqrMagnitude > 0.01f ? toward.normalized : Vector3.forward) * 6f;
            Motor.MoveTowards(divePoint, 1.6f, deltaTime);
        }

        public override void TickRecovery(EnemyAttackDefinition attack, Health target, float deltaTime)
        {
            Motor.DesiredAltitude = Motor.CruiseAltitude + (target != null ? Mathf.Max(0f, target.transform.position.y - Motor.HomeGroundY) : 0f);
            Motor.MoveTowards(Host.transform.position + Host.transform.forward * -2f, 1f, deltaTime);
        }

        public override void OnStaggerStart()
        {
            base.OnStaggerStart();
            Motor.DesiredAltitude = Host.transform.position.y - 1f;
        }

        public override void TickStagger(float deltaTime)
        {
            Motor.MoveTowards(Host.transform.position, 0.5f, deltaTime);
        }

        private bool HasAlliesNearby()
        {
            IReadOnlyList<EnemyAI> all = EnemyAI.All;
            for (int i = 0; i < all.Count; i++)
            {
                EnemyAI other = all[i];
                if (other != null && other != Host && !other.IsDead
                    && Vector3.Distance(other.transform.position, Host.transform.position) <= AloneRadius)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
