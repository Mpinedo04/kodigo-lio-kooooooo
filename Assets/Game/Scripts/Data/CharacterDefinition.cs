using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    [CreateAssetMenu(menuName = "Code Lyoko/Character Definition", fileName = "CharacterDefinition")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        [Header("Identity")]
        public LyokoCharacter character;
        public string displayName;
        [TextArea(2, 4)] public string canonNotes;

        [Header("Gameplay")]
        public AttackStyle attackStyle;
        public VehicleStyle vehicleStyle;
        public float maxHealth = 120f;
        public float walkSpeed = 7f;
        public float sprintSpeed = 10f;
        public float basicDamage = 20f;
        public float specialDamage = 35f;

        [Header("Production")]
        public bool playableInVerticalSlice;
        public string modelStatus = "Placeholder";
        public string animationStatus = "Not started";
        public string audioStatus = "Not started";
    }
}
