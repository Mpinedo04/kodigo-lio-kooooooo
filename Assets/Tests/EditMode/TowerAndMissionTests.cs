using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CodeLyokoFanGame.Tests
{
    public sealed class TowerAndMissionTests
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

        [Test]
        public void TowerInterior_TypesTheCodeLetterByLetter()
        {
            Assert.AreEqual(string.Empty, TowerInterior.TypedText(0f));
            Assert.AreEqual("CODE:", TowerInterior.TypedText(5f / 11f));
            Assert.AreEqual("CODE: LYOKO", TowerInterior.TypedText(1f));
            Assert.AreEqual("CODE: LYOKO", TowerInterior.TypedText(2f));
        }

        [Test]
        public void TowerHalo_HasADifferentColourForEachOwner()
        {
            Color neutral = TowerVisual.ColorOf(TowerState.Neutral);
            Color xana = TowerVisual.ColorOf(TowerState.Xana);
            Color jeremie = TowerVisual.ColorOf(TowerState.Jeremie);
            Color franz = TowerVisual.ColorOf(TowerState.FranzHopper);

            Assert.Greater(neutral.b, neutral.r);
            Assert.Greater(xana.r, xana.b);
            Assert.Greater(jeremie.g, jeremie.r);
            Assert.AreEqual(Color.white, franz);
        }

        [Test]
        public void OrderWheel_StickDirectionPicksTheOrder()
        {
            Assert.AreEqual(AelitaOrder.Follow, AelitaOrders.Pick(Vector2.up));
            Assert.AreEqual(AelitaOrder.Wait, AelitaOrders.Pick(Vector2.right));
            Assert.AreEqual(AelitaOrder.Hide, AelitaOrders.Pick(Vector2.down));
            Assert.AreEqual(AelitaOrder.GoToTower, AelitaOrders.Pick(Vector2.left));
            Assert.AreEqual(AelitaOrder.Follow, AelitaOrders.Next(AelitaOrder.GoToTower));
        }

        [Test]
        public void EmergencyReturn_BringsEveryoneBackWithFullHealth()
        {
            CharacterSwitcher switcher = CreateSwitcher();
            Health odd = CreateWarrior("Odd", switcher);
            Health ulrich = CreateWarrior("Ulrich", switcher);
            switcher.Activate(0);
            odd.TakeDamage(100f);
            ulrich.TakeDamage(30f);

            Vector3 checkpoint = new Vector3(5f, 1f, 5f);
            switcher.ReviveAllAt(checkpoint, Quaternion.identity);

            Assert.IsTrue(odd.gameObject.activeSelf);
            Assert.AreEqual(100f, odd.Current);
            Assert.AreEqual(100f, ulrich.Current);
            Assert.AreEqual(checkpoint, odd.transform.position);
            Assert.Greater(Vector3.Distance(checkpoint, ulrich.transform.position), 0.5f);
            Assert.IsTrue(switcher.HasAliveCharacter);
        }

        [Test]
        public void SuspendedControl_LeavesEveryoneToTheAiUntilResumed()
        {
            CharacterSwitcher switcher = CreateSwitcher();
            Health odd = CreateWarrior("Odd", switcher);
            CreateWarrior("Ulrich", switcher);
            switcher.Activate(0);
            PlayerController oddController = odd.GetComponent<PlayerController>();

            switcher.SuspendControl();
            Assert.IsTrue(switcher.IsControlSuspended);
            Assert.IsFalse(oddController.IsControlled);

            switcher.ResumeControl();
            Assert.IsFalse(switcher.IsControlSuspended);
            Assert.IsTrue(oddController.IsControlled);
        }

        private CharacterSwitcher CreateSwitcher()
        {
            GameObject systems = new GameObject("Systems");
            created.Add(systems);
            return systems.AddComponent<CharacterSwitcher>();
        }

        private Health CreateWarrior(string warriorName, CharacterSwitcher switcher)
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
