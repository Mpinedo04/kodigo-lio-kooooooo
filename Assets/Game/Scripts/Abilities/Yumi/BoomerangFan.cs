using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Yumi's tessen thrown like a boomerang (F5.15): flies out, comes back to her hand, and hits on both legs.
    // curve (degrees) bends the path to one side for the wide double throw (F5.16).
    public sealed class BoomerangFan : MonoBehaviour
    {
        private const float CatchDistance = 0.9f;
        private const float MaxLifetime = 4f;

        private readonly HashSet<Health> hitOut = new HashSet<Health>();
        private readonly HashSet<Health> hitBack = new HashSet<Health>();
        private PlayerCombat owner;
        private DamageInfo damage;
        private Vector3 direction;
        private float range;
        private float speed;
        private float curve;
        private float travelled;
        private float age;
        private bool returning;

        public static BoomerangFan Throw(PlayerCombat owner, Vector3 origin, Vector3 direction, DamageInfo damage, float range, float speed, float curve)
        {
            GameObject fan = AbilityVisuals.Shape(PrimitiveType.Cylinder, "YumiFan", AbilityVisuals.YumiRed, null, origin, new Vector3(0.65f, 0.03f, 0.65f), true);
            Collider collider = fan.GetComponent<Collider>();
            collider.isTrigger = true;

            // F09: an open fan (half disc) on a spinning pivot, with a red trail.
            fan.GetComponent<MeshRenderer>().enabled = false;
            GameObject blade = new GameObject("FanBlade");
            blade.transform.SetParent(fan.transform, false);
            blade.transform.localScale = new Vector3(1f / 0.65f, 1f / 0.03f, 1f / 0.65f);
            blade.AddComponent<MeshFilter>().sharedMesh = LowPolyMesh.Sector(0.38f, 170f, 0.03f, 8);
            blade.AddComponent<MeshRenderer>().sharedMaterial = LyokoMaterials.Toon(AbilityVisuals.YumiRed, 0.3f, 1f);
            TrailRenderer trail = fan.AddComponent<TrailRenderer>();
            trail.time = 0.18f;
            trail.startWidth = 0.35f;
            trail.endWidth = 0f;
            trail.sharedMaterial = LyokoMaterials.Glow(new Color(1f, 0.3f, 0.35f, 0.7f), 0f, 0.6f);
            Rigidbody body = fan.AddComponent<Rigidbody>();
            body.isKinematic = true;
            GameLayers.SetRecursively(fan, GameLayers.PlayerAttack);

            BoomerangFan boomerang = fan.AddComponent<BoomerangFan>();
            boomerang.owner = owner;
            boomerang.damage = damage;
            boomerang.direction = Quaternion.Euler(0f, curve, 0f) * direction.normalized;
            boomerang.range = range;
            boomerang.speed = speed;
            boomerang.curve = curve;
            return boomerang;
        }

        private Vector3 HandPosition => owner.transform.position + Vector3.up * 1.3f;

        private void Update()
        {
            float dt = Time.deltaTime;
            age += dt;
            transform.Rotate(0f, 1080f * dt, 0f, Space.Self);

            if (owner == null || !owner.isActiveAndEnabled || age >= MaxLifetime)
            {
                Finish();
                return;
            }

            if (!returning)
            {
                // Curved throws bend back towards the middle as they go.
                direction = Quaternion.Euler(0f, -curve * 1.6f * dt * speed / Mathf.Max(1f, range), 0f) * direction;
                Vector3 step = direction * speed * dt;
                transform.position += step;
                travelled += step.magnitude;
                if (travelled >= range)
                {
                    returning = true;
                }

                return;
            }

            Vector3 toHand = HandPosition - transform.position;
            if (toHand.magnitude <= CatchDistance)
            {
                Finish();
                return;
            }

            transform.position += toHand.normalized * speed * 1.15f * dt;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (owner != null && other.transform.IsChildOf(owner.transform))
            {
                return;
            }

            if (!returning && GameLayers.Is(other.gameObject, GameLayers.Environment) && !other.isTrigger)
            {
                returning = true;
                return;
            }

            DamageInfo hit = damage;
            hit.point = transform.position;
            hit.direction = returning ? HandPosition - transform.position : direction;
            HitResolver.ApplyToCollider(other, hit, Team.Player, returning ? hitBack : hitOut);
        }

        private void Finish()
        {
            if (owner != null)
            {
                owner.OnFanReturned();
            }

            owner = null;
            Destroy(gameObject);
        }
    }
}
