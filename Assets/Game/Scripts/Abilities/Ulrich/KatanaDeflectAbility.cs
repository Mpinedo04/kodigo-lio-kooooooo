using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Ulrich's passive (F5.13): a laser parried with the katana (Guard at the right moment) flies back at whoever fired it.
    public sealed class KatanaDeflectAbility : Ability
    {
        private PlayerGuard guard;

        public KatanaDeflectAbility(AbilityDefinition definition) : base(definition)
        {
        }

        protected override void Activate()
        {
        }

        public override void OnEquip()
        {
            guard = Owner.GetComponent<PlayerGuard>();
            if (guard != null)
            {
                guard.Parried += OnParried;
            }
        }

        public override void OnUnequip()
        {
            if (guard != null)
            {
                guard.Parried -= OnParried;
            }
        }

        private void OnParried(DamageInfo info)
        {
            if (!Controller.IsUnlocked(Definition) || (info.type != DamageType.Laser && info.type != DamageType.Projectile))
            {
                return;
            }

            Projectile prefab = Controller.Combat != null ? Controller.Combat.ProjectilePrefab : null;
            if (prefab == null)
            {
                return;
            }

            Vector3 origin = Transform.position + Vector3.up * 1.3f + Transform.forward * 0.6f;
            Transform shooter = AimOf(info.source);
            Vector3 aim = shooter != null ? shooter.position : origin + Transform.forward * 10f;
            Vector3 direction = (aim - origin).normalized;

            DamageInfo returned = new DamageInfo
            {
                amount = info.amount * Mathf.Max(1f, Definition.power),
                type = DamageType.Projectile,
                source = Owner.gameObject,
                sourceTeam = Team.Player,
                knockback = 2f
            };

            Projectile shot = PrefabPool.Spawn(prefab, origin, Quaternion.LookRotation(direction));
            shot.Launch(Owner.gameObject, Team.Player, direction, returned, shooter, shooter != null ? 6f : 0f, 0, 1.2f, 0f, 1.3f);
            Controller.Notify("Laser devuelto!");
        }

        private static Transform AimOf(GameObject source)
        {
            if (source == null || !source.activeInHierarchy)
            {
                return null;
            }

            EnemyAI enemy = source.GetComponentInParent<EnemyAI>();
            if (enemy != null && enemy.AimPoint != null)
            {
                return enemy.AimPoint;
            }

            return source.transform;
        }
    }
}
