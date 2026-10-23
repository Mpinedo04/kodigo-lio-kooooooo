using System.IO;
using CodeLyokoFanGame.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CodeLyokoFanGame.EditorTools
{
    // F09 9A: the look of Lyoko. Colour guide per sector (data), white hazy sky, fog, ambient light and
    // post-processing (bloom, colour grading), applied to the scene being generated.
    public static class LookBuilder
    {
        private const string ArtFolder = "Assets/Game/Generated/Art";
        private const string SectorFolder = "Assets/Game/Data/Sectors";

        // Colour guide (F9.5): Forest green, Desert orange, Ice light blue, Mountain brown/purple, Sector 5 blue/white.
        public static void ApplySectorLooks(params SectorDefinition[] sectors)
        {
            for (int i = 0; i < sectors.Length; i++)
            {
                SectorDefinition s = sectors[i];
                switch (s.sectorId)
                {
                    case SectorId.Forest:
                        Look(s, new Color(0.84f, 0.95f, 0.96f), new Color(0.95f, 0.99f, 0.98f), new Color(0.35f, 0.72f, 0.8f), new Color(0.86f, 0.96f, 0.95f), 0.006f,
                            new Color(0.2f, 0.66f, 0.32f), new Color(0.5f, 0.38f, 0.26f), new Color(0.1f, 0.9f, 1f), new Color(0.6f, 0.7f, 0.68f));
                        break;
                    case SectorId.Desert:
                        Look(s, new Color(0.98f, 0.92f, 0.84f), new Color(1f, 0.97f, 0.9f), new Color(0.95f, 0.68f, 0.42f), new Color(0.98f, 0.9f, 0.8f), 0.005f,
                            new Color(0.92f, 0.56f, 0.22f), new Color(0.72f, 0.38f, 0.18f), new Color(1f, 0.82f, 0.4f), new Color(0.75f, 0.66f, 0.56f));
                        break;
                    case SectorId.Ice:
                        Look(s, new Color(0.88f, 0.95f, 1f), new Color(0.98f, 1f, 1f), new Color(0.7f, 0.86f, 0.96f), new Color(0.9f, 0.96f, 1f), 0.007f,
                            new Color(0.76f, 0.9f, 1f), new Color(0.55f, 0.72f, 0.88f), new Color(0.9f, 1f, 1f), new Color(0.7f, 0.78f, 0.86f));
                        break;
                    case SectorId.Mountain:
                        Look(s, new Color(0.9f, 0.88f, 0.94f), new Color(0.98f, 0.96f, 0.98f), new Color(0.6f, 0.5f, 0.62f), new Color(0.9f, 0.86f, 0.92f), 0.007f,
                            new Color(0.55f, 0.4f, 0.42f), new Color(0.48f, 0.34f, 0.5f), new Color(0.75f, 0.5f, 0.85f), new Color(0.66f, 0.6f, 0.66f));
                        break;
                    case SectorId.Sector5:
                        Look(s, new Color(0.82f, 0.88f, 1f), new Color(0.95f, 0.97f, 1f), new Color(0.3f, 0.45f, 0.88f), new Color(0.82f, 0.88f, 1f), 0.006f,
                            new Color(0.62f, 0.76f, 1f), new Color(0.9f, 0.93f, 1f), new Color(0.6f, 0.85f, 1f), new Color(0.62f, 0.68f, 0.82f));
                        break;
                    case SectorId.DigitalSea:
                        Look(s, new Color(0.58f, 0.84f, 0.95f), new Color(0.75f, 0.95f, 1f), new Color(0.04f, 0.3f, 0.5f), new Color(0.45f, 0.78f, 0.92f), 0.012f,
                            new Color(0.1f, 0.5f, 0.8f), new Color(0.08f, 0.3f, 0.5f), new Color(0.1f, 0.95f, 1f), new Color(0.4f, 0.6f, 0.7f));
                        break;
                }

                EditorUtility.SetDirty(s);
            }
        }

        private static void Look(SectorDefinition s, Color top, Color horizon, Color bottom, Color fog, float fogDensity, Color ground, Color terrain, Color accent, Color ambient)
        {
            s.skyTop = top;
            s.skyHorizon = horizon;
            s.skyBottom = bottom;
            s.fogColor = fog;
            s.fogDensity = fogDensity;
            s.groundColor = ground;
            s.terrainColor = terrain;
            s.accentColor = accent;
            s.ambientColor = ambient;
        }

        public static SectorDefinition LoadSector(SectorId id)
        {
            return AssetDatabase.LoadAssetAtPath<SectorDefinition>(SectorFolder + "/" + id + ".asset");
        }

        // Sky, fog, ambient light and a global post-processing volume for the open scene.
        public static void ApplyToScene(SectorId id)
        {
            SectorDefinition sector = LoadSector(id);
            if (sector == null)
            {
                sector = ScriptableObject.CreateInstance<SectorDefinition>();
                sector.sectorId = id;
            }

            RenderSettings.skybox = SkyMaterial(sector);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Color.Lerp(sector.ambientColor, sector.skyTop, 0.35f);
            RenderSettings.ambientEquatorColor = sector.ambientColor;
            RenderSettings.ambientGroundColor = Color.Lerp(sector.ambientColor, sector.skyBottom, 0.5f) * 0.8f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = sector.fogColor;
            RenderSettings.fogDensity = sector.fogDensity;

            GameObject volumeObject = new GameObject("LyokoPostProcess");
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 0f;
            volume.sharedProfile = PostProfile(sector);
        }

        // Shaders only created from code (effects, runtime monsters) must ship with the build.
        public static void EnsureShadersIncluded()
        {
            Object graphicsSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0];
            SerializedObject serialized = new SerializedObject(graphicsSettings);
            SerializedProperty list = serialized.FindProperty("m_AlwaysIncludedShaders");
            foreach (string name in new[] { LyokoMaterials.ToonShader, LyokoMaterials.GlowShader })
            {
                Shader shader = Shader.Find(name);
                if (shader == null || Contains(list, shader))
                {
                    continue;
                }

                list.InsertArrayElementAtIndex(list.arraySize);
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = shader;
            }

            serialized.ApplyModifiedProperties();
        }

        private static bool Contains(SerializedProperty list, Shader shader)
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                {
                    return true;
                }
            }

            return false;
        }

        private static Material SkyMaterial(SectorDefinition sector)
        {
            Directory.CreateDirectory(ArtFolder);
            string path = ArtFolder + "/Sky_" + sector.sectorId + ".mat";
            Shader shader = Shader.Find("CodeLyoko/LyokoSky");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.SetColor("_TopColor", sector.skyTop);
            material.SetColor("_HorizonColor", sector.skyHorizon);
            material.SetColor("_BottomColor", sector.skyBottom);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static VolumeProfile PostProfile(SectorDefinition sector)
        {
            Directory.CreateDirectory(ArtFolder);
            string path = ArtFolder + "/Post_" + sector.sectorId + ".asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            Bloom bloom = Get<Bloom>(profile);
            bloom.threshold.Override(0.95f);
            bloom.intensity.Override(sector.bloomIntensity);
            bloom.scatter.Override(0.72f);

            Tonemapping tonemapping = Get<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.Neutral);

            ColorAdjustments color = Get<ColorAdjustments>(profile);
            color.saturation.Override(sector.saturation);
            color.contrast.Override(8f);
            color.colorFilter.Override(sector.colorFilter);

            Vignette vignette = Get<Vignette>(profile);
            vignette.intensity.Override(0.18f);
            vignette.smoothness.Override(0.5f);

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static T Get<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet(out T component))
            {
                return component;
            }

            component = profile.Add<T>(true);
            component.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }
    }
}
