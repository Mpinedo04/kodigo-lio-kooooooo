using System;
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
        [SerializeField] private int basicBurst = 1;
        [SerializeField] private float basicHoming;
        [SerializeField] private float basicProjectileSpeed = 1f;
        [SerializeField] private float specialStagger;
        [SerializeField] private float specialExplosionRadius;

        private readonly ComboTracker comboTracker = new ComboTracker();
        private List<ComboStep> fallbackSteps;
        private PlayerController owner;
        private LockOnSystem lockOn;
        private SpecialMeter meter;
        private Transform aimOverride;
        private float basicTimer;
        private float specialTimer;
        private float chargeStart = -1f;
        private int burstRemaining;
        private float burstTimer;
        private int fansOut;

        // Every swing or shot (true = heavy); the procedural animator plays the arm motion.
        public event Action<bool> Attacked;

        public SpecialMeter Meter => meter;
        public AttackStyle Style => attackStyle;
        public Projectile ProjectilePrefab => projectilePrefab;
        public float BasicDamage => basicDamage;
        public bool IsCharging => chargeStart >= 0f && Time.time - chargeStart > 0.15f;
        public float ChargeProgress => chargeStart < 0f ? 0f : Mathf.Clamp01((Time.time - chargeStart) / profile.chargeTime);
        public int CurrentComboStep => comboTracker.CurrentStep;
        public int FansAvailable => Mathf.Max(0, profile.maxFans - fansOut);

        private IReadOnlyList<ComboStep> Steps => combo != null && combo.steps.Count > 0 ? combo.steps : fallbackSteps;

        private Vector3 ShotOrigin => attackOrigin != null
            ? attackOrigin.position
            : transform.position + Vector3.up * 1.3f + transform.forward * 0.4f;

        // The AI companion aims at its own enemy; the player aims with the lock-on.
        private Transform CurrentTarget
        {
            get
            {
                if (aimOverride != null && aimOverride.gameObject.activeInHierarchy)
                {
                    return aimOverride;
                }

                bool controlled = owner == null || owner.IsControlled;
                return controlled && lockOn != null ? lockOn.CurrentTarget : null;
            }
        }

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
            basicBurst = Mathf.Max(1, definition.basicBurst);
            basicHoming = definition.basicHoming;
            basicProjectileSpeed = definition.basicProjectileSpeed > 0f ? definition.basicProjectileSpeed : 1f;
            specialStagger = definition.specialStagger;
            specialExplosionRadius = definition.specialExplosionRadius;
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

            owner = GetComponent<PlayerController>();
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

            if (burstRemaining > 0)
            {
                burstTimer -= Time.deltaTime;
                if (burstTimer <= 0f)
                {
                    burstRemaining--;
                    burstTimer = profile.burstInterval;
                    FireBasicShot();
                }
            }
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

        public void SetAimOverride(Transform target)
        {
            aimOverride = target;
        }

        // A charge or combo must not survive a character switch.
        private void OnDisable()
        {
            CancelCharge();
            comboTracker.Reset();
            burstRemaining = 0;
            fansOut = 0;
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
            Attacked?.Invoke(false);

            if (attackStyle == AttackStyle.Telekinetic)
            {
                ThrowFan(basicDamage, 0f, 1f, false);
                return;
            }

            FireBasicShot();
            burstRemaining = basicBurst - 1;
            burstTimer = profile.burstInterval;
        }

        public void SpecialAttack()
        {
            if (specialTimer > 0f)
            {
                return;
            }

            switch (attackStyle)
            {
                case AttackStyle.Melee:
                    // Charged slash: heavy, so it cracks guards and shields.
                    HeavyFeedback(Strike(specialDamage, 1.45f, 0.45f, 6f, true));
                    break;
                case AttackStyle.Telekinetic:
                    // Both fans at once in a wide arc; needs both of them back in hand.
                    if (FansAvailable < 2)
                    {
                        return;
                    }

                    ThrowFan(specialDamage * 0.5f, -profile.fanSpread, 1f, false);
                    ThrowFan(specialDamage * 0.5f, profile.fanSpread, 1f, false);
                    break;
                case AttackStyle.Support:
                    // Aelita's charged energy field: bursts in an area.
                    float boost = SupportBoost();
                    FireProjectile(specialDamage * boost, 0f, 0, 1.6f * Mathf.Sqrt(boost), true, basicHoming, specialExplosionRadius * Mathf.Sqrt(boost), basicProjectileSpeed * 0.8f, 0f);
                    break;
                case AttackStyle.Operator:
                    FireProjectile(specialDamage * 0.5f, 0f, 0, 1f, false);
                    break;
                default:
                    // Odd's charged arrow: pierces and stuns.
                    FireProjectile(specialDamage, 0f, profile.chargedProjectilePierce, 1.5f, true, 0f, 0f, 1.3f, specialStagger);
                    break;
            }

            specialTimer = specialCooldown;
            Attacked?.Invoke(true);
        }

        private void ChargedAttack()
        {
            Attacked?.Invoke(true);
            float damage = basicDamage * profile.chargedDamageMultiplier;
            switch (attackStyle)
            {
                case AttackStyle.Melee:
                    HeavyFeedback(Strike(damage, profile.chargedRadiusMultiplier, 0.4f, profile.chargedKnockback, true));
                    return;
                case AttackStyle.Telekinetic:
                    ThrowFan(damage, 0f, 1.4f, true);
                    return;
                case AttackStyle.Support:
                    float boost = SupportBoost();
                    FireProjectile(damage * boost, 0f, 0, 1.4f * Mathf.Sqrt(boost), true, basicHoming, specialExplosionRadius * 0.75f, basicProjectileSpeed, 0f);
                    return;
                default:
                    FireProjectile(damage, 0f, profile.chargedProjectilePierce, 1.7f, true);
                    return;
            }
        }

        private void FireBasicShot()
        {
            float boost = attackStyle == AttackStyle.Support ? SupportBoost() : 1f;
            FireProjectile(basicDamage * boost, 0f, 0, Mathf.Sqrt(boost), false, basicHoming, 0f, basicProjectileSpeed, 0f);
        }

        // Odd + Aelita synergy: behind Odd's shield, Aelita's energy fields are doubled.
        private float SupportBoost()
        {
            return TeamSynergy.EnergyFieldMultiplier(transform.position);
        }

        private void PerformComboStep(int index)
        {
            ComboStep step = Steps[index];
            Attacked?.Invoke(step.heavy);
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

        private void ThrowFan(float damage, float curve, float rangeMultiplier, bool heavy)
        {
            if (fansOut >= profile.maxFans)
            {
                return;
            }

            Vector3 origin = ShotOrigin;
            Vector3 direction = AimDirection(origin, Mathf.Approximately(curve, 0f));
            DamageInfo info = new DamageInfo
            {
                amount = damage,
                type = DamageType.Projectile,
                source = gameObject,
                sourceTeam = Team.Player,
                knockback = heavy ? 5f : 1.5f,
                isHeavy = heavy
            };

            fansOut++;
            BoomerangFan.Throw(this, origin, direction, info, profile.fanRange * rangeMultiplier, profile.fanSpeed, curve);
        }

        public void OnFanReturned()
        {
            fansOut = Mathf.Max(0, fansOut - 1);
        }

        private Vector3 AimDirection(Vector3 origin, bool allowAssist)
        {
            Transform target = CurrentTarget;
            if (target != null)
            {
                return (target.position - origin).normalized;
            }

            Vector3 direction = transform.forward;
            if (allowAssist)
            {
                Transform assisted = FindAimAssistTarget(origin, direction);
                if (assisted != null)
                {
                    direction = (assisted.position - origin).normalized;
                }
            }

            return direction;
        }

        private void FireProjectile(float damage, float yawOffset, int pierce, float scale, bool heavy, float homing = 0f, float explosionRadius = 0f, float speed = 1f, float stagger = 0f)
        {
            if (projectilePrefab == null)
            {
                return;
            }

            Vector3 origin = ShotOrigin;
            Transform target = CurrentTarget;
            Transform homingTarget = target;
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
                    if (homing > 0f)
                    {
                        homingTarget = assisted;
                    }
                }
            }

            float homingStrength = homingTarget == null ? 0f : Mathf.Max(homing, target != null ? profile.lockOnHomingStrength : 0f);

            DamageInfo info = new DamageInfo
            {
                amount = damage,
                type = DamageType.Projectile,
                source = gameObject,
                sourceTeam = Team.Player,
                knockback = heavy ? 6f : 1f,
                isHeavy = heavy,
                stagger = stagger
            };

            Projectile projectile = PrefabPool.Spawn(projectilePrefab, origin + direction * 0.5f, Quaternion.LookRotation(direction));
            projectile.Launch(gameObject, Team.Player, direction, info, homingTarget, homingStrength, pierce, scale, explosionRadius, speed);
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
