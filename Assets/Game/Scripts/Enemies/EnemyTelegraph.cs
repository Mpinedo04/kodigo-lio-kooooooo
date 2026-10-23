using UnityEngine;

namespace CodeLyokoFanGame
{
    // Warning before an attack: the eye of X.A.N.A. glows and, for heavy attacks, an energy orb
    // charges at the fire point. Placeholder for the real VFX of F09.
    public sealed class EnemyTelegraph
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
        private static readonly Color Calm = new Color(1f, 0.08f, 0.05f);
        private static readonly Color Hot = new Color(1f, 0.95f, 0.8f);

        private readonly EnemyAI host;
        private readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        private Renderer eyeRenderer;
        private GameObject orb;
        private float duration;
        private float timer;
        private bool heavy;

        public EnemyTelegraph(EnemyAI host)
        {
            this.host = host;
        }

        public bool IsActive { get; private set; }
        public float Progress => duration > 0f ? Mathf.Clamp01(timer / duration) : 1f;

        public void Bind(Renderer eye)
        {
            End();
            eyeRenderer = eye;
        }

        public void Begin(float seconds, bool heavyAttack)
        {
            End();
            IsActive = true;
            duration = Mathf.Max(0.05f, seconds);
            timer = 0f;
            heavy = heavyAttack;

            if (heavy && Application.isPlaying && host.FirePoint != null)
            {
                orb = EnemyVisuals.Shape(PrimitiveType.Sphere, "TelegraphOrb", host.FirePoint, Vector3.zero, Vector3.zero, EnemyVisuals.XanaRed);
            }
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive)
            {
                return;
            }

            timer += deltaTime;
            float t = Progress;
            float glow = heavy ? t : 0.5f + 0.5f * Mathf.Sin(timer * 28f);
            Color color = Color.Lerp(Calm, Hot, glow);
            ApplyEyeColor(color, 1f + 4f * glow);

            if (orb != null)
            {
                orb.transform.localScale = Vector3.one * Mathf.Lerp(0.1f, 0.9f, t) * (1f + 0.1f * Mathf.Sin(timer * 40f));
            }
        }

        public void End()
        {
            IsActive = false;
            if (eyeRenderer != null)
            {
                eyeRenderer.SetPropertyBlock(null);
            }

            if (orb != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(orb);
                }
                else
                {
                    Object.DestroyImmediate(orb);
                }

                orb = null;
            }
        }

        private void ApplyEyeColor(Color color, float intensity)
        {
            if (eyeRenderer == null)
            {
                return;
            }

            eyeRenderer.GetPropertyBlock(block);
            block.SetColor(BaseColorId, color);
            block.SetColor(ColorId, color);
            block.SetColor(EmissionId, color * intensity);
            eyeRenderer.SetPropertyBlock(block);
        }
    }
}
