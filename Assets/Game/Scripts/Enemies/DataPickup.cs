using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // A fragment of Data dropped by devirtualized monsters. Pops out, floats, and flies to the
    // nearest warrior (or companion) that comes close. Progression will spend them later.
    public sealed class DataPickup : MonoBehaviour
    {
        private const float PopTime = 0.5f;
        private const float AttractRadius = 7f;
        private const float CollectRadius = 0.8f;
        private const float Lifetime = 15f;

        private Vector3 velocity;
        private float age;
        private float baseY;
        private float speed;

        public static int TotalCollected { get; private set; }
        public static event Action<int> Collected;

        public static DataPickup Spawn(Vector3 position)
        {
            GameObject cube = EnemyVisuals.Shape(PrimitiveType.Cube, "DataFragment", null, position, Vector3.one * 0.22f, EnemyVisuals.DataCyan);
            cube.transform.rotation = Quaternion.Euler(45f, 0f, 45f);
            cube.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.Gem(0.6f, 0);
            DataPickup pickup = cube.AddComponent<DataPickup>();
            pickup.velocity = new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), UnityEngine.Random.Range(3f, 5f), UnityEngine.Random.Range(-1.5f, 1.5f));
            return pickup;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            age += deltaTime;
            transform.Rotate(0f, 180f * deltaTime, 0f, Space.World);

            if (age < PopTime)
            {
                velocity += Vector3.down * 9f * deltaTime;
                transform.position += velocity * deltaTime;
                baseY = transform.position.y;
                return;
            }

            Health collector = FindCollector();
            if (collector != null)
            {
                speed = Mathf.Min(speed + 30f * deltaTime, 18f);
                Vector3 goal = collector.transform.position + Vector3.up;
                transform.position = Vector3.MoveTowards(transform.position, goal, speed * deltaTime);
                if (Vector3.Distance(transform.position, goal) <= CollectRadius)
                {
                    TotalCollected++;
                    Collected?.Invoke(1);
                    Destroy(gameObject);
                }

                return;
            }

            transform.position = new Vector3(transform.position.x, baseY + Mathf.Sin(age * 3f) * 0.15f, transform.position.z);
            if (age >= Lifetime)
            {
                Destroy(gameObject);
            }
        }

        private Health FindCollector()
        {
            IReadOnlyList<Health> all = Health.All;
            Health best = null;
            float bestDistance = AttractRadius;
            for (int i = 0; i < all.Count; i++)
            {
                Health health = all[i];
                if (health == null || health.Team != Team.Player || health.IsDead)
                {
                    continue;
                }

                float distance = Vector3.Distance(health.transform.position + Vector3.up, transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = health;
                }
            }

            return best;
        }
    }
}
