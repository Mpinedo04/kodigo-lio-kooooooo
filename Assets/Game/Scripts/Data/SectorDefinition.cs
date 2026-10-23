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

        [Header("Look (F09): sky, fog and colour guide of the sector")]
        public Color skyTop = new Color(0.88f, 0.94f, 0.97f);
        public Color skyHorizon = new Color(0.97f, 0.99f, 1f);
        public Color skyBottom = new Color(0.55f, 0.78f, 0.86f);
        public Color fogColor = new Color(0.88f, 0.95f, 0.97f);
        public float fogDensity = 0.006f;
        public Color ambientColor = new Color(0.62f, 0.7f, 0.72f);
        [Tooltip("Main colour of the ground and terrain.")]
        public Color groundColor = new Color(0.2f, 0.62f, 0.3f);
        [Tooltip("Secondary colour: edges, trunks, rocks.")]
        public Color terrainColor = new Color(0.45f, 0.36f, 0.25f);
        [Tooltip("Glowing details (data lines, crystals).")]
        public Color accentColor = new Color(0.1f, 0.85f, 1f);
        public float bloomIntensity = 0.7f;
        [Range(-100f, 100f)] public float saturation = 12f;
        public Color colorFilter = Color.white;
    }
}
