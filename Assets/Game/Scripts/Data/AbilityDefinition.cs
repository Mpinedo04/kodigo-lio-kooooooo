using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    // Tunable data for one ability (Supersprint, Shield, Telekinesis...). Behaviour comes in F05.
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

        [Header("Progression")]
        [Tooltip("Chapter where the ability unlocks (0 = prologue).")]
        public int unlockChapter;
        public bool canon = true;
    }
}
