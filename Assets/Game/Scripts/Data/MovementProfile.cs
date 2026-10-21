using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    // Shared movement feel. Per-character speeds and traits live in CharacterDefinition.
    [CreateAssetMenu(menuName = "Code Lyoko/Movement Profile", fileName = "MovementProfile")]
    public sealed class MovementProfile : ScriptableObject
    {
        [Header("Ground")]
        public float acceleration = 55f;
        public float deceleration = 70f;
        [Tooltip("Degrees per second.")]
        public float turnSpeed = 720f;

        [Header("Air")]
        public float airAcceleration = 22f;

        [Header("Jump")]
        public float gravity = -26f;
        [Tooltip("Gravity multiplier while falling, so jumps feel less floaty.")]
        public float fallGravityMultiplier = 1.8f;
        public float maxFallSpeed = 40f;
        [Tooltip("Upward speed is multiplied by this when jump is released early (variable height).")]
        [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f;
        [Tooltip("Seconds after leaving a ledge during which a jump still works.")]
        public float coyoteTime = 0.1f;
        [Tooltip("Seconds a jump press is remembered before landing.")]
        public float jumpBufferTime = 0.12f;

        [Header("Dodge")]
        public float dodgeDuration = 0.2f;
        public float dodgeInvulnerability = 0.2f;
        public float dodgeCooldown = 0.5f;

        [Header("Edge protection")]
        [Tooltip("Stop at ledges while walking (sprinting or jumping still goes over).")]
        public bool edgeProtection = true;
        public float edgeCheckDistance = 0.35f;
        public float edgeDropHeight = 1.5f;

        [Header("Character traits")]
        public float doubleJumpForce = 7.5f;
        public int maxAirJumps = 1;
        public float wallCheckDistance = 0.5f;
        public float wallClimbSpeed = 4.5f;
        public float wallClimbMaxTime = 1.2f;
        public float longJumpUpForce = 7f;
        public float longJumpForwardSpeed = 15f;

        [Header("Vehicles")]
        public float vehicleVerticalSpeed = 6f;
    }
}
