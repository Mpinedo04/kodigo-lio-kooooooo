using UnityEngine;

namespace CodeLyokoFanGame
{
    // Glowing ring that grows and fades (Impact, Fan Dance, tower deactivation, virtualization).
    public sealed class ShockwaveEffect : MonoBehaviour
    {
        private const float Lifetime = 0.4f;

        private Renderer ring;
        private Color color;
        private float radius;
        private float age;

        public static void Spawn(Vector3 position, float radius, Color color)
        {
            GameObject effect = new GameObject("Shockwave");
            effect.transform.position = position;
            effect.AddComponent<MeshFilter>().sharedMesh = LowPolyMesh.Ring(0.5f, 0.04f, 32, 4);
            MeshRenderer renderer = effect.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = LyokoMaterials.Glow(Color.white);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ShockwaveEffect shockwave = effect.AddComponent<ShockwaveEffect>();
            shockwave.ring = renderer;
            shockwave.color = color;
            shockwave.radius = radius;
            shockwave.Apply(0f);
        }

        private void Update()
        {
            age += Time.deltaTime;
            Apply(Mathf.Clamp01(age / Lifetime));
            if (age >= Lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void Apply(float t)
        {
            float diameter = Mathf.Lerp(0.3f, radius * 2f, 1f - (1f - t) * (1f - t));
            transform.localScale = new Vector3(diameter, diameter * 0.5f, diameter);
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            Color faded = color * 2f;
            faded.a = 1f - t;
            block.SetColor("_Color", faded);
            ring.SetPropertyBlock(block);
        }
    }
}
