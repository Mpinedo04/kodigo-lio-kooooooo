using UnityEngine;

namespace CodeLyokoFanGame
{
    // Placeholder geometry for monsters and their effects (Unity primitives only).
    public static class EnemyVisuals
    {
        public static readonly Color XanaRed = new Color(1f, 0.08f, 0.05f);
        public static readonly Color XanaOrange = new Color(1f, 0.38f, 0.03f);
        public static readonly Color DarkMetal = new Color(0.1f, 0.12f, 0.13f);
        public static readonly Color DataCyan = new Color(0.1f, 0.9f, 1f);
        public static readonly Color IceBlue = new Color(0.55f, 0.85f, 1f);
        public static readonly Color PrisonViolet = new Color(0.7f, 0.25f, 1f);

        // Primitive without collider. Colour is applied only in Play mode (edit-mode builders pass materials).
        public static GameObject Shape(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject shape = GameObject.CreatePrimitive(type);
            shape.name = name;
            LowPolyMesh.Facet(shape, type);
            RemoveCollider(shape);
            if (parent != null)
            {
                shape.transform.SetParent(parent, false);
            }

            shape.transform.localPosition = localPosition;
            shape.transform.localScale = localScale;
            Tint(shape, color);
            return shape;
        }

        public static void Tint(GameObject target, Color color)
        {
            if (!Application.isPlaying || target == null)
            {
                return;
            }

            LyokoMaterials.Paint(target.GetComponent<Renderer>(), color);
        }

        // F09: monsters use the toon shader with an outline; the colour (and glow) comes from the material
        // the generators pass, or from the fallback colour at runtime.
        public static void Paint(GameObject target, Material material, Color fallback, float outline = 0f, bool glow = true)
        {
            Renderer renderer = target != null ? target.GetComponent<Renderer>() : null;
            if (renderer == null)
            {
                return;
            }

            Color color = material != null ? ColorOf(renderer, material, fallback) : fallback;
            float emission = 0f;
            if (glow && material != null && material.HasProperty("_EmissionColor"))
            {
                emission = material.GetColor("_EmissionColor").maxColorComponent / Mathf.Max(0.01f, color.maxColorComponent);
            }

            LyokoMaterials.Paint(renderer, color, emission, outline);
        }

        private static Color ColorOf(Renderer renderer, Material material, Color fallback)
        {
            if (material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor");
            }

            return material.HasProperty("_Color") ? material.color : fallback;
        }

        public static void RemoveCollider(GameObject target)
        {
            Collider collider = target.GetComponent<Collider>();
            if (collider == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                collider.enabled = false;
                Object.Destroy(collider);
            }
            else
            {
                Object.DestroyImmediate(collider);
            }
        }

        public static Color ColorOf(Renderer renderer, Color fallback)
        {
            if (renderer == null || renderer.sharedMaterial == null)
            {
                return fallback;
            }

            Material material = renderer.sharedMaterial;
            if (material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor");
            }

            return material.HasProperty("_Color") ? material.color : fallback;
        }
    }
}
