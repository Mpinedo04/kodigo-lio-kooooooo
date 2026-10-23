using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    [Serializable]
    public sealed class EnemyAttackDefinition
    {
        public string attackName;
        public EnemyAttackType type = EnemyAttackType.Laser;
        public float damage = 10f;
        public float cooldown = 1.5f;
        public float range = 15f;
        [Tooltip("Telegraph time before the attack lands (eye glows).")]
        public float windup = 0.5f;
        [Tooltip("Pause after the attack before the monster acts again.")]
        public float recovery = 0.4f;
        [Tooltip("Shots (or fire rings) per attack.")]
        public int burstCount = 1;
        public float burstInterval = 0.15f;
        [Tooltip("Beams: how long the beam stays active.")]
        public float activeTime = 0.5f;
        public float knockback;
        public bool heavy;
        [Tooltip("Fire ring: final radius. Beam: length. Melee: reach.")]
        public float areaSize = 8f;
        [Tooltip("Beam: wide side (horizontal beam width, vertical beam height).")]
        public float areaWidth = 6f;
        [Tooltip("Fire ring: band thickness. Beam: thin side (jump over / step aside).")]
        public float areaThickness = 1f;
        [Tooltip("Ice: speed multiplier while slowed (0.5 = half speed).")]
        public float slowMultiplier = 0.5f;
        [Tooltip("Ice / prison: how long the effect lasts on the warrior.")]
        public float statusDuration = 2f;
        [Tooltip("Ice / prison shots.")]
        public float projectileSpeed = 18f;
        [Tooltip("Relative chance of picking this attack when several are ready.")]
        public float weight = 1f;
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
        [Tooltip("Flyers: cruise height above the ground where they spawned.")]
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

        [Header("Perception (F06)")]
        [Tooltip("Full vision cone in degrees (360 = eyes everywhere).")]
        public float visionAngle = 160f;
        [Tooltip("Allies inside this radius are warned when this monster spots a target.")]
        public float alertAllyRadius = 12f;
        [Tooltip("Seconds without line of sight before the target is forgotten.")]
        public float forgetTargetAfter = 4f;
        [Tooltip("Pause between spotting a target and chasing it.")]
        public float reactionTime = 0.4f;
        public float patrolRadius = 4f;
        public float idleTime = 2f;

        [Header("Positioning (F06)")]
        [Tooltip("Distance kept from the target (spread ring for Kankrelats, orbit for Hornets).")]
        public float preferredDistance = 8f;
        [Tooltip("Degrees per second.")]
        public float turnSpeed = 360f;
        [Tooltip("NavMeshAgent radius.")]
        public float bodyRadius = 0.6f;
        [Tooltip("Minimum pause between two attacks of this monster.")]
        public float attackGap = 0.6f;

        [Header("Reactions (F06)")]
        [Tooltip("Scales Stagger() durations. 0 = immune (bosses).")]
        public float staggerMultiplier = 1f;
        [Tooltip("Seconds a heavy hit staggers this monster (0 = none).")]
        public float heavyHitStagger = 0.5f;
        [Tooltip("Guarded monsters (Megatank closed, Blok hiding its eye): heavy hits open the guard this long.")]
        public float guardCrackDuration = 1.5f;
        [Tooltip("Yumi's telekinesis can lift it.")]
        public bool liftableByTelekinesis;

        [Header("Breakable parts (F06, Krabe)")]
        public float partHealth = 25f;
        [Tooltip("Broken legs needed to knock it down.")]
        public int partsToKnockDown = 2;
        public float knockedDownDuration = 4f;

        [Header("Loot (F06)")]
        [Tooltip("Data fragments dropped when devirtualized.")]
        public int dataDrop = 1;

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
