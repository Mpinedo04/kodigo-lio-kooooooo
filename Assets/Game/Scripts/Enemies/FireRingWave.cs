using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Blok's ring of fire: expands along the ground from the monster. Jump over it.
    public sealed class FireRingWave : MonoBehaviour
    {
        private const int Segments = 24;
        private const float JumpClearance = 0.8f;

        private readonly HashSet<Health> alreadyHit = new HashSet<Health>();
        private Transform[] segments;
        private DamageInfo damage;
        private Vector3 center;
        private float maxRadius;
        private float thickness;
        private float duration;
        private float age;
        private float radius;

        public static FireRingWave Spawn(EnemyAI host, EnemyAttackDefinition attack, DamageInfo info)
        {
            GameObject root = new GameObject("FireRingWave");
            Vector3 position = host.transform.position;
            if (Physics.Raycast(position + Vector3.up, Vector3.down, out RaycastHit hit, 4f, EnemyPerception.EnvironmentMask, QueryTriggerInteraction.Ignore))
            {
                position.y = hit.point.y;
            }

            root.transform.position = position;
            FireRingWave wave = root.AddComponent<FireRingWave>();
            wave.center = position;
            wave.damage = info;
            wave.maxRadius = Mathf.Max(1f, attack.areaSize);
            wave.thickness = Mathf.Max(0.2f, attack.areaThickness);
            wave.duration = Mathf.Max(0.2f, attack.activeTime);
            wave.Build();
            return wave;
        }

        private void Build()
        {
            segments = new Transform[Segments];
            for (int i = 0; i < Segments; i++)
            {
                segments[i] = EnemyVisuals.Shape(PrimitiveType.Cube, "Flame_" + i, transform, Vector3.zero, Vector3.one, EnemyVisuals.XanaOrange).transform;
            }
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / duration);
            radius = Mathf.Lerp(0.8f, maxRadius, t);
            float circumference = 2f * Mathf.PI * radius;

            for (int i = 0; i < Segments; i++)
            {
                float angle = i * Mathf.PI * 2f / Segments;
                Vector3 offset = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;
                segments[i].localPosition = offset + Vector3.up * 0.3f;
                segments[i].localRotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, offset.normalized));
                segments[i].localScale = new Vector3(thickness * 0.5f, 0.6f * (1f - 0.5f * t), circumference / Segments * 0.9f);
            }

            EnemyAttacks.HitPlayersWhere(IsInside, damage, center, alreadyHit);

            if (age >= duration)
            {
                Destroy(gameObject);
            }
        }

        private bool IsInside(Health health)
        {
            Vector3 position = health.transform.position;
            if (position.y > center.y + JumpClearance)
            {
                return false;
            }

            float distance = Vector3.ProjectOnPlane(position - center, Vector3.up).magnitude;
            return Mathf.Abs(distance - radius) <= thickness * 0.5f + 0.35f;
        }
    }
}
