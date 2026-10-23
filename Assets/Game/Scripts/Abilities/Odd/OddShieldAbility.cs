using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Odd's violet shield (F5.5): blocks lasers from the front, has its own points, breaks and recharges.
    public sealed class OddShieldAbility : Ability
    {
        private GameObject visual;
        private float points;
        private float endsAt;
        private bool broken;

        public OddShieldAbility(AbilityDefinition definition) : base(definition)
        {
        }

        public float MaxPoints => Mathf.Max(1f, Definition.abilityHealth);
        public float Points => points;

        public override string Status => IsActive ? "Escudo " + Mathf.CeilToInt(points) + "/" + Mathf.CeilToInt(MaxPoints) : null;

        protected override void Activate()
        {
            points = MaxPoints;
            broken = false;
            endsAt = Time.time + Definition.duration;

            if (visual == null)
            {
                // F09: a translucent violet energy disc with a solid rim.
                visual = AbilityVisuals.Shape(PrimitiveType.Sphere, "OddShield", AbilityVisuals.OddViolet, Transform, new Vector3(0f, 1.25f, 0.75f), new Vector3(1.5f, 1.5f, 0.15f));
                visual.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.Gem(0.5f, 1);
                visual.GetComponent<Renderer>().sharedMaterial = LyokoMaterials.Glow(new Color(0.62f, 0.3f, 1f, 0.55f));
                GameObject rim = AbilityVisuals.Shape(PrimitiveType.Sphere, "ShieldRim", AbilityVisuals.OddViolet, visual.transform, Vector3.zero, Vector3.one * 2f);
                rim.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.Ring(0.25f, 0.02f, 16, 4);
                rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                rim.GetComponent<Renderer>().sharedMaterial = LyokoMaterials.Toon(AbilityVisuals.OddViolet, 1.5f);
            }

            visual.SetActive(true);
        }

        public override void Tick(float dt)
        {
            if (Time.time >= endsAt)
            {
                End();
            }
        }

        protected override void OnEnd()
        {
            if (visual != null)
            {
                visual.SetActive(false);
            }

            // Broken: full recharge. Lowered early: the less it took, the sooner it is back.
            if (!broken)
            {
                StartCooldown(Mathf.Max(1f, Definition.cooldown * (1f - points / MaxPoints)));
            }
        }

        public override bool TryIntercept(DamageInfo info)
        {
            if (!ShieldMath.Blocks(Transform.forward, info))
            {
                return false;
            }

            points -= info.amount;
            HitStop.Trigger(0.03f);
            if (points <= 0f)
            {
                broken = true;
                Controller.Notify("Escudo roto!");
                End();
            }

            return true;
        }
    }
}
