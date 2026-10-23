using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame.Tests
{
    // Drives monsters frame by frame with EnemyAI.Tick (no Play mode, no director, no NavMesh).
    public sealed class EnemyBrainTests
    {
        private readonly List<Object> created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = created.Count - 1; i >= 0; i--)
            {
                if (created[i] != null)
                {
                    Object.DestroyImmediate(created[i]);
                }
            }

            created.Clear();
        }

        private EnemyDefinition Definition(EnemyKind kind)
        {
            EnemyDefinition definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            definition.kind = kind;
            definition.displayName = kind.ToString();
            EnemyDefaults.ApplySeasonOne(definition);
            created.Add(definition);
            return definition;
        }

        private EnemyAI CreateEnemy(EnemyDefinition definition, bool armed = false)
        {
            GameObject root = new GameObject("TestEnemy_" + definition.kind);
            created.Add(root);
            root.AddComponent<Health>().SetDestroyOnDeath(false);

            GameObject eye = new GameObject("Eye");
            eye.transform.SetParent(root.transform, false);
            eye.AddComponent<SphereCollider>().isTrigger = true;
            eye.AddComponent<WeakPoint>();

            EnemyAI enemy = root.AddComponent<EnemyAI>();
            enemy.Configure(definition, armed ? DummyProjectile() : null);
            return enemy;
        }

        private Projectile DummyProjectile()
        {
            GameObject shot = new GameObject("DummyShot");
            created.Add(shot);
            return shot.AddComponent<Projectile>();
        }

        private Health CreateWarrior(Vector3 position)
        {
            GameObject warrior = new GameObject("TestWarrior");
            created.Add(warrior);
            warrior.transform.position = position;
            Health health = warrior.AddComponent<Health>();
            health.SetDestroyOnDeath(false);
            health.SetMaxHealth(100f);
            health.SetTeam(Team.Player);
            return health;
        }

        private static DamageInfo Hit(float amount, bool heavy)
        {
            return new DamageInfo { amount = amount, type = DamageType.Melee, sourceTeam = Team.Player, isHeavy = heavy, direction = Vector3.forward };
        }

        [Test]
        public void Configure_AppliesCanonHealth()
        {
            EnemyAI krab = CreateEnemy(Definition(EnemyKind.Krab));

            Assert.AreEqual(80f, krab.Health.Max);
            Assert.AreEqual(Team.Enemy, krab.Health.Team);
            Assert.AreEqual(EnemyStateId.Idle, krab.CurrentState);
        }

        [Test]
        public void Stagger_StopsTheMonsterForThatLong()
        {
            EnemyAI enemy = CreateEnemy(Definition(EnemyKind.Kankrelat));

            enemy.Stagger(1f);
            Assert.AreEqual(EnemyStateId.Stagger, enemy.CurrentState);

            enemy.Tick(0.5f);
            Assert.AreEqual(EnemyStateId.Stagger, enemy.CurrentState);

            enemy.Tick(0.6f);
            Assert.AreNotEqual(EnemyStateId.Stagger, enemy.CurrentState);
        }

        [Test]
        public void Stagger_ImmuneMonstersIgnoreIt()
        {
            EnemyDefinition definition = Definition(EnemyKind.Megatank);
            definition.staggerMultiplier = 0f;
            EnemyAI enemy = CreateEnemy(definition);

            enemy.Stagger(2f);

            Assert.AreEqual(EnemyStateId.Idle, enemy.CurrentState);
        }

        [Test]
        public void HeavyHit_StaggersSmallMonsters_LightHitDoesNot()
        {
            EnemyAI enemy = CreateEnemy(Definition(EnemyKind.Hornet));

            enemy.Health.TakeDamage(Hit(1f, false));
            Assert.AreEqual(EnemyStateId.Idle, enemy.CurrentState);

            enemy.Health.TakeDamage(Hit(1f, true));
            Assert.AreEqual(EnemyStateId.Stagger, enemy.CurrentState);
        }

        [Test]
        public void HeavyHit_CracksTheMegatankGuardAndExposesTheEye()
        {
            EnemyAI megatank = CreateEnemy(Definition(EnemyKind.Megatank));
            megatank.Tick(0.01f);
            Assert.IsTrue(megatank.IsGuarded);
            Assert.IsFalse(megatank.WeakPoint.IsExposed);

            megatank.Health.TakeDamage(Hit(5f, true));
            megatank.Tick(0.01f);

            Assert.AreEqual(EnemyStateId.Stagger, megatank.CurrentState);
            Assert.IsTrue(megatank.WeakPoint.IsExposed);

            megatank.Tick(2f);
            Assert.IsFalse(megatank.WeakPoint.IsExposed);
        }

        [Test]
        public void ExposeWeakPoint_OpensTheEyeForThatLong()
        {
            EnemyAI megatank = CreateEnemy(Definition(EnemyKind.Megatank));
            megatank.Tick(0.01f);
            Assert.IsFalse(megatank.WeakPoint.IsExposed);

            megatank.ExposeWeakPoint(1f);
            Assert.IsTrue(megatank.WeakPoint.IsExposed);

            megatank.Tick(0.5f);
            Assert.IsTrue(megatank.WeakPoint.IsExposed);

            megatank.Tick(0.6f);
            Assert.IsFalse(megatank.WeakPoint.IsExposed);
        }

        [Test]
        public void CanBeLifted_ComesFromTheDefinition()
        {
            Assert.IsTrue(CreateEnemy(Definition(EnemyKind.Kankrelat)).CanBeLifted);
            Assert.IsTrue(CreateEnemy(Definition(EnemyKind.Hornet)).CanBeLifted);
            Assert.IsFalse(CreateEnemy(Definition(EnemyKind.Blok)).CanBeLifted);
            Assert.IsFalse(CreateEnemy(Definition(EnemyKind.Megatank)).CanBeLifted);
        }

        [Test]
        public void Alert_TargetsTheWarriorAndStartsTheChase()
        {
            EnemyAI enemy = CreateEnemy(Definition(EnemyKind.Kankrelat));
            Health warrior = CreateWarrior(new Vector3(0f, 0f, 6f));

            Assert.IsTrue(enemy.Alert(warrior));
            Assert.AreEqual(EnemyStateId.Alert, enemy.CurrentState);
            Assert.AreSame(warrior, enemy.Target);

            enemy.Tick(0.5f);
            Assert.AreEqual(EnemyStateId.Chase, enemy.CurrentState);
        }

        [Test]
        public void ArmedMonster_AttacksAndTelekinesisInterruptsIt()
        {
            EnemyAI enemy = CreateEnemy(Definition(EnemyKind.Kankrelat), true);
            Health warrior = CreateWarrior(new Vector3(0f, 0f, 6f));
            enemy.Alert(warrior);
            enemy.Tick(0.5f);
            enemy.Tick(0.05f);
            Assert.AreEqual(EnemyStateId.Attack, enemy.CurrentState);

            enemy.SetSuspended(true);
            Assert.IsTrue(enemy.IsSuspended);
            Assert.AreNotEqual(EnemyStateId.Attack, enemy.CurrentState);

            EnemyStateId frozen = enemy.CurrentState;
            enemy.Tick(5f);
            Assert.AreEqual(frozen, enemy.CurrentState);

            enemy.SetSuspended(false);
            Assert.IsFalse(enemy.IsSuspended);
        }

        [Test]
        public void UnarmedMonster_NeverAttacks()
        {
            EnemyAI enemy = CreateEnemy(Definition(EnemyKind.Kankrelat));
            Health warrior = CreateWarrior(new Vector3(0f, 0f, 6f));
            enemy.Alert(warrior);

            for (int i = 0; i < 40; i++)
            {
                enemy.Tick(0.1f);
                Assert.AreNotEqual(EnemyStateId.Attack, enemy.CurrentState);
            }
        }

        [Test]
        public void Death_IsFinal()
        {
            EnemyAI enemy = CreateEnemy(Definition(EnemyKind.Kankrelat));

            enemy.Health.TakeDamage(Hit(100f, false));

            Assert.AreEqual(EnemyStateId.Dead, enemy.CurrentState);
            Assert.IsTrue(enemy.IsDead);
            enemy.Stagger(1f);
            Assert.AreEqual(EnemyStateId.Dead, enemy.CurrentState);
        }

        [Test]
        public void Krab_BreakingTwoLegsKnocksItDownWithTheEyeExposed()
        {
            EnemyAI krab = EnemyRigBuilder.Build("TestKrab", EnemyKind.Krab, Vector3.zero, null, null);
            created.Add(krab.gameObject);
            krab.Health.SetDestroyOnDeath(false);
            krab.Configure(Definition(EnemyKind.Krab), null);
            krab.Tick(0.01f);
            Assert.IsFalse(krab.WeakPoint.IsExposed);

            EnemyPart[] legs = krab.GetComponentsInChildren<EnemyPart>(true);
            Assert.AreEqual(4, legs.Length);
            Assert.AreEqual(25f, legs[0].Health.Max);

            legs[0].Health.TakeDamage(Hit(30f, false));
            Assert.AreNotEqual(EnemyStateId.Stagger, krab.CurrentState);
            legs[1].Health.TakeDamage(Hit(30f, false));
            krab.Tick(0.01f);

            Assert.AreEqual(EnemyStateId.Stagger, krab.CurrentState);
            Assert.IsTrue(krab.WeakPoint.IsExposed);
            Assert.AreEqual(80f, krab.Health.Current, "legs take damage separately");

            krab.Tick(4.1f);
            Assert.AreNotEqual(EnemyStateId.Stagger, krab.CurrentState);
            Assert.IsFalse(legs[0].IsBroken);
            Assert.IsFalse(legs[1].IsBroken);
        }
    }
}
