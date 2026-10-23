using UnityEngine;

namespace CodeLyokoFanGame
{
    // Virtualization (F9.21): the warrior is drawn from the feet up while rings of light slide down around them.
    // Played when a warrior arrives in Lyoko and after an Emergency Return.
    public sealed class VirtualizationEffect : MonoBehaviour
    {
        private const float Duration = 0.9f;
        private const int Rings = 3;

        private Transform model;
        private Vector3 modelScale;
        private Transform[] rings;
        private Renderer[] ringRenderers;
        private Color color;
        private float age;

        public static void Play(Transform warrior, Color color)
        {
            if (warrior == null || !Application.isPlaying)
            {
                return;
            }

            VirtualizationEffect existing = warrior.GetComponent<VirtualizationEffect>();
            if (existing != null)
            {
                Destroy(existing);
            }

            warrior.gameObject.AddComponent<VirtualizationEffect>().Begin(color);
        }

        private void Begin(Color newColor)
        {
            color = newColor;
            CharacterRig rig = GetComponentInChildren<CharacterRig>();
            model = rig != null ? rig.transform : null;
            if (model != null)
            {
                modelScale = model.localScale;
            }

            rings = new Transform[Rings];
            ringRenderers = new Renderer[Rings];
            for (int i = 0; i < Rings; i++)
            {
                GameObject ring = new GameObject("VirtualizationRing");
                ring.transform.SetParent(transform, false);
                ring.AddComponent<MeshFilter>().sharedMesh = LowPolyMesh.Ring(0.5f, 0.05f, 24, 4);
                MeshRenderer renderer = ring.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = LyokoMaterials.Glow(Color.white);
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rings[i] = ring.transform;
                ringRenderers[i] = renderer;
            }

            Apply(0f);
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / Duration);
            Apply(t);
            if (t >= 1f)
            {
                for (int i = 0; i < rings.Length; i++)
                {
                    Destroy(rings[i].gameObject);
                }

                Destroy(this);
            }
        }

        private void Apply(float t)
        {
            if (model != null)
            {
                float grow = Mathf.SmoothStep(0f, 1f, t);
                model.localScale = new Vector3(modelScale.x, modelScale.y * Mathf.Max(0.02f, grow), modelScale.z);
            }

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            for (int i = 0; i < rings.Length; i++)
            {
                float local = Mathf.Clamp01(t * 1.4f - i * 0.15f);
                rings[i].localPosition = Vector3.up * Mathf.Lerp(2.4f, 0.05f, local);
                rings[i].localScale = Vector3.one * Mathf.Lerp(1.6f, 1.2f, local);
                Color faded = color * 2f;
                faded.a = Mathf.Sin(local * Mathf.PI);
                block.SetColor("_Color", faded);
                ringRenderers[i].SetPropertyBlock(block);
            }
        }

        private void OnDestroy()
        {
            if (model != null)
            {
                model.localScale = modelScale;
            }
        }
    }
}
