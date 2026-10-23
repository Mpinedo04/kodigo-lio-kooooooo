using UnityEngine;

namespace CodeLyokoFanGame
{
    // Placeholder vehicle explosion: a flash that swells and fades plus chunks thrown outward.
    // Replaced by real VFX in F09.
    public sealed class VehicleExplosion : MonoBehaviour
    {
        private const int Pieces = 18;
        private const float Lifetime = 1f;

        private Transform flash;
        private Transform[] pieces;
        private Vector3[] velocities;
        private float age;

        public static void Spawn(Vector3 position, Color color)
        {
            GameObject root = new GameObject("VehicleExplosion");
            root.transform.position = position;
            root.AddComponent<VehicleExplosion>().Build(color);
        }

        private void Build(Color color)
        {
            flash = CreatePiece(PrimitiveType.Sphere, new Color(1f, 0.85f, 0.5f)).transform;
            flash.localScale = Vector3.one * 0.5f;

            pieces = new Transform[Pieces];
            velocities = new Vector3[Pieces];
            for (int i = 0; i < Pieces; i++)
            {
                GameObject piece = CreatePiece(PrimitiveType.Cube, i % 3 == 0 ? new Color(1f, 0.35f, 0.05f) : color);
                piece.transform.localPosition = Random.insideUnitSphere * 0.4f;
                piece.transform.localRotation = Random.rotation;
                piece.transform.localScale = Vector3.one * Random.Range(0.15f, 0.35f);
                pieces[i] = piece.transform;
                velocities[i] = Random.onUnitSphere * Random.Range(4f, 9f) + Vector3.up * 3f;
            }
        }

        private GameObject CreatePiece(PrimitiveType primitive, Color color)
        {
            GameObject piece = GameObject.CreatePrimitive(primitive);
            DestroyImmediate(piece.GetComponent<Collider>());
            piece.transform.SetParent(transform, false);
            piece.GetComponent<MeshFilter>().sharedMesh = primitive == PrimitiveType.Cube ? LowPolyMesh.Gem(0.55f, 0) : LowPolyMesh.Faceted(primitive);
            Renderer renderer = piece.GetComponent<Renderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            LyokoMaterials.Paint(renderer, color, 1.5f);
            return piece;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            age += dt;
            float t = Mathf.Clamp01(age / Lifetime);

            flash.localScale = Vector3.one * (t < 0.2f ? Mathf.Lerp(0.5f, 4.5f, t / 0.2f) : Mathf.Lerp(4.5f, 0f, (t - 0.2f) / 0.8f));

            for (int i = 0; i < pieces.Length; i++)
            {
                velocities[i] += Vector3.down * 12f * dt;
                pieces[i].localPosition += velocities[i] * dt;
                pieces[i].Rotate(velocities[i] * 40f * dt, Space.Self);
                pieces[i].localScale = Vector3.one * 0.25f * (1f - t);
            }

            if (age >= Lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
