using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeLyokoFanGame
{
    // Loads levels additively on top of the Boot scene and shows a simple loading screen.
    public sealed class SceneLoader : MonoBehaviour
    {
        public const string BootSceneName = "Boot";

        [SerializeField] private string firstLevel = "ForestSectorVerticalSlice";
        [SerializeField] private float minimumLoadingSeconds = 0.4f;

        private string currentLevel;
        private GUIStyle overlayStyle;

        public static SceneLoader Instance { get; private set; }
        public bool IsLoading { get; private set; }
        public string CurrentLevel => currentLevel;

        public void LoadLevel(string sceneName)
        {
            if (!IsLoading && !string.IsNullOrEmpty(sceneName))
            {
                StartCoroutine(LoadRoutine(sceneName));
            }
        }

        public void ReloadCurrentLevel()
        {
            LoadLevel(currentLevel);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            // When Play starts from a level in the editor, that level is already loaded.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene.name != BootSceneName && scene.name != "DontDestroyOnLoad")
                {
                    currentLevel = scene.name;
                    return;
                }
            }

            LoadLevel(firstLevel);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            IsLoading = true;
            float startTime = Time.unscaledTime;

            if (!string.IsNullOrEmpty(currentLevel))
            {
                Scene loaded = SceneManager.GetSceneByName(currentLevel);
                if (loaded.isLoaded)
                {
                    yield return SceneManager.UnloadSceneAsync(loaded);
                }
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (operation == null)
            {
                Debug.LogError("[SceneLoader] Scene not in Build Settings: " + sceneName);
                IsLoading = false;
                yield break;
            }

            while (!operation.isDone)
            {
                yield return null;
            }

            currentLevel = sceneName;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            while (Time.unscaledTime - startTime < minimumLoadingSeconds)
            {
                yield return null;
            }

            IsLoading = false;
        }

        private void OnGUI()
        {
            if (!IsLoading)
            {
                return;
            }

            if (overlayStyle == null)
            {
                Texture2D texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, new Color(0.01f, 0.05f, 0.08f, 1f));
                texture.Apply();
                overlayStyle = new GUIStyle
                {
                    normal = { background = texture, textColor = new Color(0.2f, 0.95f, 1f) },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 28,
                    fontStyle = FontStyle.Bold
                };
            }

            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "Virtualizando...", overlayStyle);
        }
    }
}
