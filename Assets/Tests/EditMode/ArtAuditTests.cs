using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeLyokoFanGame.Tests
{
    // F09 "done" criterion: no Unity primitive (cube, sphere, capsule, cylinder) is left in the Forest mission
    // or in the generated prefabs it uses. Run Rebuild Everything first so the scene is up to date.
    public sealed class ArtAuditTests
    {
        private const string ForestScenePath = "Assets/Levels/ForestSectorVerticalSlice.unity";
        private const string PrefabFolder = "Assets/Game/Generated/Prefabs";
        private const string BuiltinResources = "Library/unity default resources";

        [Test]
        public void ForestScene_HasNoPrimitiveMeshes()
        {
            Scene scene = EditorSceneManager.OpenScene(ForestScenePath, OpenSceneMode.Additive);
            try
            {
                List<string> found = new List<string>();
                GameObject[] roots = scene.GetRootGameObjects();
                for (int i = 0; i < roots.Length; i++)
                {
                    Collect(roots[i], found);
                }

                Assert.IsEmpty(found, "Primitives left in the Forest:\n" + string.Join("\n", found));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void GeneratedPrefabs_HaveNoPrimitiveMeshes()
        {
            List<string> found = new List<string>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder });
            for (int i = 0; i < guids.Length; i++)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (prefab != null)
                {
                    Collect(prefab, found);
                }
            }

            Assert.IsEmpty(found, "Primitives left in the generated prefabs:\n" + string.Join("\n", found));
        }

        private static void Collect(GameObject root, List<string> found)
        {
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < filters.Length; i++)
            {
                Mesh mesh = filters[i].sharedMesh;
                if (mesh != null && AssetDatabase.GetAssetPath(mesh) == BuiltinResources)
                {
                    found.Add(PathOf(filters[i].transform) + " (" + mesh.name + ")");
                }
            }
        }

        private static string PathOf(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }
    }
}
