using UnityEngine;

namespace CodeLyokoFanGame
{
    // Animation made from code for the common rig (F9.11): idle breathing, walk and run cycle, jump and fall,
    // dodge flip, arm motions for each attack style, casting pose, hit flinch, stun wobble, gliding and riding.
    // Works for controlled warriors, AI companions, Ulrich's clones and Aelita as a companion.
    // Real animation clips (Animator Controller, F9.12) can replace it later on the same bones.
    public sealed class ProceduralAnimator : MonoBehaviour
    {
        private const int Hips = 0;
        private const int Spine = 1;
        private const int Chest = 2;
        private const int Head = 3;
        private const int ArmL = 4;
        private const int ArmR = 5;
        private const int ForeL = 6;
        private const int ForeR = 7;
        private const int LegL = 8;
        private const int LegR = 9;
        private const int KneeL = 10;
        private const int KneeR = 11;
        private const int FirstTail = 12;

        [SerializeField] private CharacterRig rig;
        [Tooltip("Metres per step: lower = faster legs.")]
        [SerializeField] private float strideLength = 1.25f;
        [SerializeField] private float legSwing = 40f;
        [SerializeField] private float lightAttackTime = 0.26f;
        [SerializeField] private float heavyAttackTime = 0.42f;

        private CharacterController body;
        private PlayerController player;
        private PlayerCombat combat;
        private AbilityController abilities;
        private Health health;
        private Transform[] bones;
        private Quaternion[] rest;
        private Vector3[] pose;
        private Vector3 hipsRest;
        private Vector3 lastPosition;
        private TrailRenderer weaponTrail;
        private float phase;
        private float speed;
        private float attackTime = -10f;
        private float castTime = -10f;
        private float hitTime = -10f;
        private float dodgeTime = -10f;
        private bool heavyAttack;
        private bool wasDodging;
        private float swingSide = 1f;

        public void SetRig(CharacterRig newRig)
        {
            rig = newRig;
        }

        private void Awake()
        {
            if (rig == null)
            {
                rig = GetComponentInChildren<CharacterRig>();
            }

            body = GetComponent<CharacterController>();
            player = GetComponent<PlayerController>();
            combat = GetComponent<PlayerCombat>();
            abilities = GetComponent<AbilityController>();
            health = GetComponent<Health>();
            if (rig == null)
            {
                enabled = false;
                return;
            }

            bones = new Transform[FirstTail + rig.Tail.Count];
            bones[Hips] = rig.Hips;
            bones[Spine] = rig.Spine;
            bones[Chest] = rig.Chest;
            bones[Head] = rig.Head;
            bones[ArmL] = rig.UpperArmL;
            bones[ArmR] = rig.UpperArmR;
            bones[ForeL] = rig.LowerArmL;
            bones[ForeR] = rig.LowerArmR;
            bones[LegL] = rig.UpperLegL;
            bones[LegR] = rig.UpperLegR;
            bones[KneeL] = rig.LowerLegL;
            bones[KneeR] = rig.LowerLegR;
            for (int i = 0; i < rig.Tail.Count; i++)
            {
                bones[FirstTail + i] = rig.Tail[i];
            }

            rest = new Quaternion[bones.Length];
            pose = new Vector3[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                rest[i] = bones[i] != null ? bones[i].localRotation : Quaternion.identity;
            }

            hipsRest = rig.Hips != null ? rig.Hips.localPosition : Vector3.zero;

            if (combat != null && combat.Style == AttackStyle.Melee && rig.WeaponTip != null)
            {
                weaponTrail = rig.WeaponTip.gameObject.AddComponent<TrailRenderer>();
                weaponTrail.time = 0.16f;
                weaponTrail.startWidth = 0.22f;
                weaponTrail.endWidth = 0f;
                weaponTrail.minVertexDistance = 0.05f;
                weaponTrail.sharedMaterial = LyokoMaterials.Glow(new Color(0.75f, 1f, 0.85f, 0.8f), 0f, 0.6f);
                weaponTrail.emitting = false;
            }
        }

