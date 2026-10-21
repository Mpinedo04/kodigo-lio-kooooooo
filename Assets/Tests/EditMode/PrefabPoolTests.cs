using NUnit.Framework;
using UnityEngine;

namespace CodeLyokoFanGame.Tests
{
    public sealed class PrefabPoolTests
    {
        private GameObject prefab;
        private GameObject spawned;

        [SetUp]
        public void SetUp()
        {
            PrefabPool.Clear();
            prefab = new GameObject("PoolTestPrefab");
        }

        [TearDown]
        public void TearDown()
        {
            PrefabPool.Clear();
            if (spawned != null)
            {
                Object.DestroyImmediate(spawned);
            }

            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void Despawn_ThenSpawn_ReusesTheSameInstance()
        {
            spawned = PrefabPool.Spawn(prefab, Vector3.zero, Quaternion.identity);
            PrefabPool.Despawn(spawned);

            Assert.IsFalse(spawned.activeSelf);
            Assert.AreEqual(1, PrefabPool.CountInactive(prefab));

            GameObject reused = PrefabPool.Spawn(prefab, Vector3.one, Quaternion.identity);

            Assert.AreSame(spawned, reused);
            Assert.IsTrue(reused.activeSelf);
            Assert.AreEqual(Vector3.one, reused.transform.position);
            Assert.AreEqual(0, PrefabPool.CountInactive(prefab));
        }

        [Test]
        public void DespawnTwice_DoesNotDuplicateTheInstance()
        {
            spawned = PrefabPool.Spawn(prefab, Vector3.zero, Quaternion.identity);
            PrefabPool.Despawn(spawned);
            PrefabPool.Despawn(spawned);

            Assert.AreEqual(1, PrefabPool.CountInactive(prefab));
        }
    }
}
