using UnityEngine;

namespace CodeLyokoFanGame
{
    // "Mission failed" screen (F7.11): retry from the last checkpoint (Emergency Return) or restart the mission.
    // Placeholder IMGUI until the real UI in F10.
    public sealed class MissionFailScreen : MonoBehaviour
    {
        [SerializeField] private MissionRunner runner;

        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle textStyle;

        public void Configure(MissionRunner missionRunner)
        {
            runner = missionRunner;
        }

        private void Awake()
        {
            if (runner == null)
            {
                runner = GetComponent<MissionRunner>();
            }
        }

        private void OnGUI()
        {
            if (runner == null || runner.Phase != MissionPhase.Failed)
            {
                return;
            }

            EnsureStyles();
            Rect area = new Rect(Screen.width * 0.5f - 280f, Screen.height * 0.5f - 130f, 560f, 260f);
            GUILayout.BeginArea(area, panelStyle);
            GUILayout.Label("MISION FALLIDA", titleStyle);
            GUILayout.Label(runner.CurrentObjectiveText, textStyle);
            GUILayout.Space(12f);

            if (GUILayout.Button("Retorno de emergencia: volver al punto de control (R / Y)", GUILayout.Height(38f)))
            {
                runner.RetryFromCheckpoint();
            }

            if (GUILayout.Button("Reiniciar la mision desde el principio", GUILayout.Height(30f)))
            {
                runner.RestartMission();
            }

            GUILayout.Label("El Retorno de emergencia sube el Nivel de X.A.N.A.", textStyle);
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0.05f, 0f, 0f, 0.85f));
            texture.Apply();

            panelStyle = new GUIStyle
            {
                normal = { background = texture },
                padding = new RectOffset(20, 20, 16, 16)
            };

            titleStyle = new GUIStyle
            {
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.2f, 0.15f) }
            };

            textStyle = new GUIStyle
            {
                fontSize = 15,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
        }
    }
}
