using System;
using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    public sealed class WaveSpawner : MonoBehaviour
    {
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
        [SerializeField] private List<WaveEntry> waves = new List<WaveEntry>();
        [SerializeField] private bool spawnOnStart;
        [SerializeField] private bool spawnWhenPlayerNear = true;
        [SerializeField] private float triggerRadius = 14f;

        private int spawnedWaveIndex = -1;
        private float spawnTimer;
        private int remainingInCurrentEntry;
        private WaveEntry currentEntry;

        public bool HasSpawnedAll => spawnedWaveIndex >= waves.Count;

        public void AddWave(EnemyKind kind, int count, float radius)
        {
            waves.Add(new WaveEntry { fallbackKind = kind, count = count, radius = radius });
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
            if (spawnedWaveIndex < 0 && spawnWhenPlayerNear && IsPlayerNear())
            {
                BeginNextWave();
            }

            if (currentEntry == null || remainingInCurrentEntry <= 0)
            {
                return;
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnOne(currentEntry);
                remainingInCurrentEntry--;
                spawnTimer = currentEntry.delayBetweenSpawns;

                if (remainingInCurrentEntry <= 0)
                {
                    currentEntry = null;
                }
            }
        }

        public void BeginNextWave()
        {
            spawnedWaveIndex++;
            if (spawnedWaveIndex >= waves.Count)
            {
                return;
            }

            currentEntry = waves[spawnedWaveIndex];
            remainingInCurrentEntry = Mathf.Max(0, currentEntry.count);
            spawnTimer = 0f;
        }

        private void SpawnOne(WaveEntry entry)
        {
            Vector2 circle = UnityEngine.Random.insideUnitCircle * entry.radius;
            Vector3 position = transform.position + new Vector3(circle.x, 1.4f, circle.y);
            GameObject enemyObject = new GameObject(entry.definition != null ? entry.definition.displayName : entry.fallbackKind.ToString());
            enemyObject.tag = "Enemy";
            enemyObject.transform.position = position;
            enemyObject.AddComponent<Health>();
            EnemyAI enemy = enemyObject.AddComponent<EnemyAI>();

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "RuntimeWavePlaceholder";
            body.transform.SetParent(enemyObject.transform);
            body.transform.localPosition = Vector3.up;
            body.transform.localScale = Vector3.one;
            UnityEngine.Object.Destroy(body.GetComponent<Collider>());

            GameObject hitCollider = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hitCollider.name = "HitCollider";
            hitCollider.tag = "Enemy";
            hitCollider.transform.SetParent(enemyObject.transform);
            hitCollider.transform.localPosition = Vector3.up;
            hitCollider.GetComponent<Renderer>().enabled = false;

            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "XanaEyeWeakPoint";
            eye.tag = "Enemy";
            eye.transform.SetParent(enemyObject.transform);
            eye.transform.localPosition = new Vector3(0f, 1.25f, -0.55f);
            eye.transform.localScale = Vector3.one * 0.32f;
            eye.GetComponent<Collider>().isTrigger = true;
            eye.AddComponent<WeakPoint>();

            if (entry.definition != null)
            {
                enemy.Configure(entry.definition, enemyProjectilePrefab);
            }
            else
            {
                enemy.Configure(entry.fallbackKind, enemyProjectilePrefab);
            }
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
