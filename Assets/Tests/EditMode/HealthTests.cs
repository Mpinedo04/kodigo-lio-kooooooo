using NUnit.Framework;
using UnityEngine;

namespace CodeLyokoFanGame.Tests
{
    public sealed class HealthTests
    {
        private GameObject owner;
        private Health health;

        [SetUp]
        public void SetUp()
        {
            owner = new GameObject("HealthTestOwner");
            health = owner.AddComponent<Health>();
            health.SetDestroyOnDeath(false);
            health.SetMaxHealth(100f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(owner);
        }

        [Test]
        public void TakeDamage_ReducesCurrentHealth()
        {
            health.TakeDamage(30f);

            Assert.AreEqual(70f, health.Current);
            Assert.IsFalse(health.IsDead);
        }

        [Test]
        public void ReachingZero_DevirtualizesAndRaisesDied()
        {
            bool died = false;
            health.Died += _ => died = true;

            health.TakeDamage(150f);

            Assert.AreEqual(0f, health.Current);
            Assert.IsTrue(health.IsDead);
            Assert.IsTrue(died);
            Assert.IsFalse(owner.activeSelf);
        }

        [Test]
        public void Invulnerable_IgnoresDamage()
        {
            health.Invulnerable = true;

            health.TakeDamage(50f);

            Assert.AreEqual(100f, health.Current);
        }
    }
}
