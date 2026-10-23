using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    [Serializable]
    public sealed class MissionStep
    {
        public MissionPhase phase;
        [TextArea(1, 3)] public string objective;
        public MissionStepCondition condition = MissionStepCondition.Immediate;
        [Tooltip("MissionMarker id for ReachMarker steps.")]
        public string markerId;
        [Tooltip("Step ends when alive enemies are at or below this number (EnemiesAtMost).")]
        public int enemyThreshold;
        [Tooltip("The step lasts at least this long, so its objective can be read.")]
        public float minSeconds;
        [Tooltip("Checkpoint id activated when this step starts; Retry brings the team back here.")]
        public string checkpointId;
    }

    // A mission is data: an ordered list of steps that MissionRunner plays in a level.
    [CreateAssetMenu(menuName = "Code Lyoko/Mission Definition", fileName = "MissionDefinition")]
    public sealed class MissionDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string missionId;
        public string displayName;
        public SectorDefinition sector;
        [TextArea(2, 5)] public string briefing;

        [Header("Flow")]
        public List<MissionStep> steps = new List<MissionStep>();
        [TextArea(1, 3)] public string failedObjective = "Todos devirtualizados. Pulsa Reintentar (R / Y).";
        [TextArea(1, 3)] public string aelitaLostObjective = "Aelita ha sido devirtualizada: X.A.N.A. gana. Pulsa Reintentar (R / Y).";
    }
}
