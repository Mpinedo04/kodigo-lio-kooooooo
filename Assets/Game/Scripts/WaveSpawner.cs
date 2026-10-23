using System;
using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Spawns waves of monsters (F06: WaveDefinition assets) with a materialization effect.
    // The next wave starts when the current one is down to its threshold.
    public sealed class WaveSpawner : MonoBehaviour
    {
        // Legacy inline entry: each one is a wave of a single group.
        [Serializable]
        public sealed class WaveEntry
        {
            public EnemyDefinition definition;
            public EnemyKind fallbackKind = EnemyKind.Kankrelat;
            public int count = 1;
            public float radius = 5f;
            public float delayBetweenSpawns = 0.25f;
        }

        [SerializeField] private Projectile enemyProjectilePrefab;
        [SerializeField] private List<WaveDefinition> waveAssets = new List<WaveDefinition>();
        [SerializeField] private List<WaveEntry> waves = new List<WaveEntry>();
        [SerializeField] private bool spawnOnStart;
        [SerializeField] private bool spawnWhenPlayerNear = true;
        [SerializeField] private float triggerRadius = 14f;
        [Tooltip("Used by the legacy entries (wave assets have their own).")]
        [SerializeField] private float materializeTime = 0.9f;

        private readonly List<EnemyAI> currentWave = new List<EnemyAI>();
        private int spawnedWaveIndex = -1;
        private int groupIndex;
        private int remainingInGroup;
        private float spawnTimer;
        private float nextWaveTimer = -1f;
        private bool spawning;

        public bool HasSpawnedAll => spawnedWaveIndex >= WaveCount - 1 && !spawning && spawnedWaveIndex >= 0;
        public Projectile EnemyProjectilePrefab => enemyProjectilePrefab;
        public int CurrentWaveIndex => spawnedWaveIndex;
        public int WaveCount => waveAssets.Count > 0 ? waveAssets.Count : waves.Count;
        public int AliveInCurrentWave
        {
            get
            {
                int alive = 0;
                for (int i = 0; i < currentWave.Count; i++)
                {
                    if (currentWave[i] != null && !currentWave[i].IsDead)
                    {
                        alive++;
                    }
                }

                return alive;
            }
        }

        public void AddWave(EnemyKind kind, int count, float radius)
        {
            waves.Add(new WaveEntry { fallbackKind = kind, count = count, radius = radius });
        }

        public void SetWaves(IEnumerable<WaveDefinition> definitions)
        {
            waveAssets.Clear();
            waveAssets.AddRange(definitions);
        }

        public void SetProjectile(Projectile projectile)
        {
            enemyProjectilePrefab = projectile;
        }

        private void Awake()
        {
            for (int i = 0; i < waveAssets.Count; i++)
            {
                if (waveAssets[i] == null)
                {
                    continue;
                }

                for (int g = 0; g < waveAssets[i].groups.Count; g++)
                {
                    EnemyCatalog.Register(waveAssets[i].groups[g].definition);
                }
            }
        }

        private void Start()
        {
            if (spawnOnStart)
            {
                BeginNextWave();
            }
        }

        private void Update()
        {
            if (spawnedWaveIndex < 0)
            {
                if (spawnWhenPlayerNear && IsPlayerNear())
                {
                    BeginNextWave();
                }

                return;
            }

            if (spawning)
            {
                TickSpawning();
                return;
            }

            if (spawnedWaveIndex + 1 >= WaveCount)
            {
                return;
            }

            if (nextWaveTimer < 0f && AliveInCurrentWave <= Threshold(spawnedWaveIndex))
            {
                nextWaveTimer = DelayAfter(spawnedWaveIndex);
            }

            if (nextWaveTimer >= 0f)
            {
                nextWaveTimer -= Time.deltaTime;
                if (nextWaveTimer < 0f)
                {
                    BeginNextWave();
                }
            }
        }

        public void BeginNextWave()
        {
            if (spawnedWaveIndex + 1 >= WaveCount)
            {
                spawnedWaveIndex = WaveCount;
                return;
            }

            spawnedWaveIndex++;
            currentWave.Clear();
            groupIndex = 0;
            remainingInGroup = GroupCount(spawnedWaveIndex, 0);
            spawnTimer = 0f;
            nextWaveTimer = -1f;
            spawning = true;
        }

        private void TickSpawning()
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0f)
            {
                return;
            }

            while (remainingInGroup <= 0)
            {
                groupIndex++;
                if (groupIndex >= GroupTotal(spawnedWaveIndex))
                {
                    spawning = false;
                    return;
                }

                remainingInGroup = GroupCount(spawnedWaveIndex, groupIndex);
            }

            SpawnOne(spawnedWaveIndex, groupIndex);
            remainingInGroup--;
            spawnTimer = GroupDelay(spawnedWaveIndex, groupIndex);
        }

        private void SpawnOne(int wave, int group)
        {
            EnemyDefinition definition;
            EnemyKind kind;
            float radius;
            float materialize = materializeTime;

            if (waveAssets.Count > 0)
            {
                WaveGroup entry = waveAssets[wave].groups[group];
                definition = entry.definition;
                kind = entry.fallbackKind;
                radius = entry.radius;
                materialize = waveAssets[wave].materializeTime;
            }
            else
            {
                WaveEntry entry = waves[wave];
                definition = entry.definition;
                kind = entry.fallbackKind;
                radius = entry.radius;
            }

            Vector2 circle = UnityEngine.Random.insideUnitCircle * radius;
            Vector3 position = transform.position + new Vector3(circle.x, 0f, circle.y);
            EnemyAI enemy = EnemyFactory.Spawn(definition, kind, position, enemyProjectilePrefab, materialize);
            currentWave.Add(enemy);
        }

        private int GroupTotal(int wave)
        {
            if (waveAssets.Count > 0)
            {
                return waveAssets[wave] != null ? waveAssets[wave].groups.Count : 0;
            }

            return 1;
        }

        private int GroupCount(int wave, int group)
        {
            if (group >= GroupTotal(wave))
            {
                return 0;
            }

            return Mathf.Max(0, waveAssets.Count > 0 ? waveAssets[wave].groups[group].count : waves[wave].count);
        }

        private float GroupDelay(int wave, int group)
        {
            return waveAssets.Count > 0 ? waveAssets[wave].groups[group].delayBetweenSpawns : waves[wave].delayBetweenSpawns;
        }

        private int Threshold(int wave)
        {
            return waveAssets.Count > 0 && waveAssets[wave] != null ? waveAssets[wave].nextWaveWhenAliveAtMost : 0;
        }

        private float DelayAfter(int wave)
        {
            return waveAssets.Count > 0 && waveAssets[wave] != null ? waveAssets[wave].delayBeforeNextWave : 1.5f;
        }

        private bool IsPlayerNear()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].activeInHierarchy && Vector3.Distance(players[i].transform.position, transform.position) <= triggerRadius)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