        private void OnEnable()
        {
            lastPosition = transform.position;
            if (combat != null)
            {
                combat.Attacked += OnAttacked;
            }

            if (abilities != null)
            {
                abilities.AbilityUsed += OnAbilityUsed;
            }

            if (health != null)
            {
                health.Damaged += OnDamaged;
            }
        }

        private void OnDisable()
        {
            if (combat != null)
            {
                combat.Attacked -= OnAttacked;
            }

            if (abilities != null)
            {
                abilities.AbilityUsed -= OnAbilityUsed;
            }

            if (health != null)
            {
                health.Damaged -= OnDamaged;
            }
        }

        private void OnAttacked(bool heavy)
        {
            attackTime = Time.time;
            heavyAttack = heavy;
            swingSide = -swingSide;
        }

        private void OnAbilityUsed(Ability ability)
        {
            castTime = Time.time;
        }

        private void OnDamaged(Health damaged, DamageInfo info)
        {
            hitTime = Time.time;
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f)
            {
                return;
            }

            Vector3 velocity = body != null && body.enabled ? body.velocity : (transform.position - lastPosition) / dt;
            lastPosition = transform.position;
            float flatSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            speed = Mathf.Lerp(speed, flatSpeed, 1f - Mathf.Exp(-10f * dt));
            bool grounded = body == null || !body.enabled || body.isGrounded;
            float now = Time.time;

            for (int i = 0; i < pose.Length; i++)
            {
                pose[i] = Vector3.zero;
            }

            Vector3 hipsOffset = Vector3.zero;
            bool mounted = player != null && player.IsMounted;
            if (mounted)
            {
                Ride();
            }
            else if (!grounded)
            {
                Air(velocity.y);
            }
            else
            {
                hipsOffset = Locomotion(dt, now);
            }

            Attack(now);
            Reactions(now);
            Dodge(now);
            Tail(now);
            Apply(hipsOffset);
        }

        private Vector3 Locomotion(float dt, float now)
        {
            float move01 = Mathf.Clamp01(speed / 3f);
            float run01 = Mathf.Clamp01((speed - 4f) / 5f);
            phase += speed / strideLength * dt * Mathf.PI;
            float sin = Mathf.Sin(phase);
            float swing = sin * legSwing * move01 * (1f + run01 * 0.3f);

            pose[LegL].x = swing;
            pose[LegR].x = -swing;
            pose[KneeL].x = 5f + Mathf.Max(0f, -sin) * 55f * move01;
            pose[KneeR].x = 5f + Mathf.Max(0f, sin) * 55f * move01;
            pose[ArmL].x = -swing * 0.8f;
            pose[ArmR].x = swing * 0.8f;
            pose[ArmL].z = -6f;
            pose[ArmR].z = 6f;
            pose[ForeL].x = -12f - run01 * 50f;
            pose[ForeR].x = -12f - run01 * 50f;
            pose[Chest].x = run01 * 12f + Mathf.Sin(now * 2f) * 1.5f * (1f - move01);
            pose[Spine].y = sin * 6f * move01;
            return new Vector3(0f, -Mathf.Abs(sin) * 0.05f * move01, 0f);
        }

        private void Air(float verticalSpeed)
        {
            float rising = Mathf.Clamp(verticalSpeed / 8f, -1f, 1f);
            pose[LegL].x = -35f - rising * 10f;
            pose[LegR].x = -15f;
            pose[KneeL].x = 60f;
            pose[KneeR].x = 35f;
            bool gliding = player != null && player.GlideFallSpeed > 0f;
            if (gliding)
            {
                pose[ArmL].z = -80f;
                pose[ArmR].z = 80f;
                pose[Chest].x = 15f;
                return;
            }

            pose[ArmL].x = -40f;
            pose[ArmR].x = -40f;
            pose[ArmL].z = -25f;
            pose[ArmR].z = 25f;
        }

        private void Ride()
        {
            pose[LegL].x = -60f;
            pose[LegR].x = -60f;
            pose[KneeL].x = 80f;
            pose[KneeR].x = 80f;
            pose[ArmL].x = -40f;
            pose[ArmR].x = -40f;
            pose[Chest].x = 12f;
        }

