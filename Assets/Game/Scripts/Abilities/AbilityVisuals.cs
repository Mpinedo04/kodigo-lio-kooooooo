using UnityEngine;

namespace CodeLyokoFanGame
{
    // Shapes for abilities (shield, wings, fans): faceted meshes with the toon shader (F09).
    public static class AbilityVisuals
    {
        public static readonly Color OddViolet = new Color(0.62f, 0.3f, 1f);
        public static readonly Color UlrichGreen = new Color(0.2f, 0.75f, 0.3f);
        public static readonly Color YumiRed = new Color(0.9f, 0.15f, 0.25f);
        public static readonly Color AelitaPink = new Color(1f, 0.45f, 0.8f);
        public static readonly Color TerrainBrown = new Color(0.45f, 0.36f, 0.25f);

        public static GameObject Shape(PrimitiveType type, string name, Color color, Transform parent, Vector3 localPosition, Vector3 localScale, bool keepCollider = false)
        {
            GameObject shape = GameObject.CreatePrimitive(type);
            shape.name = name;
            LowPolyMesh.Facet(shape, type);
            if (!keepCollider)
            {
                Object.Destroy(shape.GetComponent<Collider>());
            }

            if (parent != null)
            {
                shape.transform.SetParent(parent, false);
            }

            shape.transform.localPosition = localPosition;
            shape.transform.localScale = localScale;
            LyokoMaterials.Paint(shape.GetComponent<Renderer>(), color, 0f, 1f);
            return shape;
        }
    }
}
