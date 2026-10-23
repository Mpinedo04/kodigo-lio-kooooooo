using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    // Tuning for the AI companions and the team synergies (Biblia 13.2).
    [CreateAssetMenu(menuName = "Code Lyoko/Squad Profile", fileName = "SquadProfile")]
    public sealed class SquadProfile : ScriptableObject
    {
        [Header("Resources")]
        public float concentrationMax = 100f;
        public float concentrationRegen = 15f;
        public float concentrationRegenDelay = 1f;
        [Tooltip("Yumi is stunned this long when her Concentration runs dry.")]
        public float exhaustionStun = 2f;
        public float flightEnergyMax = 100f;
        public float flightEnergyRegen = 20f;
        public float flightEnergyRegenDelay = 0.5f;

        [Header("AI companions")]
        public float followDistance = 3.5f;
        public float engageRange = 14f;
        [Tooltip("Companions do not chase enemies farther than this from the controlled warrior.")]
        public float leashRange = 20f;
        [Tooltip("A companion this far away (or stuck) appears next to the controlled warrior.")]
        public float teleportDistance = 35f;
        public float meleeAttackInterval = 0.35f;
        public float rangedAttackInterval = 0.55f;
        public float abilityThinkMin = 2f;
        public float abilityThinkMax = 4f;

        [Header("Synergies")]
        [Tooltip("Odd + Aelita: Aelita's energy fields are boosted while Odd's shield is up this close.")]
        public float oddAelitaDistance = 6f;
        public float energyFieldMultiplier = 2f;
        [Tooltip("Yumi + Aelita: Creativity with Yumi this close makes huge bridges and walls.")]
        public float yumiAelitaDistance = 10f;
        public float hugeStructureSize = 2f;
        public float hugeStructureDuration = 1.5f;
        [Tooltip("Yumi + Ulrich: Ulrich's hits on an enemy lifted by Yumi deal this much extra.")]
        public float yumiUlrichDistance = 15f;
        public float liftedHitBonus = 1.5f;
        [Tooltip("Ulrich + William: two heavy hits on the same enemy within this window break its armour.")]
        public float doubleHeavyWindow = 0.6f;
        public float armourBreakSeconds = 3f;
        public float doubleHeavyBonus = 1f;
    }
}
