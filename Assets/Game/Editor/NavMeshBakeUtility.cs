using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace CodeLyokoFanGame.EditorTools
{
    // F06: bakes a NavMesh from the Environment layer of the scene being generated (AI Navigation
    // package) and adds the CombatDirector. Call it before saving the scene.
    public static class NavMeshBakeUtility
    {
        private const string NavMeshFolder = "Assets/Game/Generated/NavMesh";

        public static NavMeshSurface PrepareEnemyScene(string sceneName)
        {
            AddCombatDirector();
            return Bake(sceneName);
        }

        public static NavMeshSurface Bake(string sceneName)
        {
            GameObject surfaceObject = new GameObject("NavMesh_Environment");
            NavMeshSurface surface = surfaceObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = 1 << GameLayers.Index(GameLayers.Environment);
            surface.BuildNavMesh();

            if (surface.navMeshData == null)
            {
                Debug.LogWarning("NavMesh bake produced no data for " + sceneName);
                return surface;
            }

            Directory.CreateDirectory(NavMeshFolder);
            string path = NavMeshFolder + "/" + sceneName + "_NavMesh.asset";
            if (AssetDatabase.LoadAssetAtPath<NavMeshData>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            AssetDatabase.CreateAsset(surface.navMeshData, path);
            EditorUtility.SetDirty(surface);
            return surface;
        }

        public static CombatDirector AddCombatDirector()
        {
            CombatDirector director = new GameObject("CombatDirector").AddComponent<CombatDirector>();
            director.SetProfile(EnemyDataBuilder.EnsureDirectorProfile());
            EditorUtility.SetDirty(director);
            return director;
        }
    }
}
