using UnityEngine;

namespace CodeLyokoFanGame.Data
{
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
