using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Yumi's telekinesis wall (F5.19): raises a slab of terrain in front of her that stops lasers for a while.
    // radius = width, power = height, range = distance in front.
    public sealed class TelekinesisWallAbility : Ability
    {
        private const float Thickness = 0.7f;

        public TelekinesisWallAbility(AbilityDefinition definition) : base(definition)
        {
        }

        public override string UnavailableReason => "Muro: no hay suelo delante";

        public override bool CanUse()
        {
            return base.CanUse() && TryGetSpot(out _);
        }

        protected override void Activate()
        {
            if (TryGetSpot(out Vector3 ground))
            {
                float height = Mathf.Max(1f, Definition.power);
                Quaternion facing = Quaternion.LookRotation(Controller.FlatAimDirection());
                Vector3 size = new Vector3(Mathf.Max(1f, Definition.radius), height, Thickness);
                TemporaryStructure.Create("TelekinesisWall", ground + Vector3.up * height * 0.5f, facing, size, AbilityVisuals.TerrainBrown, Definition.duration, TemporaryStructure.Appear.Rise, true);
            }

            End();
        }

        private bool TryGetSpot(out Vector3 ground)
        {
            Vector3 point = Transform.position + Controller.FlatAimDirection() * Definition.range;
            return AbilityMath.TryFindGround(point, 4f, out ground);
        }
    }
}
