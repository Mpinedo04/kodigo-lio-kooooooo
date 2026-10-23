using UnityEngine;

namespace CodeLyokoFanGame
{
    // Devirtualization (F9.22): the avatar bursts into glowing polygons that fly apart and shrink,
    // with a flash ring. Used by warriors, Aelita, clones and abilities.
    public sealed class DevirtualizeEffect : MonoBehaviour
    {
        private const int Pieces = 22;
        private const float Lifetime = 0.9f;

        private Transform[] pieces;
        private Vector3[] velocities;
        private Vector3[] spins;
        private float[] sizes;
        private float age;

        public static void Spawn(Vector3 position, Color color)
        {
            GameObject root = new GameObject("DevirtualizeEffect");
            root.transform.position = position;
            root.AddComponent<DevirtualizeEffect>().Build(color);
            ShockwaveEffect.Spawn(position, 2.2f, color);
        }

        private void Build(Color color)
        {
            pieces = new Transform[Pieces];
            velocities = new Vector3[Pieces];
            spins = new Vector3[Pieces];
            sizes = new float[Pieces];
            Material solid = LyokoMaterials.Toon(color, 1.2f);
            Material glow = LyokoMaterials.Glow(color * 1.5f);
            Mesh shard = LowPolyMesh.Gem(0.5f, 0);

            for (int i = 0; i < Pieces; i++)
            {
                GameObject piece = new GameObject("Shard");
                piece.transform.SetParent(transform, false);
                piece.transform.localPosition = new Vector3(Random.Range(-0.35f, 0.35f), Random.Range(-0.8f, 0.7f), Random.Range(-0.35f, 0.35f));
                piece.transform.rotation = Random.rotation;
                piece.AddComponent<MeshFilter>().sharedMesh = shard;
                MeshRenderer renderer = piece.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = i % 3 == 0 ? glow : solid;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                sizes[i] = Random.Range(0.1f, 0.26f);
                pieces[i] = piece.transform;
                velocities[i] = Random.onUnitSphere * Random.Range(1.5f, 4.5f) + Vector3.up * Random.Range(1f, 3f);
                spins[i] = Random.insideUnitSphere * 720f;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            age += dt;
            float shrink = Mathf.Clamp01(1f - age / Lifetime);

            for (int i = 0; i < pieces.Length; i++)
            {
                velocities[i] *= 1f - 1.5f * dt;
                pieces[i].localPosition += velocities[i] * dt;
                pieces[i].Rotate(spins[i] * dt, Space.World);
                pieces[i].localScale = Vector3.one * sizes[i] * shrink;
            }

            if (age >= Lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
