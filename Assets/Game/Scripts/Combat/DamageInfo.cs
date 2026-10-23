using UnityEngine;

namespace CodeLyokoFanGame
{
    // Everything a hit carries: how much, from whom, what kind, where and how hard it pushes.
    public struct DamageInfo
    {
        public float amount;
        public DamageType type;
        public GameObject source;
        public Team sourceTeam;
        public Vector3 point;
        public Vector3 direction;
        public bool isCritical;
        public bool isHeavy;
        public float knockback;
        // Seconds the target is stunned (Odd's charged arrow); 0 = none.
        public float stagger;
        public bool ignoreInvulnerability;

        public static DamageInfo Simple(float amount, GameObject source = null, DamageType type = DamageType.Environment)
        {
            return new DamageInfo { amount = amount, source = source, type = type };
        }

        public DamageInfo WithAmount(float newAmount)
        {
            DamageInfo copy = this;
            copy.amount = newAmount;
            return copy;
        }
    }
}
