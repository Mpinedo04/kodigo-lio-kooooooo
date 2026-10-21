using NUnit.Framework;
using UnityEngine;

namespace CodeLyokoFanGame.Tests
{
    public sealed class WeakPointTests
    {
        private GameObject enemy;
        private Health health;
        private WeakPoint eye;

        [SetUp]
        public void SetUp()
        {
            enemy = new GameObject("WeakPointTestEnemy");
            health = enemy.AddComponent<Health>();
            health.SetDestroyOnDeath(false);
            health.SetMaxHealth(100f);

            GameObject eyeObject = new GameObject("XanaEye");
            eyeObject.transform.SetParent(enemy.transform);
            eye = eyeObject.AddComponent<WeakPoint>();
            eye.SetTarget(health);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(enemy);
        }

        [Test]
        public void ExposedEye_MultipliesDamage()
        {
            eye.Configure(2.5f, true);

            eye.ApplyDamage(10f, null);

            Assert.AreEqual(75f, health.Current);
        }

        [Test]
        public void HiddenEye_TakesNoDamage()
        {
            eye.Configure(2.5f, false);

            eye.ApplyDamage(10f, null);

            Assert.AreEqual(100f, health.Current);
        }
    }
}
