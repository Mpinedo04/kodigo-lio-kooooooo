using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // EnemyGym debug panel (IMGUI): spawn each monster or groups, kill them all, and see every
    // monster's state above its head. Test scenes only.
    public sealed class EnemySpawnPanel : MonoBehaviour
    {
        [SerializeField] private List<EnemyDefinition> definitions = new List<EnemyDefinition>();
        [SerializeField] private WaveDefinition mixedGroup;
        [SerializeField] private WaveDefinition kankrelatGroup;
        [SerializeField] private Projectile enemyProjectile;
        [SerializeField] private Transform spawnCenter;
        [SerializeField] private bool showStateLabels = true;

        private bool collapsed;
        private bool invulnerableWarriors;
        private GUIStyle labelStyle;

        public void Configure(IEnumerable<EnemyDefinition> monsters, WaveDefinition mixed, WaveDefinition kankrelats, Projectile projectile, Transform center)
        {
            definitions.Clear();
            definitions.AddRange(monsters);
            mixedGroup = mixed;
            kankrelatGroup = kankrelats;
            enemyProjectile = projectile;
            spawnCenter = center;
        }

        private void Awake()
        {
            for (int i = 0; i < definitions.Count; i++)
            {
                EnemyCatalog.Register(definitions[i]);
            }
        }

        private void Update()
        {
            if (invulnerableWarriors)
            {
                SetWarriorsInvulnerable(true);
            }
        }

        private Vector3 Center => spawnCenter != null ? spawnCenter.position : transform.position;

        private void OnGUI()
        {
            if (showStateLabels)
            {
                DrawStateLabels();
            }

            float height = collapsed ? 60f : 420f;
            GUILayout.BeginArea(new Rect(Screen.width - 330f, Screen.height - height - 20f, 310f, height), GUI.skin.box);
            GUILayout.Label("EnemyGym (F06)");
            if (GUILayout.Button(collapsed ? "Mostrar panel" : "Ocultar panel"))
            {
                collapsed = !collapsed;
            }

            if (!collapsed)
            {
                DrawButtons();
            }

            GUILayout.EndArea();
        }

        private void DrawButtons()
        {
            GUILayout.Label("Aparecer monstruo:");
            GUILayout.BeginHorizontal();
            for (int i = 0; i < definitions.Count; i++)
            {
                if (i == 3)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }

                EnemyDefinition definition = definitions[i];
                if (definition != null && GUILayout.Button(definition.displayName))
                {
                    EnemyFactory.Spawn(definition, definition.kind, Center + RandomOffset(2f), enemyProjectile, 0.9f);
                }
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Grupo de Kankrelats"))
            {
                EnemyFactory.SpawnWave(kankrelatGroup, Center, enemyProjectile);
            }

            if (GUILayout.Button("Grupo mixto (3 Kankrelats + Blok + Hornet)"))
            {
                if (mixedGroup != null)
                {
                    EnemyFactory.SpawnWave(mixedGroup, Center, enemyProjectile);
                }
                else
                {
                    EnemyFactory.SpawnGroup(EnemyFactory.MixedTestGroup, Center, 5f, enemyProjectile, 0.9f);
                }
            }

            if (GUILayout.Button("Devirtualizar a todos los monstruos"))
            {
                KillAll();
            }

            bool newInvulnerable = GUILayout.Toggle(invulnerableWarriors, " Guerreros invencibles");
            if (newInvulnerable != invulnerableWarriors)
            {
                invulnerableWarriors = newInvulnerable;
                SetWarriorsInvulnerable(invulnerableWarriors);
            }

            showStateLabels = GUILayout.Toggle(showStateLabels, " Ver estado encima de cada monstruo");

            CombatDirector director = CombatDirector.Current;
            GUILayout.Label("Monstruos: " + EnemyAI.All.Count + "  |  Atacando: " + (director != null ? director.ActiveAttackers : 0));
            GUILayout.Label("Datos recogidos: " + DataPickup.TotalCollected);
        }

        private void DrawStateLabels()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
                labelStyle.normal.textColor = Color.white;
            }

            IReadOnlyList<EnemyAI> all = EnemyAI.All;
            for (int i = 0; i < all.Count; i++)
            {
                EnemyAI enemy = all[i];
                if (enemy == null)
                {
                    continue;
                }

                Vector3 screen = camera.WorldToScreenPoint(enemy.transform.position + Vector3.up * 3.2f);
                if (screen.z <= 0f)
                {
                    continue;
                }

                string text = enemy.Kind + " " + enemy.CurrentState + " " + Mathf.CeilToInt(enemy.Health.Current) + "PV";
                if (enemy.Target != null)
                {
                    text += " > " + enemy.Target.name;
                }

                GUI.Label(new Rect(screen.x - 120f, Screen.height - screen.y - 10f, 240f, 20f), text, labelStyle);
            }
        }

        private static void KillAll()
        {
            List<EnemyAI> enemies = new List<EnemyAI>(EnemyAI.All);
            for (int i = 0; i < enemies.Count; i++)
            {
                Health health = enemies[i] != null ? enemies[i].Health : null;
                if (health != null && !health.IsDead)
                {
                    health.Kill(DamageType.Ability);
                }
            }
        }

        private static void SetWarriorsInvulnerable(bool value)
        {
            PlayerController[] players = FindObjectsOfType<PlayerController>(true);
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].TryGetComponent(out Health health))
                {
                    health.Invulnerable = value;
                }
            }
        }

        private static Vector3 RandomOffset(float radius)
        {
            Vector2 circle = Random.insideUnitCircle * radius;
            return new Vector3(circle.x, 0f, circle.y);
        }
    }
}
