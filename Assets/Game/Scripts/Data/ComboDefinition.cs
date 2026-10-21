using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    [Serializable]
    public sealed class ComboStep
    {
        public string stepName = "Golpe";
        public float damageMultiplier = 1f;
        public float radiusMultiplier = 1f;
        [Tooltip("Extra reach in front of the hitbox (meters).")]
        public float forwardOffset;
        [Tooltip("How long the swing lasts before a new combo can start.")]
        public float duration = 0.35f;
        [Tooltip("Seconds after the swing starts when the next press chains (earlier presses are buffered).")]
        public float chainWindowStart = 0.15f;
        [Tooltip("After this, the combo resets to the first step.")]
        public float chainWindowEnd = 0.6f;
        public float knockback = 2f;
        public bool heavy;
    }

    // Melee chain, e.g. Ulrich's three katana strikes.
    [CreateAssetMenu(menuName = "Code Lyoko/Combo Definition", fileName = "ComboDefinition")]
    public sealed class ComboDefinition : ScriptableObject
    {
        public List<ComboStep> steps = new List<ComboStep>();
    }
}
