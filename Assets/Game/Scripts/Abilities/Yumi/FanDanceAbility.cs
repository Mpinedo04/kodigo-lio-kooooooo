using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Yumi's ultimate, Fan Dance (F5.21): a whirlwind of fans that pulls nearby monsters in, then bursts.
    public sealed class FanDanceAbility : Ability
    {
        private const float PulseInterval = 0.25f;
        private const float PullForce = 5f;
        private const float FinalPush = 10f;
        private const int FanCount = 4;

        private Transform whirl;
        private float endsAt;
        private float nextPulse;

        public FanDanceAbility(AbilityDefinition definition) : base(definition)
        {
        }

        public override string Status => IsActive ? "Danza de abanicos" : null;

        // Pressing again does not cut it short.
        public override void OnPressedAgain()
        {
        }

        protected override void Activate()
        {
            endsAt = Time.time + Definition.duration;
            nextPulse = Time.time;
            whirl = new GameObject("FanDance").transform;
            whirl.SetParent(Transform, false);
            whirl.localPosition = Vector3.up;
            for (int i = 0; i < FanCount; i++)
            {
                Quaternion around = Quaternion.Euler(0f, 360f / FanCount * i, 0f);
                GameObject fan = AbilityVisuals.Shape(PrimitiveType.Cylinder, "Fan", AbilityVisuals.YumiRed, whirl, around * Vector3.forward * 2f, new Vector3(0.7f, 0.03f, 0.7f));
                fan.transform.localRotation = around;
            }

            Controller.Notify("Danza de abanicos!");
        }

        public override void Tick(float dt)
        {
            if (whirl != null)
            {
                whirl.Rotate(0f, 720f * dt, 0f, Space.World);
            }

            if (Time.time >= nextPulse)
            {
                nextPulse += PulseInterval;
                Pulse(Definition.damage, PullForce, true);
            }

            if (Time.time >= endsAt)
            {
                Pulse(Definition.damage * 4f, FinalPush, false);
                ShockwaveEffect.Spawn(Transform.position + Vector3.up * 0.1f, Definition.radius, AbilityVisuals.YumiRed);
                CameraRig.Shake(0.4f);
                End();
            }
        }

        private void Pulse(float amount, float force, bool pull)
        {
            DamageInfo gust = new DamageInfo
            {
                amount = amount,
                type = DamageType.Ability,
                source = Owner.gameObject,
                sourceTeam = Team.Player,
                knockback = force,
                isHeavy = !pull
            };

            AbilityMath.AreaHit(Transform.position + Vector3.up, Definition.radius, gust, pull);
        }

        protected override void OnEnd()
        {
            if (whirl != null)
            {
                Object.Destroy(whirl.gameObject);
                whirl = null;
            }
        }
    }
}
