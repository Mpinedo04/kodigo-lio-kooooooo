using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Ulrich's ultimate, Impact (F5.14, Season 4): a blow that knocks down everything around him.
    public sealed class ImpactAbility : Ability
    {
        public ImpactAbility(AbilityDefinition definition) : base(definition)
        {
        }

        protected override void Activate()
        {
            DamageInfo blow = new DamageInfo
            {
                amount = Definition.damage,
                type = DamageType.Melee,
                source = Owner.gameObject,
                sourceTeam = Team.Player,
                knockback = Definition.power,
                isHeavy = true,
                stagger = 1.5f
            };

            AbilityMath.AreaHit(Transform.position + Vector3.up * 0.5f, Definition.radius, blow, false);
            ShockwaveEffect.Spawn(Transform.position + Vector3.up * 0.1f, Definition.radius, AbilityVisuals.UlrichGreen);
            HitStop.Trigger(0.1f);
            CameraRig.Shake(0.8f);
            End();
        }
    }
}
