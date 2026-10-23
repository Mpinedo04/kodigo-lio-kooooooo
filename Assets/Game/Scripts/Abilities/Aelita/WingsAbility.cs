using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Aelita's wings (F5.24, Season 3+): hold Jump while falling to glide; press Jump in the air to flap up.
    // Both use flight energy, which refills on the ground. speed = glide fall speed, power = flap speed,
    // cost = energy per flap, costPerSecond = energy while gliding.
    public sealed class WingsAbility : Ability
    {
        private const float MinAirTimeToFlap = 0.12f;

        private GameObject wings;
        private float airTime;

        public WingsAbility(AbilityDefinition definition) : base(definition)
        {
        }

        public override string Status => wings != null && wings.activeSelf ? "Alas" : null;

        protected override void Activate()
        {
        }

        public override void OnEquip()
        {
            wings = new GameObject("Wings");
            wings.transform.SetParent(Transform, false);
            wings.transform.localPosition = new Vector3(0f, 1.4f, -0.35f);
            AbilityVisuals.Shape(PrimitiveType.Cube, "WingLeft", AbilityVisuals.AelitaPink, wings.transform, new Vector3(-0.7f, 0.1f, 0f), new Vector3(1.2f, 0.5f, 0.05f)).transform.localRotation = Quaternion.Euler(0f, 0f, 15f);
            AbilityVisuals.Shape(PrimitiveType.Cube, "WingRight", AbilityVisuals.AelitaPink, wings.transform, new Vector3(0.7f, 0.1f, 0f), new Vector3(1.2f, 0.5f, 0.05f)).transform.localRotation = Quaternion.Euler(0f, 0f, -15f);
            wings.SetActive(false);
        }

        public override void Tick(float dt)
        {
            InputReader input = Controller.Input;
            if (!Owner.IsControlled || Owner.IsMounted || input == null || Owner.IsGrounded)
            {
                Owner.GlideFallSpeed = 0f;
                airTime = 0f;
                ShowWings(false);
                return;
            }

            airTime += dt;
            bool flying = false;
            if (input.JumpPressed && airTime >= MinAirTimeToFlap && Controller.FlightEnergy.TrySpend(Definition.cost))
            {
                Owner.SetVerticalVelocity(Definition.power);
                flying = true;
            }
            else if (input.JumpHeld && Owner.Velocity.y <= 0f && Controller.FlightEnergy.Current > 0f)
            {
                Controller.FlightEnergy.Drain(Definition.costPerSecond * dt);
                flying = true;
            }

            Owner.GlideFallSpeed = flying ? Definition.speed : 0f;
            ShowWings(flying || Owner.Velocity.y > 0.5f && wings != null && wings.activeSelf);
        }

        private void ShowWings(bool show)
        {
            if (wings != null && wings.activeSelf != show)
            {
                wings.SetActive(show);
            }
        }
    }
}
