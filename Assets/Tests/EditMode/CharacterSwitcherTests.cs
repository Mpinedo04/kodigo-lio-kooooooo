using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CodeLyokoFanGame.Tests
{
    public sealed class CharacterSwitcherTests
    {
        private readonly List<GameObject> created = new List<GameObject>();
        private CharacterSwitcher switcher;
        private Health odd;
        private Health ulrich;
        private Health yumi;

        [SetUp]
        public void SetUp()
        {
            GameObject systems = new GameObject("Systems");
            created.Add(systems);
            switcher = systems.AddComponent<CharacterSwitcher>();

            odd = CreateWarrior("Odd");
            ulrich = CreateWarrior("Ulrich");
            yumi = CreateWarrior("Yumi");
            switcher.Activate(0);
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

        [Test]
        public void Switching_KeepsEachWarriorsHealth()
        {
            odd.TakeDamage(40f);

            switcher.Activate(1);
            switcher.Activate(0);

            Assert.AreEqual(60f, odd.Current);
            Assert.AreEqual(100f, ulrich.Current);
            Assert.AreSame(odd.gameObject, switcher.ActiveCharacter.gameObject);
        }

        [Test]
        public void DevirtualizedWarrior_CannotBeSelected()
        {
            ulrich.TakeDamage(100f);

            switcher.Activate(1);

            Assert.AreSame(odd.gameObject, switcher.ActiveCharacter.gameObject);
            Assert.IsFalse(ulrich.gameObject.activeSelf);
        }

        [Test]
        public void WarriorLostInTheDigitalSea_NextOneDoesNotSpawnInTheSea()
        {
            odd.transform.position = new Vector3(0f, -20f, 0f);
            odd.TakeDamage(100f);

            switcher.Activate(1);

            Assert.Greater(ulrich.transform.position.y, -1f);
            Assert.AreSame(ulrich.gameObject, switcher.ActiveCharacter.gameObject);
        }

        [Test]
        public void HasAliveCharacter_IsFalseWhenAllAreDevirtualized()
        {
            odd.TakeDamage(100f);
            ulrich.TakeDamage(100f);
            yumi.TakeDamage(100f);

            Assert.IsFalse(switcher.HasAliveCharacter);
        }

        private Health CreateWarrior(string warriorName)
        {
            GameObject warrior = new GameObject(warriorName);
            created.Add(warrior);
            Health health = warrior.AddComponent<Health>();
            health.SetDestroyOnDeath(false);
            health.SetMaxHealth(100f);
            switcher.Register(warrior.AddComponent<PlayerController>());
            return health;
        }
    }
}
