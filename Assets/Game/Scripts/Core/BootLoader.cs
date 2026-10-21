using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeLyokoFanGame
{
    // Makes any level playable on its own: if Play starts without the Boot scene, it is added.
    public static class BootLoader
    {
        private const string BootScenePath = "Assets/Scenes/Boot.unity";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootScene()
        {
            if (SceneManager.GetSceneByName(SceneLoader.BootSceneName).isLoaded)
            {
                return;
            }

            if (SceneUtility.GetBuildIndexByScenePath(BootScenePath) < 0)
            {
                return;
            }

            SceneManager.LoadScene(SceneLoader.BootSceneName, LoadSceneMode.Additive);
        }
    }
}
