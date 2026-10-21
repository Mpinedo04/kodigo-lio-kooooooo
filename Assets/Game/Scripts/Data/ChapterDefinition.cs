using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    [CreateAssetMenu(menuName = "Code Lyoko/Chapter Definition", fileName = "ChapterDefinition")]
    public sealed class ChapterDefinition : ScriptableObject
    {
        [Header("Identity")]
        public int chapterNumber;
        public string title;
        public string seasonLabel;
        [TextArea(3, 6)] public string synopsis;

        [Header("Content")]
        public List<MissionDefinition> missions = new List<MissionDefinition>();

        [Header("Unlocks on completion")]
        public List<LyokoCharacter> unlockedCharacters = new List<LyokoCharacter>();
        public List<SectorDefinition> unlockedSectors = new List<SectorDefinition>();
        public List<AbilityDefinition> unlockedAbilities = new List<AbilityDefinition>();
    }
}
