using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    [CreateAssetMenu(menuName = "Code Lyoko/Sector Definition", fileName = "SectorDefinition")]
    public sealed class SectorDefinition : ScriptableObject
    {
        public SectorId sectorId;
        public string displayName;
        public Color themeColor = Color.white;
        [Tooltip("Scene name in Build Settings; empty while the sector has no level.")]
        public string sceneName;
        public AudioClip music;
        public int towerCount;
        [TextArea(2, 4)] public string notes;
    }
}
