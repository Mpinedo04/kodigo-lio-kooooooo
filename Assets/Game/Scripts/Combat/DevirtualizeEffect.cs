using UnityEngine;

namespace CodeLyokoFanGame
{
    // Placeholder devirtualization: the body bursts into small polygons that shrink away.
    // Replaced by real VFX in F09.
    public sealed class DevirtualizeEffect : MonoBehaviour
    {
        private const int Pieces = 14;
        private const float Lifetime = 0.8f;

        private Transform[] pieces;
        private Vector3[] velocities;
        private float age;

        public static void Spawn(Vector3 position, Color color)
        {
            GameObject root = new GameObject("DevirtualizeEffect");
            root.transform.position = position;
            root.AddComponent<DevirtualizeEffect>().Build(color);
        }

        private void Build(Color color)
        {
            pieces = new Transform[Pieces];
            velocities = new Vector3[Pieces];

            for (int i = 0; i < Pieces; i++)
            {
                GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(piece.GetComponent<Collider>());
                piece.transform.SetParent(transform, false);
                piece.transform.localPosition = Random.insideUnitSphere * 0.5f;
                piece.transform.localScale = Vector3.one * Random.Range(0.12f, 0.28f);
                piece.GetComponent<Renderer>().material.color = color;
                pieces[i] = piece.transform;
                velocities[i] = Random.onUnitSphere * Random.Range(2f, 5f) + Vector3.up * 2f;
            }
        }

        private void Update()
        {
            age += Time.deltaTime;
            float shrink = Mathf.Clamp01(1f - age / Lifetime);

            for (int i = 0; i < pieces.Length; i++)
            {
                pieces[i].localPosition += velocities[i] * Time.deltaTime;
                pieces[i].localScale = Vector3.one * 0.2f * shrink;
            }

            if (age >= Lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
