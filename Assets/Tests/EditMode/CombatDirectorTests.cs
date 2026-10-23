using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame.Tests
{
    public sealed class CombatDirectorTests
    {
        private readonly List<Object> created = new List<Object>();
        private CombatDirectorProfile profile;

        [SetUp]
        public void SetUp()
        {
            profile = ScriptableObject.CreateInstance<CombatDirectorProfile>();
            created.Add(profile);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < created.Count; i++)
            {
                Object.DestroyImmediate(created[i]);
            }

            created.Clear();
        }

        private Health CreateTarget(string name, Vector3 position, Team team = Team.Player)
        {
            GameObject target = new GameObject(name);
            created.Add(target);
            target.transform.position = position;
            Health health = target.AddComponent<Health>();
            health.SetDestroyOnDeath(false);
            health.SetMaxHealth(100f);
            health.SetTeam(team);
            return health;
        }

        // ---- Scoring ------------------------------------------------------------------------------

        [Test]
        public void Score_CloserAndHigherPriorityTargetsScoreMore()
        {
            float near = CombatDirector.Score(1f, 5f, 0, profile, false);
            float far = CombatDirector.Score(1f, 20f, 0, profile, false);
            float precious = CombatDirector.Score(3f, 20f, 0, profile, false);

            Assert.Greater(near, far);
            Assert.Greater(precious, far);
            Assert.AreEqual(0f, CombatDirector.Score(0f, 1f, 0, profile, false));
        }

        [Test]
        public void Score_CrowdedTargetsLoseAppealOnlyPastTheThreshold()
        {
            profile.crowdThreshold = 2;
            float alone = CombatDirector.Score(1f, 10f, 0, profile, false);
            float one = CombatDirector.Score(1f, 10f, 1, profile, false);
            float two = CombatDirector.Score(1f, 10f, 2, profile, false);

            Assert.AreEqual(alone, one, 0.0001f);
            Assert.Less(two, one);
        }

        [Test]
        public void SelectTarget_PrefersTheCloserWarriorAndIgnoresEnemies()
        {
            Health near = CreateTarget("Near", new Vector3(0f, 0f, 5f));
            Health far = CreateTarget("Far", new Vector3(0f, 0f, 15f));
            Health monster = CreateTarget("Monster", new Vector3(0f, 0f, 1f), Team.Enemy);
            List<Health> candidates = new List<Health> { far, monster, near };

            Health chosen = CombatDirector.SelectTarget(candidates, Vector3.zero, null, 30f, null, profile, null, null);

            Assert.AreSame(near, chosen);
        }

        [Test]
        public void SelectTarget_AggroPriorityCanOutbidDistance()
        {
            Health warrior = CreateTarget("Warrior", new Vector3(0f, 0f, 5f));
            Health aelita = CreateTarget("Aelita", new Vector3(0f, 0f, 9f));
            aelita.gameObject.AddComponent<AggroTarget>().SetPriority(3f);

            Health chosen = CombatDirector.SelectTarget(new List<Health> { warrior, aelita }, Vector3.zero, null, 30f, null, profile, null, null);

            Assert.AreSame(aelita, chosen);
        }

        [Test]
        public void SelectTarget_KeepsTheCurrentTargetUnlessANewOneIsClearlyBetter()
        {
            Health current = CreateTarget("Current", new Vector3(0f, 0f, 8f));
            Health slightlyCloser = CreateTarget("SlightlyCloser", new Vector3(0f, 0f, 7f));
            List<Health> candidates = new List<Health> { current, slightlyCloser };

            Assert.AreSame(current, CombatDirector.SelectTarget(candidates, Vector3.zero, current, 30f, null, profile, null, null));
            Assert.AreSame(slightlyCloser, CombatDirector.SelectTarget(candidates, Vector3.zero, null, 30f, null, profile, null, null));
        }

        [Test]
        public void SelectTarget_SkipsDeadOutOfRangeAndUnseenTargets()
        {
            Health dead = CreateTarget("Dead", new Vector3(0f, 0f, 2f));
            dead.TakeDamage(1000f);
            Health tooFar = CreateTarget("TooFar", new Vector3(0f, 0f, 50f));
            Health hidden = CreateTarget("Hidden", new Vector3(0f, 0f, 4f));
            List<Health> candidates = new List<Health> { dead, tooFar, hidden };

            Health chosen = CombatDirector.SelectTarget(candidates, Vector3.zero, null, 30f, h => h != hidden, profile, null, null);

            Assert.IsNull(chosen);
        }

        [Test]
        public void SelectTarget_ControlledWarriorBonusBeatsASlightlyCloserCompanion()
        {
            Health controlled = CreateTarget("Controlled", new Vector3(0f, 0f, 8f));
            Health companion = CreateTarget("Companion", new Vector3(0f, 0f, 6f));

            Health chosen = CombatDirector.SelectTarget(new List<Health> { companion, controlled }, Vector3.zero, null, 30f, null, profile, controlled, null);

            Assert.AreSame(controlled, chosen);
        }

        // ---- Attack slots -------------------------------------------------------------------------

        [Test]
        public void AttackSlots_LimitTotalAndPerTarget()
        {
            AttackSlots slots = new AttackSlots();
            object warrior = new object();
            object aelita = new object();

            Assert.IsTrue(slots.TryAcquire("a", warrior, 3, 2));
            Assert.IsTrue(slots.TryAcquire("b", warrior, 3, 2));
            Assert.IsFalse(slots.TryAcquire("c", warrior, 3, 2), "third attacker on the same target");
            Assert.IsTrue(slots.TryAcquire("c", aelita, 3, 2));
            Assert.IsFalse(slots.TryAcquire("d", aelita, 3, 2), "global limit");
            Assert.AreEqual(3, slots.Count);
        }

        [Test]
        public void AttackSlots_ReleaseFreesASlotAndReacquiringIsIdempotent()
        {
            AttackSlots slots = new AttackSlots();
            Assert.IsTrue(slots.TryAcquire("a", null, 1, 0));
            Assert.IsTrue(slots.TryAcquire("a", null, 1, 0));
            Assert.IsFalse(slots.TryAcquire("b", null, 1, 0));

            slots.Release("a");

            Assert.IsFalse(slots.Holds("a"));
            Assert.IsTrue(slots.TryAcquire("b", null, 1, 0));
        }
    }
}