        // Arm motion per attack style: katana slash, arrow shot, fan throw, energy field.
        private void Attack(float now)
        {
            float duration = heavyAttack ? heavyAttackTime : lightAttackTime;
            float a = (now - attackTime) / duration;
            bool attacking = a >= 0f && a < 1f;
            if (weaponTrail != null)
            {
                weaponTrail.emitting = attacking;
            }

            if (attacking && combat != null)
            {
                float e = Mathf.SmoothStep(0f, 1f, a);
                switch (combat.Style)
                {
                    case AttackStyle.Melee:
                        pose[ArmR] = new Vector3(Mathf.Lerp(-150f, -15f, e), 0f, Mathf.Lerp(20f, -35f, e) * swingSide);
                        pose[ForeR].x = -20f;
                        pose[Chest].y = Mathf.Lerp(-30f, 35f, e) * swingSide * (heavyAttack ? 1.3f : 1f);
                        pose[ArmL].x = -30f;
                        break;
                    case AttackStyle.Telekinetic:
                        pose[ArmR] = new Vector3(-85f, Mathf.Lerp(-70f, 40f, e), 0f);
                        pose[Chest].y = Mathf.Lerp(35f, -25f, e);
                        break;
                    case AttackStyle.Support:
                        pose[ArmL] = new Vector3(-80f, 0f, 15f);
                        pose[ArmR] = new Vector3(-80f, 0f, -15f);
                        pose[Chest].x = -5f;
                        break;
                    default:
                        pose[ArmR] = new Vector3(-88f, 0f, 0f);
                        pose[ForeR].x = 0f;
                        pose[Chest].x = -4f * (1f - e);
                        if (heavyAttack)
                        {
                            pose[ArmL] = new Vector3(-88f, 0f, 10f);
                        }

                        break;
                }
            }

            float c = (now - castTime) / 0.4f;
            if (c >= 0f && c < 1f)
            {
                float lift = Mathf.Sin(c * Mathf.PI);
                pose[ArmL] = new Vector3(-110f * lift, 0f, -30f * lift);
                pose[ArmR] = new Vector3(-110f * lift, 0f, 30f * lift);
                pose[Head].x = -10f * lift;
            }
        }

        private void Reactions(float now)
        {
            float h = (now - hitTime) / 0.3f;
            if (h >= 0f && h < 1f)
            {
                float flinch = 1f - h;
                pose[Chest].x -= 18f * flinch;
                pose[Head].x -= 12f * flinch;
            }

            if (player != null && player.IsStunned)
            {
                pose[Head].z += Mathf.Sin(now * 9f) * 15f;
                pose[Chest].z += Mathf.Sin(now * 9f + 1f) * 5f;
                pose[ArmL] = new Vector3(5f, 0f, -5f);
                pose[ArmR] = new Vector3(5f, 0f, 5f);
            }
        }

        // A quick forward flip around the hips while dodging.
        private void Dodge(float now)
        {
            bool dodging = player != null && player.IsDodging;
            if (dodging && !wasDodging)
            {
                dodgeTime = now;
            }

            wasDodging = dodging;
            float d = (now - dodgeTime) / 0.3f;
            if (d >= 0f && d < 1f)
            {
                pose[Hips].x += Mathf.SmoothStep(0f, 360f, d);
                pose[LegL].x = -70f;
                pose[LegR].x = -70f;
                pose[KneeL].x = 100f;
                pose[KneeR].x = 100f;
            }
        }

        private void Tail(float now)
        {
            float run01 = Mathf.Clamp01(speed / 8f);
            for (int i = FirstTail; i < pose.Length; i++)
            {
                int segment = i - FirstTail;
                pose[i].y = Mathf.Sin(now * 3f + segment * 0.7f) * (12f + run01 * 10f);
                pose[i].x = -8f - run01 * 6f;
            }
        }

        private void Apply(Vector3 hipsOffset)
        {
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] != null)
                {
                    bones[i].localRotation = rest[i] * Quaternion.Euler(pose[i]);
                }
            }

            if (rig.Hips != null)
            {
                rig.Hips.localPosition = hipsRest + hipsOffset;
            }
        }
    }
}
