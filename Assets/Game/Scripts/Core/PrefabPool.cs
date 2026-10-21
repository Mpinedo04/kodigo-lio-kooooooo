using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeLyokoFanGame
{
    // Reuses instances of frequently spawned prefabs (projectiles, effects) instead of
    // instantiating and destroying them every time.
    public static class PrefabPool
    {
        private static readonly Dictionary<GameObject, Stack<GameObject>> Pools = new Dictionary<GameObject, Stack<GameObject>>();

        static PrefabPool()
        {
            SceneManager.sceneUnloaded += _ => Prune();
        }

        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            GameObject instance = Spawn(prefab.gameObject, position, rotation);
            return instance.GetComponent<T>();
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (Pools.TryGetValue(prefab, out Stack<GameObject> stack))
            {
                while (stack.Count > 0)
                {
                    GameObject pooled = stack.Pop();
                    if (pooled != null)
                    {
                        pooled.transform.SetPositionAndRotation(position, rotation);
                        pooled.SetActive(true);
                        return pooled;
                    }
                }
            }

            GameObject created = Object.Instantiate(prefab, position, rotation);
            PooledObject marker = created.GetComponent<PooledObject>();
            if (marker == null)
            {
                marker = created.AddComponent<PooledObject>();
            }

            marker.Prefab = prefab;
            return created;
        }

        public static void Despawn(GameObject instance)
        {
            if (instance == null || !instance.activeSelf)
            {
                return;
            }

            PooledObject marker = instance.GetComponent<PooledObject>();
            if (marker == null || marker.Prefab == null)
            {
                Object.Destroy(instance);
                return;
            }

            instance.SetActive(false);

            if (!Pools.TryGetValue(marker.Prefab, out Stack<GameObject> stack))
            {
                stack = new Stack<GameObject>();
                Pools.Add(marker.Prefab, stack);
            }

            stack.Push(instance);
        }

        public static int CountInactive(GameObject prefab)
        {
            return Pools.TryGetValue(prefab, out Stack<GameObject> stack) ? stack.Count : 0;
        }

        public static void Clear()
        {
            Pools.Clear();
        }

        // Instances that lived in an unloaded scene are gone; forget them.
        private static void Prune()
        {
            List<GameObject> emptyKeys = null;
            foreach (KeyValuePair<GameObject, Stack<GameObject>> pair in Pools)
            {
                Stack<GameObject> alive = new Stack<GameObject>();
                foreach (GameObject instance in pair.Value)
                {
                    if (instance != null)
                    {
                        alive.Push(instance);
                    }
                }

                pair.Value.Clear();
                foreach (GameObject instance in alive)
                {
                    pair.Value.Push(instance);
                }

                if (pair.Key == null || pair.Value.Count == 0)
                {
                    emptyKeys ??= new List<GameObject>();
                    emptyKeys.Add(pair.Key);
                }
            }

            if (emptyKeys == null)
            {
                return;
            }

            for (int i = 0; i < emptyKeys.Count; i++)
            {
                Pools.Remove(emptyKeys[i]);
            }
        }
    }
}
