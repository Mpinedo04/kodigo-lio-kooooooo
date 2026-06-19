using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    [CreateAssetMenu(menuName = "Code Lyoko/Vehicle Definition", fileName = "VehicleDefinition")]
    public sealed class VehicleDefinition : ScriptableObject
    {
        [Header("Identity")]
        public VehicleStyle style;
        public string displayName;
        public LyokoCharacter defaultRider;
        [TextArea(2, 4)] public string canonNotes;

        [Header("Handling")]
        public float speedMultiplier = 1.5f;
        public float acceleration = 18f;
        public float turnSharpness = 10f;
        public bool canFly = true;
        public bool canCarryPassenger;

        [Header("Production")]
        public bool appearsInVerticalSlice;
        public string modelStatus = "Placeholder";
        public string animationStatus = "Not started";
        public string sfxStatus = "Not started";
    }
}
