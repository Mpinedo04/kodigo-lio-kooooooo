using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace CodeLyokoFanGame.EditorTools
{
    // Single place that decides the Build Settings scene order. Boot must stay first.
    public static class ProjectScenes
    {
        public const string BootScenePath = "Assets/Scenes/Boot.unity";
        public const string ForestScenePath = "Assets/Levels/ForestSectorVerticalSlice.unity";

        private static readonly string[] Ordered = { BootScenePath, ForestScenePath };

        public static void RefreshBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            for (int i = 0; i < Ordered.Length; i++)
            {
                if (File.Exists(Ordered[i]))
                {
                    scenes.Add(new EditorBuildSettingsScene(Ordered[i], true));
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
