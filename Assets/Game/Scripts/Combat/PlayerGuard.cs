using System;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Parry: pressing Guard just before a frontal hit cancels it. Ulrich's laser deflection builds on this in F05.
    public sealed class PlayerGuard : MonoBehaviour
    {
        [SerializeField] private CombatProfile profile;

        private readonly ParryWindow window = new ParryWindow();

        public event Action<DamageInfo> Parried;

        public float LastParryTime { get; private set; } = float.NegativeInfinity;

        public void Configure(CombatProfile combatProfile)
        {
            profile = combatProfile;
        }

        public void Press()
        {
            window.TryOpen(Time.time, profile != null ? profile.parryWindow : 0.15f, profile != null ? profile.parryCooldown : 0.4f);
        }

        public bool TryParry(DamageInfo info)
        {
            if (!window.IsOpen(Time.time) || info.type == DamageType.Fall)
            {
                return false;
            }

            // Only hits coming from the front can be parried.
            if (info.direction.sqrMagnitude > 0.0001f && Vector3.Dot(transform.forward, -info.direction.normalized) <= 0f)
            {
                return false;
            }

            LastParryTime = Time.time;
            Parried?.Invoke(info);
            HitStop.Trigger(profile != null ? profile.parryHitStop : 0.06f);
            CameraRig.Shake(0.25f);
            return true;
        }
    }
}
