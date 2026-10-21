using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    [Serializable]
    public sealed class CharacterOutfit
    {
        [Tooltip("For example: T1-T3, T4, Evolution, Garage Kids.")]
        public string label;
        public GameObject prefab;
        public int unlockChapter;
    }

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
        public bool playable = true;
        public bool companion;
        public bool operatorOnly;
        public float maxHealth = 100f;
        public float walkSpeed = 7f;
        public float sprintSpeed = 10f;
        public float jumpForce = 8f;
        public float dodgeSpeed = 18f;
        public float basicDamage = 20f;
        public float specialDamage = 35f;
        public float basicCooldown = 0.28f;
        public float specialCooldown = 1.6f;

        [Tooltip("Melee chain for Melee attack style (Ulrich, William).")]
        public ComboDefinition meleeCombo;

        [Header("Movement traits")]
        public bool canDoubleJump;
        public bool canWallClimb;
        public bool canLongJump;
        public bool canGlide;

        [Header("Abilities")]
        public List<AbilityDefinition> abilities = new List<AbilityDefinition>();

        [Header("Presentation")]
        public Sprite portrait;
        public Sprite icon;
        public List<CharacterOutfit> outfits = new List<CharacterOutfit>();
        public List<AudioClip> voiceLines = new List<AudioClip>();

        [Header("Production")]
        public bool playableInVerticalSlice;
        public GameObject prototypePrefab;
        public GameObject finalModelPrefab;
        public string modelStatus = "Placeholder";
        public string animationStatus = "Not started";
        public string audioStatus = "Not started";
        public string sourceAssetId;
    }
}
