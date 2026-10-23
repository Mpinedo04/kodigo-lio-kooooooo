using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    // Fairness rules for groups of monsters (F06): how many attack at once and how targets are shared.
    [CreateAssetMenu(menuName = "Code Lyoko/Combat Director Profile", fileName = "CombatDirectorProfile")]
    public sealed class CombatDirectorProfile : ScriptableObject
    {
        [Header("Attack slots")]
        [Tooltip("Monsters that may be attacking (wind-up, shot, recovery) at the same time.")]
        public int maxSimultaneousAttackers = 3;
        [Tooltip("Monsters that may be attacking the same target at the same time.")]
        public int maxAttackersPerTarget = 2;
        [Tooltip("A target cannot receive a new attack sooner than this after the previous one started.")]
        public float minSecondsBetweenAttacksOnSameTarget = 0.8f;

        [Header("Target choice")]
        [Tooltip("Distance at which a target's score halves.")]
        public float distanceFalloff = 12f;
        [Tooltip("A target chased by this many monsters starts to look less attractive to the rest.")]
        public int crowdThreshold = 2;
        [Tooltip("Score penalty per monster above the crowd threshold.")]
        public float crowdPenalty = 0.6f;
        [Tooltip("Score bonus for the warrior the player controls (Aelita and clones can outbid it with AggroTarget).")]
        public float controlledWarriorBonus = 1.5f;
        [Tooltip("A new target must score this much better before a monster switches.")]
        public float switchMargin = 1.3f;
        [Tooltip("The current target is kept up to detection range x this.")]
        public float keepTargetRangeMultiplier = 1.4f;

        [Header("Timing")]
        public float perceptionInterval = 0.2f;
        public float retargetInterval = 0.75f;
        [Tooltip("Monsters notice a warrior this close even behind them.")]
        public float closeSenseRadius = 3f;
    }
}
