using System;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Slows (Blok ice) and roots (Blok prison ray) on anything: warriors, Aelita, monsters.
    // Movement code reads SpeedMultiplier and IsRooted; this component only keeps the timers.
    public sealed class StatusEffects : MonoBehaviour
    {
        private static readonly Color SlowColor = new Color(0.55f, 0.85f, 1f);
        private static readonly Color RootColor = new Color(0.7f, 0.25f, 1f);

        private float slowRemaining;
        private float slowMultiplier = 1f;
        private float rootRemaining;
        private GameObject slowVisual;
        private GameObject rootVisual;

        public event Action<StatusEffects> Changed;

        public bool IsSlowed => slowRemaining > 0f;
        public bool IsRooted => rootRemaining > 0f;
        public float SpeedMultiplier => IsSlowed ? slowMultiplier : 1f;
        public float SlowRemaining => slowRemaining;
        public float RootRemaining => rootRemaining;

        public static StatusEffects For(GameObject target)
        {
            if (target == null)
            {
                return null;
            }

            return target.TryGetComponent(out StatusEffects effects) ? effects : target.AddComponent<StatusEffects>();
        }

        // The strongest slow wins; the longest remaining time is kept.
        public void ApplySlow(float speedMultiplier, float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            float clamped = Mathf.Clamp01(speedMultiplier);
            slowMultiplier = IsSlowed ? Mathf.Min(slowMultiplier, clamped) : clamped;
            slowRemaining = Mathf.Max(slowRemaining, duration);
            Changed?.Invoke(this);
        }

        public void ApplyRoot(float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            rootRemaining = Mathf.Max(rootRemaining, duration);
            Changed?.Invoke(this);
        }

        public void Clear()
        {
            slowRemaining = 0f;
            slowMultiplier = 1f;
            rootRemaining = 0f;
            RefreshVisuals();
            Changed?.Invoke(this);
        }

        // Public so tests and paused systems can advance the timers by hand.
        public void Tick(float deltaTime)
        {
            bool wasSlowed = IsSlowed;
            bool wasRooted = IsRooted;
            slowRemaining = Mathf.Max(0f, slowRemaining - deltaTime);
            rootRemaining = Mathf.Max(0f, rootRemaining - deltaTime);

            if (!IsSlowed)
            {
                slowMultiplier = 1f;
            }

            if (wasSlowed != IsSlowed || wasRooted != IsRooted)
            {
                Changed?.Invoke(this);
            }
        }

        private void Update()
        {
            Tick(Time.deltaTime);
            RefreshVisuals();
        }

        private void OnDisable()
        {
            DestroyVisual(ref slowVisual);
            DestroyVisual(ref rootVisual);
        }

        // Feedback: blue disc at the feet while slowed, violet cage while rooted (faceted, toon shader).
        private void RefreshVisuals()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            UpdateVisual(ref slowVisual, IsSlowed, SlowColor, new Vector3(1.6f, 0.03f, 1.6f), 0.05f);
            UpdateVisual(ref rootVisual, IsRooted, RootColor, new Vector3(1.2f, 1.1f, 1.2f), 1.1f);
        }

        private void UpdateVisual(ref GameObject visual, bool active, Color color, Vector3 scale, float height)
        {
            if (!active)
            {
                DestroyVisual(ref visual);
                return;
            }

            if (visual == null)
            {
                visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                visual.name = "StatusEffectVisual";
                Destroy(visual.GetComponent<Collider>());
                LowPolyMesh.Facet(visual, PrimitiveType.Cylinder);
                LyokoMaterials.Paint(visual.GetComponent<Renderer>(), color, 0.6f);
                visual.transform.SetParent(transform, false);
            }

            visual.transform.localPosition = Vector3.up * height;
            visual.transform.localScale = scale * (1f + 0.06f * Mathf.Sin(Time.time * 12f));
        }

        private static void DestroyVisual(ref GameObject visual)
        {
            if (visual != null)
            {
                Destroy(visual);
                visual = null;
            }
        }
    }
}
