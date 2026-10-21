using UnityEngine;

namespace CodeLyokoFanGame
{
    // Developer cheats (F1): god mode, kill enemies, jump to a mission step, spawn monsters, see HP.
    // Disabled in release builds.
    public sealed class DebugMenu : MonoBehaviour
    {
        private static readonly EnemyKind[] SpawnableKinds =
        {
            EnemyKind.Kankrelat, EnemyKind.Blok, EnemyKind.Krab, EnemyKind.Hornet, EnemyKind.Megatank
        };

        [SerializeField] private InputReader input;

        private bool visible;
        private bool invulnerable;
        private Rect window;
        private Vector2 healthScroll;

        private void Awake()
        {
#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
            enabled = false;
#endif
        }

        private void Update()
        {
            if (input != null && input.ToggleDebugPressed)
            {
                visible = !visible;
            }

            if (invulnerable)
            {
                SetAlliesInvulnerable(true);
            }
        }

        private void OnGUI()
        {
            if (!visible)
            {
                return;
            }

            if (window.width <= 0f)
            {
                window = new Rect(Screen.width - 390f, 20f, 370f, 700f);
            }

            window = GUILayout.Window(4242, window, DrawWindow, "Depuracion (F1)");
        }

        private void DrawWindow(int id)
        {
            bool newInvulnerable = GUILayout.Toggle(invulnerable, " Invencible (guerreros y Aelita)");
            if (newInvulnerable != invulnerable)
            {
                invulnerable = newInvulnerable;
                SetAlliesInvulnerable(invulnerable);
            }

            if (GUILayout.Button("Devirtualizar a todos los enemigos"))
            {
                KillAllEnemies();
            }

            if (GUILayout.Button("Llenar energia especial"))
            {
                FillSpecialMeters();
            }

            bool damageNumbers = GUILayout.Toggle(GameSettings.ShowDamageNumbers, " Numeros de dano");
            if (damageNumbers != GameSettings.ShowDamageNumbers)
            {
                GameSettings.ShowDamageNumbers = damageNumbers;
            }

            DrawCameraSettings();
            DrawMissionSteps();
            DrawSpawnButtons();
            DrawHealthList();
            GUI.DragWindow();
        }

        private static void DrawCameraSettings()
        {
            GUILayout.Label("Camara: sensibilidad " + GameSettings.LookSensitivity.ToString("0.00"));
            float sensitivity = GUILayout.HorizontalSlider(GameSettings.LookSensitivity, 0.2f, 3f);
            if (!Mathf.Approximately(sensitivity, GameSettings.LookSensitivity))
            {
                GameSettings.LookSensitivity = sensitivity;
            }

            bool invertY = GUILayout.Toggle(GameSettings.InvertY, " Invertir eje Y");
            if (invertY != GameSettings.InvertY)
            {
                GameSettings.InvertY = invertY;
            }
        }

        private static void DrawMissionSteps()
        {
            MissionRunner runner = FindObjectOfType<MissionRunner>();
            if (runner == null || runner.Mission == null)
            {
                return;
            }

            GUILayout.Label("Saltar a paso de mision (actual: " + runner.CurrentStepIndex + "):");
            for (int i = 0; i < runner.Mission.steps.Count; i++)
            {
                if (GUILayout.Button(i + ": " + runner.Mission.steps[i].phase))
                {
                    runner.JumpToStep(i);
                }
            }
        }

        private void DrawSpawnButtons()
        {
            GUILayout.Label("Aparecer enemigo delante del guerrero:");
            GUILayout.BeginHorizontal();
            for (int i = 0; i < SpawnableKinds.Length; i++)
            {
                if (i == 3)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }

                if (GUILayout.Button(SpawnableKinds[i].ToString()))
                {
                    Spawn(SpawnableKinds[i]);
                }
            }

            GUILayout.EndHorizontal();
        }

        private void DrawHealthList()
        {
            GUILayout.Label("Vida:");
            healthScroll = GUILayout.BeginScrollView(healthScroll, GUILayout.Height(170f));
            Health[] all = FindObjectsOfType<Health>();
            for (int i = 0; i < all.Length; i++)
            {
                GUILayout.Label(all[i].name + ": " + Mathf.CeilToInt(all[i].Current) + "/" + Mathf.CeilToInt(all[i].Max));
            }

            GUILayout.EndScrollView();
        }

        private static void SetAlliesInvulnerable(bool value)
        {
            PlayerController[] players = FindObjectsOfType<PlayerController>(true);
            for (int i = 0; i < players.Length; i++)
            {
                Health health = players[i].GetComponent<Health>();
                if (health != null)
                {
                    health.Invulnerable = value;
                }
            }

            AelitaCompanion aelita = FindObjectOfType<AelitaCompanion>();
            if (aelita != null && aelita.TryGetComponent(out Health aelitaHealth))
            {
                aelitaHealth.Invulnerable = value;
            }
        }

        private static void FillSpecialMeters()
        {
            SpecialMeter[] meters = FindObjectsOfType<SpecialMeter>(true);
            for (int i = 0; i < meters.Length; i++)
            {
                meters[i].Add(meters[i].Max);
            }
        }

        private static void KillAllEnemies()
        {
            EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
            for (int i = 0; i < enemies.Length; i++)
            {
                Health health = enemies[i].GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(health.Current);
                }
            }
        }

        private static void Spawn(EnemyKind kind)
        {
            CharacterSwitcher switcher = FindObjectOfType<CharacterSwitcher>();
            PlayerController player = switcher != null ? switcher.ActiveCharacter : null;
            Vector3 position = player != null
                ? player.transform.position + player.transform.forward * 8f + Vector3.up * 1.4f
                : Vector3.up * 2f;

            EnemyFactory.CreatePlaceholder(null, kind, position, FindEnemyProjectile());
        }

        private static Projectile FindEnemyProjectile()
        {
            WaveSpawner spawner = FindObjectOfType<WaveSpawner>();
            if (spawner != null && spawner.EnemyProjectilePrefab != null)
            {
                return spawner.EnemyProjectilePrefab;
            }

            EnemyAI enemy = FindObjectOfType<EnemyAI>();
            return enemy != null ? enemy.ProjectilePrefab : null;
        }
    }
}
