using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    [Serializable]
    public sealed class EnemyAttackDefinition
    {
        public string attackName;
        public float damage = 10f;
        public float cooldown = 1.5f;
        public float range = 15f;
        [Tooltip("Telegraph time before the attack lands (eye glows).")]
        public float windup = 0.5f;
        public GameObject projectilePrefab;
    }

    [Serializable]
    public sealed class BossPhaseDefinition
    {
        public string phaseName;
        [Range(0f, 1f)] public float startsAtHealth01 = 1f;
        [TextArea(1, 3)] public string notes;
    }

    [CreateAssetMenu(menuName = "Code Lyoko/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        public EnemyKind kind;
        public string displayName;
        [TextArea(2, 4)] public string canonNotes;

        [Header("Combat")]
        public EnemyBehaviorStyle behaviorStyle = EnemyBehaviorStyle.SwarmShooter;
        public float maxHealth = 60f;
        public float detectionRange = 24f;
        public float attackRange = 18f;
        public float moveSpeed = 3f;
        public float projectileDamage = 10f;
        public float fireCooldown = 1.2f;
        public float specialCooldown = 4f;
        public float hoverHeight;
        public float contactDamage;
        public bool flying;
        public bool boss;
        public bool digitalSeaOnly;

        [Header("Weak Point")]
        public string weakPointNotes = "Eye of X.A.N.A.";
        public float weakPointDamageMultiplier = 2.5f;
        public bool weakPointAlwaysExposed = true;
        [Tooltip("Small monsters: a hit on the exposed eye devirtualizes them at once.")]
        public bool weakPointInstantKill;
        [Tooltip("1 = normal push, 0 = immovable.")]
        public float knockbackScale = 1f;

        [Header("Attacks and boss phases")]
        public List<EnemyAttackDefinition> attacks = new List<EnemyAttackDefinition>();
        public List<BossPhaseDefinition> bossPhases = new List<BossPhaseDefinition>();

        [Header("Feedback")]
        public AudioClip attackSound;
        public AudioClip hitSound;
        public AudioClip devirtualizeSound;
        public GameObject devirtualizeVfx;

        [Header("Production")]
        public bool appearsInVerticalSlice;
        public GameObject prototypePrefab;
        public GameObject finalModelPrefab;
        public string modelStatus = "Placeholder";
        public string animationStatus = "Not started";
        public string vfxStatus = "Not started";
        public string sourceAssetId;
    }
}
