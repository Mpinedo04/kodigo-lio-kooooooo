using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class GameHud : MonoBehaviour
    {
        [SerializeField] private CharacterSwitcher switcher;
        [SerializeField] private MissionRunner missionRunner;
        [SerializeField] private TowerObjective tower;
        [SerializeField] private AelitaCompanion aelita;

        private GUIStyle panelStyle;
        private GUIStyle titleStyle;

        public void Configure(CharacterSwitcher newSwitcher, MissionRunner newRunner, TowerObjective newTower, AelitaCompanion newAelita)
        {
            switcher = newSwitcher;
            missionRunner = newRunner;
            tower = newTower;
            aelita = newAelita;
        }

        private void OnGUI()
        {
            EnsureStyles();
            DrawMissionPanel();
            DrawStatusPanel();
        }

        private void DrawMissionPanel()
        {
            GUILayout.BeginArea(new Rect(18, 18, 560, 110), panelStyle);
            string title = missionRunner != null && missionRunner.Mission != null ? missionRunner.Mission.displayName : "CODE LYOKO";
            GUILayout.Label(title, titleStyle);
            GUILayout.Label("1-3 / cruceta: personaje  |  V: vehiculo  |  Tab / R3: fijar  |  E / RT: torre");

            if (missionRunner != null)
            {
                GUILayout.Label("Objetivo: " + missionRunner.CurrentObjectiveText);
            }

            GUILayout.EndArea();
        }

        private void DrawStatusPanel()
        {
            GUILayout.BeginArea(new Rect(18, Screen.height - 196, 560, 178), panelStyle);
            GUILayout.Label("Lyoko HUD", titleStyle);

            PlayerController player = switcher != null ? switcher.ActiveCharacter : null;
            if (player != null)
            {
                Health health = player.GetComponent<Health>();
                GUILayout.Label("Personaje: " + player.Character + " | Vida: " + FormatHealth(health) + " | Vehiculo: " + (player.IsMounted ? "Activo" : "No"));
                DrawCombatLine(player);
            }

            if (aelita != null)
            {
                Health aelitaHealth = aelita.GetComponent<Health>();
                GUILayout.Label("Aelita: " + FormatHealth(aelitaHealth));
            }

            if (missionRunner != null && missionRunner.ShowsEnemyCount)
            {
                GUILayout.Label("Enemigos restantes: " + missionRunner.RemainingEnemies);
            }

            if (tower != null)
            {
                GUILayout.Label("Torre: " + Mathf.RoundToInt(tower.Progress01 * 100f) + "%");
            }

            GUILayout.EndArea();
        }

        private static void DrawCombatLine(PlayerController player)
        {
            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            if (combat == null || combat.Meter == null)
            {
                return;
            }

            string line = "Energia especial: " + Mathf.RoundToInt(combat.Meter.Normalized * 100f) + "%" + (combat.Meter.IsFull ? " (LLENA)" : string.Empty);
            if (combat.IsCharging)
            {
                line += "  |  Carga: " + Mathf.RoundToInt(combat.ChargeProgress * 100f) + "%";
            }

            PlayerGuard guard = player.GetComponent<PlayerGuard>();
            if (guard != null && Time.time - guard.LastParryTime < 0.6f)
            {
                line += "  |  DESVIO!";
            }

            GUILayout.Label(line);
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
