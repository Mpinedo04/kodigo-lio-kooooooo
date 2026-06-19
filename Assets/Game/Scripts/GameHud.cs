using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class GameHud : MonoBehaviour
    {
        [SerializeField] private CharacterSwitcher switcher;
        [SerializeField] private MissionDirector missionDirector;
        [SerializeField] private TowerObjective tower;
        [SerializeField] private AelitaCompanion aelita;

        private GUIStyle panelStyle;
        private GUIStyle titleStyle;

        public void Configure(CharacterSwitcher newSwitcher, MissionDirector newDirector, TowerObjective newTower, AelitaCompanion newAelita)
        {
            switcher = newSwitcher;
            missionDirector = newDirector;
            tower = newTower;
            aelita = newAelita;
        }

        private void OnGUI()
        {
            EnsureStyles();
            GUILayout.BeginArea(new Rect(18, Screen.height - 150, 560, 132), panelStyle);
            GUILayout.Label("Lyoko HUD", titleStyle);

            PlayerController player = switcher != null ? switcher.ActiveCharacter : null;
            if (player != null)
            {
                Health health = player.GetComponent<Health>();
                GUILayout.Label("Personaje: " + player.Character + " | Vida: " + FormatHealth(health) + " | Vehiculo: " + (player.IsMounted ? "Activo" : "No"));
            }

            if (aelita != null)
            {
                Health aelitaHealth = aelita.GetComponent<Health>();
                GUILayout.Label("Aelita: " + FormatHealth(aelitaHealth));
            }

            if (missionDirector != null)
            {
                GUILayout.Label("Objetivo: " + missionDirector.CurrentObjectiveText);
            }

            if (tower != null)
            {
                GUILayout.Label("Torre: " + Mathf.RoundToInt(tower.Progress01 * 100f) + "%");
            }

            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.68f));
            texture.Apply();

            panelStyle = new GUIStyle
            {
                normal = { background = texture, textColor = Color.white },
                padding = new RectOffset(14, 14, 10, 10),
                fontSize = 15
            };

            titleStyle = new GUIStyle
            {
                normal = { textColor = new Color(0.2f, 0.95f, 1f) },
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
        }

        private static string FormatHealth(Health health)
        {
            if (health == null)
            {
                return "N/A";
            }

            return Mathf.CeilToInt(health.Current) + "/" + Mathf.CeilToInt(health.Max);
        }
    }
}
