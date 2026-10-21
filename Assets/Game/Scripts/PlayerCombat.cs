using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    public sealed class PlayerCombat : MonoBehaviour
    {
        private static readonly Collider[] AimBuffer = new Collider[48];
        private static readonly List<Vector3> AimPositions = new List<Vector3>();
        private static readonly List<Transform> AimTargets = new List<Transform>();

        [SerializeField] private AttackStyle attackStyle = AttackStyle.Ranged;
        [SerializeField] private Projectile projectilePrefab;
        [Tooltip("Where shots come from. Empty = chest height in front of the warrior.")]
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private Hitbox hitbox;
        [SerializeField] private CombatProfile profile;
        [SerializeField] private ComboDefinition combo;
        [SerializeField] private float basicDamage = 20f;
        [SerializeField] private float specialDamage = 35f;
        [SerializeField] private float meleeRadius = 2.2f;
        [SerializeField] private float basicCooldown = 0.28f;
        [SerializeField] private float specialCooldown = 1.6f;

        private readonly ComboTracker comboTracker = new ComboTracker();
        private List<ComboStep> fallbackSteps;
        private LockOnSystem lockOn;
        private SpecialMeter meter;
        private float basicTimer;
        private float specialTimer;
        private float chargeStart = -1f;

        public SpecialMeter Meter => meter;
        public bool IsCharging => chargeStart >= 0f && Time.time - chargeStart > 0.15f;
        public float ChargeProgress => chargeStart < 0f ? 0f : Mathf.Clamp01((Time.time - chargeStart) / profile.chargeTime);
        public int CurrentComboStep => comboTracker.CurrentStep;

        private IReadOnlyList<ComboStep> Steps => combo != null && combo.steps.Count > 0 ? combo.steps : fallbackSteps;

        private Vector3 ShotOrigin => attackOrigin != null
            ? attackOrigin.position
            : transform.position + Vector3.up * 1.3f + transform.forward * 0.4f;

        public void ApplyDefinition(CharacterDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            attackStyle = definition.attackStyle;
            basicDamage = definition.basicDamage;
            specialDamage = definition.specialDamage;
            basicCooldown = definition.basicCooldown;
            specialCooldown = definition.specialCooldown;
            if (definition.meleeCombo != null)
            {
                combo = definition.meleeCombo;
            }
        }

        public void Configure(AttackStyle style, Projectile projectile, LockOnSystem targeter)
        {
            attackStyle = style;
            projectilePrefab = projectile;
            lockOn = targeter;
        }

        private void Awake()
        {
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CombatProfile>();
            }

            fallbackSteps = new List<ComboStep> { new ComboStep { stepName = "Golpe", duration = basicCooldown, chainWindowStart = basicCooldown, chainWindowEnd = basicCooldown + 0.3f } };

            if (TryGetComponent(out Health health))
            {
                health.SetTeam(Team.Player);
                health.SetHitInvulnerability(profile.hitInvulnerability);
            }

            if (!TryGetComponent(out meter))
            {
                meter = gameObject.AddComponent<SpecialMeter>();
            }

            meter.Configure(profile.meterGainPerDamageDealt, profile.meterGainPerDamageTaken);

            if (hitbox == null)
            {
                GameObject hitboxObject = new GameObject("MeleeHitbox");
                hitboxObject.transform.SetParent(transform, false);
                hitboxObject.transform.localPosition = new Vector3(0f, 1f, 1.4f);
                hitbox = hitboxObject.AddComponent<Hitbox>();
                hitbox.Configure(meleeRadius);
            }

            if (TryGetComponent(out PlayerGuard guard))
            {
                guard.Configure(profile);
            }
        }

        private void Update()
        {
            basicTimer -= Time.deltaTime;
            specialTimer -= Time.deltaTime;
        }

        // Called every frame on foot: press = attack (or next combo step), hold and release = charged attack.
        public void HandleAttackInput(bool pressed, bool held)
        {
            float now = Time.time;

            if (pressed)
            {
                BasicAttack();
                chargeStart = now;
            }

            if (chargeStart >= 0f && !held)
            {
                if (now - chargeStart >= profile.chargeTime)
                {
                    ChargedAttack();
                }

                chargeStart = -1f;
            }

            if (attackStyle == AttackStyle.Melee)
            {
                int buffered = comboTracker.Tick(now, Steps);
                if (buffered >= 0)
                {
                    PerformComboStep(buffered);
                }
            }
        }

        public void CancelCharge()
        {
            chargeStart = -1f;
        }

        public void SetLockOn(LockOnSystem targeter)
        {
            lockOn = targeter;
        }

        // A charge or combo must not survive a character switch.
        private void OnDisable()
        {
            CancelCharge();
            comboTracker.Reset();
        }

        public void BasicAttack()
        {
            if (attackStyle == AttackStyle.Melee)
            {
                int step = comboTracker.Press(Time.time, Steps);
                if (step >= 0)
                {
                    PerformComboStep(step);
                }

                return;
            }

            if (basicTimer > 0f)
            {
                return;
            }

            basicTimer = basicCooldown;
            FireProjectile(basicDamage, 0f, 0, 1f, false);
        }

        public void SpecialAttack()
        {
            if (specialTimer > 0f)
            {
                return;
            }

            specialTimer = specialCooldown;

            switch (attackStyle)
            {
                case AttackStyle.Melee:
                    HeavyFeedback(Strike(specialDamage, 1.45f, 0.45f, 6f, true));
                    break;
                case AttackStyle.Telekinetic:
                    HeavyFeedback(Strike(specialDamage, 2.2f, 0.8f, 8f, true));
                    break;
                case AttackStyle.Support:
                    Strike(specialDamage * 0.65f, 1.8f, 0.2f, 3f, false);
                    break;
                case AttackStyle.Operator:
                    FireProjectile(specialDamage * 0.5f, 0f, 0, 1f, false);
                    break;
                default:
                    FireProjectile(specialDamage, -8f, 0, 1f, false);
                    FireProjectile(specialDamage, 0f, 0, 1f, false);
                    FireProjectile(specialDamage, 8f, 0, 1f, false);
                    break;
            }
        }

        private void ChargedAttack()
        {
            float damage = basicDamage * profile.chargedDamageMultiplier;
            if (attackStyle == AttackStyle.Melee)
            {
                HeavyFeedback(Strike(damage, profile.chargedRadiusMultiplier, 0.4f, profile.chargedKnockback, true));
                return;
            }

            FireProjectile(damage, 0f, profile.chargedProjectilePierce, 1.7f, true);
        }

        private void PerformComboStep(int index)
        {
            ComboStep step = Steps[index];
            int hits = Strike(basicDamage * step.damageMultiplier, step.radiusMultiplier, step.forwardOffset, step.knockback, step.heavy);
            if (hits <= 0)
            {
                return;
            }

            if (step.heavy)
            {
                HeavyFeedback(hits);
            }
            else
            {
                HitStop.Trigger(profile.lightHitStop);
            }
        }

        private int Strike(float damage, float radiusMultiplier, float forwardOffset, float knockback, bool heavy)
        {
            DamageInfo info = new DamageInfo
            {
                amount = damage,
                type = DamageType.Melee,
                source = gameObject,
                sourceTeam = Team.Player,
                direction = transform.forward,
                knockback = knockback,
                isHeavy = heavy
            };

            return hitbox.Strike(info, Team.Player, radiusMultiplier, forwardOffset, heavy);
        }

        private void HeavyFeedback(int hits)
        {
            if (hits <= 0)
            {
                return;
            }

            HitStop.Trigger(profile.heavyHitStop);
            CameraRig.Shake(0.4f);
        }

        private void FireProjectile(float damage, float yawOffset, int pierce, float scale, bool heavy)
        {
            if (projectilePrefab == null)
            {
                return;
            }

            Vector3 origin = ShotOrigin;
            Transform target = lockOn != null ? lockOn.CurrentTarget : null;
            Vector3 direction = Quaternion.Euler(0f, yawOffset, 0f) * transform.forward;

            if (target != null)
            {
                direction = Quaternion.Euler(0f, yawOffset, 0f) * (target.position - origin).normalized;
            }
            else if (Mathf.Approximately(yawOffset, 0f))
            {
                Transform assisted = FindAimAssistTarget(origin, direction);
                if (assisted != null)
                {
                    direction = (assisted.position - origin).normalized;
                }
            }

            DamageInfo info = new DamageInfo
            {
                amount = damage,
                type = DamageType.Projectile,
                source = gameObject,
                sourceTeam = Team.Player,
                knockback = heavy ? 6f : 1f,
                isHeavy = heavy
            };

            Projectile projectile = PrefabPool.Spawn(projectilePrefab, origin + direction * 0.5f, Quaternion.LookRotation(direction));
            projectile.Launch(gameObject, Team.Player, direction, info, target, target != null ? profile.lockOnHomingStrength : 0f, pierce, scale);
        }

        private Transform FindAimAssistTarget(Vector3 origin, Vector3 direction)
        {
            AimPositions.Clear();
            AimTargets.Clear();
            int count = Physics.OverlapSphereNonAlloc(origin, profile.aimAssistRange, AimBuffer, Physics.AllLayers, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                EnemyAI enemy = AimBuffer[i].GetComponentInParent<EnemyAI>();
                if (enemy == null)
                {
                    continue;
                }

                Transform aim = enemy.AimPoint != null ? enemy.AimPoint : enemy.transform;
                if (!AimTargets.Contains(aim))
                {
                    AimTargets.Add(aim);
                    AimPositions.Add(aim.position);
                }
            }

            int best = TargetingMath.PickAimAssist(origin, direction, AimPositions, profile.aimAssistAngle, profile.aimAssistRange);
            return best >= 0 ? AimTargets[best] : null;
        }
    }
}
