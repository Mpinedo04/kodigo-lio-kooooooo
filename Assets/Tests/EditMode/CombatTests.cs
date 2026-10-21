using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame.Tests
{
    public sealed class CombatTests
    {
        private readonly List<GameObject> created = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < created.Count; i++)
            {
                Object.DestroyImmediate(created[i]);
            }

            created.Clear();
        }

        // ---- Combos -------------------------------------------------------------

        private static List<ComboStep> ThreeHitCombo()
        {
            List<ComboStep> steps = new List<ComboStep>();
            for (int i = 0; i < 3; i++)
            {
                steps.Add(new ComboStep { duration = 0.3f, chainWindowStart = 0.1f, chainWindowEnd = 0.5f });
            }

            return steps;
        }

        [Test]
        public void Combo_PressesInsideTheWindowChainSteps()
        {
            ComboTracker tracker = new ComboTracker();
            List<ComboStep> steps = ThreeHitCombo();

            Assert.AreEqual(0, tracker.Press(0f, steps));
            Assert.AreEqual(1, tracker.Press(0.2f, steps));
            Assert.AreEqual(2, tracker.Press(0.4f, steps));
        }

        [Test]
        public void Combo_EarlyPressIsBufferedUntilTheWindowOpens()
        {
            ComboTracker tracker = new ComboTracker();
            List<ComboStep> steps = ThreeHitCombo();
            tracker.Press(0f, steps);

            Assert.AreEqual(-1, tracker.Press(0.05f, steps));
            Assert.AreEqual(-1, tracker.Tick(0.08f, steps));
            Assert.AreEqual(1, tracker.Tick(0.1f, steps));
        }

        [Test]
        public void Combo_ResetsAfterTheWindowCloses()
        {
            ComboTracker tracker = new ComboTracker();
            List<ComboStep> steps = ThreeHitCombo();
            tracker.Press(0f, steps);

            Assert.AreEqual(0, tracker.Press(1f, steps));
        }

        // ---- Parry --------------------------------------------------------------

        [Test]
        public void Parry_WindowIsShortAndHasACooldown()
        {
            ParryWindow window = new ParryWindow();

            Assert.IsTrue(window.TryOpen(1f, 0.15f, 0.4f));
            Assert.IsTrue(window.IsOpen(1.1f));
            Assert.IsFalse(window.IsOpen(1.2f));
            Assert.IsFalse(window.TryOpen(1.3f, 0.15f, 0.4f));
            Assert.IsTrue(window.TryOpen(1.45f, 0.15f, 0.4f));
        }

        // ---- Health -------------------------------------------------------------

        [Test]
        public void Kill_IgnoresInvulnerability()
        {
            Health health = CreateHealth("Target", Team.Enemy, 100f);
            health.Invulnerable = true;

            health.Kill();

            Assert.IsTrue(health.IsDead);
        }

        [Test]
        public void Heal_NeverGoesAboveMax()
        {
            Health health = CreateHealth("Target", Team.Player, 100f);
            health.TakeDamage(30f);

            health.Heal(50f);

            Assert.AreEqual(100f, health.Current);
        }

        [Test]
        public void HitInvulnerability_BlocksTheNextHit()
        {
            Health health = CreateHealth("Warrior", Team.Player, 100f);
            health.SetHitInvulnerability(0.5f);

            health.TakeDamage(10f);
            health.TakeDamage(10f);

            Assert.AreEqual(90f, health.Current);
            Assert.IsTrue(health.IsRecovering);
        }

        // ---- Special meter ------------------------------------------------------

        [Test]
        public void SpecialMeter_FillsForAttackerAndVictim()
        {
            Health attackerHealth = CreateHealth("Attacker", Team.Player, 100f);
            SpecialMeter attackerMeter = attackerHealth.gameObject.AddComponent<SpecialMeter>();
            attackerMeter.Configure(0.4f, 0.6f);
            Health target = CreateHealth("Target", Team.Enemy, 100f);
            SpecialMeter targetMeter = target.gameObject.AddComponent<SpecialMeter>();
            targetMeter.Configure(0.4f, 0.6f);

            target.TakeDamage(new DamageInfo { amount = 50f, source = attackerHealth.gameObject });

            Assert.AreEqual(20f, attackerMeter.Current, 0.001f);
            Assert.AreEqual(30f, targetMeter.Current, 0.001f);
            Assert.IsTrue(targetMeter.TrySpend(30f));
            Assert.IsFalse(targetMeter.TrySpend(1f));
        }

        // ---- Hit resolution -----------------------------------------------------

        [Test]
        public void HitResolver_HitsEachEnemyOnce_PreferringTheExposedEye()
        {
            Health enemy = CreateHealth("Enemy", Team.Enemy, 100f);
            Collider body = enemy.gameObject.AddComponent<BoxCollider>();
            GameObject eyeObject = new GameObject("Eye");
            eyeObject.transform.SetParent(enemy.transform);
            Collider eye = eyeObject.AddComponent<SphereCollider>();
            WeakPoint weakPoint = eyeObject.AddComponent<WeakPoint>();
            weakPoint.SetTarget(enemy);
            weakPoint.Configure(2.5f, true);

            int hits = HitResolver.ApplyToColliders(new[] { body, eye }, 2, new DamageInfo { amount = 10f }, Team.Player);

            Assert.AreEqual(1, hits);
            Assert.AreEqual(75f, enemy.Current);
            Assert.IsTrue(enemy.LastDamage.isCritical);
        }

        [Test]
        public void HitResolver_PlainSwingTreatsTheEyeAsBody()
        {
            Health enemy = CreateHealth("Enemy", Team.Enemy, 100f);
            GameObject eyeObject = new GameObject("Eye");
            eyeObject.transform.SetParent(enemy.transform);
            Collider eye = eyeObject.AddComponent<SphereCollider>();
            WeakPoint weakPoint = eyeObject.AddComponent<WeakPoint>();
            weakPoint.SetTarget(enemy);
            weakPoint.Configure(2.5f, true, true);

            HitResolver.ApplyToColliders(new[] { eye }, 1, new DamageInfo { amount = 10f }, Team.Player, null, false);

            Assert.AreEqual(90f, enemy.Current);
            Assert.IsFalse(enemy.LastDamage.isCritical);
        }

        [Test]
        public void HitResolver_IgnoresTheAttackersOwnTeam()
        {
            Health ally = CreateHealth("Ally", Team.Player, 100f);
            Collider body = ally.gameObject.AddComponent<BoxCollider>();

            Health hit = HitResolver.ApplyToCollider(body, new DamageInfo { amount = 10f }, Team.Player);

            Assert.IsNull(hit);
            Assert.AreEqual(100f, ally.Current);
        }

        [Test]
        public void WeakPoint_InstantKillDevirtualizesSmallMonsters()
        {
            Health kankrelat = CreateHealth("Kankrelat", Team.Enemy, 60f);
            WeakPoint eye = new GameObject("Eye").AddComponent<WeakPoint>();
            created.Add(eye.gameObject);
            eye.SetTarget(kankrelat);
            eye.Configure(2.5f, true, true);

            eye.ApplyDamage(1f, null);

            Assert.IsTrue(kankrelat.IsDead);
        }

        // ---- Targeting ----------------------------------------------------------

        [Test]
        public void Targeting_PrefersTheEnemyClosestToTheScreenCentre()
        {
            List<Vector3> candidates = new List<Vector3> { new Vector3(10f, 0f, 10f), new Vector3(0f, 0f, 15f) };

            int best = TargetingMath.PickBest(Vector3.zero, Vector3.forward, Vector3.zero, candidates);

            Assert.AreEqual(1, best);
        }

        [Test]
        public void Targeting_SwitchesToTheNextEnemyOnTheRequestedSide()
        {
            List<Vector3> candidates = new List<Vector3> { new Vector3(-5f, 0f, 10f), new Vector3(0f, 0f, 10f), new Vector3(5f, 0f, 10f) };

            Assert.AreEqual(2, TargetingMath.PickNext(Vector3.zero, candidates[1], candidates, 1));
            Assert.AreEqual(0, TargetingMath.PickNext(Vector3.zero, candidates[1], candidates, -1));
        }

        [Test]
        public void AimAssist_OnlyBendsShotsInsideTheCone()
        {
            List<Vector3> candidates = new List<Vector3> { new Vector3(1f, 0f, 10f), new Vector3(10f, 0f, 10f) };

            Assert.AreEqual(0, TargetingMath.PickAimAssist(Vector3.zero, Vector3.forward, candidates, 12f, 30f));
            Assert.AreEqual(-1, TargetingMath.PickAimAssist(Vector3.zero, Vector3.right * -1f, candidates, 12f, 30f));
        }

        private Health CreateHealth(string objectName, Team team, float max)
        {
            GameObject owner = new GameObject(objectName);
            created.Add(owner);
            Health health = owner.AddComponent<Health>();
            health.SetDestroyOnDeath(false);
            health.SetTeam(team);
            health.SetMaxHealth(max);
            return health;
        }
    }
}
