using System.Collections.Generic;
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
        private GUIStyle centeredStyle;
        private GUIStyle dangerStyle;
        private Texture2D aelitaBar;

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
            DrawAelitaBar();
        }

        private void DrawMissionPanel()
        {
            GUILayout.BeginArea(new Rect(18, 18, 560, 110), panelStyle);
            string title = missionRunner != null && missionRunner.Mission != null ? missionRunner.Mission.displayName : "CODE LYOKO";
            GUILayout.Label(title, titleStyle);
            GUILayout.Label("1-4 / cruceta: personaje  |  Q F X / LB RB: habilidades  |  V: vehiculo  |  Tab: fijar  |  E: torre");

            if (missionRunner != null)
            {
                GUILayout.Label("Objetivo: " + missionRunner.CurrentObjectiveText);
            }

            GUILayout.EndArea();
        }

        private void DrawStatusPanel()
        {
            GUILayout.BeginArea(new Rect(18, Screen.height - 322, 640, 304), panelStyle);
            GUILayout.Label("Lyoko HUD", titleStyle);

            PlayerController player = switcher != null ? switcher.ActiveCharacter : null;
            if (player != null)
            {
                Health health = player.GetComponent<Health>();
                GUILayout.Label("Personaje: " + player.Character + " | Vida: " + FormatHealth(health) + " | Vehiculo: " + (player.IsMounted ? "Activo" : "No") + (player.IsStunned ? "  | ATURDIDO" : string.Empty));
                DrawCombatLine(player);
                DrawVehicleLine(player);
                DrawAbilityLines(player);
                DrawCompanions(player);
            }

            if (Time.unscaledTime - TeamSynergy.LastSynergyTime < 2f)
            {
                GUILayout.Label("SINERGIA: " + TeamSynergy.LastSynergy, titleStyle);
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

        // Placeholder for icons and cooldowns (F5.3): one tag per slot with its state.
        private static void DrawAbilityLines(PlayerController player)
        {
            AbilityController abilities = player.GetComponent<AbilityController>();
            if (abilities == null)
            {
                return;
            }

            GUILayout.Label(SlotText(abilities, AbilitySlot.Ability1, "Q") + "   " + SlotText(abilities, AbilitySlot.Ability2, "F") + "   " + SlotText(abilities, AbilitySlot.Ultimate, "X"));

            List<string> extras = new List<string>();
            if (player.Character == LyokoCharacter.Yumi)
            {
                extras.Add("Concentracion: " + Mathf.RoundToInt(abilities.Concentration.Normalized * 100f) + "%");
            }

            if (player.Character == LyokoCharacter.Aelita)
            {
                extras.Add("Vuelo: " + Mathf.RoundToInt(abilities.FlightEnergy.Normalized * 100f) + "%");
            }

            IReadOnlyList<Ability> all = abilities.Abilities;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].Status != null)
                {
                    extras.Add(all[i].Status);
                }
            }

            if (Time.unscaledTime - abilities.LastMessageTime < 2f)
            {
                extras.Add(abilities.LastMessage);
            }

            if (extras.Count > 0)
            {
                GUILayout.Label(string.Join("  |  ", extras));
            }
        }

        private static string SlotText(AbilityController abilities, AbilitySlot slot, string key)
        {
            Ability ability = abilities.GetSlot(slot);
            if (ability == null)
            {
                return "[" + key + "] -";
            }

            string state;
            if (!abilities.IsUnlocked(ability.Definition))
            {
                state = "bloqueada";
            }
            else if (ability.IsActive)
            {
                state = "ACTIVA";
            }
            else if (ability.CooldownRemaining > 0f)
            {
                state = ability.CooldownRemaining.ToString("0.0") + "s";
            }
            else if (!abilities.CanAfford(ability.Definition))
            {
                state = "sin energia";
            }
            else
            {
                state = "lista";
            }

            return "[" + key + "] " + ability.Definition.displayName + ": " + state;
        }

        private void DrawCompanions(PlayerController player)
        {
            if (switcher == null)
            {
                return;
            }

            List<string> companions = new List<string>();
            IReadOnlyList<PlayerController> characters = switcher.Characters;
            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i] == null || characters[i] == player)
                {
                    continue;
                }

                Health health = characters[i].GetComponent<Health>();
                string state = health != null && health.IsDead ? "devirtualizado" : FormatHealth(health);
                companions.Add((i + 1) + " " + characters[i].Character + " " + state);
            }

            if (companions.Count > 0)
            {
                GUILayout.Label("Companeros (IA): " + string.Join("  |  ", companions));
            }
        }

        // Vehicle health, speed, tricks and passenger (F08); nothing while on foot with the vehicle ready.
        private static void DrawVehicleLine(PlayerController player)
        {
            string status = player.Vehicle != null ? player.Vehicle.HudStatus : null;
            if (!string.IsNullOrEmpty(status))
            {
                GUILayout.Label(status);
            }
        }

        // Aelita's health is always on screen, with a warning when she is in danger (F7.4).
        private void DrawAelitaBar()
        {
            if (aelita == null)
            {
                return;
            }

            float health01 = Mathf.Clamp01(aelita.Health01);
            Rect frame = new Rect(Screen.width - 352f, 18f, 334f, 24f);
            GUI.Box(frame, GUIContent.none, panelStyle);
            GUI.DrawTexture(new Rect(frame.x + 3f, frame.y + 3f, (frame.width - 6f) * health01, frame.height - 6f), aelitaBar);
            string state = aelita.isActiveAndEnabled ? "Orden: " + AelitaOrders.Label(aelita.Order) + " (Z)" : "en la torre";
            GUI.Label(new Rect(frame.x, frame.yMax + 2f, frame.width, 22f), "AELITA " + Mathf.CeilToInt(health01 * 100f) + "%  |  " + state, centeredStyle);
            if (aelita.isActiveAndEnabled && aelita.InDanger && Mathf.Repeat(Time.unscaledTime, 0.6f) < 0.4f)
            {
                GUI.Label(new Rect(frame.x, frame.yMax + 26f, frame.width, 26f), "AELITA EN PELIGRO!", dangerStyle);
            }
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

            centeredStyle = new GUIStyle
            {
                normal = { textColor = Color.white },
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };

            dangerStyle = new GUIStyle(centeredStyle)
            {
                normal = { textColor = new Color(1f, 0.25f, 0.2f) },
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };

            aelitaBar = new Texture2D(1, 1);
            aelitaBar.SetPixel(0, 0, new Color(1f, 0.45f, 0.8f));
            aelitaBar.Apply();
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
