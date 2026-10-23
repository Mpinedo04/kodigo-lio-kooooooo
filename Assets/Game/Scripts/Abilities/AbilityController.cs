using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Ability 1, Ability 2 and Ultimate of a warrior (F5.1): unlocks, cooldowns, costs, resources and input.
    // The AI companion uses TryUse with AiTarget set.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController))]
    public sealed class AbilityController : MonoBehaviour, IDamageInterceptor
    {
        // F1 debug menu: ignore the chapter locks everywhere.
        public static bool UnlockAllOverride;

        [Tooltip("Ignore the chapter locks (test scenes).")]
        [SerializeField] private bool unlockAll;
        [Tooltip("Gamepad: LB alone is Ability 1 but LB+RB is the Ultimate, so wait this long before deciding.")]
        [SerializeField] private float pressBuffer = 0.06f;
        [SerializeField] private SquadProfile squad;

        private readonly List<Ability> abilities = new List<Ability>();
        private readonly Ability[] slots = new Ability[3];
        private InputReader input;
        private Transform cameraTransform;
        private LockOnSystem lockOn;
        private int pendingSlot = -1;
        private float pendingSince;
        private bool built;

        public PlayerController Owner { get; private set; }
        public PlayerCombat Combat { get; private set; }
        public Health Health { get; private set; }
        public SpecialMeter Meter { get; private set; }
        public InputReader Input => input;
        public Transform CameraTransform => cameraTransform;
        public ResourcePool Concentration { get; private set; }
        public ResourcePool FlightEnergy { get; private set; }
        public SquadProfile Squad => squad;
        // Set by CompanionAI: the enemy the AI wants its abilities aimed at.
        public Transform AiTarget { get; set; }
        public IReadOnlyList<Ability> Abilities => abilities;
        // An ability just started (the animator plays a casting pose).
        public event System.Action<Ability> AbilityUsed;
        public string LastMessage { get; private set; }
        public float LastMessageTime { get; private set; } = float.NegativeInfinity;

        public bool BlocksAttacks
        {
            get
            {
                for (int i = 0; i < abilities.Count; i++)
                {
                    if (abilities[i].IsActive && abilities[i].BlocksAttacks)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // The enemy the abilities aim at: lock-on for the player, AiTarget for a companion.
        public Transform CurrentTarget
        {
            get
            {
                if (Owner != null && !Owner.IsControlled)
                {
                    return AiTarget != null && AiTarget.gameObject.activeInHierarchy ? AiTarget : null;
                }

                return lockOn != null ? lockOn.CurrentTarget : null;
            }
        }

        public void SetSquadProfile(SquadProfile profile)
        {
            squad = profile;
        }

        public void SetUnlockAll(bool value)
        {
            unlockAll = value;
        }

        private void Awake()
        {
            Owner = GetComponent<PlayerController>();
            Combat = GetComponent<PlayerCombat>();
            Health = GetComponent<Health>();
            Meter = GetComponent<SpecialMeter>();

            if (squad == null)
            {
                squad = ScriptableObject.CreateInstance<SquadProfile>();
            }

            TeamSynergy.SetProfile(squad);
            Concentration = new ResourcePool(squad.concentrationMax, squad.concentrationRegen, squad.concentrationRegenDelay);
            FlightEnergy = new ResourcePool(squad.flightEnergyMax, squad.flightEnergyRegen, squad.flightEnergyRegenDelay);
        }

        public void Initialize(InputReader reader, Transform cameraForMovement, LockOnSystem targeter)
        {
            input = reader;
            cameraTransform = cameraForMovement;
            lockOn = targeter;
            if (Meter == null)
            {
                Meter = GetComponent<SpecialMeter>();
            }

            Build();
        }

        private void Build()
        {
            if (built || Owner == null || Owner.Definition == null)
            {
                return;
            }

            built = true;
            List<AbilityDefinition> definitions = Owner.Definition.abilities;
            for (int i = 0; i < definitions.Count; i++)
            {
                Ability ability = AbilityFactory.Create(definitions[i]);
                if (ability == null)
                {
                    continue;
                }

                abilities.Add(ability);
                if (definitions[i].slot != AbilitySlot.Passive)
                {
                    slots[(int)definitions[i].slot] = ability;
                }

                ability.Bind(this);
            }
        }

        public Ability GetSlot(AbilitySlot slot)
        {
            return slot == AbilitySlot.Passive ? null : slots[(int)slot];
        }

        public AbilityDefinition FindDefinition(string abilityId)
        {
            if (Owner == null || Owner.Definition == null)
            {
                return null;
            }

            List<AbilityDefinition> definitions = Owner.Definition.abilities;
            for (int i = 0; i < definitions.Count; i++)
            {
                if (definitions[i] != null && definitions[i].abilityId == abilityId)
                {
                    return definitions[i];
                }
            }

            return null;
        }

        public T Find<T>() where T : Ability
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                if (abilities[i] is T match)
                {
                    return match;
                }
            }

            return null;
        }

        // Unlocked by the chapter in progress. No chapter set (test scenes, free mode) = everything unlocked.
        public bool IsUnlocked(AbilityDefinition definition)
        {
            if (unlockAll || UnlockAllOverride || definition == null)
            {
                return true;
            }

            ChapterDefinition chapter = GameManager.Instance != null ? GameManager.Instance.CurrentChapter : null;
            return chapter == null || definition.unlockChapter <= chapter.chapterNumber;
        }

        // True when the warrior has this ability (by id) and it is unlocked; passives like Triangular use it.
        public bool HasUnlocked(string abilityId)
        {
            AbilityDefinition definition = FindDefinition(abilityId);
            return definition != null && IsUnlocked(definition);
        }

        public bool TryUse(AbilitySlot slot)
        {
            Ability ability = GetSlot(slot);
            if (ability == null)
            {
                return false;
            }

            if (ability.IsActive)
            {
                ability.OnPressedAgain();
                return true;
            }

            AbilityDefinition definition = ability.Definition;
            if (!IsUnlocked(definition))
            {
                Notify(definition.displayName + ": bloqueada hasta el capitulo " + definition.unlockChapter);
                return false;
            }

            if ((Owner != null && (Owner.IsStunned || Owner.IsMounted)) || !ability.CanUse())
            {
                if (ability.CooldownRemaining <= 0f && ability.UnavailableReason != null)
                {
                    Notify(ability.UnavailableReason);
                }

                return false;
            }

            if (!CanAfford(definition))
            {
                Notify(definition.displayName + ": no hay energia suficiente");
                return false;
            }

            Spend(definition);
            ability.Begin();
            AbilityUsed?.Invoke(ability);
            return true;
        }

        public bool CanAfford(AbilityDefinition definition)
        {
            switch (definition.resource)
            {
                case AbilityResource.SpecialMeter:
                    return Meter != null && Meter.Current >= definition.cost;
                case AbilityResource.Concentration:
                    return Concentration.CanAfford(definition.cost);
                case AbilityResource.FlightEnergy:
                    return FlightEnergy.CanAfford(definition.cost);
                default:
                    return true;
            }
        }

        private void Spend(AbilityDefinition definition)
        {
            switch (definition.resource)
            {
                case AbilityResource.SpecialMeter:
                    Meter.TrySpend(definition.cost);
                    break;
                case AbilityResource.Concentration:
                    Concentration.TrySpend(definition.cost);
                    break;
                case AbilityResource.FlightEnergy:
                    FlightEnergy.TrySpend(definition.cost);
                    break;
            }
        }

        // Telekinesis held: when Concentration runs dry Yumi is stunned (Biblia 5.3).
        public bool DrainConcentration(float amount)
        {
            if (!Concentration.Drain(amount))
            {
                return false;
            }

            Owner.Stun(squad.exhaustionStun);
            Notify("Concentracion agotada!");
            return true;
        }

        public void Notify(string message)
        {
            LastMessage = message;
            LastMessageTime = Time.unscaledTime;
        }

        // Flat direction for dashes and walls: towards the target, the stick, or where the camera looks.
        public Vector3 FlatAimDirection()
        {
            Transform target = CurrentTarget;
            Vector3 direction;
            if (target != null)
            {
                direction = target.position - transform.position;
            }
            else if (Owner != null && Owner.IsControlled && input != null && cameraTransform != null)
            {
                Vector2 move = input.Move;
                Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
                direction = move.sqrMagnitude > 0.04f ? forward * move.y + right * move.x : forward;
            }
            else
            {
                direction = transform.forward;
            }

            direction = Vector3.ProjectOnPlane(direction, Vector3.up);
            return direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
        }

        // Point being aimed at in 3D: the target, the centre of the screen, or straight ahead.
        public Vector3 AimPoint(float maxDistance)
        {
            Transform target = CurrentTarget;
            if (target != null)
            {
                return target.position;
            }

            if (Owner != null && Owner.IsControlled && cameraTransform != null)
            {
                Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
                int mask = ~(1 << GameLayers.Index(GameLayers.Player) | 1 << GameLayers.Index(GameLayers.PlayerAttack));
                if (Physics.Raycast(ray, out RaycastHit hit, maxDistance + 20f, mask, QueryTriggerInteraction.Ignore))
                {
                    return hit.point;
                }

                return ray.GetPoint(maxDistance);
            }

            return transform.position + Vector3.up * 1.2f + transform.forward * maxDistance;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Concentration.Tick(dt);
            FlightEnergy.Tick(dt);

            for (int i = 0; i < abilities.Count; i++)
            {
                Ability ability = abilities[i];
                if (ability.IsActive || ability.IsPassive && IsUnlocked(ability.Definition))
                {
                    ability.Tick(dt);
                }
            }

            if (Owner != null && Owner.IsControlled && input != null)
            {
                ReadInput();
            }
        }

        private void ReadInput()
        {
            if (input.UltimatePressed)
            {
                pendingSlot = -1;
                TryUse(AbilitySlot.Ultimate);
                return;
            }

            if (input.Ability1Pressed)
            {
                pendingSlot = (int)AbilitySlot.Ability1;
                pendingSince = Time.unscaledTime;
            }
            else if (input.Ability2Pressed)
            {
                pendingSlot = (int)AbilitySlot.Ability2;
                pendingSince = Time.unscaledTime;
            }

            if (pendingSlot >= 0 && Time.unscaledTime - pendingSince >= pressBuffer)
            {
                AbilitySlot slot = (AbilitySlot)pendingSlot;
                pendingSlot = -1;
                TryUse(slot);
            }
        }

        public bool TryIntercept(DamageInfo info)
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                if (abilities[i].IsActive && abilities[i].TryIntercept(info))
                {
                    return true;
                }
            }

            return false;
        }

        // Devirtualized or removed: nothing keeps running (slow motion, clones, held objects).
        private void OnDisable()
        {
            pendingSlot = -1;
            for (int i = 0; i < abilities.Count; i++)
            {
                abilities[i].End();
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                abilities[i].OnUnequip();
            }
        }
    }
}
