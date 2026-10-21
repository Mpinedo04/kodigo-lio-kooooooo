using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeLyokoFanGame.EditorTools
{
    // Boot scene: always loaded, holds GameManager, SceneLoader and the debug menu.
    public static class BootSceneBuilder
    {
        [MenuItem("Tools/Code Lyoko Fan Game/Build Boot Scene")]
        public static void BuildBootScene()
        {
            InputReader input = CoreAssetsBuilder.EnsureInputReader();
            GameEventSet events = CoreAssetsBuilder.EnsureEventSet();

            Directory.CreateDirectory(Path.GetDirectoryName(ProjectScenes.BootScenePath));
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject root = new GameObject("GameManager");
            GameManager manager = root.AddComponent<GameManager>();
            SetPrivate(manager, "events", events);
            root.AddComponent<SceneLoader>();
            root.AddComponent<DamageNumbers>();
            DebugMenu debugMenu = root.AddComponent<DebugMenu>();
            SetPrivate(debugMenu, "input", input);

            EditorSceneManager.SaveScene(scene, ProjectScenes.BootScenePath);
            ProjectScenes.RefreshBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("Boot scene generated at " + ProjectScenes.BootScenePath);
        }

        private static void SetPrivate<T>(Object target, string fieldName, T value)
        {
            System.Reflection.FieldInfo field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogError("Field not found: " + target.GetType().Name + "." + fieldName);
                return;
            }

            field.SetValue(target, value);
            EditorUtility.SetDirty(target);
        }
    }
}
