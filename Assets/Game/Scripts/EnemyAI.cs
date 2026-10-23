using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // The monster component everything else talks to (lock-on, aim assist, missions, abilities).
    // Thin host: the state machine is EnemyBrain, per-monster rules are EnemyBehaviour (Enemies/).
    [RequireComponent(typeof(Health))]
    public sealed class EnemyAI : MonoBehaviour, IKnockbackReceiver
    {
        private static readonly List<EnemyAI> Active = new List<EnemyAI>();

        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private EnemyKind kind = EnemyKind.Kankrelat;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform aimPoint;
        [SerializeField] private WeakPoint weakPoint;
        [Tooltip("Main body mesh: rolls, opens, falls (optional).")]
        [SerializeField] private Transform bodyVisual;

        private Health health;
        private EnemyDefinition activeDefinition;
        private EnemyDefinition runtimeFallback;
        private EnemyBehaviour behaviour;
        private EnemyMotor motor;
        private EnemyBrain brain;
        private EnemyTelegraph telegraph;
        private bool initialized;
        private bool started;
        private bool subscribed;
        private bool lastExposed;
        private float forcedExposeTimer;

        public static IReadOnlyList<EnemyAI> All => Active;

        public EnemyKind Kind => kind;
        public Transform AimPoint => aimPoint;
        public Transform FirePoint => firePoint;
        public Projectile ProjectilePrefab => projectilePrefab;
        public Transform BodyVisual => bodyVisual;
        public WeakPoint WeakPoint => weakPoint;
        public Health Health => health != null ? health : health = GetComponent<Health>();
        public EnemyDefinition Definition
        {
            get
            {
                EnsureInitialized();
                return activeDefinition;
            }
        }

        public EnemyStateId CurrentState => brain != null ? brain.State : EnemyStateId.Idle;
        public Health Target => brain != null ? brain.Target : null;
        public bool IsAttacking => brain != null && brain.IsAttacking;
        public bool IsDead => Health == null || Health.IsDead || CurrentState == EnemyStateId.Dead;
        // Monsters built without a projectile (lock-on dummies) never attack.
        public bool IsArmed => projectilePrefab != null;
        public bool IsSuspended { get; private set; }
        public bool IsMaterializing { get; private set; }
        public bool CanBeLifted => Definition != null && Definition.liftableByTelekinesis && !IsDead;
        public bool IsWeakPointForced => forcedExposeTimer > 0f;
        public bool IsGuarded => behaviour != null && behaviour.IsGuarded;

        internal EnemyBrain Brain
        {
            get
            {
                EnsureInitialized();
                return brain;
            }
        }

        internal EnemyTelegraph Telegraph => telegraph;

        // ---- Setup ----------------------------------------------------------------------------------

        public void Configure(EnemyDefinition newDefinition, Projectile projectile)
        {
            definition = newDefinition;
            if (definition != null)
            {
                kind = definition.kind;
            }

            projectilePrefab = projectile;
            Reinitialize();
        }

        public void Configure(EnemyKind newKind, Projectile projectile)
        {
            kind = newKind;
            definition = EnemyCatalog.Find(newKind);
            projectilePrefab = projectile;
            Reinitialize();
        }

        public void SetAnchors(Transform fire, Transform aim)
        {
            firePoint = fire != null ? fire : transform;
            aimPoint = aim != null ? aim : transform;
        }

        public void SetBodyVisual(Transform body)
        {
            bodyVisual = body;
        }

        public Transform FindPart(string partName)
        {
            return transform.Find(partName);
        }

        // ---- Abilities and synergies (F05) --------------------------------------------------------------

        // Stunned for that long (no movement, no attacks). Scaled by EnemyDefinition.staggerMultiplier (0 = immune).
        public void Stagger(float seconds)
        {
            EnsureInitialized();
            float scaled = seconds * Mathf.Max(0f, activeDefinition.staggerMultiplier);
            if (scaled > 0f)
            {
                brain.Stagger(scaled, false);
            }
        }

        // Yumi's telekinesis: the brain pauses and external code moves the transform freely.
        // On release ground units fall, land and snap back onto the NavMesh.
        public void SetSuspended(bool suspended)
        {
            EnsureInitialized();
            if (IsSuspended == suspended || IsDead)
            {
                return;
            }

            if (suspended)
            {
                brain.Interrupt();
                telegraph.End();
                CombatDirector director = CombatDirector.Current;
                if (director != null)
                {
                    director.EndAttack(this);
                }
            }

            IsSuspended = suspended;
            motor.SetSuspended(suspended);
            behaviour.OnSuspended(suspended);
        }

        // Forces the eye of X.A.N.A. open (team synergies cracking a Megatank).
        public void ExposeWeakPoint(float seconds)
        {
            EnsureInitialized();
            forcedExposeTimer = Mathf.Max(forcedExposeTimer, seconds);
            UpdateWeakPoint();
        }

        // Makes an idle monster go after a target (ally warnings, scripted events). False if it already has one.
        public bool Alert(Health target)
        {
            EnsureInitialized();
            return brain.Alert(target);
        }

        public void ApplyKnockback(Vector3 velocity)
        {
            EnsureInitialized();
            if (!IsMaterializing)
            {
                motor.AddKnockback(velocity * activeDefinition.knockbackScale);
            }
        }

        internal void SetMaterializing(bool value)
        {
            EnsureInitialized();
            IsMaterializing = value;
        }

        internal void NotifyPartDamaged(DamageInfo info)
        {
            EnsureInitialized();
            brain.NotifyDamaged(info);
        }

        // Advances the monster by hand (EditMode tests).
        internal void Tick(float deltaTime)
        {
            EnsureInitialized();
            if (IsDead)
            {
                return;
            }

            forcedExposeTimer = Mathf.Max(0f, forcedExposeTimer - deltaTime);
            telegraph.Tick(deltaTime);
            brain.Tick(deltaTime);
            if (!brain.Paused)
            {
                behaviour.Tick(deltaTime);
            }

            motor.Tick(deltaTime);
            UpdateWeakPoint();
        }

        // ---- Unity ------------------------------------------------------------------------------------

        private void Awake()
        {
            Health.SetTeam(Team.Enemy);
            Subscribe();
        }

        private void OnEnable()
        {
            if (!Active.Contains(this))
            {
                Active.Add(this);
            }
        }

        private void OnDisable()
        {
            Active.Remove(this);
            CombatDirector director = CombatDirector.Current;
            if (director != null)
            {
                director.Forget(this);
            }
        }

        private void Start()
        {
            EnsureInitialized();
            motor.Initialize();
            started = true;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            Unsubscribe();
            if (telegraph != null)
            {
                telegraph.End();
            }

            DestroyFallback();
        }

        // ---- Internals ----------------------------------------------------------------------------------

        private void EnsureInitialized()
        {
            if (!initialized)
            {
                Initialize();
            }
        }

        private void Reinitialize()
        {
            if (initialized)
            {
                brain.Dispose();
                behaviour.OnDeath();
                initialized = false;
            }

            Initialize();
            if (started)
            {
                motor.Initialize();
            }
        }

        private void Initialize()
        {
            initialized = true;
            Subscribe();

            if (firePoint == null)
            {
                firePoint = transform;
            }

            if (aimPoint == null)
            {
                aimPoint = transform;
            }

            activeDefinition = definition;
            if (activeDefinition == null)
            {
                DestroyFallback();
                runtimeFallback = EnemyDefaults.CreateRuntime(kind);
                activeDefinition = runtimeFallback;
            }
            else
            {
                kind = activeDefinition.kind;
                EnemyCatalog.Register(activeDefinition);
            }

            Health.SetTeam(Team.Enemy);
            Health.SetMaxHealth(activeDefinition.maxHealth);
            ConfigureWeakPoint();

            telegraph = new EnemyTelegraph(this);
            telegraph.Bind(weakPoint != null ? weakPoint.GetComponent<Renderer>() : null);
            motor = new EnemyMotor(this, activeDefinition.flying);
            behaviour = EnemyBehaviour.Create(kind);
            brain = new EnemyBrain(this, behaviour, motor, new EnemyPerception(this));
            behaviour.Bind(this, brain, motor);
            lastExposed = !behaviour.IsEyeExposed;
            UpdateWeakPoint();
        }

        private void ConfigureWeakPoint()
        {
            if (weakPoint == null)
            {
                weakPoint = GetComponentInChildren<WeakPoint>(true);
            }

            if (weakPoint == null)
            {
                return;
            }

            if (weakPoint.Target == null)
            {
                weakPoint.SetTarget(Health);
            }

            weakPoint.Configure(activeDefinition.weakPointDamageMultiplier, activeDefinition.weakPointAlwaysExposed, activeDefinition.weakPointInstantKill);
        }

        private void UpdateWeakPoint()
        {
            if (weakPoint == null || behaviour == null || brain == null)
            {
                return;
            }

            bool exposed = !IsDead && (behaviour.IsEyeExposed || forcedExposeTimer > 0f || (brain.IsStaggered && brain.StaggerCracked));
            if (exposed != lastExposed || exposed != weakPoint.IsExposed)
            {
                lastExposed = exposed;
                weakPoint.SetExposed(exposed);
            }
        }

        private void Subscribe()
        {
            if (subscribed || Health == null)
            {
                return;
            }

            subscribed = true;
            Health.Damaged += OnDamaged;
            Health.Died += OnDied;
        }

        private void Unsubscribe()
        {
            if (!subscribed || health == null)
            {
                return;
            }

            subscribed = false;
            health.Damaged -= OnDamaged;
            health.Died -= OnDied;
        }

        // Heavy hits stagger small and medium monsters and crack the guard of guarded ones.
        private void OnDamaged(Health damaged, DamageInfo info)
        {
            EnsureInitialized();
            brain.NotifyDamaged(info);
            if (!info.isHeavy || IsDead)
            {
                return;
            }

            if (behaviour.IsGuarded && activeDefinition.guardCrackDuration > 0f)
            {
                brain.Stagger(activeDefinition.guardCrackDuration, true);
            }
            else if (activeDefinition.heavyHitStagger > 0f)
            {
                Stagger(activeDefinition.heavyHitStagger);
            }
        }

        private void OnDied(Health dead)
        {
            EnsureInitialized();
            brain.Die();
            if (weakPoint != null)
            {
                weakPoint.SetExposed(false);
            }

            GameManager.RaiseEvent(GameEventId.EnemyKilled);
            EnemyDevirtualize.Spawn(this, activeDefinition.dataDrop);
        }

        private void DestroyFallback()
        {
            if (runtimeFallback == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(runtimeFallback);
            }
            else
            {
                DestroyImmediate(runtimeFallback);
            }

            runtimeFallback = null;
        }
    }
}
