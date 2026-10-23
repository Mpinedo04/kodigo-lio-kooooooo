using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // F06.6: the devirtualized monster bursts into fragments of its own colours and drops Data.
    // Lives on its own object, so the monster itself can be removed right away.
    public sealed class EnemyDevirtualize : MonoBehaviour
    {
        private const float Lifetime = 1.3f;
        private const float Gravity = 9f;
        private const int MaxFragments = 28;

        private readonly List<Transform> fragments = new List<Transform>();
        private readonly List<Vector3> velocities = new List<Vector3>();
        private readonly List<Vector3> spins = new List<Vector3>();
        private readonly List<Vector3> sizes = new List<Vector3>();
        private float age;

        public static void Spawn(EnemyAI enemy, int dataCount)
        {
            if (enemy == null || !Application.isPlaying)
            {
                return;
            }

            GameObject root = new GameObject("EnemyDevirtualize_" + enemy.Kind);
            root.transform.position = enemy.transform.position;
            EnemyDevirtualize effect = root.AddComponent<EnemyDevirtualize>();
            Vector3 center = effect.Build(enemy);

            for (int i = 0; i < dataCount; i++)
            {
                DataPickup.Spawn(center + Random.insideUnitSphere * 0.4f);
            }
        }

        private Vector3 Build(EnemyAI enemy)
        {
            Renderer[] renderers = enemy.GetComponentsInChildren<Renderer>();
            Bounds total = new Bounds(enemy.transform.position + Vector3.up, Vector3.one);
            bool hasBounds = false;
            int perRenderer = Mathf.Clamp(MaxFragments / Mathf.Max(1, renderers.Length), 2, 8);

            for (int r = 0; r < renderers.Length; r++)
            {
                Renderer source = renderers[r];
                if (!source.enabled || fragments.Count >= MaxFragments)
                {
                    continue;
                }

                Bounds bounds = source.bounds;
                if (hasBounds)
                {
                    total.Encapsulate(bounds);
                }
                else
                {
                    total = bounds;
                    hasBounds = true;
                }

                Color color = EnemyVisuals.ColorOf(source, EnemyVisuals.XanaRed);
                for (int i = 0; i < perRenderer && fragments.Count < MaxFragments; i++)
                {
                    Vector3 point = bounds.center + Vector3.Scale(Random.insideUnitSphere * 0.5f, bounds.size);
                    float size = Mathf.Clamp(bounds.size.magnitude * Random.Range(0.12f, 0.22f), 0.08f, 0.6f);
                    GameObject piece = EnemyVisuals.Shape(PrimitiveType.Cube, "Fragment", transform, Vector3.zero, Vector3.one * size, color);
                    piece.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.Gem(0.55f, 0);
                    piece.transform.position = point;
                    piece.transform.rotation = Random.rotation;
                    fragments.Add(piece.transform);
                    sizes.Add(piece.transform.localScale);
                    Vector3 outward = (point - bounds.center).sqrMagnitude > 0.0001f ? (point - bounds.center).normalized : Random.onUnitSphere;
                    velocities.Add(outward * Random.Range(2.5f, 6f) + Vector3.up * Random.Range(2f, 4.5f));
                    spins.Add(Random.insideUnitSphere * 540f);
                }
            }

            return total.center;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            age += deltaTime;
            float shrink = Mathf.Clamp01((Lifetime - age) / (Lifetime * 0.5f));

            for (int i = 0; i < fragments.Count; i++)
            {
                velocities[i] += Vector3.down * Gravity * deltaTime;
                fragments[i].position += velocities[i] * deltaTime;
                fragments[i].Rotate(spins[i] * deltaTime, Space.World);
                fragments[i].localScale = sizes[i] * shrink;
            }

            if (age >= Lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
