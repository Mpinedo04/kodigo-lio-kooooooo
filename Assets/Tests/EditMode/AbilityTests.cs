using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame.Tests
{
    public sealed class AbilityTests
    {
        private readonly List<Object> created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < created.Count; i++)
            {
                Object.DestroyImmediate(created[i]);
            }

            created.Clear();
            SlowMotion.End();
        }

        [Test]
        public void ResourcePool_SpendsDrainsAndRefillsAfterAPause()
        {
            ResourcePool pool = new ResourcePool(100f, 10f, 1f);

            Assert.IsTrue(pool.TrySpend(30f));
            Assert.AreEqual(70f, pool.Current, 0.001f);
            Assert.IsFalse(pool.TrySpend(80f));

            pool.Tick(0.5f);
            Assert.AreEqual(70f, pool.Current, 0.001f);
            pool.Tick(1f);
            Assert.AreEqual(80f, pool.Current, 0.001f);

            Assert.IsTrue(pool.Drain(200f));
            Assert.AreEqual(0f, pool.Current);
        }

        [Test]
        public void Factory_BuildsEveryWarriorAbility()
        {
            string[] ids =
            {
                "odd_shield", "odd_future_flash", "odd_feline_rain",
                "ulrich_supersprint", "ulrich_triplicate", "ulrich_impact", "ulrich_katana_deflect",
                "yumi_tk_grab", "yumi_tk_wall", "yumi_fan_dance",
                "aelita_creativity_bridge", "aelita_creativity_wall", "aelita_wings"
            };

            for (int i = 0; i < ids.Length; i++)
            {
                Assert.IsNotNull(AbilityFactory.Create(Definition(ids[i])), ids[i]);
            }

            Assert.IsNull(AbilityFactory.Create(Definition("odd_cat_claws")));
            Assert.IsInstanceOf<SupersprintAbility>(AbilityFactory.Create(Definition("ulrich_supersprint")));
        }

        [Test]
        public void Shield_BlocksOnlyShotsComingFromTheFront()
        {
            DamageInfo laser = new DamageInfo { amount = 10f, type = DamageType.Laser, direction = Vector3.back };
            Assert.IsTrue(ShieldMath.Blocks(Vector3.forward, laser));

            laser.direction = Vector3.forward;
            Assert.IsFalse(ShieldMath.Blocks(Vector3.forward, laser));

            DamageInfo slash = new DamageInfo { amount = 10f, type = DamageType.Melee, direction = Vector3.back };
            Assert.IsFalse(ShieldMath.Blocks(Vector3.forward, slash));
        }

        [Test]
        public void Triangular_SurroundsTheTargetEvenly()
        {
            Vector3[] corners = AbilityMath.TriangleAround(Vector3.zero, new Vector3(0f, 0f, -5f), 2f);

            Assert.AreEqual(3, corners.Length);
            for (int i = 0; i < corners.Length; i++)
            {
                Assert.AreEqual(2f, corners[i].magnitude, 0.001f);
            }

            Assert.AreEqual(-2f, corners[0].z, 0.001f);
            Assert.AreEqual(120f, Vector3.Angle(corners[0], corners[1]), 0.1f);
            Assert.AreEqual(120f, Vector3.Angle(corners[1], corners[2]), 0.1f);
        }

        [Test]
        public void TelekinesisState_TracksLiftedMonsters()
        {
            GameObject kankrelat = new GameObject("Kankrelat");
            created.Add(kankrelat);

            TelekinesisState.Mark(kankrelat, true);
            Assert.IsTrue(TelekinesisState.IsLifted(kankrelat));

            TelekinesisState.Mark(kankrelat, false);
            Assert.IsFalse(TelekinesisState.IsLifted(kankrelat));
        }

        [Test]
        public void SlowMotion_RemembersItsScaleUntilItEnds()
        {
            SlowMotion.Begin(0.35f);
            Assert.IsTrue(SlowMotion.IsActive);
            Assert.AreEqual(0.35f, SlowMotion.CurrentScale, 0.001f);

            SlowMotion.End();
            Assert.AreEqual(1f, SlowMotion.CurrentScale, 0.001f);
        }

        [Test]
        public void Synergy_DistanceCheck()
        {
            Assert.IsTrue(TeamSynergy.AreClose(Vector3.zero, new Vector3(3f, 0f, 4f), 5f));
            Assert.IsFalse(TeamSynergy.AreClose(Vector3.zero, new Vector3(3f, 0f, 4.1f), 5f));
        }

        [Test]
        public void Switching_LeavesTheOtherWarriorInLyokoAsCompanion()
        {
            GameObject systems = new GameObject("Systems");
            created.Add(systems);
            CharacterSwitcher switcher = systems.AddComponent<CharacterSwitcher>();
            PlayerController odd = CreateWarrior("Odd", switcher);
            PlayerController ulrich = CreateWarrior("Ulrich", switcher);

            switcher.Activate(0);
            switcher.Activate(1);

            Assert.IsTrue(odd.gameObject.activeSelf);
            Assert.IsFalse(odd.IsControlled);
            Assert.IsTrue(ulrich.IsControlled);
        }

        private PlayerController CreateWarrior(string warriorName, CharacterSwitcher switcher)
        {
            GameObject warrior = new GameObject(warriorName);
            created.Add(warrior);
            Health health = warrior.AddComponent<Health>();
            health.SetDestroyOnDeath(false);
            health.SetMaxHealth(100f);
            PlayerController controller = warrior.AddComponent<PlayerController>();
            switcher.Register(controller);
            return controller;
        }

        private AbilityDefinition Definition(string id)
        {
            AbilityDefinition definition = ScriptableObject.CreateInstance<AbilityDefinition>();
            definition.abilityId = id;
            created.Add(definition);
            return definition;
        }
    }
}
