using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    [Serializable]
    public sealed class WaveGroup
    {
        public EnemyDefinition definition;
        [Tooltip("Used when no definition is set.")]
        public EnemyKind fallbackKind = EnemyKind.Kankrelat;
        public int count = 1;
        public float radius = 5f;
        public float delayBetweenSpawns = 0.35f;

        public EnemyKind Kind => definition != null ? definition.kind : fallbackKind;
    }

    // One wave of monsters for a WaveSpawner (F06). Groups spawn one after another.
    [CreateAssetMenu(menuName = "Code Lyoko/Wave Definition", fileName = "WaveDefinition")]
    public sealed class WaveDefinition : ScriptableObject
    {
        public string waveName;
        public List<WaveGroup> groups = new List<WaveGroup>();
        [Tooltip("The next wave starts when this many (or fewer) monsters of this wave are left.")]
        public int nextWaveWhenAliveAtMost;
        public float delayBeforeNextWave = 2f;
        [Tooltip("Seconds each monster takes to materialize (invulnerable and still meanwhile).")]
        public float materializeTime = 0.9f;

        public int TotalCount
        {
            get
            {
                int total = 0;
                for (int i = 0; i < groups.Count; i++)
                {
                    total += Mathf.Max(0, groups[i].count);
                }

                return total;
            }
        }
    }
}
