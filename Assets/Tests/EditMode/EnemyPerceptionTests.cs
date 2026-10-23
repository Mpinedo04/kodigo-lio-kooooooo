using NUnit.Framework;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame.Tests
{
    public sealed class EnemyPerceptionTests
    {
        [Test]
        public void VisionCone_SeesInFrontButNotBehind()
        {
            Assert.IsTrue(EnemyPerception.InVisionCone(Vector3.zero, Vector3.forward, new Vector3(1f, 0f, 10f), 20f, 120f));
            Assert.IsFalse(EnemyPerception.InVisionCone(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, -10f), 20f, 120f));
        }

        [Test]
        public void VisionCone_RespectsRangeAndFullCircle()
        {
            Assert.IsFalse(EnemyPerception.InVisionCone(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 30f), 20f, 360f));
            Assert.IsTrue(EnemyPerception.InVisionCone(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, -10f), 20f, 360f));
        }

        [Test]
        public void VisionCone_IgnoresHeightSoFlyersSeeBelow()
        {
            Assert.IsTrue(EnemyPerception.InVisionCone(new Vector3(0f, 5f, 0f), Vector3.forward, new Vector3(0f, 0f, 3f), 20f, 60f));
        }

        [Test]
        public void WaveDefinition_TotalCountAddsEveryGroup()
        {
            WaveDefinition wave = ScriptableObject.CreateInstance<WaveDefinition>();
            wave.groups.Add(new WaveGroup { fallbackKind = EnemyKind.Kankrelat, count = 3 });
            wave.groups.Add(new WaveGroup { fallbackKind = EnemyKind.Blok, count = 1 });
            wave.groups.Add(new WaveGroup { fallbackKind = EnemyKind.Hornet, count = 1 });

            Assert.AreEqual(5, wave.TotalCount);
            Object.DestroyImmediate(wave);
        }

        [Test]
        public void AggroTarget_DefaultsToPriorityOneAndChestHeight()
        {
            GameObject warrior = new GameObject("Warrior");
            warrior.transform.position = new Vector3(2f, 0f, 3f);

            Assert.AreEqual(1f, AggroTarget.PriorityOf(warrior.transform));
            Assert.AreEqual(new Vector3(2f, 1f, 3f), AggroTarget.AimPositionOf(warrior.transform));

            AggroTarget aggro = warrior.AddComponent<AggroTarget>();
            aggro.SetPriority(2.5f);
            Assert.AreEqual(2.5f, AggroTarget.PriorityOf(warrior.transform));
            aggro.SetPriority(-1f);
            Assert.AreEqual(0f, AggroTarget.PriorityOf(warrior.transform));
            Assert.AreSame(warrior.transform, aggro.AimPoint);

            Object.DestroyImmediate(warrior);
        }
    }
}
