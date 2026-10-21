using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    // Shared combat feel for the warriors. Per-character damage lives in CharacterDefinition.
    [CreateAssetMenu(menuName = "Code Lyoko/Combat Profile", fileName = "CombatProfile")]
    public sealed class CombatProfile : ScriptableObject
    {
        [Header("Charged attack (hold Attack)")]
        public float chargeTime = 0.5f;
        public float chargedDamageMultiplier = 2.2f;
        public float chargedRadiusMultiplier = 1.4f;
        public float chargedKnockback = 9f;
        public int chargedProjectilePierce = 3;

        [Header("Getting hit")]
        [Tooltip("Invulnerability after a hit; the warrior flickers meanwhile.")]
        public float hitInvulnerability = 0.5f;

        [Header("Hit-stop")]
        public float lightHitStop = 0.03f;
        public float heavyHitStop = 0.07f;

        [Header("Parry (Guard just before the hit)")]
        public float parryWindow = 0.15f;
        public float parryCooldown = 0.4f;
        public float parryHitStop = 0.06f;

        [Header("Special meter")]
        public float meterGainPerDamageDealt = 0.4f;
        public float meterGainPerDamageTaken = 0.6f;

        [Header("Ranged aim")]
        [Tooltip("Shots bend towards an enemy inside this cone (degrees) when nothing is locked.")]
        public float aimAssistAngle = 12f;
        public float aimAssistRange = 30f;
        public float lockOnHomingStrength = 4f;
    }
}
