using UnityEngine;
using UnityEngine.Serialization;

namespace CodeLyokoFanGame.Data
{
    // Every tunable value of one vehicle. VehicleController holds the shared physics and reads these.
    [CreateAssetMenu(menuName = "Code Lyoko/Vehicle Definition", fileName = "VehicleDefinition")]
    public sealed class VehicleDefinition : ScriptableObject
    {
        [Header("Identity")]
        public VehicleStyle style;
        public string displayName;
        public LyokoCharacter defaultRider;
        [TextArea(2, 4)] public string canonNotes;

        [Header("Speed")]
        [Tooltip("Top speed holding forward (m/s).")]
        public float cruiseSpeed = 16f;
        [Tooltip("Top speed while boosting = cruise speed x this.")]
        [FormerlySerializedAs("speedMultiplier")] public float boostMultiplier = 1.5f;
        public float acceleration = 18f;
        [Tooltip("Speed lost per second when not accelerating.")]
        public float drag = 10f;
        [Tooltip("Share of the speed kept while the nose points away from where you steer (sharp turns slow you down).")]
        [Range(0f, 1f)] public float cornerSpeedFactor = 0.45f;

        [Header("Steering")]
        [Tooltip("Degrees per second at low speed.")]
        public float turnRate = 150f;
        [Tooltip("Turn rate multiplier at top speed (lower = wider turns when fast).")]
        public float highSpeedTurnFactor = 0.6f;
        [Tooltip("How fast the velocity follows the nose (low = slides).")]
        [FormerlySerializedAs("turnSharpness")] public float grip = 10f;
        public float bankAngle = 25f;

        [Header("Flight")]
        public bool canFly = true;
        public float verticalSpeed = 8f;
        public float verticalAcceleration = 24f;
        [Tooltip("0 = altitude only with Ascend/Descend. 1 = also climbs or dives where the camera looks.")]
        [Range(0f, 1f)] public float pitchSteering;
        [Tooltip("Camera pitch (degrees, + = looking up) that means level flight.")]
        public float neutralCameraPitch = -11f;
        public float pitchDeadZone = 4f;
        public float pitchRange = 10f;
        [Tooltip("World height the vehicle cannot climb above.")]
        public float maxAltitude = 45f;

        [Header("Ground (vehicles that cannot fly)")]
        public float gravity = 32f;
        [Tooltip("Keeps the vehicle glued to the ground over bumps and crests closer than this.")]
        public float groundSnapDistance = 1.2f;
        public float jumpForce = 10f;
        [Tooltip("Jump force multiplier when the boost is held (turbo jump).")]
        public float boostJumpMultiplier = 1.4f;

        [Header("Drift")]
        public bool canDrift;
        public float driftTurnMultiplier = 1.7f;
        public float driftGrip = 1.8f;
        public float driftMinSpeed = 8f;
        [Tooltip("Seconds of drifting for the strongest mini-boost.")]
        public float driftFullChargeTime = 1.2f;
        public float driftBoostSpeed = 8f;
        public float driftBoostDuration = 0.9f;

        [Header("Ram")]
        [Tooltip("Damage when hitting an enemy at full speed. 0 = this vehicle does not ram.")]
        public float ramDamage;
        public float ramMinSpeed = 12f;
        public float ramRadius = 1.3f;
        public float ramKnockback = 9f;
        public float ramRehitDelay = 0.6f;
        [Range(0f, 1f)] public float ramSpeedKept = 0.7f;

        [Header("Tricks")]
        public bool canTrick;
        [Tooltip("Tricks only while airborne (a trick still running when you land fails).")]
        public bool tricksOnlyAirborne;
        public float trickDuration = 0.55f;
        public float trickCooldown = 0.1f;
        [Tooltip("Special energy given to the rider per landed trick.")]
        public float trickMeterGain = 8f;
        [Tooltip("Seconds between landings to keep the trick chain.")]
        public float trickComboWindow = 1.6f;
        public float trickComboBonus = 0.5f;
        public float maxTrickMultiplier = 3f;

        [Header("Durability")]
        public float maxHealth = 120f;
        public float hitInvulnerability = 0.25f;
        [Tooltip("Seconds before a destroyed vehicle can be summoned again.")]
        public float respawnCooldown = 6f;
        public float regenPerSecondStored = 4f;
        public float explosionDamage = 20f;
        public float explosionRadius = 4f;

        [Header("Rider")]
        [Tooltip("How high the rider's feet sit above the bottom of the vehicle.")]
        public float seatHeight = 0.6f;
        public bool riderCanShoot = true;
        public bool riderCanUseSpecial = true;
        public float mountDuration = 0.35f;
        public float dismountDuration = 0.3f;
        public float hoverBob = 0.08f;
        public float hoverRate = 5f;

        [Header("Passenger")]
        public bool canCarryPassenger;
        public Vector3 passengerSeatOffset = new Vector3(0f, -0.1f, -0.85f);
        public float passengerBoardRadius = 7f;
        public float passengerFireInterval = 1.1f;
        public float passengerFireRange = 24f;
        public float passengerDamage = 10f;

        [Header("Other")]
        public bool digitalSeaVehicle;

        [Header("Production")]
        public bool appearsInVerticalSlice;
        public GameObject prototypePrefab;
        public GameObject finalModelPrefab;
        public string modelStatus = "Placeholder";
        public string animationStatus = "Not started";
        public string sfxStatus = "Not started";
        public string sourceAssetId;

        public float BoostSpeed => cruiseSpeed * Mathf.Max(1f, boostMultiplier);
    }
}
