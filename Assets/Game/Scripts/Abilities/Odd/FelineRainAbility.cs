using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Odd's ultimate, Feline Rain (F5.8): a high leap and a rain of laser arrows over an area.
    public sealed class FelineRainAbility : Ability
    {
        private const float RainHeight = 14f;
        private const float LeapDelay = 0.35f;

        private Vector3 center;
        private float startsAt;
        private int fired;

        public FelineRainAbility(AbilityDefinition definition) : base(definition)
        {
        }

        // Pressing again does not cut it short.
        public override void OnPressedAgain()
        {
        }

        protected override void Activate()
        {
            Transform target = Controller.CurrentTarget;
            center = target != null ? target.position : Transform.position + Controller.FlatAimDirection() * 6f;
            startsAt = Time.time + LeapDelay;
            fired = 0;

            if (Owner.IsControlled)
            {
                Owner.SetVerticalVelocity(Definition.speed);
            }

            Controller.Notify("Lluvia felina!");
        }

        public override void Tick(float dt)
        {
            if (Time.time < startsAt)
            {
                return;
            }

            int total = Mathf.Max(1, Definition.count);
            float interval = Definition.duration / total;
            while (fired < total && Time.time >= startsAt + fired * interval)
            {
                FireArrow();
                fired++;
            }

            if (fired >= total)
            {
                End();
            }
        }

        private void FireArrow()
        {
            Projectile prefab = Controller.Combat != null ? Controller.Combat.ProjectilePrefab : null;
            if (prefab == null)
            {
                return;
            }

            Vector2 spread = Random.insideUnitCircle * Definition.radius;
            Vector3 landing = center + new Vector3(spread.x, 0f, spread.y);
            Vector3 from = landing + Vector3.up * RainHeight + Random.insideUnitSphere;
            Vector3 direction = (landing - from).normalized;

            DamageInfo info = new DamageInfo
            {
                amount = Definition.damage,
                type = DamageType.Projectile,
                source = Owner.gameObject,
                sourceTeam = Team.Player,
                knockback = 1.5f
            };

            Projectile arrow = PrefabPool.Spawn(prefab, from, Quaternion.LookRotation(direction));
            arrow.Launch(Owner.gameObject, Team.Player, direction, info, null, 0f, 0, 1.2f, 0f, 1.4f);
        }
    }
}
