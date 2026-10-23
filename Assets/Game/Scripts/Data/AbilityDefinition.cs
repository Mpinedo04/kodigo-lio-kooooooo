using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    // Tunable data for one ability (Supersprint, Shield, Telekinesis...). The behaviour is the Ability subclass
    // that AbilityFactory picks from abilityId.
    [CreateAssetMenu(menuName = "Code Lyoko/Ability Definition", fileName = "AbilityDefinition")]
    public sealed class AbilityDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string abilityId;
        public string displayName;
        public LyokoCharacter owner;
        [TextArea(2, 4)] public string description;
        public Sprite icon;

        [Header("Gameplay")]
        public AbilitySlot slot;
        public AbilityKind kind;
        public float cooldown = 4f;
        public AbilityResource resource = AbilityResource.None;
        public float cost;
        public float duration;
        public float power = 1f;
        public GameObject effectPrefab;

        [Header("Tuning (each ability uses the ones it needs)")]
        public float damage;
        public float radius;
        public float range;
        public float speed;
        public int count;
        [Tooltip("Hit points of what the ability creates (Odd's shield, Ulrich's clones).")]
        public float abilityHealth;
        [Tooltip("Resource drained per second while the ability is held (telekinesis).")]
        public float costPerSecond;
        [Tooltip("Game speed while active (Future Vision slow motion).")]
        [Range(0.05f, 1f)] public float timeScale = 1f;

        [Header("Progression")]
        [Tooltip("Chapter where the ability unlocks (0 = prologue).")]
        public int unlockChapter;
        public bool canon = true;
    }
}
