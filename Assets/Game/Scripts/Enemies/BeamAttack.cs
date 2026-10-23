using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Megatank beam. Horizontal: wide and low (jump over it). Vertical: tall and thin (step aside).
    // Walls (Environment) cut it short. Also used as the 1-second warning preview.
    public sealed class BeamAttack : MonoBehaviour
    {
        private static readonly Collider[] Buffer = new Collider[32];

        private readonly HashSet<Health> alreadyHit = new HashSet<Health>();
        private DamageInfo damage;
        private Vector3 halfExtents;
        private float duration;
        private float age;
        private bool preview;
        private Transform visual;

        public static BeamAttack Spawn(EnemyAI host, EnemyAttackDefinition attack, DamageInfo info)
        {
            BeamAttack beam = Create(host, attack, false);
            beam.damage = info;
            beam.duration = Mathf.Max(0.1f, attack.activeTime);
            return beam;
        }

        public static BeamAttack Preview(EnemyAI host, EnemyAttackDefinition attack)
        {
            BeamAttack beam = Create(host, attack, true);
            beam.duration = float.MaxValue;
            return beam;
        }

        public void Aim(EnemyAI host, EnemyAttackDefinition attack)
        {
            Place(host, attack);
        }

        private static BeamAttack Create(EnemyAI host, EnemyAttackDefinition attack, bool isPreview)
        {
            GameObject root = new GameObject(isPreview ? "BeamPreview" : "MegatankBeam");
            BeamAttack beam = root.AddComponent<BeamAttack>();
            beam.preview = isPreview;
            Color color = isPreview ? new Color(1f, 0.35f, 0.3f) : EnemyVisuals.XanaRed;
            beam.visual = EnemyVisuals.Shape(PrimitiveType.Cube, "BeamVisual", root.transform, Vector3.zero, Vector3.one, color).transform;
            beam.visual.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.Box(Vector3.one);
            beam.Place(host, attack);
            return beam;
        }

        private void Place(EnemyAI host, EnemyAttackDefinition attack)
        {
            bool horizontal = attack.type == EnemyAttackType.BeamHorizontal;
            Vector3 origin = EnemyAttacks.FireOrigin(host);
            Vector3 forward = Vector3.ProjectOnPlane(host.transform.forward, Vector3.up).normalized;
            float groundY = host.transform.position.y;
            float length = Mathf.Max(1f, attack.areaSize);

            float beamHeight = horizontal ? groundY + 0.2f + attack.areaThickness * 0.5f : groundY + attack.areaWidth * 0.5f;
            Vector3 start = new Vector3(origin.x, beamHeight, origin.z);
            if (Physics.Raycast(start, forward, out RaycastHit wall, length, EnemyPerception.EnvironmentMask, QueryTriggerInteraction.Ignore))
            {
                length = Mathf.Max(1f, wall.distance);
            }

            Vector3 size = horizontal
                ? new Vector3(attack.areaWidth, attack.areaThickness, length)
                : new Vector3(attack.areaThickness, attack.areaWidth, length);
            if (preview)
            {
                size = horizontal ? new Vector3(attack.areaWidth, 0.05f, length) : new Vector3(0.12f, attack.areaWidth, length);
            }

            halfExtents = size * 0.5f;
            transform.SetPositionAndRotation(start + forward * length * 0.5f, Quaternion.LookRotation(forward));
            visual.localScale = size;
        }

        private void Update()
        {
            age += Time.deltaTime;
            if (age >= duration)
            {
                Destroy(gameObject);
                return;
            }

            if (preview)
            {
                return;
            }

            float flicker = 1f + 0.08f * Mathf.Sin(age * 60f);
            visual.localScale = new Vector3(halfExtents.x * 2f * flicker, halfExtents.y * 2f * flicker, halfExtents.z * 2f);
            int count = Physics.OverlapBoxNonAlloc(transform.position, halfExtents, Buffer, transform.rotation, Physics.AllLayers, QueryTriggerInteraction.Collide);
            DamageInfo hit = damage;
            hit.point = transform.position;
            hit.direction = transform.forward;
            HitResolver.ApplyToColliders(Buffer, count, hit, Team.Enemy, alreadyHit);
        }
    }
}
