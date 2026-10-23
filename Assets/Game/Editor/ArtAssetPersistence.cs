using System.IO;
using UnityEditor;
using UnityEngine;

namespace CodeLyokoFanGame.EditorTools
{
    // F09: when the generators build art in the Editor, the meshes and materials made by LowPolyMesh and
    // LyokoMaterials are saved as assets (or update the existing ones), so the scenes and prefabs keep them.
    [InitializeOnLoad]
    public static class ArtAssetPersistence
    {
        private const string MeshFolder = "Assets/Game/Generated/Art/Meshes";
        private const string MaterialFolder = "Assets/Game/Generated/Art/Materials";

        static ArtAssetPersistence()
        {
            LowPolyMesh.Persist = SaveMesh;
            LyokoMaterials.Persist = SaveMaterial;
        }

        private static Mesh SaveMesh(string key, Mesh mesh)
        {
            Directory.CreateDirectory(MeshFolder);
            string path = MeshFolder + "/" + Safe(key) + ".asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }

            EditorUtility.CopySerialized(mesh, existing);
            existing.name = key;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static Material SaveMaterial(string key, Material material)
        {
            Directory.CreateDirectory(MaterialFolder);
            string path = MaterialFolder + "/" + Safe(key) + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(material, path);
                return material;
            }

            existing.shader = material.shader;
            existing.CopyPropertiesFromMaterial(material);
            existing.SetShaderPassEnabled(LyokoMaterials.OutlinePass, material.GetShaderPassEnabled(LyokoMaterials.OutlinePass));
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static string Safe(string key)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                key = key.Replace(invalid, '_');
            }

            return key;
        }
    }
}
