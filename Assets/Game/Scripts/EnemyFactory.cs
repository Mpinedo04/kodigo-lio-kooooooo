using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Builds placeholder monsters at runtime (waves, debug spawning) until real prefabs exist.
    public static class EnemyFactory
    {
        // F06 test group: 3 Kankrelats + Blok + Hornet.
        public static readonly EnemyKind[] MixedTestGroup =
        {
            EnemyKind.Kankrelat, EnemyKind.Kankrelat, EnemyKind.Kankrelat, EnemyKind.Blok, EnemyKind.Hornet
        };

        public static EnemyAI CreatePlaceholder(EnemyDefinition definition, EnemyKind fallbackKind, Vector3 position, Projectile projectile)
        {
            return Spawn(definition, fallbackKind, position, projectile, 0f);
        }

        // materializeTime > 0: the monster grows in a column of light, invulnerable and still meanwhile.
        public static EnemyAI Spawn(EnemyDefinition definition, EnemyKind fallbackKind, Vector3 position, Projectile projectile, float materializeTime)
        {
            EnemyKind kind = definition != null ? definition.kind : fallbackKind;
            if (definition == null)
            {
                definition = EnemyCatalog.Find(kind);
            }

            bool flying = definition != null ? definition.flying : kind == EnemyKind.Hornet;
            Vector3 spawnPoint = flying ? position : FindGroundPoint(position, 4f);
            if (flying && definition != null)
            {
                spawnPoint = FindGroundPoint(position, 4f) + Vector3.up * definition.hoverHeight;
            }

            string name = definition != null && !string.IsNullOrEmpty(definition.displayName) ? definition.displayName : kind.ToString();
            EnemyAI enemy = EnemyRigBuilder.Build(name, kind, spawnPoint, null, null);
            if (definition != null)
            {
                enemy.Configure(definition, projectile);
            }
            else
            {
                enemy.Configure(kind, projectile);
            }

            EnemyMaterialize.Begin(enemy, materializeTime);
            return enemy;
        }

        public static List<EnemyAI> SpawnGroup(IList<EnemyKind> kinds, Vector3 center, float radius, Projectile projectile, float materializeTime)
        {
            List<EnemyAI> spawned = new List<EnemyAI>();
            for (int i = 0; i < kinds.Count; i++)
            {
                float angle = i * Mathf.PI * 2f / Mathf.Max(1, kinds.Count);
                Vector3 offset = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;
                spawned.Add(Spawn(null, kinds[i], center + offset, projectile, materializeTime));
            }

            return spawned;
        }

        public static List<EnemyAI> SpawnWave(WaveDefinition wave, Vector3 center, Projectile projectile)
        {
            List<EnemyAI> spawned = new List<EnemyAI>();
            if (wave == null)
            {
                return spawned;
            }

            for (int g = 0; g < wave.groups.Count; g++)
            {
                WaveGroup group = wave.groups[g];
                for (int i = 0; i < group.count; i++)
                {
                    Vector2 circle = Random.insideUnitCircle * group.radius;
                    spawned.Add(Spawn(group.definition, group.fallbackKind, center + new Vector3(circle.x, 0f, circle.y), projectile, wave.materializeTime));
                }
            }

            return spawned;
        }

        // Prefers the NavMesh, then any Environment floor below, then the point itself.
        public static Vector3 FindGroundPoint(Vector3 desired, float searchRadius)
        {
            if (NavMesh.SamplePosition(desired, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }

            if (Physics.Raycast(desired + Vector3.up * 3f, Vector3.down, out RaycastHit ground, 30f, EnemyPerception.EnvironmentMask, QueryTriggerInteraction.Ignore))
            {
                return ground.point;
            }

            return desired;
        }
    }
}
