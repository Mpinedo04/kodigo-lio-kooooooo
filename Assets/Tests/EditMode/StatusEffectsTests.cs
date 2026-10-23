using NUnit.Framework;
using UnityEngine;

namespace CodeLyokoFanGame.Tests
{
    public sealed class StatusEffectsTests
    {
        private GameObject owner;
        private StatusEffects effects;

        [SetUp]
        public void SetUp()
        {
            owner = new GameObject("StatusEffectsOwner");
            effects = StatusEffects.For(owner);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(owner);
        }

        [Test]
        public void For_AddsTheComponentOnlyOnce()
        {
            Assert.AreSame(effects, StatusEffects.For(owner));
            Assert.AreEqual(1, owner.GetComponents<StatusEffects>().Length);
        }

        [Test]
        public void NoEffects_FullSpeedAndFree()
        {
            Assert.AreEqual(1f, effects.SpeedMultiplier);
            Assert.IsFalse(effects.IsRooted);
            Assert.IsFalse(effects.IsSlowed);
        }

        [Test]
        public void Slow_ReducesSpeedUntilItExpires()
        {
            effects.ApplySlow(0.5f, 2f);

            Assert.IsTrue(effects.IsSlowed);
            Assert.AreEqual(0.5f, effects.SpeedMultiplier, 0.0001f);

            effects.Tick(1.5f);
            Assert.AreEqual(0.5f, effects.SpeedMultiplier, 0.0001f);

            effects.Tick(0.6f);
            Assert.IsFalse(effects.IsSlowed);
            Assert.AreEqual(1f, effects.SpeedMultiplier);
        }

        [Test]
        public void Slow_StrongestWinsAndLongestTimeIsKept()
        {
            effects.ApplySlow(0.6f, 3f);
            effects.ApplySlow(0.3f, 1f);

            Assert.AreEqual(0.3f, effects.SpeedMultiplier, 0.0001f);
            Assert.AreEqual(3f, effects.SlowRemaining, 0.0001f);
        }

        [Test]
        public void Root_HoldsInPlaceUntilItExpires()
        {
            effects.ApplyRoot(1.5f);
            Assert.IsTrue(effects.IsRooted);

            effects.Tick(1f);
            Assert.IsTrue(effects.IsRooted);

            effects.Tick(0.6f);
            Assert.IsFalse(effects.IsRooted);
        }

        [Test]
        public void Clear_RemovesEverything()
        {
            effects.ApplySlow(0.4f, 5f);
            effects.ApplyRoot(5f);

            effects.Clear();

            Assert.IsFalse(effects.IsRooted);
            Assert.AreEqual(1f, effects.SpeedMultiplier);
        }
    }
}
