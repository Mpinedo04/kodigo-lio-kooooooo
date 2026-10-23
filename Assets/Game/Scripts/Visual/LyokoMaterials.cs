using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Toon and glow materials shared by colour (F9.1). In Play mode they live in memory; when the generators
    // run in the Editor, Persist (set by ArtAssetPersistence) saves them as assets so scenes keep them.
    public static class LyokoMaterials
    {
        public const string ToonShader = "CodeLyoko/LyokoToon";
        public const string GlowShader = "CodeLyoko/LyokoGlow";
        public const string OutlinePass = "SRPDefaultUnlit";

        private static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();

        public static Func<string, Material, Material> Persist { get; set; }

        public static readonly Color OutlineColor = new Color(0.06f, 0.05f, 0.1f);

        public static Material Toon(Color color, float emission = 0f, float outline = 0f)
        {
            string key = "Toon_" + ColorUtility.ToHtmlStringRGB(color) + "_E" + Mathf.RoundToInt(emission * 10f) + "_O" + Mathf.RoundToInt(outline * 10f);
            if (Cache.TryGetValue(key, out Material cached) && cached != null)
            {
                return cached;
            }

            Shader shader = Shader.Find(ToonShader) ?? Shader.Find("Universal Render Pipeline/Lit");
            Material material = new Material(shader) { name = key };
            ConfigureToon(material, color, emission, outline);
            return Store(key, material);
        }

        public static Material Glow(Color color, float round = 0f, float edgeFade = 0f)
        {
            string key = "Glow_" + ColorUtility.ToHtmlStringRGBA(color) + "_R" + Mathf.RoundToInt(round * 10f) + "_F" + Mathf.RoundToInt(edgeFade * 10f);
            if (Cache.TryGetValue(key, out Material cached) && cached != null)
            {
                return cached;
            }

            Shader shader = Shader.Find(GlowShader) ?? Shader.Find("Universal Render Pipeline/Particles/Unlit");
            Material material = new Material(shader) { name = key };
            material.SetColor("_Color", color);
            material.SetFloat("_Round", round);
            material.SetFloat("_EdgeFade", edgeFade);
            return Store(key, material);
        }

        // Also used by the generators for named material assets.
        public static void ConfigureToon(Material material, Color color, float emission, float outline)
        {
            material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            Color glow = emission > 0f ? color * emission : Color.black;
            material.SetColor("_EmissionColor", glow);
            if (emission > 0f)
            {
                material.EnableKeyword("_EMISSION");
            }

            if (material.HasProperty("_OutlineWidth"))
            {
                material.SetFloat("_OutlineWidth", outline);
                material.SetColor("_OutlineColor", OutlineColor);
            }

            material.SetShaderPassEnabled(OutlinePass, outline > 0f);
        }

        public static void Paint(Renderer renderer, Color color, float emission = 0f, float outline = 0f)
        {
            if (renderer != null)
            {
                renderer.sharedMaterial = Toon(color, emission, outline);
            }
        }

        private static Material Store(string key, Material material)
        {
            if (!Application.isPlaying && Persist != null)
            {
                material = Persist(key, material);
            }

            Cache[key] = material;
            return material;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            Cache.Clear();
        }
    }
}
